using System.Drawing;
using System.Linq;
using BaseLib.Param;
using BaseLibS.Num;
using BaseLibS.Param;
using BaseLibS.Util;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;

namespace PerseusPluginLib.Rearrange{
	public class UniqueValues : IMatrixProcessing{
		public bool HasButton { get { return false; } }
		public Bitmap DisplayImage { get { return null; } }
		public string Description{
			get{
				return "Values in the selected text columns are made unique. The strings are " +
					"interpreted as separated by semicolons. These semicolon-separated values are made unique.";
			}
		}
		public string Name { get { return "Unique values"; } }
		public string Heading { get { return "Rearrange"; } }
		public bool IsActive { get { return true; } }
		public float DisplayRank { get { return 16; } }
		public string[] HelpSupplTables { get { return new string[0]; } }
		public int NumSupplTables { get { return 0; } }
		public string HelpOutput { get { return ""; } }
		public string[] HelpDocuments { get { return new string[0]; } }
		public int NumDocuments { get { return 0; } }
		public string Url { get { return "http://141.61.102.17/perseus_doku/doku.php?id=perseus:activities:MatrixProcessing:Rearrange:UniqueValues"; } }

		public int GetMaxThreads(Parameters parameters) {
			return 1;
		}

		public void ProcessData(IMatrixData mdata, Parameters param1, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo){
				int[] stringCols = param1.GetParam<int[]>("Text columns").Value;
			if (stringCols.Length == 0){
				processInfo.ErrString = "Please select some columns.";
				return;
			}
			foreach (string[] col in stringCols.Select(stringCol => mdata.StringColumns[stringCol])){
				for (int i = 0; i < col.Length; i++){
					string q = col[i];
					if (q.Length == 0){
						continue;
					}
					string[] w = q.Split(';');
					w = ArrayUtils.UniqueValues(w);
					col[i] = StringUtils.Concat(";", w);
				}
			}
		}

		public Parameters GetParameters(IMatrixData mdata, ref string errorString) {
			return
				new Parameters(new Parameter[]{
					new MultiChoiceParam("Text columns"){
						Values = mdata.StringColumnNames, Value = new int[0],
						Help = "Select here the text colums that should be expanded."
					}
				});
		}
	}
}