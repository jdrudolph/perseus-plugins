using System.Drawing;
using BaseLib.Param;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;

namespace PerseusPluginLib.Basic{
	public class CloneProcessing : IMatrixProcessing{
		public bool HasButton { get { return false; } }
		public Bitmap DisplayImage { get { return BaseLib.Properties.Resources.sheep; } }
		public string Description { get { return "A copy of the input matrix is generated."; } }
		public string HelpOutput { get { return "Same as input matrix."; } }
		public string[] HelpSupplTables { get { return new string[0]; } }
		public int NumSupplTables { get { return 0; } }
		public string Name { get { return "Clone"; } }
		public string Heading { get { return "Basic"; } }
		public bool IsActive { get { return true; } }
		public float DisplayRank { get { return 100; } }
		public string[] HelpDocuments { get { return new string[0]; } }
		public int NumDocuments { get { return 0; } }
		public int GetMaxThreads(Parameters parameters) { return 1; }
		public string Url { get { return "http://141.61.102.17/perseus_doku/doku.php?id=perseus:activities:MatrixProcessing:Basic:CloneProcessing"; } }

		public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo) { }

		public Parameters GetParameters(IMatrixData mdata, ref string errorString) { return new Parameters(new Parameter[]{}); }
	}
}