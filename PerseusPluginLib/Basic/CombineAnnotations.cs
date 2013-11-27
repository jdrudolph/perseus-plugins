using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using BaseLib.ParamWf;
using BaseLib.Util;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;

namespace PerseusPluginLib.Basic{
	public class CombineAnnotations : IMatrixProcessing{
		public bool HasButton { get { return false; } }
		public Image ButtonImage { get { return null; } }
		public string HelpDescription { get { return "Search multiple categorical or string columns for the occurence of a set of terms."; } }
		public string HelpOutput { get { return "A new categorical column is generated indicating the presence of any of these terms."; } }
		public string[] HelpSupplTables { get { return new string[0]; } }
		public int NumSupplTables { get { return 0; } }
		public string Name { get { return "Combine annotations"; } }
		public string Heading { get { return "Basic"; } }
		public bool IsActive { get { return true; } }
		public float DisplayOrder { get { return 3; } }
		public DocumentType HelpDescriptionType { get { return DocumentType.PlainText; } }
		public DocumentType HelpOutputType { get { return DocumentType.PlainText; } }
		public DocumentType[] HelpSupplTablesType { get { return new DocumentType[0]; } }
		public string[] HelpDocuments { get { return new string[0]; } }
		public DocumentType[] HelpDocumentTypes { get { return new DocumentType[0]; } }
		public int NumDocuments { get { return 0; } }

		public int GetMaxThreads(ParametersWf parameters) {
			return 1;
		}

		public void ProcessData(IMatrixData mdata, ParametersWf param, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo){
			string colName = param.GetStringParam("Name of new column").Value;
			int[] columns = param.GetMultiChoiceParam("Categories").Value;
			bool inverse = param.GetBoolParam("Inverse").Value;
			int[] catCols;
			int[] stringCols;
			Split(columns, out catCols, out stringCols, mdata.CategoryColumnCount);
			string[] word1 = param.GetMultiStringParam("Search terms").Value;
			if (word1.Length == 0){
				processInfo.ErrString = "Please specify one or more search terms.";
				return;
			}
			if (string.IsNullOrEmpty(colName)){
				colName = word1[0];
			}
			string[] word = new string[word1.Length];
			for (int i = 0; i < word.Length; i++){
				word[i] = word1[i].ToLower().Trim();
			}
			bool[] indicator = new bool[mdata.RowCount];
			foreach (int col in catCols){
				string[][] cat = mdata.GetCategoryColumnAt(col);
				for (int i = 0; i < cat.Length; i++){
					foreach (string s in cat[i]){
						foreach (string s1 in word){
							if (s.ToLower().Contains(s1)){
								indicator[i] = true;
								break;
							}
						}
					}
				}
			}
			foreach (string[] txt in stringCols.Select(col => mdata.StringColumns[col])){
				for (int i = 0; i < txt.Length; i++){
					string s = txt[i];
					foreach (string s1 in word){
						if (s.ToLower().Contains(s1)){
							indicator[i] = true;
							break;
						}
					}
				}
			}
			string[][] newCol = new string[indicator.Length][];
			for (int i = 0; i < newCol.Length; i++){
				bool yes = inverse ? !indicator[i] : indicator[i];
				newCol[i] = yes ? new[]{"+"} : new string[0];
			}
			mdata.AddCategoryColumn(colName, "", newCol);
		}

		private static void Split(IEnumerable<int> columns, out int[] catCols, out int[] stringCols, int catColCount){
			List<int> catCols1 = new List<int>();
			List<int> stringCols1 = new List<int>();
			foreach (int column in columns){
				if (column < catColCount){
					catCols1.Add(column);
				} else{
					stringCols1.Add(column - catColCount);
				}
			}
			catCols = catCols1.ToArray();
			stringCols = stringCols1.ToArray();
		}

		public ParametersWf GetParameters(IMatrixData mdata, ref string errorString) {
			string[] choice = ArrayUtils.Concat(mdata.CategoryColumnNames, mdata.StringColumnNames);
			int[] selection = new int[0];
			return
				new ParametersWf(new ParameterWf[]{
					new MultiChoiceParamWf("Categories")
					{Value = selection, Values = choice, Help = "Search these columns for the search terms specified."},
					new MultiStringParamWf("Search terms"){Help = "Look for these terms in the selected columns"},
					new StringParamWf("Name of new column"),
					new BoolParamWf("Inverse"){Help = "If true, those rows are indicated which do not contain any of the search terms."}
				});
		}
	}
}