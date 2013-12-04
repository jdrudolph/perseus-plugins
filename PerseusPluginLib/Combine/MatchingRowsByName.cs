using System;
using System.Collections.Generic;
using System.Windows.Media;
using BaseLib.Param;
using BaseLib.Util;
using BaseLib.Wpf;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;
using PerseusPluginLib.Properties;

namespace PerseusPluginLib.Combine{
	public class MatchingRowsByName : IMatrixMultiProcessing{
		public bool HasButton { get { return true; } }
		public ImageSource DisplayImage { get { return WpfUtils.LoadBitmap(Resources.combineButton_Image); } }
		public string Name { get { return "Matching rows by name"; } }
		public bool IsActive { get { return true; } }
		public float DisplayOrder { get { return -5; } }
		public string HelpOutput { get { return ""; } }
		public string HelpDescription{
			get{
				return
					"The base matrix is copied. Rows of the second matrix are associated with rows of the base matrix via matching " +
						"expressions in a textual column from each matrix. Selected columns of the second matrix are attached to the " +
						"first matrix. If exactly one row of the second matrix corresponds to a row of the base matrix, values are " +
						"just copied. If more than one row of the second matrix matches to a row of the first matrix, the corresponding " +
						"values are averaged (actually the median is taken) for numerical and expression columns and concatenated " +
						"for textual and categorical columns.";
			}
		}
		public DocumentType HelpDescriptionType { get { return DocumentType.PlainText; } }
		public DocumentType HelpOutputType { get { return DocumentType.PlainText; } }
		public string[] HelpSupplTables { get { return new string[0]; } }
		public DocumentType[] HelpSupplTablesType { get { return new DocumentType[0]; } }
		public int NumSupplTables { get { return 0; } }
		public string[] HelpDocuments { get { return new string[0]; } }
		public DocumentType[] HelpDocumentTypes { get { return new DocumentType[0]; } }
		public int NumDocuments { get { return 0; } }
		public int MinNumInput { get { return 2; } }
		public int MaxNumInput { get { return 2; } }
		public string Heading { get { return null; } }

		public string GetInputName(int index){
			return index == 0 ? "Base matrix" : "Other matrix";
		}

		public int GetMaxThreads(Parameters parameters) {
			return 1;
		}

		public IMatrixData ProcessData(IMatrixData[] inputData, Parameters parameters, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo){
			IMatrixData mdata1 = inputData[0];
			IMatrixData mdata2 = inputData[1];
			bool indicator = parameters.GetBoolParam("Indicator").Value;
			int otherCol = parameters.GetSingleChoiceParam("Matching column 2").Value;
			Func<double[], double> avExpression = GetAveraging(parameters.GetSingleChoiceParam("Combine expression values").Value);
			Func<double[], double> avNumerical = GetAveraging(parameters.GetSingleChoiceParam("Combine numerical values").Value);
			string[] q = mdata2.StringColumns[otherCol];
			string[][] w = new string[q.Length][];
			for (int i = 0; i < q.Length; i++){
				string r = q[i].Trim();
				w[i] = r.Length == 0 ? new string[0] : r.Split(';');
				w[i] = ArrayUtils.UniqueValues(w[i]);
			}
			Dictionary<string, List<int>> id2Cols = new Dictionary<string, List<int>>();
			for (int i = 0; i < w.Length; i++){
				foreach (string s in w[i]){
					if (!id2Cols.ContainsKey(s)){
						id2Cols.Add(s, new List<int>());
					}
					id2Cols[s].Add(i);
				}
			}
			int pgCol = parameters.GetSingleChoiceParam("Matching column 1").Value;
			string[] d = mdata1.StringColumns[pgCol];
			string[][] x = new string[d.Length][];
			for (int i = 0; i < d.Length; i++){
				string r = d[i].Trim();
				x[i] = r.Length == 0 ? new string[0] : r.Split(';');
				x[i] = ArrayUtils.UniqueValues(x[i]);
			}
			int[][] indexMap = new int[x.Length][];
			string[][] indicatorCol = new string[x.Length][];
			for (int i = 0; i < indexMap.Length; i++){
				List<int> qwer = new List<int>();
				foreach (string s in x[i]){
					if (id2Cols.ContainsKey(s)){
						List<int> en = id2Cols[s];
						qwer.AddRange(en);
					}
				}
				indexMap[i] = qwer.ToArray();
				indexMap[i] = ArrayUtils.UniqueValues(indexMap[i]);
				indicatorCol[i] = indexMap[i].Length > 0 ? new[]{"+"} : new string[0];
			}
			IMatrixData result = (IMatrixData) mdata1.Clone();
			SetAnnotationRows(result, mdata1, mdata2);
			if (indicator){
				result.AddCategoryColumn(mdata2.Name, "", indicatorCol);
			}
				{
					int[] exCols = parameters.GetMultiChoiceParam("Expression columns").Value;
					float[,] newExColumns = new float[mdata1.RowCount,exCols.Length];
					float[,] newQuality = new float[mdata1.RowCount,exCols.Length];
					bool[,] newIsImputed = new bool[mdata1.RowCount,exCols.Length];
					string[] newExColNames = new string[exCols.Length];
					float[,] oldEx = mdata2.ExpressionValues;
					float[,] oldQual = mdata2.QualityValues;
					bool[,] oldImp = mdata2.IsImputed;
					for (int i = 0; i < exCols.Length; i++){
						newExColNames[i] = mdata2.ExpressionColumnNames[exCols[i]];
						for (int j = 0; j < mdata1.RowCount; j++){
							int[] inds = indexMap[j];
							List<double> values = new List<double>();
							List<double> qual = new List<double>();
							List<bool> imp = new List<bool>();
							foreach (int ind in inds){
								double v = oldEx[ind, exCols[i]];
								if (!double.IsNaN(v) && !double.IsInfinity(v)){
									values.Add(v);
									double qx = oldQual[ind, exCols[i]];
									if (!double.IsNaN(qx) && !double.IsInfinity(qx)){
										qual.Add(qx);
									}
									bool isi = oldImp[ind, exCols[i]];
									imp.Add(isi);
								}
							}
							newExColumns[j, i] = values.Count == 0 ? float.NaN : (float) avExpression(values.ToArray());
							newQuality[j, i] = qual.Count == 0 ? float.NaN : (float) avExpression(qual.ToArray());
							newIsImputed[j, i] = imp.Count != 0 && AvImp(imp.ToArray());
						}
					}
					MakeNewNames(newExColNames, result.ExpressionColumnNames);
					AddExpressionColumns(result, newExColNames, newExColumns, newQuality, newIsImputed);
				}
				{
					int[] numCols = parameters.GetMultiChoiceParam("Numerical columns").Value;
					double[][] newNumericalColumns = new double[numCols.Length][];
					string[] newNumColNames = new string[numCols.Length];
					for (int i = 0; i < numCols.Length; i++){
						double[] oldCol = mdata2.NumericColumns[numCols[i]];
						newNumColNames[i] = mdata2.NumericColumnNames[numCols[i]];
						newNumericalColumns[i] = new double[mdata1.RowCount];
						for (int j = 0; j < mdata1.RowCount; j++){
							int[] inds = indexMap[j];
							List<double> values = new List<double>();
							foreach (int ind in inds){
								double v = oldCol[ind];
								if (!double.IsNaN(v)){
									values.Add(v);
								}
							}
							newNumericalColumns[i][j] = values.Count == 0 ? double.NaN : avNumerical(values.ToArray());
						}
					}
					for (int i = 0; i < numCols.Length; i++){
						result.AddNumericColumn(newNumColNames[i], "", newNumericalColumns[i]);
					}
				}
				{
					int[] catCols = parameters.GetMultiChoiceParam("Categorical columns").Value;
					string[][][] newCatColumns = new string[catCols.Length][][];
					string[] newCatColNames = new string[catCols.Length];
					for (int i = 0; i < catCols.Length; i++){
						string[][] oldCol = mdata2.GetCategoryColumnAt(catCols[i]);
						newCatColNames[i] = mdata2.CategoryColumnNames[catCols[i]];
						newCatColumns[i] = new string[mdata1.RowCount][];
						for (int j = 0; j < mdata1.RowCount; j++){
							int[] inds = indexMap[j];
							List<string[]> values = new List<string[]>();
							foreach (int ind in inds){
								string[] v = oldCol[ind];
								if (v.Length > 0){
									values.Add(v);
								}
							}
							newCatColumns[i][j] = values.Count == 0
								? new string[0] : ArrayUtils.UniqueValues(ArrayUtils.Concat(values.ToArray()));
						}
					}
					for (int i = 0; i < catCols.Length; i++){
						result.AddCategoryColumn(newCatColNames[i], "", newCatColumns[i]);
					}
				}
				{
					int[] stringCols = parameters.GetMultiChoiceParam("String columns").Value;
					string[][] newStringColumns = new string[stringCols.Length][];
					string[] newStringColNames = new string[stringCols.Length];
					for (int i = 0; i < stringCols.Length; i++){
						string[] oldCol = mdata2.StringColumns[stringCols[i]];
						newStringColNames[i] = mdata2.StringColumnNames[stringCols[i]];
						newStringColumns[i] = new string[mdata1.RowCount];
						for (int j = 0; j < mdata1.RowCount; j++){
							int[] inds = indexMap[j];
							List<string> values = new List<string>();
							foreach (int ind in inds){
								string v = oldCol[ind];
								if (v.Length > 0){
									values.Add(v);
								}
							}
							newStringColumns[i][j] = values.Count == 0 ? "" : StringUtils.Concat(";", values.ToArray());
						}
					}
					for (int i = 0; i < stringCols.Length; i++){
						result.AddStringColumn(newStringColNames[i], "", newStringColumns[i]);
					}
				}
			result.Origin = "Combination";
			return result;
		}

		public Parameters GetParameters(IMatrixData[] inputData, ref string errString) {
			IMatrixData matrixData1 = inputData[0];
			IMatrixData matrixData2 = inputData[1];
			List<string> controlChoice1 = matrixData1.StringColumnNames;
			int index1 = 0;
			for (int i = 0; i < controlChoice1.Count; i++){
				if (controlChoice1[i].ToLower().Contains("uniprot")){
					index1 = i;
					break;
				}
			}
			List<string> controlChoice2 = matrixData2.StringColumnNames;
			int index2 = 0;
			for (int i = 0; i < controlChoice2.Count; i++){
				if (controlChoice2[i].ToLower().Contains("uniprot")){
					index2 = i;
					break;
				}
			}
			List<string> numCol = matrixData2.NumericColumnNames;
			int[] numSel = new int[0];
			List<string> catCol = matrixData2.CategoryColumnNames;
			int[] catSel = new int[0];
			List<string> textCol = matrixData2.StringColumnNames;
			int[] textSel = new int[0];
			List<string> exCol = matrixData2.ExpressionColumnNames;
			int[] exSel = new int[0];
			return
				new Parameters(new Parameter[]{
					new SingleChoiceParam("Matching column 1")
					{Values = controlChoice1, Value = index1, Help = "The column in the first matrix that is used for matching rows."},
					new SingleChoiceParam("Matching column 2")
					{Values = controlChoice2, Value = index2, Help = "The column in the second matrix that is used for matching rows."}
					,
					new BoolParam("Indicator"){
						Help =
							"If checked, a categorical column will be added in which it is indicated by a '+' if at least one row of the second " +
								"matrix matches."
					},
					new MultiChoiceParam("Expression columns"){
						Value = exSel, Values = exCol,
						Help = "Expression columns of the second matrix that should be added to the first matrix."
					},
					new SingleChoiceParam("Combine expression values"){
						Values = new[]{"Median", "Mean", "Minimum", "Maximum", "Sum"},
						Help =
							"In case multiple rows of the second matrix match to a row of the first matrix, how should multiple " +
								"expression values be combined?"
					},
					new MultiChoiceParam("Categorical columns"){
						Values = catCol, Value = catSel,
						Help = "Categorical columns of the second matrix that should be added to the first matrix."
					},
					new MultiChoiceParam("String columns"){
						Values = textCol, Value = textSel,
						Help = "String columns of the second matrix that should be added to the first matrix."
					},
					new MultiChoiceParam("Numerical columns"){
						Values = numCol, Value = numSel,
						Help = "Numerical columns of the second matrix that should be added to the first matrix."
					},
					new SingleChoiceParam("Combine numerical values"){
						Values = new[]{"Median", "Mean", "Minimum", "Maximum", "Sum"},
						Help =
							"In case multiple rows of the second matrix match to a row of the first matrix, how should multiple " +
								"numerical values be combined?"
					}
				});
		}

		private static bool AvImp(IEnumerable<bool> b){
			foreach (bool b1 in b){
				if (b1){
					return true;
				}
			}
			return false;
		}

		private static void SetAnnotationRows(IMatrixData result, IMatrixData mdata1, IMatrixData mdata2){
			result.CategoryRowNames.Clear();
			result.CategoryRowDescriptions.Clear();
			result.ClearCategoryRows();
			result.NumericRowNames.Clear();
			result.NumericRowDescriptions.Clear();
			result.NumericRows.Clear();
			string[] allCatNames = ArrayUtils.Concat(mdata1.CategoryRowNames, mdata2.CategoryRowNames);
			allCatNames = ArrayUtils.UniqueValues(allCatNames);
			result.CategoryRowNames = new List<string>();
			string[] allCatDescriptions = new string[allCatNames.Length];
			for (int i = 0; i < allCatNames.Length; i++){
				allCatDescriptions[i] = GetDescription(allCatNames[i], mdata1.CategoryRowNames, mdata2.CategoryRowNames,
					mdata1.CategoryRowDescriptions, mdata2.CategoryRowDescriptions);
			}
			result.CategoryRowDescriptions = new List<string>();
			for (int index = 0; index < allCatNames.Length; index++){
				string t = allCatNames[index];
				string[][] categoryRow = new string[mdata1.ExpressionColumnCount + mdata2.ExpressionColumnCount][];
				for (int j = 0; j < categoryRow.Length; j++){
					categoryRow[j] = new string[0];
				}
				int ind1 = mdata1.CategoryRowNames.IndexOf(t);
				if (ind1 >= 0){
					string[][] c1 = mdata1.GetCategoryRowAt(ind1);
					for (int j = 0; j < c1.Length; j++){
						categoryRow[j] = c1[j];
					}
				}
				int ind2 = mdata2.CategoryRowNames.IndexOf(t);
				if (ind2 >= 0){
					string[][] c2 = mdata2.GetCategoryRowAt(ind2);
					for (int j = 0; j < c2.Length; j++){
						categoryRow[mdata1.ExpressionColumnCount + j] = c2[j];
					}
				}
				result.AddCategoryRow(allCatNames[index], allCatDescriptions[index], categoryRow);
			}
			string[] allNumNames = ArrayUtils.Concat(mdata1.NumericRowNames, mdata2.NumericRowNames);
			allNumNames = ArrayUtils.UniqueValues(allNumNames);
			result.NumericRowNames = new List<string>(allNumNames);
			string[] allNumDescriptions = new string[allNumNames.Length];
			for (int i = 0; i < allNumNames.Length; i++){
				allNumDescriptions[i] = GetDescription(allNumNames[i], mdata1.NumericRowNames, mdata2.NumericRowNames,
					mdata1.NumericRowDescriptions, mdata2.NumericRowDescriptions);
			}
			result.NumericRowDescriptions = new List<string>(allNumDescriptions);
			foreach (string t in allNumNames){
				double[] numericRow = new double[mdata1.ExpressionColumnCount + mdata2.ExpressionColumnCount];
				for (int j = 0; j < numericRow.Length; j++){
					numericRow[j] = double.NaN;
				}
				int ind1 = mdata1.NumericRowNames.IndexOf(t);
				if (ind1 >= 0){
					double[] c1 = mdata1.NumericRows[ind1];
					for (int j = 0; j < c1.Length; j++){
						numericRow[j] = c1[j];
					}
				}
				int ind2 = mdata2.NumericRowNames.IndexOf(t);
				if (ind2 >= 0){
					double[] c2 = mdata2.NumericRows[ind2];
					for (int j = 0; j < c2.Length; j++){
						numericRow[mdata1.ExpressionColumnCount + j] = c2[j];
					}
				}
				result.NumericRows.Add(numericRow);
			}
		}

		private static string GetDescription(string name, IList<string> names1, IList<string> names2,
			IList<string> descriptions1, IList<string> descriptions2){
			int ind = names1.IndexOf(name);
			if (ind >= 0){
				return descriptions1[ind];
			}
			ind = names2.IndexOf(name);
			return descriptions2[ind];
		}

		private static Func<double[], double> GetAveraging(int ind){
			switch (ind){
				case 0:
					return ArrayUtils.Median;
				case 1:
					return ArrayUtils.Mean;
				case 2:
					return ArrayUtils.Min;
				case 3:
					return ArrayUtils.Max;
				case 4:
					return ArrayUtils.Sum;
				default:
					throw new Exception("Never get here.");
			}
		}

		public static void AddExpressionColumns(IMatrixData data, string[] names, float[,] vals, float[,] qual, bool[,] imp){
			float[,] newVals = new float[data.RowCount,data.ExpressionColumnCount + vals.GetLength(1)];
			float[,] newQual = new float[data.RowCount,data.ExpressionColumnCount + vals.GetLength(1)];
			bool[,] newImp = new bool[data.RowCount,data.ExpressionColumnCount + vals.GetLength(1)];
			for (int i = 0; i < data.RowCount; i++){
				for (int j = 0; j < data.ExpressionColumnCount; j++){
					newVals[i, j] = data[i, j];
					newQual[i, j] = data.QualityValues[i, j];
					newImp[i, j] = data.IsImputed[i, j];
				}
				for (int j = 0; j < vals.GetLength(1); j++){
					newVals[i, data.ExpressionColumnCount + j] = vals[i, j];
					newQual[i, data.ExpressionColumnCount + j] = qual[i, j];
					newImp[i, data.ExpressionColumnCount + j] = imp[i, j];
				}
			}
			data.ExpressionValues = newVals;
			data.QualityValues = newQual;
			data.IsImputed = newImp;
			data.ExpressionColumnNames.AddRange(names);
			data.ExpressionColumnDescriptions.AddRange(names);
		}

		private static void MakeNewNames(IList<string> newExColNames, IEnumerable<string> expressionColumnNames){
			HashSet<string> taken = new HashSet<string>(expressionColumnNames);
			for (int i = 0; i < newExColNames.Count; i++){
				if (taken.Contains(newExColNames[i])){
					newExColNames[i] += "_1";
				}
			}
		}
	}
}