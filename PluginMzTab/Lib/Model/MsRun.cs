using System.Security.Policy;
using System.Text;

namespace PluginMzTab.Lib.Model{
/**
 * The external MS data file.
 *
 * User: Qingwei
 * Date: 23/05/13
 */

    public class MsRun : IndexedElement{
        /**
     * A parameter specifying the data format of the external MS data file.
     */
        private Param format;

        /**
     * Parameter specifying the id format used in the external data file.
     */
        private Param idFormat;

        /**
     * Parameter specifying the fragmentation method used in the external data file.
     */
        private Param fragementMethod;

        /**
     * Location of the external data file.
     */
        private Url location;

        public MsRun(int id) : base(MetadataElement.MS_RUN, id){}

        public Param Format { get { return format; } set { format = value; } }

        public Param IdFormat { get { return idFormat; } set { idFormat = value; } }

        public Param FragmentationMethod { get { return fragementMethod; } set { fragementMethod = value; } }

        public Url Location{
            get { return location; }
            set{
                location = value;
                if (location != null && location.Value != null && location.Value.Contains("\\")){
                    location = new Url(location.Value.Replace("\\", "/"));
                }
            }
        }

        /**
     * ms_file[1-n]-[format|location|id_format]   {CVParam|URL}
     */

        public override string ToString(){
            StringBuilder sb = new StringBuilder();

            if (format != null){
                sb.Append(printProperty(MetadataProperty.MS_RUN_FORMAT, format)).Append(MZTabConstants.NEW_LINE);
            }
            if (location != null){
                sb.Append(printProperty(MetadataProperty.MS_RUN_LOCATION, location.Value))
                  .Append(MZTabConstants.NEW_LINE);
            }
            if (idFormat != null){
                sb.Append(printProperty(MetadataProperty.MS_RUN_ID_FORMAT, idFormat)).Append(MZTabConstants.NEW_LINE);
            }
            if (fragementMethod != null){
                sb.Append(printProperty(MetadataProperty.MS_RUN_FRAGMENTATION_METHOD, fragementMethod))
                  .Append(MZTabConstants.NEW_LINE);
            }

            return sb.ToString();
        }
    }
}