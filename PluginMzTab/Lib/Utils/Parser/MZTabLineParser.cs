using PluginMzTab.Lib.Model;
using PluginMzTab.Lib.Utils.Errors;

namespace PluginMzTab.Lib.Utils.Parser{
    public class MZTabLineParser{
        protected int _lineNumber;

        protected Section _section;

        protected string _line;

        /**
        * based on MZTabConstants.TAB char to split raw line into string array.
        */
        protected string[] _items;

        protected MZTabErrorList _errorList;

        /**
        * We assume that user before call this method, have parse the raw line
        * is not empty line and start with section prefix.
        */

        protected void Parse(int lineNumber, string line, MZTabErrorList errorList){
            _lineNumber = lineNumber;
            _line = line;
            _errorList = errorList ?? new MZTabErrorList();

            _items = line.Split(MZTabConstants.TAB);
            _items[0] = _items[0].Trim();
            _items[_items.Length - 1] = _items[_items.Length - 1].Trim();

            _section = Section.findSection(_items[0]);

            if (_section == null){
                MZTabError error = new MZTabError(FormatErrorType.LinePrefix, lineNumber, _items[0]);
                _errorList.Add(error);
            }
        }
    }
}