using System.Collections.Generic;
using System.Drawing;
using BaseLib.Param;
using BaseLibS.Param;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;
using PerseusPluginLib.Properties;
using PerseusPluginLib.Utils;

namespace PerseusPluginLib.Filter{
	public class FilterCategoricalColumn : IMatrixProcessing{
		public bool HasButton { get { return true; } }
		public Bitmap DisplayImage { get { return Resources.filter2; } }
		public string Description { get { return "Those rows are kept or removed that have the specified value in the selected categorical column."; } }
		public string HelpOutput { get { return "The filtered matrix."; } }
		public string Name { get { return "Filter rows based on categorical column"; } }
		public string Heading { get { return "Filter rows"; } }
		public bool IsActive { get { return true; } }
		public float DisplayRank { get { return 0; } }
		public string[] HelpSupplTables { get { return new string[0]; } }
		public int NumSupplTables { get { return 0; } }
		public string[] HelpDocuments { get { return new string[0]; } }
		public int NumDocuments { get { return 0; } }
		public int GetMaxThreads(Parameters parameters) { return 1; }
		public string Url { get { return "http://141.61.102.17/perseus_doku/doku.php?id=perseus:activities:MatrixProcessing:Filterrows:FilterCategoricalColumn"; } }

		public Parameters GetParameters(IMatrixData mdata, ref string errorString){
			Parameters[] subParams = new Parameters[mdata.CategoryColumnCount];
			for (int i = 0; i < mdata.CategoryColumnCount; i++){
				string[] values = mdata.GetCategoryColumnValuesAt(i);
				int[] sel = values.Length == 1 ? new[]{0} : new int[0];
				subParams[i] =
					new Parameters(new Parameter[]{
						new MultiChoiceParam("Values", sel){
							Values = values,
							Help = "The value that should be present to discard/keep the corresponding row."
						}
					});
			}
			return
				new Parameters(new Parameter[]{
					new SingleChoiceWithSubParams("Column"){
						Values = mdata.CategoryColumnNames,
						SubParams = subParams,
						Help = "The categorical column that the filtering should be based on.",
						ParamNameWidth = 50,
						TotalWidth = 731
					},
					new SingleChoiceParam("Mode"){
						Values = new[]{"Remove matching rows", "Keep matching rows"},
						Help =
							"If 'Remove matching rows' is selected, rows having the values specified above will be removed while " +
								"all other rows will be kept. If 'Keep matching rows' is selected, the opposite will happen."
					},
					PerseusPluginUtils.GetFilterModeParam(true)
				});
		}

		public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo){
				ParameterWithSubParams<int> p = param.GetParamWithSubParams<int>("Column");
			int colInd = p.Value;
			if (colInd < 0){
				processInfo.ErrString = "No categorical columns available.";
				return;
			}
			Parameter<int[]> mcp = p.GetSubParameters().GetMultiChoiceParam("Values");
			int[] inds = mcp.Value;
			if (inds.Length == 0){
				processInfo.ErrString = "Please select at least one term for filtering.";
				return;
			}
			string[] values = new string[inds.Length];
			string[] v = mdata.GetCategoryColumnValuesAt(colInd);
			for (int i = 0; i < values.Length; i++){
				values[i] = v[inds[i]];
			}
			HashSet<string> value = new HashSet<string>(values);
			bool remove = param.GetParam<int>("Mode").Value == 0;
			List<int> valids = new List<int>();
			for (int i = 0; i < mdata.RowCount; i++){
				bool valid = true;
				foreach (string w in mdata.GetCategoryColumnEntryAt(colInd, i)){
					if (value.Contains(w)){
						valid = false;
						break;
					}
				}
				if ((valid && remove) || (!valid && !remove)){
					valids.Add(i);
				}
			}
			PerseusPluginUtils.FilterRows(mdata, param, valids.ToArray());
		}
	}
}