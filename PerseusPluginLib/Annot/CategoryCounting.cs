using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using BaseLib.ParamWf;
using BaseLib.Util;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;

namespace PerseusPluginLib.Annot{
	public class CategoryCounting : IMatrixProcessing{
		public bool HasButton { get { return false; } }
		public Image ButtonImage { get { return null; } }
		public string HelpDescription { get { return ""; } }
		public string HelpOutput { get { return ""; } }
		public string[] HelpSupplTables { get { return new string[0]; } }
		public int NumSupplTables { get { return 0; } }
		public string Name { get { return "Category counting"; } }
		public string Heading { get { return "Annotation columns"; } }
		public bool IsActive { get { return true; } }
		public float DisplayOrder { get { return 3; } }
		public DocumentType HelpDescriptionType { get { return DocumentType.PlainText; } }
		public DocumentType HelpOutputType { get { return DocumentType.PlainText; } }
		public DocumentType[] HelpSupplTablesType { get { return new DocumentType[0]; } }
		public string[] HelpDocuments { get { return new string[0]; } }
		public DocumentType[] HelpDocumentTypes { get { return new DocumentType[0]; } }
		public int NumDocuments { get { return 0; } }

		public int GetMaxThreads(ParametersWf parameters) {
			return 1;
		}

		public void ProcessData(IMatrixData mdata, ParametersWf param, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo){
			int minCount = param.GetIntParam("Min. count").Value;
			int selCol = param.GetSingleChoiceParam("Selection").Value;
			string value = param.GetStringParam("Value").Value;
			int[] catIndices = param.GetMultiChoiceParam("Categories").Value;
			bool[] selection = null;
			if (selCol < mdata.CategoryColumnCount){
				selection = new bool[mdata.RowCount];
				string[][] x = mdata.GetCategoryColumnAt(selCol);
				for (int i = 0; i < selection.Length; i++){
					if (x[i] != null){
						for (int j = 0; j < x[i].Length; j++){
							if (x[i][j].Equals(value)){
								selection[i] = true;
								break;
							}
						}
					}
				}
			}
			CountingResult result = CountCategories(mdata, selection, selCol, catIndices);
			CreateMatrixData(result, mdata, minCount, selection);
		}

		private static void CreateMatrixData(CountingResult result, IMatrixData data, int minCount, IEnumerable selection){
			List<string[]> type = new List<string[]>();
			List<string[]> name = new List<string[]>();
			List<double> count = new List<double>();
			List<double> percOfTotal = new List<double>();
			List<double> selCount = new List<double>();
			List<double> selPerc = new List<double>();
			for (int i = 0; i < result.Count; i++){
				int c = result.GetTotalCountAt(i);
				if (c < minCount){
					continue;
				}
				type.Add(new[]{result.GetType1At(i)});
				name.Add(new[]{result.GetName1At(i)});
				count.Add(c);
				percOfTotal.Add(Math.Round(10000.0*c/data.RowCount)/100.0);
				if (selection != null){
					int c1 = result.GetSelectCountAt(i);
					selCount.Add(c1);
					selPerc.Add(Math.Round(1000.0*c1/c)/10.0);
				}
			}
			float[,] ex = new float[type.Count,0];
			List<string[][]> catCols = new List<string[][]>{type.ToArray(), name.ToArray()};
			List<string> catColNames = new List<string>(new[]{"Type", "Name"});
			List<double[]> numCols = new List<double[]>{count.ToArray(), percOfTotal.ToArray()};
			if (selection != null){
				numCols.Add(selCount.ToArray());
				numCols.Add(selPerc.ToArray());
			}
			List<string> numColNames = new List<string>(new[]{"Count", "Percentage of total"});
			if (selection != null){
				numColNames.AddRange(new[]{"Selection count", "Selection percentage"});
			}
			data.SetData("Count", new List<string>(), ex, new List<string>(), new List<string[]>(), catColNames, catCols,
				numColNames, numCols, new List<string>(), new List<double[][]>());
		}

		public static CountingResult CountCategories(IMatrixData data, bool[] selection, int selCol, int[] catIndices){
			CountingResult result = new CountingResult();
			foreach (int i in catIndices.Where(i => i != selCol)){
				CountTerms(data.CategoryColumnNames[i], data.GetCategoryColumnAt(i), result, selection);
			}
			result.Sort();
			return result;
		}

		private static void CountTerms(string categoryName2, IList<string[]> annot2, CountingResult result,
			IList<bool> selection){
			foreach (string[] t in annot2){
				Array.Sort(t);
			}
			string[] allTerms2 = GetAllTerms(annot2);
			int n2 = allTerms2.Length;
			int[] allTotal = new int[n2];
			int[] selectTotal = new int[n2];
			for (int i2 = 0; i2 < n2; i2++){
				for (int j = 0; j < annot2.Count; j++){
					if (Array.BinarySearch(annot2[j], allTerms2[i2]) >= 0){
						allTotal[i2]++;
						if (selection != null && selection[j]){
							selectTotal[i2]++;
						}
					}
				}
			}
			result.Add(categoryName2, allTerms2, allTotal, selectTotal);
		}

		private static string[] GetAllTerms(IEnumerable<string[]> annot){
			HashSet<string> all = new HashSet<string>();
			foreach (string[] x in annot){
				all.UnionWith(x);
			}
			string[] y = ArrayUtils.ToArray(all);
			Array.Sort(y);
			y = ArrayUtils.Remove(y, "");
			return y;
		}

		public ParametersWf GetParameters(IMatrixData mdata, ref string errorString){
			List<string> choice = mdata.CategoryColumnNames;
			int[] selection = ArrayUtils.ConsecutiveInts(choice.Count);
			string[] sel = ArrayUtils.Concat(mdata.CategoryColumnNames.ToArray(), "<None>");
			return
				new ParametersWf(new ParameterWf[]{
					new MultiChoiceParamWf("Categories"){Values = choice, Value = selection}, new IntParamWf("Min. count", 1),
					new SingleChoiceParamWf("Selection"){Values = sel, Value = sel.Length - 1}, new StringParamWf("Value", "+")
				});
		}
	}
}