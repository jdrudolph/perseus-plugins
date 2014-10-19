using System.Text;

namespace PluginMzTab.Lib.Model{
    /**
	 * User: qingwei
	 * Date: 14/10/13
     */

    public abstract class Mod : IndexedElement{
        private Param param;
        private string site;
        private string position;

        public Mod(MetadataElement element, int id) : base(element, id){}

        public Param Param { get { return param; } set { param = value; } }

        public string Site { get { return site; } set { site = value; } }

        public string Position { get { return position; } set { position = value; } }

        public override string ToString(){
            StringBuilder sb = new StringBuilder();

            if (param != null){
                sb.Append(printElement(param)).Append(MZTabConstants.NEW_LINE);
            }

            if (!string.IsNullOrEmpty(site)){
                sb.Append(printProperty(MetadataProperty.FindProperty(Element, "site"), site))
                  .Append(MZTabConstants.NEW_LINE);
            }

            if (!string.IsNullOrEmpty(position)){
                sb.Append(printProperty(MetadataProperty.FindProperty(Element, "position"), position))
                  .Append(MZTabConstants.NEW_LINE);
            }

            return sb.ToString();
        }
    }
}