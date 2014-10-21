using System.Text;

namespace PluginMzTab.Lib.Model{

    public class CV : IndexedElement{
        private string label;
        private string fullName;
        private string version;
        private string url;

        public CV(int id) : base(MetadataElement.CV, id){}

        public string Label { get { return label; } set { label = value; } }

        public string FullName { get { return fullName; } set { fullName = value; } }

        public string Version { get { return version; } set { version = value; } }

        public string Url { get { return url; } set { url = value; } }

        public override string ToString(){
            StringBuilder sb = new StringBuilder();

            if (! string.IsNullOrEmpty(label)){
                sb.Append(printProperty(MetadataProperty.CV_LABEL, label)).Append(MZTabConstants.NEW_LINE);
            }
            if (!string.IsNullOrEmpty(fullName)){
                sb.Append(printProperty(MetadataProperty.CV_FULL_NAME, fullName)).Append(MZTabConstants.NEW_LINE);
            }
            if (!string.IsNullOrEmpty(version)){
                sb.Append(printProperty(MetadataProperty.CV_VERSION, version)).Append(MZTabConstants.NEW_LINE);
            }
            if (!string.IsNullOrEmpty(url)){
                sb.Append(printProperty(MetadataProperty.CV_URL, url)).Append(MZTabConstants.NEW_LINE);
            }

            return sb.ToString();
        }
    }
}