using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using BaseLib.Param;
using BaseLib.Parse;
using BaseLibS.Mol;
using BaseLibS.Util;
using MsLib.Search;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;
using PerseusApi.Utils;
using PluginMzTab.Lib.Model;
using PluginMzTab.Plugin.Extended;
using PluginMzTab.Plugin.Param;
using PluginMzTab.Plugin.Utils;

namespace PluginMzTab.Plugin.MzTab{
	public class DefineExperiment : MzTabProcessing{
		private List<Instrument> instruments;
		public override string Name { get { return "Define Experiment"; } }
		public override bool IsActive { get { return true; } }
		public override bool HasButton { get { return false; } }
		public override Bitmap DisplayImage { get { return null; } }
		public override float DisplayRank { get { return 0; } }
		public override string Description { get { return null; } }
		public override int NumSupplTables { get { return 2; } }
		public override int NumDocuments { get { return 1; } }
		public override string HelpOutput { get { return null; } }
		public override string[] HelpSupplTables { get { return new[]{"spectra ref", "database ref"}; } }
		public override string[] HelpDocuments { get { return new[]{"Logger"}; } }
		public override int MinNumInput { get { return Tables.Length; } }
		public override int MaxNumInput { get { return Tables.Length; } }
		public override string Url { get { return "http://141.61.102.17/perseus_doku/doku.php?id=perseus:plugins:mztab:define_experiment"; } }
		public override string[] Tables { get { return new[]{Matrix.ExperimentalDesign, Matrix.Summary, Matrix.Parameters}; } }

		public override IMatrixData ProcessData(IMatrixData[] inputData, Parameters param, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo){
			TextWriter defaultOut = Console.Out;
			TextWriter defaultErr = Console.Error;
			try{
				if (documents == null){
					documents = new IDocumentData[NumDocuments];
					for (int i = 0; i < NumDocuments; i++){
						documents[i] = (IDocumentData) inputData[0].CreateNewInstance(DataType.Document);
					}
				}
				TextWriter logger = null;
				if (documents.Length > 0){
					logger = new StreamWriter(new DocumentStream(documents[0]));
					Console.SetOut(logger);
					Console.SetError(logger);
				}
				int nThreads = GetMaxThreads(param);
				IList<MsRunImpl> runs = new List<MsRunImpl>();
				var singleSub = param.GetParam(MetadataElement.MS_RUN.Name) as SingleChoiceWithSubParams;
				if (singleSub != null){
					var sub = singleSub.SubParams[singleSub.Value].GetAllParameters().FirstOrDefault() as MsRunParam;
					if (sub != null){
						if (sub.Value != null){
							foreach (MsRunImpl run in sub.Value){
								runs.Add(run);
							}
						}
					}
				}
				IList<StudyVariable> studyVariables = new List<StudyVariable>();
				singleSub = param.GetParam(MetadataElement.STUDY_VARIABLE.Name) as SingleChoiceWithSubParams;
				if (singleSub != null){
					var sub = singleSub.SubParams[singleSub.Value].GetAllParameters().FirstOrDefault() as StudyVariableParam;
					if (sub != null){
						if (sub.Value != null){
							foreach (StudyVariable variable in sub.Value){
								studyVariables.Add(variable);
							}
						}
					}
				}
				IList<Sample> samples = new List<Sample>();
				singleSub = param.GetParam(MetadataElement.SAMPLE.Name) as SingleChoiceWithSubParams;
				if (singleSub != null){
					var sub = singleSub.SubParams[singleSub.Value].GetAllParameters().FirstOrDefault() as SampleParam;
					if (sub != null){
						if (sub.Value != null){
							foreach (Sample sample in sub.Value){
								samples.Add(sample);
							}
						}
					}
				}
				IList<Assay> assays = new List<Assay>();
				singleSub = param.GetParam(MetadataElement.ASSAY.Name) as SingleChoiceWithSubParams;
				if (singleSub != null){
					var sub = singleSub.SubParams[singleSub.Value].GetAllParameters().FirstOrDefault() as AssayParam;
					if (sub != null){
						if (sub.Value != null){
							foreach (Assay assay in sub.Value){
								assays.Add(assay);
							}
						}
					}
				}
				IList<Database> databases = new List<Database>();
				singleSub = param.GetParam("database") as SingleChoiceWithSubParams;
				if (singleSub != null){
					var sub = singleSub.SubParams[singleSub.Value].GetAllParameters().FirstOrDefault() as DatabaseParam;
					if (sub != null && sub.Value != null){
						foreach (Database db in sub.Value){
							databases.Add(db);
						}
					}
				}
				var output = (IMatrixData) inputData[0].CreateNewInstance(DataType.Matrix);
				var columnnames = new List<string>{
					MetadataElement.STUDY_VARIABLE.Name,
					MetadataElement.ASSAY.Name,
					MetadataElement.MS_RUN.Name,
					MetadataElement.SAMPLE.Name,
					MetadataElement.INSTRUMENT.Name
				};
				var matrix = new List<string[]>();
				for (int i = 0; i < columnnames.Count; i++){
					matrix.Add(new string[assays.Count]);
				}
				for (int i = 0; i < assays.Count; i++){
					Assay assay = assays[i];
					MsRunImpl runImpl = runs.FirstOrDefault(x => x.Id.Equals(assay.MsRun.Id));
					Instrument instrument = instruments.FirstOrDefault(x => x.Id.Equals(assay.MsRun.Id));
					if (runImpl == null){
						continue;
					}
					StudyVariable studyVariable = i < studyVariables.Count ? studyVariables[i] : null;
					Sample sample = i < samples.Count ? samples[i] : null;
					foreach (StudyVariable s in studyVariables){
						if (s.AssayMap.ContainsKey(assay.Id)){
							studyVariable = s;
							try{
								int sampleId = studyVariable.SampleMap.FirstOrDefault().Key;
								sample = samples.FirstOrDefault(x => x.Id.Equals(sampleId));
							} catch (Exception){
								Console.Error.WriteLine("Can not find sample");
							}
							break;
						}
					}
					AddRow(matrix, columnnames, i, runImpl, assay, sample, studyVariable, instrument);
				}
				output.Name = Matrix.Experiment;
				output.ColumnNames = new List<string>();
				output.Values = new float[assays.Count,columnnames.Count];
				output.SetAnnotationColumns(columnnames, matrix, new List<string>(), new List<string[][]>(), new List<string>(),
					new List<double[]>(), new List<string>(), new List<double[][]>());
				output.ClearAnnotationRows();
				IList<IMatrixData> supplement = new List<IMatrixData>();
				try{
					IList<MsRunImpl> aplfiles = runs.Where(x => x.Location != null && x.Location.Value.EndsWith(".apl")).ToList();
					IMatrixData temp = ProcessAplFiles(processInfo, nThreads, aplfiles, inputData[0]);
					if (temp != null){
						supplement.Add(temp);
					}
				} catch (Exception e){
					throw new Exception("Could not parse spectra file(s)! " + e.Message + "\n" + e.StackTrace);
				}
				try{
					IMatrixData temp = ProcessDbFiles(processInfo, databases.Count < nThreads ? 1 : nThreads, databases, inputData[0]);
					if (temp != null){
						supplement.Add(temp);
					}
				} catch (Exception e){
					throw new Exception("Could not parse database file(s)! " + e.Message + "\n" + e.StackTrace);
				}
				if (logger != null){
					logger.Dispose();
				}
				supplTables = supplement.ToArray();
				processInfo.Status("Define Experiment: DONE!");
				processInfo.Progress(100);
				return output;
			} catch (Exception e){
				string msg = "Process aborted! " + e.Message;
				MessageBox.Show(msg);
				processInfo.Status(msg);
			} finally{
				Console.SetOut(defaultOut);
				Console.SetError(defaultErr);
			}
			return null;
		}

		private IMatrixData ProcessAplFiles(ProcessInfo processInfo, int nThreads, IList<MsRunImpl> aplfiles,
			IMatrixData template){
			string tempFile = Path.Combine(FileUtils.GetTempFolder(), "spectraref.txt");
			if (File.Exists(tempFile)){
				File.Delete(tempFile);
			}
			IMatrixData matrix;
			StreamWriter writer = null;
			try{
				var enums = new Enum[]{
					spectraRef.raw_file, spectraRef.charge, spectraRef.scan_number, spectraRef.location, spectraRef.format,
					spectraRef.id_format, spectraRef.fragmentation, spectraRef.mz, spectraRef.index
				};
				IList<string> header = enums.Select(Constants.GetPattern).ToList();
				if (aplfiles == null || aplfiles.Count == 0){
					return null;
				}
				int nTasks = aplfiles.Count;
				processInfo.Progress(0);
				processInfo.Status(string.Format("Read Andromeda peaklist files [{0}|{1}]", 0, nTasks));
				writer = new StreamWriter(tempFile);
				writer.WriteLine(StringUtils.Concat("\t", header));
				writer.WriteLine("#!{Type}" + StringUtils.Concat("\t", header.Select(x => "T")));
				var distr = new ThreadDistributor(nThreads, nTasks,
					x =>
						ParseAplFile(aplfiles[x], writer, string.Format("Read Andromeda peaklist files [{0}|{1}]", x + 1, nTasks),
							(x + 1)*100/nTasks, processInfo));
				distr.Start();
				processInfo.Status("Close all files");
				writer.Close();
				writer.Dispose();
				writer = null;
				processInfo.Progress(0);
				processInfo.Status("Create SpectraRef matrix");
				matrix = (IMatrixData) template.CreateNewInstance(DataType.Matrix);
				LoadData(matrix, tempFile, processInfo);
			} catch (Exception ex){
				throw ex;
			} finally{
				if (writer != null){
					writer.Close();
				}
				if (File.Exists(tempFile)){
					File.Delete(tempFile);
				}
			}
			return matrix;
		}

		private IMatrixData ProcessDbFiles(ProcessInfo processInfo, int nThreads, IList<Database> databases,
			IMatrixData template){
			string tempFile = Path.Combine(FileUtils.GetTempFolder(), "databaseref.txt");
			IMatrixData matrix;
			StreamWriter writer = null;
			try{
				processInfo.Progress(0);
				processInfo.Status(string.Format("Read database files [{0}|{1}]", 0, "?"));
				var enums = new Enum[]{
					databaseRef.file, databaseRef.source, databaseRef.specie, databaseRef.taxid, databaseRef.version,
					databaseRef.identifier
				};
				IList<string> header = enums.Select(Constants.GetPattern).ToList();
				if (databases == null || databases.Count == 0){
					return null;
				}
				writer = new StreamWriter(tempFile);
				int nTasks = databases.Count;
				writer.WriteLine(StringUtils.Concat("\t", header));
				writer.WriteLine("#!{Type}" + StringUtils.Concat("\t", header.Select(x => "T")));
				var distr = new ThreadDistributor(nThreads, nTasks,
					x =>
						ParseDatabase(writer, databases[x], string.Format("Read database files [{0}|{1}]", x + 1, nTasks),
							(x + 1)*100/nTasks, processInfo));
				distr.Start();
				processInfo.Status("Close all files");
				writer.Close();
				writer.Dispose();
				writer = null;
				processInfo.Progress(0);
				processInfo.Status("Create DatabaseRef Matrix");
				matrix = (IMatrixData) template.CreateNewInstance(DataType.Matrix);
				LoadData(matrix, tempFile, processInfo);
			} catch (Exception ex){
				throw ex;
			} finally{
				if (writer != null){
					writer.Close();
				}
				if (File.Exists(tempFile)){
					File.Delete(tempFile);
				}
			}
			return matrix;
		}

		private void LoadData(IMatrixData mdata, string filename, ProcessInfo processInfo){
			TextReader reader = null;
			try{
				bool csv = filename.ToLower().EndsWith(".csv");
				char separator = csv ? ',' : '\t';
				var annotationRows = new Dictionary<string, string[]>();
				string[] colNames = TabSep.GetColumnNames(filename, PerseusUtils.commentPrefix, PerseusUtils.commentPrefixExceptions,
					annotationRows, separator);
				reader = new StreamReader(filename);
				int nrows = TabSep.GetRowCount(filename, 0, PerseusUtils.commentPrefix, PerseusUtils.commentPrefixExceptions);
				string origin = filename;
				int ncols = colNames.Length;
				var tInds = new int[ncols];
				for (int i = 0; i < tInds.Length; i++){
					tInds[i] = i;
				}
				PerseusUtils.LoadMatrixData(annotationRows, new int[0], new int[0], new int[0], tInds, new int[0], processInfo,
					colNames, mdata, reader, nrows, origin, separator, false);
				GC.Collect();
			} catch (Exception e){
				processInfo.ErrString = e.Message;
			} finally{
				if (reader != null){
					reader.Close();
					reader.Dispose();
				}
			}
		}

		private static void ParseAplFile(MsRunImpl aplfile, StreamWriter writer, string status, int progress,
			ProcessInfo processInfo){
			lock (processInfo){
				processInfo.Progress(progress);
				processInfo.Status(status);
			}
			string file = aplfile.Location.Value;
			string form = aplfile.Format == null ? "" : aplfile.Format.Name;
			string idform = aplfile.IdFormat == null ? "" : aplfile.IdFormat.Name;
			int m = 0;
			var regex = new Regex("Raw[f|F]ile: (.*) Index: ([0-9]+)");
			var parser = new AplParser(delegate(AplEntry entry){
				if (regex.IsMatch(entry.Title)){
					string rawfile = regex.Match(entry.Title).Groups[1].Value;
					string scannumber = regex.Match(entry.Title).Groups[2].Value;
					m++;
					var items = new object[]{
						rawfile, entry.PrecursorCharge, scannumber, file, form, idform, entry.Fragmentation, entry.Mz,
						m.ToString(CultureInfo.InvariantCulture)
					};
					lock (writer){
						writer.WriteLine(StringUtils.Concat("\t", items));
					}
				}
			});
			parser.Parse(file);
			lock (writer){
				writer.Flush();
			}
		}

		private static void ParseDatabase(StreamWriter writer, Database db, string status, int progress,
			ProcessInfo processInfo){
			if (db.File == null || !File.Exists(db.File)){
				return;
			}
			var reader = new StreamReader(db.File);
			string line;
			var regex = new Regex(db.SearchExpression);
			while ((line = reader.ReadLine()) != null){
				if (line.StartsWith(">")){
					string identifier = regex.Match(line).Groups[1].Value;
					var items = new object[]
					{db.File, db.Source, db.Species, db.Taxid, db.Version, db.Prefix == null ? identifier : db.Prefix + identifier};
					lock (writer){
						writer.WriteLine(StringUtils.Concat("\t", items));
					}
				}
			}
			reader.Close();
			lock (processInfo){
				processInfo.Progress(progress);
				processInfo.Status(status);
			}
		}

		private string GetParamListString(IList<Lib.Model.Param> param){
			if (param == null){
				return null;
			}
			if (param.Count == 0){
				return null;
			}
			Lib.Model.Param p = param.FirstOrDefault();
			if (p == null){
				return null;
			}
			return p.Name;
		}

		private void AddRow(List<string[]> matrix, List<string> columnnames, int row, MsRunImpl runImpl, Assay assay,
			Sample sample, StudyVariable studyVariable, Instrument instrument){
			string value = runImpl == null
				? ""
				: string.Format(@"{0} <{1};{2};{3};{4}>", runImpl.Description, runImpl.FilePath,
					runImpl.Format == null ? "" : runImpl.Format.Name, runImpl.IdFormat == null ? "" : runImpl.IdFormat.Name,
					runImpl.FragmentationMethod == null ? "" : runImpl.FragmentationMethod.Name);
			matrix[columnnames.IndexOf(MetadataElement.MS_RUN.Name)][row] = value;
			value = assay == null
				? ""
				: string.Format(@"{0} <{1}>", assay.QuantificationReagent.Name,
					StringUtils.Concat(";", ConvertToString(assay.QuantificationModMap)));
			matrix[columnnames.IndexOf(MetadataElement.ASSAY.Name)][row] = value;
			value = sample == null
				? ""
				: string.Format(@"{0} <{1};{2};{3};{4}>", sample.Description, GetParamListString(sample.SpeciesList),
					GetParamListString(sample.TissueList), GetParamListString(sample.CellTypeList),
					GetParamListString(sample.DiseaseList));
			matrix[columnnames.IndexOf(MetadataElement.SAMPLE.Name)][row] = value;
			matrix[columnnames.IndexOf(MetadataElement.STUDY_VARIABLE.Name)][row] = studyVariable == null
				? ""
				: studyVariable.Description;
			matrix[columnnames.IndexOf(MetadataElement.INSTRUMENT.Name)][row] = instrument == null
				? ""
				: string.Format(@"{0} <{1};{2};{3}>", instrument.Name == null ? "" : instrument.Name.Name,
					instrument.Analyzer == null ? "" : instrument.Analyzer.Name,
					instrument.Detector == null ? "" : instrument.Detector.Name,
					instrument.Source == null ? "" : instrument.Source.Name);
		}

		private IList<string> ConvertToString(SortedDictionary<int, AssayQuantificationMod> map){
			IList<string> result = new List<string>();
			if (map != null){
				foreach (AssayQuantificationMod value in map.Values){
					if (value != null && value.Param != null && value.Param.Name != null){
						result.Add(value.Param.Name);
					}
				}
			}
			return result;
		}

		public override Parameters GetParameters(IMatrixData[] inputData, ref string errString){
			ValidateParameters(inputData, ref errString);
			IList<Parameter> list = new List<Parameter>();
			var runs = new List<MsRunImpl>();
			var studyVariables = new List<StudyVariable>();
			var samples = new List<Sample>();
			var assays = new List<Assay>();
			instruments = new List<Instrument>();
			IMatrixData summary = GetMatrixData(Matrix.Summary, inputData);
			IMatrixData experimentalDesign = GetMatrixData(Matrix.ExperimentalDesign, inputData);
			IMatrixData parameters = GetMatrixData(Matrix.Parameters, inputData);
			Dictionary<string, string> paramDict = MzTabMatrixUtils.ConvertToParamDict(parameters);
			GetExperminetValues(summary, experimentalDesign, null, null, ref runs, ref studyVariables, ref assays, ref samples,
				ref instruments);
			IList<Database> databases = new List<Database>();
			var fastas = new List<string>{"contaminants.fasta"};
			if (paramDict.ContainsKey("Fasta file")){
				fastas.AddRange(paramDict["Fasta file"].Split(new[]{";", ","}, StringSplitOptions.RemoveEmptyEntries));
			}
			for (int i = 0; i < fastas.Count; i++){
				string fasta = fastas[i];
				string prefix = i == 0 ? "CON__" : null;
				try{
					string filename = Path.GetFileName(fasta);
					SequenceDatabase db = BaseLibS.Mol.Tables.Databases[filename];
					databases.Add(new Database(File.Exists(fasta) ? fasta : "", "", prefix, db.Source, db.Species, db.Taxid,
						db.SearchExpression){Prefix = prefix});
				} catch (Exception){
					databases.Add(new Database(File.Exists(fasta) ? fasta : "", "", prefix));
					Console.Error.WriteLine("The selected database was not specified in ./conf/database.xml");
				}
			}
			AddDatabaseParameters(list, databases, null);
			AddMsRunParameters(list, runs, null, true);
			AddAssayParameters(list, assays, null, true);
			AddSampleParameters(list, samples, null, true);
			AddStudyVariableParameters(list, studyVariables, null, true);
			return new Parameters(list);
		}

		private void AddDatabaseParameters(IList<Parameter> list, IList<Database> databases, IList<ParameterDescription> help){
			string name = "database";
			string desc = null;
			name = CheckIfDescriptionExists(help, name, ref desc, null);
			Database[] array = databases.Any() ? databases.ToArray() : null;
			var group = new SingleChoiceWithSubParams(name){Help = desc, ParamNameWidth = 0, TotalWidth = 800};
			int count = array == null ? 3 : Math.Max(array.Length + 1, 3);
			group.Values = new string[count];
			group.SubParams = new Parameters[count];
			for (int i = 0; i < count; i++){
				int n = i + 1;
				group.Values[i] = n.ToString(CultureInfo.InvariantCulture);
				group.SubParams[i] =
					new Parameters(
						new DatabaseParam(array != null && array.Length >= n ? ArrayUtils.SubArray(array, n) : new Database[n]));
			}
			group.Value = 0;
			if (array != null && group.SubParams.Count >= array.Length){
				group.Value = array.Length - 1;
			}
			list.Add(group);
		}
	}
}