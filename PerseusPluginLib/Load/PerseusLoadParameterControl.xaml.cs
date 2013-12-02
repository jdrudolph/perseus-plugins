using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;
using BaseLib.Parse;
using BaseLib.Util;

namespace PerseusPluginLib.Load {
	/// <summary>
	/// Interaction logic for PerseusLoadParameterControl.xaml
	/// </summary>
	public partial class PerseusLoadParameterControl : UserControl {
		private static readonly HashSet<string> commentPrefix = new HashSet<string>(new[]{"#", "!"});
		private static readonly HashSet<string> commentPrefixExceptions = new HashSet<string>(new[]{"#N/A", "#n/a"});
		private static readonly HashSet<string> categoricalCols =
			new HashSet<string>(new[]{
				"pfam names", "gocc names", "gomf names", "gobp names", "kegg pathway names", "chromosome", "strand",
				"interpro name", "prints name", "prosite name", "smart name", "sequence motifs", "reactome", "transcription factors"
				, "microrna", "scop class", "scop fold", "scop superfamily", "scop family", "phospho motifs", "mim", "pdb", "intact"
				, "corum", "motifs", "best motif", "reverse", "contaminant", "only identified by site", "type", "amino acid",
				"raw file", "experiment", "charge", "modifications", "md modification", "dp aa", "dp decoy", "dp modification",
				"fraction"
			});
		private static readonly HashSet<string> textualCols =
			new HashSet<string>(new[]{
				"protein ids", "majority protein ids", "protein names", "gene names", "uniprot", "ensembl", "ensg", "ensp", "enst",
				"mgi", "kegg ortholog", "dip", "hprd interactors", "sequence window", "sequence", "orf name", "names", "proteins",
				"positions within proteins", "leading proteins", "md sequence", "md proteins", "md gene names", "md protein names",
				"dp base sequence", "dp probabilities", "dp proteins", "dp gene names", "dp protein names", "name","dn sequence"
			});

		private static readonly HashSet<string> numericCols =
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
				, "dp cluster index", "dp cluster mass", "dp cluster mass sd", "dp cluster size total", "dp cluster size forward",
				"dp cluster size reverse", "dp peptide length difference", "dn score","dn normalized score","dn nterm mass","dn cterm mass",
				"dn score diff"
			});
		private static readonly HashSet<string> multiNumericCols =
			new HashSet<string>(new[]{
				"mass deviations [da]", "mass deviations [ppm]"
			});
		public string Filter { get; set; }
		public PerseusLoadParameterControl() : this(new string[0]) {}
		public PerseusLoadParameterControl(IList<string> items) : this(items, null) {}


		public PerseusLoadParameterControl(IList<string> items, string filename){
			InitializeComponent();
			multiListSelector1.Init(items, new[] { "Expression", "Numerical", "Categorical", "Text", "Multi-numerical" });
			if (!string.IsNullOrEmpty(filename)){
				UpdateFile(filename);
			}
		}

		public string Filename { get { return textBox1.Text; } }
		public int[] ExpressionColumnIndices { get { return multiListSelector1.GetSelectedIndices(0); } }
		public int[] NumericalColumnIndices { get { return multiListSelector1.GetSelectedIndices(1); } }
		public int[] CategoryColumnIndices { get { return multiListSelector1.GetSelectedIndices(2); } }
		public int[] TextColumnIndices { get { return multiListSelector1.GetSelectedIndices(3); } }
		public int[] MultiNumericalColumnIndices { get { return multiListSelector1.GetSelectedIndices(4); } }
		public string[] Value{
			get{
				string[] result = new string[7];
				result[0] = Filename;
				result[1] = StringUtils.Concat(";", multiListSelector1.items);
				result[2] = StringUtils.Concat(";", ExpressionColumnIndices);
				result[3] = StringUtils.Concat(";", NumericalColumnIndices);
				result[4] = StringUtils.Concat(";", CategoryColumnIndices);
				result[5] = StringUtils.Concat(";", TextColumnIndices);
				result[6] = StringUtils.Concat(";", MultiNumericalColumnIndices);
				return result;
			}
			set{
				textBox1.Text = value[0];
				multiListSelector1.items = value[1].Length > 0 ? value[1].Split(';') : new string[0];
				for (int i = 0; i < 5; i++){
					foreach (int ind in GetIndices(value[i + 2])){
						multiListSelector1.SetSelected(i, ind, true);
					}
				}
			}
		}

		private static IEnumerable<int> GetIndices(string s){
			string[] q = s.Length > 0 ? s.Split(';') : new string[0];
			int[] result = new int[q.Length];
			for (int i = 0; i < result.Length; i++){
				result[i] = int.Parse(q[i]);
			}
			return result;
		}

		private void ButtonClick(object sender, EventArgs e){
			Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
			if (Filter != null && !Filter.Equals("")){
				ofd.Filter = Filter;
			}
			if (!ofd.ShowDialog().Value){
				return;
			}
			string filename = ofd.FileName;
			if (string.IsNullOrEmpty(filename)){
				System.Windows.MessageBox.Show("Please specify a filename");
				return;
			}
			if (!File.Exists(filename)){
				System.Windows.MessageBox.Show("File '" + filename + "' does not exist.");
				return;
			}
			UpdateFile(filename);
		}

		internal void UpdateFile(string filename){
			textBox1.Text = filename;
			bool csv = filename.ToLower().EndsWith(".csv");
			char separator = csv ? ',' : '\t';
			string[] colNames;
			Dictionary<string, string[]> annotationRows = new Dictionary<string, string[]>();
			try{
				colNames = TabSep.GetColumnNames(filename, commentPrefix, commentPrefixExceptions, annotationRows, separator);
			} catch (Exception){
				System.Windows.MessageBox.Show("Could not open the file '" + filename + "'. It is probably opened by another program.");
				return;
			}
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
			string msg = TabSep.CanOpen(filename);
			if (msg != null){
				System.Windows.MessageBox.Show(msg);
				return;
			}
			multiListSelector1.Init(colNames);
			if (colTypes != null){
				SelectExact(colNames, colTypes, colVisible);
			} else{
				SelectHeuristic(colNames);
			}
		}

		private void SelectExact(ICollection<string> colNames, IList<string> colTypes, IList<bool> colVisible){
			for (int i = 0; i < colNames.Count; i++){
				if (colVisible == null || colVisible[i]){
					switch (colTypes[i]){
						case "E":
							multiListSelector1.SetSelected(0, i, true);
							break;
						case "N":
							multiListSelector1.SetSelected(1, i, true);
							break;
						case "C":
							multiListSelector1.SetSelected(2, i, true);
							break;
						case "T":
							multiListSelector1.SetSelected(3, i, true);
							break;
						case "M":
							multiListSelector1.SetSelected(4, i, true);
							break;
						default:
							throw new Exception("Unknown type: " + colTypes[i]);
					}
				}
			}
		}

		private void SelectHeuristic(IList<string> colNames){
			char guessedType = GuessSilacType(colNames);
			for (int i = 0; i < colNames.Count; i++){
				if (categoricalCols.Contains(colNames[i].ToLower())){
					multiListSelector1.SetSelected(2, i, true);
					continue;
				}
				if (textualCols.Contains(colNames[i].ToLower())){
					multiListSelector1.SetSelected(3, i, true);
					continue;
				}
				if (numericCols.Contains(colNames[i].ToLower())) {
					multiListSelector1.SetSelected(1, i, true);
					continue;
				}
				if (multiNumericCols.Contains(colNames[i].ToLower())) {
					multiListSelector1.SetSelected(4, i, true);
					continue;
				}
				switch (guessedType) {
					case 's':
						if (colNames[i].StartsWith("Norm. Intensity")){
							multiListSelector1.SetSelected(0, i, true);
						}
						break;
					case 'd':
						if (colNames[i].StartsWith("Ratio H/L Normalized ")){
							multiListSelector1.SetSelected(0, i, true);
						}
						break;
				}
			}
		}

		private static char GuessSilacType(IEnumerable<string> colnames){
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

		public string Text { get { return textBox1.Text; } set { textBox1.Text = value; } }
	}
}
