using System.Text;

namespace PluginMzTab.Lib.Model{
/**
 * Additional columns can be added to the end of the protein table.
 * These column headers MUST start with the prefix “opt_”.
 * Column names MUST only contain the following characters:
 * ‘A’-‘Z’, ‘a’-‘z’, ‘0’-‘9’, ‘_’, ‘-’, ‘[’, ‘]’, and ‘:’.
 */

    public class OptionColumn : MZTabColumn{
        public static readonly string OPT = "opt";
        public static readonly string GLOBAL = "global";

        public static string GetHeader(IndexedElement element, string value){
            StringBuilder sb = new StringBuilder();

            sb.Append(OPT).Append("_").Append(element == null ? GLOBAL : element.Reference);
            sb.Append("_").Append(value.Replace(" ", "_"));

            return sb.ToString();
        }

        public OptionColumn(IndexedElement element, string value, System.Type columnType, int offset)
            : base(GetHeader(element, value), columnType, true, offset + 1 + ""){}
    }
}