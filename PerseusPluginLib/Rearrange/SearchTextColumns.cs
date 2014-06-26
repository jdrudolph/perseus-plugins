using System.Drawing;
using BaseLib.Param;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;

namespace PerseusPluginLib.Rearrange{
	public class SearchTextColumns : IMatrixProcessing{
		public bool HasButton { get { return false; } }
		public Bitmap DisplayImage { get { return null; } }
		public string Description { get { return "A new categorical column is generated representing search results in a text column."; } }
		public string HelpOutput { get { return ""; } }
		public string[] HelpSupplTables { get { return new string[0]; } }
		public int NumSupplTables { get { return 0; } }
		public string Name { get { return "Search text column"; } }
		public string Heading { get { return "Rearrange"; } }
		public bool IsActive { get { return true; } }
		public float DisplayRank { get { return 23; } }
		public string[] HelpDocuments { get { return new string[0]; } }
		public int NumDocuments { get { return 0; } }
		public string Url { get { return "http://141.61.102.17/perseus_doku/doku.php?id=perseus:searchtextcolumns"; } }
		public int GetMaxThreads(Parameters parameters) { return 1; }

		public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo){
			string word = param.GetStringParam("Find what").Value;
			int colInd = param.GetSingleChoiceParam("Look in").Value;
			bool matchCase = param.GetBoolParam("Match case").Value;
			bool matchWholeWord = param.GetBoolParam("Match whole word").Value;
			string scolName = mdata.StringColumnNames[colInd];
			string[] scol = mdata.StringColumns[colInd];
			string[][] catCol = new string[mdata.RowCount][];
			for (int i = 0; i < catCol.Length; i++){
				bool found = Find(scol[i], word, matchCase, matchWholeWord);
				catCol[i] = found ? new[]{"+"} : new string[0];
			}
			mdata.AddCategoryColumn("Search: " + scolName, "Search: " + scolName, catCol);
		}

		private static bool Find(string text, string word, bool matchCase, bool matchWholeWord){
			text = text.Trim();
			if (!matchCase){
				text = text.ToLower();
				word = word.ToLower();
			}
			if (!matchWholeWord){
				return text.Contains(word);
			}
			string[] q = text.Split(';');
			foreach (string t in q){
				string r = t.Trim();
				if (r.Equals(word)){
					return true;
				}
			}
			return false;
		}

		public Parameters GetParameters(IMatrixData mdata, ref string errorString){
			return
				new Parameters(new Parameter[]{
					new StringParam("Find what"), new SingleChoiceParam("Look in"){Values = mdata.StringColumnNames},
					new BoolParam("Match case"), new BoolParam("Match whole word")
				});
		}
	}
}