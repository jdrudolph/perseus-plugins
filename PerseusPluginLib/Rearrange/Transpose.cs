using System.Collections.Generic;
using System.Drawing;
using BaseLib.Param;
using BaseLib.Util;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;
using PerseusApi.Utils;

namespace PerseusPluginLib.Rearrange{
	public class Transpose : IMatrixProcessing{
		public bool HasButton { get { return false; } }
		public Bitmap DisplayImage { get { return null; } }
		public string Description{
			get{
				return
					"The matrix of expression values is being transposed, i.e. rows become columns and columns become rows.";
			}
		}
		public string HelpOutput{
			get{
				return
					"The transpose of the matrix of expression values. One string column can be selected to become the new column names.";
			}
		}
		public string[] HelpSupplTables { get { return new string[0]; } }
		public int NumSupplTables { get { return 0; } }
		public string Name { get { return "Transpose"; } }
		public string Heading { get { return "Rearrange"; } }
		public bool IsActive { get { return true; } }
		public float DisplayOrder { get { return 5; } }
		public string[] HelpDocuments { get { return new string[0]; } }
		public int NumDocuments { get { return 0; } }

		public int GetMaxThreads(Parameters parameters) {
			return 1;
		}

		public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo){
			int nameCol = param.GetSingleChoiceParam("New column names").Value;
			List<string> colNames;
			if (nameCol >= 0){
				HashSet<string> taken = new HashSet<string>();
				colNames = new List<string>();
				foreach (string n in mdata.StringColumns[nameCol]){
                    string n1 = PerseusUtils.GetNextAvailableName(n, taken);
					taken.Add(n1);
					colNames.Add(n1);
				}
			} else{
				colNames = new List<string>();
				for (int i = 0; i < mdata.RowCount; i++){
					colNames.Add("Column" + (i + 1));
				}
			}
			List<string> rowNames = mdata.ExpressionColumnNames;
			mdata.SetData(mdata.Name, mdata.Description, colNames, colNames, ArrayUtils.Transpose(mdata.ExpressionValues),
				ArrayUtils.Transpose(mdata.IsImputed), ArrayUtils.Transpose(mdata.QualityValues), mdata.QualityName,
				mdata.QualityBiggerIsBetter, new List<string>(new[]{"Name"}), new List<string>(new[]{"Name"}),
				new List<string[]>(new[]{rowNames.ToArray()}), mdata.CategoryRowNames, mdata.CategoryRowDescriptions,
				GetCategoryRows(mdata), mdata.NumericRowNames, mdata.NumericRowDescriptions, mdata.NumericRows, new List<string>(),
				new List<string>(), new List<double[][]>(), mdata.CategoryColumnNames, mdata.CategoryColumnDescriptions,
				GetCategoryColumns(mdata), mdata.NumericColumnNames, mdata.NumericColumnDescriptions, mdata.NumericColumns);
		}

		private static List<string[][]> GetCategoryRows(IMatrixData mdata) {
			List<string[][]> result = new List<string[][]>();
			for (int i = 0; i < mdata.CategoryRowCount; i++){
				result.Add(mdata.GetCategoryRowAt(i)); 
			}
			return result;
		}

		private static List<string[][]> GetCategoryColumns(IMatrixData mdata) {
			List<string[][]> result = new List<string[][]>();
			for (int i = 0; i < mdata.CategoryColumnCount; i++) {
				result.Add(mdata.GetCategoryColumnAt(i));
			}
			return result;
		}

		public Parameters GetParameters(IMatrixData mdata, ref string errorString) {
			return
				new Parameters(new Parameter[]{
					new SingleChoiceParam("New column names"){
						Values = mdata.StringColumnNames,
						Help = "Select the column that should become the column names of the transposed matrix."
					}
				});
		}
	}
}