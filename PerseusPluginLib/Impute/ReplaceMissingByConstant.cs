using System.Collections.Generic;
using System.Drawing;
using BaseLib.Param;
using BaseLibS.Num;
using BaseLibS.Param;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;

namespace PerseusPluginLib.Impute{
	public class ReplaceMissingByConstant : IMatrixProcessing{
		public bool HasButton{
			get { return false; }
		}

		public Bitmap DisplayImage{
			get { return null; }
		}

		public string Description{
			get { return "Replaces all missing values in expression columns with a constant."; }
		}

		public string HelpOutput{
			get { return "Same matrix but with missing values replaced."; }
		}

		public string[] HelpSupplTables{
			get { return new string[0]; }
		}

		public int NumSupplTables{
			get { return 0; }
		}

		public string Name{
			get { return "Replace missing values by constant"; }
		}

		public string Heading{
			get { return "Imputation"; }
		}

		public bool IsActive{
			get { return true; }
		}

		public float DisplayRank{
			get { return 1; }
		}

		public string[] HelpDocuments{
			get { return new string[0]; }
		}

		public int NumDocuments{
			get { return 0; }
		}

		public string Url{
			get{
				return
					"http://coxdocs.org/doku.php?id=perseus:user:activities:MatrixProcessing:Imputation:ReplaceMissingByConstant";
			}
		}

		public int GetMaxThreads(Parameters parameters){
			return 1;
		}

		public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables,
		                        ref IDocumentData[] documents, ProcessInfo processInfo){
			float value = (float) param.GetParam<double>("Value").Value;
			int[] inds = param.GetParam<int[]>("Columns").Value;
			List<int> mainInds = new List<int>();
			List<int> numInds = new List<int>();
			foreach (int ind in inds){
				if (ind < mdata.ColumnCount){
					mainInds.Add(ind);
				} else{
					numInds.Add(ind - mdata.ColumnCount);
				}
			}
			ReplaceMissingsByVal(value, mdata, mainInds, numInds);
		}

		public Parameters GetParameters(IMatrixData mdata, ref string errorString){
			return
				new Parameters(new Parameter[]{
					new DoubleParam("Value", 0){Help = "The value that is going to be filled in for missing values."},
					new MultiChoiceParam("Columns", ArrayUtils.ConsecutiveInts(mdata.ColumnCount)){
						Values = ArrayUtils.Concat(mdata.ColumnNames, mdata.NumericColumnNames)
					}
				});
		}

		private static void ReplaceMissingsByVal(float value, IMatrixData data, IEnumerable<int> mainInds,
		                                         IEnumerable<int> numInds){
			foreach (int j in mainInds){
				for (int i = 0; i < data.RowCount; i++){
					if (float.IsNaN(data.Values[i, j])){
						data.Values[i, j] = value;
						data.IsImputed[i, j] = true;
					}
				}
			}
			foreach (int j in numInds){
				for (int i = 0; i < data.RowCount; i++){
					if (double.IsNaN(data.NumericColumns[j][i])){
						data.NumericColumns[j][i] = value;
					}
				}
			}
		}
	}
}