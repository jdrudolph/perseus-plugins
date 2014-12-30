using System;
using System.Drawing;
using BaseLib.Param;
using BaseLibS.Param;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;

namespace PerseusPluginLib.Norm{
	public class UnitVector : IMatrixProcessing{
		public bool HasButton { get { return false; } }
		public Bitmap DisplayImage { get { return null; } }
		public string HelpOutput { get { return "Normalized expression matrix."; } }
		public string[] HelpSupplTables { get { return new string[0]; } }
		public int NumSupplTables { get { return 0; } }
		public string[] HelpDocuments { get { return new string[0]; } }
		public int NumDocuments { get { return 0; } }
		public string Url { get { return "http://141.61.102.17/perseus_doku/doku.php?id=perseus:activities:MatrixProcessing:Normalization:UnitVector"; } }
		public string Name { get { return "Unit vectors"; } }
		public string Heading { get { return "Normalization"; } }
		public bool IsActive { get { return true; } }
		public float DisplayRank { get { return -8; } }

		public string Description{
			get{
				return
					"The rows/columns are regarded as high-dimensional vectors. They are divided by their lengts resulting in a matrix of unit vectors.";
			}
		}

		public int GetMaxThreads(Parameters parameters) { return 1; }

		public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo){
			SingleChoiceParam access = param.GetSingleChoiceParam("Matrix access");
			bool rows = access.Value == 0;
			UnitVectors(rows, mdata);
		}

		public Parameters GetParameters(IMatrixData mdata, ref string errorString){
			return
				new Parameters(new Parameter[]{
					new SingleChoiceParam("Matrix access"){
						Values = new[]{"Rows", "Columns"},
						Help = "Specifies if the analysis is performed on the rows or the columns of the matrix."
					}
				});
		}

		public void UnitVectors(bool rows, IMatrixData data){
			if (rows){
				for (int i = 0; i < data.RowCount; i++){
					double len = 0;
					for (int j = 0; j < data.ColumnCount; j++){
						double q = data[i, j];
						len += q*q;
					}
					len = Math.Sqrt(len);
					for (int j = 0; j < data.ColumnCount; j++){
						data[i, j] /= (float) len;
					}
				}
			} else{
				for (int j = 0; j < data.ColumnCount; j++){
					double len = 0;
					for (int i = 0; i < data.RowCount; i++){
						double q = data[i, j];
						len += q*q;
					}
					len = Math.Sqrt(len);
					for (int i = 0; i < data.RowCount; i++){
						data[i, j] /= (float) len;
					}
				}
			}
		}
	}
}