using System.Collections.Generic;
using System.Drawing;
using BaseLib.ParamWf;
using BaseLib.Util;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;

namespace PerseusPluginLib.Group{
	public class CreateNumericalAnnotRow : IMatrixProcessing{
		public bool HasButton { get { return false; } }
		public Image ButtonImage { get { return null; } }
		public string HelpDescription { get { return ""; } }
		public string HelpOutput { get { return "Same matrix with numerical annotation row added."; } }
		public string[] HelpSupplTables { get { return new string[0]; } }
		public int NumSupplTables { get { return 0; } }
		public string Name { get { return "Numerical annotation rows"; } }
		public string Heading { get { return "Annotation rows"; } }
		public bool IsActive { get { return true; } }
		public float DisplayOrder { get { return 2; } }
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
				SingleChoiceWithSubParamsWf scwsp = param.GetSingleChoiceWithSubParams("Action");
				ParametersWf spar = scwsp.GetSubParameters();
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

		private static void ProcessDataRename(IMatrixData mdata, ParametersWf param) {
			int groupColInd = param.GetSingleChoiceParam("Numerical row").Value;
			string newName = param.GetStringParam("New name").Value;
			string newDescription = param.GetStringParam("New description").Value;
			mdata.NumericRowNames[groupColInd] = newName;
			mdata.NumericRowDescriptions[groupColInd] = newDescription;
		}

		private static void ProcessDataDelete(IMatrixData mdata, ParametersWf param) {
			int groupColInd = param.GetSingleChoiceParam("Numerical row").Value;
			mdata.NumericRows.RemoveAt(groupColInd);
			mdata.NumericRowNames.RemoveAt(groupColInd);
			mdata.NumericRowDescriptions.RemoveAt(groupColInd);
		}

		private static void ProcessDataEdit(IMatrixData mdata, ParametersWf param) {
			SingleChoiceWithSubParamsWf s = param.GetSingleChoiceWithSubParams("Numerical row");
			int groupColInd = s.Value;
			ParametersWf sp = s.GetSubParameters();
			for (int i = 0; i < mdata.ExpressionColumnCount; i++){
				string t = mdata.ExpressionColumnNames[i];
				double x = sp.GetDoubleParam(t).Value;
				mdata.NumericRows[groupColInd][i] = x;
			}
		}

		public ParametersWf GetEditParameters(IMatrixData mdata) {
			ParametersWf[] subParams = new ParametersWf[mdata.NumericRowCount];
			for (int i = 0; i < subParams.Length; i++){
				subParams[i] = GetEditParameters(mdata, i);
			}
			List<ParameterWf> par = new List<ParameterWf>{
				new SingleChoiceWithSubParamsWf("Numerical row")
				{Values = mdata.NumericRowNames, SubParams = subParams, Help = "Select the numerical row that should be edited."}
			};
			return new ParametersWf(par);
		}

		public ParametersWf GetEditParameters(IMatrixData mdata, int ind) {
			List<ParameterWf> par = new List<ParameterWf>();
			for (int i = 0; i < mdata.ExpressionColumnCount; i++){
				string t = mdata.ExpressionColumnNames[i];
				string help = "Specify a numerical value for the column '" + t + "'.";
				par.Add(new DoubleParamWf(t, mdata.NumericRows[ind][i]) { Help = help });
			}
			return new ParametersWf(par);
		}

		private static void ProcessDataCreate(IMatrixData mdata, ParametersWf param) {
			string name = param.GetStringParam("Row name").Value;
			double[] groupCol = new double[mdata.ExpressionColumnCount];
			for (int i = 0; i < mdata.ExpressionColumnCount; i++){
				string ename = mdata.ExpressionColumnNames[i];
				double value = param.GetDoubleParam(ename).Value;
				groupCol[i] = value;
			}
			mdata.AddNumericRow(name, name, groupCol);
		}

		public ParametersWf GetParameters(IMatrixData mdata, ref string errorString) {
			SingleChoiceWithSubParamsWf scwsp = new SingleChoiceWithSubParamsWf("Action") {
				Values = new[]{"Create", "Edit", "Rename", "Delete"},
				SubParams =
					new[]{GetCreateParameters(mdata), GetEditParameters(mdata), GetRenameParameters(mdata), GetDeleteParameters(mdata)},
				ParamNameWidth = 136, TotalWidth = 731
			};
			return new ParametersWf(new ParameterWf[] { scwsp });
		}

		public ParametersWf GetDeleteParameters(IMatrixData mdata) {
			List<ParameterWf> par = new List<ParameterWf>{
				new SingleChoiceParamWf("Numerical row")
				{Values = mdata.NumericRowNames, Help = "Select the numerical row that should be deleted."}
			};
			return new ParametersWf(par);
		}

		public ParametersWf GetRenameParameters(IMatrixData mdata) {
			List<ParameterWf> par = new List<ParameterWf>{
				new SingleChoiceParamWf("Numerical row")
				{Values = mdata.NumericRowNames, Help = "Select the numerical row that should be renamed."},
				new StringParamWf("New name"), new StringParamWf("New description"),
			};
			return new ParametersWf(par);
		}

		public ParametersWf GetCreateParameters(IMatrixData mdata) {
			List<ParameterWf> par = new List<ParameterWf> { new StringParamWf("Row name") { Value = "Quantity1", Help = "Name of the new numerical annotation row." } };
			for (int i = 0; i < mdata.ExpressionColumnNames.Count; i++){
				string t = mdata.ExpressionColumnNames[i];
				string help = "Specify a numerical value for the column '" + t + "'.";
				par.Add(new DoubleParamWf(t, (i + 1.0)) { Help = help });
			}
			return new ParametersWf(par);
		}
	}
}