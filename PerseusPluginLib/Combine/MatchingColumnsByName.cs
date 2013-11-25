using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using BasicLib.ParamWf;
using BasicLib.Util;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;
using PerseusPluginLib.Properties;

namespace PerseusPluginLib.Combine{
	public class MatchingColumnsByName : IMatrixMultiProcessing{
		public bool HasButton { get { return true; } }
		public Image ButtonImage { get { return Resources.combineButton_Image; } }
		public string Name { get { return "Matching columns by name"; } }
		public bool IsActive { get { return true; } }
		public float DisplayOrder { get { return -4; } }
		public string DataType1 { get { return "First matrix"; } }
		public string DataType2 { get { return "Second matrix"; } }
		public string HelpOutput { get { return ""; } }
		public string HelpDescription { get { return ""; } }
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

		public string GetInputName(int index) {
			return index == 0 ? "Base matrix" : "Other matrix";
		}

		public int GetMaxThreads(ParametersWf parameters) {
			return 1;
		}

		private static string[] SpecialSort(IList<string> x, IList<string> y, out Dictionary<string, int> xdic,
			out Dictionary<string, int> ydic){
			HashSet<string> hx = ArrayUtils.ToHashSet(x);
			HashSet<string> hy = ArrayUtils.ToHashSet(y);
			HashSet<string> common = new HashSet<string>();
			foreach (string s in hx.Where(hy.Contains)){
				common.Add(s);
			}
			foreach (string s in common){
				if (hx.Contains(s)){
					hx.Remove(s);
				}
				if (hy.Contains(s)){
					hy.Remove(s);
				}
			}
			List<string> result = new List<string>();
			foreach (string t in x){
				if (common.Contains(t)){
					result.Add(t);
				}
			}
			foreach (string t in x){
				if (!common.Contains(t)){
					result.Add(t);
				}
			}
			foreach (string t in y){
				if (!common.Contains(t)){
					result.Add(t);
				}
			}
			xdic = new Dictionary<string, int>();
			for (int i = 0; i < x.Count; i++){
				xdic.Add(x[i], i);
			}
			ydic = new Dictionary<string, int>();
			for (int i = 0; i < y.Count; i++){
				ydic.Add(y[i], i);
			}
			return result.ToArray();
		}

		public IMatrixData ProcessData(IMatrixData[] inputData, ParametersWf param, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo){
			IMatrixData mdata1 = inputData[0];
			IMatrixData mdata2 = inputData[1];
			int nrows1 = mdata1.RowCount;
			int nrows2 = mdata2.RowCount;
			int nrows = nrows1 + nrows2;
			Dictionary<string, int> dic1;
			Dictionary<string, int> dic2;
			string[] expColNames = SpecialSort(mdata1.ExpressionColumnNames, mdata2.ExpressionColumnNames, out dic1, out dic2);
			float[,] ex = new float[nrows, expColNames.Length];
			for (int i = 0; i < ex.GetLength(0); i++) {
				for (int j = 0; j < ex.GetLength(1); j++) {
					ex[i, j] = float.NaN;
				}
			}
			for (int i = 0; i < expColNames.Length; i++) {
				if (dic1.ContainsKey(expColNames[i])) {
					int ind = dic1[expColNames[i]];
					for (int j = 0; j < nrows1; j++) {
						ex[j, i] = mdata1[j, ind];
					}
				}
				if (dic2.ContainsKey(expColNames[i])) {
					int ind = dic2[expColNames[i]];
					for (int j = 0; j < nrows2; j++) {
						ex[nrows1 + j, i] = mdata2[j, ind];
					}
				}
			}
			string[] numColNames = SpecialSort(mdata1.NumericColumnNames, mdata2.NumericColumnNames, out dic1, out dic2);
			List<double[]> numCols = new List<double[]>();
			for (int i = 0; i < numColNames.Length; i++) {
				numCols.Add(new double[nrows]);
				for (int j = 0; j < nrows; j++) {
					numCols[numCols.Count - 1][j] = double.NaN;
				}
			}
			for (int i = 0; i < numColNames.Length; i++) {
				if (dic1.ContainsKey(numColNames[i])) {
					int ind = dic1[numColNames[i]];
					for (int j = 0; j < nrows1; j++) {
						numCols[i][j] = mdata1.NumericColumns[ind][j];
					}
				}
				if (dic2.ContainsKey(numColNames[i])) {
					int ind = dic2[numColNames[i]];
					for (int j = 0; j < nrows2; j++) {
						numCols[i][nrows1 + j] = mdata2.NumericColumns[ind][j];
					}
				}
			}
			string[] stringColNames = SpecialSort(mdata1.StringColumnNames, mdata2.StringColumnNames, out dic1, out dic2);
			List<string[]> stringCols = new List<string[]>();
			for (int i = 0; i < stringColNames.Length; i++) {
				stringCols.Add(new string[nrows]);
				for (int j = 0; j < nrows; j++) {
					stringCols[stringCols.Count - 1][j] = "";
				}
			}
			for (int i = 0; i < stringColNames.Length; i++) {
				if (dic1.ContainsKey(stringColNames[i])) {
					int ind = dic1[stringColNames[i]];
					for (int j = 0; j < nrows1; j++) {
						stringCols[i][j] = mdata1.StringColumns[ind][j];
					}
				}
				if (dic2.ContainsKey(stringColNames[i])) {
					int ind = dic2[stringColNames[i]];
					for (int j = 0; j < nrows2; j++) {
						stringCols[i][nrows1 + j] = mdata2.StringColumns[ind][j];
					}
				}
			}
			string[] catColNames = SpecialSort(mdata1.CategoryColumnNames, mdata2.CategoryColumnNames, out dic1, out dic2);
			List<string[][]> catCols = new List<string[][]>();
			for (int i = 0; i < catColNames.Length; i++) {
				catCols.Add(new string[nrows][]);
				for (int j = 0; j < nrows; j++) {
					catCols[catCols.Count - 1][j] = new string[0];
				}
			}
			for (int i = 0; i < catColNames.Length; i++) {
				if (dic1.ContainsKey(catColNames[i])) {
					int ind = dic1[stringColNames[i]];
					for (int j = 0; j < nrows1; j++) {
						catCols[i][j] = mdata1.GetCategoryColumnAt(ind)[j];
					}
				}
				if (dic2.ContainsKey(catColNames[i])) {
					int ind = dic2[catColNames[i]];
					for (int j = 0; j < nrows2; j++) {
						catCols[i][nrows1 + j] = mdata2.GetCategoryColumnAt(ind)[j];
					}
				}
			}
			string[] multiNumColNames = SpecialSort(mdata1.MultiNumericColumnNames, mdata2.MultiNumericColumnNames, out dic1,
				out dic2);
			List<double[][]> multiNumCols = new List<double[][]>();
			for (int i = 0; i < multiNumColNames.Length; i++) {
				multiNumCols.Add(new double[nrows][]);
				for (int j = 0; j < nrows; j++) {
					multiNumCols[multiNumCols.Count - 1][j] = new double[0];
				}
			}
			for (int i = 0; i < multiNumColNames.Length; i++) {
				if (dic1.ContainsKey(multiNumColNames[i])) {
					int ind = dic1[multiNumColNames[i]];
					for (int j = 0; j < nrows1; j++) {
						multiNumCols[i][j] = mdata1.MultiNumericColumns[ind][j];
					}
				}
				if (dic2.ContainsKey(multiNumColNames[i])) {
					int ind = dic2[multiNumColNames[i]];
					for (int j = 0; j < nrows2; j++) {
						multiNumCols[i][nrows1 + j] = mdata2.MultiNumericColumns[ind][j];
					}
				}
			}
			IMatrixData result = (IMatrixData)mdata1.CreateNewInstance();
			result.ExpressionColumnNames = new List<string>(expColNames);
			result.ExpressionColumnDescriptions = result.ExpressionColumnNames;
			result.ExpressionValues = ex;
			result.NumericColumnNames = new List<string>(numColNames);
			result.NumericColumnDescriptions = result.NumericColumnNames;
			result.NumericColumns = numCols;
			result.StringColumnNames = new List<string>(stringColNames);
			result.StringColumnDescriptions = result.StringColumnDescriptions;
			result.StringColumns = stringCols;
			result.CategoryColumnNames = new List<string>(catColNames);
			result.CategoryColumnDescriptions = result.CategoryColumnNames;
			result.CategoryColumns = catCols;
			result.MultiNumericColumnNames = new List<string>(multiNumColNames);
			result.MultiNumericColumnDescriptions = result.MultiNumericColumnNames;
			result.MultiNumericColumns = multiNumCols;
			return result;
		}

		public ParametersWf GetParameters(IMatrixData[] inputData, ref string errString) {
			return new ParametersWf();
		}
	}
}