using System.Drawing;
using System.Text.RegularExpressions;
using BaseLib.Param;
using BaseLibS.Param;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;

namespace PerseusPluginLib.Rearrange{
	public class ProcessTextColumns : IMatrixProcessing{
		public bool HasButton { get { return false; } }
		public Bitmap DisplayImage { get { return null; } }
		public string Description { get { return "Values in string columns can be manipulated according to a regular expression."; } }
		public string HelpOutput { get { return ""; } }
		public string[] HelpSupplTables { get { return new string[0]; } }
		public int NumSupplTables { get { return 0; } }
		public string Name { get { return "Process text column"; } }
		public string Heading { get { return "Rearrange"; } }
		public bool IsActive { get { return true; } }
		public float DisplayRank { get { return 22; } }
		public string[] HelpDocuments { get { return new string[0]; } }
		public int NumDocuments { get { return 0; } }
		public string Url { get { return "http://141.61.102.17/perseus_doku/doku.php?id=perseus:activities:MatrixProcessing:Rearrange:ProcessTextColumns"; } }

		public int GetMaxThreads(Parameters parameters){
			return 1;
		}

		public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo){
				string regexStr = param.GetParam<string>("Regular expression").Value;
			Regex regex = new Regex(regexStr);
			int[] inds = param.GetMultiChoiceParam("Columns").Value;
			bool keepColumns = param.GetParam<bool>("Keep original columns").Value;
			bool semicolons = param.GetParam<bool>("Strings separated by semicolons are independent").Value;
			foreach (int col in inds){
                ProcessCol(mdata, regex, col, keepColumns, semicolons);
			}
		}

        private static void ProcessCol(IMatrixData mdata, Regex regex, int col, bool keepColumns, bool semicolons)
        {
			string[] values = new string[mdata.RowCount];
			for (int row = 0; row < mdata.RowCount; row++)
			{
                string fullString = mdata.StringColumns[col][row];
                string[] inputParts;
                string[] resultParts;
                if (semicolons)
                {
                    inputParts = fullString.Split(';');
                }
                else
                {
                    inputParts = new string[] {fullString};
                }
                values[row] = regex.Match(inputParts[0]).Groups[1].ToString();
                for ( int i = 1; i < inputParts.Length; i++ )
                {
                    values[row] += ";" + regex.Match(inputParts[i]).Groups[1];
                }
			}
			if (keepColumns){
				mdata.AddStringColumn(mdata.StringColumnNames[col], null, values);
			} else{
				mdata.StringColumns[col] = values;
			}
		}

		public Parameters GetParameters(IMatrixData mdata, ref string errorString){
			return
				new Parameters(new Parameter[]{
					new MultiChoiceParam("Columns"){Values = mdata.StringColumnNames},
					new StringParam("Regular expression", "^([^;]+)"),
                    new BoolParam("Keep original columns", false),
                    new BoolParam("Strings separated by semicolons are independent", false)
				});
		}
	}
}