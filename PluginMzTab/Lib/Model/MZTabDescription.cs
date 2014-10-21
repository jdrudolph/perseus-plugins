using System.Text;

namespace PluginMzTab.Lib.Model{
    public enum MzTabMode{
        Complete,
        Summary
    }

    public enum MzTabType{
        Quantification,
        Identification
    }

    public class MZTabDescription{
        public static readonly string default_version = "1.0 rc5";

        private string version;
        private MzTabMode _mzTabMode;
        private MzTabType _mzTabType;
        private string id;

        public MZTabDescription(MzTabMode mzTabMode, MzTabType mzTabType) : this(null, mzTabMode, mzTabType){}

        public MZTabDescription(string version, MzTabMode mzTabMode, MzTabType mzTabType){
            this.version = version ?? default_version;

            _mzTabMode = mzTabMode;
            _mzTabType = mzTabType;
        }

        public string Version { get { return version; } set { version = value; } }

        public MzTabMode MzTabMode { get { return _mzTabMode; } set { _mzTabMode = value; } }

        public string Id { get { return id; } set { id = value; } }

        public MzTabType MzTabType { get { return _mzTabType; } set { _mzTabType = value; } }

        private StringBuilder printPrefix(StringBuilder sb){
            sb.Append(Section.Metadata.Prefix).Append(MZTabConstants.TAB);

            return sb;
        }

        public override string ToString(){
            StringBuilder sb = new StringBuilder();
            printPrefix(sb)
                .Append(MetadataElement.MZTAB)
                .Append(MZTabConstants.MINUS)
                .Append(MetadataProperty.MZTAB_VERSION)
                .Append(MZTabConstants.TAB)
                .Append(version)
                .Append(MZTabConstants.NEW_LINE);

            printPrefix(sb)
                .Append(MetadataElement.MZTAB)
                .Append(MZTabConstants.MINUS)
                .Append(MetadataProperty.MZTAB_MODE)
                .Append(MZTabConstants.TAB)
                .Append(_mzTabMode)
                .Append(MZTabConstants.NEW_LINE);

            printPrefix(sb)
                .Append(MetadataElement.MZTAB)
                .Append(MZTabConstants.MINUS)
                .Append(MetadataProperty.MZTAB_TYPE)
                .Append(MZTabConstants.TAB)
                .Append(_mzTabType)
                .Append(MZTabConstants.NEW_LINE);

            if (id != null){
                printPrefix(sb)
                    .Append(MetadataElement.MZTAB)
                    .Append(MZTabConstants.MINUS)
                    .Append(MetadataProperty.MZTAB_ID)
                    .Append(MZTabConstants.TAB)
                    .Append(id)
                    .Append(MZTabConstants.NEW_LINE);
            }

            return sb.ToString();
        }
    }
}