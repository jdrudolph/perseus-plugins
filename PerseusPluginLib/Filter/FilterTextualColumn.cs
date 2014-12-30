using System.Collections.Generic;
using System.Drawing;
using BaseLib.Param;
using BaseLibS.Param;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;
using PerseusPluginLib.Utils;

namespace PerseusPluginLib.Filter{
	public class FilterTextualColumn : IMatrixProcessing{
		public bool HasButton { get { return false; } }
		public Bitmap DisplayImage { get { return null; } }
		public string Description { get { return "Only those rows are kept that have a value in the textual column that matches the search string."; } }
		public string HelpOutput { get { return "The filtered matrix."; } }
		public string[] HelpSupplTables { get { return new string[0]; } }
		public int NumSupplTables { get { return 0; } }
		public string Name { get { return "Filter rows based on text column"; } }
		public string Heading { get { return "Filter rows"; } }
		public bool IsActive { get { return true; } }
		public float DisplayRank { get { return 2; } }
		public string[] HelpDocuments { get { return new string[0]; } }
		public int NumDocuments { get { return 0; } }
		public int GetMaxThreads(Parameters parameters) { return 1; }
		public string Url { get { return "http://141.61.102.17/perseus_doku/doku.php?id=perseus:activities:MatrixProcessing:Filterrows:FilterTextualColumn"; } }

		public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo){
			int colInd = param.GetSingleChoiceParam("Column").Value;
			string searchString = param.GetStringParam("Search string").Value;
			if (string.IsNullOrEmpty(searchString)){
				processInfo.ErrString = "Please provide a search string";
				return;
			}
			bool remove = param.GetSingleChoiceParam("Mode").Value == 0;
			bool matchCase = param.GetBoolParam("Match case").Value;
			bool matchWholeWord = param.GetBoolParam("Match whole word").Value;
			string[] vals = mdata.StringColumns[colInd];
			List<int> valids = new List<int>();
			for (int i = 0; i < vals.Length; i++){
				bool matches = Matches(vals[i], searchString, matchCase, matchWholeWord);
				if (matches && !remove){
					valids.Add(i);
				} else if (!matches && remove){
					valids.Add(i);
				}
			}
			PerseusPluginUtils.FilterRows(mdata, param, valids.ToArray());
		}

		private static bool Matches(string text, string searchString, bool matchCase, bool matchWholeWord){
			if (text == null && text.Length == 0){
				return false;
			}
			string[] words = text.Length == 0 ? new string[0] : text.Split(';');
			foreach (string word in words){
				if (MatchesWord(word, searchString, matchCase, matchWholeWord)){
					return true;
				}
			}
			return false;
		}

		private static bool MatchesWord(string word, string searchString, bool matchCase, bool matchWholeWord){
			if (!matchCase){
				word = word.ToUpper();
				searchString = searchString.ToUpper();
			}
			searchString = searchString.Trim();
			word = word.Trim();
			return matchWholeWord ? searchString.Equals(word) : word.Contains(searchString);
		}

		public Parameters GetParameters(IMatrixData mdata, ref string errorString){
			return
				new Parameters(new Parameter[]{
					new SingleChoiceParam("Column"){
						Values = mdata.StringColumnNames,
						Help = "The text column that the filtering should be based on."
					},
					new StringParam("Search string"){Help = "String that is searched in the specified column."},
					new BoolParam("Match case"), new BoolParam("Match whole word"){Value = true},
					new SingleChoiceParam("Mode"){
						Values = new[]{"Remove matching rows", "Keep matching rows"},
						Help =
							"If 'Remove matching rows' is selected, rows matching the criteria will be removed while " +
								"all other rows will be kept. If 'Keep matching rows' is selected, the opposite will happen.",
						Value = 0
					},
					PerseusPluginUtils.GetFilterModeParam(true)
				});
		}
	}
}