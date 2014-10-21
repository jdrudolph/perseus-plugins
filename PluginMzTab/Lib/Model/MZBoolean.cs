using System.Collections.Generic;

namespace PluginMzTab.Lib.Model{
/**
 * In mztab, using 0-false, 1-true to express the bool value.
 *
 * User: Qingwei
 * Date: 06/02/13
 */

    public class MZBoolean{
        public static MZBoolean True = new MZBoolean("1");
        public static MZBoolean False = new MZBoolean("0");

        private string value;

        /**
     * Boolean(0/1)
     */

        public MZBoolean(string value){
            this.value = value;
        }

        public override string ToString(){
            return value;
        }

        private static IList<string> trueTemplate = new[]{"1", "yes", "+", "true"};
        private static IList<string> falseTemplate = new[]{"0", "no", "-", "", "false"};

        public static MZBoolean FindBoolean(string booleanLabel){
            booleanLabel = booleanLabel.Trim();
            if (trueTemplate.Contains(booleanLabel)){
                return True;
            }
            if (falseTemplate.Contains(booleanLabel)){
                return False;
            }
            return null;
        }
    }
}