using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text.RegularExpressions;
using BaseLib.Param;
using BaseLib.Util;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;
using PluginMzTab.Lib.Model;
using PluginMzTab.Plugin.Extended;
using PluginMzTab.Plugin.Param;

namespace PluginMzTab.Plugin.Utils{
    public abstract class MzTabProcessing : IMatrixMultiProcessing{
        public abstract string Name { get; }
        public abstract float DisplayRank { get; }
        public abstract bool IsActive { get; }
        public abstract bool HasButton { get; }
        public abstract Bitmap DisplayImage { get; }
        public abstract string Description { get; }
        public string Heading { get { return "MzTab"; } }
        public abstract string HelpOutput { get; }
        public abstract string[] HelpSupplTables { get; }
        public abstract int NumSupplTables { get; }
        public abstract string[] HelpDocuments { get; }
        public abstract int NumDocuments { get; }
        public abstract int MinNumInput { get; }
        public abstract int MaxNumInput { get; }

        public abstract string Url { get; }

        public abstract string[] Tables { get; }

        public int GetMaxThreads(Parameters parameters) {
            return 4;
        }

        public string GetInputName(int index){
            if (index < 0 || index > Tables.Length){
                return null;
            }
            return Tables[index];
        }

        public abstract IMatrixData ProcessData(IMatrixData[] inputData, Parameters param, ref IMatrixData[] supplTables,
                                                ref IDocumentData[] documents, ProcessInfo processInfo);

        public abstract Parameters GetParameters(IMatrixData[] inputData, ref string errString);

        internal void ValidateParameters(IMatrixData[] inputData, ref string errString) {
            foreach (IMatrixData matrix in inputData) {
                Constants.ValidateColumnNames(matrix, ref errString);
            }
        }

        internal IMatrixData GetMatrixData(string name, IMatrixData[] inputData){
            int index = -1;
            for (int i = 0; i < MaxNumInput; i++){
                string temp = GetInputName(i);
                if (String.IsNullOrEmpty(temp)){
                    continue;
                }
                if (temp.Equals(name, StringComparison.CurrentCultureIgnoreCase)){
                    index = i;
                }
            }

            if (index == -1){
                return null;
            }

            return inputData[index];
        }

        protected readonly CVLookUp cv = new CVLookUp();

        protected string[] _parameterNames;

        protected string[] ParameterNames(Parameter[] parameters) {
            if (_parameterNames == null) {
                var tmp = parameters.Select(x => x.Name);
                tmp = tmp.Select(SimplifyParameterName);
                _parameterNames = tmp.ToArray();
            }
            return _parameterNames;
        }

        protected static string SimplifyParameterName(string name) {
            return name.Replace("(*)", "").Replace("*", "").Replace("(optional)", "");
        }

        protected Parameter FindParam(Parameters parameters, string name) {
            Parameter[] parameter = parameters.GetAllParameters();
            int i = ArrayUtils.IndexOf(ParameterNames(parameter), name);
            if (i == -1) {
                return null;
            }
            return parameter[i];
        }

        internal static string CheckIfDescriptionExists(IList<ParameterDescription> help, string title, ref string desc,
                                                        Metadata mtd) {
            string shortName = ParameterDescription.Shorten(title);
            if (help != null && help.Count > 0) {
                ParameterDescription parameterDescription = help.FirstOrDefault(x => x.Match(shortName));
                if (parameterDescription != null) {
                    if (mtd != null) {
                        string type = parameterDescription.GetFieldType(mtd.TabDescription.MzTabType,
                                                                        mtd.TabDescription.MzTabMode);
                        if (type != null) {
                            if (type.Equals("mandatory")) {
                                title += "*";
                            } else if (type.Equals("(mandatory)")) {
                                title += "(*)";
                            } else if (type.Equals("optional")) {
                                title += "(optional)";
                            } else if (type.Equals("none")) {
                                //TODO:
                            }
                        }
                    }
                    desc = parameterDescription.Definition;
                }
            }
            return title;
        }

        protected void GetExperminetValues(IMatrixData summary, IMatrixData experimentalDesignTemplate,
                                           IMatrixData experiment, IMatrixData spectraRef, ref List<MsRunImpl> msruns,
                                           ref List<StudyVariable> studyvariables, ref List<Assay> assays,
                                           ref List<Sample> samples, ref List<Instrument> instruments) {
            if (msruns == null) {
                msruns = new List<MsRunImpl>();
            }

            if (studyvariables == null) {
                studyvariables = new List<StudyVariable>();
            }

            if (assays == null) {
                assays = new List<Assay>();
            }

            if (samples == null) {
                samples = new List<Sample>();
            }

            if (instruments == null) {
                instruments = new List<Instrument>();
            }

            #region parse experiment

            if (experiment != null) {
                int studyvarIndex = experiment.StringColumnNames.IndexOf(MetadataElement.STUDY_VARIABLE.Name);
                int assayIndex = experiment.StringColumnNames.IndexOf(MetadataElement.ASSAY.Name);
                int msrunIndex = experiment.StringColumnNames.IndexOf(MetadataElement.MS_RUN.Name);
                int sampleIndex = experiment.StringColumnNames.IndexOf(MetadataElement.SAMPLE.Name);

                Regex sampleRegex = new Regex(@"^([^\[]+) <([^;]*);([^;]*);([^;]*);([^;]*)>");
                Regex runRegex = new Regex(@"^([^\[]+) <([^;]*);([^;]*);([^;]*);([^;]*)>");
                Regex assayRegex = new Regex(@"^([^\[]+) <([^>]*)>");

                for (int row = 0; row < experiment.RowCount; row++) {
                    string studyvariableDescription = experiment.StringColumns[studyvarIndex][row];
                    string assayReagent = experiment.StringColumns[assayIndex][row];
                    string msrunText = experiment.StringColumns[msrunIndex][row];
                    string sampleDescription = experiment.StringColumns[sampleIndex][row];
                    Lib.Model.Param specie = null;
                    Lib.Model.Param tissue = null;
                    Lib.Model.Param cellType = null;
                    Lib.Model.Param disease = null;
                    IList<Lib.Model.Param> mod = new List<Lib.Model.Param>();

                    if (sampleDescription != null && sampleRegex.IsMatch(sampleDescription)) {
                        var match = sampleRegex.Match(sampleDescription);
                        sampleDescription = match.Groups[1].Value;

                        string temp = match.Groups[2].Value;
                        if (!String.IsNullOrEmpty(temp)) {
                            specie = cv.GetParam(temp, "NEWT");
                        }

                        temp = match.Groups[3].Value;
                        if (!String.IsNullOrEmpty(temp)) {
                            tissue = cv.GetParam(temp, "BTO");
                        }

                        temp = match.Groups[4].Value;
                        if (!String.IsNullOrEmpty(temp)) {
                            cellType = cv.GetParam(temp, "CL");
                        }

                        temp = match.Groups[5].Value;
                        if (!String.IsNullOrEmpty(temp)) {
                            disease = cv.GetParam(temp, "DOID");
                        }
                    }
                    if (assayRegex != null && assayRegex.IsMatch(assayReagent)) {
                        var match = assayRegex.Match(assayReagent);
                        string temp = match.Groups[2].Value;
                        if (!String.IsNullOrEmpty(temp)) {
                            foreach (var t in temp.Split(';')) {
                                mod.Add(cv.GetParam(t, "PRIDE"));
                            }
                        }

                        assayReagent = match.Groups[1].Value;
                    }

                    string filename = null;
                    string path = null;
                    Lib.Model.Param format = null;
                    Lib.Model.Param idformat = null;
                    Lib.Model.Param fragementaion = null;
                    if (runRegex != null && runRegex.IsMatch(msrunText)) {
                        var match = runRegex.Match(msrunText);
                        filename = match.Groups[1].Value;

                        string temp = match.Groups[2].Value;
                        if (!String.IsNullOrEmpty(temp)) {
                            path = temp;
                        }

                        temp = match.Groups[3].Value;
                        if (!String.IsNullOrEmpty(temp)) {
                            format = cv.GetParam(temp, "MS");
                        }

                        temp = match.Groups[4].Value;
                        if (!String.IsNullOrEmpty(temp)) {
                            idformat = cv.GetParam(temp, "MS");
                        }

                        temp = match.Groups[5].Value;
                        if (!String.IsNullOrEmpty(temp)) {
                            fragementaion = cv.GetParam(temp, "MS");
                        }
                    }

                    StudyVariable studyvariable;
                    if (!studyvariables.Any(x => x.Description.Equals(studyvariableDescription))) {
                        studyvariable = new StudyVariable(studyvariables.Count + 1) {
                            Description = studyvariableDescription
                        };
                        studyvariables.Add(studyvariable);
                    } else {
                        studyvariable = studyvariables.First(x => x.Description.Equals(studyvariableDescription));
                    }

                    Assay assay = new Assay(assays.Count + 1) {
                        QuantificationReagent = cv.GetParam(assayReagent, "PRIDE")
                    };

                    foreach (var m in mod) {
                        if (m == null) {
                            continue;
                        }
                        assay.addQuantificationMod(new AssayQuantificationMod(assay,
                                                                              assay.QuantificationModMap.Count + 1) {
                                                                                  Param = m
                                                                              });
                    }

                    assays.Add(assay);

                    MsRunImpl msrun;
                    if (!String.IsNullOrEmpty(filename) &&
                        !msruns.Any(x => x.Description != null && x.Description.Equals(filename))) {
                        msrun = new MsRunImpl(msruns.Count + 1) {
                            Format = format,
                            IdFormat = idformat,
                            FragmentationMethod = fragementaion
                        };

                        msruns.Add(msrun);
                        msrun.Location = new Url(String.IsNullOrEmpty(path) ? filename : Path.Combine(path, filename));
                    } else {
                        msrun = msruns.First(x => x.Description != null && x.Description.Equals(filename));
                    }

                    Sample sample;
                    if (!samples.Any(x => x.Description.Equals(sampleDescription))) {
                        sample = new Sample(samples.Count + 1) { Description = sampleDescription };
                        if (specie != null) {
                            sample.AddSpecies(specie);
                        }
                        if (tissue != null) {
                            sample.AddTissue(tissue);
                        }
                        if (cellType != null) {
                            sample.AddCellType(cellType);
                        }
                        if (disease != null) {
                            sample.AddDisease(disease);
                        }
                        samples.Add(sample);
                    } else {
                        sample = samples.First(x => x.Description.Equals(sampleDescription));
                    }

                    if (!studyvariable.AssayMap.ContainsKey(assay.Id)) {
                        studyvariable.AddAssay(assay);
                    }
                    if (!studyvariable.SampleMap.ContainsKey(sample.Id)) {
                        studyvariable.AddSample(sample);
                    }

                    assay.MsRun = msrun;
                    assay.Sample = sample;
                }
            }

            #endregion

            Dictionary<int, IList<string>> dictionary = new Dictionary<int, IList<string>>();

            #region parse experimentalDesign

            if (experimentalDesignTemplate != null) {
                string[] rawfiles = null;

                int index = Constants.GetKeywordIndex(experimentalDesign.rawfile,
                                                     experimentalDesignTemplate.StringColumnNames);
                if (index != -1) {
                    rawfiles = experimentalDesignTemplate.StringColumns[index];
                }

                string[] experimentNames = null;
                if (
                    (index =
                     Constants.GetKeywordIndex(experimentalDesign.variable,
                                              experimentalDesignTemplate.StringColumnNames)) !=
                    -1) {
                    experimentNames = experimentalDesignTemplate.StringColumns[index];
                } else if (
                      (index =
                       Constants.GetKeywordIndex(experimentalDesign.variable,
                                                experimentalDesignTemplate.CategoryColumnNames)) != -1) {
                    experimentNames = MzTabMatrixUtils.ConvertToStringArray(experimentalDesignTemplate.GetCategoryColumnAt(index));
                }

                if (rawfiles != null && experimentNames != null) {
                    for (int i = 0; i < rawfiles.Length && i < experimentNames.Length; i++) {
                        string name = experimentNames[i];
                        StudyVariable variable = studyvariables.FirstOrDefault(x => x.Description.Equals(name));
                        if (variable == null) {
                            variable = new StudyVariable(studyvariables.Count + 1) { Description = name };
                            studyvariables.Add(variable);
                        }

                        string rawfile = rawfiles[i];
                        MsRunImpl runImpl = msruns.FirstOrDefault(x => x.Description.Equals(rawfile));
                        if (runImpl == null) {
                            runImpl = new MsRunImpl(msruns.Count + 1) {
                                Location = new Url(rawfile),
                                Format = cv.GetParam("MS:1000563", "MS"),
                                IdFormat = cv.GetParam("MS:1000768", "MS")
                            };
                            msruns.Add(runImpl);
                        }

                        if (rawfile != null) {
                            if (!dictionary.ContainsKey(variable.Id)) {
                                dictionary.Add(variable.Id, new List<string>());
                            }
                            dictionary[variable.Id].Add(rawfile);
                        }
                    }
                } else {
                    Console.Out.WriteLine("Rawfiles " + rawfiles);
                    Console.Out.WriteLine("experimentNames " + experimentNames);
                    throw new Exception("Could not parse " + Matrix.ExperimentalDesign);
                }
            }

            #endregion

            #region add default samples from studyvariables

            if (studyvariables != null && studyvariables.Count > 0) {
                foreach (StudyVariable variable in studyvariables) {
                    string text = variable.Description;

                    Sample sample = samples.FirstOrDefault(x => text.Contains(x.Description));
                    if (sample == null) {
                        sample = new Sample(samples.Count + 1) { Description = text };
                        samples.Add(sample);
                    }
                    variable.AddSample(sample);
                }
            }

            #endregion

            #region parse summary

            if (summary != null) {
                int maxRow = msruns.Count;

                string multi = "1";
                string[] labels0 = null;
                int index;

                if ((index = Constants.GetKeywordIndex(Utils.summary.labels0, summary.StringColumnNames)) != -1) {
                    labels0 = summary.StringColumns[index];
                    multi = "1";
                } else if ((index = Constants.GetKeywordIndex(Utils.summary.labels0, summary.CategoryColumnNames)) !=
                           -1) {
                    labels0 = MzTabMatrixUtils.ConvertToStringArray(summary.GetCategoryColumnAt(index));
                    multi = "1";
                }

                string[] labels1 = null;
                if ((index = Constants.GetKeywordIndex(Utils.summary.labels1, summary.StringColumnNames)) != -1) {
                    labels1 = summary.StringColumns[index];
                    multi = "2";
                } else if ((index = Constants.GetKeywordIndex(Utils.summary.labels1, summary.CategoryColumnNames)) !=
                           -1) {
                    labels1 = MzTabMatrixUtils.ConvertToStringArray(summary.GetCategoryColumnAt(index));
                    multi = "2";
                }

                string[] labels2 = null;
                if ((index = Constants.GetKeywordIndex(Utils.summary.labels2, summary.StringColumnNames)) != -1) {
                    labels2 = summary.StringColumns[index];
                    multi = "3";
                } else if ((index = Constants.GetKeywordIndex(Utils.summary.labels2, summary.CategoryColumnNames)) !=
                           -1) {
                    labels2 = MzTabMatrixUtils.ConvertToStringArray(summary.GetCategoryColumnAt(index));
                    multi = "3";
                }

                string[] multiplicity;
                if ((index = Constants.GetKeywordIndex(Utils.summary.multiplicity, summary.StringColumnNames)) !=
                    -1) {
                    multiplicity = summary.StringColumns[index];
                    multiplicity = multiplicity.Where(x => !String.IsNullOrEmpty(x)).ToArray();
                } else if (
                      (index =
                       Constants.GetKeywordIndex(Utils.summary.multiplicity, summary.CategoryColumnNames)) !=
                      -1) {
                    multiplicity = MzTabMatrixUtils.ConvertToStringArray(summary.GetCategoryColumnAt(index));
                    multiplicity = multiplicity.Where(x => !String.IsNullOrEmpty(x)).ToArray();
                } else {
                    multiplicity = new string[maxRow];
                    for (int i = 0; i < multiplicity.Length; i++) {
                        multiplicity[i] = multi;
                    }
                }

                string[] labels;
                switch (multi) {
                    case "1":
                        labels = null;
                        break;
                    case "2":
                        labels = new[] { "L", "H" };
                        break;
                    case "3":
                        labels = new[] { "L", "H", "M" };
                        break;
                    default:
                        labels = null;
                        break;
                }

                if (labels != null) {
                    List<StudyVariable> list = new List<StudyVariable>();
                    Dictionary<int, IList<string>> dict = new Dictionary<int, IList<string>>();

                    foreach (StudyVariable studyVariable in studyvariables){
                        foreach (var variable in SILAC(studyVariable, labels)) {
                            IList<string> rawfile = null;
                            if (dictionary.ContainsKey(variable.Id)) {
                                rawfile = dictionary[variable.Id];
                            }

                            StudyVariable tmp = new StudyVariable(list.Count + 1){Description = variable.Description};
                            tmp.AddAllAssays(variable.AssayMap.Values.ToList());
                            tmp.AddAllSamples(variable.SampleMap.Values.ToList());

                            list.Add(tmp);

                            if (rawfile != null) {
                                if (!dict.ContainsKey(tmp.Id)) {
                                    dict.Add(tmp.Id, rawfile);
                                }
                            }
                        }
                    }
                    studyvariables = list;
                    dictionary = dict;
                }

                string[] rawfiles = null;
                if ((index = Constants.GetKeywordIndex(Utils.summary.rawfile, summary.StringColumnNames)) != -1) {
                    rawfiles = summary.StringColumns[index];
                    rawfiles = rawfiles.Where(x => !String.IsNullOrEmpty(x)).ToArray();
                } else if ((index = Constants.GetKeywordIndex(Utils.summary.rawfile, summary.CategoryColumnNames)) !=
                           -1) {
                    rawfiles = MzTabMatrixUtils.ConvertToStringArray(summary.GetCategoryColumnAt(index));
                    rawfiles = rawfiles.Where(x => !String.IsNullOrEmpty(x)).ToArray();
                }

                string[] orbitrapInstruments = new[] { "LTQ Orbitrap", "LTQ Orbitrap XL", "LTQ Orbitrap Velos", "LTQ Orbitrap Elite", "Q Exactive" };
                string[] instrument = null;
                if ((index = Constants.GetKeywordIndex(Utils.summary.instrument, summary.StringColumnNames)) != -1) {
                    instrument = summary.StringColumns[index];
                    instrument = instrument.Where(x => !String.IsNullOrEmpty(x)).ToArray();
                } else if ((index = Constants.GetKeywordIndex(Utils.summary.instrument, summary.CategoryColumnNames)) !=
                           -1) {
                    instrument = MzTabMatrixUtils.ConvertToStringArray(summary.GetCategoryColumnAt(index));
                    instrument = instrument.Where(x => !String.IsNullOrEmpty(x)).ToArray();
                }

                if (rawfiles != null) {
                    for (int i = 0; i < rawfiles.Length; i++) {
                        int id = assays.Count + 1;
                        string rawfile = rawfiles[i];

                        if (!dictionary.Values.Any(x => x.Contains(rawfile))) {
                            continue;
                        }

                        IList<StudyVariable> temp = new List<StudyVariable>();
                        foreach (var v in dictionary.Where(x => x.Value.Contains(rawfile))) {
                            temp.Add(studyvariables.FirstOrDefault(x => x.Id == v.Key));
                        }

                        StudyVariable variable1 = null;
                        StudyVariable variable2 = null;
                        StudyVariable variable3 = null;
                        if (temp != null) {
                            if (temp.Any()) {
                                variable1 = temp[0];
                            }
                            if (temp.Count() > 1) {
                                variable2 = temp[1];
                            }
                            if (temp.Count() > 2) {
                                variable3 = temp[2];
                            }
                        }

                        if (multiplicity[i].Equals("1")) {
                            #region Add assay for label free

                            Assay assay = new Assay(id) {
                                QuantificationReagent = cv.GetParam("Unlabeled sample", "PRIDE"),
                                MsRun = msruns[i]
                            };
                            if (variable1 != null) {
                                assay.Sample = variable1.SampleMap.Values.FirstOrDefault();
                                variable1.AddAssay(assay);
                            }
                            assays.Add(assay);

                            #endregion
                        } else if (multiplicity[i].Equals("2")) {
                            #region Add assays for Double SILAC labeling

                            Assay assay = new Assay(id) {
                                QuantificationReagent = cv.GetParam("SILAC light", "PRIDE"),
                                MsRun = msruns[i]
                            };
                            IList<AssayQuantificationMod> mods = MzTabMatrixUtils.GetQuantificationMod(labels0, i, assay);
                            if (mods != null) {
                                foreach (var m in mods) {
                                    assay.addQuantificationMod(m);
                                }
                            }
                            if (variable1 != null) {
                                assay.Sample = variable1.SampleMap.Values.FirstOrDefault();
                                variable1.AddAssay(assay);
                            }
                            assays.Add(assay);

                            assay = new Assay(id + 1) {
                                QuantificationReagent = cv.GetParam("SILAC heavy", "PRIDE"),
                                MsRun = msruns[i]
                            };
                            mods = MzTabMatrixUtils.GetQuantificationMod(labels1, i, assay);
                            if (mods != null) {
                                foreach (var m in mods) {
                                    assay.addQuantificationMod(m);
                                }
                            }
                            if (variable2 != null) {
                                assay.Sample = variable2.SampleMap.Values.FirstOrDefault();
                                variable2.AddAssay(assay);
                            }
                            assays.Add(assay);

                            #endregion
                        } else if (multiplicity[i].Equals("3")) {
                            #region Add assays for Triple SILAC labeling

                            Assay assay = new Assay(id) {
                                QuantificationReagent = cv.GetParam("SILAC light", "PRIDE"),
                                MsRun = msruns[i]
                            };
                            IList<AssayQuantificationMod> mods = MzTabMatrixUtils.GetQuantificationMod(labels0, i, assay);
                            if (mods != null) {
                                foreach (var m in mods) {
                                    assay.addQuantificationMod(m);
                                }
                            }
                            if (variable1 != null) {
                                assay.Sample = variable1.SampleMap.Values.FirstOrDefault();
                                variable1.AddAssay(assay);
                            }
                            assays.Add(assay);

                            assay = new Assay(id + 1) {
                                QuantificationReagent = cv.GetParam("SILAC medium", "PRIDE"),
                                MsRun = msruns[i]
                            };
                            mods = MzTabMatrixUtils.GetQuantificationMod(labels1, i, assay);
                            if (mods != null) {
                                foreach (var m in mods) {
                                    assay.addQuantificationMod(m);
                                }
                            }
                            if (variable2 != null) {
                                assay.Sample = variable2.SampleMap.Values.FirstOrDefault();
                                variable2.AddAssay(assay);
                            }
                            assays.Add(assay);

                            assay = new Assay(id + 2) {
                                QuantificationReagent = cv.GetParam("SILAC heavy", "PRIDE"),
                                MsRun = msruns[i]
                            };
                            mods = MzTabMatrixUtils.GetQuantificationMod(labels2, i, assay);
                            if (mods != null) {
                                foreach (var m in mods) {
                                    assay.addQuantificationMod(m);
                                }
                            }
                            if (variable3 != null) {
                                assay.Sample = variable3.SampleMap.Values.FirstOrDefault();
                                variable3.AddAssay(assay);
                            }
                            assays.Add(assay);

                            #endregion
                        }


                        if (instrument != null && !String.IsNullOrEmpty(instrument[i])) {
                            var tmp = new Instrument(instruments.Count + 1) { Name = cv.GetParam(instrument[i], "MS") };
                            if (orbitrapInstruments.Contains(instrument[i])) {
                                tmp.Source = cv.GetParam("electrospray ionization", "MS");
                                tmp.Analyzer = cv.GetParam("orbitrap", "MS");
                            }
                            instruments.Add(tmp);
                        }
                    }
                }
            }

            #endregion

            #region parse search

            Lib.Model.Param run_idFormat = cv.GetParam("MS:1000774", "MS");
            Lib.Model.Param run_format = cv.GetParam("Andromeda Peak list file", "MS");

            if (spectraRef != null && Constants.GetKeywordName(Utils.spectraRef.location, spectraRef.StringColumnNames) != null) {
                int colindex = Constants.GetKeywordIndex(Utils.spectraRef.location, spectraRef.StringColumnNames);
                string[] values =
                    ArrayUtils.UniqueValues(spectraRef.StringColumns[colindex]);

                for (int i = 0; i < values.Length; i++) {
                    Lib.Model.Param frag = null;
                    if (values[i].Contains("CID")) {
                        frag = cv.GetParam("MS:1000133", "MS");
                    } else if (values[i].Contains("HCD")) {
                        frag = cv.GetParam("MS:1000422", "MS");
                    }

                    msruns.Add(new MsRunImpl(msruns.Count + 1) {
                        IdFormat = run_idFormat,
                        Format = run_format,
                        FragmentationMethod = frag,
                        Location = new Url(values[i])
                    });
                }
            } else {
                msruns.Add(new MsRunImpl(msruns.Count + 1) {
                    IdFormat = run_idFormat,
                    Format = run_format
                });
            }

            #endregion
        }

        private IEnumerable<StudyVariable> SILAC(StudyVariable variable, string[] labels) {
            StudyVariable[] result = new StudyVariable[labels.Length];

            for (int i = 0; i < result.Length; i++) {
                result[i] = new StudyVariable(variable.Id);
                result[i].AddAllAssays(variable.AssayMap.Values.ToList());
                result[i].AddAllSamples(variable.SampleMap.Values.ToList());
                result[i].Description = String.Format("{0} {1}", labels[i], variable.Description);
            }

            return result;
        }

        protected void AddMsRunParameters(IList<Parameter> list, IList<MsRunImpl> runs, IList<ParameterDescription> help,
                                          bool defineGroupNumber) {
            string name = MetadataElement.MS_RUN.Name;
            string desc = null;
            CheckIfDescriptionExists(help, name, ref desc, null);

            MsRunImpl[] array = runs.Any() ? runs.ToArray() : null;

            IList<MsRunImpl> groups = MsRunPanel.UniqueGroups(runs);

            if (defineGroupNumber) {
                SingleChoiceWithSubParams group = new SingleChoiceWithSubParams(name) {
                    ParamNameWidth = 0,
                    TotalWidth = 700,
                    Help = desc
                };

                int count = groups == null ? 5 : groups.Count;

                group.Values = new List<string>();
                group.SubParams = new List<Parameters>();


                for (int i = 1; i < count; i++) {
                    int n = i + 1;
                    if (n < 1) {
                        continue;
                    }
                    group.Values.Add(n.ToString(CultureInfo.InvariantCulture));
                    group.SubParams.Add(new Parameters(new MsRunParam(n, array, cv, true)));
                }

                if (groups != null && group.SubParams.Count >= groups.Count) {
                    group.Value = group.Values.IndexOf(groups.Count.ToString(CultureInfo.InvariantCulture));
                }

                list.Add(group);
            } else {
                list.Add(new MsRunParam(groups.Count, array, cv, false, name) { Help = desc });
            }
        }

        protected void AddSampleParameters(IList<Parameter> list, IList<Sample> samples,
                                           IList<ParameterDescription> help, bool defineNumber) {
            Sample[] array = samples.Any() ? samples.ToArray() : null;

            if (defineNumber) {
                SingleChoiceWithSubParams group = new SingleChoiceWithSubParams(MetadataElement.SAMPLE.Name) {
                    ParamNameWidth = 0,
                    TotalWidth = 700
                };

                int count = array == null ? 15 : array.Length;

                group.Values = new List<string>();
                group.SubParams = new List<Parameters>();

                int[] temp = new[] { count };
                foreach (int n in temp){
                    if (n < 1) {
                        continue;
                    }
                    @group.Values.Add(n.ToString(CultureInfo.InvariantCulture));
                    @group.SubParams.Add(new Parameters(new SampleParam(array != null && array.Length >= n
                                                                           ? ArrayUtils.SubArray(array, n)
                                                                           : new Sample[n], true, cv)));
                }

                if (array != null && group.SubParams.Count >= array.Length) {
                    group.Value = group.Values.IndexOf(array.Length.ToString(CultureInfo.InvariantCulture));
                }

                list.Add(group);
            } else {
                list.Add(new SampleParam(array, false, cv, MetadataElement.SAMPLE.Name));
            }
        }

        protected void AddStudyVariableParameters(IList<Parameter> list, IList<StudyVariable> studyVariables,
                                                  IList<ParameterDescription> help, bool defineNumber) {
            StudyVariable[] array = studyVariables.Any() ? studyVariables.ToArray() : null;

            if (defineNumber) {
                SingleChoiceWithSubParams group = new SingleChoiceWithSubParams(MetadataElement.STUDY_VARIABLE.Name) {
                    ParamNameWidth = 0,
                    TotalWidth = 700
                };
                int count = array == null ? 10 : array.Length;
                group.Values = new List<string>();
                group.SubParams = new List<Parameters>();

                int[] temp = new[] { count };
                foreach (int n in temp){
                    if (n < 1) {
                        continue;
                    }
                    @group.Values.Add(n.ToString(CultureInfo.InvariantCulture));
                    @group.SubParams.Add(new Parameters(new StudyVariableParam(array != null && array.Length >= n
                                                                                  ? ArrayUtils.SubArray(array, n)
                                                                                  : new StudyVariable[n], true, cv)));
                }

                if (array != null && group.SubParams.Count >= array.Length) {
                    group.Value = group.Values.IndexOf(array.Length.ToString(CultureInfo.InvariantCulture));
                }

                list.Add(group);
            } else {
                list.Add(new StudyVariableParam(array, false, cv, MetadataElement.STUDY_VARIABLE.Name));
            }
        }

        protected void AddAssayParameters(IList<Parameter> list, IList<Assay> assays, IList<ParameterDescription> help,
                                          bool defineGroupNumber) {
            Assay[] array = assays.Any() ? assays.ToArray() : null;

            IList<Assay> groups = AssayPanel.UniqueGroups(assays);

            if (defineGroupNumber) {
                SingleChoiceWithSubParams group = new SingleChoiceWithSubParams(MetadataElement.ASSAY.Name) {
                    ParamNameWidth = 0,
                    TotalWidth = 700
                };

                int count = groups == null ? 5 : groups.Count < 3 ? 3 : groups.Count + 1;
                @group.Values = new string[count];
                @group.SubParams = new Parameters[count];
                for (int i = 0; i < count; i++) {
                    int n = i + 1;
                    @group.Values[i] = n.ToString(CultureInfo.InvariantCulture);
                    @group.SubParams[i] = new Parameters(new AssayParam(n, array, true, cv));
                }

                @group.Value = 0;
                if (groups != null && @group.SubParams.Count >= groups.Count) {
                    @group.Value = groups.Count - 1;
                }

                list.Add(@group);
            } else {
                list.Add(new AssayParam(groups.Count, array, false, cv, MetadataElement.ASSAY.Name));
            }
        }

        protected void AddInstrumentParameters(IList<Parameter> list, IList<Instrument> instruments,
                                               IList<ParameterDescription> help) {
            string name = MetadataElement.INSTRUMENT.Name;
            string desc = null;
            name = CheckIfDescriptionExists(help, name, ref desc, null);

            Instrument[] array = instruments.Any() ? instruments.ToArray() : null;
            SingleChoiceWithSubParams group = new SingleChoiceWithSubParams(name) {
                Help = desc,
                ParamNameWidth = 0,
                TotalWidth = 800,
                Value = 0,
                Values = new[] { "1", "2", "3" },
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

            if (array != null && group.SubParams.Count >= array.Length) {
                group.Value = array.Length - 1;
            }

            list.Add(group);
        }

        protected void AddOptionalColumns(MZTabColumnFactory factory, List<MZTabColumn> columns, string elementName) {
            Regex regex = new Regex(String.Format(@"(opt)_{0}\[[0-9]+\]_(.*)", elementName));
            IList<string> names = columns.Select(x => x.Name).ToList();
            foreach (MZTabColumn col in factory.OptionalColumnMapping.Values) {
                if (regex.IsMatch(col.Name)) {
                    var match = regex.Match(col.Name);
                    string name = String.Format("{0}_{1}", match.Groups[1].Value, match.Groups[2].Value);
                    if (names.Contains(name)) {
                        continue;
                    }
                    names.Add(name);
                    columns.Add(new MZTabColumn(name, col.Type, col.isOptional(), col.Order));
                }
            }
        }
    }
}