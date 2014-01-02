using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
			return
				new Parameters(new Parameter[]{
					new FileParam("File"){
						Filter = "Text (Tab delimited) (*.txt)|*.txt|CSV (Comma delimited) (*.csv)|*.csv|All files (*.*)|*.*",
						Help = "Please specify here the name of the file to be uploaded including its full path."
					}
				});
		}

		public void LoadData(IMatrixData mdata, Parameters parameters, ProcessInfo processInfo){
			string filename = parameters.GetFileParam("File").Value;
			List<string> lines = new List<string>();
			StreamReader reader = new StreamReader(filename);
			string line;
			while ((line = reader.ReadLine()) != null){
				lines.Add(line);
			}
			reader.Close();
			mdata.SetData("", "", new List<string>(), new List<string>(), new float[lines.Count,0], new bool[lines.Count,0],
				new float[lines.Count,0], "", true, new List<string>(new[]{"All data"}),
				new List<string>(new[]{"Complete file in one text column."}), new List<string[]>(new[]{lines.ToArray()}),
				new List<string>(), new List<string>(), new List<string[][]>(), new List<string>(), new List<string>(),
				new List<double[]>(), new List<string>(), new List<string>(), new List<double[][]>(), new List<string>(),
				new List<string>(), new List<string[][]>(), new List<string>(), new List<string>(), new List<double[]>());
		}
	}
}