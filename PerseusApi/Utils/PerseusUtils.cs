using System;
using System.Collections.Generic;
using System.IO;
using BaseLib.Parse;
using BaseLib.Util;
using BaseLib.Wpf;
using PerseusApi.Generic;
using PerseusApi.Matrix;

namespace PerseusApi.Utils{
	public static class PerseusUtils{
		public static readonly HashSet<string> categoricalColDefaultNames =
			new HashSet<string>(new[]{
				"pfam names", "gocc names", "gomf names", "gobp names", "kegg pathway names", "chromosome", "strand",
				"interpro name", "prints name", "prosite name", "smart name", "sequence motifs", "reactome", "transcription factors"
				, "microrna", "scop class", "scop fold", "scop superfamily", "scop family", "phospho motifs", "mim", "pdb", "intact"
				, "corum", "motifs", "best motif", "reverse", "contaminant", "only identified by site", "type", "amino acid",
				"raw file", "experiment", "charge", "modifications", "md modification", "dp aa", "dp decoy", "dp modification",
				"fraction", "dp cluster index", "authors", "publication", "year", "publisher", "geography", "geography id",
				"identified", "fragmentation", "mass analyzer", "labeling state", "ion mode", "mode", "composition"
			});

		public static readonly HashSet<string> textualColDefaultNames =
			new HashSet<string>(new[]{
				"protein ids", "majority protein ids", "protein names", "gene names", "uniprot", "ensembl", "ensg", "ensp", "enst",
				"mgi", "kegg ortholog", "dip", "hprd interactors", "sequence window", "sequence", "orf name", "names", "proteins",
				"positions within proteins", "leading proteins", "md sequence", "md proteins", "md gene names", "md protein names",
				"dp base sequence", "dp probabilities", "dp proteins", "dp gene names", "dp protein names", "name", "dn sequence",
				"title", "volume", "number", "pages", "modified sequence"
			});

		public static readonly HashSet<string> numericColDefaultNames =
			new HashSet<string>(new[]{
				"length", "position", "total position", "peptides (seq)", "razor peptides (seq)", "unique peptides (seq)",
				"localization prob", "size", "p value", "benj. hoch. fdr", "score", "delta score", "combinatorics", "intensity",
				"score for localization", "pep", "m/z", "mass", "resolution", "uncalibrated - calibrated m/z [ppm]",
				"mass error [ppm]", "uncalibrated mass error [ppm]", "uncalibrated - calibrated m/z [da]", "mass error [da]",
				"uncalibrated mass error [da]", "max intensity m/z 0", "retention length", "retention time",
				"calibrated retention time", "calibrated retention time start", "calibrated retention time finish",
				"retention time calibration", "match time difference", "match q-value", "match score", "number of data points",
				"number of scans", "number of isotopic peaks", "pif", "fraction of total spectrum", "base peak fraction",
				"ms/ms count", "ms/ms m/z", "md base scan number", "md mass error", "md time difference", "dp mass difference",
				"dp time difference", "dp score", "dp pep", "dp positional probability", "dp base scan number", "dp mod scan number"
				, "dp cluster mass", "dp cluster mass sd", "dp cluster size total", "dp cluster size forward",
				"dp cluster size reverse", "dp peptide length difference", "dn score", "dn normalized score", "dn nterm mass",
				"dn cterm mass", "dn score diff", "views", "estimated minutes watched", "average view duration",
				"average percentage viewed", "subscriber views", "subscriber minutes watched", "clicks", "clickable impressions",
				"click through rate", "closes", "closable impressions", "close rate", "impressions", "likes", "likes added",
				"likes removed", "dislikes", "dislikes added", "dislikes removed", "shares", "comments", "favorites",
				"favorites added", "favorites removed", "subscribers", "subscribers gained", "subscribers lost",
				"average view duration (minutes)", "scan number", "ion injection time", "total ion current", "base peak intensity",
				"elapsed time", "precursor full scan number", "precursor intensity", "precursor apex fraction",
				"precursor apex offset", "precursor apex offset time", "scan event number", "scan index", "ms scan index",
				"ms scan number", "agc fill", "parent intensity fraction", "intens comp factor", "ctcd comp", "rawovftt",
				"cycle time", "dead time", "basepeak intensity", "mass calibration", "peak length", "isotope pattern length",
				"multiplet length", "peaks / s", "single peaks / s", "isotope patterns / s", "single isotope patterns / s",
				"multiplets / s", "identified multiplets / s", "multiplet identification rate [%]", "ms/ms / s",
				"identified ms/ms / s", "ms/ms identification rate [%]", "mass fractional part", "mass deficit",
				"mass precision [ppm]", "max intensity m/z 1", "retention length (fwhm)", "min scan number", "max scan number",
				"lys count", "arg count", "intensity", "intensity h", "intensity m", "intensity l", "r count", "k count",
				"closest known m/z", "delta [ppm]", "delta [mda]"
			});

		public static readonly HashSet<string> multiNumericColDefaultNames =
			new HashSet<string>(new[]
			{"mass deviations [da]", "mass deviations [ppm]", "number of phospho (sty)", "protein group ids"});

		public static readonly HashSet<string> commentPrefix = new HashSet<string>(new[]{"#", "!"});
		public static readonly HashSet<string> commentPrefixExceptions = new HashSet<string>(new[]{"#N/A", "#n/a"});

		public static void LoadMatrixData(IDictionary<string, string[]> annotationRows, int[] eInds, int[] cInds, int[] nInds,
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
			LoadMatrixData(colNames, colDescriptions, eInds, cInds, nInds, tInds, mInds, origin, mdata, annotationRows,
				processInfo.Progress, processInfo.Status, separator, reader, nrows);
		}

		private static void LoadMatrixData(IList<string> colNames, IList<string> colDescriptions,
			IList<int> expressionColIndices, IList<int> catColIndices, IList<int> numColIndices, IList<int> textColIndices,
			IList<int> multiNumColIndices, string origin, IMatrixData matrixData, IDictionary<string, string[]> annotationRows,
			Action<int> progress, Action<string> status, char separator, TextReader reader, int nrows){
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

		public static void SelectExact(ICollection<string> colNames, IList<string> colTypes, IList<bool> colVisible,
			MultiListSelectorControl mls){
			for (int i = 0; i < colNames.Count; i++){
				if (colVisible == null || colVisible[i]){
					switch (colTypes[i]){
						case "E":
							mls.SetSelected(0, i, true);
							break;
						case "N":
							mls.SetSelected(1, i, true);
							break;
						case "C":
							mls.SetSelected(2, i, true);
							break;
						case "T":
							mls.SetSelected(3, i, true);
							break;
						case "M":
							mls.SetSelected(4, i, true);
							break;
						default:
							throw new Exception("Unknown type: " + colTypes[i]);
					}
				}
			}
		}

		public static void SelectHeuristic(IList<string> colNames, MultiListSelectorControl mls){
			char guessedType = GuessSilacType(colNames);
			for (int i = 0; i < colNames.Count; i++){
				if (categoricalColDefaultNames.Contains(colNames[i].ToLower())){
					mls.SetSelected(2, i, true);
					continue;
				}
				if (textualColDefaultNames.Contains(colNames[i].ToLower())){
					mls.SetSelected(3, i, true);
					continue;
				}
				if (numericColDefaultNames.Contains(colNames[i].ToLower())){
					mls.SetSelected(1, i, true);
					continue;
				}
				if (multiNumericColDefaultNames.Contains(colNames[i].ToLower())){
					mls.SetSelected(4, i, true);
					continue;
				}
				switch (guessedType){
					case 's':
						if (colNames[i].StartsWith("Norm. Intensity")){
							mls.SetSelected(0, i, true);
						}
						break;
					case 'd':
						if (colNames[i].StartsWith("Ratio H/L Normalized ")){
							mls.SetSelected(0, i, true);
						}
						break;
				}
			}
		}

		public static char GuessSilacType(IEnumerable<string> colnames){
			bool isSilac = false;
			foreach (string s in colnames){
				if (s.StartsWith("Ratio M/L")){
					return 't';
				}
				if (s.StartsWith("Ratio H/L")){
					isSilac = true;
				}
			}
			return isSilac ? 'd' : 's';
		}

		public static string GetNextAvailableName(string s, ICollection<string> taken){
			if (!taken.Contains(s)){
				return s;
			}
			while (true){
				s = GetNext(s);
				if (!taken.Contains(s)){
					return s;
				}
			}
		}

		private static string GetNext(string s){
			if (!HasNumberExtension(s)){
				return s + "_1";
			}
			int x = s.LastIndexOf('_');
			string s1 = s.Substring(x + 1);
			int num = int.Parse(s1);
			return s.Substring(0, x + 1) + (num + 1);
		}

		private static bool HasNumberExtension(string s){
			int x = s.LastIndexOf('_');
			if (x < 0){
				return false;
			}
			string s1 = s.Substring(x + 1);
			int num;
			bool succ = int.TryParse(s1, out num);
			return succ;
		}
	}
}