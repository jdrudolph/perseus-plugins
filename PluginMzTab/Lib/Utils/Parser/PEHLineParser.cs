using System;
using PluginMzTab.Lib.Model;
using PluginMzTab.Lib.Utils.Errors;

namespace PluginMzTab.Lib.Utils.Parser{
    /**
     * Parse and validate Peptide header line into a {@link MZTabColumnFactory}.
     */

    public class PEHLineParser : MZTabHeaderLineParser{
        public PEHLineParser(Metadata metadata) : base(MZTabColumnFactory.GetInstance(Section.Peptide_Header), metadata){}

        public new void Parse(int lineNumber, String line, MZTabErrorList errorList){
            base.Parse(lineNumber, line, errorList);
        }

        /**
     * In "Quantification" file, following optional columns are mandatory provide:
     * 1. peptide_abundance_study_variable[1-n]
     * 2. peptide_abundance_stdev_study_variable[1-n]
     * 3. peptide_abundance_std_error_study_variable[1-n]
     *
     * Beside above, in "Complete" and "Quantification" file, following optional columns also mandatory provide:
     * 1. search_engine_score_ms_run[1-n]
     * 2. peptide_abundance_assay[1-n]
     * 3. spectra_ref             // This is special, currently all "Quantification" file's peptide line header
     *                            // should provide, because it is difficult to judge MS2 based quantification employed.
     *
     * NOTICE: this hock method will be called at end of parse() function.
     *
     * @see MZTabHeaderLineParser#parse(int, String, uk.ac.ebi.pride.jmztab.utils.errors.MZTabErrorList)
     * @see #refineOptionalColumn(uk.ac.ebi.pride.jmztab.model.MZTabDescription.Mode, uk.ac.ebi.pride.jmztab.model.MZTabDescription.Type, String)
     */

        protected override void refine(){
            MzTabMode mode = metadata.MzTabMode;
            MzTabType type = metadata.MzTabType;

            if (mode == MzTabMode.Complete){
                if (type == MzTabType.Quantification){
                    foreach (MsRun msRun in metadata.MsRunMap.Values){
                        String msRunLabel = "_ms_run[" + msRun.Id + "]";
                        refineOptionalColumn(mode, type, "search_engine_score" + msRunLabel);
                        foreach (Assay assay in metadata.AssayMap.Values){
                            String assayLabel = "_assay[" + assay.Id + "]";
                            refineOptionalColumn(mode, type, "peptide_abundance" + assayLabel);
                        }
                    }
                }

                if (type == MzTabType.Quantification){
                    if (metadata.PeptideQuantificationUnit == null){
                        throw new MZTabException(new MZTabError(LogicalErrorType.NotDefineInMetadata, _lineNumber,
                                                                "peptide-quantification_unit", mode.ToString(),
                                                                type.ToString()));
                    }
                    foreach (StudyVariable studyVariable in metadata.StudyVariableMap.Values){
                        String svLabel = "_study_variable[" + studyVariable.Id + "]";
                        refineOptionalColumn(mode, type, "peptide_abundance" + svLabel);
                        refineOptionalColumn(mode, type, "peptide_abundance_stdev" + svLabel);
                        refineOptionalColumn(mode, type, "peptide_abundance_std_error" + svLabel);
                    }
                }
            }
        }
    }
}