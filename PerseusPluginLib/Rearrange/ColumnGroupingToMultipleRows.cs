using System;
using System.Collections.Generic;
using System.Windows.Media;
using BaseLib.Param;
using BaseLib.Util;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;
using PerseusPluginLib.Utils;

namespace PerseusPluginLib.Rearrange{
	public class ColumnGroupingToMultipleRows : IMatrixProcessing{
		public bool HasButton { get { return false; } }
		public ImageSource ButtonImage { get { return null; } }
		public string HelpDescription { get { return ""; } }
		public string HelpOutput { get { return ""; } }
		public string[] HelpSupplTables { get { return new string[0]; } }
		public int NumSupplTables { get { return 0; } }
		public string Name { get { return "Column grouping to multiple rows"; } }
		public string Heading { get { return "Matrix rearrangements"; } }
		public bool IsActive { get { return true; } }
		public float DisplayOrder { get { return 20; } }
		public DocumentType HelpDescriptionType { get { return DocumentType.PlainText; } }
		public DocumentType HelpOutputType { get { return DocumentType.PlainText; } }
		public DocumentType[] HelpSupplTablesType { get { return new DocumentType[0]; } }
		public string[] HelpDocuments { get { return new string[0]; } }
		public DocumentType[] HelpDocumentTypes { get { return new DocumentType[0]; } }
		public int NumDocuments { get { return 0; } }

		public int GetMaxThreads(Parameters parameters) {
			return 1;
		}

		public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo){
			int newColGroupInd = param.GetSingleChoiceParam("New expression columns").Value;
			int seriesInd = param.GetSingleChoiceParam("Series").Value;
			if (newColGroupInd < 0 || seriesInd < 0){
				processInfo.ErrString = "Invalid grouping";
				return;
			}
			if (newColGroupInd == seriesInd){
				processInfo.ErrString = "Selected rows cannot be equal.";
				return;
			}
			int[] remainingCatRows = ArrayUtils.Complement(new[]{newColGroupInd, seriesInd}, mdata.CategoryRowCount);
			string[][] newColGroup = mdata.GetCategoryRowAt(newColGroupInd);
			int ncols = newColGroup.Length;
			string[] allNewCols = ArrayUtils.UniqueValues(ArrayUtils.Concat(newColGroup));
			int nNewCols = allNewCols.Length;
			string[][] series = mdata.GetCategoryRowAt(seriesInd);
			string[] allSeries = ArrayUtils.UniqueValues(ArrayUtils.Concat(series));
			int nseries = allSeries.Length;
			List<int>[,] colIndices = new List<int>[nseries,nNewCols];
			List<int>[] colIndices2 = new List<int>[nNewCols];
			for (int j = 0; j < nNewCols; j++){
				colIndices2[j] = new List<int>();
				for (int i = 0; i < nseries; i++){
					colIndices[i, j] = new List<int>();
				}
			}
			for (int i = 0; i < ncols; i++){
				string[] newCols1 = newColGroup[i];
				string[] series1 = series[i];
				foreach (string t1 in newCols1){
					int newCol2Ind = Array.BinarySearch(allNewCols, t1);
					foreach (string t in series1){
						int series2Ind = Array.BinarySearch(allSeries, t);
						colIndices[series2Ind, newCol2Ind].Add(i);
						colIndices2[newCol2Ind].Add(i);
					}
				}
			}
			float[,] newExpVals = new float[mdata.RowCount*nseries,nNewCols];
			float[,] newQuality = new float[mdata.RowCount*nseries,nNewCols];
			bool[,] newIsImputed = new bool[mdata.RowCount*nseries,nNewCols];
			int row = 0;
			for (int i = 0; i < mdata.RowCount; i++){
				float[] expRow = mdata.GetExpressionRow(i);
				float[] qualityRow = mdata.GetQualityRow(i);
				bool[] isImputedRow = mdata.GetIsImputednRow(i);
				for (int j = 0; j < nseries; j++){
					for (int k = 0; k < nNewCols; k++){
						List<int> inds = colIndices[j, k];
						Summarize(ArrayUtils.SubArray(expRow, inds), ArrayUtils.SubArray(qualityRow, inds),
							ArrayUtils.SubArray(isImputedRow, inds), out newExpVals[row, k], out newQuality[row, k], out newIsImputed[row, k]);
					}
					row++;
				}
			}
			List<string> newExpNames = new List<string>();
			newExpNames.AddRange(allNewCols);
			List<string> newExpDesc = new List<string>();
			newExpDesc.AddRange(allNewCols);
			mdata.SetData(mdata.Name, mdata.Description, newExpNames, newExpDesc, newExpVals, newIsImputed, newQuality,
				mdata.QualityName, mdata.QualityBiggerIsBetter, mdata.StringColumnNames, mdata.StringColumnDescriptions,
				Expand(mdata.StringColumns, nseries), mdata.CategoryColumnNames, mdata.CategoryColumnDescriptions,
				ExpandCat(mdata, nseries), mdata.NumericColumnNames, mdata.NumericColumnDescriptions,
				Expand(mdata.NumericColumns, nseries), mdata.MultiNumericColumnNames, mdata.MultiNumericColumnDescriptions,
				Expand(mdata.MultiNumericColumns, nseries), ArrayUtils.SubList(mdata.CategoryRowNames, remainingCatRows),
				ArrayUtils.SubList(mdata.CategoryRowDescriptions, remainingCatRows),
				TransformCatCols(PerseusPluginUtils.GetCategoryRows(mdata, remainingCatRows), colIndices2), mdata.NumericRowNames,
				mdata.NumericRowDescriptions, TransformNumCols(mdata.NumericRows, colIndices2));
		}

		private static List<string[][]> ExpandCat(IMatrixData mdata, int nseries){
			List<string[][]> result = new List<string[][]>();
			for (int index = 0; index < mdata.CategoryColumnCount; index++){
				string[][] column = mdata.GetCategoryColumnAt(index);
				result.Add(Expand(column, nseries));
			}
			return result;
		}

		private static List<double[]> TransformNumCols(IEnumerable<double[]> numericRows, IList<List<int>> colIndices2){
			List<double[]> result = new List<double[]>();
			foreach (double[] row in numericRows){
				result.Add(TransformNumCol(row, colIndices2));
			}
			return result;
		}

		private static List<string[][]> TransformCatCols(IEnumerable<string[][]> subList, IList<List<int>> colIndices2){
			List<string[][]> result = new List<string[][]>();
			foreach (string[][] row in subList){
				result.Add(TransformCatCol(row, colIndices2));
			}
			return result;
		}

		private static double[] TransformNumCol(IList<double> row, IList<List<int>> colIndices2){
			double[] result = new double[colIndices2.Count];
			for (int i = 0; i < result.Length; i++){
				result[i] = ArrayUtils.Median(ArrayUtils.SubArray(row, colIndices2[i]));
			}
			return result;
		}

		private static string[][] TransformCatCol(IList<string[]> row, IList<List<int>> colIndices2){
			string[][] result = new string[colIndices2.Count][];
			for (int i = 0; i < result.Length; i++){
				result[i] = ArrayUtils.UniqueValues(ArrayUtils.Concat(ArrayUtils.SubArray(row, colIndices2[i])));
			}
			return result;
		}

		private static List<T[]> Expand<T>(IEnumerable<T[]> columns, int nseries){
			List<T[]> result = new List<T[]>();
			foreach (T[] column in columns){
				result.Add(Expand(column, nseries));
			}
			return result;
		}

		private static T[] Expand<T>(T[] columns, int nseries){
			T[] result = new T[columns.Length*nseries];
			int ind = 0;
			foreach (T t in columns){
				for (int j = 0; j < nseries; j++){
					result[ind] = t;
					ind++;
				}
			}
			return result;
		}

		private static void Summarize(IList<float> expr, IList<float> qual, IList<bool> isImp, out float exprOut,
			out float qualOut, out bool isImpOut){
			if (expr.Count == 0){
				exprOut = float.NaN;
				qualOut = float.NaN;
				isImpOut = false;
				return;
			}
			List<int> imp = new List<int>();
			List<int> noimp = new List<int>();
			for (int i = 0; i < isImp.Count; i++){
				if (isImp[i]){
					imp.Add(i);
				} else{
					noimp.Add(i);
				}
			}
			if (noimp.Count > 0){
				isImpOut = false;
				exprOut = ArrayUtils.Median(ArrayUtils.SubArray(expr, noimp));
				qualOut = ArrayUtils.Median(ArrayUtils.SubArray(qual, noimp));
			} else{
				isImpOut = true;
				exprOut = ArrayUtils.Median(expr);
				qualOut = ArrayUtils.Median(qual);
			}
		}

		public Parameters GetParameters(IMatrixData mdata, ref string errorString) {
			return
				new Parameters(new Parameter[]{
					new SingleChoiceParam("New expression columns")
					{Values = mdata.CategoryRowNames, Help = "This could for instance be the time points of multiple time series"},
					new SingleChoiceParam("Series")
					{Values = mdata.CategoryRowNames, Help = "This could for instance distinguish multiple time series"}
				});
		}
	}
}