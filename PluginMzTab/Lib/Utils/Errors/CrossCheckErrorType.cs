namespace PluginMzTab.Lib.Utils.Errors{
    /**
     * Provide crosscheck service, that is parse the consistent between current mztab file and
     * some other resource (eg, database, xml file and so on).
     *
     * Not implement yet.
     *
     * User: Qingwei
     * Date: 29/01/13
     */

    public class CrossCheckErrorType : MZTabErrorType{
        public static MZTabErrorType Species = createWarn(Category.CrossCheck, "Species");
    }
}