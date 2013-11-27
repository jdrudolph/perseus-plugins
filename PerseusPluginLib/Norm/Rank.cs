using System.Collections.Generic;
using System.Drawing;
using BaseLib.ParamWf;
using BaseLib.Util;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;

namespace PerseusPluginLib.Norm{
	internal class Rank : IMatrixProcessing{
		public bool HasButton { get { return false; } }
		public Image ButtonImage { get { return null; } }
		public string HelpDescription { get { return "The values in each row/column are replaced by ranks."; } }
		public string HelpOutput { get { return "Normalized expression matrix."; } }
		public DocumentType HelpDescriptionType { get { return DocumentType.PlainText; } }
		public DocumentType HelpOutputType { get { return DocumentType.PlainText; } }
		public DocumentType[] HelpSupplTablesType { get { return new DocumentType[0]; } }
		public string[] HelpSupplTables { get { return new string[0]; } }
		public int NumSupplTables { get { return 0; } }
		public string Name { get { return "Rank"; } }
		public string Heading { get { return "Normalization"; } }
		public bool IsActive { get { return true; } }
		public float DisplayOrder { get { return -9; } }
		public string[] HelpDocuments { get { return new string[0]; } }
		public DocumentType[] HelpDocumentTypes { get { return new DocumentType[0]; } }
		public int NumDocuments { get { return 0; } }

		public int GetMaxThreads(ParametersWf parameters) {
			return 1;
		}

		public void ProcessData(IMatrixData mdata, ParametersWf param, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo){
				SingleChoiceParamWf access = param.GetSingleChoiceParam("Matrix access");
			bool rows = access.Value == 0;
			Rank1(rows, mdata);
		}

		public ParametersWf GetParameters(IMatrixData mdata, ref string errorString) {
			return
				new ParametersWf(new ParameterWf[]{
					new SingleChoiceParamWf("Matrix access"){
						Values = new[]{"Rows", "Columns"},
						Help = "Specifies if the analysis is performed on the rows or the columns of the matrix."
					}
				});
		}

		public static void Rank1(bool rows, IMatrixData data){
			if (rows){
				for (int i = 0; i < data.RowCount; i++){
					List<double> vals = new List<double>();
					List<int> indices = new List<int>();
					for (int j = 0; j < data.ExpressionColumnCount; j++){
						double q = data[i, j];
						if (!double.IsNaN(q)){
							vals.Add(q);
							indices.Add(j);
						}
					}
					double[] ranks = ArrayUtils.Rank(vals);
					for (int j = 0; j < data.ExpressionColumnCount; j++){
						data[i, j] = float.NaN;
					}
					for (int j = 0; j < ranks.Length; j++){
						data[i, indices[j]] = (float) ranks[j];
					}
				}
			} else{
				for (int j = 0; j < data.ExpressionColumnCount; j++){
					List<double> vals = new List<double>();
					List<int> indices = new List<int>();
					for (int i = 0; i < data.RowCount; i++){
						double q = data[i, j];
						if (!double.IsNaN(q)){
							vals.Add(q);
							indices.Add(i);
						}
					}
					double[] ranks = ArrayUtils.Rank(vals);
					for (int i = 0; i < data.RowCount; i++){
						data[i, j] = float.NaN;
					}
					for (int i = 0; i < ranks.Length; i++){
						data[indices[i], j] = (float) ranks[i];
					}
				}
			}
		}
	}
}