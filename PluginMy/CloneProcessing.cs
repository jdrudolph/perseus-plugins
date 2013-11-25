using System.Drawing;
using BasicLib.ParamWf;
using BasicLib.Util;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;

namespace PluginMy{
	public class CloneProcessing : IMatrixProcessing{
		public bool HasButton { get { return false; } }
		public Image ButtonImage { get { return null; } }
		public string HelpDescription { get { return "A copy of the input matrix is generated."; } }
		public string HelpOutput { get { return "Same as input matrix."; } }
		public DocumentType HelpDescriptionType { get { return DocumentType.PlainText; } }
		public DocumentType HelpOutputType { get { return DocumentType.PlainText; } }
		public DocumentType[] HelpSupplTablesType { get { return new DocumentType[0]; } }
		public string[] HelpSupplTables { get { return new string[0]; } }
		public int NumSupplTables { get { return 0; } }
		public string Name { get { return "Subtract constant"; } }
		public string Heading { get { return "My plugins"; } }
		public bool IsActive { get { return true; } }
		public float DisplayOrder { get { return 100; } }
		public string[] HelpDocuments { get { return new string[0]; } }
		public DocumentType[] HelpDocumentTypes { get { return new DocumentType[0]; } }
		public int NumDocuments { get { return 0; } }

		public int GetMaxThreads(ParametersWf parameters) {
			return 1;
		}

		public void ProcessData(IMatrixData mdata, ParametersWf param, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo){
			double shift = param.GetDoubleParam("shift").Value;
			for (int i = 0; i < mdata.RowCount; i++){
				for(int j = 0;j < mdata.ExpressionColumnCount; j++){
					mdata[i, j] -= (float)shift;
				}
			}
		}

		public ParametersWf GetParameters(IMatrixData mdata, ref string errorString) {
			return new ParametersWf(new ParameterWf[]{
				new DoubleParamWf("shift", 0){Help = "This value is subtracted from each cell."} 
			});
		}
	}
}