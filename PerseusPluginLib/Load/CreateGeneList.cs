using System.Collections.Generic;
using System.Drawing;
using System.IO;
using BaseLib.Param;
using BaseLib.Parse;
using BaseLib.Util;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;
using PerseusPluginLib.Properties;

namespace PerseusPluginLib.Load{
	public class CreateGeneList : IMatrixUpload{
		public bool HasButton { get { return true; } }
		public Bitmap DisplayImage { get { return Resources.list; } }
		public string Description { get { return "Start with a list of all protein-coding genes from an organism."; } }
		public string Name { get { return "Create gene list"; } }
		public bool IsActive { get { return true; } }
		public float DisplayRank { get { return 4; } }
		public string[] HelpSupplTables { get { return new string[0]; } }
		public int NumSupplTables { get { return 0; } }
		public string[] HelpDocuments { get { return new string[0]; } }
		public int NumDocuments { get { return 0; } }
		public string Url { get { return null; } }

		public int GetMaxThreads(Parameters parameters){
			return 1;
		}

		public void LoadData(IMatrixData matrixData, Parameters parameters, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo){
			int ind = parameters.GetSingleChoiceParam("Organism").Value;
			string filename = GetOrganismFiles()[ind];
			List<string> stringColnames = new List<string>(TabSep.GetColumnNames(filename, '\t'));
			List<string[]> stringCols = new List<string[]>();
			foreach (string t in stringColnames){
				string[] col = TabSep.GetColumn(t, filename, '\t');
				stringCols.Add(col);
			}
			matrixData.SetData("Gene list", new List<string>(), new float[stringCols[0].Length,0], stringColnames, stringCols,
				new List<string>(), new List<string[][]>(), new List<string>(), new List<double[]>(), new List<string>(),
				new List<double[][]>());
			matrixData.Origin = "Gene list";
		}

		private static string[] GetOrganismFiles(){
			string path = FileUtils.GetConfigPath() + "\\perseus\\genelists";
			string[] files = Directory.GetFiles(path);
			List<string> result = new List<string>();
			foreach (string s in files){
				if (s.EndsWith(".txt")){
					result.Add(s);
				}
			}
			return result.ToArray();
		}

		private static string[] GetOrganismNames(){
			string[] files = GetOrganismFiles();
			for (int i = 0; i < files.Length; i++){
				files[i] = files[i].Substring(files[i].LastIndexOf('\\') + 1);
				files[i] = files[i].Substring(0, files[i].Length - 4);
			}
			return files;
		}

		public Parameters GetParameters(ref string errorString){
			string[] organisms = GetOrganismNames();
			return
				new Parameters(new Parameter[]{
					new SingleChoiceParam("Organism"){
						Values = organisms,
						Help = "Select the organism for which the gene list should be created."
					}
				});
		}
	}
}