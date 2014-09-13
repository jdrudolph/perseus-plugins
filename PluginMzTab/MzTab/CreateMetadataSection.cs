using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Security.Policy;
using System.Text.RegularExpressions;
using BaseLib.Param;
using BaseLib.Util;
using BaseLibS.Util;
using MzTabLibrary.model;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;
using PluginMzTab.extended;
using PluginMzTab.param;
using PluginMzTab.utils;
using Parameters = BaseLib.Param.Parameters;

namespace PluginMzTab.mztab{
    public class CreateMetadataSection : CreateSection {
        
        public override string Name { get { return "Create Metadata section"; } }
        public override float DisplayRank { get { return 1; } }
        public override bool IsActive { get { return true; } }
        public override bool HasButton { get { return false; } }
		public override Bitmap DisplayImage { get { return null; } }
        public override string Description { get { return helpDescription; } }
        public override string HelpOutput { get { return helpOutput; } }
        public override string[] HelpSupplTables { get { return null; } }
        public override int NumSupplTables { get { return 0; } }
        public override string[] HelpDocuments { get { return new []{"Output"}; } }
        public override int NumDocuments { get { return 1; } }

        public override int MinNumInput { get { return 3; } }
        public override int MaxNumInput { get { return 3; } }

        public override string[] Tables { get { return new[] { Matrix.Experiment, Matrix.Summary, Matrix.Parameters}; } }
        
		public override IMatrixData ProcessData(IMatrixData[] inputData, Parameters param, ref IMatrixData[] supplTables, ref IDocumentData[] documents, ProcessInfo processInfo) {
            Metadata mtd = new Metadata();

            string name = string.Format("{0}{1}{2}", MetadataElement.MZTAB, MZTabConstants.MINUS, MetadataProperty.MZTAB_VERSION);
            StringParam stringParam = FindParam(param, name) as StringParam;
            if (stringParam != null && !string.IsNullOrEmpty(stringParam.Value)){
                mtd.MZTabVersion = stringParam.Value;
            }

            name = string.Format("{0}{1}{2}", MetadataElement.MZTAB, MZTabConstants.MINUS, MetadataProperty.MZTAB_MODE);
			SingleChoiceParam single = FindParam(param, name) as SingleChoiceParam;
            if (single != null && !string.IsNullOrEmpty(single.SelectedValue)){
                MzTabMode mode;
                if (Enum.TryParse(single.SelectedValue, true, out mode)){
                    mtd.MzTabMode = mode;
                }
            }

            name = string.Format("{0}{1}{2}", MetadataElement.MZTAB, MZTabConstants.MINUS, MetadataProperty.MZTAB_TYPE);
			single = FindParam(param, name) as SingleChoiceParam;
            if (single != null && !string.IsNullOrEmpty(single.SelectedValue)){
                MzTabType type;
                if (Enum.TryParse(single.SelectedValue, true, out type)){
                    mtd.MzTabType = type;
                }
            }

            name = string.Format("{0}{1}{2}", MetadataElement.MZTAB, MZTabConstants.MINUS, MetadataProperty.MZTAB_ID);
			stringParam = FindParam(param, name) as StringParam;
            if (stringParam != null && !string.IsNullOrEmpty(stringParam.Value)){
                mtd.MZTabID = stringParam.Value;
            }

			stringParam = FindParam(param, MetadataElement.TITLE.Name) as StringParam;
            if (stringParam != null && !string.IsNullOrEmpty(stringParam.Value)){
                mtd.SetTitle(stringParam.Value);
            }

			stringParam = FindParam(param, MetadataElement.DESCRIPTION.Name) as StringParam;
            if (stringParam != null && !string.IsNullOrEmpty(stringParam.Value)){
                mtd.SetDescription(stringParam.Value);
            }

			SingleChoiceWithSubParams singleSub = FindParam(param, MetadataElement.SAMPLE_PROCESSING.Name) as SingleChoiceWithSubParams;
            if (singleSub != null){
                SampleProcessingParam sub = singleSub.SubParams[singleSub.Value].GetAllParameters().FirstOrDefault() as SampleProcessingParam;
                if (sub != null){
                    if (sub.Value != null){
                        for (int j = 0; j < sub.Value.Length; j++){
                            mtd.SampleProcessingMap.Add(j + 1, sub.Value[j]);
                        }
                    }
                }
            }

			singleSub = FindParam(param, MetadataElement.INSTRUMENT.Name) as SingleChoiceWithSubParams;
            if (singleSub != null){
                InstrumentParam sub = singleSub.SubParams[singleSub.Value].GetAllParameters().FirstOrDefault() as InstrumentParam;
                if (sub != null){
                    if (sub.Value != null){
                        for (int j = 0; j < sub.Value.Length; j++){
                            mtd.InstrumentMap.Add(j + 1, sub.Value[j]);
                        }
                    }
                }
            }

			singleSub = FindParam(param, MetadataElement.SOFTWARE.Name) as SingleChoiceWithSubParams;
            if (singleSub != null){
                SoftwareParam sub = singleSub.SubParams[singleSub.Value].GetAllParameters().FirstOrDefault() as SoftwareParam;
                if (sub != null){
                    if (sub.Value != null){
                        for (int j = 0; j < sub.Value.Length; j++){
                            mtd.SoftwareMap.Add(j + 1, sub.Value[j]);
                        }
                    }
                }
            }

			MultiChoiceParam multi = FindParam(param, MetadataElement.FALSE_DISCOVERY_RATE.Name) as MultiChoiceParam;
            if (multi != null && multi.SelectedValues != null && multi.SelectedValues.Any()){
                foreach (string sel in multi.SelectedValues){
                    string[] items = sel.Split('=');
                    if (items.Length != 2){
                        continue;
                    }
                    string key = items[0];
                    string value = items[1];


                    IList<string> all;

                    if ((all = Constants.GetAll(parameters.protein_fdr)) != null && all.Any(x => key.StartsWith(x))){
                        key = "prot:global FDR";
                    }else if ((all = Constants.GetAll(parameters.psm_fdr)) != null && all.Any(x => key.StartsWith(x))){
                        key = "pep:global FDR";
                    }else if ((all = Constants.GetAll(parameters.site_fdr)) != null && all.Any(x => key.StartsWith(x))){
                        key = "site:global FDR";
                    }
                    Param p = cv.GetParam(key, "MS", value);
                    if (p != null){
                        mtd.FalseDiscoveryRate.Add(p);
                    }
                }
            }

			singleSub = FindParam(param, MetadataElement.PUBLICATION.Name) as SingleChoiceWithSubParams;
            if (singleSub != null){
                PublicationParam sub = singleSub.SubParams[singleSub.Value].GetAllParameters().FirstOrDefault() as PublicationParam;
                if (sub != null){
                    if (sub.Value != null){
                        for (int j = 0; j < sub.Value.Length; j++){
                            mtd.PublicationMap.Add(j + 1, sub.Value[j]);
                        }
                    }
                }
            }

			singleSub = FindParam(param, MetadataElement.CONTACT.Name) as SingleChoiceWithSubParams;
            if (singleSub != null){
                ContactParam sub = singleSub.SubParams[singleSub.Value].GetAllParameters().FirstOrDefault() as ContactParam;
                if (sub != null){
                    if (sub.Value != null){
                        for (int j = 0; j < sub.Value.Length; j++){
                            mtd.ContactMap.Add(j + 1, sub.Value[j]);
                        }
                    }
                }
            }

			singleSub = FindParam(param, MetadataElement.FIXED_MOD.Name) as SingleChoiceWithSubParams;
            if (singleSub != null){
                ModificationParam sub = singleSub.SubParams[singleSub.Value].GetAllParameters().FirstOrDefault() as ModificationParam;
                if (sub != null){
                    if (sub.Value != null){
                        for (int j = 0; j < sub.Value.Length; j++){
                            mtd.FixedModMap.Add(j + 1, (FixedMod)sub.Value[j]);
                        }
                    }
                }
            }

			singleSub = FindParam(param, MetadataElement.VARIABLE_MOD.Name) as SingleChoiceWithSubParams;
            if (singleSub != null){
                ModificationParam sub = singleSub.SubParams[singleSub.Value].GetAllParameters().FirstOrDefault() as ModificationParam;
                if (sub != null){
                    if (sub.Value != null){
                        for (int j = 0; j < sub.Value.Length; j++){
                            mtd.VariableModMap.Add(j + 1, (VariableMod)sub.Value[j]);
                        }
                    }
                }
            }

			single = FindParam(param, MetadataElement.QUANTIFICATION_METHOD.Name) as SingleChoiceParam;
            if (single != null && !string.IsNullOrEmpty(single.SelectedValue)){
                Param temp;
                mtd.QuantificationMethod = Param.TryParse(single.SelectedValue, out temp)
                                               ? temp
                                               : cv.GetParam(single.SelectedValue, "MS");
            }

            name = string.Format("{0}{1}{2}", MetadataElement.PROTEIN, MZTabConstants.MINUS, MetadataProperty.PROTEIN_QUANTIFICATION_UNIT);
			single = FindParam(param, name) as SingleChoiceParam;
            if (single != null && !string.IsNullOrEmpty(single.SelectedValue)){
                Param temp;
                mtd.ProteinQuantificationUnit = Param.TryParse(single.SelectedValue, out temp)
                                                    ? temp
                                                    : cv.GetParam(single.SelectedValue, "PRIDE");
            }

            name = string.Format("{0}{1}{2}", MetadataElement.PEPTIDE, MZTabConstants.MINUS, MetadataProperty.PROTEIN_QUANTIFICATION_UNIT);
			single = FindParam(param, name) as SingleChoiceParam;
            if (single != null && !string.IsNullOrEmpty(single.SelectedValue)){
                Param temp;
                mtd.PeptideQuantificationUnit = Param.TryParse(single.SelectedValue, out temp)
                                                    ? temp
                                                    : cv.GetParam(single.SelectedValue, "PRIDE");
            }

            Regex regex =new Regex("[A-Z]{1}:/");
            MsRunParam msrunParam = FindParam(param, MetadataElement.MS_RUN.Name) as MsRunParam;
            if (msrunParam != null){
                if (msrunParam.Value != null){
                    for (int j = 0; j < msrunParam.Value.Length; j++){
                        MsRunImpl runImpl = msrunParam.Value[j];
                        if (regex.IsMatch(runImpl.Location.Value)){
                            runImpl.Location = new Url("file:\\\\\\" + runImpl.Location.Value);
                        }
                        mtd.MsRunMap.Add(j + 1, runImpl);
                    }
                }
            }

            SampleParam sampleParam = FindParam(param, MetadataElement.SAMPLE.Name)   as SampleParam;
            if (sampleParam != null){
                if (sampleParam.Value != null){
                    for (int j = 0; j < sampleParam.Value.Length; j++){
                        mtd.SampleMap.Add(j + 1, sampleParam.Value[j]);
                    }
                }
            }

            AssayParam assayParam = FindParam(param, MetadataElement.ASSAY.Name) as AssayParam;
            if (assayParam != null){
                if (assayParam.Value != null){
                    for (int j = 0; j < assayParam.Value.Length; j++){
                        mtd.AssayMap.Add(j + 1, assayParam.Value[j]);
                    }
                }
            }

            StudyVariableParam studyVariableParam = FindParam(param, MetadataElement.STUDY_VARIABLE.Name) as StudyVariableParam;
            if (studyVariableParam != null){
                if (studyVariableParam.Value != null){
                    for (int j = 0; j < studyVariableParam.Value.Length; j++){
                        mtd.StudyVariableMap.Add(j + 1, studyVariableParam.Value[j]);
                    }
                }
            }

			multi = FindParam(param, MetadataElement.CV.Name) as MultiChoiceParam;
            if (multi != null && multi.SelectedValues != null){
                var headers = cv.Headers;
                foreach (string value in multi.SelectedValues){
                    var temp = headers.FirstOrDefault(x => x.FullName.Equals(value));
                    if (temp == null){
                        continue;
                    }
                    int id = mtd.CvMap.Count + 1;
                    mtd.CvMap.Add(id, new CV(id){FullName = temp.FullName, Label = temp.Label, Url = temp.Url, Version = temp.Version});
                }
            }
            
            string mtdString = mtd.ToString();
            string[] lines = mtdString.Split(new[]{"\n"}, StringSplitOptions.RemoveEmptyEntries);

            List<string> columnames = new List<string>{"MTH", "key", "value"};
            List<string[]> columns = columnames.Select(columname => new string[lines.Length]).ToList();

            for (int n = 0; n < lines.Length; n++){
                string[] items = lines[n].Split(new[]{"\t", "\n", "\r"}, StringSplitOptions.None);
                if (items.Length < 3){
                    continue;
                }
                columns[0][n] = items[0];
                columns[1][n] = items[1];
                columns[2][n] = items[2];
            }
            int nrows = lines.Length;
            int ncols = columnames.Count;
            IMatrixData data = (IMatrixData)inputData[0].CreateNewInstance(DataType.Matrix);
            data.SetData(Matrix.MetadataSection, new List<string>(), new float[nrows, ncols], columnames, columns,
                          new List<string>(), new List<string[][]>(), new List<string>(), new List<double[]>(),
                          new List<string>(), new List<double[][]>(), new List<string>(), new List<string[][]>(),
                          new List<string>(), new List<double[]>());

            if (true){
                CheckSection(new[] { data }, ref documents, data.RowCount, processInfo.Progress, processInfo.Status);
            }

            return data;
        }
      
        public Metadata CreateMetadataDefault(IMatrixData parameters, IMatrixData summary, IMatrixData experiment){
            Metadata mtd = new Metadata(new MZTabDescription(MzTabMode.Complete, MzTabType.Quantification));

            List<MsRunImpl> runs = null;
            List<StudyVariable> studyvariables = null;
            List<Assay> assays = null;
            List<Sample> samples = null;

            GetExperminetValues(summary, null, experiment, ref runs, ref studyvariables, ref assays, ref samples);

            foreach (var msRun in runs){
                mtd.MsRunMap.Add(msRun.Id, msRun);
            }

            foreach (var studyvariable in studyvariables){
                mtd.StudyVariableMap.Add(studyvariable.Id, studyvariable);
            }

            foreach (var sample in samples){
                mtd.SampleMap.Add(sample.Id, sample);
            }

            foreach (var assay in assays){
                mtd.AssayMap.Add(assay.Id, assay);
            }

            SplitList<Param> sampleProcessing = new SplitList<Param>{
                cv.GetParam("enzyme digestion", "SEP"),
                cv.GetParam("reversed-phase chromatography", "SEP")
            };
            //sampleProcessing.Add(_cv.GetParam("ion-exchange chromatography", "SEP"));

            mtd.SampleProcessingMap.Add(1, sampleProcessing);
            
            string[] orbitrapInstruments = new[]{"LTQ Orbitrap", "LTQ Orbitrap XL", "LTQ Orbitrap Velos", "LTQ Orbitrap Elite", "Q Exactive"};

            int index;
            if (summary != null && (index = Constants.GetColumnIndex(utils.summary.instrument, summary.StringColumnNames)) != -1){
                string[] instruments = summary.StringColumns[index];
                instruments = ArrayUtils.UniqueValues(instruments);
                for (int i = 0; i < instruments.Count(); i++){
                    if (string.IsNullOrEmpty(instruments[i])){
                        continue;
                    }

                    int id = i + 1;
                    Param par = cv.GetParam(instruments[i], "MS");
                    mtd.AddInstrumentName(id, par);
                    if (orbitrapInstruments.Contains(par.Name)){
                        mtd.AddInstrumentSource(id, cv.GetParam("electrospray ionization", "MS"));
                        mtd.AddInstrumentAnalyzer(id, cv.GetParam("orbitrap", "MS"));
                    }
                }
            }


            var paramDict = ConvertToParamDict(parameters);

            string key = Constants.GetColumnName(utils.parameters.fixedMod, paramDict.Keys.ToArray());
            if (key != null){
                string[] values = paramDict[key].Split(';');
                foreach (var mod in values.Select(x => BaseLib.Mol.Tables.Modifications[x])){
                    int id = mtd.FixedModMap.Count + 1;
                    mtd.AddFixedModParam(id, cv.GetModificationParam(mod));
                    mtd.AddFixedModPosition(id, mod.Position.ToString());
                    mtd.AddFixedModSite(id, StringUtils.Concat(", ", mod.GetSiteArray()));
                }
            }

            key = Constants.GetColumnName(utils.parameters.variableMod, paramDict.Keys.ToArray());
            if (key != null){
                string[] values = paramDict[key].Split(';');
				foreach (var mod in values.Select(x => BaseLib.Mol.Tables.Modifications[x]))
				{
                    int id = mtd.VariableModMap.Count + 1;
                    mtd.AddVariableModParam(id, cv.GetModificationParam(mod));
                    mtd.AddVariableModPosition(id, mod.Position.ToString());
                    mtd.AddVariableModSite(id, StringUtils.Concat(", ", mod.GetSiteArray()));
                }
            }

            string version = Constants.GetColumnName(utils.parameters.version, paramDict.Keys.ToArray());
            Software software = new Software(1) { Param = cv.GetParam("MaxQuant", "MS", GetParameterString(parameters, version, null)) };    
            mtd.SoftwareMap.Add(software.Id, software);

            software = new Software(2) { Param = cv.GetParam("Andromeda", "MS", GetParameterString(parameters, version, null)) };
            mtd.SoftwareMap.Add(software.Id, software);

            if (assays.Count > 0){
                var tmp = ArrayUtils.UniqueValues(assays.Select(x => x.QuantificationReagent.Name).ToArray());
                if (tmp.Length == 1 && tmp.First().Equals("Unlabeled sample")){
                    mtd.QuantificationMethod = cv.GetParam("label-free proteingroup level quantitation", "MS");                    
                }else if (tmp.Any(x => x.Contains("SILAC"))){
                    mtd.QuantificationMethod = cv.GetParam("SILAC quantitation analysis", "MS");
                }
            }            

            return mtd;
        }

        private static Dictionary<string, string> ConvertToParamDict(IMatrixData parameters){
            Dictionary<string, string> paramDict = new Dictionary<string, string>();
            if (parameters != null){
                int col1 = parameters.StringColumnNames.IndexOf("Parameter");
                int col2 = parameters.StringColumnNames.IndexOf("Value");
                for (int row = 0; row < parameters.RowCount; row++){
                    string key = parameters.StringColumns[col1][row];
                    string value = parameters.StringColumns[col2][row];
                    if (paramDict.ContainsKey(key)){
                        Console.Error.WriteLine("Key {0} is already in the Dictionary.", key);
                        continue;
                    }
                    paramDict.Add(key, value);
                }
            }
            return paramDict;
        }

        private string GetParameterString(IMatrixData parameters, string name, string separator){
            int index = parameters.StringColumnNames.IndexOf("Parameter");
            if (index == -1){
                return null;
            }

            int row = ArrayUtils.IndexOf(parameters.StringColumns[index], name);
            int col = parameters.StringColumnNames.IndexOf("Value");
            if (row == -1 || col == -1){
                return null;
            }

            string value = parameters.StringColumns[col][row];

            return separator == null
                       ? value
                       : StringUtils.Concat(separator, new[]{parameters.StringColumns[index][row], value});
        }

		public override Parameters GetParameters(IMatrixData[] inputData, ref string errString)
        {
            IMatrixData expDesign = GetMatrixData(Matrix.Experiment, inputData);
            IMatrixData summary = GetMatrixData(Matrix.Summary, inputData);
            IMatrixData parameters = GetMatrixData(Matrix.Parameters, inputData);

            Metadata mtd = CreateMetadataDefault(parameters, summary, expDesign);

            IList<ParameterDescription> help = ParameterDescription.Read();
            if (help != null)
            {
                help = help.Where(x => x.Section.Equals(Section.Metadata)).ToArray();
            }

			IList<Parameter> list = new List<Parameter>();

            var paramDict = ConvertToParamDict(parameters);

            AddMzParameters(list, help, mtd);

            AddTitleParameter(list, help, mtd);

            AddDescriptionParameter(list, help, mtd);

            AddSampleProcessingParameters(list, help, new List<SplitList<Param>>(mtd.SampleProcessingMap.Values));

            AddInstrumentParameters(list, new List<Instrument>(mtd.InstrumentMap.Values), help);

            AddSoftware(list, new List<Software>(mtd.SoftwareMap.Values), paramDict, help);

            AddFDRParameters(list, paramDict, help);

            AddPublicationParameters(list, new List<Publication>(mtd.PublicationMap.Values), help);

            AddContactParameters(list, new List<Contact>(mtd.ContactMap.Values), help);

            //URI is missing

            AddModificationParameters(list, new List<Mod>(mtd.FixedModMap.Values), MetadataElement.FIXED_MOD.Name, help);

            AddModificationParameters(list, new List<Mod>(mtd.VariableModMap.Values), MetadataElement.VARIABLE_MOD.Name, help);

            AddQuantificationParameters(list, mtd.QuantificationMethod, mtd.ProteinQuantificationUnit, mtd.PeptideQuantificationUnit, help);

            //Custom missing

            AddSampleParameters(list, new List<Sample>(mtd.SampleMap.Values), help, false);

            AddStudyVariableParameters(list, new List<StudyVariable>(mtd.StudyVariableMap.Values), help, false);

            AddAssayParameters(list, new List<Assay>(mtd.AssayMap.Values), help, false);

            AddMsRunParameters(list, new List<MsRunImpl>(mtd.MsRunMap.Values.Select(x => x as MsRunImpl)), help, false);

            AddCVParameters(list, new List<CV>(mtd.CvMap.Values), help);
            
			return new Parameters(list);
        }

		private void AddMzParameters(IList<Parameter> list, IList<ParameterDescription> help, Metadata mtd) {
            string name = string.Format("{0}{1}{2}", MetadataElement.MZTAB, MZTabConstants.MINUS,
                                        MetadataProperty.MZTAB_VERSION);
            string desc = null;
            name = CheckIfDescriptionExists(help, name, ref desc, mtd);
            StringParam version = new StringParam(name, Constants.versions.Last()){
                Help = desc
            };
            list.Add(version);

            IList<string> values = Enum.GetNames(typeof (MzTabMode));
            name = string.Format("{0}{1}{2}", MetadataElement.MZTAB, MZTabConstants.MINUS, MetadataProperty.MZTAB_MODE);
            desc = null;
            name = CheckIfDescriptionExists(help, name, ref desc, mtd);
			SingleChoiceParam mode = new SingleChoiceParam(name) {
                Values = values,
                Value = values.IndexOf(mtd.TabDescription.MzTabMode.ToString()),
                Help = desc
            };
            list.Add(mode);

            values = Enum.GetNames(typeof (MzTabType));
            name = string.Format("{0}{1}{2}", MetadataElement.MZTAB, MZTabConstants.MINUS, MetadataProperty.MZTAB_TYPE);
            desc = null;
            name = CheckIfDescriptionExists(help, name, ref desc, mtd);
			SingleChoiceParam type = new SingleChoiceParam(name) {
                Values = values,
                Value = values.IndexOf(mtd.TabDescription.MzTabType.ToString()),
                Help = desc
            };
            list.Add(type);

            name = string.Format("{0}{1}{2}", MetadataElement.MZTAB, MZTabConstants.MINUS, MetadataProperty.MZTAB_ID);
            desc = null;
            name = CheckIfDescriptionExists(help, name, ref desc, mtd);
			StringParam id = new StringParam(name) { Value = mtd.TabDescription.Id, Help = desc };
            list.Add(id);
        }

		private void AddTitleParameter(IList<Parameter> list, IList<ParameterDescription> help, Metadata mtd) {
            string name = MetadataElement.TITLE.Name;
            string desc = null;
            name = CheckIfDescriptionExists(help, name, ref desc, mtd);
			StringParam id = new StringParam(name) { Value = mtd.Title, Help = desc };
            list.Add(id);
        }

		private void AddDescriptionParameter(IList<Parameter> list, IList<ParameterDescription> help, Metadata mtd) {
            string name = MetadataElement.DESCRIPTION.Name;
            string desc = null;
            name = CheckIfDescriptionExists(help, name, ref desc, mtd);
			StringParam id = new StringParam(name) { Value = mtd.Description, Help = desc };
            list.Add(id);
        }

		private void AddSampleProcessingParameters(IList<Parameter> list, IList<ParameterDescription> help, IList<SplitList<Param>> steps) {
            string name = MetadataElement.SAMPLE_PROCESSING.Name;
            string desc = null;
            name = CheckIfDescriptionExists(help, name, ref desc, null);

            SplitList<Param>[] array = steps.Any() ? steps.ToArray() : null;
			SingleChoiceWithSubParams group = new SingleChoiceWithSubParams(name) {
                Help = desc,
                ParamNameWidth = 0,
                TotalWidth = 800,
                Value = 0,
                Values = new[]{"1", "2", "3"},
                SubParams =
                    new[]{
                        new Parameters(new Parameter[]{
                            new SampleProcessingParam(
                                           array != null && array.Length >= 1
                                               ? ArrayUtils.SubArray(array, 1)
                                               : new SplitList<Param>[1], cv)
                        }),
                        new Parameters(new Parameter[]{
                            new SampleProcessingParam(array != null && array.Length >= 2
                                                          ? ArrayUtils.SubArray(array, 2)
                                                          : new SplitList<Param>[2], cv)
                        }),
                        new Parameters(new Parameter[]{
                            new SampleProcessingParam(array != null && array.Length >= 3
                                                          ? ArrayUtils.SubArray(array, 3)
                                                          : new SplitList<Param>[3], cv)
                        })
                    }
            };
            if (array != null && group.SubParams.Count >= array.Length){
                group.Value = array.Length - 1;
            }

            list.Add(group);
        }

		private void AddInstrumentParameters(IList<Parameter> list, IList<Instrument> instruments, IList<ParameterDescription> help) {
            string name = MetadataElement.INSTRUMENT.Name;
            string desc = null;
            name = CheckIfDescriptionExists(help, name, ref desc, null);

            Instrument[] array = instruments.Any() ? instruments.ToArray() : null;
			SingleChoiceWithSubParams group = new SingleChoiceWithSubParams(name) {
                Help = desc,
                ParamNameWidth = 0,
                TotalWidth = 800,
                Value = 0,
                Values = new[]{"1", "2", "3"},
                SubParams =
                    new[]{
                        new Parameters(new Parameter[]{
                            new InstrumentParam(array != null && array.Length >= 1
                                                    ? ArrayUtils.SubArray(array, 1)
                                                    : new Instrument[1], cv)
                        }),
                        new Parameters(new Parameter[]{
                            new InstrumentParam(array != null && array.Length >= 2
                                                    ? ArrayUtils.SubArray(array, 2)
                                                    : new Instrument[2], cv)
                        }),
                        new Parameters(new Parameter[]{
                            new InstrumentParam(array != null && array.Length >= 3
                                                    ? ArrayUtils.SubArray(array, 3)
                                                    : new Instrument[3], cv)
                        })
                    }
            };

            if (array != null && group.SubParams.Count >= array.Length){
                group.Value = array.Length - 1;
            }

            list.Add(group);
        }

		private void AddSoftware(IList<Parameter> list, List<Software> software, Dictionary<string, string> parameters, IList<ParameterDescription> help){
            string name = MetadataElement.SOFTWARE.Name;
            string desc = null;
            name = CheckIfDescriptionExists(help, name, ref desc, null);

            Software[] array = software.Any() ? software.ToArray() : null;
			SingleChoiceWithSubParams group = new SingleChoiceWithSubParams(name) {
                Help = desc,
                ParamNameWidth = 0,
                TotalWidth = 800,
                Value = 0,
                Values = new[]{"1", "2", "3"},
                SubParams = new Parameters[3]
            };

            group.SubParams[0] =
                new Parameters(new Parameter[]{
                    new SoftwareParam(
                                   array != null && array.Length >= 1 ? ArrayUtils.SubArray(array, 1) : new Software[1],
                                   cv, parameters)
                });
            group.SubParams[1] = new Parameters(new Parameter[]{
                new SoftwareParam(array != null && array.Length >= 2
                                      ? ArrayUtils.SubArray(array, 2)
                                      : new Software[2], cv, parameters)
            });
            group.SubParams[2] = new Parameters(new Parameter[]{
                new SoftwareParam(array != null && array.Length >= 3
                                      ? ArrayUtils.SubArray(array, 3)
                                      : new Software[3], cv, parameters)
            });

            if (array != null && group.SubParams.Count >= array.Length){
                group.Value = array.Length - 1;
            }

            list.Add(group);
        }

		private void AddFDRParameters(IList<Parameter> list, Dictionary<string, string> parameters, IList<ParameterDescription> help){
            string name = MetadataElement.FALSE_DISCOVERY_RATE.Name;
            string desc = null;
            name = CheckIfDescriptionExists(help, name, ref desc, null);
            
            IList<string> values = new List<string>();
            IList<int> selection = new List<int>();
            if (parameters != null){
                
                List<string> defaultValues = new List<string>();
                defaultValues.AddRange(Constants.GetAll(utils.parameters.protein_fdr));
                defaultValues.AddRange(Constants.GetAll(utils.parameters.psm_fdr));
                defaultValues.AddRange(Constants.GetAll(utils.parameters.site_fdr));

                int i = 0;
                foreach (var temp in parameters){
                    values.Add(string.Format("{0}={1}", temp.Key, temp.Value));
                    if (defaultValues.Contains(temp.Key)){
                        selection.Add(i);
                    }
                    i++;
                }
            }

            MultiChoiceParam multi = new MultiChoiceParam(name, selection.ToArray()) { Help = desc, Values = values };

            list.Add(multi);
        }

		private void AddPublicationParameters(IList<Parameter> list, IList<Publication> publications, IList<ParameterDescription> help){
            string name = MetadataElement.PUBLICATION.Name;
            string desc = null;
            name = CheckIfDescriptionExists(help, name, ref desc, null);

            Publication[] array = publications.Any() ? publications.ToArray() : null;
			SingleChoiceWithSubParams group = new SingleChoiceWithSubParams(name) {
                Help = desc,
                ParamNameWidth = 0,
                TotalWidth = 800
            };

            int count = array == null ? 3 : array.Length;
            group.Values = new string[count];
			group.SubParams = new Parameters[count];

            for (int i = 0; i < count; i++){
                int n = i + 1;
                group.Values[i] = n.ToString(CultureInfo.InvariantCulture);
				group.SubParams[i] = new Parameters(new PublicationParam(array != null && array.Length >= n
                                                                             ? ArrayUtils.SubArray(array, n)
                                                                             : new Publication[n], cv));
            }

            group.Value = 0;

            if (array != null && group.SubParams.Count >= array.Length){
                group.Value = array.Length - 1;
            }

            list.Add(group);
        }

		private void AddContactParameters(IList<Parameter> list, IList<Contact> contacts, IList<ParameterDescription> help){
            string name = MetadataElement.CONTACT.Name;
            string desc = null;
            name = CheckIfDescriptionExists(help, name, ref desc, null);

            Contact[] array = contacts.Any() ? contacts.ToArray() : null;
			SingleChoiceWithSubParams group = new SingleChoiceWithSubParams(name) {
                Help = desc,
                ParamNameWidth = 0,
                TotalWidth = 800
            };
            int count = array == null ? 3 : array.Length;
            group.Values = new string[count];
			group.SubParams = new Parameters[count];

            for (int i = 0; i < count; i++){
                int n = i + 1;
                group.Values[i] = n.ToString(CultureInfo.InvariantCulture);
				group.SubParams[i] = new Parameters(new ContactParam(array != null && array.Length >= n
                                                                         ? ArrayUtils.SubArray(array, n)
                                                                         : new Contact[n], cv));
            }

            group.Value = 0;

            if (array != null && group.SubParams.Count >= array.Length){
                group.Value = array.Length - 1;
            }

            list.Add(group);
        }

		private void AddModificationParameters(IList<Parameter> list, IList<Mod> modifications, string name, IList<ParameterDescription> help){
            string desc = null;
            name = CheckIfDescriptionExists(help, name, ref desc, null);

            Mod[] array = modifications.Any() ? modifications.ToArray() : null;
			SingleChoiceWithSubParams group = new SingleChoiceWithSubParams(name)
            {
                Help=desc,
                ParamNameWidth = 0,
                TotalWidth = 800
            };
            int count = array == null ? 3 : array.Length + 1;
            group.Values = new string[count];
			group.SubParams = new Parameters[count];

            for (int i = 0; i < count; i++)
            {
                int n = i + 1;
                group.Values[i] = n.ToString(CultureInfo.InvariantCulture);
				group.SubParams[i] = new Parameters(new ModificationParam(array != null && array.Length >= n
                                                                         ? ArrayUtils.SubArray(array, n)
                                                                         : new Mod[n], cv));
            }

            group.Value = 0;

            if (array != null && group.SubParams.Count >= array.Length)
            {
                group.Value = array.Length - 1;
            }

            list.Add(group);
        }

		private void AddQuantificationParameters(IList<Parameter> list, Param quantificationMethod, Param protUnit, Param pepUnit, IList<ParameterDescription> help){
            string name = MetadataElement.QUANTIFICATION_METHOD.Name;
            string desc = null;
            name = CheckIfDescriptionExists(help, name, ref desc, null);

            IList<string> values = cv.GetOnlyChildNamesOfTerm("MS:1001833", "MS");
            values.Add("");

            int index = -1;
            if (quantificationMethod != null){
                index = MzTabMatrixUtils.GetSelectedIndex(quantificationMethod.Name, values);
            }
			SingleChoiceParam method = new SingleChoiceParam(name, index) {
                Help = desc,
                Values = values
            };
            list.Add(method);

            values = cv.GetOnlyChildNamesOfTerm("PRIDE:0000392", "PRIDE");
            name = string.Format("{0}{1}{2}", MetadataElement.PROTEIN, MZTabConstants.MINUS, MetadataProperty.PROTEIN_QUANTIFICATION_UNIT);
            desc = null;
            name = CheckIfDescriptionExists(help, name, ref desc, null);
            index = MzTabMatrixUtils.GetSelectedIndex(protUnit == null ? cv.GetParam("Ratio", "PRIDE").Name : protUnit.Name, values);
			SingleChoiceParam protein_unit = new SingleChoiceParam(name, index) { Help = desc, Values = values };
            list.Add(protein_unit);

            name = string.Format("{0}{1}{2}", MetadataElement.PEPTIDE, MZTabConstants.MINUS, MetadataProperty.PEPTIDE_QUANTIFICATION_UNIT);
            desc = null;
            name = CheckIfDescriptionExists(help, name, ref desc, null);
            index = MzTabMatrixUtils.GetSelectedIndex(pepUnit == null ? cv.GetParam("Ratio", "PRIDE").Name : pepUnit.Name, values);
			SingleChoiceParam peptide_unit = new SingleChoiceParam(name, index) { Help = desc, Values = values };
            list.Add(peptide_unit);
        }

		private void AddCVParameters(IList<Parameter> list, List<CV> cvs, IList<ParameterDescription> help){
            if (cvs == null || cvs.Count == 0)
            {
                cvs = new List<CV>(cv.Headers);
            }

            string name = MetadataElement.CV.Name;
            string desc = null;
            name = CheckIfDescriptionExists(help, name, ref desc, null);

			MultiChoiceParam param = new MultiChoiceParam(name) {
                Values = cvs.Select(x => x.FullName ?? x.Label).ToList(), Help = desc
            };
            for (int i = 0; i < cvs.Count; i++){
                param.AddSelectedIndex(i);
            }
            list.Add(param);
        }

        private const string helpDescription = @"The metadata section can provide additional information about the dataset(s) reported in the mzTab file.";

        private readonly string helpOutput = ParameterDescription.GetText(DocumentType.PlainText, Section.Metadata);    
    }
}