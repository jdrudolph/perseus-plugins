using System;
using System.Collections.Generic;
using System.Drawing;
using BaseLib.Param;
using BaseLib.Util;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;

namespace PerseusPluginLib.Norm{
	public class SubtractColumn : IMatrixProcessing{
		public bool HasButton { get { return false; } }
		public Bitmap DisplayImage { get { return null; } }
		public string HelpDescription { get { return "Subtract the specified column from all other columns."; } }
		public string HelpOutput { get { return "Normalized expression matrix."; } }
		public string[] HelpSupplTables { get { return new string[0]; } }
		public int NumSupplTables { get { return 0; } }
		public string Name { get { return "Subtract column"; } }
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
			double[] controlValues = GetControlValues(mdata, param);
			Func<double, double, double> f = (x, y) => (x - y);
			int[] exCols;
			int[] numCols;
			GetCols(mdata, param, out exCols, out numCols);
			foreach (int exCol in exCols){
				ApplyExp(exCol, mdata, f, controlValues);
			}
			foreach (int numCol in numCols){
				ApplyNum(numCol, mdata, f, controlValues);
			}
		}

		public static void ApplyExp(int exCol, IMatrixData mdata, Func<double, double, double> func,
			IList<double> controlValues){
			for (int i = 0; i < mdata.RowCount; i++){
				mdata[i, exCol] = (float) func(mdata[i, exCol], controlValues[i]);
			}
		}

		public static void ApplyNum(int numCol, IMatrixData mdata, Func<double, double, double> func,
			IList<double> controlValues){
			for (int i = 0; i < mdata.RowCount; i++){
				mdata.NumericColumns[numCol][i] = (float) func(mdata.NumericColumns[numCol][i], controlValues[i]);
			}
		}

		public static void GetCols(IMatrixData mdata, Parameters param, out int[] exCols, out int[] numCols) {
			List<int> exCols1 = new List<int>();
			List<int> numCols1 = new List<int>();
			int[] cols = param.GetMultiChoiceParam("Columns").Value;
			foreach (int col in cols){
				if (col < mdata.ExpressionColumnCount){
					exCols1.Add(col);
				} else{
					numCols1.Add(col - mdata.ExpressionColumnCount);
				}
			}
			exCols = exCols1.ToArray();
			numCols = numCols1.ToArray();
		}

		public static double[] GetControlValues(IMatrixData mdata, Parameters param){
			int controlIndex = param.GetSingleChoiceParam("Control column").Value;
			if (controlIndex < mdata.ExpressionColumnCount){
				return ArrayUtils.ToDoubles(mdata.GetExpressionColumn(controlIndex));
			}
			controlIndex -= mdata.ExpressionColumnCount;
			return mdata.NumericColumns[controlIndex];
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
							"Select here the expression and/or numeric colums from which the values in the 'control column' " +
								"will be subtracted."
					},
					new SingleChoiceParam("Control column"){
						Values = controlChoice,
						Help = "The values in this column will be " + "subtracted from all columns selected in the field 'Columns'"
					}
				});
		}
	}
}