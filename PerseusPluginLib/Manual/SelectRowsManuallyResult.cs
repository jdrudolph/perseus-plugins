using System;
using System.Windows;
using PerseusApi.Generic;
using PerseusApi.Matrix;

namespace PerseusPluginLib.Manual{
	[Serializable]
	public class SelectRowsManuallyResult : IAnalysisResult{
		private readonly IMatrixData mdata;

		public SelectRowsManuallyResult(IMatrixData mdata){
			this.mdata = mdata;
		}

		public UIElement CreateUiElement(Action<string> updateStatusLabel, Action<IData> newData){
			return new SelectRowsManuallyControl1(mdata, newData);
		}

		public string Heading { get { return "Select rows manually"; } }
	}
}