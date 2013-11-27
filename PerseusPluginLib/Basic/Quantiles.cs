using System.Collections.Generic;
using System.Drawing;
using BaseLib.ParamWf;
using BaseLib.Util;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;
using PerseusPluginLib.Properties;

namespace PerseusPluginLib.Basic{
	public class Quantiles : IMatrixProcessing{
		public bool HasButton { get { return true; } }
		public Image ButtonImage { get { return Resources.quantiles; } }
		public string Name { get { return "Quantiles"; } }
		public string Heading { get { return "Basic"; } }
		public bool IsActive { get { return true; } }
		public float DisplayOrder { get { return -4; } }
		public string HelpOutput { get { return "For each selected expression coulumn a categorical column is added containing the quantile information."; } }
		public string[] HelpSupplTables { get { return new string[0]; } }
		public int NumSupplTables { get { return 0; } }
		public DocumentType HelpDescriptionType { get { return DocumentType.PlainText; } }
		public DocumentType HelpOutputType { get { return DocumentType.PlainText; } }
		public DocumentType[] HelpSupplTablesType { get { return new DocumentType[0]; } }
		public string[] HelpDocuments { get { return new string[0]; } }
		public DocumentType[] HelpDocumentTypes { get { return new DocumentType[0]; } }
		public int NumDocuments { get { return 0; } }
		public string HelpDescription{
			get{
				return
					"Expression columns are transformed into quantiles. These can than for instance used in subsequent enrichment analysis.";
			}
		}

		public int GetMaxThreads(ParametersWf parameters) {
			return 1;
		}

		public void ProcessData(IMatrixData mdata, ParametersWf param, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo){
			int numQuantiles = param.GetIntParam("Number of quantiles").Value;
			int[] colInds = param.GetMultiChoiceParam("Columns").Value;
			foreach (int colInd in colInds){
				float[] vals = mdata.GetExpressionColumn(colInd);
				List<int> v = new List<int>();
				for (int i = 0; i < vals.Length; i++){
					if (!float.IsNaN(vals[i])){
						v.Add(i);
					}
				}
				int[] o = v.ToArray();
				vals = ArrayUtils.SubArray(vals, o);
				int[] q = ArrayUtils.Order(vals);
				o = ArrayUtils.SubArray(o, q);
				string[][] catCol = new string[mdata.RowCount][];
				for (int i = 0; i < catCol.Length; i++){
					catCol[i] = new[]{"missing"};
				}
				for (int i = 0; i < o.Length; i++){
					int catVal = (i*numQuantiles)/o.Length + 1;
					catCol[o[i]] = new[]{"Q" + catVal};
				}
				string name = mdata.ExpressionColumnNames[colInd] + "_q";
				string desc = "The column " + mdata.ExpressionColumnNames[colInd] + " has been divided into " + numQuantiles +
					" quantiles.";
				mdata.AddCategoryColumn(name, desc, catCol);
			}
		}

		public ParametersWf GetParameters(IMatrixData mdata, ref string errorString) {
			List<string> values = mdata.ExpressionColumnNames;
			return
				new ParametersWf(new ParameterWf[]{
					new IntParamWf("Number of quantiles", 5)
					{Help = "This defines the number of quantiles that each column is going to be divided into."},
					new MultiChoiceParamWf("Columns"){
						Value = ArrayUtils.ConsecutiveInts(values.Count), Values = values,
						Help = "Please select here the columns that should be transformed into quantiles."
					}
				});
		}
	}
}