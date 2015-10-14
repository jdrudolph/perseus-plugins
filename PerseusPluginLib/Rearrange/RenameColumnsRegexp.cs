using System.Drawing;
using System.Text.RegularExpressions;
using BaseLib.Param;
using BaseLibS.Param;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;

namespace PerseusPluginLib.Rearrange{
	public class RenameColumnsRegexp : IMatrixProcessing{
		public bool HasButton => false;
		public Bitmap DisplayImage => null;

		public string Description
			=> "Rename expression columns with the help of matching part of the name by a regular expression.";

		public string HelpOutput => "";
		public string[] HelpSupplTables => new string[0];
		public int NumSupplTables => 0;
		public string Name => "Rename columns [reg. ex.]";
		public string Heading => "Rearrange";
		public bool IsActive => true;
		public float DisplayRank => 1;
		public string[] HelpDocuments => new string[0];
		public int NumDocuments => 0;

		public string Url
			=> "http://141.61.102.17/perseus_doku/doku.php?id=perseus:activities:MatrixProcessing:Rearrange:RenameColumnsRegexp";

		public int GetMaxThreads(Parameters parameters){
			return 1;
		}

		public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo){
			string regexStr = param.GetParam<string>("Regular expression").Value;
			Regex regex = new Regex(regexStr);
			for (int i = 0; i < mdata.ColumnCount; i++){
				string newName = regex.Match(mdata.ColumnNames[i]).Groups[1].ToString();
				if (string.IsNullOrEmpty(newName)){
					processInfo.ErrString = "Applying parse rule to '" + mdata.ColumnNames[i] + "' results in an empty string.";
					return;
				}
				mdata.ColumnNames[i] = newName;
			}
		}

		public Parameters GetParameters(IMatrixData mdata, ref string errorString){
			return
				new Parameters(new Parameter[]{
					new StringParam("Regular expression"){
						Help =
							"The regular expression that determines how the new column names are created from the old " +
							"column names. As an example if you want to transform 'Ratio H/L Normalized Something' " +
							"into 'Something' the suitable regular expression is 'Ratio H/L Normalized (.*)'"
					}
				});
		}
	}
}