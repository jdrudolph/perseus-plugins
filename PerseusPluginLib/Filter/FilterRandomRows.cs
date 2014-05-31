using System;
using System.Drawing;
using BaseLib.Num;
using BaseLib.Param;
using BaseLib.Util;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;
using PerseusPluginLib.Utils;

namespace PerseusPluginLib.Filter{
	public class FilterRandomRows : IMatrixProcessing{
		public bool HasButton { get { return false; } }
		public Bitmap DisplayImage { get { return null; } }
		public string Description { get { return "A given number of rows is kept based on random decisions."; } }
		public string HelpOutput { get { return "The filtered matrix."; } }
		public string[] HelpSupplTables { get { return new string[0]; } }
		public int NumSupplTables { get { return 0; } }
		public string Name { get { return "Filter rows based on random sampling"; } }
		public string Heading { get { return "Filter rows"; } }
		public bool IsActive { get { return true; } }
		public float DisplayRank { get { return 10; } }
		public string[] HelpDocuments { get { return new string[0]; } }
		public int NumDocuments { get { return 0; } }
		public int GetMaxThreads(Parameters parameters) { return 1; }
		public string Url { get { return null; } }

		public Parameters GetParameters(IMatrixData mdata, ref string errorString){
			return
				new Parameters(new Parameter[]
				{new IntParam("Number of rows", mdata.RowCount), PerseusPluginUtils.GetFilterModeParam(true)});
		}

		public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo){
			int nrows = param.GetIntParam("Number of rows").Value;
			nrows = Math.Min(nrows, mdata.RowCount);
			Random2 rand = new Random2();
			int[] rows = ArrayUtils.SubArray(rand.NextPermutation(mdata.RowCount), nrows);
			PerseusPluginUtils.FilterRows(mdata, param, rows);
		}
	}
}