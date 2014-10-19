using System.Text;

namespace PluginMzTab.Lib.Model{

    public class AssayQuantificationMod : Mod{
        private readonly Assay assay;

        public AssayQuantificationMod(Assay assay, int id)
            : base(MetadataSubElement.ASSAY_QUANTIFICATION_MOD.Element, id){
            this.assay = assay;
        }

        /**
	     * assay[id]-element[id]
	     */

        protected internal override string Reference{
            get{
                StringBuilder sb = new StringBuilder();

                sb.Append(MetadataElement.ASSAY).Append("[").Append(assay.Id).Append("]").Append(MZTabConstants.MINUS);
                sb.Append(MetadataSubElement.ASSAY_QUANTIFICATION_MOD).Append("[").Append(Id).Append("]");

                return sb.ToString();
            }
        }

        public override string ToString(){
            StringBuilder sb = new StringBuilder();

            if (Param != null){
                printPrefix(sb)
                    .Append(Reference)
                    .Append(MZTabConstants.TAB)
                    .Append(Param)
                    .Append(MZTabConstants.NEW_LINE);
            }

            if (!string.IsNullOrEmpty(Site)){
                printPrefix(sb)
                    .Append(Reference)
                    .Append(MZTabConstants.MINUS)
                    .Append("site")
                    .Append(MZTabConstants.TAB)
                    .Append(Site)
                    .Append(MZTabConstants.NEW_LINE);
            }

            if (!string.IsNullOrEmpty(Position)){
                printPrefix(sb)
                    .Append(Reference)
                    .Append(MZTabConstants.MINUS)
                    .Append("position")
                    .Append(MZTabConstants.TAB)
                    .Append(Position)
                    .Append(MZTabConstants.NEW_LINE);
            }

            return sb.ToString();
        }
    }
}