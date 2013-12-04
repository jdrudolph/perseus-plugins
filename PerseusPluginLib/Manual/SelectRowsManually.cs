using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BaseLib.Param;
using BaseLib.Util;
using BaseLib.Wpf;
using PerseusApi.Generic;
using PerseusApi.Matrix;
using PerseusPluginLib.Properties;

namespace PerseusPluginLib.Manual{
	public class SelectRowsManually : IMatrixAnalysis{
		public string HelpDescription { get { return ""; } }
		public bool HasButton { get { return true; } }
		public BitmapSource DisplayImage { get { return WpfUtils.LoadBitmap(Resources.hand); } }
		public string Heading { get { return "Manual editing"; } }
		public string Name { get { return "Select rows manually"; } }
		public bool IsActive { get { return true; } }
		public float DisplayOrder { get { return 0; } }
		public DocumentType HelpDescriptionType { get { return DocumentType.PlainText; } }

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