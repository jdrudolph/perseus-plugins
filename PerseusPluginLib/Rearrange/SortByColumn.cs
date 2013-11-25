using System.Drawing;
using BasicLib.ParamWf;
using BasicLib.Util;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;

namespace PerseusPluginLib.Rearrange{
	public class SortByColumn : IMatrixProcessing{
		public bool HasButton { get { return false; } }
		public Image ButtonImage { get { return null; } }
		public string HelpDescription { get { return "Simple sorting by a column."; } }
		public DocumentType HelpDescriptionType { get { return DocumentType.PlainText; } }
		public string HelpOutput { get { return "The same matrix but sorted by the specified column."; } }
		public string[] HelpSupplTables { get { return new string[0]; } }
		public DocumentType HelpOutputType { get { return DocumentType.PlainText; } }
		public DocumentType[] HelpSupplTablesType { get { return new DocumentType[0]; } }
		public int NumSupplTables { get { return 0; } }
		public string Name { get { return "Sort by column"; } }
		public string Heading { get { return "Matrix rearrangements"; } }
		public bool IsActive { get { return true; } }
		public float DisplayOrder { get { return 6; } }
		public string[] HelpDocuments { get { return new string[0]; } }
		public DocumentType[] HelpDocumentTypes { get { return new DocumentType[0]; } }
		public int NumDocuments { get { return 0; } }

		public int GetMaxThreads(ParametersWf parameters) {
			return 1;
		}

		public void ProcessData(IMatrixData mdata, ParametersWf param, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo){
			int ind = param.GetSingleChoiceParam("Column").Value;
			bool descending = param.GetBoolParam("Descending").Value;
			if (ind < mdata.ExpressionColumnCount){
				float[] v = mdata.GetExpressionColumn(ind);
				int[] o = ArrayUtils.Order(v);
				if (descending){
					ArrayUtils.Revert(o);
				}
				mdata.ExtractExpressionRows(o);
			} else{
				double[] v = mdata.NumericColumns[ind - mdata.ExpressionColumnCount];
				int[] o = ArrayUtils.Order(v);
				if (descending){
					ArrayUtils.Revert(o);
				}
				mdata.ExtractExpressionRows(o);
			}
		}

		public ParametersWf GetParameters(IMatrixData mdata, ref string errorString) {
			string[] choice = ArrayUtils.Concat(mdata.ExpressionColumnNames, mdata.NumericColumnNames);
			return
				new ParametersWf(new ParameterWf[]{
					new SingleChoiceParamWf("Column"){Values = choice, Help = "Select here the column that should be used for sorting."},
					new BoolParamWf("Descending"){Help = "If checked the values will be sorted largest to smallest."}
				});
		}
	}
}