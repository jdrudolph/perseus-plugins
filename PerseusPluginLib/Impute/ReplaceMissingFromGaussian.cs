using System.Collections.Generic;
using System.Drawing;
using BaseLib.Param;
using BaseLibS.Num;
using BaseLibS.Param;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;
using PerseusPluginLib.Properties;

namespace PerseusPluginLib.Impute{
	public class ReplaceMissingFromGaussian : IMatrixProcessing{
		public bool HasButton { get { return true; } }
		public Bitmap DisplayImage { get { return Resources.histo; } }
		public string HelpOutput { get { return ""; } }
		public int NumSupplTables { get { return 0; } }
		public string[] HelpSupplTables { get { return new string[0]; } }
		public string Name { get { return "Replace missing values from normal distribution"; } }
		public string Heading { get { return "Imputation"; } }
		public bool IsActive { get { return true; } }
		public float DisplayRank { get { return 0; } }
		public string[] HelpDocuments { get { return new string[0]; } }
		public int NumDocuments { get { return 0; } }

		public string Url{
			get{
				return
					"http://coxdocs.org/doku.php?id=perseus:user:activities:MatrixProcessing:Imputation:ReplaceMissingFromGaussian";
			}
		}

		public string Description{
			get{
				return
					"Missing values will be replaced by random numbers that are drawn from a normal distribution. The parameters of this" +
						" distribution can be optimized to simulate a typical abundance region that the missing values would have if they " +
						"had been measured. In the absence of any a priori knowledge, the distribution of random numbers should be " +
						"similar to the valid values. Often, missing values represent low abundance measurements. The default " +
						"values are chosen to mimic this case.";
			}
		}

		public int GetMaxThreads(Parameters parameters) { return 1; }

		public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo){
			double width = param.GetParam<double>("Width").Value;
			double shift = param.GetParam<double>("Down shift").Value;
			bool separateColumns = param.GetParam<int>("Mode").Value == 1;
			int[] cols = param.GetParam<int[]>("Columns").Value;
			if (separateColumns){
				ReplaceMissingsByGaussianByColumn(width, shift, mdata, cols);
			} else{
				ReplaceMissingsByGaussianWholeMatrix(width, shift, mdata, cols);
			}
		}

		public Parameters GetParameters(IMatrixData mdata, ref string errorString){
			return
				new Parameters(new Parameter[]{
					new DoubleParam("Width", 0.3){
						Help =
							"The width of the gaussian distibution relative to the standard deviation of measured values. A value of 0.5 " +
								"would mean that the width of the distribution used for drawing random numbers is half of the standard " +
								"deviation of the data."
					},
					new DoubleParam("Down shift", 1.8){
						Help =
							"The amount by which the distribution used for the random numbers is shifted downward. This is in units of the" +
								" standard deviation of the valid data."
					},
					new SingleChoiceParam("Mode", 1){Values = new[]{"Total matrix", "Separately for each column"}},
					new MultiChoiceParam("Columns", ArrayUtils.ConsecutiveInts(mdata.ColumnCount)){Values = mdata.ColumnNames}
				});
		}

		public static void ReplaceMissingsByGaussianByColumn(double width, double shift, IMatrixData data, int[] colInds){
			foreach (int colInd in colInds){
				ReplaceMissingsByGaussianForOneColumn(width, shift, data, colInd);
			}
		}

		private static void ReplaceMissingsByGaussianForOneColumn(double width, double shift, IMatrixData data, int colInd){
			List<float> allValues = new List<float>();
			for (int i = 0; i < data.RowCount; i++){
				float x = data.Values[i, colInd];
				if (!float.IsNaN(x) && !float.IsInfinity(x)){
					allValues.Add(x);
				}
			}
			double stddev;
			double mean = ArrayUtils.MeanAndStddev(allValues.ToArray(), out stddev);
			double m = mean - shift*stddev;
			double s = stddev*width;
			Random2 r = new Random2();
			for (int i = 0; i < data.RowCount; i++){
				if (float.IsNaN(data.Values[i, colInd]) || float.IsInfinity(data.Values[i, colInd])){
					data.Values[i, colInd] = (float) r.NextGaussian(m, s);
					data.IsImputed[i, colInd] = true;
				}
			}
		}

		public static void ReplaceMissingsByGaussianWholeMatrix(double width, double shift, IMatrixData data, int[] colInds){
			List<float> allValues = new List<float>();
			for (int i = 0; i < data.RowCount; i++){
				foreach (int t in colInds){
					float x = data.Values[i, t];
					if (!float.IsNaN(x) && !float.IsInfinity(x)){
						allValues.Add(x);
					}
				}
			}
			double stddev;
			double mean = ArrayUtils.MeanAndStddev(allValues.ToArray(), out stddev);
			double m = mean - shift*stddev;
			double s = stddev*width;
			Random2 r = new Random2();
			for (int i = 0; i < data.RowCount; i++){
				foreach (int t in colInds){
					if (float.IsNaN(data.Values[i, t]) || float.IsInfinity(data.Values[i, t])){
						data.Values[i, t] = (float) r.NextGaussian(m, s);
						data.IsImputed[i, t] = true;
					}
				}
			}
		}
	}
}