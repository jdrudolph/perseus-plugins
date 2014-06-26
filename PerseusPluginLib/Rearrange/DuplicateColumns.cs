using System.Collections.Generic;
using System.Drawing;
using BaseLib.Param;
using BaseLib.Util;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;
using PerseusApi.Utils;

namespace PerseusPluginLib.Rearrange{
	public class DuplicateColumns : IMatrixProcessing{
		public bool HasButton { get { return false; } }
		public Bitmap DisplayImage { get { return null; } }
		public string HelpOutput { get { return "Same matrix but with duplicated columns added."; } }
		public string[] HelpSupplTables { get { return new string[0]; } }
		public int NumSupplTables { get { return 0; } }
		public string Heading { get { return "Rearrange"; } }
		public string Name { get { return "Duplicate columns"; } }
		public bool IsActive { get { return true; } }
		public float DisplayRank { get { return 3; } }
		public string[] HelpDocuments { get { return new string[0]; } }
		public int NumDocuments { get { return 0; } }
		public int GetMaxThreads(Parameters parameters) { return 1; }
		public string Description { get { return "Columns of all types can be duplicated."; } }
		public string Url { get { return "http://141.61.102.17/perseus_doku/doku.php?id=perseus:duplicatecolumns"; } }

		public void ProcessData(IMatrixData data, Parameters param, ref IMatrixData[] supplTables,
		                        ref IDocumentData[] documents, ProcessInfo processInfo){
			int[] exColInds = param.GetMultiChoiceParam("Expression columns").Value;
			int[] numColInds = param.GetMultiChoiceParam("Numerical columns").Value;
			int[] multiNumColInds = param.GetMultiChoiceParam("Multi-numerical columns").Value;
			int[] catColInds = param.GetMultiChoiceParam("Categorical columns").Value;
			int[] textColInds = param.GetMultiChoiceParam("Text columns").Value;
			if (exColInds.Length > 0){
				int ncol = data.ExpressionColumnCount;
				data.ExtractExpressionColumns(ArrayUtils.Concat(ArrayUtils.ConsecutiveInts(data.ExpressionColumnCount), exColInds));
				HashSet<string> taken = new HashSet<string>(data.ExpressionColumnNames);
				for (int i = 0; i < exColInds.Length; i++){
					string s = PerseusUtils.GetNextAvailableName(data.ExpressionColumnNames[ncol + i], taken);
					data.ExpressionColumnNames[ncol + i] = s;
					taken.Add(s);
				}
			}
			foreach (int ind in numColInds){
				HashSet<string> taken = new HashSet<string>(data.NumericColumnNames);
				string s = PerseusUtils.GetNextAvailableName(data.NumericColumnNames[ind], taken);
				data.AddNumericColumn(s, data.NumericColumnDescriptions[ind], (double[]) data.NumericColumns[ind].Clone());
				taken.Add(s);
			}
			foreach (int ind in multiNumColInds){
				HashSet<string> taken = new HashSet<string>(data.MultiNumericColumnNames);
				string s = PerseusUtils.GetNextAvailableName(data.MultiNumericColumnNames[ind], taken);
				data.AddMultiNumericColumn(s, data.MultiNumericColumnDescriptions[ind],
				                           (double[][]) data.MultiNumericColumns[ind].Clone());
				taken.Add(s);
			}
			foreach (int ind in catColInds){
				HashSet<string> taken = new HashSet<string>(data.CategoryColumnNames);
				string s = PerseusUtils.GetNextAvailableName(data.CategoryColumnNames[ind], taken);
				data.AddCategoryColumn(s, data.CategoryColumnDescriptions[ind], data.GetCategoryColumnAt(ind));
				taken.Add(s);
			}
			foreach (int ind in textColInds){
				HashSet<string> taken = new HashSet<string>(data.StringColumnNames);
				string s = PerseusUtils.GetNextAvailableName(data.StringColumnNames[ind], taken);
				data.AddStringColumn(s, data.StringColumnDescriptions[ind], (string[]) data.StringColumns[ind].Clone());
				taken.Add(s);
			}
		}

		public Parameters GetParameters(IMatrixData mdata, ref string errorString){
			List<string> exCols = mdata.ExpressionColumnNames;
			List<string> numCols = mdata.NumericColumnNames;
			List<string> multiNumCols = mdata.MultiNumericColumnNames;
			List<string> catCols = mdata.CategoryColumnNames;
			List<string> textCols = mdata.StringColumnNames;
			return
				new Parameters(new Parameter[]{
					new MultiChoiceParam("Expression columns"){
						Value = new int[0],
						Values = exCols,
						Help = "Specify here the expression columns that should be duplicated."
					},
					new MultiChoiceParam("Numerical columns"){
						Value = new int[0],
						Values = numCols,
						Help = "Specify here the numerical columns that should be duplicated."
					},
					new MultiChoiceParam("Multi-numerical columns"){
						Value = new int[0],
						Values = multiNumCols,
						Help = "Specify here the multi-numerical columns that should be duplicated."
					},
					new MultiChoiceParam("Categorical columns"){
						Value = new int[0],
						Values = catCols,
						Help = "Specify here the categorical columns that should be duplicated."
					},
					new MultiChoiceParam("Text columns"){
						Value = new int[0],
						Values = textCols,
						Help = "Specify here the text columns that should be duplicated."
					}
				});
		}
	}
}