using System;
using PluginMzTab.Lib.Model;
using PluginMzTab.Lib.Utils.Errors;

namespace PluginMzTab.Lib.Utils.Parser{
/**
 * Parse and validate Protein header line into a {@link MZTabColumnFactory}.
 */

    public class PRHLineParser : MZTabHeaderLineParser{
        public PRHLineParser(Metadata metadata) : base(MZTabColumnFactory.GetInstance(Section.Protein_Header), metadata){}

        public new void Parse(int lineNumber, String line, MZTabErrorList errorList){
            base.Parse(lineNumber, line, errorList);
        }

        /**
     * Principle 1: in "Quantification" file, following optional columns are mandatory provide:
     * 1. protein_abundance_study_variable[1-n]
     * 2. protein_abundance_stdev_study_variable[1-n]
     * 3. protein_abundance_std_error_study_variable[1-n]
     *
     * In "Complete" and "Identification" file, following optional columns also mandatory provide:
     * 1. search_engine_score_ms_run[1-n]
     * 2. num_psms_ms_run[1-n]
     * 3. num_peptides_distinct_ms_run[1-n]
     * 4. num_peptides_unique_ms_run[1-n]
     *
     * Beside principle 1, in "Complete" and "Quantification" file, following optional columns also mandatory provide:
     * 1. search_engine_score_ms_run[1-n]
     * 2. protein_abundance_assay[1-n]
     *
     * NOTICE: this hock method will be called at end of parse() function.
     *
     * @see MZTabHeaderLineParser#parse(int, String, uk.ac.ebi.pride.jmztab.utils.errors.MZTabErrorList)
     * @see #refineOptionalColumn(uk.ac.ebi.pride.jmztab.model.MZTabDescription.Mode, uk.ac.ebi.pride.jmztab.model.MzTabType, String)
     */

        protected override void refine(){
            MzTabMode mode = metadata.MzTabMode;
            MzTabType type = metadata.MzTabType;

            if (mode == MzTabMode.Complete){
                if (type == MzTabType.Identification){
                    foreach (MsRun msRun in metadata.MsRunMap.Values){
                        String msRunLabel = "_ms_run[" + msRun.Id + "]";
                        refineOptionalColumn(mode, type, "search_engine_score" + msRunLabel);
                        refineOptionalColumn(mode, type, "num_psms" + msRunLabel);
                        refineOptionalColumn(mode, type, "num_peptides_distinct" + msRunLabel);
                        refineOptionalColumn(mode, type, "num_peptides_unique" + msRunLabel);
                    }
                }
                else{
                    foreach (Assay assay in metadata.AssayMap.Values){
                        String assayLabel = "_assay[" + assay.Id + "]";
                        refineOptionalColumn(mode, type, "protein_abundance" + assayLabel);
                    }
                }
            }

            if (type == MzTabType.Quantification){
                if (metadata.ProteinQuantificationUnit == null){
                    throw new MZTabException(new MZTabError(LogicalErrorType.NotDefineInMetadata, _lineNumber,
                                                            "protein-quantification_unit", mode.ToString(),
                                                            type.ToString()));
                }
                foreach (StudyVariable studyVariable in metadata.StudyVariableMap.Values){
                    String svLabel = "_study_variable[" + studyVariable.Id + "]";
                    refineOptionalColumn(mode, type, "protein_abundance" + svLabel);
                    refineOptionalColumn(mode, type, "protein_abundance_stdev" + svLabel);
                    refineOptionalColumn(mode, type, "protein_abundance_std_error" + svLabel);
                }
            }
        }
    }
}