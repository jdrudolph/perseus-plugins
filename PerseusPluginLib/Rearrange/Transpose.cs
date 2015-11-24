using System.Collections.Generic;
using System.Drawing;
using BaseLib.Param;
using BaseLibS.Param;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;
using PerseusApi.Utils;

namespace PerseusPluginLib.Rearrange{
	public class Transpose : IMatrixProcessing{
		public bool HasButton{
			get { return false; }
		}

		public Bitmap DisplayImage{
			get { return null; }
		}

		public string Description{
			get { return "The matrix of expression values is being transposed, i.e. rows become columns and columns become rows."; }
		}

		public string HelpOutput{
			get{
				return
					"The transpose of the matrix of expression values. One string column can be selected to become the new column names.";
			}
		}

		public string[] HelpSupplTables{
			get { return new string[0]; }
		}

		public int NumSupplTables{
			get { return 0; }
		}

		public string Name{
			get { return "Transpose"; }
		}

		public string Heading{
			get { return "Rearrange"; }
		}

		public bool IsActive{
			get { return true; }
		}

		public float DisplayRank{
			get { return 5; }
		}

		public string[] HelpDocuments{
			get { return new string[0]; }
		}

		public int NumDocuments{
			get { return 0; }
		}

		public string Url{
			get { return "http://coxdocs.org/doku.php?id=perseus:user:activities:MatrixProcessing:Rearrange:Transpose"; }
		}

		public int GetMaxThreads(Parameters parameters){
			return 1;
		}

		public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo){
			int nameCol = param.GetParam<int>("New column names").Value;
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
			List<string> rowNames = mdata.ColumnNames;
			mdata.Values = mdata.Values.Transpose();
			if (mdata.IsImputed != null){
				mdata.IsImputed = mdata.IsImputed.Transpose();
			}
			if (mdata.Quality != null){
				mdata.Quality = mdata.Quality.Transpose();
			}
			List<string> stringColumnNames = mdata.StringColumnNames;
			List<string> categoryColumnNames = mdata.CategoryColumnNames;
			List<string> numericColumnNames = mdata.NumericColumnNames;
			List<string> multiNumericColumnNames = mdata.MultiNumericColumnNames;
			List<string> stringColumnDescriptions = mdata.StringColumnDescriptions;
			List<string> categoryColumnDescriptions = mdata.CategoryColumnDescriptions;
			List<string> numericColumnDescriptions = mdata.NumericColumnDescriptions;
			List<string> multiNumericColumnDescriptions = mdata.MultiNumericColumnDescriptions;
			List<string[]> stringColumns = mdata.StringColumns;
			List<string[][]> categoryColumns = GetCategoryColumns(mdata);
			List<double[]> numericColumns = mdata.NumericColumns;
			List<double[][]> multiNumericColumns = mdata.MultiNumericColumns;
			mdata.SetAnnotationColumns(new List<string>(new[]{"Name"}), new List<string>(new[]{"Name"}),
				new List<string[]>(new[]{rowNames.ToArray()}), mdata.CategoryRowNames, mdata.CategoryRowDescriptions,
				GetCategoryRows(mdata), mdata.NumericRowNames, mdata.NumericRowDescriptions, mdata.NumericRows, new List<string>(),
				new List<string>(), new List<double[][]>());
			mdata.ColumnNames = colNames;
			mdata.SetAnnotationRows(stringColumnNames, stringColumnDescriptions, stringColumns, categoryColumnNames,
				categoryColumnDescriptions, categoryColumns, numericColumnNames, numericColumnDescriptions, numericColumns,
				multiNumericColumnNames, multiNumericColumnDescriptions, multiNumericColumns);
		}

		private static List<string[][]> GetCategoryRows(IDataWithAnnotationRows mdata){
			List<string[][]> result = new List<string[][]>();
			for (int i = 0; i < mdata.CategoryRowCount; i++){
				result.Add(mdata.GetCategoryRowAt(i));
			}
			return result;
		}

		private static List<string[][]> GetCategoryColumns(IDataWithAnnotationColumns mdata){
			List<string[][]> result = new List<string[][]>();
			for (int i = 0; i < mdata.CategoryColumnCount; i++){
				result.Add(mdata.GetCategoryColumnAt(i));
			}
			return result;
		}

		public Parameters GetParameters(IMatrixData mdata, ref string errorString){
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