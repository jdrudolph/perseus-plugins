using System.Collections.Generic;
using System.Drawing;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BaseLib.Param;
using BaseLib.Util;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;

namespace PluginMy{
	public class FilterDe : IMatrixProcessing {
		public bool HasButton { get { return false; } }
		public BitmapSource DisplayImage { get { return null; } }
		public string HelpDescription { get { return "Rows containing D or E in the specified columns are discarded."; } }
		public string HelpOutput { get { return "Filtered matrix."; } }
		public DocumentType HelpDescriptionType { get { return DocumentType.PlainText; } }
		public DocumentType HelpOutputType { get { return DocumentType.PlainText; } }
		public DocumentType[] HelpSupplTablesType { get { return new DocumentType[0]; } }
		public string[] HelpSupplTables { get { return new string[0]; } }
		public int NumSupplTables { get { return 0; } }
		public string Name { get { return "Filter DE"; } }
		public string Heading { get { return "My plugins"; } }
		public bool IsActive { get { return true; } }
		public float DisplayOrder { get { return 101; } }
		public string[] HelpDocuments { get { return new string[0]; } }
		public DocumentType[] HelpDocumentTypes { get { return new DocumentType[0]; } }
		public int NumDocuments { get { return 0; } }

		public int GetMaxThreads(Parameters parameters) {
			return 1;
		}

		public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo){
			int colIndex = param.GetSingleChoiceParam("Column with second last AA").Value;
			string aas = param.GetStringParam("Amino acids").Value;
			string[][] col = mdata.GetCategoryColumnAt(colIndex);
			List<int> validRows = new List<int>();
			for (int i = 0; i < mdata.RowCount; i++){
				string[] x = col[i];
				for (int j = 0; j < x.Length; j++){
					if (x[j].Length != 1){
						processInfo.ErrString = "Some of the entries in column " + mdata.CategoryColumnNames[colIndex] +
							" do not contain amino acids";
						return;
					}
				}
				bool keep = JudgeIfKept(aas, x);
				if (keep){
					validRows.Add(i);
				}
			}
			mdata.ExtractExpressionRows(validRows.ToArray());
		}

		private bool JudgeIfKept(string aas, string[] cat){
			for (int i = 0; i < aas.Length; i++){
				for (int j = 0; j < cat.Length; j++){
					char x= aas[i];
					char y = cat[j][0];
					if (x == y){
						return false;
					}

				}
			}
			return true;
		}

		public Parameters GetParameters(IMatrixData mdata, ref string errorString) {
			return new Parameters(new Parameter[]{
				new SingleChoiceParam("Column with second last AA"){Values = mdata.CategoryColumnNames},
 				new StringParam("Amino acids", "DE") 
			});
		}
	}
}