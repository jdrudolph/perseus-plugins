using System;
using System.Collections.Generic;
using System.Drawing;
using BaseLib.Param;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;

namespace PerseusPluginLib.Quality{
	public class CreateQualityMatrix : IMatrixProcessing{
		public bool HasButton { get { return false; } }
		public Bitmap DisplayImage { get { return null; } }

		public string Description{
			get{
				return "Create a matrix of quality values from a set of numerical columns. There has to " +
					"be one numerical column per expression column.";
			}
		}

		public string HelpOutput { get { return ""; } }
		public string[] HelpSupplTables { get { return new string[0]; } }
		public int NumSupplTables { get { return 0; } }
		public string Name { get { return "Create quality matrix"; } }
		public string Heading { get { return "Quality"; } }
		public bool IsActive { get { return true; } }
		public float DisplayRank { get { return 0; } }
		public string[] HelpDocuments { get { return new string[0]; } }
		public int NumDocuments { get { return 0; } }
		public int GetMaxThreads(Parameters parameters) { return 1; }
		public string Url { get { return "http://141.61.102.17/perseus_doku/doku.php?id=perseus:activities:MatrixProcessing:Quality:CreateQualityMatrix"; } }

		public Parameters GetParameters(IMatrixData mdata, ref string errorString){
			string[] reducedExpColNames = ReduceNames(mdata.ColumnNames);
			List<Parameter> p = new List<Parameter>();
			for (int i = 0; i < mdata.ColumnCount; i++){
				SingleChoiceParam scp = new SingleChoiceParam(mdata.ColumnNames[i]){
					Values = mdata.NumericColumnNames,
					Value = GetSelectedValue(mdata.NumericColumnNames, reducedExpColNames[i])
				};
				p.Add(scp);
			}
			return new Parameters(p);
		}

		public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo){
			float[,] q = new float[mdata.RowCount,mdata.ColumnCount];
			for (int j = 0; j < mdata.ColumnCount; j++){
				int ind = param.GetSingleChoiceParam(mdata.ColumnNames[j]).Value;
				double[] w = mdata.NumericColumns[ind];
				for (int i = 0; i < mdata.RowCount; i++){
					q[i, j] = (float) w[i];
				}
			}
			mdata.QualityValues = q;
			mdata.QualityBiggerIsBetter = false;
		}

		private static int GetSelectedValue(IList<string> numericColumnNames, string find){
			for (int i = 0; i < numericColumnNames.Count; i++){
				if (numericColumnNames[i].Contains(find)){
					return i;
				}
			}
			return 0;
		}

		private static string[] ReduceNames(IList<string> expressionColumnNames){
			string prefix = GetCommonPrefix(expressionColumnNames);
			string suffix = GetCommonSuffix(expressionColumnNames);
			string[] result = new string[expressionColumnNames.Count];
			for (int i = 0; i < result.Length; i++){
				result[i] = expressionColumnNames[i].Substring(prefix.Length);
				result[i] = result[i].Substring(0, result[i].Length - suffix.Length);
			}
			return result;
		}

		private static string GetCommonSuffix(IList<string> x){
			if (x.Count == 0){
				return "";
			}
			if (x.Count == 1){
				return x[0];
			}
			string suffix = x[0];
			for (int i = 1; i < x.Count; i++){
				suffix = GetCommonSuffix(suffix, x[i]);
			}
			return suffix;
		}

		private static string GetCommonSuffix(string x1, string x2){
			if (x1.Length > x2.Length){
				string s = x2;
				x2 = x1;
				x1 = s;
			}
			if (x2.EndsWith(x1)){
				return x1;
			}
			for (int i = 0; i < x1.Length; i++){
				if (x1[x1.Length - 1 - i] != x2[x2.Length - 1 - i]){
					return x1.Substring(x1.Length - i, i);
				}
			}
			throw new Exception("Never get here.");
		}

		private static string GetCommonPrefix(IList<string> x){
			if (x.Count == 0){
				return "";
			}
			if (x.Count == 1){
				return x[0];
			}
			string prefix = x[0];
			for (int i = 1; i < x.Count; i++){
				prefix = GetCommonPrefix(prefix, x[i]);
			}
			return prefix;
		}

		private static string GetCommonPrefix(string x1, string x2){
			if (x1.Length > x2.Length){
				string s = x2;
				x2 = x1;
				x1 = s;
			}
			if (x2.StartsWith(x1)){
				return x1;
			}
			for (int i = 0; i < x1.Length; i++){
				if (x1[i] != x2[i]){
					return x1.Substring(0, i);
				}
			}
			throw new Exception("Never get here.");
		}
	}
}