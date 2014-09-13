using System;
using System.Drawing;
using BaseLib.Param;
using PerseusApi.Generic;
using PerseusApi.Matrix;
using PerseusPluginLib.Properties;

namespace PerseusPluginLib.Manual{
	public class SelectRowsManually : IMatrixAnalysis{
		public string Description { get { return ""; } }
		public bool HasButton { get { return true; } }
		public Bitmap DisplayImage { get { return Resources.hand; } }
		public string Heading { get { return null; } }
		public string Name { get { return "Select rows manually"; } }
		public bool IsActive { get { return true; } }
		public float DisplayRank { get { return 2; } }
		public string Url { get { return "http://141.61.102.17/perseus_doku/doku.php?id=perseus:activities:MatrixAnalysis:Misc:SelectRowsManually"; } }

		public int GetMaxThreads(Parameters parameters) {
			return 1;
		}

		public IAnalysisResult AnalyzeData(IMatrixData mdata, Parameters param, ProcessInfo processInfo) {
			return new SelectRowsManuallyResult(mdata);
		}

		public Parameters GetParameters(IMatrixData mdata, ref string errorString) {
			return new Parameters(new Parameter[] { });
		}

		public Tuple<IMatrixProcessing, Func<Parameters, IMatrixData, Parameters, string>>[] Replacements { get { return new Tuple<IMatrixProcessing, Func<Parameters, IMatrixData, Parameters, string>>[0]; } }
		public bool CanStartWithEmptyData { get { return false; } }
	}
}