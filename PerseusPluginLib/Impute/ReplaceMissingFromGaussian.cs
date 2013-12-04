using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BaseLib.Num;
using BaseLib.Param;
using BaseLib.Util;
using BaseLib.Wpf;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;
using PerseusPluginLib.Properties;

namespace PerseusPluginLib.Impute{
	public class ReplaceMissingFromGaussian : IMatrixProcessing{
		public bool HasButton { get { return true; } }
		public BitmapSource DisplayImage { get { return WpfUtils.LoadBitmap(Resources.histo); } }
		public DocumentType HelpDescriptionType { get { return DocumentType.PlainText; } }
		public string HelpOutput { get { return ""; } }
		public DocumentType HelpOutputType { get { return DocumentType.PlainText; } }
		public int NumSupplTables { get { return 0; } }
		public string[] HelpSupplTables { get { return new string[0]; } }
		public DocumentType[] HelpSupplTablesType { get { return new DocumentType[0]; } }
		public string Name { get { return "Replace missing values from normal distribution"; } }
		public string Heading { get { return "Imputation"; } }
		public bool IsActive { get { return true; } }
		public float DisplayOrder { get { return 0; } }
		public string[] HelpDocuments { get { return new string[0]; } }
		public DocumentType[] HelpDocumentTypes { get { return new DocumentType[0]; } }
		public int NumDocuments { get { return 0; } }
		public string HelpDescription{
			get{
				return
					"Missing values will be replaced by random numbers that are drawn from a normal distribution. The parameters of this" +
						" distribution can be optimized to simulate a typical abundance region that the missing values would have if they " +
						"had been measured. In the absence of any a priori knowledge, the distribution of random numbers should be " +
						"similar to the valid values. Often, missing values represent low abundance measurements. The default " +
						"values are chosen to mimic this case.";
			}
		}

		public int GetMaxThreads(Parameters parameters) {
			return 1;
		}

		public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo){
			double width = param.GetDoubleParam("Width").Value;
			double shift = param.GetDoubleParam("Down shift").Value;
			bool separateColumns = param.GetSingleChoiceParam("Mode").Value == 1;
			if (separateColumns){
				ReplaceMissingsByGaussianByColumn(width, shift, mdata);
			} else{
				ReplaceMissingsByGaussianWholeMatrix(width, shift, mdata);
			}
		}

		public Parameters GetParameters(IMatrixData mdata, ref string errorString) {
			return
				new Parameters(new Parameter[]{
					new DoubleParam("Width", 0.3){
						Help =
							"The width of the gaussian distibution relative to the standard deviation of measured values. A value of 0.5 " +
								"would mean that the width of the distribution used for drawing random numbers is half od the standard " +
								"deviation of the data."
					},
					new DoubleParam("Down shift", 1.8){
						Help =
							"The amount by which the distribution used for the random numbers is shifted downwards. This is in units of the" +
								" standard deviation if the valid data."
					},
					new SingleChoiceParam("Mode", 1){Values = new[]{"Total matrix", "Separately for each columns"}}
				});
		}

		public static void ReplaceMissingsByGaussianByColumn(double width, double shift, IMatrixData data){
			int[] colInds = ArrayUtils.ConsecutiveInts(data.ExpressionColumnCount);
			foreach (int colInd in colInds){
				ReplaceMissingsByGaussianForOneColumn(width, shift, data, colInd);
			}
		}

		private static void ReplaceMissingsByGaussianForOneColumn(double width, double shift, IMatrixData data, int colInd){
			List<float> allValues = new List<float>();
			for (int i = 0; i < data.RowCount; i++){
				float x = data[i, colInd];
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
				if (float.IsNaN(data[i, colInd]) || float.IsInfinity(data[i, colInd])){
					data[i, colInd] = (float) r.NextGaussian(m, s);
					data.IsImputed[i, colInd] = true;
				}
			}
		}

		public static void ReplaceMissingsByGaussianWholeMatrix(double width, double shift, IMatrixData data){
			int[] colInds = ArrayUtils.ConsecutiveInts(data.ExpressionColumnCount);
			List<float> allValues = new List<float>();
			for (int i = 0; i < data.RowCount; i++){
				foreach (int t in colInds){
					float x = data[i, t];
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
					if (float.IsNaN(data[i, t]) || float.IsInfinity(data[i, t])){
						data[i, t] = (float) r.NextGaussian(m, s);
						data.IsImputed[i, t] = true;
					}
				}
			}
		}
	}
}