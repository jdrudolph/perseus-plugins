using System.Text;

namespace PluginMzTab.Lib.Model{
/**
 * CV parameter accessions MAY be used for optional columns following the format:
 * opt_cv_{param accession}_{parameter name}. Spaces within the parameter’s name MUST be replaced by ‘_’.
 *
 * User: Qingwei
 * Date: 30/05/13
 */

    public class CVParamOptionColumn : OptionColumn{
        private static readonly string CV = "cv_";

        private CVParam param;

        public CVParamOptionColumn(IndexedElement element, CVParam param, System.Type columnType, int offset)
            : base(element, CV + param.Accession + "_" + param.Name.Replace(" ", "_"), columnType, offset){
            this.param = param;
        }

        public static string GetHeader(IndexedElement element, CVParam param){
            StringBuilder sb = new StringBuilder();

            sb.Append(OPT).Append("_").Append(element == null ? GLOBAL : element.Reference);
            sb.Append("_").Append(CV).Append(param.Accession).Append("_").Append(param.Name.Replace(" ", "_"));

            return sb.ToString();
        }

        public CVParam Param { get { return param; } }
    }
}