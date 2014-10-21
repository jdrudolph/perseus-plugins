using System;

namespace PluginMzTab.Lib.Utils.Errors{
    public class MZTabErrorOverflowException : SystemException{
        /**
        * If error count great than {@link uk.ac.ebi.pride.jmztab.utils.MZTabProperties#MAX_ERROR_COUNT}
        * System will stop validate and throw overflow exception.
        */
        public MZTabErrorOverflowException(){}
    }
}