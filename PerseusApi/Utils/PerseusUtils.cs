using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BaseLib.Param;
using BaseLib.Wpf;
using BaseLibS.Num;
using BaseLibS.Param;
using BaseLibS.Parse;
using BaseLibS.Util;
using Calc;
using PerseusApi.Generic;
using PerseusApi.Matrix;

namespace PerseusApi.Utils{
	public static class PerseusUtils{
		public static readonly HashSet<string> categoricalColDefaultNames =
			new HashSet<string>(new[]{
				"pfam names", "gocc names", "gomf names", "gobp names", "kegg pathway names", "chromosome", "strand",
				"interpro name", "prints name", "prosite name", "smart name", "sequence motifs", "reactome", "transcription factors",
				"microrna", "scop class", "scop fold", "scop superfamily", "scop family", "phospho motifs", "mim", "pdb", "intact",
				"corum", "motifs", "best motif", "reverse", "contaminant", "potential contaminant", "only identified by site",
				"type", "amino acid", "raw file", "experiment", "charge", "modifications", "md modification", "dp aa", "dp decoy",
				"dp modification", "fraction", "dp cluster index", "authors", "publication", "year", "publisher", "geography",
				"geography id", "identified", "fragmentation", "mass analyzer", "labeling state", "ion mode", "mode", "composition",
				"isotope cluster index"
			});

		public static readonly HashSet<string> textualColDefaultNames =
			new HashSet<string>(new[]{
				"protein ids", "protein", "majority protein ids", "protein names", "gene names", "uniprot", "ensembl", "ensg",
				"ensp", "enst", "mgi", "kegg ortholog", "dip", "hprd interactors", "sequence window", "sequence", "orf name",
				"names", "proteins", "positions within proteins", "leading proteins", "md sequence", "md proteins", "md gene names",
				"md protein names", "dp base sequence", "dp probabilities", "dp proteins", "dp gene names", "dp protein names",
				"name", "dn sequence", "title", "volume", "number", "pages", "modified sequence"
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
				"dp time difference", "dp score", "dp pep", "dp positional probability", "dp base scan number", "dp mod scan number",
				"dp cluster mass", "dp cluster mass sd", "dp cluster size total", "dp cluster size forward",
				"dp cluster size reverse", "dp peptide length difference", "dn score", "dn normalized score", "dn nterm mass",
				"dn cterm mass", "dn missing mass", "dn score diff", "views", "estimated minutes watched", "average view duration",
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
				"lys count", "arg count", "intensity", "intensity h", "intensity m", "intensity l", "r count", "k count", "jitter",
				"closest known m/z", "delta [ppm]", "delta [mda]", "q-value", "number of frames", "min frame number",
				"max frame number", "ion mobility index", "ion mobility index length", "ion mobility index length (fwhm)",
				"isotope correlation", "peptides", "razor + unique peptides", "unique peptides", "sequence coverage [%]",
				"unique sequence coverage [%]", "unique + razor sequence coverage [%]", "mol. weight [kda]"
			});

		public static readonly HashSet<string> multiNumericColDefaultNames =
			new HashSet<string>(new[]
			{"mass deviations [da]", "mass deviations [ppm]", "number of phospho (sty)", "protein group ids"});

		public static readonly HashSet<string> commentPrefix = new HashSet<string>(new[]{"#", "!"});
		public static readonly HashSet<string> commentPrefixExceptions = new HashSet<string>(new[]{"#N/A", "#n/a"});

		public static void LoadMatrixData(IDictionary<string, string[]> annotationRows, int[] eInds, int[] cInds, int[] nInds,
			int[] tInds, int[] mInds, ProcessInfo processInfo, IList<string> colNames, IMatrixData mdata, StreamReader reader,
			string filename, int nrows, string origin, char separator, bool shortenExpressionNames){
			string[] colDescriptions = null;
			if (annotationRows.ContainsKey("Description")){
				colDescriptions = annotationRows["Description"];
				annotationRows.Remove("Description");
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
				processInfo.Progress, processInfo.Status, separator, reader, filename, nrows, shortenExpressionNames);
		}

		private static void LoadMatrixData(IList<string> colNames, IList<string> colDescriptions,
			IList<int> expressionColIndices, IList<int> catColIndices, IList<int> numColIndices, IList<int> textColIndices,
			IList<int> multiNumColIndices, string origin, IMatrixData matrixData, IDictionary<string, string[]> annotationRows,
			Action<int> progress, Action<string> status, char separator, StreamReader reader, string filename, int nrows,
			bool shortenExpressionNames){
			Dictionary<string, string[]> catAnnotatRows;
			Dictionary<string, string[]> numAnnotatRows;
			status("Reading data");
			SplitAnnotRows(annotationRows, out catAnnotatRows, out numAnnotatRows);
			List<string[][]> categoryAnnotation = new List<string[][]>();
			for (int i = 0; i < catColIndices.Count; i++){
				categoryAnnotation.Add(new string[nrows][]);
			}
			List<double[]> numericAnnotation = new List<double[]>();
			for (int i = 0; i < numColIndices.Count; i++){
				numericAnnotation.Add(new double[nrows]);
			}
			List<double[][]> multiNumericAnnotation = new List<double[][]>();
			for (int i = 0; i < multiNumColIndices.Count; i++){
				multiNumericAnnotation.Add(new double[nrows][]);
			}
			List<string[]> stringAnnotation = new List<string[]>();
			for (int i = 0; i < textColIndices.Count; i++){
				stringAnnotation.Add(new string[nrows]);
			}
			float[,] expressionValues = new float[nrows, expressionColIndices.Count];
			float[,] qualityValues = null;
			bool[,] isImputedValues = null;
			bool hasAddtlMatrices = false;
			if (!string.IsNullOrEmpty(filename)){
				hasAddtlMatrices = GetHasAddtlMatrices(filename, expressionColIndices, separator);
			}
			if (hasAddtlMatrices){
				qualityValues = new float[nrows, expressionColIndices.Count];
				isImputedValues = new bool[nrows, expressionColIndices.Count];
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
						if (hasAddtlMatrices){
							ParseExp(s, out expressionValues[count, i], out isImputedValues[count, i], out qualityValues[count, i]);
						} else{
							if (count < expressionValues.GetLength(0)){
								bool success = float.TryParse(s, out expressionValues[count, i]);
								if (!success){
									expressionValues[count, i] = float.NaN;
								}
							}
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
						if (categoryAnnotation[i].Length > count){
							categoryAnnotation[i][count] = ww;
						}
					}
				}
				for (int i = 0; i < numColIndices.Count; i++){
					if (numColIndices[i] >= w.Length){
						numericAnnotation[i][count] = double.NaN;
					} else{
						double q;
						bool success = double.TryParse(w[numColIndices[i]].Trim(), out q);
						if (numericAnnotation[i].Length > count){
							numericAnnotation[i][count] = success ? q : double.NaN;
						}
					}
				}
				for (int i = 0; i < textColIndices.Count; i++){
					if (textColIndices[i] >= w.Length){
						stringAnnotation[i][count] = "";
					} else{
						string q = w[textColIndices[i]].Trim();
						if (stringAnnotation[i].Length > count){
							stringAnnotation[i][count] = RemoveSplitWhitespace(RemoveQuotes(q));
						}
					}
				}
				count++;
			}
			reader.Close();
			string[] columnNames = ArrayUtils.SubArray(colNames, expressionColIndices);
			if (shortenExpressionNames){
				columnNames = StringUtils.RemoveCommonSubstrings(columnNames);
			}
			string[] catColnames = ArrayUtils.SubArray(colNames, catColIndices);
			string[] numColnames = ArrayUtils.SubArray(colNames, numColIndices);
			string[] multiNumColnames = ArrayUtils.SubArray(colNames, multiNumColIndices);
			string[] textColnames = ArrayUtils.SubArray(colNames, textColIndices);
			matrixData.Name = origin;
			matrixData.ColumnNames = RemoveQuotes(columnNames);
			matrixData.Values.Set(expressionValues);
			if (hasAddtlMatrices){
				matrixData.Quality.Set(qualityValues);
				matrixData.IsImputed.Set(isImputedValues);
			} else{
				matrixData.Quality.Set(new float[expressionValues.GetLength(0), expressionValues.GetLength(1)]);
				matrixData.IsImputed.Set(new bool[expressionValues.GetLength(0), expressionValues.GetLength(1)]);
			}
			matrixData.SetAnnotationColumns(RemoveQuotes(textColnames), stringAnnotation, RemoveQuotes(catColnames),
				categoryAnnotation, RemoveQuotes(numColnames), numericAnnotation, RemoveQuotes(multiNumColnames),
				multiNumericAnnotation);
			if (colDescriptions != null){
				string[] columnDesc = ArrayUtils.SubArray(colDescriptions, expressionColIndices);
				string[] catColDesc = ArrayUtils.SubArray(colDescriptions, catColIndices);
				string[] numColDesc = ArrayUtils.SubArray(colDescriptions, numColIndices);
				string[] multiNumColDesc = ArrayUtils.SubArray(colDescriptions, multiNumColIndices);
				string[] textColDesc = ArrayUtils.SubArray(colDescriptions, textColIndices);
				matrixData.ColumnDescriptions = new List<string>(columnDesc);
				matrixData.NumericColumnDescriptions = new List<string>(numColDesc);
				matrixData.CategoryColumnDescriptions = new List<string>(catColDesc);
				matrixData.StringColumnDescriptions = new List<string>(textColDesc);
				matrixData.MultiNumericColumnDescriptions = new List<string>(multiNumColDesc);
			}
			foreach (string key in catAnnotatRows.Keys){
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
			foreach (string key in numAnnotatRows.Keys){
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

		public static void ParseExp(string s, out float expressionValue, out bool isImputedValue, out float qualityValue){
			string[] w = s.Split(';');
			expressionValue = float.NaN;
			isImputedValue = false;
			qualityValue = float.NaN;
			if (w.Length > 0){
				bool success = float.TryParse(w[0], out expressionValue);
				if (!success){
					expressionValue = float.NaN;
				}
			}
			if (w.Length > 1){
				bool success = bool.TryParse(w[1], out isImputedValue);
				if (!success){
					isImputedValue = false;
				}
			}
			if (w.Length > 2){
				bool success = float.TryParse(w[2], out qualityValue);
				if (!success){
					qualityValue = float.NaN;
				}
			}
		}

		public static bool GetHasAddtlMatrices(string filename, IList<int> expressionColIndices, char separator){
			if (expressionColIndices.Count == 0){
				return false;
			}
			StreamReader reader = new StreamReader(filename);
			int expressionColIndex = expressionColIndices[0];
			reader.ReadLine();
			string line;
			bool hasAddtl = false;
			while ((line = reader.ReadLine()) != null){
				if (TabSep.IsCommentLine(line, commentPrefix, commentPrefixExceptions)){
					continue;
				}
				string[] w = SplitLine(line, separator);
				if (expressionColIndex < w.Length){
					string s = StringUtils.RemoveWhitespace(w[expressionColIndex]);
					hasAddtl = s.Contains(";");
					break;
				}
			}
			reader.Close();
			return hasAddtl;
		}

		public static string RemoveSplitWhitespace(string s){
			if (!s.Contains(";")){
				return s.Trim();
			}
			string[] q = s.Split(';');
			for (int i = 0; i < q.Length; i++){
				q[i] = q[i].Trim();
			}
			return StringUtils.Concat(";", q);
		}

		public static void SplitAnnotRows(IDictionary<string, string[]> annotRows,
			out Dictionary<string, string[]> catAnnotRows, out Dictionary<string, string[]> numAnnotRows){
			catAnnotRows = new Dictionary<string, string[]>();
			numAnnotRows = new Dictionary<string, string[]>();
			foreach (string name in annotRows.Keys){
				if (name.StartsWith("N:")){
					numAnnotRows.Add(name.Substring(2), annotRows[name]);
				} else if (name.StartsWith("C:")){
					catAnnotRows.Add(name.Substring(2), annotRows[name]);
				}
			}
		}

		public static string RemoveQuotes(string name){
			if (name.Length > 2 && name.StartsWith("\"") && name.EndsWith("\"")){
				return name.Substring(1, name.Length - 2);
			}
			return name;
		}

		public static List<string> RemoveQuotes(IEnumerable<string> names){
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

		public static string[] SplitLine(string line, char separator){
			line = line.Trim(' ');
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

		public static void SelectExact(ICollection<string> colNames, IList<string> colTypes, MultiListSelectorControl mls){
			for (int i = 0; i < colNames.Count; i++){
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

		public static string[][] GetAvailableAnnots(out string[] baseNames, out string[] files){
			AnnotType[][] types;
			return GetAvailableAnnots(out baseNames, out types, out files);
		}

		public static string[][] GetAvailableAnnots(out string[] baseNames, out AnnotType[][] types, out string[] files){
			files = GetAnnotFiles();
			baseNames = new string[files.Length];
			types = new AnnotType[files.Length][];
			string[][] names = new string[files.Length][];
			for (int i = 0; i < names.Length; i++){
				names[i] = GetAvailableAnnots(files[i], out baseNames[i], out types[i]);
			}
			return names;
		}

		private static string[] GetAvailableAnnots(string file, out string baseName, out AnnotType[] types){
			StreamReader reader = FileUtils.GetReader(file);
			string line = reader.ReadLine();
			string[] header = line.Split('\t');
			line = reader.ReadLine();
			string[] desc = line.Split('\t');
			reader.Close();
			baseName = header[0];
			string[] result = ArrayUtils.SubArray(header, 1, header.Length);
			types = new AnnotType[desc.Length - 1];
			for (int i = 0; i < types.Length; i++){
				types[i] = FromString1(desc[i + 1]);
			}
			return result;
		}

		private static AnnotType FromString1(string s){
			switch (s){
				case "Text":
					return AnnotType.Text;
				case "Categorical":
					return AnnotType.Categorical;
				case "Numerical":
					return AnnotType.Numerical;
				default:
					return AnnotType.Categorical;
			}
		}

		private static string[] GetAnnotFiles(){
			string folder = FileUtils.executablePath + "\\conf\\annotations";
			string[] files = Directory.GetFiles(folder);
			List<string> result = new List<string>();
			foreach (string file in files){
				string fileLow = file.ToLower();
				if (fileLow.EndsWith(".txt.gz") || fileLow.EndsWith(".txt")){
					result.Add(file);
				}
			}
			return result.ToArray();
		}

		public static bool ProcessDataAddAnnotation(int nrows, Parameters para, string[] baseIds, ProcessInfo processInfo,
			out string[] name, out int[] catColInds, out int[] textColInds, out int[] numColInds, out string[][][] catCols,
			out string[][] textCols, out double[][] numCols){
			string[] baseNames;
			AnnotType[][] types;
			string[] files;
			string[][] names = GetAvailableAnnots(out baseNames, out types, out files);
			const bool deHyphenate = true;
			ParameterWithSubParams<int> spd = para.GetParamWithSubParams<int>("Source");
			int ind = spd.Value;
			Parameters param = spd.GetSubParameters();
			AnnotType[] type = types[ind];
			name = names[ind];
			int[] addtlSources = para.GetParam<int[]>("Additional sources").Value;
			addtlSources = ArrayUtils.Remove(addtlSources, ind);
			foreach (int addtlSource in addtlSources){
				AnnotType[] type1 = types[addtlSource];
				string[] name1 = names[addtlSource];
				if (!ArrayUtils.EqualArrays(type, type1)){
					processInfo.ErrString = "Additional annotation file does not have the same column structure.";
					catColInds = new int[]{};
					textColInds = new int[]{};
					numColInds = new int[]{};
					catCols = new string[][][]{};
					textCols = new string[][]{};
					numCols = new double[][]{};
					return false;
				}
				if (!ArrayUtils.EqualArrays(name, name1)){
					processInfo.ErrString = "Additional annotation file does not have the same column structure.";
					catColInds = new int[]{};
					textColInds = new int[]{};
					numColInds = new int[]{};
					catCols = new string[][][]{};
					textCols = new string[][]{};
					numCols = new double[][]{};
					return false;
				}
			}
			int[] selection = param.GetParam<int[]>("Annotations to be added").Value;
			type = ArrayUtils.SubArray(type, selection);
			name = ArrayUtils.SubArray(name, selection);
			HashSet<string> allIds = GetAllIds(baseIds, deHyphenate);
			Dictionary<string, string[]> mapping = ReadMapping(allIds, files[ind], selection);
			foreach (int addtlSource in addtlSources){
				Dictionary<string, string[]> mapping1 = ReadMapping(allIds, files[addtlSource], selection);
				foreach (string key in mapping1.Keys.Where(key => !mapping.ContainsKey(key))){
					mapping.Add(key, mapping1[key]);
				}
			}
			SplitIds(type, out textColInds, out catColInds, out numColInds);
			catCols = new string[catColInds.Length][][];
			for (int i = 0; i < catCols.Length; i++){
				catCols[i] = new string[nrows][];
			}
			textCols = new string[textColInds.Length][];
			for (int i = 0; i < textCols.Length; i++){
				textCols[i] = new string[nrows];
			}
			numCols = new double[numColInds.Length][];
			for (int i = 0; i < numCols.Length; i++){
				numCols[i] = new double[nrows];
			}
			for (int i = 0; i < nrows; i++){
				string[] ids = baseIds[i].Length > 0 ? baseIds[i].Split(';') : new string[0];
				HashSet<string>[] catVals = new HashSet<string>[catCols.Length];
				for (int j = 0; j < catVals.Length; j++){
					catVals[j] = new HashSet<string>();
				}
				HashSet<string>[] textVals = new HashSet<string>[textCols.Length];
				for (int j = 0; j < textVals.Length; j++){
					textVals[j] = new HashSet<string>();
				}
				List<double>[] numVals = new List<double>[numCols.Length];
				for (int j = 0; j < numVals.Length; j++){
					numVals[j] = new List<double>();
				}
				foreach (string id in ids){
					if (mapping.ContainsKey(id)){
						string[] values = mapping[id];
						AddCatVals(ArrayUtils.SubArray(values, catColInds), catVals);
						AddTextVals(ArrayUtils.SubArray(values, textColInds), textVals);
						AddNumVals(ArrayUtils.SubArray(values, numColInds), numVals);
					} else if (id.Contains("-")){
						string q = id.Substring(0, id.IndexOf('-'));
						if (mapping.ContainsKey(q)){
							string[] values = mapping[q];
							AddCatVals(ArrayUtils.SubArray(values, catColInds), catVals);
							AddTextVals(ArrayUtils.SubArray(values, textColInds), textVals);
							AddNumVals(ArrayUtils.SubArray(values, numColInds), numVals);
						}
					}
				}
				for (int j = 0; j < catVals.Length; j++){
					string[] q = ArrayUtils.ToArray(catVals[j]);
					Array.Sort(q);
					catCols[j][i] = q;
				}
				for (int j = 0; j < textVals.Length; j++){
					string[] q = ArrayUtils.ToArray(textVals[j]);
					Array.Sort(q);
					textCols[j][i] = StringUtils.Concat(";", q);
				}
				for (int j = 0; j < numVals.Length; j++){
					numCols[j][i] = ArrayUtils.Median(numVals[j]);
				}
			}
			return true;
		}

		private static void AddCatVals(IList<string> values, IList<HashSet<string>> catVals){
			for (int i = 0; i < values.Count; i++){
				AddCatVals(values[i], catVals[i]);
			}
		}

		private static void AddTextVals(IList<string> values, IList<HashSet<string>> textVals){
			for (int i = 0; i < values.Count; i++){
				AddTextVals(values[i], textVals[i]);
			}
		}

		private static void AddNumVals(IList<string> values, IList<List<double>> numVals){
			for (int i = 0; i < values.Count; i++){
				AddNumVals(values[i], numVals[i]);
			}
		}

		private static void AddCatVals(string value, ISet<string> catVals){
			string[] q = value.Length > 0 ? value.Split(';') : new string[0];
			foreach (string s in q){
				catVals.Add(s);
			}
		}

		private static void AddTextVals(string value, ISet<string> textVals){
			string[] q = value.Length > 0 ? value.Split(';') : new string[0];
			foreach (string s in q){
				textVals.Add(s);
			}
		}

		private static void AddNumVals(string value, ICollection<double> numVals){
			string[] q = value.Length > 0 ? value.Split(';') : new string[0];
			foreach (string s in q){
				numVals.Add(double.Parse(s));
			}
		}

		private static void SplitIds(IList<AnnotType> types, out int[] textCols, out int[] catCols, out int[] numCols){
			List<int> tc = new List<int>();
			List<int> cc = new List<int>();
			List<int> nc = new List<int>();
			for (int i = 0; i < types.Count; i++){
				switch (types[i]){
					case AnnotType.Categorical:
						cc.Add(i);
						break;
					case AnnotType.Text:
						tc.Add(i);
						break;
					case AnnotType.Numerical:
						nc.Add(i);
						break;
					default:
						throw new Exception("Never get here.");
				}
			}
			textCols = tc.ToArray();
			catCols = cc.ToArray();
			numCols = nc.ToArray();
		}

		private static Dictionary<string, string[]> ReadMapping(ICollection<string> allIds, string file, IList<int> selection){
			for (int i = 0; i < selection.Count; i++){
				selection[i]++;
			}
			StreamReader reader = FileUtils.GetReader(file);
			reader.ReadLine();
			reader.ReadLine();
			string line;
			Dictionary<string, string[]> result = new Dictionary<string, string[]>();
			while ((line = reader.ReadLine()) != null){
				string[] q = line.Split('\t');
				string w = q[0];
				string[] ids = w.Length > 0 ? w.Split(';') : new string[0];
				string[] value = ArrayUtils.SubArray(q, selection);
				foreach (string id in ids){
					if (!allIds.Contains(id)){
						continue;
					}
					if (!result.ContainsKey(id)){
						result.Add(id, value);
					}
				}
			}
			return result;
		}

		private static HashSet<string> GetAllIds(IEnumerable<string> x, bool deHyphenate){
			HashSet<string> result = new HashSet<string>();
			foreach (string y in x){
				string[] z = y.Length > 0 ? y.Split(';') : new string[0];
				foreach (string q in z){
					result.Add(q);
					if (deHyphenate && q.Contains("-")){
						string r = q.Substring(0, q.IndexOf("-", StringComparison.InvariantCulture));
						result.Add(r);
					}
				}
			}
			return result;
		}

		public static Parameter[] GetMultiNumFilterParams(string[] selection){
			return new Parameter[]{};
		}

		public static Parameter[] GetTextFilterParams(string[] selection){
			return new Parameter[]{};
		}

		public static Parameter[] GetCatFilterParams(string[] selection){
			return new Parameter[]{};
		}

		public static Parameter[] GetNumFilterParams(string[] selection){
			return new[]{
				GetColumnSelectionParameter(selection), GetRelationsParameter(),
				new SingleChoiceParam("Combine through", 0){Values = new[]{"intersection", "union"}}
			};
		}

		private static Parameter GetColumnSelectionParameter(string[] selection){
			const int maxCols = 5;
			string[] values = new string[maxCols];
			Parameters[] subParams = new Parameters[maxCols];
			for (int i = 1; i <= maxCols; i++){
				values[i - 1] = "" + i;
				Parameter[] px = new Parameter[i];
				for (int j = 0; j < i; j++){
					px[j] = new SingleChoiceParam(GetVariableName(j), j){Values = selection};
				}
				Parameters p = new Parameters(px);
				subParams[i - 1] = p;
			}
			return new SingleChoiceWithSubParams("Number of columns", 0){
				Values = values,
				SubParams = subParams,
				ParamNameWidth = 120,
				TotalWidth = 800
			};
		}

		public static string GetVariableName(int i){
			const string x = "xyzabc";
			return "" + x[i];
		}

		private static Parameter GetRelationsParameter(){
			const int maxCols = 5;
			string[] values = new string[maxCols];
			Parameters[] subParams = new Parameters[maxCols];
			for (int i = 1; i <= maxCols; i++){
				values[i - 1] = "" + i;
				Parameter[] px = new Parameter[i];
				for (int j = 0; j < i; j++){
					px[j] = new StringParam("Relation " + (j + 1));
				}
				Parameters p = new Parameters(px);
				subParams[i - 1] = p;
			}
			return new SingleChoiceWithSubParams("Number of relations", 0){
				Values = values,
				SubParams = subParams,
				ParamNameWidth = 120,
				TotalWidth = 800
			};
		}

		public static Relation[] GetRelations(Parameters parameters, string[] realVariableNames){
			ParameterWithSubParams<int> sp = parameters.GetParamWithSubParams<int>("Number of relations");
			int nrel = sp.Value + 1;
			List<Relation> result = new List<Relation>();
			Parameters param = sp.GetSubParameters();
			for (int j = 0; j < nrel; j++){
				string rel = param.GetParam<string>("Relation " + (j + 1)).Value;
				if (rel.StartsWith(">") || rel.StartsWith("<") || rel.StartsWith("=")){
					rel = "x" + rel;
				}
				string err1;
				Relation r = Relation.CreateFromString(rel, realVariableNames, new string[0], out err1);
				result.Add(r);
			}
			return result.ToArray();
		}
	}
}