using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using BaseLib.Parse;
using BaseLib.Util;
using PerseusApi.Utils;

namespace PerseusPluginLib.Load{
	/// <summary>
	/// Interaction logic for PerseusLoadParameterControl.xaml
	/// </summary>
	public partial class PerseusLoadParameterControl : UserControl{
		public string Filter { get; set; }
		public PerseusLoadParameterControl() : this(new string[0]) {}
		public PerseusLoadParameterControl(IList<string> items) : this(items, null) {}

		public PerseusLoadParameterControl(IList<string> items, string filename){
			InitializeComponent();
			MultiListSelector1.Init(items, new[]{"Expression", "Numerical", "Categorical", "Text", "Multi-numerical"});
			if (!string.IsNullOrEmpty(filename)){
				UpdateFile(filename);
			}
		}

		public string Filename { get { return TextBox1.Text; } }
		public int[] ExpressionColumnIndices { get { return MultiListSelector1.GetSelectedIndices(0); } }
		public int[] NumericalColumnIndices { get { return MultiListSelector1.GetSelectedIndices(1); } }
		public int[] CategoryColumnIndices { get { return MultiListSelector1.GetSelectedIndices(2); } }
		public int[] TextColumnIndices { get { return MultiListSelector1.GetSelectedIndices(3); } }
		public int[] MultiNumericalColumnIndices { get { return MultiListSelector1.GetSelectedIndices(4); } }
		public string[] Value{
			get{
				string[] result = new string[7];
				result[0] = Filename;
				result[1] = StringUtils.Concat(";", MultiListSelector1.items);
				result[2] = StringUtils.Concat(";", ExpressionColumnIndices);
				result[3] = StringUtils.Concat(";", NumericalColumnIndices);
				result[4] = StringUtils.Concat(";", CategoryColumnIndices);
				result[5] = StringUtils.Concat(";", TextColumnIndices);
				result[6] = StringUtils.Concat(";", MultiNumericalColumnIndices);
				return result;
			}
			set{
				TextBox1.Text = value[0];
				MultiListSelector1.items = value[1].Length > 0 ? value[1].Split(';') : new string[0];
				for (int i = 0; i < 5; i++){
					foreach (int ind in GetIndices(value[i + 2])){
						MultiListSelector1.SetSelected(i, ind, true);
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

		internal void UpdateFile(string filename){
			TextBox1.Text = filename;
			bool csv = filename.ToLower().EndsWith(".csv");
			char separator = csv ? ',' : '\t';
			string[] colNames;
			Dictionary<string, string[]> annotationRows = new Dictionary<string, string[]>();
			try{
				colNames = TabSep.GetColumnNames(filename, PerseusUtils.commentPrefix, PerseusUtils.commentPrefixExceptions,
					annotationRows, separator);
			} catch (Exception){
				MessageBox.Show("Could not open the file '" + filename + "'. It is probably opened by another program.");
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
				MessageBox.Show(msg);
				return;
			}
			MultiListSelector1.Init(colNames);
			if (colTypes != null){
				PerseusUtils.SelectExact(colNames, colTypes, colVisible, MultiListSelector1);
			} else{
				PerseusUtils.SelectHeuristic(colNames, MultiListSelector1);
			}
		}

		public string Text { get { return TextBox1.Text; } set { TextBox1.Text = value; } }

		private void Button1_OnClick(object sender, RoutedEventArgs e){
			Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
			if (Filter != null && !Filter.Equals("")){
				ofd.Filter = Filter;
			}
			if (!ofd.ShowDialog().Value){
				return;
			}
			string filename = ofd.FileName;
			if (string.IsNullOrEmpty(filename)){
				MessageBox.Show("Please specify a filename");
				return;
			}
			if (!File.Exists(filename)){
				MessageBox.Show("File '" + filename + "' does not exist.");
				return;
			}
			UpdateFile(filename);
		}
	}
}