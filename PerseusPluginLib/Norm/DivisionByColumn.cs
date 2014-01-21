using System.Drawing;
using BaseLib.Param;
using BaseLib.Util;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;

namespace PerseusPluginLib.Norm{
	public class DivisionByColumn : IMatrixProcessing{
		public bool HasButton { get { return false; } }
		public Bitmap DisplayImage { get { return null; } }
		public string HelpDescription { get { return "Divide all columns by the specified column."; } }
		public string HelpOutput { get { return "Normalized expression matrix."; } }
		public string[] HelpSupplTables { get { return new string[0]; } }
		public int NumSupplTables { get { return 0; } }
		public string Name { get { return "Divide by column"; } }
		public string Heading { get { return "Normalization"; } }
		public bool IsActive { get { return true; } }
		public float DisplayOrder { get { return 0; } }
		public string[] HelpDocuments { get { return new string[0]; } }
		public int NumDocuments { get { return 0; } }

		public int GetMaxThreads(Parameters parameters){
			return 1;
		}

		public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo){
			SubtractColumn.ProcessData(mdata, param, (x, y) => (x/y));
		}

		public Parameters GetParameters(IMatrixData mdata, ref string errorString){
			string[] values = ArrayUtils.Concat(mdata.ExpressionColumnNames, mdata.NumericColumnNames);
			int[] sel = ArrayUtils.ConsecutiveInts(mdata.ExpressionColumnCount);
			string[] controlChoice = ArrayUtils.Concat(mdata.ExpressionColumnNames, mdata.NumericColumnNames);
			return
				new Parameters(new Parameter[]{
					new MultiChoiceParam("Columns"){
						Values = values, Value = sel,
						Help =
							"Select here the expression and/or numeric colums whose values will be divided by the values in the 'control column'."
					},
					new SingleChoiceParam("Control column"){
						Values = controlChoice,
						Help =
							"The values in the columns selected in the field 'Columns' will be divided by the values in the column selected here."
					}
				});
		}
	}
}