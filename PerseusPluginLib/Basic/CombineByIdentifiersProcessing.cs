using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BaseLib.Data;
using BaseLib.Param;
using BaseLib.Util;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;

namespace PerseusPluginLib.Basic{
	public enum AverageType{
		Sum,
		Mean,
		Median,
		Maximum,
		Minimum
	}

	public class CombineByIdentifiersProcessing : IMatrixProcessing{
		public bool HasButton { get { return false; } }
		public Bitmap DisplayImage { get { return null; } }
		public string Description { get { return ""; } }
		public string HelpOutput { get { return ""; } }
		public string[] HelpSupplTables { get { return new string[0]; } }
		public int NumSupplTables { get { return 0; } }
		public string Name { get { return "Combine rows by identifiers"; } }
		public string Heading { get { return "Basic"; } }
		public bool IsActive { get { return false; } }
		public float DisplayOrder { get { return 20; } }
		public DocumentType HelpDescriptionType { get { return DocumentType.PlainText; } }
		public DocumentType HelpOutputType { get { return DocumentType.PlainText; } }
		public DocumentType[] HelpSupplTablesType { get { return new DocumentType[0]; } }
		public string[] HelpDocuments { get { return new string[0]; } }
		public DocumentType[] HelpDocumentTypes { get { return new DocumentType[0]; } }
		public int NumDocuments { get { return 0; } }

		public int GetMaxThreads(Parameters parameters) {
			return 1;
		}

		public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo){
			bool keepEmpty = param.GetBoolParam("Keep rows without ID").Value;
			AverageType atype = GetAverageType(param.GetSingleChoiceParam("Average type for expression columns").Value);
			string[] ids2 = mdata.StringColumns[param.GetSingleChoiceParam("ID column").Value];
			string[][] ids = SplitIds(ids2);
			int[] present;
			int[] absent;
			GetPresentAbsentIndices(ids, out present, out absent);
			ids = ArrayUtils.SubArray(ids, present);
			int[][] rowInds = new int[present.Length][];
			for (int i = 0; i < rowInds.Length; i++){
				rowInds[i] = new[]{present[i]};
			}
			ClusterRows(ref rowInds, ref ids);
			if (keepEmpty){
				rowInds = ProlongRowInds(rowInds, absent);
			}
			int nrows = rowInds.Length;
			int ncols = mdata.ExpressionColumnCount;
			float[,] expVals = new float[nrows,ncols];
			for (int j = 0; j < ncols; j++){
				float[] c = mdata.GetExpressionColumn(j);
				for (int i = 0; i < nrows; i++){
					float[] d = ArrayUtils.SubArray(c, rowInds[i]);
					expVals[i, j] = Average(d, atype);
				}
			}
			mdata.ExpressionValues = expVals;
			for (int i = 0; i < mdata.NumericColumnCount; i++){
				string name = mdata.NumericColumnNames[i];
				AverageType atype1 = GetAverageType(param.GetSingleChoiceParam("Average type for " + name).Value);
				double[] c = mdata.NumericColumns[i];
				double[] newCol = new double[nrows];
				for (int k = 0; k < nrows; k++){
					double[] d = ArrayUtils.SubArray(c, rowInds[k]);
					newCol[k] = Average(d, atype1);
				}
				mdata.NumericColumns[i] = newCol;
			}
			for (int i = 0; i < mdata.CategoryColumnCount; i++){
				string[][] c = mdata.GetCategoryColumnAt(i);
				string[][] newCol = new string[nrows][];
				for (int k = 0; k < nrows; k++){
					string[][] d = ArrayUtils.SubArray(c, rowInds[k]);
					newCol[k] = Average(d);
				}
				mdata.SetCategoryColumnAt(newCol,i);
			}
			for (int i = 0; i < mdata.StringColumnCount; i++){
				string[] c = mdata.StringColumns[i];
				string[] newCol = new string[nrows];
				for (int k = 0; k < nrows; k++){
					string[] d = ArrayUtils.SubArray(c, rowInds[k]);
					newCol[k] = Average(d);
				}
				mdata.StringColumns[i] = newCol;
			}
			for (int i = 0; i < mdata.MultiNumericColumnCount; i++){
				double[][] c = mdata.MultiNumericColumns[i];
				double[][] newCol = new double[nrows][];
				for (int k = 0; k < nrows; k++){
					double[][] d = ArrayUtils.SubArray(c, rowInds[k]);
					newCol[k] = Average(d);
				}
				mdata.MultiNumericColumns[i] = newCol;
			}
		}

		private static double[] Average(IList<double[]> d){
			return d.Count == 0 ? new double[0] : d[0];
		}

		private static string Average(IList<string> s){
			if (s.Count == 0){
				return "";
			}
			if (s.Count == 1){
				return s[0];
			}
			HashSet<string> result = new HashSet<string>();
			foreach (string s1 in s){
				if (s1.Length > 0){
					string[] q = s1.Split(';');
					foreach (string s2 in q){
						result.Add(s2);
					}
				}
			}
			string[] w = ArrayUtils.ToArray(result);
			Array.Sort(w);
			return StringUtils.Concat(";", w);
		}

		private static string[] Average(IList<string[]> s){
			return ArrayUtils.UniqueValues(ArrayUtils.Concat(s));
		}

		private static double Average(IEnumerable<double> c, AverageType atype){
			List<double> g = new List<double>();
			foreach (double f in c){
				if (!double.IsNaN(f) && !double.IsInfinity(f)){
					g.Add(f);
				}
			}
			if (g.Count == 0){
				return double.NaN;
			}
			switch (atype){
				case AverageType.Mean:
					return ArrayUtils.Mean(g);
				case AverageType.Maximum:
					return ArrayUtils.Max(g);
				case AverageType.Median:
					return ArrayUtils.Median(g);
				case AverageType.Minimum:
					return ArrayUtils.Min(g);
				case AverageType.Sum:
					return ArrayUtils.Sum(g);
				default:
					throw new Exception("Never get here.");
			}
		}

		private static float Average(IEnumerable<float> c, AverageType atype){
			List<float> g = new List<float>();
			foreach (float f in c){
				if (!float.IsNaN(f) && !float.IsInfinity(f)){
					g.Add(f);
				}
			}
			if (g.Count == 0){
				return float.NaN;
			}
			switch (atype){
				case AverageType.Mean:
					return (float) ArrayUtils.Mean(g);
				case AverageType.Maximum:
					return ArrayUtils.Max(g);
				case AverageType.Median:
					return ArrayUtils.Median(g);
				case AverageType.Minimum:
					return ArrayUtils.Min(g);
				case AverageType.Sum:
					return (float) ArrayUtils.Sum(g);
				default:
					throw new Exception("Never get here.");
			}
		}

		private static int[][] ProlongRowInds(IList<int[]> rowInds, IList<int> absent){
			int[][] result = new int[rowInds.Count + absent.Count][];
			for (int i = 0; i < rowInds.Count; i++){
				result[i] = rowInds[i];
			}
			for (int i = 0; i < absent.Count; i++){
				result[rowInds.Count + i] = new[]{absent[i]};
			}
			return result;
		}

		private static void ClusterRows(ref int[][] rowInds, ref string[][] geneIds){
			int n = rowInds.Length;
			for (int i = 0; i < n; i++){
				Array.Sort(geneIds[i]);
			}
			IndexedBitMatrix contains = new IndexedBitMatrix(n, n);
			for (int i = 0; i < n; i++){
				for (int j = 0; j < n; j++){
					if (i == j){
						continue;
					}
					contains.Set(i, j, Contains(geneIds[i], geneIds[j]));
				}
			}
			int count;
			do{
				count = 0;
				int start = 0;
				while (true){
					int container = -1;
					int contained = -1;
					for (int i = start; i < rowInds.Length; i++){
						container = GetContainer(i, contains);
						if (container != -1){
							contained = i;
							break;
						}
					}
					if (container == -1){
						break;
					}
					for (int i = 0; i < n; i++){
						contains.Set(i, contained, false);
						contains.Set(contained, i, false);
					}
					geneIds[contained] = new string[0];
					rowInds[container] = ArrayUtils.Concat(rowInds[container], rowInds[contained]);
					rowInds[contained] = new int[0];
					start = contained + 1;
					count++;
				}
			} while (count > 0);
			List<int> valids = new List<int>();
			for (int i = 0; i < n; i++){
				if (geneIds[i].Length > 0){
					valids.Add(i);
				}
			}
			int[] a = valids.ToArray();
			rowInds = ArrayUtils.SubArray(rowInds, a);
			geneIds = ArrayUtils.SubArray(geneIds, a);
		}

		private static int GetContainer(int contained, IndexedBitMatrix contains){
			int n = contains.RowCount;
			for (int i = 0; i < n; i++){
				if (contains.Get(i, contained)){
					return i;
				}
			}
			return -1;
		}

		private static bool Contains(string[] p1, ICollection<string> p2){
			if (p2.Count > p1.Length){
				return false;
			}
			foreach (string p in p2){
				int index = Array.BinarySearch(p1, p);
				if (index < 0){
					return false;
				}
			}
			return true;
		}

		private static void GetPresentAbsentIndices(IList<string[]> ids, out int[] present, out int[] absent){
			List<int> present1 = new List<int>();
			List<int> absent1 = new List<int>();
			for (int i = 0; i < ids.Count; i++){
				if (ids[i].Length > 0){
					present1.Add(i);
				} else{
					absent1.Add(i);
				}
			}
			present = present1.ToArray();
			absent = absent1.ToArray();
		}

		private static string[][] SplitIds(IList<string> a){
			string[][] result = new string[a.Count][];
			for (int i = 0; i < result.Length; i++){
				result[i] = a[i].Length > 0 ? a[i].Split(';') : new string[0];
				Array.Sort(result[i]);
			}
			return result;
		}

		private static AverageType GetAverageType(int avInd){
			switch (avInd){
				case 0:
					return AverageType.Sum;
				case 1:
					return AverageType.Mean;
				case 2:
					return AverageType.Median;
				case 3:
					return AverageType.Maximum;
				case 4:
					return AverageType.Minimum;
				default:
					throw new Exception("Never get here.");
			}
		}

		public Parameters GetParameters(IMatrixData mdata, ref string errorString) {
			string[] averageTypeChoice = new[]{"Sum", "Mean", "Median", "Maximum", "Minimum"};
			List<Parameter> parameters = new List<Parameter>{
				new SingleChoiceParam("ID column")
				{Values = mdata.StringColumnNames, Help = "Column containing IDs that are going to be clustered."},
				new BoolParam("Keep rows without ID"),
				new SingleChoiceParam("Average type for expression columns"){Values = averageTypeChoice, Value = 2}
			};
			foreach (string n in mdata.NumericColumnNames){
				parameters.Add(new SingleChoiceParam("Average type for " + n) { Values = averageTypeChoice, Value = 2 });
			}
			return new Parameters(parameters);
		}
	}
}