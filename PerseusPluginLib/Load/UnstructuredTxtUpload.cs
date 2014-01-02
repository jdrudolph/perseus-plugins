using System.Drawing;
using BaseLib.Param;
using PerseusApi.Generic;
using PerseusApi.Matrix;
using PerseusPluginLib.Properties;

namespace PerseusPluginLib.Load{
	public class UnstructuredTxtUpload : IMatrixUpload{
		public bool HasButton { get { return true; } }
		public Bitmap DisplayImage { get { return Resources.raw; } }
		public string Name { get { return "Raw upload"; } }
		public bool IsActive { get { return true; } }
		public float DisplayOrder { get { return 10; } }
		public string HelpDescription { get { return "Load all lines from a text file and put them into a single text column."; } }

		public int GetMaxThreads(Parameters parameters){
			return 1;
		}

		public Parameters GetParameters(ref string errString){
			return new Parameters();
		}

		public void LoadData(IMatrixData matrixData, Parameters parameters, ProcessInfo processInfo) {}
	}
}