using System;
using PluginMzTab.Lib.Model;
using PluginMzTab.Lib.Utils.Errors;

namespace PluginMzTab.Lib.Utils.Parser{
    /**
     * Comment line parser.
     * Comment lines can be placed anywhere in an mzTab file. These lines must start with the three-letter
     * code COM and are ignored by most parsers. Empty lines can also occur anywhere in an mzTab file and are ignored.
     *
     * @see MZTabLineParser
     */

    public class COMLineParser : MZTabLineParser{
        public new void Parse(int lineNumber, String line, MZTabErrorList errorList){
            base.Parse(lineNumber, line, errorList);
        }

        public Comment getComment(){
            String msg = _items.Length == 1 ? "" : _items[1];
            return new Comment(msg);
        }
    }
}