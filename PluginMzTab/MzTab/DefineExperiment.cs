using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using BaseLib.Param;
using BaseLibS.Util;
using MzTabLibrary.model;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;
using PluginMzTab.extended;
using PluginMzTab.param;

namespace PluginMzTab.mztab{
    public class DefineExperiment : CreateSection{
        public override string Name { get { return "Define Experiment"; } }

        public override bool IsActive { get { return true; } }
        public override bool HasButton { get { return false; } }
        public override Bitmap DisplayImage { get { return null; } }
        public override float DisplayRank { get { return 0; } }
        public override string Description { get { return null; } }
        public override string HelpOutput { get { return null; } }
        public override string[] HelpSupplTables { get { return null; } }
        public override int NumSupplTables { get { return 0; } }
        public override string[] HelpDocuments { get { return null; } }
        public override int NumDocuments { get { return 0; } }
        public override int MinNumInput { get { return 2; } }
        public override int MaxNumInput { get { return 2; } }

        public override string[] Tables { get { return new[]{Matrix.ExperimentalDesign, Matrix.Summary}; } }

        public override IMatrixData ProcessData(IMatrixData[] inputData, Parameters param, ref IMatrixData[] supplTables,
                                                ref IDocumentData[] documents, ProcessInfo processInfo){
            IList<MsRunImpl> runs = new List<MsRunImpl>();
            SingleChoiceWithSubParams singleSub =
                param.GetParam(MetadataElement.MS_RUN.Name) as SingleChoiceWithSubParams;
            if (singleSub != null){
                MsRunParam sub = singleSub.SubParams[singleSub.Value].GetAllParameters().FirstOrDefault() as MsRunParam;
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
                StudyVariableParam sub =
                    singleSub.SubParams[singleSub.Value].GetAllParameters().FirstOrDefault() as StudyVariableParam;
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
                SampleParam sub =
                    singleSub.SubParams[singleSub.Value].GetAllParameters().FirstOrDefault() as SampleParam;
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
                AssayParam sub = singleSub.SubParams[singleSub.Value].GetAllParameters().FirstOrDefault() as AssayParam;
                if (sub != null){
                    if (sub.Value != null){
                        foreach (Assay assay in sub.Value){
                            assays.Add(assay);
                        }
                    }
                }
            }

            IMatrixData output = (IMatrixData) inputData[0].CreateNewInstance(DataType.Matrix);
            List<string> columnnames = new List<string>{
                MetadataElement.STUDY_VARIABLE.Name,
                MetadataElement.ASSAY.Name,
                MetadataElement.MS_RUN.Name,
                MetadataElement.SAMPLE.Name
            };


            int count = assays.Count;

            List<string[]> matrix = new List<string[]>();
            for (int i = 0; i < columnnames.Count; i++){
                matrix.Add(new string[count]);
            }

            for (int i = 0; i < assays.Count; i++){
                Assay assay = assays[i];
                MsRunImpl runImpl = runs.FirstOrDefault(x => x.Id.Equals(assay.MsRun.Id));

                if (runImpl == null){
                    continue;
                }

                var studyVariable = i < studyVariables.Count ? studyVariables[i] : null;
                var sample = i < samples.Count ? samples[i] : null;
                foreach (var s in studyVariables){
                    if (s.AssayMap.ContainsKey(assay.Id)){
                        studyVariable = s;
                        try{
                            int sampleId = studyVariable.SampleMap.FirstOrDefault().Key;
                            sample = samples.FirstOrDefault(x => x.Id.Equals(sampleId));
                        }
                        catch (Exception){
                            Console.Error.WriteLine("Can not find sample");
                        }
                        break;
                    }
                }

                AddRow(matrix, columnnames, i, runImpl, assay, sample, studyVariable);
            }

            output.SetData(Matrix.Experiment, new List<string>(), new float[count,columnnames.Count], columnnames,
                           matrix,
                           new List<string>(), new List<string[][]>(), new List<string>(), new List<double[]>(),
                           new List<string>(), new List<double[][]>(), new List<string>(), new List<string[][]>(),
                           new List<string>(), new List<double[]>());


            return output;
        }

        private string GetParamListString(IList<Param> param){
            if (param == null){
                return null;
            }
            if (param.Count == 0){
                return null;
            }
            Param p = param.FirstOrDefault();
            if (p == null){
                return null;
            }
            return p.Name;
        }

        private void AddRow(List<string[]> matrix, List<string> columnnames, int row, MsRunImpl runImpl, Assay assay,
                            Sample sample, StudyVariable studyVariable){
            string value = runImpl == null
                               ? ""
                               : string.Format(@"{0} <{1};{2};{3};{4}>", runImpl.Description, runImpl.FilePath,
                                               runImpl.Format == null ? "" : runImpl.Format.Name,
                                               runImpl.IdFormat == null ? "" : runImpl.IdFormat.Name,
                                               runImpl.FragmentationMethod == null
                                                   ? ""
                                                   : runImpl.FragmentationMethod.Name);
            matrix[columnnames.IndexOf(MetadataElement.MS_RUN.Name)][row] = value;

            value = assay == null
                        ? ""
                        : string.Format(@"{0} <{1}>", assay.QuantificationReagent.Name,
                                        StringUtils.Concat(";", ConvertToString(assay.QuantificationModMap)));
            matrix[columnnames.IndexOf(MetadataElement.ASSAY.Name)][row] = value;

            value = sample == null
                        ? ""
                        : string.Format(@"{0} <{1};{2};{3};{4}>", sample.Description,
                                        GetParamListString(sample.SpeciesList), GetParamListString(sample.TissueList),
                                        GetParamListString(sample.CellTypeList), GetParamListString(sample.DiseaseList));
            matrix[columnnames.IndexOf(MetadataElement.SAMPLE.Name)][row] = value;

            matrix[columnnames.IndexOf(MetadataElement.STUDY_VARIABLE.Name)][row] = studyVariable == null
                                                                                        ? ""
                                                                                        : studyVariable.Description;
        }

        private IList<string> ConvertToString(SortedDictionary<int, AssayQuantificationMod> map){
            IList<string> result = new List<string>();
            if (map != null){
                foreach (var value in map.Values){
                    if (value != null && value.Param != null && value.Param.Name != null){
                        result.Add(value.Param.Name);
                    }
                }
            }
            return result;
        }

        public override Parameters GetParameters(IMatrixData[] inputData, ref string errString){
            IList<Parameter> list = new List<Parameter>();

            List<MsRunImpl> runs = new List<MsRunImpl>();
            List<StudyVariable> studyVariables = new List<StudyVariable>();
            List<Sample> samples = new List<Sample>();
            List<Assay> assays = new List<Assay>();


            IMatrixData summary = GetMatrixData(Matrix.Summary, inputData);
            IMatrixData experimentalDesign = GetMatrixData(Matrix.ExperimentalDesign, inputData);

            GetExperminetValues(summary, experimentalDesign, null, ref runs, ref studyVariables, ref assays, ref samples);


            AddSampleParameters(list, samples, null, true);

            AddStudyVariableParameters(list, studyVariables, null, true);

            AddMsRunParameters(list, runs, null, true);

            AddAssayParameters(list, assays, null, true);

            return new Parameters(list);
        }
    }
}