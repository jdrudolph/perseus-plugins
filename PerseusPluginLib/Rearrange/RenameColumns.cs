using System.Collections.Generic;
using System.Drawing;
using BaseLib.Param;
using BaseLibS.Param;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;

namespace PerseusPluginLib.Rearrange{
	public class RenameColumns : IMatrixProcessing{
		public bool HasButton { get { return false; } }
		public Bitmap DisplayImage { get { return null; } }
		public string Description { get { return "New names can be specified for each expression column. The new names are typed in explicitly."; } }
		public string HelpOutput { get { return "Same matrix but with the new expression column names."; } }
		public string[] HelpSupplTables { get { return new string[0]; } }
		public int NumSupplTables { get { return 0; } }
		public string Name { get { return "Rename columns"; } }
		public string Heading { get { return "Rearrange"; } }
		public bool IsActive { get { return true; } }
		public float DisplayRank { get { return 0; } }
		public string[] HelpDocuments { get { return new string[0]; } }
		public int NumDocuments { get { return 0; } }
		public string Url { get { return "http://141.61.102.17/perseus_doku/doku.php?id=perseus:activities:MatrixProcessing:Rearrange:RenameColumns"; } }

		public int GetMaxThreads(Parameters parameters) {
			return 1;
		}

		public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo){
			List<string> expressionColumnNames = new List<string>();
			HashSet<string> taken = new HashSet<string>();
			for (int i = 0; i < mdata.ColumnCount; i++){
				string newName = param.GetParam<string>(mdata.ColumnNames[i]).Value;
				if (taken.Contains(newName)){
					processInfo.ErrString = "Name " + newName + " is contained multiple times";
					return;
				}
				taken.Add(newName);
				expressionColumnNames.Add(newName);
			}
			mdata.ColumnNames = expressionColumnNames;
			taken = new HashSet<string>();
			List<string> numericColumnNames = new List<string>();
			for (int i = 0; i < mdata.NumericColumnCount; i++){
				string newName = param.GetParam<string>(mdata.NumericColumnNames[i]).Value;
				if (taken.Contains(newName)){
					processInfo.ErrString = "Name " + newName + " is contained multiple times";
					return;
				}
				taken.Add(newName);
				numericColumnNames.Add(newName);
			}
			mdata.NumericColumnNames = numericColumnNames;
			taken = new HashSet<string>();
			List<string> categoryColumnNames = new List<string>();
			for (int i = 0; i < mdata.CategoryColumnCount; i++){
				string newName = param.GetParam<string>(mdata.CategoryColumnNames[i]).Value;
				if (taken.Contains(newName)){
					processInfo.ErrString = "Name " + newName + " is contained multiple times";
					return;
				}
				taken.Add(newName);
				categoryColumnNames.Add(newName);
			}
			mdata.CategoryColumnNames = categoryColumnNames;
			taken = new HashSet<string>();
			List<string> stringColumnNames = new List<string>();
			for (int i = 0; i < mdata.StringColumnCount; i++){
				string newName = param.GetParam<string>(mdata.StringColumnNames[i]).Value;
				if (taken.Contains(newName)){
					processInfo.ErrString = "Name " + newName + " is contained multiple times";
					return;
				}
				taken.Add(newName);
				stringColumnNames.Add(newName);
			}
			mdata.StringColumnNames = stringColumnNames;
			taken = new HashSet<string>();
			List<string> multiNumericColumnNames = new List<string>();
			for (int i = 0; i < mdata.MultiNumericColumnCount; i++){
				string newName = param.GetParam<string>(mdata.MultiNumericColumnNames[i]).Value;
				if (taken.Contains(newName)){
					processInfo.ErrString = "Name " + newName + " is contained multiple times";
					return;
				}
				taken.Add(newName);
				multiNumericColumnNames.Add(newName);
			}
			mdata.MultiNumericColumnNames = multiNumericColumnNames;
		}

		public Parameters GetParameters(IMatrixData mdata, ref string errorString) {
			List<Parameter> par = new List<Parameter>();
			foreach (string t in mdata.ColumnNames){
				string help = "Specify the new name for the column '" + t + "'.";
				par.Add(new StringParam(t) { Value = t, Help = help });
			}
			foreach (string t in mdata.NumericColumnNames){
				string help = "Specify the new name for the column '" + t + "'.";
				par.Add(new StringParam(t) { Value = t, Help = help });
			}
			foreach (string t in mdata.CategoryColumnNames){
				string help = "Specify the new name for the column '" + t + "'.";
				par.Add(new StringParam(t) { Value = t, Help = help });
			}
			foreach (string t in mdata.StringColumnNames){
				string help = "Specify the new name for the column '" + t + "'.";
				par.Add(new StringParam(t) { Value = t, Help = help });
			}
			foreach (string t in mdata.MultiNumericColumnNames){
				string help = "Specify the new name for the column '" + t + "'.";
				par.Add(new StringParam(t) { Value = t, Help = help });
			}
			return new Parameters(par);
		}
	}
}