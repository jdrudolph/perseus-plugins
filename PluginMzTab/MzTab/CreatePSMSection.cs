using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using BaseLib.Param;
using BaseLib.Util;
using MzTabLibrary.model;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;
using PluginMzTab.utils;

namespace PluginMzTab.mztab{
    public class CreatePSMSection : CreateSection{
        private readonly MZTabColumnFactory _factory = MZTabColumnFactory.GetInstance(Section.PSM);

        public override string Name { get { return "Create PSM section"; } }
        public override bool IsActive { get { return true; } }
        public override bool HasButton { get { return false; } }
        public override Bitmap DisplayImage { get { return null; } }
        public override float DisplayRank { get { return 5; } }
        public override string Description { get { return helpDescription; } }
        public override string HelpOutput { get { return helpOutput; } }
        public override string[] HelpSupplTables { get { return null; } }
        public override int NumSupplTables { get { return 0; } }
        public override string[] HelpDocuments { get { return new[]{"Output"}; } }
        public override int NumDocuments { get { return 1; } }
        public override int MinNumInput { get { return 2; } }
        public override int MaxNumInput { get { return 2; } }

        public override string[] Tables { get { return new[] { Matrix.MetadataSection, Matrix.MsMs }; } }

        public override IMatrixData ProcessData(IMatrixData[] inputData, Parameters param, ref IMatrixData[] supplTables, ref IDocumentData[] documents, ProcessInfo processInfo){
            return ProcessData(inputData, ref documents, Matrix.MsMs, param, _factory, PSMColumn.ACCESSION.Name, Section.PSM_Header, Section.PSM, processInfo.Progress, processInfo.Status);
        }

        public override Parameters GetParameters(IMatrixData[] inputData, ref string errString){
            Metadata mtd = ParseMetadata(GetMatrixData(Matrix.MetadataSection, inputData));

            IList<ParameterDescription> help = ParameterDescription.Read();
            if (help != null){
                help = help.Where(x => x.Section.Equals(Section.Peptide)).ToArray();
            }

            IMatrixData mdata = GetMatrixData(Matrix.MsMs, inputData);

            IList<Parameter> list = new List<Parameter>();

            AddParameter(list, PSMColumn.SEQUENCE, mdata, new List<string>{PSMColumn.SEQUENCE.Name}, help, mtd);

            AddParameter(list, PSMColumn.PSM_ID, mdata, new List<string>{PSMColumn.PSM_ID.Name}, help, mtd);

            AddParameter(list, PSMColumn.ACCESSION, mdata, new List<string>{PSMColumn.ACCESSION.Name}, help, mtd);

            AddParameter(list, PSMColumn.UNIQUE, mdata, new List<string>{PSMColumn.UNIQUE.Name}, help, mtd);

            AddParameter(list, PSMColumn.DATABASE, mdata, new List<string>{PSMColumn.DATABASE.Name}, help, mtd);

            AddParameter(list, PSMColumn.DATABASE_VERSION, mdata,new List<string>{PSMColumn.DATABASE_VERSION.Name}, help, mtd);

            AddParameter(list, PSMColumn.SEARCH_ENGINE, mdata, new List<string>{PSMColumn.SEARCH_ENGINE.Name}, help, mtd);

            AddParameter(list, PSMColumn.SEARCH_ENGINE_SCORE, mdata, new List<string>{PSMColumn.SEARCH_ENGINE_SCORE.Name }, help, mtd);

            AddParameter(list, PSMColumn.RELIABILITY, mdata, new List<string>{PSMColumn.RELIABILITY.Name}, help, mtd);

            AddParameter(list, PSMColumn.MODIFICATIONS, mdata, new List<string>{PSMColumn.MODIFICATIONS.Name}, help, mtd);

            AddParameter(list, PSMColumn.RETENTION_TIME, mdata, new List<string>{PSMColumn.RETENTION_TIME.Name}, help, mtd);

            AddParameter(list, PSMColumn.CHARGE, mdata, new List<string>{PSMColumn.CHARGE.Name}, help, mtd);

            AddParameter(list, PSMColumn.EXP_MASS_TO_CHARGE, mdata, new List<string>{PSMColumn.EXP_MASS_TO_CHARGE.Name }, help, mtd);

            AddParameter(list, PSMColumn.CALC_MASS_TO_CHARGE, mdata, new List<string>{PSMColumn.CALC_MASS_TO_CHARGE.Name}, help, mtd);

            AddParameter(list, PSMColumn.URI, mdata, new List<string>{PSMColumn.URI.Name}, help, mtd);

            AddParameter(list, PSMColumn.SPECTRA_REF, mdata, new List<string>{PSMColumn.SPECTRA_REF.Name}, help, mtd);

            AddParameter(list, PSMColumn.PRE, mdata, new List<string>{PSMColumn.PRE.Name}, help, mtd);

            AddParameter(list, PSMColumn.POST, mdata, new List<string>{PSMColumn.POST.Name}, help, mtd);

            AddParameter(list, PSMColumn.START, mdata, new List<string>{PSMColumn.START.Name }, help, mtd);

            AddParameter(list, PSMColumn.END, mdata, new List<string>{PSMColumn.END.Name}, help, mtd);

            return new Parameters(list);
        }

        private const string helpDescription = "The PSM section is table-based. The PSM section MUST always come after the metadata section, peptide section and or protein section if they are present in the file. All table columns MUST be Tab separated. There MUST NOT be any empty cells. All columns, unless specified otherwise, are mandatory. The order of columns is not specified although for ease of human interpretation, it is RECOMMENDED to follow the order specified below. General information about mztab you can find on https://code.google.com/p/mztab/";

        private readonly string helpOutput = ParameterDescription.GetText(DocumentType.PlainText, Section.PSM);
    }
}