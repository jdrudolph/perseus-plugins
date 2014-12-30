using System;
using System.Collections.Generic;
using System.Drawing;
using BaseLib.Param;
using BaseLibS.Param;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;

namespace PerseusPluginLib.Rearrange{
	public class CombineCategoricalColumns : IMatrixProcessing{
		public bool HasButton { get { return false; } }
		public Bitmap DisplayImage { get { return null; } }
		public string Description { get { return "Combine the terms in two categorical columns to form a new categorical column."; } }
		public string HelpOutput { get { return "A new categorical column is generated with combined terms."; } }
		public string[] HelpSupplTables { get { return new string[0]; } }
		public int NumSupplTables { get { return 0; } }
		public string Name { get { return "Combine categorical columns"; } }
		public string Heading { get { return "Rearrange"; } }
		public bool IsActive { get { return true; } }
		public float DisplayRank { get { return 17.5f; } }
		public string[] HelpDocuments { get { return new string[0]; } }
		public int NumDocuments { get { return 0; } }
		public string Url { get { return "http://141.61.102.17/perseus_doku/doku.php?id=perseus:activities:MatrixProcessing:Rearrange:CombineCategoricalColumns"; } }

		public int GetMaxThreads(Parameters parameters) {
			return 1;
		}

		public Parameters GetParameters(IMatrixData mdata, ref string errorString) {
			return
				new Parameters(new Parameter[]{
					new SingleChoiceParam("First column", 0){Values = mdata.CategoryColumnNames},
					new SingleChoiceParam("Second column", 0){Values = mdata.CategoryColumnNames}
				});
		}

		public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo){
			if (mdata.CategoryColumnCount < 2){
				processInfo.ErrString = "There are less than two categorical columns available.";
				return;
			}
			int colInd1 = param.GetSingleChoiceParam("First column").Value;
			int colInd2 = param.GetSingleChoiceParam("Second column").Value;
			string[][] col1 = mdata.GetCategoryColumnAt(colInd1);
			string[][] col2 = mdata.GetCategoryColumnAt(colInd2);
			string[][] result = new string[col1.Length][];
			for (int i = 0; i < result.Length; i++){
				result[i] = CombineTerms(col1[i], col2[i]);
			}
			string colName = mdata.CategoryColumnNames[colInd1] + "_" + mdata.CategoryColumnNames[colInd2];
			mdata.AddCategoryColumn(colName, "", result);
		}

		private static string[] CombineTerms(ICollection<string> x, ICollection<string> y){
			string[] result = new string[x.Count*y.Count];
			int count = 0;
			foreach (string t in x){
				foreach (string t1 in y){
					result[count++] = t + "_" + t1;
				}
			}
			Array.Sort(result);
			return result;
		}
	}
}