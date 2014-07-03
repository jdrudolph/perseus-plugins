using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using BaseLib.Param;
using BaseLib.Util;
using PerseusApi.Generic;
using PerseusApi.Matrix;
using PerseusPluginLib.Properties;

namespace PerseusPluginLib.Export{
	public class TabSeparatedExport : IMatrixExport{
		public bool HasButton { get { return true; } }
		public Bitmap DisplayImage { get { return Resources.Save_icon; } }
		public string Description { get { return "Save the matrix to a tab-separated text file. Information on column types will be retained."; } }
		public string Name { get { return "Generic matrix export"; } }
		public bool IsActive { get { return true; } }
		public float DisplayRank { get { return 0; } }
		public int GetMaxThreads(Parameters parameters) { return 1; }
		public string Url { get { return "http://141.61.102.17/perseus_doku/doku.php?id=perseus:activities:MatrixExport:TabSeparatedExport"; } }

		public void Export(Parameters parameters, IMatrixData data, ProcessInfo processInfo){
			string filename = parameters.GetFileParam("File name").Value;
			StreamWriter writer;
			try{
				writer = new StreamWriter(filename);
			} catch (Exception e){
				processInfo.ErrString = e.Message;
				return;
			}
			List<string> words = new List<string>();
			for (int i = 0; i < data.ExpressionColumnCount; i++){
				words.Add(Trunc(data.ExpressionColumnNames[i]));
			}
			for (int i = 0; i < data.CategoryColumnCount; i++){
				words.Add(Trunc(data.CategoryColumnNames[i]));
			}
			for (int i = 0; i < data.NumericColumnCount; i++){
				words.Add(Trunc(data.NumericColumnNames[i]));
			}
			for (int i = 0; i < data.StringColumnCount; i++){
				words.Add(Trunc(data.StringColumnNames[i]));
			}
			for (int i = 0; i < data.MultiNumericColumnCount; i++){
				words.Add(Trunc(data.MultiNumericColumnNames[i]));
			}
			writer.WriteLine(StringUtils.Concat("\t", words));
			if (HasAnyDescription(data)){
				words = new List<string>();
				for (int i = 0; i < data.ExpressionColumnCount; i++){
					words.Add(Trunc(data.ExpressionColumnDescriptions[i] ?? ""));
				}
				for (int i = 0; i < data.CategoryColumnCount; i++){
					words.Add(Trunc(data.CategoryColumnDescriptions[i] ?? ""));
				}
				for (int i = 0; i < data.NumericColumnCount; i++){
					words.Add(Trunc(data.NumericColumnDescriptions[i] ?? ""));
				}
				for (int i = 0; i < data.StringColumnCount; i++){
					words.Add(Trunc(data.StringColumnDescriptions[i] ?? ""));
				}
				for (int i = 0; i < data.MultiNumericColumnCount; i++){
					words.Add(Trunc(data.MultiNumericColumnDescriptions[i] ?? ""));
				}
				writer.WriteLine("#!{Description}" + StringUtils.Concat("\t", words));
			}
			words = new List<string>();
			for (int i = 0; i < data.ExpressionColumnCount; i++){
				words.Add("E");
			}
			for (int i = 0; i < data.CategoryColumnCount; i++){
				words.Add("C");
			}
			for (int i = 0; i < data.NumericColumnCount; i++){
				words.Add("N");
			}
			for (int i = 0; i < data.StringColumnCount; i++){
				words.Add("T");
			}
			for (int i = 0; i < data.MultiNumericColumnCount; i++){
				words.Add("M");
			}
			writer.WriteLine("#!{Type}" + StringUtils.Concat("\t", words));
			for (int i = 0; i < data.NumericRowCount; i++){
				words = new List<string>();
				for (int j = 0; j < data.ExpressionColumnCount; j++){
					words.Add("" + data.NumericRows[i][j]);
				}
				for (int j = 0; j < data.CategoryColumnCount; j++){
					words.Add("");
				}
				for (int j = 0; j < data.NumericColumnCount; j++){
					words.Add("");
				}
				for (int j = 0; j < data.StringColumnCount; j++){
					words.Add("");
				}
				for (int j = 0; j < data.MultiNumericColumnCount; j++){
					words.Add("");
				}
				writer.WriteLine("#!{N:" + data.NumericRowNames[i] + "}" + StringUtils.Concat("\t", words));
			}
			for (int i = 0; i < data.CategoryRowCount; i++){
				words = new List<string>();
				for (int j = 0; j < data.ExpressionColumnCount; j++){
					string[] s = data.GetCategoryRowAt(i)[j];
					words.Add(s.Length == 0 ? "" : StringUtils.Concat(";", s));
				}
				for (int j = 0; j < data.CategoryColumnCount; j++){
					words.Add("");
				}
				for (int j = 0; j < data.NumericColumnCount; j++){
					words.Add("");
				}
				for (int j = 0; j < data.StringColumnCount; j++){
					words.Add("");
				}
				for (int j = 0; j < data.MultiNumericColumnCount; j++){
					words.Add("");
				}
				writer.WriteLine("#!{C:" + data.CategoryRowNames[i] + "}" + StringUtils.Concat("\t", words));
			}
			for (int j = 0; j < data.RowCount; j++){
				words = new List<string>();
				for (int i = 0; i < data.ExpressionColumnCount; i++){
					words.Add(Trunc("" + data[j, i]));
				}
				for (int i = 0; i < data.CategoryColumnCount; i++){
					string[] q = data.GetCategoryColumnEntryAt(i, j) ?? new string[0];
					words.Add(Trunc((q.Length > 0 ? StringUtils.Concat(";", q) : "")));
				}
				for (int i = 0; i < data.NumericColumnCount; i++){
					words.Add(Trunc("" + data.NumericColumns[i][j]));
				}
				for (int i = 0; i < data.StringColumnCount; i++){
					words.Add(Trunc(data.StringColumns[i][j]));
				}
				for (int i = 0; i < data.MultiNumericColumnCount; i++){
					double[] q = data.MultiNumericColumns[i][j];
					words.Add(Trunc((q.Length > 0 ? StringUtils.Concat(";", q) : "")));
				}
				string s = StringUtils.Concat("\t", words);
				s = s.Replace("\"", "");
				writer.WriteLine(s);
			}
			writer.Close();
		}

		private static bool HasAnyDescription(IMatrixData data){
			for (int i = 0; i < data.ExpressionColumnCount; i++){
				if (data.ExpressionColumnDescriptions[i] != null && data.ExpressionColumnDescriptions[i].Length > 0){
					return true;
				}
			}
			for (int i = 0; i < data.CategoryColumnCount; i++){
				if (data.CategoryColumnDescriptions[i] != null && data.CategoryColumnDescriptions[i].Length > 0){
					return true;
				}
			}
			for (int i = 0; i < data.NumericColumnCount; i++){
				if (data.NumericColumnDescriptions[i] != null && data.NumericColumnDescriptions[i].Length > 0){
					return true;
				}
			}
			for (int i = 0; i < data.StringColumnCount; i++){
				if (data.StringColumnDescriptions[i] != null && data.StringColumnDescriptions[i].Length > 0){
					return true;
				}
			}
			for (int i = 0; i < data.MultiNumericColumnCount; i++){
				if (data.MultiNumericColumnDescriptions[i] != null && data.MultiNumericColumnDescriptions[i].Length > 0){
					return true;
				}
			}
			return false;
		}

		public Parameters GetParameters(IMatrixData matrixData, ref string errorString){
			return
				new Parameters(new Parameter[]{new FileParam("File name"){Filter = "Tab separated file (*.txt)|*.txt", Save = true}});
		}

		private const int maxlen = 30000;
		private static string Trunc(string s) { return s == null ? "" : s.Length <= maxlen ? s : s.Substring(0, maxlen); }
	}
}