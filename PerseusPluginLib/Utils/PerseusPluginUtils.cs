using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BaseLib.Param;
using BaseLib.Util;
using PerseusApi.Matrix;

namespace PerseusPluginLib.Utils{
	public static class PerseusPluginUtils{
		public static SingleChoiceParam GetFilterModeParam(bool column) {
			return new SingleChoiceParam("Filter mode")
			{Values = new[]{"Reduce matrix", column ? "Add categorical column" : "Add categorical row"}};
		}

		private static SingleChoiceParam GetModeParam1() {
			return new SingleChoiceParam("Mode") {
				Values = new[]{"Remove matching rows", "Keep matching rows"},
				Help =
					"If 'Remove matching rows' is selected, rows having the value specified above will be removed while " +
						"all other rows will be kept. If 'Keep matching rows' is selected, the opposite will happen."
			};
		}

		private static SingleChoiceParam GetModeParam2() {
			return new SingleChoiceParam("Mode") {
				Values = new[]{"Mark matching rows", "Mark non-matching rows"},
				Help =
					"If 'Mark matching rows' is selected, rows having the value specified above will be indicated with a '+' in the output column. " +
						"If 'Keep matching rows' is selected, the opposite will happen."
			};
		}

		internal static SingleChoiceWithSubParams GetFilterModeParamNew() {
			SingleChoiceWithSubParams p = new SingleChoiceWithSubParams("Filter mode") {
				Values = new[]{"Reduce matrix", "Add categorical column", "Split matrix"},
				SubParams =
					new List<Parameters>(new[] { new Parameters(GetModeParam1()), new Parameters(GetModeParam2()), new Parameters() })
			};
			return p;
		}

		public static void FilterRows(IMatrixData mdata, Parameters parameters, int[] rows) {
			bool reduceMatrix = GetReduceMatrix(parameters);
			if (reduceMatrix){
				mdata.ExtractExpressionRows(rows);
			} else{
				Array.Sort(rows);
				string[][] col = new string[mdata.RowCount][];
				for (int i = 0; i < col.Length; i++){
					bool contains = Array.BinarySearch(rows, i) >= 0;
					col[i] = contains ? new[]{"Keep"} : new[]{"Discard"};
				}
				mdata.AddCategoryColumn("Filter", "", col);
			}
		}

		private static bool GetReduceMatrix(Parameters parameters) {
			return parameters.GetSingleChoiceParam("Filter mode").Value == 0;
		}

		public static void FilterColumns(IMatrixData mdata, Parameters parameters, int[] cols) {
			bool reduceMatrix = GetReduceMatrix(parameters);
			if (reduceMatrix){
				mdata.ExtractExpressionColumns(cols);
			} else{
				Array.Sort(cols);
				string[][] row = new string[mdata.ExpressionColumnCount][];
				for (int i = 0; i < row.Length; i++){
					bool contains = Array.BinarySearch(cols, i) >= 0;
					row[i] = contains ? new[]{"Keep"} : new[]{"Discard"};
				}
				mdata.AddCategoryRow("Filter", "", row);
			}
		}

		public static string[][] CollapseCatCol(string[][] catCol, int[][] collapse){
			string[][] result = new string[collapse.Length][];
			for (int i = 0; i < collapse.Length; i++){
				result[i] = CollapseCatCol(catCol, collapse[i]);
			}
			return result;
		}

		private static string[] CollapseCatCol(IList<string[]> catCol, IEnumerable<int> collapse){
			HashSet<string> all = new HashSet<string>();
			foreach (int x in collapse){
				all.UnionWith(catCol[x]);
			}
			string[] y = ArrayUtils.ToArray(all);
			Array.Sort(y);
			return y;
		}

		public static float[] CollapseNumCol(float[] numCol, int[][] collapse){
			float[] result = new float[collapse.Length];
			for (int i = 0; i < collapse.Length; i++){
				result[i] = CollapseNumCol(numCol, collapse[i]);
			}
			return result;
		}

		private static float CollapseNumCol(IList<float> numCol, IEnumerable<int> collapse){
			List<float> all = new List<float>();
			foreach (int x in collapse){
				if (!float.IsNaN(numCol[x]) && !float.IsInfinity(numCol[x])){
					all.Add(numCol[x]);
				}
			}
			float y = ArrayUtils.Median(all.ToArray());
			return y;
		}

		public static double[] CollapseNumCol(double[] numCol, int[][] collapse){
			double[] result = new double[collapse.Length];
			for (int i = 0; i < collapse.Length; i++){
				result[i] = CollapseNumCol(numCol, collapse[i]);
			}
			return result;
		}

		private static double CollapseNumCol(IList<double> numCol, IEnumerable<int> collapse){
			List<double> all = new List<double>();
			foreach (int x in collapse){
				if (!double.IsNaN(numCol[x]) && !double.IsInfinity(numCol[x])){
					all.Add(numCol[x]);
				}
			}
			double y = ArrayUtils.Median(all.ToArray());
			return y;
		}

		public static int[][] GetExpressionColIndices(IList<string[]> groupCol, string[] groupNames){
			int[][] colInds = new int[groupNames.Length][];
			for (int i = 0; i < colInds.Length; i++){
				colInds[i] = GetExpressionColIndices(groupCol, groupNames[i]);
			}
			return colInds;
		}

		private static int[] GetExpressionColIndices(IList<string[]> groupCol, string groupName){
			List<int> result = new List<int>();
			for (int i = 0; i < groupCol.Count; i++){
				string[] w = groupCol[i];
				Array.Sort(w);
				if (Array.BinarySearch(w, groupName) >= 0){
					result.Add(i);
				}
			}
			return result.ToArray();
		}

		public static int[] GetIndicesOfCol(IMatrixData data, string categoryName, string value){
			int index = GetIndexOfCol(data, categoryName);
			List<int> result = new List<int>();
			for (int i = 0; i < data.ExpressionColumnCount; i++){
				string[] s = data.GetCategoryRowAt(index)[i];
				foreach (string s1 in s){
					if (s1.Equals(value)){
						result.Add(i);
						break;
					}
				}
			}
			return result.ToArray();
		}

		public static int[] GetIndicesOfCol(IMatrixData data, string categoryName, HashSet<string> values){
			int index = GetIndexOfCol(data, categoryName);
			List<int> result = new List<int>();
			for (int i = 0; i < data.ExpressionColumnCount; i++){
				string[] s = data.GetCategoryRowAt(index)[i];
				foreach (string s1 in s){
					if (values.Contains(s1)){
						result.Add(i);
						break;
					}
				}
			}
			return result.ToArray();
		}

		public static int[] GetIndicesOf(IMatrixData data, string categoryName, string value){
			int index = GetIndexOf(data, categoryName);
			List<int> result = new List<int>();
			for (int i = 0; i < data.RowCount; i++){
				string[] s = data.GetCategoryColumnAt(index)[i];
				foreach (string s1 in s){
					if (s1.Equals(value)){
						result.Add(i);
						break;
					}
				}
			}
			return result.ToArray();
		}

		public static int[] GetIndicesOf(IMatrixData data, string categoryName, HashSet<string> values){
			int index = GetIndexOf(data, categoryName);
			List<int> result = new List<int>();
			for (int i = 0; i < data.RowCount; i++){
				string[] s = data.GetCategoryColumnAt(index)[i];
				foreach (string s1 in s){
					if (values.Contains(s1)){
						result.Add(i);
						break;
					}
				}
			}
			return result.ToArray();
		}

		private static int GetIndexOf(IMatrixData data, string categoryName){
			for (int i = 0; i < data.CategoryColumnNames.Count; i++){
				if (data.CategoryColumnNames[i].Equals(categoryName)){
					return i;
				}
			}
			return -1;
		}

		private static int GetIndexOfCol(IMatrixData data, string categoryName){
			for (int i = 0; i < data.CategoryRowNames.Count; i++){
				if (data.CategoryRowNames[i].Equals(categoryName)){
					return i;
				}
			}
			return -1;
		}

		public static List<string[][]> GetCategoryColumns(IMatrixData mdata, IList<int> inds){
			List<string[][]> result = new List<string[][]>();
			foreach (int ind in inds){
				result.Add(mdata.GetCategoryColumnAt(ind));
			}
			return result;
		}

		public static List<string[][]> GetCategoryColumns(IMatrixData mdata){
			List<string[][]> result = new List<string[][]>();
			for (int index = 0; index < mdata.CategoryColumnCount; index++){
				result.Add(mdata.GetCategoryColumnAt(index));
			}
			return result;
		}

		public static List<string[][]> GetCategoryRows(IMatrixData mdata, IList<int> inds){
			List<string[][]> result = new List<string[][]>();
			foreach (int ind in inds){
				result.Add(mdata.GetCategoryRowAt(ind));
			}
			return result;
		}

		public static List<string[][]> GetCategoryRows(IMatrixData mdata){
			List<string[][]> result = new List<string[][]>();
			for (int index = 0; index < mdata.CategoryRowCount; index++){
				result.Add(mdata.GetCategoryRowAt(index));
			}
			return result;
		}

		public static string[][] CalcPvalueSignificance(double[] pvals, double threshold){
			string[][] result = new string[pvals.Length][];
			for (int i = 0; i < result.Length; i++){
				result[i] = pvals[i] <= threshold ? new[]{"+"} : new string[0];
			}
			return result;
		}

		public static string[][] CalcBenjaminiHochbergFdr(double[] pvals, double threshold){
			int n = pvals.Length;
			int[] o = ArrayUtils.Order(pvals);
			int lastind = -1;
			for (int i = 0; i < n; i++){
				double fdr = Math.Min(1, pvals[o[i]]*n/(1.0 + i));
				if (fdr <= threshold){
					lastind = i;
				}
			}
			string[][] result = new string[pvals.Length][];
			for (int i = 0; i < result.Length; i++){
				result[i] = new string[0];
			}
			for (int i = 0; i <= lastind; i++){
				result[o[i]] = new[]{"+"};
			}
			return result;
		}
	}
}