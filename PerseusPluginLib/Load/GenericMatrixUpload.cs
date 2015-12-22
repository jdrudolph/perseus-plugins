using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using BaseLibS.Param;
using BaseLibS.Parse;
using BaseLibS.Util;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;
using PerseusApi.Utils;

namespace PerseusPluginLib.Load{
	public class GenericMatrixUpload : IMatrixUpload{
		public bool HasButton => true;
		public Bitmap DisplayImage => BaseLib.Properties.Resources.upload64;
		public string Name => "Generic matrix upload";
		public bool IsActive => true;
		public float DisplayRank => 0;
		public string[] HelpSupplTables => new string[0];
		public int NumSupplTables => 0;
		public string[] HelpDocuments => new string[0];
		public int NumDocuments => 0;
		public string Url => "http://coxdocs.org/doku.php?id=perseus:user:activities:MatrixUpload:GenericMatrixUpload";

		public string Description
			=>
				"Load data from a tab-separated file. The first row should contain the column names, also separated by tab characters. " +
				"All following rows contain the tab-separated values. Such a file can for instance be generated from an excel sheet by " +
				"using the export as a tab-separated .txt file.";

		public int GetMaxThreads(Parameters parameters){
			return 1;
		}

		public Parameters GetParameters(ref string errorString){
			return
				new Parameters(new Parameter[]{
					new PerseusLoadMatrixParam("File"){
						Filter = "Text (Tab delimited) (*.txt)|*.txt;*.txt.gz|CSV (Comma delimited) (*.csv)|*.csv;*.csv.gz",
						Help = "Please specify here the name of the file to be uploaded including its full path."
					}
				});
		}

		public void LoadData(IMatrixData mdata, Parameters parameters, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo){
			PerseusLoadMatrixParam par = (PerseusLoadMatrixParam) parameters.GetParam("File");
			string filename = par.Filename;
			if (string.IsNullOrEmpty(filename)){
				processInfo.ErrString = "Please specify a filename";
				return;
			}
			if (!File.Exists(filename)){
				processInfo.ErrString = "File '" + filename + "' does not exist.";
				return;
			}
			string ftl = filename.ToLower();
			bool csv = ftl.EndsWith(".csv") || ftl.EndsWith(".csv.gz");
			char separator = csv ? ',' : '\t';
			string[] colNames;
			Dictionary<string, string[]> annotationRows = new Dictionary<string, string[]>();
			try{
				colNames = TabSep.GetColumnNames(filename, PerseusUtils.commentPrefix, PerseusUtils.commentPrefixExceptions,
					annotationRows, separator);
			} catch (Exception){
				processInfo.ErrString = "Could not open the file '" + filename + "'. It is probably opened in another program.";
				return;
			}
			string origin = filename;
			int[] eInds = par.ExpressionColumnIndices;
			int[] cInds = par.CategoryColumnIndices;
			int[] nInds = par.NumericalColumnIndices;
			int[] tInds = par.TextColumnIndices;
			int[] mInds = par.MultiNumericalColumnIndices;
			int nrows = GetRowCount(filename);
			StreamReader reader = FileUtils.GetReader(filename);
			PerseusUtils.LoadMatrixData(annotationRows, eInds, cInds, nInds, tInds, mInds, processInfo, colNames, mdata, reader,
				filename, nrows, origin, separator, par.ShortenExpressionColumnNames);
			GC.Collect();
		}

		private static int GetRowCount(string filename){
			StreamReader reader = FileUtils.GetReader(filename);
			int count = PerseusUtils.GetRowCount(reader);
			reader.Close();
			return count;
		}
	}
}