using System.Collections.Generic;
using System.Windows.Media;
using BaseLib.Param;
using BaseLib.Util;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;

namespace PerseusPluginLib.Norm{
	public class WidthAdjustment : IMatrixProcessing{
		public bool HasButton { get { return false; } }
		public ImageSource DisplayImage { get { return null; } }
		public string HelpDescription{
			get{
				return "The first, second and third quartile (q1, q2, q3) are calculated from the " +
					"distribution of all values. The second quartile (which is the median) is subtracted from each value " +
					"to center the distribution. Then we divide by the width in an asymmetric way. All values that are " +
					"positive after subtraction of the median are divided by q3 – q2 while all negative values are divided by q2 – q1.";
			}
		}
		public string HelpOutput { get { return ""; } }
		public DocumentType HelpDescriptionType { get { return DocumentType.PlainText; } }
		public DocumentType HelpOutputType { get { return DocumentType.PlainText; } }
		public DocumentType[] HelpSupplTablesType { get { return new DocumentType[0]; } }
		public string[] HelpSupplTables { get { return new string[0]; } }
		public int NumSupplTables { get { return 0; } }
		public string Name { get { return "Width adjustment"; } }
		public string Heading { get { return "Normalization"; } }
		public bool IsActive { get { return true; } }
		public float DisplayOrder { get { return -7; } }
		public string[] HelpDocuments { get { return new string[0]; } }
		public DocumentType[] HelpDocumentTypes { get { return new DocumentType[0]; } }
		public int NumDocuments { get { return 0; } }

		public int GetMaxThreads(Parameters parameters) {
			return 1;
		}

		public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo){
			float[,] vals = mdata.ExpressionValues;
			double[] dm = new double[mdata.ExpressionColumnCount];
			double[] dp = new double[mdata.ExpressionColumnCount];
			for (int i = 0; i < mdata.ExpressionColumnCount; i++){
				List<float> v = new List<float>();
				foreach (float f in mdata.GetExpressionColumn(i)){
					if (!float.IsNaN(f) && !float.IsInfinity(f)){
						v.Add(f);
					}
				}
				float[] d = v.ToArray();
				float[] q = ArrayUtils.Quantiles(d, new[]{0.25, 0.5, 0.75});
				for (int j = 0; j < mdata.RowCount; j++){
					vals[j, i] -= q[1];
				}
				dm[i] = q[1] - q[0];
				dp[i] = q[2] - q[1];
			}
			double adm = ArrayUtils.Median(dm);
			double adp = ArrayUtils.Median(dp);
			for (int i = 0; i < mdata.ExpressionColumnCount; i++){
				for (int j = 0; j < mdata.RowCount; j++){
					if (vals[j, i] < 0){
						vals[j, i] = (float) (vals[j, i]*adm/dm[i]);
					} else{
						vals[j, i] = (float) (vals[j, i]*adp/dp[i]);
					}
				}
			}
		}

		public Parameters GetParameters(IMatrixData mdata, ref string errorString) {
			return new Parameters(new Parameter[0]);
		}
	}
}