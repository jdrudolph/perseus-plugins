using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using BaseLib.Param;
using BaseLibS.Util;
using MzTabLibrary.model;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;
using PluginMzTab.utils;

namespace PluginMzTab.mztab{
    public class CreateProteinSection : CreateSection{
        private readonly MZTabColumnFactory _factory = MZTabColumnFactory.GetInstance(Section.Protein);

        public override string Name { get { return "Create Protein section"; } }
        public override bool IsActive { get { return true; } }
        public override bool HasButton { get { return false; } }
        public override Bitmap DisplayImage { get { return null; } }
        public override float DisplayRank { get { return 3; } }
        public override string Description { get { return helpDescription; } }
        public override string HelpOutput { get { return helpOutput; } }
        public override string[] HelpSupplTables { get { return null; } }
        public override int NumSupplTables { get { return 0; } }
        public override string[] HelpDocuments { get { return new[]{"Output"}; } }
        public override int NumDocuments { get { return 1; } }
        public override int MinNumInput { get { return 2; } }
        public override int MaxNumInput { get { return 2; } }

        public override string[] Tables { get { return new[] { Matrix.MetadataSection, Matrix.ProteinGroups }; } }

        public override IMatrixData ProcessData(IMatrixData[] inputData, Parameters param, ref IMatrixData[] supplTables, ref IDocumentData[] documents, ProcessInfo processInfo){
            return ProcessData(inputData, ref documents, Matrix.ProteinGroups, param, _factory, ProteinColumn.ACCESSION.Name, Section.Protein_Header, Section.Protein, processInfo.Progress, processInfo.Status);
        }

        public override Parameters GetParameters(IMatrixData[] inputData, ref string errString){
            Metadata mtd = ParseMetadata(GetMatrixData(Matrix.MetadataSection, inputData));

            IList<ParameterDescription> help = ParameterDescription.Read();
            if (help != null){
                help = help.Where(x => x.Section.Equals(Section.Protein)).ToArray();
            }

            IMatrixData mdata = GetMatrixData(Matrix.ProteinGroups, inputData);

            IList<Parameter> list = new List<Parameter>();

            AddParameter(list, ProteinColumn.ACCESSION, mdata, new List<string>{ProteinColumn.ACCESSION.Name}, help, mtd);

            AddParameter(list, ProteinColumn.DESCRIPTION, mdata, new List<string>{ProteinColumn.DESCRIPTION.Name, "Protein names"}, help, mtd);

            AddParameter(list, ProteinColumn.TAXID, mdata, new List<string>{ProteinColumn.TAXID.Name}, help, mtd);

            AddParameter(list, ProteinColumn.SPECIES, mdata, new List<string>{ProteinColumn.SPECIES.Name}, help, mtd);

            AddParameter(list, ProteinColumn.DATABASE, mdata, new List<string>{ProteinColumn.DATABASE.Name}, help, mtd);

            AddParameter(list, ProteinColumn.DATABASE_VERSION, mdata, new List<string>{ProteinColumn.DATABASE_VERSION.Name}, help, mtd);

            AddParameter(list, ProteinColumn.SEARCH_ENGINE, mdata, new List<string>{ProteinColumn.SEARCH_ENGINE.Name}, help, mtd);

            AddParameter(list, ProteinColumn.BEST_SEARCH_ENGINE_SCORE, mdata, new List<string>{ProteinColumn.BEST_SEARCH_ENGINE_SCORE.Name}, help, mtd);

            AddParameter(list, ProteinColumn.RELIABILITY, mdata, new List<string>{ProteinColumn.RELIABILITY.Name}, help, mtd);

            AddParameter(list, ProteinColumn.AMBIGUITY_MEMBERS, mdata, new List<string>{ProteinColumn.AMBIGUITY_MEMBERS.Name}, help, mtd);

            AddParameter(list, ProteinColumn.MODIFICATIONS, mdata, new List<string>{ProteinColumn.MODIFICATIONS.Name}, help, mtd);

            AddParameter(list, ProteinColumn.URI, mdata, new List<string>{ProteinColumn.URI.Name}, help, mtd);

            AddParameter(list, ProteinColumn.GO_TERMS, mdata, new List<string>{ProteinColumn.GO_TERMS.Name}, help, mtd);

            AddParameter(list, ProteinColumn.PROTEIN_COVERAGE, mdata, new List<string>{ProteinColumn.PROTEIN_COVERAGE.Name}, help, mtd);

            Parameters parameters = new Parameters(list);

            //MSRUN specific       
            List<MZTabColumn> columns = new List<MZTabColumn>{
                ProteinColumn.SEARCH_ENGINE_SCORE,
                ProteinColumn.NUM_PSMS,
                ProteinColumn.NUM_PEPTIDES_DISTINCT,
                ProteinColumn.NUM_PEPTIDES_UNIQUE
            };

            foreach (var msRun in mtd.MsRunMap.Values.ToArray()){
                foreach (var column in columns){
                    _factory.AddOptionalColumn(column, msRun);
                }
            }

            IList<Parameter> group = new List<Parameter>();
            foreach (var col in columns){
                AddParameter(group, col, MetadataElement.MS_RUN, mdata, help, mtd);
            }
            parameters.AddParameterGroup(group, string.Format("{0}[1...{1}]", MetadataElement.MS_RUN.Name, mtd.MsRunMap.Count), false);

            #region ASSAY specific

            group = new List<Parameter>();
            Assay[] assays = mtd.AssayMap.Values.ToArray();
            foreach (Assay t in assays){
                _factory.AddAbundanceOptionalColumn(t);
            }

            var temp = _factory.ColumnMapping.Values.Where(x => x is AbundanceColumn && (x as AbundanceColumn).Element is Assay).ToList();
            columns = ArrayUtils.UniqueValues(temp.Select(x => x.Name).ToArray()).Select(name => temp.FirstOrDefault(x => x.Name.Equals(name))).ToList();

            AddOptionalColumns(_factory, columns, MetadataElement.ASSAY.Name);

            foreach (var col in columns){
                AddParameter(group, col, MetadataElement.ASSAY, mdata, help, mtd);
            }
            parameters.AddParameterGroup(group, string.Format("{0}[1...{1}]", MetadataElement.ASSAY.Name, assays.Length), false);

            #endregion


            #region STUDYVARIABLE specific

            group = new List<Parameter>();
            StudyVariable[] studyVariables = mtd.StudyVariableMap.Values.ToArray();
            foreach (StudyVariable t in studyVariables){
                _factory.AddAbundanceOptionalColumn(t);

                if (Constants.FirstColumnNameStartingWith(proteingroups.ratio_HL, mdata.NumericColumnNames) != null){
                    _factory.AddOptionalColumn(t, Constants.HeavyToLightRatio, typeof(double));
                }

                if (Constants.FirstColumnNameStartingWith(proteingroups.lfq_intensity, mdata.NumericColumnNames) != null){
                    _factory.AddOptionalColumn(t, Constants.LfqIntensity, typeof(double));
                }  
            }

            temp = _factory.ColumnMapping.Values.Where(x => x is AbundanceColumn && (x as AbundanceColumn).Element is StudyVariable).ToList();
            columns = ArrayUtils.UniqueValues(temp.Select(x => x.Name).ToArray()).Select(name => temp.FirstOrDefault(x => x.Name.Equals(name))).ToList();

            AddOptionalColumns(_factory, columns, MetadataElement.STUDY_VARIABLE.Name);

            foreach (var col in columns){
                AddParameter(group, col, MetadataElement.STUDY_VARIABLE, mdata, help, mtd);
            }

            parameters.AddParameterGroup(group, string.Format("{0}[1...{1}]", MetadataElement.STUDY_VARIABLE.Name, studyVariables.Length), false);

            #endregion

            return parameters;
        }

        private const string helpDescription =
            @"The protein section is table-based. The protein section MUST always come after the metadata section. All table columns MUST be tab-separated. There MUST NOT be any empty cells. Missing values MUST be reported using “null”. All columns are mandatory unless specified otherwise. The order of columns is not specified although for ease of human interpretation, it is RECOMMENDED to follow the order specified below.";

        private readonly string helpOutput = ParameterDescription.GetText(DocumentType.PlainText, Section.Protein);
    }
}