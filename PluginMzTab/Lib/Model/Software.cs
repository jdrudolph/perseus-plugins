using System.Collections.Generic;
using System.Text;

/**
 * SoftwareVersion used to analyze the data and obtain the results reported.
 * The parameter’s value SHOULD contain the software’s version.
 * The order (numbering) should reflect the order in which the tools were used.
 *
 * A software setting used. This field MAY occur multiple times for a single software.
 * The value of this field is deliberately set as a string, since there currently
 * do not exist cvParams for every possible setting
 */

namespace PluginMzTab.Lib.Model{
    public class Software : IndexedElement{
        private Param param;

        public Software(int id) : base(MetadataElement.SOFTWARE, id){}

        /**
     * A software setting used. This field MAY occur multiple times for a single software.
     * The value of this field is deliberately set as a string, since there currently
     * do not exist cvParams for every possible setting
     */
        private readonly List<string> settingList = new List<string>();

        public Param Param { get { return param; } set { param = value; } }

        public IList<string> SettingList { get { return settingList.AsReadOnly(); } }

        public void AddSetting(string setting){
            settingList.Add(setting);
        }

        /**
     * MTD  software[1]  [MS, MS:1001207, Mascot, 2.3]
     * MTD  software[1]-setting  Fragment tolerance = 0.1 Da
     * MTD  software[1]-setting  Parent tolerance = 0.5 Da
     */

        public override string ToString(){
            StringBuilder sb = new StringBuilder();

            if (param != null){
                sb.Append(printElement(param)).Append(MZTabConstants.NEW_LINE);
            }

            foreach (string setting in settingList){
                sb.Append(printProperty(MetadataProperty.SOFTWARE_SETTING, setting)).Append(MZTabConstants.NEW_LINE);
            }

            return sb.ToString();
        }
    }
}