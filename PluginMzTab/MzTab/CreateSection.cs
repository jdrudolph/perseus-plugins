using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using BaseLib.Param;
using BaseLibS.Util;
using MzTabLibrary.model;
using MzTabLibrary.utils;
using MzTabLibrary.utils.errors;
using MzTabLibrary.utils.parser;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;
using PluginMzTab.extended;
using PluginMzTab.param;
using PluginMzTab.utils;
using Modification = MzTabLibrary.model.Modification;
using Parameters = BaseLib.Param.Parameters;

namespace PluginMzTab.mztab{
    public abstract class CreateSection : MzTabProcessing{
        protected readonly CVLookUp cv = new CVLookUp();
        
        protected string[] _parameterNames;
        
        protected string[] ParameterNames(Parameter[] parameters){
            if (_parameterNames == null){
                var tmp = parameters.Select(x => x.Name);
                tmp = tmp.Select(SimplifyParameterName);
                _parameterNames = tmp.ToArray();
            }
            return _parameterNames;
        }

        protected static string SimplifyParameterName(string name){
            return name.Replace("(*)", "").Replace("*", "").Replace("(optional)", "");
        }

        protected Parameter FindParam(Parameters parameters, string name){
            Parameter[] parameter = parameters.GetAllParameters();
            int i = ArrayUtils.IndexOf(ParameterNames(parameter), name);
            if (i == -1){
                return null;
            }
            return parameter[i];
        }
        
        internal IMatrixData ProcessData(IMatrixData[] inputData, ref IDocumentData[] documents, string matrixName,
                                         Parameters param, MZTabColumnFactory factory, string leadingColumnName,
                                         Section header, Section section, Action<int> progress, Action<string> status){
            IMatrixData mdata = GetMatrixData(matrixName, inputData);

            if (mdata == null){
                return null;
            }
            status("Create new matrix");
            IList<string> columnnames = new List<string>();
            IList<string[]> columns = new List<string[]>();
            IList<string> order = new List<string>();

            #region add columns

            foreach (Parameter parameter in param.GetAllParameters()){
                SingleChoiceParam columnParam = parameter as SingleChoiceParam;

                string[] values;
                if (columnParam != null){
                    string columnname = columnParam.SelectedValue;
                    values = GetValues(mdata, columnname);
                    if (values != null){
                        AddColumn(factory, parameter.Name, values, columnnames, columns, order);
                    }
                    continue;
                }

                SingleChoiceWithSubParams singleChoiceWithSubParams = parameter as SingleChoiceWithSubParams;
                if (singleChoiceWithSubParams != null){
                    foreach (Parameters s in singleChoiceWithSubParams.SubParams){
                        Parameter[] list = s.GetAllParameters();
                        foreach (Parameter t in list){
                            SingleChoiceParam single = t as SingleChoiceParam;
                            if (single != null){
                                values = GetValues(mdata, single.SelectedValue);
                                /*if (values == null){
                                    values = new string[mdata.RowCount];
                                    values = values.Select(x => "Unknown").ToArray();
                                }*/
                                if (values != null){
                                    AddColumn(factory, single.Name, values, columnnames, columns, order);
                                }
                            }
                        }
                    }
                    continue;
                }

                MultiChoiceParam multiChoiceParam = parameter as MultiChoiceParam;
                if (multiChoiceParam != null){
                    if (parameter.Name == "order"){
                        continue;
                    }

                    for (int i = 0; i < multiChoiceParam.SelectedValues.Length; i++){
                        var columnname = multiChoiceParam.SelectedValues[i];
                        values = GetValues(mdata, columnname);

                        if (values != null){
                            string name = SimplifyParameterName(parameter.Name);

                            if (name.StartsWith("opt_")){
                                string[] keys = new[]{
                                    MetadataElement.ASSAY.Name, MetadataElement.STUDY_VARIABLE.Name,
                                    MetadataElement.MS_RUN.Name, MetadataElement.SAMPLE.Name
                                };
                                foreach (string key in keys){
                                    if (name.Contains(key)){
                                        name = name.Replace("_" + key, "")
                                                   .Replace("opt_", "opt_" + key + "[" + (i + 1) + "]_");
                                        break;
                                    }
                                }
                            }
                            else{
                                name = string.Format("{0}[{1}]", name, i + 1);
                            }

                            AddColumn(factory, name, values, columnnames, columns, order);
                        }
                    }
                }
            }

            AddColumn(factory, "modifications", new string[mdata.RowCount], columnnames, columns, order);

            #endregion

            /*if (columnnames.Contains(ProteinColumn.TAXID.Name)){
                int i = columnnames.IndexOf(ProteinColumn.TAXID.Name);
                columns[i] = columns[i].Select(x => x == "NaN" ? Constants.nullValue : x).ToArray();
            }*/

            #region Remove those rows where the identifier is NULL

            IList<int> indizes = new List<int>();
            for (int row = 0; row < columns[columnnames.IndexOf(leadingColumnName)].Length; row++){
                if (columns[columnnames.IndexOf(leadingColumnName)][row] != Constants.nullValue){
                    indizes.Add(row);
                }
            }

            for (int col = 0; col < columns.Count; col++){
                columns[col] = ArrayUtils.SubArray(columns[col], indizes);
            }

            int nrows = indizes.Count;

            #endregion

            columnnames = PrepareColumns(mdata, columnnames, order, header.Prefix, section.Prefix, ref columns);

            IMatrixData data = (IMatrixData) mdata.CreateNewInstance(DataType.Matrix);

            data.SetData(section.Name, new List<string>(), new float[nrows,columnnames.Count], columnnames.ToList(),
                         columns.ToList(),
                         new List<string>(), new List<string[][]>(), new List<string>(), new List<double[]>(),
                         new List<string>(), new List<double[][]>(), new List<string>(), new List<string[][]>(),
                         new List<string>(), new List<double[]>());

            if (true){
                status("Check Section");
                CheckSection(new[]{GetMatrixData(Matrix.MetadataSection, inputData), data}, ref documents, data.RowCount,
                             progress, status);
            }
            status("");
            return data;
        }

        internal static string[] GetValues(IMatrixData mdata, string columnname){
            int index;
            return GetValues(mdata, columnname, out index);
        }

        internal static string[] GetValues(IMatrixData mdata, string columnname, out int index){
            if (columnname == null){
                index = -1;
                return null;
            }

            if (mdata.StringColumnNames.Contains(columnname)){
                index = mdata.StringColumnNames.IndexOf(columnname);
                return mdata.StringColumns[index];
            }

            if (mdata.CategoryColumnNames.Contains(columnname)){
                //return mdata.GetCategoryColumnAt(mdata.CategoryColumnNames.IndexOf(columnname));
            }

            if (mdata.NumericColumnNames.Contains(columnname)){
                index = mdata.NumericColumnNames.IndexOf(columnname);
                return mdata.NumericColumns[index].Select(x => x.ToString(CultureInfo.InvariantCulture)).ToArray();
            }

            if (mdata.MultiNumericColumnNames.Contains(columnname)){
                index = mdata.MultiNumericColumnNames.IndexOf(columnname);
                return mdata.MultiNumericColumns[index].Select(x => x.ToString()).ToArray();
            }

            if (mdata.ExpressionColumnNames.Contains(columnname)){
                index = mdata.ExpressionColumnNames.IndexOf(columnname);
                return mdata.GetExpressionColumn(index).Select(x => x.ToString(CultureInfo.InvariantCulture)).ToArray();
            }
            index = -1;
            return null;
        }

        internal static void AddColumn(MZTabColumnFactory factory, string name, string[] values,
                                       IList<string> columnnames, IList<string[]> columns, IList<string> order){
            name = SimplifyParameterName(name);

            MZTabColumn col = factory.FindColumnByHeader(name.Trim());

            if (col == null || values == null){
                return;
            }

            if (columnnames.Contains(col.Header)){
                return;
            }

            for (int i = 0; i < values.Length; i++){
                if (string.IsNullOrEmpty(values[i])){
                    values[i] = Constants.nullValue;
                }
            }

            columnnames.Add(col.Header);
            columns.Add(values);

            order.Add(col.Order);
        }

        internal static IList<string> PrepareColumns(IMatrixData mdata, IList<string> columnnames, IList<string> order,
                                                     string headerPrefix, string rowPrefix, ref IList<string[]> columns){
            columnnames = ArrayUtils.SubArray(columnnames, ArrayUtils.Order(order));
            columns = ArrayUtils.SubArray(columns, ArrayUtils.Order(order));

            var temp = new List<string>{headerPrefix};
            temp.AddRange(columnnames);
            columnnames = temp;


            string[] prefix = new string[mdata.RowCount];
            for (int i = 0; i < prefix.Length; i++){
                prefix[i] = rowPrefix;
            }

            var temp2 = new List<string[]>{prefix};
            temp2.AddRange(columns);
            columns = temp2;

            return columnnames;
        }

        internal static void AddParameter(IList<Parameter> list, MZTabColumn column, IMatrixData mdata,
                                          IList<string> selection, IList<ParameterDescription> help, Metadata mtd){
            IList<string> values = GetColumnHeaderOfType(mdata, column.Type);
            values = MzTabMatrixUtils.Sort(values);
            values.Add("");

            int index = -1;
            if (selection != null && selection.Count > 0){
                foreach (string defaultValue in selection){
                    index = defaultValue == null ? -1 : values.IndexOf(defaultValue);
                    if (index != -1){
                        break;
                    }
                }
            }

            string desc = null;
            string name = CheckIfDescriptionExists(help, column, ref desc, mtd);

            SingleChoiceParam columnParam = new SingleChoiceParam(name, index){Values = values, Help = desc};
            list.Add(columnParam);
        }

        internal static void AddParameter(IList<Parameter> list, MZTabColumn column, MetadataElement element,
                                          IMatrixData mdata, IList<ParameterDescription> help, Metadata mtd){
            IList<string> values = GetColumnHeaderOfType(mdata, column.Type);
            values = MzTabMatrixUtils.Sort(values);

            string pattern = string.Format("{0}_{1}\\[[0-9]+\\]", column.Name, element.Name);
            if (column.Name.StartsWith("opt")){
                pattern = string.Format("opt_{0}\\[[0-9]+\\]_{1}", element.Name, column.Name.Replace("opt_", ""));
            }

            Regex regex = new Regex(pattern);

            IList<int> selection = new List<int>();
            for (int i = 0; i < values.Count; i++){
                if (regex.IsMatch(values[i])){
                    selection.Add(i);
                }
            }

            string desc = null;
            string paramName = CheckIfDescriptionExists(help, string.Format("{0}_{1}", column.Name, element.Name),
                                                        ref desc, mtd);

            MultiChoiceParam param = new MultiChoiceParam(paramName){Values = values, Value = selection.ToArray()};

            list.Add(param);
        }

        internal static void AddParameter(IList<Parameter> list, MZTabColumn[][] variables, IMatrixData mdata,
                                          Metadata mtd, IList<string> items, string name,
                                          IList<ParameterDescription> help){
            SingleChoiceWithSubParams param = new SingleChoiceWithSubParams(name){
                ParamNameWidth = 250,
                TotalWidth = 500,
                Value = 0,
                Values = items,
                Help = null,
                SubParams = new Parameters[items.Count]
            };

            for (int c = 0; c < items.Count; c++){
                IList<Parameter> internalList = new List<Parameter>();

                foreach (var variable in variables){
                    MZTabColumn column = c >= variable.Length ? null : variable[c];
                    if (column == null){
                        continue;
                    }
                    IList<string> values = GetColumnHeaderOfType(mdata, column.Type);
                    values = MzTabMatrixUtils.Sort(values);
                    values.Add("");

                    string desc = null;
                    string title = CheckIfDescriptionExists(help, column, ref desc, mtd);
                    internalList.Add(new SingleChoiceParam(title, -1){Values = values, Help = desc});
                }

                param.SubParams[c] = new Parameters(internalList);
            }

            list.Add(param);
        }

        internal static string CheckIfDescriptionExists(IList<ParameterDescription> help, MZTabColumn column,
                                                        ref string desc, Metadata mtd){
            if (column == null){
                return null;
            }
            string title = column.Header;
            return CheckIfDescriptionExists(help, title, ref desc, mtd);
        }

        internal static string CheckIfDescriptionExists(IList<ParameterDescription> help, string title, ref string desc,
                                                        Metadata mtd){
            string shortName = ParameterDescription.Shorten(title);
            if (help != null && help.Count > 0){
                ParameterDescription parameterDescription = help.FirstOrDefault(x => x.Match(shortName));
                if (parameterDescription != null){
                    if (mtd != null){
                        string type = parameterDescription.GetFieldType(mtd.TabDescription.MzTabType,
                                                                        mtd.TabDescription.MzTabMode);
                        if (type != null){
                            if (type.Equals("mandatory")){
                                title += "*";
                            }
                            else if (type.Equals("(mandatory)")){
                                title += "(*)";
                            }
                            else if (type.Equals("optional")){
                                title += "(optional)";
                            }
                            else if (type.Equals("none")){
                                //TODO:
                            }
                        }
                    }
                    desc = parameterDescription.Definition;
                }
            }
            return title;
        }

        internal static IList<string> GetColumnHeaderOfType(IMatrixData mdata, Type type){
            if (mdata == null || type == null){
                return null;
            }

            List<string> columnNames = new List<string>();
            if (type == typeof (string)){
                columnNames.AddRange(mdata.StringColumnNames);
                columnNames.AddRange(mdata.CategoryColumnNames);
            }
            else if (type == typeof (Uri)){
                columnNames.AddRange(mdata.StringColumnNames);
            }
            else if (type == typeof (int)){
                columnNames.AddRange(mdata.NumericColumnNames);
                columnNames.AddRange(mdata.CategoryColumnNames);
                columnNames.AddRange(mdata.ExpressionColumnNames);
            }
            else if (type == typeof (double)){
                columnNames.AddRange(mdata.NumericColumnNames);
                columnNames.AddRange(mdata.ExpressionColumnNames);
            }
            else if (type == typeof (Param)){
                columnNames.AddRange(mdata.StringColumnNames);
            }
            else if (type == typeof (MZBoolean)){
                columnNames.AddRange(mdata.StringColumnNames);
                columnNames.AddRange(mdata.CategoryColumnNames);
            }
            else if (type == typeof (Reliability)){
                columnNames.AddRange(mdata.CategoryColumnNames);
                columnNames.AddRange(mdata.StringColumnNames);
            }
            else if (type == typeof (SplitList<string>)){
                columnNames.AddRange(mdata.StringColumnNames);
                columnNames.AddRange(mdata.CategoryColumnNames);
            }
            else if (type == typeof (SplitList<double>)){
                columnNames.AddRange(mdata.NumericColumnNames);
                columnNames.AddRange(mdata.StringColumnNames);
                columnNames.AddRange(mdata.ExpressionColumnNames);
            }
            else if (type == typeof (SplitList<Param>)){
                columnNames.AddRange(mdata.StringColumnNames);
            }
            else if (type == typeof (SplitList<SpectraRef>)){
                columnNames.AddRange(mdata.StringColumnNames);
            }
            else if (type == typeof (SplitList<Modification>)){
                columnNames.AddRange(mdata.StringColumnNames);
            }
            else{
                MessageBox.Show(@"Can not decide which type is matching. " + type);
            }

            return columnNames;
        }

        protected void GetExperminetValues(IMatrixData summary, IMatrixData experimentalDesignTemplate,
                                           IMatrixData experiment, ref List<MsRunImpl> msruns,
                                           ref List<StudyVariable> studyvariables, ref List<Assay> assays,
                                           ref List<Sample> samples){
            if (msruns == null){
                msruns = new List<MsRunImpl>();
            }

            if (studyvariables == null){
                studyvariables = new List<StudyVariable>();
            }

            if (assays == null){
                assays = new List<Assay>();
            }

            if (samples == null){
                samples = new List<Sample>();
            }

            #region parse experiment

            if (experiment != null){
                int studyvarIndex = experiment.StringColumnNames.IndexOf(MetadataElement.STUDY_VARIABLE.Name);
                int assayIndex = experiment.StringColumnNames.IndexOf(MetadataElement.ASSAY.Name);
                int msrunIndex = experiment.StringColumnNames.IndexOf(MetadataElement.MS_RUN.Name);
                int sampleIndex = experiment.StringColumnNames.IndexOf(MetadataElement.SAMPLE.Name);

                Regex sampleRegex = new Regex(@"^([^\[]+) <([^;]*);([^;]*);([^;]*);([^;]*)>");
                Regex runRegex = new Regex(@"^([^\[]+) <([^;]*);([^;]*);([^;]*);([^;]*)>");
                Regex assayRegex = new Regex(@"^([^\[]+) <([^>]*)>");

                for (int row = 0; row < experiment.RowCount; row++){
                    string studyvariableDescription = experiment.StringColumns[studyvarIndex][row];
                    string assayReagent = experiment.StringColumns[assayIndex][row];
                    string msrunText = experiment.StringColumns[msrunIndex][row];
                    string sampleDescription = experiment.StringColumns[sampleIndex][row];
                    Param specie = null;
                    Param tissue = null;
                    Param cellType = null;
                    Param disease = null;
                    IList<Param> mod = new List<Param>();

                    if (sampleDescription != null && sampleRegex.IsMatch(sampleDescription)){
                        var match = sampleRegex.Match(sampleDescription);
                        sampleDescription = match.Groups[1].Value;

                        string temp = match.Groups[2].Value;
                        if (!string.IsNullOrEmpty(temp)){
                            specie = cv.GetParam(temp, "NEWT");
                        }

                        temp = match.Groups[3].Value;
                        if (!string.IsNullOrEmpty(temp)){
                            tissue = cv.GetParam(temp, "BTO");
                        }

                        temp = match.Groups[4].Value;
                        if (!string.IsNullOrEmpty(temp)){
                            cellType = cv.GetParam(temp, "CL");
                        }

                        temp = match.Groups[5].Value;
                        if (!string.IsNullOrEmpty(temp)){
                            disease = cv.GetParam(temp, "DOID");
                        }
                    }
                    if (assayRegex != null && assayRegex.IsMatch(assayReagent)){
                        var match = assayRegex.Match(assayReagent);
                        string temp = match.Groups[2].Value;
                        if (!string.IsNullOrEmpty(temp)){
                            foreach (var t in temp.Split(';')){
                                mod.Add(cv.GetParam(t, "PRIDE"));
                            }
                        }

                        assayReagent = match.Groups[1].Value;
                    }

                    string filename = null;
                    string path = null;
                    Param format = null;
                    Param idformat = null;
                    Param fragementaion = null;
                    if (runRegex != null && runRegex.IsMatch(msrunText)){
                        var match = runRegex.Match(msrunText);
                        filename = match.Groups[1].Value;

                        string temp = match.Groups[2].Value;
                        if (!string.IsNullOrEmpty(temp)){
                            path = temp;
                        }

                        temp = match.Groups[3].Value;
                        if (!string.IsNullOrEmpty(temp)){
                            format = cv.GetParam(temp, "MS");
                        }

                        temp = match.Groups[4].Value;
                        if (!string.IsNullOrEmpty(temp)){
                            idformat = cv.GetParam(temp, "MS");
                        }

                        temp = match.Groups[5].Value;
                        if (!string.IsNullOrEmpty(temp)){
                            fragementaion = cv.GetParam(temp, "MS");
                        }
                    }

                    StudyVariable studyvariable;
                    if (!studyvariables.Any(x => x.Description.Equals(studyvariableDescription))){
                        studyvariable = new StudyVariable(studyvariables.Count + 1){
                            Description = studyvariableDescription
                        };
                        studyvariables.Add(studyvariable);
                    }
                    else{
                        studyvariable = studyvariables.First(x => x.Description.Equals(studyvariableDescription));
                    }

                    Assay assay = new Assay(assays.Count + 1){
                        QuantificationReagent = cv.GetParam(assayReagent, "PRIDE")
                    };

                    foreach (var m in mod){
                        if (m == null){
                            continue;
                        }
                        assay.addQuantificationMod(new AssayQuantificationMod(assay,
                                                                              assay.QuantificationModMap.Count + 1){
                                                                                  Param = m
                                                                              });
                    }

                    assays.Add(assay);

                    MsRunImpl msrun;
                    if (!string.IsNullOrEmpty(filename) &&
                        !msruns.Any(x => x.Description != null && x.Description.Equals(filename))){
                        msrun = new MsRunImpl(msruns.Count + 1){
                            Format = format,
                            IdFormat = idformat,
                            FragmentationMethod = fragementaion
                        };

                        msruns.Add(msrun);
                        msrun.Location = new Url(string.IsNullOrEmpty(path) ? filename : Path.Combine(path, filename));
                    }
                    else{
                        msrun = msruns.First(x => x.Description != null && x.Description.Equals(filename));
                    }

                    Sample sample;
                    if (!samples.Any(x => x.Description.Equals(sampleDescription))){
                        sample = new Sample(samples.Count + 1){Description = sampleDescription};
                        if (specie != null){
                            sample.AddSpecies(specie);
                        }
                        if (tissue != null){
                            sample.AddTissue(tissue);
                        }
                        if (cellType != null){
                            sample.AddCellType(cellType);
                        }
                        if (disease != null){
                            sample.AddDisease(disease);
                        }
                        samples.Add(sample);
                    }
                    else{
                        sample = samples.First(x => x.Description.Equals(sampleDescription));
                    }

                    if (!studyvariable.AssayMap.ContainsKey(assay.Id)){
                        studyvariable.AddAssay(assay);
                    }
                    if (!studyvariable.SampleMap.ContainsKey(sample.Id)){
                        studyvariable.AddSample(sample);
                    }

                    assay.MsRun = msrun;
                    assay.Sample = sample;
                }

                return;
            }

            #endregion

            Dictionary<int, IList<string>> dictionary = new Dictionary<int, IList<string>>();

            #region parse experimentalDesign

            if (experimentalDesignTemplate != null){
                string folder = null;
                try{
                    string tmp = experimentalDesignTemplate.Origin;
                    if (tmp != null){
                        tmp = Path.GetDirectoryName(tmp);
                        if (Directory.Exists(tmp)){
                            folder = tmp;
                        }
                    }
                }
                catch (Exception){}

                string[] rawfiles = null;

                int index = Constants.GetColumnIndex(utils.experiment.rawfile,
                                                     experimentalDesignTemplate.StringColumnNames);
                if (index != -1){
                    rawfiles = experimentalDesignTemplate.StringColumns[index];
                }

                string[] experimentNames = null;
                if (
                    (index =
                     Constants.GetColumnIndex(utils.experiment.variable, experimentalDesignTemplate.StringColumnNames)) !=
                    -1){
                    experimentNames = experimentalDesignTemplate.StringColumns[index];
                }
                else if (
                    (index =
                     Constants.GetColumnIndex(utils.experiment.variable,
                                              experimentalDesignTemplate.CategoryColumnNames)) != -1){
                    experimentNames = ConvertToStringArray(experimentalDesignTemplate.GetCategoryColumnAt(index));
                }

                if (rawfiles != null && experimentNames != null){
                    for (int i = 0; i < rawfiles.Length && i < experimentNames.Length; i++){
                        string name = experimentNames[i];
                        StudyVariable variable = studyvariables.FirstOrDefault(x => x.Description.Equals(name));
                        if (variable == null){
                            variable = new StudyVariable(studyvariables.Count + 1){Description = name};
                            studyvariables.Add(variable);
                        }

                        string rawfile = rawfiles[i];
                        MsRunImpl runImpl = msruns.FirstOrDefault(x => x.Description.Equals(rawfile));
                        if (runImpl == null){
                            runImpl = new MsRunImpl(msruns.Count + 1){
                                Location = new Url(folder == null ? rawfile : Path.Combine(folder, rawfile))
                            };
                            msruns.Add(runImpl);
                        }

                        if (rawfile != null){
                            if (!dictionary.ContainsKey(variable.Id)){
                                dictionary.Add(variable.Id, new List<string>());
                            }
                            dictionary[variable.Id].Add(rawfile);
                        }
                    }
                }
                else{
                    throw new Exception("Could not parse " + Matrix.ExperimentalDesign);
                }
            }

            #endregion

            #region add default samples from studyvariables

            if (studyvariables != null && studyvariables.Count > 0){
                foreach (StudyVariable variable in studyvariables){
                    string text = variable.Description;

                    Sample sample = samples.FirstOrDefault(x => x.Description.Equals(text));
                    if (sample == null){
                        sample = new Sample(samples.Count + 1){Description = text};
                        samples.Add(sample);
                    }
                    variable.AddSample(sample);
                }
            }

            #endregion

            #region parse summary

            if (summary != null){
                int maxRow = msruns.Count;

                string multi = "1";
                string[] labels0 = null;
                int index;

                if ((index = Constants.GetColumnIndex(utils.summary.labels0, summary.StringColumnNames)) != -1){
                    labels0 = summary.StringColumns[index];
                    multi = "1";
                }
                else if ((index = Constants.GetColumnIndex(utils.summary.labels0, summary.CategoryColumnNames)) != -1){
                    labels0 = ConvertToStringArray(summary.GetCategoryColumnAt(index));
                    multi = "1";
                }

                string[] labels1 = null;
                if ((index = Constants.GetColumnIndex(utils.summary.labels1, summary.StringColumnNames)) != -1){
                    labels1 = summary.StringColumns[index];
                    multi = "2";
                }
                else if ((index = Constants.GetColumnIndex(utils.summary.labels1, summary.CategoryColumnNames)) != -1){
                    labels1 = ConvertToStringArray(summary.GetCategoryColumnAt(index));
                    multi = "2";
                }

                string[] labels2 = null;
                if ((index = Constants.GetColumnIndex(utils.summary.labels2, summary.StringColumnNames)) != -1){
                    labels2 = summary.StringColumns[index];
                    multi = "3";
                }
                else if ((index = Constants.GetColumnIndex(utils.summary.labels2, summary.CategoryColumnNames)) != -1){
                    labels2 = ConvertToStringArray(summary.GetCategoryColumnAt(index));
                    multi = "3";
                }

                string[] multiplicity;
                if ((index = Constants.GetColumnIndex(utils.summary.multiplicity, summary.StringColumnNames)) != -1){
                    multiplicity = summary.StringColumns[index];
                    multiplicity = multiplicity.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                }
                else if ((index = Constants.GetColumnIndex(utils.summary.multiplicity, summary.CategoryColumnNames)) !=
                         -1){
                    multiplicity = ConvertToStringArray(summary.GetCategoryColumnAt(index));
                    multiplicity = multiplicity.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                }
                else{
                    multiplicity = new string[maxRow];
                    for (int i = 0; i < multiplicity.Length; i++){
                        multiplicity[i] = multi;
                    }
                }

                string[] labels;
                switch (multi){
                    case "1":
                        labels = null;
                        break;
                    case "2":
                        labels = new[]{"L", "H"};
                        break;
                    case "3":
                        labels = new[]{"L", "H", "M"};
                        break;
                    default:
                        labels = null;
                        break;
                }

                if (labels != null){
                    List<StudyVariable> list = new List<StudyVariable>();
                    Dictionary<int, IList<string>> dict = new Dictionary<int, IList<string>>();

                    for (int i = 0; i < studyvariables.Count; i++){
                        foreach (var variable in SILAC(studyvariables[i], labels)){
                            IList<string> rawfile = null;
                            if (dictionary.ContainsKey(variable.Id)){
                                rawfile = dictionary[variable.Id];
                            }

                            StudyVariable tmp = new StudyVariable(list.Count + 1);
                            tmp.Description = variable.Description;
                            tmp.AddAllAssays(variable.AssayMap.Values.ToList());
                            tmp.AddAllSamples(variable.SampleMap.Values.ToList());

                            list.Add(tmp);

                            if (rawfile != null){
                                if (!dict.ContainsKey(tmp.Id)){
                                    dict.Add(tmp.Id, rawfile);
                                }
                            }
                        }
                    }
                    studyvariables = list;
                    dictionary = dict;
                }

                string[] rawfiles = null;
                if ((index = Constants.GetColumnIndex(utils.summary.rawfile, summary.StringColumnNames)) != -1){
                    rawfiles = summary.StringColumns[index];
                    rawfiles = rawfiles.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                }
                else if ((index = Constants.GetColumnIndex(utils.summary.rawfile, summary.CategoryColumnNames)) != -1){
                    rawfiles = ConvertToStringArray(summary.GetCategoryColumnAt(index));
                    rawfiles = rawfiles.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                }

                if (rawfiles != null){
                    for (int i = 0; i < rawfiles.Length; i++){
                        int id = assays.Count + 1;
                        string rawfile = rawfiles[i];

                        if (!dictionary.Values.Any(x => x.Contains(rawfile))){
                            continue;
                        }

                        IList<StudyVariable> temp = new List<StudyVariable>();
                        foreach (var v in dictionary.Where(x => x.Value.Contains(rawfile))){
                            temp.Add(studyvariables.FirstOrDefault(x => x.Id == v.Key));
                        }

                        StudyVariable variable1 = null;
                        StudyVariable variable2 = null;
                        StudyVariable variable3 = null;
                        if (temp != null){
                            if (temp.Any()){
                                variable1 = temp[0];
                            }
                            if (temp.Count() > 1){
                                variable2 = temp[1];
                            }
                            if (temp.Count() > 2){
                                variable3 = temp[2];
                            }
                        }

                        if (multiplicity[i].Equals("1")){
                            #region Add assay for label free

                            Assay assay = new Assay(id){
                                QuantificationReagent = cv.GetParam("Unlabeled sample", "PRIDE"),
                                MsRun = msruns[i]
                            };
                            if (variable1 != null){
                                assay.Sample = variable1.SampleMap.Values.FirstOrDefault();
                                variable1.AddAssay(assay);
                            }
                            assays.Add(assay);

                            #endregion
                        }
                        else if (multiplicity[i].Equals("2")){
                            #region Add assays for Double SILAC labeling

                            Assay assay = new Assay(id){
                                QuantificationReagent = cv.GetParam("SILAC light", "PRIDE"),
                                MsRun = msruns[i]
                            };
                            IList<AssayQuantificationMod> mods = GetQuantificationMod(labels0, i, assay);
                            if (mods != null){
                                foreach (var m in mods){
                                    assay.addQuantificationMod(m);
                                }
                            }
                            if (variable1 != null){
                                assay.Sample = variable1.SampleMap.Values.FirstOrDefault();
                                variable1.AddAssay(assay);
                            }
                            assays.Add(assay);

                            assay = new Assay(id + 1){
                                QuantificationReagent = cv.GetParam("SILAC heavy", "PRIDE"),
                                MsRun = msruns[i]
                            };
                            mods = GetQuantificationMod(labels1, i, assay);
                            if (mods != null){
                                foreach (var m in mods){
                                    assay.addQuantificationMod(m);
                                }
                            }
                            if (variable2 != null){
                                assay.Sample = variable2.SampleMap.Values.FirstOrDefault();
                                variable2.AddAssay(assay);
                            }
                            assays.Add(assay);

                            #endregion
                        }
                        else if (multiplicity[i].Equals("3")){
                            #region Add assays for Triple SILAC labeling

                            Assay assay = new Assay(id){
                                QuantificationReagent = cv.GetParam("SILAC light", "PRIDE"),
                                MsRun = msruns[i]
                            };
                            IList<AssayQuantificationMod> mods = GetQuantificationMod(labels0, i, assay);
                            if (mods != null){
                                foreach (var m in mods){
                                    assay.addQuantificationMod(m);
                                }
                            }
                            if (variable1 != null){
                                assay.Sample = variable1.SampleMap.Values.FirstOrDefault();
                                variable1.AddAssay(assay);
                            }
                            assays.Add(assay);

                            assay = new Assay(id + 1){
                                QuantificationReagent = cv.GetParam("SILAC medium", "PRIDE"),
                                MsRun = msruns[i]
                            };
                            mods = GetQuantificationMod(labels1, i, assay);
                            if (mods != null){
                                foreach (var m in mods){
                                    assay.addQuantificationMod(m);
                                }
                            }
                            if (variable2 != null){
                                assay.Sample = variable2.SampleMap.Values.FirstOrDefault();
                                variable2.AddAssay(assay);
                            }
                            assays.Add(assay);

                            assay = new Assay(id + 2){
                                QuantificationReagent = cv.GetParam("SILAC heavy", "PRIDE"),
                                MsRun = msruns[i]
                            };
                            mods = GetQuantificationMod(labels2, i, assay);
                            if (mods != null){
                                foreach (var m in mods){
                                    assay.addQuantificationMod(m);
                                }
                            }
                            if (variable3 != null){
                                assay.Sample = variable3.SampleMap.Values.FirstOrDefault();
                                variable3.AddAssay(assay);
                            }
                            assays.Add(assay);

                            #endregion
                        }
                    }
                }
            }

            #endregion
        }

        private StudyVariable[] SILAC(StudyVariable variable, string[] labels){
            StudyVariable[] result = new StudyVariable[labels.Length];

            for (int i = 0; i < result.Length; i++){
                result[i] = new StudyVariable(variable.Id);
                result[i].AddAllAssays(variable.AssayMap.Values.ToList());
                result[i].AddAllSamples(variable.SampleMap.Values.ToList());
                result[i].Description = string.Format("{0} {1}", labels[i], variable.Description);
            }

            return result;
        }

        private string[] ConvertToStringArray(string[][] categoricalColumn){
            return categoricalColumn != null && categoricalColumn.Length > 0
                       ? categoricalColumn.Select(x => StringUtils.Concat(";", x)).ToArray()
                       : null;
        }

        protected static IList<AssayQuantificationMod> GetQuantificationMod(string[] labels, int i, Assay assay){
            if (labels == null){
                return null;
            }
            IList<AssayQuantificationMod> list = new List<AssayQuantificationMod>();

            int n = 1;
            foreach (var label in labels[i].Split(';')){
				if (!BaseLib.Mol.Tables.Modifications.ContainsKey(label))
				{
                    continue;
                }
				BaseLib.Mol.Modification mod = BaseLib.Mol.Tables.Modifications[label];
                list.Add(new AssayQuantificationMod(assay, n++){
                    Param = new UserParam(mod.Name, null),
                    Position = mod.Position.ToString(),
                    Site = StringUtils.Concat(";", mod.GetSiteArray())
                });
            }

            return list;
        }

        protected void AddMsRunParameters(IList<Parameter> list, IList<MsRunImpl> runs, IList<ParameterDescription> help,
                                          bool defineGroupNumber){
            string name = MetadataElement.MS_RUN.Name;
            string desc = null;
            CheckIfDescriptionExists(help, name, ref desc, null);

            MsRunImpl[] array = runs.Any() ? runs.ToArray() : null;

            IList<MsRunImpl> groups = MsRunPanel.UniqueGroups(runs);

            if (defineGroupNumber){
                SingleChoiceWithSubParams group = new SingleChoiceWithSubParams(name){
                    ParamNameWidth = 0,
                    TotalWidth = 700,
                    Help = desc
                };

                int count = groups == null ? 5 : groups.Count;

                group.Values = new List<string>();
                group.SubParams = new List<Parameters>();


                for (int i = 0; i < count; i++){
                    int n = i + 1;
                    if (n < 1){
                        continue;
                    }
                    group.Values.Add(n.ToString(CultureInfo.InvariantCulture));
                    group.SubParams.Add(new Parameters(new MsRunParam(n, array, cv, true)));
                }

                if (groups != null && group.SubParams.Count >= groups.Count){
                    group.Value = group.Values.IndexOf(groups.Count.ToString(CultureInfo.InvariantCulture));
                }

                list.Add(group);
            }
            else{
                list.Add(new MsRunParam(1, array, cv, false, name){Help = desc});
            }
        }

        protected void AddSampleParameters(IList<Parameter> list, IList<Sample> samples,
                                           IList<ParameterDescription> help, bool defineNumber){
            Sample[] array = samples.Any() ? samples.ToArray() : null;

            if (defineNumber){
                SingleChoiceWithSubParams group = new SingleChoiceWithSubParams(MetadataElement.SAMPLE.Name){
                    ParamNameWidth = 0,
                    TotalWidth = 700
                };

                int count = array == null ? 15 : array.Length;

                group.Values = new List<string>();
                group.SubParams = new List<Parameters>();

                int[] temp = new[]{count};
                for (int i = 0; i < temp.Length; i++){
                    int n = temp[i];
                    if (n < 1){
                        continue;
                    }
                    group.Values.Add(n.ToString(CultureInfo.InvariantCulture));
                    group.SubParams.Add(new Parameters(new SampleParam(array != null && array.Length >= n
                                                                           ? ArrayUtils.SubArray(array, n)
                                                                           : new Sample[n], true, cv)));
                }

                if (array != null && group.SubParams.Count >= array.Length){
                    group.Value = group.Values.IndexOf(array.Length.ToString(CultureInfo.InvariantCulture));
                }

                list.Add(group);
            }
            else{
                list.Add(new SampleParam(array, false, cv, MetadataElement.SAMPLE.Name));
            }
        }

        protected void AddStudyVariableParameters(IList<Parameter> list, IList<StudyVariable> studyVariables,
                                                  IList<ParameterDescription> help, bool defineNumber){
            StudyVariable[] array = studyVariables.Any() ? studyVariables.ToArray() : null;

            if (defineNumber){
                SingleChoiceWithSubParams group = new SingleChoiceWithSubParams(MetadataElement.STUDY_VARIABLE.Name){
                    ParamNameWidth = 0,
                    TotalWidth = 700
                };
                int count = array == null ? 10 : array.Length;
                group.Values = new List<string>();
                group.SubParams = new List<Parameters>();

                int[] temp = new[]{count};
                for (int i = 0; i < temp.Length; i++){
                    int n = temp[i];
                    if (n < 1){
                        continue;
                    }
                    group.Values.Add(n.ToString(CultureInfo.InvariantCulture));
                    group.SubParams.Add(new Parameters(new StudyVariableParam(array != null && array.Length >= n
                                                                                  ? ArrayUtils.SubArray(array, n)
                                                                                  : new StudyVariable[n], true, cv)));
                }

                if (array != null && group.SubParams.Count >= array.Length){
                    group.Value = group.Values.IndexOf(array.Length.ToString(CultureInfo.InvariantCulture));
                }

                list.Add(group);
            }
            else{
                list.Add(new StudyVariableParam(array, false, cv, MetadataElement.STUDY_VARIABLE.Name));
            }
        }

        protected void AddAssayParameters(IList<Parameter> list, IList<Assay> assays, IList<ParameterDescription> help,
                                          bool defineGroupNumber){
            Assay[] array = assays.Any() ? assays.ToArray() : null;

            IList<Assay> groups = AssayPanel.UniqueGroups(assays);

            if (defineGroupNumber){
                SingleChoiceWithSubParams group = new SingleChoiceWithSubParams(MetadataElement.ASSAY.Name){
                    ParamNameWidth = 0,
                    TotalWidth = 700
                };

                int count = groups == null ? 5 : groups.Count < 3 ? 3 : groups.Count + 1;
                @group.Values = new string[count];
                @group.SubParams = new Parameters[count];
                for (int i = 0; i < count; i++){
                    int n = i + 1;
                    @group.Values[i] = n.ToString(CultureInfo.InvariantCulture);
                    @group.SubParams[i] = new Parameters(new AssayParam(n, array, true, cv));
                }

                @group.Value = 0;
                if (groups != null && @group.SubParams.Count >= groups.Count){
                    @group.Value = groups.Count - 1;
                }

                list.Add(@group);
            }
            else{
                list.Add(new AssayParam(groups.Count, array, false, cv, MetadataElement.ASSAY.Name));
            }
        }

        protected void AddOptionalColumns(MZTabColumnFactory factory, List<MZTabColumn> columns, string elementName){
            Regex regex = new Regex(string.Format(@"(opt)_{0}\[[0-9]+\]_(.*)", elementName));
            IList<string> names = columns.Select(x => x.Name).ToList();
            foreach (MZTabColumn col in factory.OptionalColumnMapping.Values){
                if (regex.IsMatch(col.Name)){
                    var match = regex.Match(col.Name);
                    string name = string.Format("{0}_{1}", match.Groups[1].Value, match.Groups[2].Value);
                    if (names.Contains(name)){
                        continue;
                    }
                    names.Add(name);
                    columns.Add(new MZTabColumn(name, col.Type, col.isOptional(), col.Order));
                }
            }
        }

        protected void CheckSection(IMatrixData[] inputData, ref IDocumentData[] documents, int nrows,
                                    Action<int> progress, Action<string> status){
            try{
                if (documents == null){
                    documents = new IDocumentData[NumDocuments];
                }

                for (int i = 0; i < NumDocuments; i++){
                    if (documents[i] == null){
                        documents[i] = (IDocumentData)inputData[i].CreateNewInstance(DataType.Document);
                    }
                }

                MatrixStream stream = new MatrixStream(inputData, new[]{false, true});
                TextWriter outstream = new StreamWriter(new DocumentStream(documents[0]));

                MZTabErrorList errorList = new MZTabErrorList(Level.Info);

                try{
                    check(new StreamReader(stream), outstream, errorList, nrows, progress, status);
                    //refine();
                }
                catch (MZTabException e){
                    outstream.Write(MZTabProperties.MZTabExceptionMessage);
                    errorList.Add(e.Error);
                }
                catch (MZTabErrorOverflowException){
                    outstream.Write(MZTabProperties.MZTabErrorOverflowExceptionMessage);
                }

                errorList.print(outstream);
                if (errorList.IsNullOrEmpty()){
                    outstream.Write("No errors in this section!" + MZTabConstants.NEW_LINE);
                }

                outstream.Close();
                //stream.Close();
            }
            catch (Exception e){
                MessageBox.Show(e.Message, e.StackTrace);
            }
        }

        public void check(StreamReader reader, TextWriter outstream, MZTabErrorList errorList, int nrows,
                          Action<int> progress, Action<string> status){
            COMLineParser comParser = new COMLineParser();
            MTDLineParser mtdParser = new MTDLineParser();
            PRHLineParser prhParser = null;
            PRTLineParser prtParser = null;
            PEHLineParser pehParser = null;
            PEPLineParser pepParser = null;
            PSHLineParser pshParser = null;
            PSMLineParser psmParser = null;
            SMHLineParser smhParser = null;
            SMLLineParser smlParser = null;

            SortedDictionary<int, Comment> commentMap = new SortedDictionary<int, Comment>();
            SortedDictionary<int, Protein> proteinMap = new SortedDictionary<int, Protein>();
            SortedDictionary<int, Peptide> peptideMap = new SortedDictionary<int, Peptide>();
            SortedDictionary<int, PSM> psmMap = new SortedDictionary<int, PSM>();
            SortedDictionary<int, SmallMolecule> smallMoleculeMap = new SortedDictionary<int, SmallMolecule>();

            PositionMapping prtPositionMapping = null;
            PositionMapping pepPositionMapping = null;
            PositionMapping psmPositionMapping = null;
            PositionMapping smlPositionMapping = null;

            int highWaterMark = 1;
            int lineNumber = 0;
            try{
                string line;
                while ((line = reader.ReadLine()) != null){
                    progress((lineNumber*100)/nrows);
                    status("Check line " + lineNumber);
                    lineNumber++;

                    if (string.IsNullOrEmpty(line) || line.StartsWith("MTH") || line.StartsWith("#")){
                        continue;
                    }

                    if (line.StartsWith(Section.Comment.Prefix)){
                        comParser.Parse(lineNumber, line, errorList);
                        commentMap.Add(lineNumber, comParser.getComment());
                        continue;
                    }

                    Section section = MZTabFileParser.getSection(line);
                    MZTabError error;
                    if (section == null){
                        error = new MZTabError(FormatErrorType.LinePrefix, lineNumber, MZTabFileParser.subString(line));
                        throw new MZTabException(error);
                    }
                    if (section.Level < highWaterMark){
                        Section currentSection = Section.FindSection(highWaterMark);
                        error = new MZTabError(LogicalErrorType.LineOrder, lineNumber, currentSection.Name, section.Name);
                        throw new MZTabException(error);
                    }

                    highWaterMark = section.Level;
                    // There exists errors during checking metadata section.
                    if (highWaterMark == 1 && !errorList.IsNullOrEmpty()){
                        break;
                    }

                    switch (highWaterMark){
                        case 1:
                            // metadata section.
                            mtdParser.Parse(lineNumber, line, errorList);
                            break;
                        case 2:
                            if (prhParser != null){
                                // header line only display once!
                                error = new MZTabError(LogicalErrorType.HeaderLine, lineNumber,
                                                       MZTabFileParser.subString(line));
                                throw new MZTabException(error);
                            }

                            // protein header section
                            prhParser = new PRHLineParser(mtdParser.Metadata);
                            prhParser.Parse(lineNumber, line, errorList);
                            prtPositionMapping = new PositionMapping(prhParser.getFactory(), line);

                            // tell system to continue check protein data line.
                            highWaterMark = 3;
                            break;
                        case 3:
                            if (prhParser == null){
                                // header line should be check first.
                                error = new MZTabError(LogicalErrorType.NoHeaderLine, lineNumber,
                                                       MZTabFileParser.subString(line));
                                throw new MZTabException(error);
                            }

                            if (prtParser == null){
                                prtParser = new PRTLineParser(prhParser.getFactory(), prtPositionMapping,
                                                              mtdParser.Metadata,
                                                              errorList);
                            }
                            prtParser.Parse(lineNumber, line, errorList);
                            proteinMap.Add(lineNumber, prtParser.getRecord(line));

                            break;
                        case 4:
                            if (pehParser != null){
                                // header line only display once!
                                error = new MZTabError(LogicalErrorType.HeaderLine, lineNumber,
                                                       MZTabFileParser.subString(line));
                                throw new MZTabException(error);
                            }

                            if (mtdParser.Metadata.MzTabType == MzTabType.Identification){
                                errorList.Add(new MZTabError(LogicalErrorType.PeptideSection, lineNumber,
                                                             MZTabFileParser.subString(line)));
                            }

                            // peptide header section
                            pehParser = new PEHLineParser(mtdParser.Metadata);
                            pehParser.Parse(lineNumber, line, errorList);
                            pepPositionMapping = new PositionMapping(pehParser.getFactory(), line);

                            // tell system to continue check peptide data line.
                            highWaterMark = 5;
                            break;
                        case 5:
                            if (pehParser == null){
                                // header line should be check first.
                                error = new MZTabError(LogicalErrorType.NoHeaderLine, lineNumber,
                                                       MZTabFileParser.subString(line));
                                throw new MZTabException(error);
                            }

                            if (pepParser == null){
                                pepParser = new PEPLineParser(pehParser.getFactory(), pepPositionMapping,
                                                              mtdParser.Metadata,
                                                              errorList);
                            }
                            pepParser.Parse(lineNumber, line, errorList);
                            peptideMap.Add(lineNumber, pepParser.getRecord(line));

                            break;
                        case 6:
                            if (pshParser != null){
                                // header line only display once!
                                error = new MZTabError(LogicalErrorType.HeaderLine, lineNumber,
                                                       MZTabFileParser.subString(line));
                                throw new MZTabException(error);
                            }

                            // psm header section
                            pshParser = new PSHLineParser(mtdParser.Metadata);
                            pshParser.Parse(lineNumber, line, errorList);
                            psmPositionMapping = new PositionMapping(pshParser.getFactory(), line);

                            // tell system to continue check peptide data line.
                            highWaterMark = 7;
                            break;
                        case 7:
                            if (pshParser == null){
                                // header line should be check first.
                                error = new MZTabError(LogicalErrorType.NoHeaderLine, lineNumber,
                                                       MZTabFileParser.subString(line));
                                throw new MZTabException(error);
                            }

                            if (psmParser == null){
                                psmParser = new PSMLineParser(pshParser.getFactory(), psmPositionMapping,
                                                              mtdParser.Metadata,
                                                              errorList);
                            }
                            psmParser.Parse(lineNumber, line, errorList);
                            psmMap.Add(lineNumber, psmParser.getRecord(line));

                            break;
                        case 8:
                            if (smhParser != null){
                                // header line only display once!
                                error = new MZTabError(LogicalErrorType.HeaderLine, lineNumber,
                                                       MZTabFileParser.subString(line));
                                throw new MZTabException(error);
                            }

                            // small molecule header section
                            smhParser = new SMHLineParser(mtdParser.Metadata);
                            smhParser.Parse(lineNumber, line, errorList);
                            smlPositionMapping = new PositionMapping(smhParser.getFactory(), line);

                            // tell system to continue check small molecule data line.
                            highWaterMark = 9;
                            break;
                        case 9:
                            if (smhParser == null){
                                // header line should be check first.
                                error = new MZTabError(LogicalErrorType.NoHeaderLine, lineNumber,
                                                       MZTabFileParser.subString(line));
                                throw new MZTabException(error);
                            }

                            if (smlParser == null){
                                smlParser = new SMLLineParser(smhParser.getFactory(), smlPositionMapping,
                                                              mtdParser.Metadata,
                                                              errorList);
                            }
                            smlParser.Parse(lineNumber, line, errorList);
                            smallMoleculeMap.Add(lineNumber, smlParser.getRecord(line));

                            break;
                    }
                }
            }
            catch (Exception e){
                errorList.Add(new ParserError(lineNumber, e.Message));
            }


            if (reader != null){
                reader.Close();
            }

            if (errorList.IsNullOrEmpty()){
                MZTabFile mzTabFile = new MZTabFile(mtdParser.Metadata);
                foreach (int id in commentMap.Keys){
                    mzTabFile.addComment(id, commentMap[id]);
                }

                if (prhParser != null){
                    MZTabColumnFactory proteinColumnFactory = prhParser.getFactory();
                    mzTabFile.setProteinColumnFactory(proteinColumnFactory);
                    foreach (int id in proteinMap.Keys){
                        mzTabFile.addProtein(id, proteinMap[id]);
                    }
                }

                if (pehParser != null){
                    MZTabColumnFactory peptideColumnFactory = pehParser.getFactory();
                    mzTabFile.setPeptideColumnFactory(peptideColumnFactory);
                    foreach (int id in peptideMap.Keys){
                        mzTabFile.addPeptide(id, peptideMap[id]);
                    }
                }

                if (pshParser != null){
                    MZTabColumnFactory psmColumnFactory = pshParser.getFactory();
                    mzTabFile.setPSMColumnFactory(psmColumnFactory);
                    foreach (int id in psmMap.Keys){
                        mzTabFile.addPSM(id, psmMap[id]);
                    }
                }

                if (smhParser != null){
                    MZTabColumnFactory smallMoleculeColumnFactory = smhParser.getFactory();
                    mzTabFile.setSmallMoleculeColumnFactory(smallMoleculeColumnFactory);
                    foreach (int id in smallMoleculeMap.Keys){
                        mzTabFile.addSmallMolecule(id, smallMoleculeMap[id]);
                    }
                }
            }
        }
    }

    public class ParserError : MZTabError{
        private readonly string _msg;

        public ParserError(int line, string msg): base(MZTabErrorType.createError(Category.Logical, "Exception"), line){
            _msg = msg;            
        }

        public override string ToString(){
            return _msg;
        }
    }
}