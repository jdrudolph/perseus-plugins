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
    public class CreatePeptideSection : CreateSection{
        private readonly MZTabColumnFactory _factory = MZTabColumnFactory.GetInstance(Section.Peptide);

        public override string Name { get { return "Create Peptide section"; } }
        public override bool IsActive { get { return true; } }
        public override bool HasButton { get { return false; } }
        public override Bitmap DisplayImage { get { return null; } }
        public override float DisplayRank { get { return 4; } }
        public override string Description { get { return helpDescription; } }
        public override string HelpOutput { get { return helpOutput; } }
        public override string[] HelpSupplTables { get { return null; } }
        public override int NumSupplTables { get { return 0; } }
        public override string[] HelpDocuments { get { return new[]{"Output"}; } }
        public override int NumDocuments { get { return 1; } }
        public override int MinNumInput { get { return 2; } }
        public override int MaxNumInput { get { return 2; } }

        public override string[] Tables { get { return new[] { Matrix.MetadataSection, Matrix.Peptides}; } }

        public override IMatrixData ProcessData(IMatrixData[] inputData, Parameters param, ref IMatrixData[] outputTables, ref IDocumentData[] documents, ProcessInfo processInfo){
            return ProcessData(inputData, ref documents, Matrix.Peptides, param, _factory, PeptideColumn.ACCESSION.Name, Section.Peptide_Header, Section.Peptide, processInfo.Progress, processInfo.Status);
        }

        public override Parameters GetParameters(IMatrixData[] inputData, ref string errString){
            Metadata mtd = ParseMetadata(GetMatrixData(Matrix.MetadataSection, inputData));

            IList<ParameterDescription> help = ParameterDescription.Read();
            if (help != null){
                help = help.Where(x => x.Section.Equals(Section.Peptide)).ToArray();
            }

            IMatrixData mdata = GetMatrixData(Matrix.Peptides, inputData);

            IList<Parameter> list = new List<Parameter>();
            
            AddParameter(list, PeptideColumn.SEQUENCE, mdata, new List<string>{PeptideColumn.SEQUENCE.Name}, help, mtd);

            AddParameter(list, PeptideColumn.ACCESSION, mdata, new List<string>{PeptideColumn.ACCESSION.Name}, help, mtd);

            AddParameter(list, PeptideColumn.UNIQUE, mdata, new List<string>{PeptideColumn.UNIQUE.Name}, help, mtd);

            AddParameter(list, PeptideColumn.DATABASE, mdata, new List<string>{PeptideColumn.DATABASE.Name}, help, mtd);

            AddParameter(list, PeptideColumn.DATABASE_VERSION, mdata, new List<string>{PeptideColumn.DATABASE_VERSION.Name}, help, mtd);

            AddParameter(list, PeptideColumn.SEARCH_ENGINE, mdata, new List<string>{PeptideColumn.SEARCH_ENGINE.Name}, help, mtd);

            AddParameter(list, PeptideColumn.BEST_SEARCH_ENGINE_SCORE, mdata, new List<string>{PeptideColumn.BEST_SEARCH_ENGINE_SCORE.Name}, help, mtd);

            AddParameter(list, PeptideColumn.RELIABILITY, mdata, new List<string>{PeptideColumn.RELIABILITY.Name}, help, mtd);

            AddParameter(list, PeptideColumn.MODIFICATIONS, mdata, new List<string>{PeptideColumn.MODIFICATIONS.Name}, help, mtd);

            AddParameter(list, PeptideColumn.RETENTION_TIME, mdata, new List<string>{PeptideColumn.RETENTION_TIME.Name}, help, mtd);

            AddParameter(list, PeptideColumn.RETENTION_TIME_WINDOW, mdata, new List<string>{PeptideColumn.RETENTION_TIME_WINDOW.Name}, help, mtd);

            AddParameter(list, PeptideColumn.CHARGE, mdata, new List<string>{PeptideColumn.CHARGE.Name }, help, mtd);

            AddParameter(list, PeptideColumn.MASS_TO_CHARGE, mdata, new List<string>{PeptideColumn.MASS_TO_CHARGE.Name }, help, mtd);

            AddParameter(list, PeptideColumn.URI, mdata, new List<string>{PeptideColumn.URI.Name }, help, mtd);

            AddParameter(list, PeptideColumn.SPECTRA_REF, mdata, new List<string>{PeptideColumn.SPECTRA_REF.Name}, help, mtd);

            Parameters parameters = new Parameters(list);

            //MSRUN specific
            IList<Parameter> group = new List<Parameter>();
            List<MZTabColumn> columns = new List<MZTabColumn>{
                PeptideColumn.SEARCH_ENGINE_SCORE
            };

            foreach (var msRun in mtd.MsRunMap.Values.ToArray()){
                foreach (var column in columns){
                    _factory.AddOptionalColumn(column, msRun);
                }
            }
            
            foreach (var col in columns){
                AddParameter(group, col, MetadataElement.MS_RUN, mdata, help, mtd);
            }
            parameters.AddParameterGroup(group, string.Format("{0}[1...{1}]", MetadataElement.MS_RUN.Name, mtd.MsRunMap.Count), false);

            //ASSAY specific
            group = new List<Parameter>();
            Assay[] assays = mtd.AssayMap.Values.ToArray();
            foreach (Assay t in assays){
                _factory.AddAbundanceOptionalColumn(t);
                _factory.AddOptionalColumn(t, "ratio_heavy_to_light", typeof (double));
            }

            var tmp = _factory.ColumnMapping.Values.Where(x => x is AbundanceColumn && (x as AbundanceColumn).Element is Assay).ToList();
            columns = ArrayUtils.UniqueValues(tmp.Select(x => x.Name).ToArray()) .Select(name => tmp.FirstOrDefault(x => x.Name.Equals(name))).ToList();

            AddOptionalColumns(_factory, columns, MetadataElement.ASSAY.Name);

            foreach (var col in columns){
                AddParameter(group, col, MetadataElement.ASSAY, mdata, help, mtd);
            }
            parameters.AddParameterGroup(group, string.Format("{0}[1...{1}]", MetadataElement.ASSAY.Name,  assays.Length), false);


            //STUDYVARIABLE specific
            group = new List<Parameter>();
            StudyVariable[] studyVariables = mtd.StudyVariableMap.Values.ToArray();
            foreach (StudyVariable t in studyVariables){
                _factory.AddAbundanceOptionalColumn(t);
                _factory.AddOptionalColumn(t, "ratio_heavy_to_light", typeof (double));
            }

            tmp = _factory.ColumnMapping.Values.Where(x => (x is AbundanceColumn && (x as AbundanceColumn).Element is StudyVariable)).ToList();
            columns = ArrayUtils.UniqueValues(tmp.Select(x => x.Name).ToArray()).Select(name => tmp.FirstOrDefault(x => x.Name.Equals(name))).ToList();

            AddOptionalColumns(_factory, columns, MetadataElement.STUDY_VARIABLE.Name);

            foreach (var col in columns){
                AddParameter(group, col, MetadataElement.STUDY_VARIABLE, mdata, help, mtd);
            }

            parameters.AddParameterGroup(group, string.Format("{0}[1...{1}]", MetadataElement.STUDY_VARIABLE.Name, studyVariables.Length), false);

            return parameters;
        }

        private const string helpDescription =
            "The peptide section is table based. The peptide section must always come after the metadata section and or protein section if these are present in the file. All table columns MUST be tab separated. There MUST NOT be any empty cells. All columns, unless specified otherwise, are mandatory. The order of columns is not specified although for ease of human interpretation, it is RECOMMENDED to follow the order specified below. General information about mztab you can find on https://code.google.com/p/mztab/";

        private readonly string helpOutput = ParameterDescription.GetText(DocumentType.PlainText, Section.Peptide);
    }
}