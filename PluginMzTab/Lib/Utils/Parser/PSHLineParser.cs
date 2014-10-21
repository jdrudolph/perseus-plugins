using System;
using PluginMzTab.Lib.Model;
using PluginMzTab.Lib.Utils.Errors;

namespace PluginMzTab.Lib.Utils.Parser{
/**
 * Parse and validate PSM header line into a {@link MZTabColumnFactory}.
 */

    public class PSHLineParser : MZTabHeaderLineParser{
        public PSHLineParser(Metadata metadata) : base(MZTabColumnFactory.GetInstance(Section.PSM_Header), metadata){}

        public new void Parse(int lineNumber, String line, MZTabErrorList errorList){
            base.Parse(lineNumber, line, errorList);
        }

        /**
         * No optional columns defined in the PSM header line, so no refine check for it.
         */

        protected override void refine(){
            // if PSM section is present, fixed_mod[1-n] and variable_mod[1-n] should be defined.
            //        if (metadata.getFixedModMap().size() == 0) {
            //            throw new MZTabException(new MZTabError(LogicalErrorType.FixedMod, lineNumber));
            //        }
            //
            //        if (metadata.getVariableModMap().size() == 0) {
            //            throw new MZTabException(new MZTabError(LogicalErrorType.VariableMod, lineNumber));
            //        }
        }
    }
}