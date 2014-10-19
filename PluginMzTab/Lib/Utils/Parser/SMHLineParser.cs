using PluginMzTab.Lib.Model;
using PluginMzTab.Lib.Utils.Errors;

namespace PluginMzTab.Lib.Utils.Parser{
    /**
     * Parse and validate Small Molecule header line into a {@link MZTabColumnFactory}.
     */

    public class SMHLineParser : MZTabHeaderLineParser{
        public SMHLineParser(Metadata metadata)
            : base(MZTabColumnFactory.GetInstance(Section.Small_Molecule_Header), metadata){}

        public new void Parse(int lineNumber, string line, MZTabErrorList errorList){
            base.Parse(lineNumber, line, errorList);
        }

        /**
         * In "Quantification" file, following optional columns are mandatory provide:
         * 1. smallmolecule_abundance_study_variable[1-n]
         * 2. smallmolecule_abundance_stdev_study_variable[1-n]
         * 3. smallmolecule_abundance_std_error_study_variable[1-n]
         *
         * Beside above, in "Complete" and "Quantification" file, following optional columns also mandatory provide:
         * 1. search_engine_score_ms_run[1-n]
         *
         * NOTICE: this hock method will be called at end of parse() function.
         *
         * @see MZTabHeaderLineParser#parse(int, string, uk.ac.ebi.pride.jmztab.utils.errors.MZTabErrorList)
         * @see #refineOptionalColumn(uk.ac.ebi.pride.jmztab.model.MZTabDescription.Mode, uk.ac.ebi.pride.jmztab.model.MZTabDescription.Type, string)
         */

        protected override void refine(){
            MzTabMode mode = metadata.MzTabMode;
            MzTabType type = metadata.MzTabType;

            if (mode == MzTabMode.Complete){
                if (type == MzTabType.Quantification){
                    foreach (MsRun msRun in metadata.MsRunMap.Values){
                        string msRunLabel = "_ms_run[" + msRun.Id + "]";
                        refineOptionalColumn(mode, type, "search_engine_score" + msRunLabel);
                    }
                }
            }

            if (type == MzTabType.Quantification){
                if (metadata.SmallMoleculeQuantificationUnit == null){
                    throw new MZTabException(new MZTabError(LogicalErrorType.NotDefineInMetadata, _lineNumber,
                                                            "smallmolecule-quantification_unit", mode.ToString(),
                                                            type.ToString()));
                }
                foreach (StudyVariable studyVariable in metadata.StudyVariableMap.Values){
                    string svLabel = "_study_variable[" + studyVariable.Id + "]";
                    refineOptionalColumn(mode, type, "smallmolecule_abundance" + svLabel);
                    refineOptionalColumn(mode, type, "smallmolecule_abundance_stdev" + svLabel);
                    refineOptionalColumn(mode, type, "smallmolecule_abundance_std_error" + svLabel);
                }
            }
        }
    }
}