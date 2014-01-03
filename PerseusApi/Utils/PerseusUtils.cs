using System;
using System.Collections.Generic;
using System.IO;
using BaseLib.Parse;
using BaseLib.Util;
using PerseusApi.Generic;
using PerseusApi.Matrix;

namespace PerseusApi.Utils{
	public static class PerseusUtils{
		public static readonly HashSet<string> commentPrefix = new HashSet<string>(new[]{"#", "!"});
		public static readonly HashSet<string> commentPrefixExceptions = new HashSet<string>(new[]{"#N/A", "#n/a"});

		public static void LoadData(IDictionary<string, string[]> annotationRows, int[] eInds, int[] cInds, int[] nInds,
			int[] tInds, int[] mInds, ProcessInfo processInfo, IList<string> colNames, IMatrixData mdata, TextReader reader,
			int nrows, string origin, char separator){
			string[] colDescriptions = null;
			string[] colTypes = null;
			bool[] colVisible = null;
			if (annotationRows.ContainsKey("Description")){
				colDescriptions = annotationRows["Description"];
				annotationRows.Remove("Description");
			}
			if (annotationRows.ContainsKey("Type")){
				colTypes = annotationRows["Type"];
				annotationRows.Remove("Type");
			}
			if (annotationRows.ContainsKey("Visible")){
				string[] colVis = annotationRows["Visible"];
				colVisible = new bool[colVis.Length];
				for (int i = 0; i < colVisible.Length; i++){
					colVisible[i] = bool.Parse(colVis[i]);
				}
				annotationRows.Remove("Visible");
			}
			int[] allInds = ArrayUtils.Concat(new[]{eInds, cInds, nInds, tInds, mInds});
			Array.Sort(allInds);
			for (int i = 0; i < allInds.Length - 1; i++){
				if (allInds[i + 1] == allInds[i]){
					processInfo.ErrString = "Column '" + colNames[allInds[i]] + "' has been selected multiple times";
					return;
				}
			}
			string[] allColNames = ArrayUtils.SubArray(colNames, allInds);
			Array.Sort(allColNames);
			for (int i = 0; i < allColNames.Length - 1; i++){
				if (allColNames[i + 1].Equals(allColNames[i])){
					processInfo.ErrString = "Column name '" + allColNames[i] + "' occurs multiple times.";
					return;
				}
			}
			LoadData(colNames, colDescriptions, eInds, cInds, nInds, tInds, mInds, origin, mdata, annotationRows,
				processInfo.Progress, processInfo.Status, separator, reader, nrows);
		}

		private static void LoadData(IList<string> colNames, IList<string> colDescriptions, IList<int> expressionColIndices,
			IList<int> catColIndices, IList<int> numColIndices, IList<int> textColIndices, IList<int> multiNumColIndices,
			string origin, IMatrixData matrixData, IDictionary<string, string[]> annotationRows, Action<int> progress,
			Action<string> status, char separator, TextReader reader, int nrows){
			Dictionary<string, string[]> catAnnotatRows;
			Dictionary<string, string[]> numAnnotatRows;
			status("Reading data");
			SplitAnnotRows(annotationRows, out catAnnotatRows, out numAnnotatRows);
			float[,] expressionValues = new float[nrows,expressionColIndices.Count];
			List<string[][]> categoryAnnotation = new List<string[][]>();
			foreach (int t in catColIndices){
				categoryAnnotation.Add(new string[nrows][]);
			}
			List<double[]> numericAnnotation = new List<double[]>();
			foreach (int t in numColIndices){
				numericAnnotation.Add(new double[nrows]);
			}
			List<double[][]> multiNumericAnnotation = new List<double[][]>();
			foreach (int t in multiNumColIndices){
				multiNumericAnnotation.Add(new double[nrows][]);
			}
			List<string[]> stringAnnotation = new List<string[]>();
			foreach (int t in textColIndices){
				stringAnnotation.Add(new string[nrows]);
			}
			reader.ReadLine();
			int count = 0;
			string line;
			while ((line = reader.ReadLine()) != null){
				progress((100*(count + 1))/nrows);
				if (TabSep.IsCommentLine(line, commentPrefix, commentPrefixExceptions)){
					continue;
				}
				string[] w = SplitLine(line, separator);
				for (int i = 0; i < expressionColIndices.Count; i++){
					if (expressionColIndices[i] >= w.Length){
						expressionValues[count, i] = float.NaN;
					} else{
						string s = StringUtils.RemoveWhitespace(w[expressionColIndices[i]]);
						bool success = float.TryParse(s, out expressionValues[count, i]);
						if (!success){
							expressionValues[count, i] = float.NaN;
						}
					}
				}
				for (int i = 0; i < multiNumColIndices.Count; i++){
					if (multiNumColIndices[i] >= w.Length){
						multiNumericAnnotation[i][count] = new double[0];
					} else{
						string q = w[multiNumColIndices[i]].Trim();
						if (q.Length >= 2 && q[0] == '\"' && q[q.Length - 1] == '\"'){
							q = q.Substring(1, q.Length - 2);
						}
						if (q.Length >= 2 && q[0] == '\'' && q[q.Length - 1] == '\''){
							q = q.Substring(1, q.Length - 2);
						}
						string[] ww = q.Length == 0 ? new string[0] : q.Split(';');
						multiNumericAnnotation[i][count] = new double[ww.Length];
						for (int j = 0; j < ww.Length; j++){
							double q1;
							bool success = double.TryParse(ww[j], out q1);
							multiNumericAnnotation[i][count][j] = success ? q1 : double.NaN;
						}
					}
				}
				for (int i = 0; i < catColIndices.Count; i++){
					if (catColIndices[i] >= w.Length){
						categoryAnnotation[i][count] = new string[0];
					} else{
						string q = w[catColIndices[i]].Trim();
						if (q.Length >= 2 && q[0] == '\"' && q[q.Length - 1] == '\"'){
							q = q.Substring(1, q.Length - 2);
						}
						if (q.Length >= 2 && q[0] == '\'' && q[q.Length - 1] == '\''){
							q = q.Substring(1, q.Length - 2);
						}
						string[] ww = q.Length == 0 ? new string[0] : q.Split(';');
						List<int> valids = new List<int>();
						for (int j = 0; j < ww.Length; j++){
							ww[j] = ww[j].Trim();
							if (ww[j].Length > 0){
								valids.Add(j);
							}
						}
						ww = ArrayUtils.SubArray(ww, valids);
						Array.Sort(ww);
						categoryAnnotation[i][count] = ww;
					}
				}
				for (int i = 0; i < numColIndices.Count; i++){
					if (numColIndices[i] >= w.Length){
						numericAnnotation[i][count] = double.NaN;
					} else{
						double q;
						bool success = double.TryParse(w[numColIndices[i]].Trim(), out q);
						numericAnnotation[i][count] = success ? q : double.NaN;
					}
				}
				for (int i = 0; i < textColIndices.Count; i++){
					if (textColIndices[i] >= w.Length){
						stringAnnotation[i][count] = "";
					} else{
						string q = w[textColIndices[i]].Trim();
						stringAnnotation[i][count] = RemoveSplitWhitespace(RemoveQuotes(q));
					}
				}
				count++;
			}
			reader.Close();
			string[] columnNames = ArrayUtils.SubArray(colNames, expressionColIndices);
			string[] catColnames = ArrayUtils.SubArray(colNames, catColIndices);
			string[] numColnames = ArrayUtils.SubArray(colNames, numColIndices);
			string[] multiNumColnames = ArrayUtils.SubArray(colNames, multiNumColIndices);
			string[] textColnames = ArrayUtils.SubArray(colNames, textColIndices);
			matrixData.SetData(origin, RemoveQuotes(columnNames), expressionValues, RemoveQuotes(textColnames), stringAnnotation,
				RemoveQuotes(catColnames), categoryAnnotation, RemoveQuotes(numColnames), numericAnnotation,
				RemoveQuotes(multiNumColnames), multiNumericAnnotation);
			if (colDescriptions != null){
				string[] columnDesc = ArrayUtils.SubArray(colDescriptions, expressionColIndices);
				string[] catColDesc = ArrayUtils.SubArray(colDescriptions, catColIndices);
				string[] numColDesc = ArrayUtils.SubArray(colDescriptions, numColIndices);
				string[] multiNumColDesc = ArrayUtils.SubArray(colDescriptions, multiNumColIndices);
				string[] textColDesc = ArrayUtils.SubArray(colDescriptions, textColIndices);
				matrixData.ExpressionColumnDescriptions = new List<string>(columnDesc);
				matrixData.NumericColumnDescriptions = new List<string>(numColDesc);
				matrixData.CategoryColumnDescriptions = new List<string>(catColDesc);
				matrixData.StringColumnDescriptions = new List<string>(textColDesc);
				matrixData.MultiNumericColumnDescriptions = new List<string>(multiNumColDesc);
			}
			foreach (string key in ArrayUtils.GetKeys(catAnnotatRows)){
				string name = key;
				string[] svals = ArrayUtils.SubArray(catAnnotatRows[key], expressionColIndices);
				string[][] cat = new string[svals.Length][];
				for (int i = 0; i < cat.Length; i++){
					string s = svals[i].Trim();
					cat[i] = s.Length > 0 ? s.Split(';') : new string[0];
					List<int> valids = new List<int>();
					for (int j = 0; j < cat[i].Length; j++){
						cat[i][j] = cat[i][j].Trim();
						if (cat[i][j].Length > 0){
							valids.Add(j);
						}
					}
					cat[i] = ArrayUtils.SubArray(cat[i], valids);
					Array.Sort(cat[i]);
				}
				matrixData.AddCategoryRow(name, name, cat);
			}
			foreach (string key in ArrayUtils.GetKeys(numAnnotatRows)){
				string name = key;
				string[] svals = ArrayUtils.SubArray(numAnnotatRows[key], expressionColIndices);
				double[] num = new double[svals.Length];
				for (int i = 0; i < num.Length; i++){
					string s = svals[i].Trim();
					num[i] = double.NaN;
					double.TryParse(s, out num[i]);
				}
				matrixData.AddNumericRow(name, name, num);
			}
			matrixData.Origin = origin;
			progress(0);
			status("");
		}

		private static string RemoveSplitWhitespace(string s){
			if (!s.Contains(";")){
				return s.Trim();
			}
			string[] q = s.Split(';');
			for (int i = 0; i < q.Length; i++){
				q[i] = q[i].Trim();
			}
			return StringUtils.Concat(";", q);
		}

		private static void SplitAnnotRows(IDictionary<string, string[]> annotRows,
			out Dictionary<string, string[]> catAnnotRows, out Dictionary<string, string[]> numAnnotRows){
			catAnnotRows = new Dictionary<string, string[]>();
			numAnnotRows = new Dictionary<string, string[]>();
			foreach (string name in ArrayUtils.GetKeys(annotRows)){
				if (name.StartsWith("N:")){
					numAnnotRows.Add(name.Substring(2), annotRows[name]);
				} else if (name.StartsWith("C:")){
					catAnnotRows.Add(name.Substring(2), annotRows[name]);
				}
			}
		}

		private static string RemoveQuotes(string name){
			if (name.Length > 2 && name.StartsWith("\"") && name.EndsWith("\"")){
				return name.Substring(1, name.Length - 2);
			}
			return name;
		}

		private static List<string> RemoveQuotes(IEnumerable<string> names){
			List<string> result = new List<string>();
			foreach (string name in names){
				if (name.Length > 2 && name.StartsWith("\"") && name.EndsWith("\"")){
					result.Add(name.Substring(1, name.Length - 2));
				} else{
					result.Add(name);
				}
			}
			return result;
		}

		private static string[] SplitLine(string line, char separator){
			line = line.Trim(new[]{' '});
			bool inQuote = false;
			List<int> sepInds = new List<int>();
			for (int i = 0; i < line.Length; i++){
				char c = line[i];
				if (c == '\"'){
					if (inQuote){
						if (i == line.Length - 1 || line[i + 1] == separator){
							inQuote = false;
						}
					} else{
						if (i == 0 || line[i - 1] == separator){
							inQuote = true;
						}
					}
				} else if (c == separator && !inQuote){
					sepInds.Add(i);
				}
			}
			string[] w = StringUtils.SplitAtIndices(line, sepInds);
			for (int i = 0; i < w.Length; i++){
				string s = w[i].Trim();
				if (s.Length > 1){
					if (s[0] == '\"' && s[s.Length - 1] == '\"'){
						s = s.Substring(1, s.Length - 2);
					}
				}
				w[i] = s;
			}
			return w;
		}
	}
}