using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using BaseLib.Param;
using BaseLib.Parse;
using BaseLib.Util;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;
using PerseusApi.Utils;
using PerseusPluginLib.Properties;

namespace PerseusPluginLib.Load{
	public class GenericMatrixUpload : IMatrixUpload{
		public bool HasButton { get { return true; } }
		public Bitmap DisplayImage { get { return Resources.upload64; } }
		public string Name { get { return "Generic matrix upload"; } }
		public bool IsActive { get { return true; } }
		public float DisplayRank { get { return 0; } }
		public string[] HelpSupplTables { get { return new string[0]; } }
		public int NumSupplTables { get { return 0; } }
		public string[] HelpDocuments { get { return new string[0]; } }
		public int NumDocuments { get { return 0; } }
		public string Url { get { return "http://141.61.102.17/perseus_doku/doku.php?id=perseus:genericmatrixupload"; } }

		public string Description{
			get{
				return
					"Load data from a tab-separated file. The first row should contain the column names, also separated by tab characters. " +
						"All following rows contain the tab-separated values. Such a file can for instance be generated from an excen sheet by " +
						"using the export as a tab-separated .txt file.";
			}
		}

		public int GetMaxThreads(Parameters parameters) { return 1; }

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
			TextReader reader = FileUtils.GetReader(filename);
			int nrows = TabSep.GetRowCount(filename, 0, PerseusUtils.commentPrefix, PerseusUtils.commentPrefixExceptions);
			string origin = filename;
			int[] eInds = par.ExpressionColumnIndices;
			int[] cInds = par.CategoryColumnIndices;
			int[] nInds = par.NumericalColumnIndices;
			int[] tInds = par.TextColumnIndices;
			int[] mInds = par.MultiNumericalColumnIndices;
			PerseusUtils.LoadMatrixData(annotationRows, eInds, cInds, nInds, tInds, mInds, processInfo, colNames, mdata, reader,
				nrows, origin, separator);
			GC.Collect();
		}
	}
}