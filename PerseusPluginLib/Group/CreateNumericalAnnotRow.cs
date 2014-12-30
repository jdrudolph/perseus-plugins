using System.Collections.Generic;
using System.Drawing;
using BaseLib.Param;
using BaseLibS.Param;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;

namespace PerseusPluginLib.Group{
	public class CreateNumericalAnnotRow : IMatrixProcessing{
		public bool HasButton { get { return false; } }
		public Bitmap DisplayImage { get { return null; } }

		public string Description{
			get{
				return "Add or edit numerical annotation rows. This could for instance " +
					"define the times of samples for time series data.";
			}
		}

		public string HelpOutput { get { return "Same matrix with numerical annotation row added or modified."; } }
		public string[] HelpSupplTables { get { return new string[0]; } }
		public int NumSupplTables { get { return 0; } }
		public string Name { get { return "Numerical annotation rows"; } }
		public string Heading { get { return "Annot. rows"; } }
		public bool IsActive { get { return true; } }
		public float DisplayRank { get { return 2; } }
		public string[] HelpDocuments { get { return new string[0]; } }
		public int NumDocuments { get { return 0; } }
		public string Url { get { return "http://141.61.102.17/perseus_doku/doku.php?id=perseus:activities:MatrixProcessing:Annotrows:CreateNumericalAnnotRow"; } }
		public int GetMaxThreads(Parameters parameters) { return 1; }

		public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo){
			SingleChoiceWithSubParams scwsp = param.GetSingleChoiceWithSubParams("Action");
			Parameters spar = scwsp.GetSubParameters();
			switch (scwsp.Value){
				case 0:
					ProcessDataCreate(mdata, spar);
					break;
				case 1:
					ProcessDataEdit(mdata, spar);
					break;
				case 2:
					ProcessDataRename(mdata, spar);
					break;
				case 3:
					ProcessDataDelete(mdata, spar);
					break;
			}
		}

		private static void ProcessDataRename(IMatrixData mdata, Parameters param){
			int groupColInd = param.GetSingleChoiceParam("Numerical row").Value;
			string newName = param.GetStringParam("New name").Value;
			string newDescription = param.GetStringParam("New description").Value;
			mdata.NumericRowNames[groupColInd] = newName;
			mdata.NumericRowDescriptions[groupColInd] = newDescription;
		}

		private static void ProcessDataDelete(IMatrixData mdata, Parameters param){
			int groupColInd = param.GetSingleChoiceParam("Numerical row").Value;
			mdata.NumericRows.RemoveAt(groupColInd);
			mdata.NumericRowNames.RemoveAt(groupColInd);
			mdata.NumericRowDescriptions.RemoveAt(groupColInd);
		}

		private static void ProcessDataEdit(IMatrixData mdata, Parameters param){
			SingleChoiceWithSubParams s = param.GetSingleChoiceWithSubParams("Numerical row");
			int groupColInd = s.Value;
			Parameters sp = s.GetSubParameters();
			for (int i = 0; i < mdata.ColumnCount; i++){
				string t = mdata.ColumnNames[i];
				double x = sp.GetDoubleParam(t).Value;
				mdata.NumericRows[groupColInd][i] = x;
			}
		}

		public Parameters GetEditParameters(IMatrixData mdata){
			Parameters[] subParams = new Parameters[mdata.NumericRowCount];
			for (int i = 0; i < subParams.Length; i++){
				subParams[i] = GetEditParameters(mdata, i);
			}
			List<Parameter> par = new List<Parameter>{
				new SingleChoiceWithSubParams("Numerical row"){
					Values = mdata.NumericRowNames,
					SubParams = subParams,
					Help = "Select the numerical row that should be edited."
				}
			};
			return new Parameters(par);
		}

		public Parameters GetEditParameters(IMatrixData mdata, int ind){
			List<Parameter> par = new List<Parameter>();
			for (int i = 0; i < mdata.ColumnCount; i++){
				string t = mdata.ColumnNames[i];
				string help = "Specify a numerical value for the column '" + t + "'.";
				par.Add(new DoubleParam(t, mdata.NumericRows[ind][i]){Help = help});
			}
			return new Parameters(par);
		}

		private static void ProcessDataCreate(IMatrixData mdata, Parameters param){
			string name = param.GetStringParam("Row name").Value;
			double[] groupCol = new double[mdata.ColumnCount];
			for (int i = 0; i < mdata.ColumnCount; i++){
				string ename = mdata.ColumnNames[i];
				double value = param.GetDoubleParam(ename).Value;
				groupCol[i] = value;
			}
			mdata.AddNumericRow(name, name, groupCol);
		}

		public Parameters GetParameters(IMatrixData mdata, ref string errorString){
			SingleChoiceWithSubParams scwsp = new SingleChoiceWithSubParams("Action"){
				Values = new[]{"Create", "Edit", "Rename", "Delete"},
				SubParams =
					new[]{GetCreateParameters(mdata), GetEditParameters(mdata), GetRenameParameters(mdata), GetDeleteParameters(mdata)},
				ParamNameWidth = 136,
				TotalWidth = 731
			};
			return new Parameters(new Parameter[]{scwsp});
		}

		public Parameters GetDeleteParameters(IMatrixData mdata){
			List<Parameter> par = new List<Parameter>{
				new SingleChoiceParam("Numerical row"){
					Values = mdata.NumericRowNames,
					Help = "Select the numerical row that should be deleted."
				}
			};
			return new Parameters(par);
		}

		public Parameters GetRenameParameters(IMatrixData mdata){
			List<Parameter> par = new List<Parameter>{
				new SingleChoiceParam("Numerical row"){
					Values = mdata.NumericRowNames,
					Help = "Select the numerical row that should be renamed."
				},
				new StringParam("New name"),
				new StringParam("New description"),
			};
			return new Parameters(par);
		}

		public Parameters GetCreateParameters(IMatrixData mdata){
			List<Parameter> par = new List<Parameter>{
				new StringParam("Row name"){Value = "Quantity1", Help = "Name of the new numerical annotation row."}
			};
			for (int i = 0; i < mdata.ColumnNames.Count; i++){
				string t = mdata.ColumnNames[i];
				string help = "Specify a numerical value for the column '" + t + "'.";
				par.Add(new DoubleParam(t, (i + 1.0)){Help = help});
			}
			return new Parameters(par);
		}
	}
}