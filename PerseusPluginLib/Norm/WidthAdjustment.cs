using System.Collections.Generic;
using System.Drawing;
using BaseLibS.Num;
using BaseLibS.Param;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;

namespace PerseusPluginLib.Norm{
	public class WidthAdjustment : IMatrixProcessing{
		public bool HasButton { get { return false; } }
		public Bitmap DisplayImage { get { return null; } }

		public string Description{
			get{
				return "The first, second and third quartile (q1, q2, q3) are calculated from the " +
					"distribution of all values. The second quartile (which is the median) is subtracted from each value " +
					"to center the distribution. Then we divide by the width in an asymmetric way. All values that are " +
					"positive after subtraction of the median are divided by q3 – q2 while all negative values are divided by q2 – q1.";
			}
		}

		public string HelpOutput { get { return ""; } }
		public string[] HelpSupplTables { get { return new string[0]; } }
		public int NumSupplTables { get { return 0; } }
		public string Name { get { return "Width adjustment"; } }
		public string Heading { get { return "Normalization"; } }
		public bool IsActive { get { return true; } }
		public float DisplayRank { get { return -7; } }
		public string[] HelpDocuments { get { return new string[0]; } }
		public int NumDocuments { get { return 0; } }

		public string Url{
			get{
				return
					"http://141.61.102.17/perseus_doku/doku.php?id=perseus:activities:MatrixProcessing:Normalization:WidthAdjustment";
			}
		}

		public int GetMaxThreads(Parameters parameters) { return 1; }

		public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo){
			double[] dm = new double[mdata.ColumnCount];
			double[] dp = new double[mdata.ColumnCount];
			for (int i = 0; i < mdata.ColumnCount; i++){
				List<float> v = new List<float>();
				foreach (float f in mdata.Values.GetColumn(i)){
					if (!float.IsNaN(f) && !float.IsInfinity(f)){
						v.Add(f);
					}
				}
				float[] d = v.ToArray();
				float[] q = ArrayUtils.Quantiles(d, new[]{0.25, 0.5, 0.75});
				for (int j = 0; j < mdata.RowCount; j++){
					mdata.Values[j, i] -= q[1];
				}
				dm[i] = q[1] - q[0];
				dp[i] = q[2] - q[1];
			}
			double adm = ArrayUtils.Median(dm);
			double adp = ArrayUtils.Median(dp);
			for (int i = 0; i < mdata.ColumnCount; i++){
				for (int j = 0; j < mdata.RowCount; j++){
					if (mdata.Values[j, i] < 0){
						mdata.Values[j, i] = (float) (mdata.Values[j, i]*adm/dm[i]);
					} else{
						mdata.Values[j, i] = (float) (mdata.Values[j, i]*adp/dp[i]);
					}
				}
			}
		}

		public Parameters GetParameters(IMatrixData mdata, ref string errorString) { return new Parameters(new Parameter[0]); }
	}
}