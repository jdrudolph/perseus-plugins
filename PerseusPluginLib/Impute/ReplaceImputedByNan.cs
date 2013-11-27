using System.Drawing;
using BaseLib.ParamWf;
using BaseLib.Util;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;

namespace PerseusPluginLib.Impute{
	public class ReplaceImputedByNan : IMatrixProcessing{
		public bool HasButton { get { return false; } }
		public Image ButtonImage { get { return null; } }
		public string HelpDescription { get { return "Replaces all values that have been imputed with NaN."; } }
		public string HelpOutput { get { return "Same matrix but with imputed values deleted."; } }
		public string[] HelpSupplTables { get { return new string[0]; } }
		public int NumSupplTables { get { return 0; } }
		public string Name { get { return "Replace imputed values by NaN"; } }
		public string Heading { get { return "Imputation"; } }
		public bool IsActive { get { return true; } }
		public float DisplayOrder { get { return 1; } }
		public string[] HelpDocuments { get { return new string[0]; } }
		public DocumentType[] HelpDocumentTypes { get { return new DocumentType[0]; } }
		public int NumDocuments { get { return 0; } }

		public int GetMaxThreads(ParametersWf parameters) {
			return 1;
		}

		public ParametersWf GetParameters(IMatrixData mdata, ref string errorString) {
			return new ParametersWf(new ParameterWf[] { });
		}

		public void ProcessData(IMatrixData mdata, ParametersWf param, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo){
			Replace(mdata);
		}

		public static void Replace(IMatrixData data){
			for (int i = 0; i < data.RowCount; i++){
				for (int j = 0; j < data.ExpressionColumnCount; j++){
					if (data.IsImputed[i, j]){
						data[i, j] = float.NaN;
					}
				}
			}
		}
	}
}