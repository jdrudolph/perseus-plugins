using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using PluginMzTab.Lib.Model;

namespace PluginMzTab.Lib.Utils.Errors{
    public class MZTabError{
        private readonly int _lineNumber;
        private readonly MZTabErrorType _type;
        private readonly string _message;

        public MZTabError(MZTabErrorType type, int lineNumber, IList<string> values){
            _type = type;
            _lineNumber = lineNumber;
            _message = Fill(1, values, type.Original);
        }

        public MZTabError(MZTabErrorType type, int lineNumber)
            : this(type, lineNumber, new List<string>()){}

        public MZTabError(MZTabErrorType type, int lineNumber, string value)
            : this(type, lineNumber, new List<string>{value}){}

        public MZTabError(MZTabErrorType type, int lineNumber, string value1, string value2)
            : this(type, lineNumber, new List<string>{value1, value2}){}

        public MZTabError(MZTabErrorType type, int lineNumber, string value1, string value2, string value3)
            : this(type, lineNumber, new List<string>{value1, value2, value3}){}

        /**
     * fill "{id}" parameter list one by one.
     */

        private string Fill(int count, IList<string> values, string msg){
            Regex regex = new Regex("\\{\\w\\}");

            if (values.Count == 0){
                return msg;
            }

            foreach (string value in values){
                if (regex.IsMatch(msg)){
                    msg = regex.Replace(msg, value, count);
                }
            }

            return msg;
        }

        public MZTabErrorType Type { get { return _type; } }

        public string Message { get { return _message; } }


        public override string ToString(){
            StringBuilder sb = new StringBuilder();

            sb.Append("[").Append(_type.Level).Append("-").Append(_type.Code).Append("] ");
            sb.Append("line ").Append(_lineNumber).Append(": ");
            sb.Append(_message).Append(MZTabConstants.NEW_LINE);

            return sb.ToString();
        }
    }
}