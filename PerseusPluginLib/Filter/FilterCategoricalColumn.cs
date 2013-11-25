using System.Collections.Generic;
using System.Drawing;
using BasicLib.ParamWf;
using BasicLib.Util;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;
using PerseusPluginLib.Properties;
using PerseusPluginLib.Utils;

namespace PerseusPluginLib.Filter{
	public class FilterCategoricalColumn : IMatrixProcessing{
		public bool HasButton { get { return true; } }
		public Image ButtonImage { get { return Resources.filter2; } }
		public string HelpDescription { get { return "Those rows are kept or removed that have the specified value in the selected categorical column."; } }
		public string HelpOutput { get { return "The filtered matrix."; } }
		public DocumentType HelpDescriptionType { get { return DocumentType.PlainText; } }
		public DocumentType HelpOutputType { get { return DocumentType.PlainText; } }
		public DocumentType[] HelpSupplTablesType { get { return new DocumentType[0]; } }
		public string[] HelpSupplTables { get { return new string[0]; } }
		public int NumSupplTables { get { return 0; } }
		public string Name { get { return "Filter rows based on categorical column"; } }
		public string Heading { get { return "Filter rows"; } }
		public bool IsActive { get { return true; } }
		public float DisplayOrder { get { return 0; } }
		public string[] HelpDocuments { get { return new string[0]; } }
		public DocumentType[] HelpDocumentTypes { get { return new DocumentType[0]; } }
		public int NumDocuments { get { return 0; } }

		public int GetMaxThreads(ParametersWf parameters) {
			return 1;
		}

		public ParametersWf GetParameters(IMatrixData mdata, ref string errorString) {
			ParametersWf[] subParams = new ParametersWf[mdata.CategoryColumnCount];
			for (int i = 0; i < mdata.CategoryColumnCount; i++){
				string[] values = mdata.GetCategoryColumnValuesAt(i);
				int[] sel = values.Length == 1 ? new[]{0} : new int[0];
				subParams[i] =
					new ParametersWf(new ParameterWf[]{
						new MultiChoiceParamWf("Values", sel)
						{Values = values, Help = "The value that should be present to discard/keep the corresponding row."}
					});
			}
			return
				new ParametersWf(new ParameterWf[]{
					new SingleChoiceWithSubParamsWf("Column"){
						Values = mdata.CategoryColumnNames, SubParams = subParams,
						Help = "The categorical column that the filtering should be based on.", ParamNameWidth = 50, TotalWidth = 731
					},
					new SingleChoiceParamWf("Mode"){
						Values = new[]{"Remove matching rows", "Keep matching rows"},
						Help =
							"If 'Remove matching rows' is selected, rows having the values specified above will be removed while " +
								"all other rows will be kept. If 'Keep matching rows' is selected, the opposite will happen."
					},
					PerseusPluginUtils.GetFilterModeParam(true)
				});
		}

		public void ProcessData(IMatrixData mdata, ParametersWf param, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo){
				SingleChoiceWithSubParamsWf p = param.GetSingleChoiceWithSubParams("Column");
			int colInd = p.Value;
			if (colInd < 0){
				processInfo.ErrString = "No categorical columns available.";
				return;
			}
			MultiChoiceParamWf mcp = p.GetSubParameters().GetMultiChoiceParam("Values");
			int[] inds = mcp.Value;
			if (inds.Length == 0){
				processInfo.ErrString = "Please select at least one term for filtering.";
				return;
			}
			string[] values = new string[inds.Length];
			for (int i = 0; i < values.Length; i++){
				values[i] = mdata.GetCategoryColumnValuesAt(colInd)[inds[i]];
			}
			HashSet<string> value = new HashSet<string>(values);
			bool remove = param.GetSingleChoiceParam("Mode").Value == 0;
			string[][] cats = mdata.GetCategoryColumnAt(colInd);
			List<int> valids = new List<int>();
			for (int i = 0; i < cats.Length; i++){
				bool valid = true;
				foreach (string w in cats[i]){
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