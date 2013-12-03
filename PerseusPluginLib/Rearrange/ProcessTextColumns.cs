using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Media;
using BaseLib.Param;
using BaseLib.Util;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;

namespace PerseusPluginLib.Rearrange{
	public class ProcessTextColumns : IMatrixProcessing{
		public bool HasButton { get { return false; } }
		public ImageSource DisplayImage { get { return null; } }
		public string HelpDescription { get { return "Values in string columns can be manipulated according to a regular expression."; } }
		public DocumentType HelpDescriptionType { get { return DocumentType.PlainText; } }
		public string HelpOutput { get { return ""; } }
		public string[] HelpSupplTables { get { return new string[0]; } }
		public DocumentType HelpOutputType { get { return DocumentType.PlainText; } }
		public DocumentType[] HelpSupplTablesType { get { return new DocumentType[0]; } }
		public int NumSupplTables { get { return 0; } }
		public string Name { get { return "Process text column"; } }
		public string Heading { get { return "Rearrange"; } }
		public bool IsActive { get { return true; } }
		public float DisplayOrder { get { return 22; } }
		public string[] HelpDocuments { get { return new string[0]; } }
		public DocumentType[] HelpDocumentTypes { get { return new DocumentType[0]; } }
		public int NumDocuments { get { return 0; } }

		public int GetMaxThreads(Parameters parameters) {
			return 1;
		}

		public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo){
			string regexStr = param.GetStringParam("Regular expression").Value;
			Regex regex = new Regex(regexStr);
			int[] inds = param.GetMultiChoiceParam("Columns").Value;
			foreach (int ind in inds){
				ProcessCol(mdata.StringColumns[ind], regex);
			}
		}

		private static void ProcessCol(IList<string> col, Regex regex){
			for (int i = 0; i < col.Count; i++){
				string newVal = regex.Match(col[i]).Groups[1].ToString();
				col[i] = newVal;
			}
		}

		public Parameters GetParameters(IMatrixData mdata, ref string errorString) {
			return
				new Parameters(new Parameter[]{
					new MultiChoiceParam("Columns"){Values = mdata.StringColumnNames},
					new StringParam("Regular expression", "^([^;]+)")
				});
		}
	}
}