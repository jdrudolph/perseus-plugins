using System.Drawing;
using BaseLib.Param;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;

namespace PerseusPluginLib.Quality{
	public class FilterQuality : IMatrixProcessing{
		public bool HasButton { get { return false; } }
		public Bitmap DisplayImage { get { return null; } }
		public string HelpDescription { get { return ""; } }
		public string HelpOutput { get { return ""; } }
		public string[] HelpSupplTables { get { return new string[0]; } }
		public int NumSupplTables { get { return 0; } }
		public string Name { get { return "Filter quality"; } }
		public string Heading { get { return "Quality"; } }
		public bool IsActive { get { return true; } }
		public float DisplayOrder { get { return 0; } }
		public string[] HelpDocuments { get { return new string[0]; } }
		public int NumDocuments { get { return 0; } }

		public int GetMaxThreads(Parameters parameters) {
			return 1;
		}

		public Parameters GetParameters(IMatrixData mdata, ref string errorString) {
			return new Parameters(new Parameter[] { new DoubleParam("Threshold", 0) });
		}

		public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo){
			if (!mdata.HasQuality){
				processInfo.ErrString = "No quality data loaded.";
				return;
			}
			double threshold = param.GetDoubleParam("Threshold").Value;
			for (int i = 0; i < mdata.RowCount; i++){
				for (int j = 0; j < mdata.ExpressionColumnCount; j++){
					float value = mdata.QualityValues[i, j];
					if (mdata.QualityBiggerIsBetter){
						if (value < threshold){
							mdata[i, j] = float.NaN;
						}
					} else{
						if (value > threshold){
							mdata[i, j] = float.NaN;
						}
					}
				}
			}
		}
	}
}