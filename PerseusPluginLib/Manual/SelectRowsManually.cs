using System;
using System.Drawing;
using BasicLib.ParamWf;
using BasicLib.Util;
using PerseusApi.Generic;
using PerseusApi.Matrix;
using PerseusPluginLib.Properties;

namespace PerseusPluginLib.Manual{
	public class SelectRowsManually : IMatrixAnalysis{
		public string HelpDescription { get { return ""; } }
		public bool HasButton { get { return true; } }
		public Image ButtonImage { get { return Resources.hand; } }
		public string Heading { get { return "Manual editing"; } }
		public string Name { get { return "Select rows manually"; } }
		public bool IsActive { get { return true; } }
		public float DisplayOrder { get { return 0; } }
		public DocumentType HelpDescriptionType { get { return DocumentType.PlainText; } }

		public int GetMaxThreads(ParametersWf parameters) {
			return 1;
		}

		public IAnalysisResult AnalyzeData(IMatrixData mdata, ParametersWf param, ProcessInfo processInfo) {
			return new SelectRowsManuallyResult(mdata);
		}

		public ParametersWf GetParameters(IMatrixData mdata, ref string errorString) {
			return new ParametersWf(new ParameterWf[] { });
		}

		public Tuple<IMatrixProcessing, Func<ParametersWf, IMatrixData, ParametersWf, string>>[] Replacements { get { return new Tuple<IMatrixProcessing, Func<ParametersWf, IMatrixData, ParametersWf, string>>[0]; } }
		public bool CanStartWithEmptyData { get { return false; } }
	}
}