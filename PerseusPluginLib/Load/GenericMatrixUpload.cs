using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using BaseLib.Param;
using BaseLib.Parse;
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
		public float DisplayOrder { get { return 0; } }
		public string HelpDescription{
			get{
				return
					"Load data from a tab-separated file. The first row should contain the column names, also separated by tab characters. " +
						"All following rows contain the tab-separated values. Such a file can for instance be generated from an excen sheet by " +
						"using the export as a tab-separated .txt file.";
			}
		}

		public int GetMaxThreads(Parameters parameters){
			return 1;
		}

		public Parameters GetParameters(ref string errorString){
			return
				new Parameters(new Parameter[]{
					new PerseusLoadMatrixParam("File"){
						Filter = "Text (Tab delimited) (*.txt)|*.txt|CSV (Comma delimited) (*.csv)|*.csv",
						Help = "Please specify here the name of the file to be uploaded including its full path."
					}
				});
		}

		public void LoadData(IMatrixData mdata, Parameters parameters, ProcessInfo processInfo){
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
			bool csv = filename.ToLower().EndsWith(".csv");
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
			TextReader reader = new StreamReader(filename);
			int nrows = TabSep.GetRowCount(filename, 0, PerseusUtils.commentPrefix, PerseusUtils.commentPrefixExceptions);
			string origin = filename;
			int[] eInds = par.ExpressionColumnIndices;
			int[] cInds = par.CategoryColumnIndices;
			int[] nInds = par.NumericalColumnIndices;
			int[] tInds = par.TextColumnIndices;
			int[] mInds = par.MultiNumericalColumnIndices;
			PerseusUtils.LoadMatrixData(annotationRows, eInds, cInds, nInds, tInds, mInds, processInfo, colNames, mdata, reader, nrows,
				origin, separator);
		}
	}
}