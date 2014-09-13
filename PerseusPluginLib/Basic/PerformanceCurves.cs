using System;
using System.Collections.Generic;
using System.Drawing;
using BaseLib.Param;
using BaseLibS.Num;
using BaseLibS.Util;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;

namespace PerseusPluginLib.Basic{
	public class PerformanceCurves : IMatrixProcessing{
		public bool HasButton { get { return false; } }
		public Bitmap DisplayImage { get { return null; } }
		public string Name { get { return "Performance curves"; } }
		public string HelpOutput { get { return ""; } }
		public string[] HelpSupplTables { get { return new string[0]; } }
		public int NumSupplTables { get { return 0; } }
		public string Description { get { return "Calculate predictive performance measures like precision-recall or ROC curves."; } }
		public bool IsActive { get { return true; } }
		public float DisplayRank { get { return 10; } }
		public string[] HelpDocuments { get { return new string[0]; } }
		public int NumDocuments { get { return 0; } }
		public string Url { get { return "http://141.61.102.17/perseus_doku/doku.php?id=perseus:activities:MatrixProcessing:Basic:PerformanceCurves"; } }

		public int GetMaxThreads(Parameters parameters) {
			return 1;
		}

		public void ProcessData(IMatrixData data, Parameters param, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo){
			bool falseAreIndicated = param.GetSingleChoiceParam("Indicated are").Value == 0;
			int catCol = param.GetSingleChoiceParam("In column").Value;
			string word = param.GetStringParam("Indicator").Value;
			int[] scoreColumns = param.GetMultiChoiceParam("Scores").Value;
			if (scoreColumns.Length == 0){
				processInfo.ErrString = "Please specify at least one column with scores.";
				return;
			}
			bool largeIsGood = param.GetBoolParam("Large values are good").Value;
			int[] showColumns = param.GetMultiChoiceParam("Display quantity").Value;
			if (showColumns.Length == 0){
				processInfo.ErrString = "Please select at least one quantity to display";
				return;
			}
			bool[] indCol = GetIndicatorColumn(falseAreIndicated, catCol, word, data);
			List<string> expColNames = new List<string>();
			List<float[]> expCols = new List<float[]>();
			foreach (int scoreColumn in scoreColumns){
				double[] vals = scoreColumn < data.NumericColumnCount
					? data.NumericColumns[scoreColumn]
					: ArrayUtils.ToDoubles(data.GetExpressionColumn(scoreColumn - data.NumericColumnCount));
				string name = scoreColumn < data.NumericColumnCount
					? data.NumericColumnNames[scoreColumn] : data.ExpressionColumnNames[scoreColumn - data.NumericColumnCount];
				int[] order = GetOrder(vals, largeIsGood);
				CalcCurve(ArrayUtils.SubArray(indCol, order), showColumns, name, expCols, expColNames);
			}
			float[,] expData = ToMatrix(expCols);
			data.SetData(data.Name, expColNames, expData, new List<string>(), new List<string[]>(), new List<string>(),
				new List<string[][]>(), new List<string>(), new List<double[]>(), new List<string>(), new List<double[][]>());
		}

		private static float[,] ToMatrix(IList<float[]> x){
			float[,] result = new float[x[0].Length,x.Count];
			for (int i = 0; i < result.GetLength(0); i++){
				for (int j = 0; j < result.GetLength(1); j++){
					result[i, j] = x[j][i];
				}
			}
			return result;
		}

		public static void CalcCurve(IList<bool> x, IList<int> showColumns, string name, List<float[]> expCols,
			List<string> expColNames){
			CalcCurve(x, ArrayUtils.SubArray(PerformanceColumnType.allTypes, showColumns), name, expCols, expColNames);
		}

		public static void CalcCurve(IList<bool> x, PerformanceColumnType[] types, string name, List<float[]> expCols,
			List<string> expColNames){
			float[][] columns = new float[types.Length][];
			string[] columnNames = new string[types.Length];
			for (int i = 0; i < types.Length; i++){
				columns[i] = new float[x.Count + 1];
				columnNames[i] = name + " " + types[i].Name;
			}
			int np = 0;
			int nn = 0;
			foreach (bool t in x){
				if (t){
					np++;
				} else{
					nn++;
				}
			}
			double tp = 0;
			double fp = 0;
			double tn = nn;
			double fn = np;
			for (int j = 0; j < types.Length; j++){
				columns[j][0] = (float) types[j].Calculate(tp, tn, fp, fn, np, nn);
			}
			for (int i = 0; i < x.Count; i++){
				if (x[i]){
					tp++;
					fn--;
				} else{
					fp++;
					tn--;
				}
				for (int j = 0; j < types.Length; j++){
					columns[j][i + 1] = (float) types[j].Calculate(tp, tn, fp, fn, np, nn);
				}
			}
			expColNames.AddRange(columnNames);
			expCols.AddRange(columns);
		}

		public static int[] GetOrder(double[] vals, bool largeIsGood){
			List<int> valids = new List<int>();
			List<int> invalids = new List<int>();
			for (int i = 0; i < vals.Length; i++){
				if (double.IsNaN(vals[i])){
					invalids.Add(i);
				} else{
					valids.Add(i);
				}
			}
			vals = ArrayUtils.SubArray(vals, valids);
			int[] o = OrderValues(vals);
			o = ArrayUtils.SubArray(valids, o);
			if (largeIsGood){
				ArrayUtils.Revert(o);
			}
			return ArrayUtils.Concat(o, invalids.ToArray());
		}

		private static int[] OrderValues(IList<double> vals){
			int[] o = ArrayUtils.Order(vals);
			RandomizeConstantRegions(o, vals);
			return o;
		}

		private static void RandomizeConstantRegions(int[] o, IList<double> vals){
			int startInd = 0;
			for (int i = 1; i < o.Length; i++){
				if (vals[o[i]] != vals[o[startInd]]){
					if (i - startInd > 1){
						RandomizeConstantRegion(o, startInd, i);
					}
					startInd = i;
				}
			}
		}

		private static void RandomizeConstantRegion(int[] o, int startInd, int endInd){
			int len = endInd - startInd;
			Random2 r = new Random2();
			int[] p = r.NextPermutation(len);
			int[] permuted = new int[len];
			for (int i = 0; i < len; i++){
				permuted[i] = o[startInd + p[i]];
			}
			Array.Copy(permuted, 0, o, startInd, len);
		}

		public static bool[] GetIndicatorColumn(bool falseAreIndicated, int catColInd, string word, IMatrixData data){
			string[][] catCol = data.GetCategoryColumnAt(catColInd);
			bool[] result = new bool[data.RowCount];
			for (int i = 0; i < result.Length; i++){
				string[] cats = catCol[i];
				Array.Sort(cats);
				bool contains = Array.BinarySearch(cats, word) >= 0;
				if (falseAreIndicated){
					result[i] = !contains;
				} else{
					result[i] = contains;
				}
			}
			return result;
		}

		public Parameters GetParameters(IMatrixData mdata, ref string errorString) {
			string[] numChoice = ArrayUtils.Concat(mdata.NumericColumnNames, mdata.ExpressionColumnNames);
			return
				new Parameters(new Parameter[]{
					new SingleChoiceParam("Indicated are"){Values = new[]{"False", "True"}},
					new SingleChoiceParam("In column"){Values = mdata.CategoryColumnNames}, new StringParam("Indicator"){Value = "+"},
					new MultiChoiceParam("Scores"){Value = new[]{0}, Values = numChoice},
					new BoolParam("Large values are good"){Value = true},
					new MultiChoiceParam("Display quantity"){Values = PerformanceColumnType.AllTypeNames}
				});
		}

		public string Heading { get { return "Basic"; } }
	}
}