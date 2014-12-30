using System.Collections.Generic;
using System.Drawing;
using BaseLib.Param;
using BaseLibS.Param;
using BaseLibS.Util;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;

namespace PerseusPluginLib.Norm{
	public class UnZScore : IMatrixProcessing{
		public bool HasButton { get { return false; } }
		public Bitmap DisplayImage { get { return null; } }
		public string Name { get { return "Un-Z-score"; } }
		public string Heading { get { return "Normalization"; } }
		public bool IsActive { get { return true; } }
		public float DisplayRank { get { return 50; } }
		public string HelpOutput { get { return "Normalized expression matrix."; } }
		public string[] HelpSupplTables { get { return new string[0]; } }
		public int NumSupplTables { get { return 0; } }
		public string[] HelpDocuments { get { return new string[0]; } }
		public int NumDocuments { get { return 0; } }
		public string Url { get { return "http://141.61.102.17/perseus_doku/doku.php?id=perseus:activities:MatrixProcessing:Normalization:UnZScore"; } }
		public string Description
		{
			get{
				return "Providing the means and standard deviations used in a z-score transformation the data is " +
					"transformed back to what it was before z-scoring.";
			}
		}

		public int GetMaxThreads(Parameters parameters){
			return int.MaxValue;
		}

		public Parameters GetParameters(IMatrixData mdata, ref string errorString){
			int rowMeanInd = 0;
			int rowDevInd = 0;
			for (int i = 0; i < mdata.NumericColumnCount; i++){
				if (mdata.NumericColumnNames[i].ToLower().Equals("mean")){
					rowMeanInd = i;
				}
				if (mdata.NumericColumnNames[i].ToLower().Equals("std. dev.")){
					rowDevInd = i;
				}
			}
			Parameters rowSubParams =
				new Parameters(new Parameter[]{
					new SingleChoiceParam("Mean", rowMeanInd){Values = mdata.NumericColumnNames},
					new SingleChoiceParam("Std. dev.", rowDevInd){Values = mdata.NumericColumnNames}
				});
			int colMeanInd = 0;
			int colDevInd = 0;
			for (int i = 0; i < mdata.NumericRowCount; i++){
				if (mdata.NumericRowNames[i].ToLower().Equals("mean")){
					colMeanInd = i;
				}
				if (mdata.NumericRowNames[i].ToLower().Equals("std. dev.")){
					colDevInd = i;
				}
			}
			Parameters columnSubParams =
				new Parameters(new Parameter[]{
					new SingleChoiceParam("Mean", colMeanInd){Values = mdata.NumericRowNames},
					new SingleChoiceParam("Std. dev.", colDevInd){Values = mdata.NumericRowNames}
				});
			return
				new Parameters(new Parameter[]{
					new SingleChoiceWithSubParams("Matrix access"){
						Values = new[]{"Rows", "Columns"}, ParamNameWidth = 136, TotalWidth = 731,
						Help = "Specifies if the z-scoring is performed on the rows or the columns of the matrix.",
						SubParams = new[]{rowSubParams, columnSubParams}
					}
				});
		}

		public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo){
			SingleChoiceWithSubParams access = param.GetSingleChoiceWithSubParams("Matrix access");
			bool rows = access.Value == 0;
			int meanInd = access.GetSubParameters().GetSingleChoiceParam("Mean").Value;
			int devInd = access.GetSubParameters().GetSingleChoiceParam("Std. dev.").Value;
			double[] means = rows ? mdata.NumericColumns[meanInd] : mdata.NumericRows[meanInd];
			double[] stddevs = rows ? mdata.NumericColumns[devInd] : mdata.NumericRows[devInd];
			UnZscore(rows, mdata, processInfo.NumThreads, means, stddevs);
		}

		private static void UnZscore(bool rows, IMatrixData data, int nthreads, double[] means, double[] stddevs){
			if (rows){
				double[] doubles = means;
				double[] stddevs1 = stddevs;
				new ThreadDistributor(nthreads, data.RowCount, i => Calc1(i, data, doubles, stddevs1)).Start();
			} else{
				double[] doubles = means;
				double[] stddevs1 = stddevs;
				new ThreadDistributor(nthreads, data.ColumnCount, j => Calc2(j, data, doubles, stddevs1)).Start();
			}
		}

		private static void Calc1(int i, IMatrixData data, IList<double> means, IList<double> stddevs){
			double[] vals = new double[data.ColumnCount];
			for (int j = 0; j < data.ColumnCount; j++){
				vals[j] = data[i, j];
			}
			double stddev = stddevs[i];
			double mean = means[i];
			for (int j = 0; j < data.ColumnCount; j++){
				data[i, j] = (float) ((data[i, j]*stddev) + mean);
			}
		}

		private static void Calc2(int j, IMatrixData data, IList<double> means, IList<double> stddevs){
			double[] vals = new double[data.RowCount];
			for (int i = 0; i < data.RowCount; i++){
				vals[i] = data[i, j];
			}
			double stddev = stddevs[j];
			double mean = means[j];
			for (int i = 0; i < data.RowCount; i++){
				data[i, j] = (float) ((data[i, j]*stddev) + mean);
			}
		}
	}
}