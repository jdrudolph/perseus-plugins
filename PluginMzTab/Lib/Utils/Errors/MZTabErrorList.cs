using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PluginMzTab.Lib.Model;

namespace PluginMzTab.Lib.Utils.Errors{
    public class MZTabErrorList{
        private readonly List<MZTabError> _errorList = new List<MZTabError>();
        private readonly Level level;

        /**
         * Generate a error list, which max size is {@link MZTabProperties#MAX_ERROR_COUNT},
         * and only allow {@link MZTabErrorType.Level#Error} or greater level errors to be added
         * into list.
         */
        public MZTabErrorList() : this(Level.Error){}

        /**
         * Generate a error list, which max size is {@link MZTabProperties#MAX_ERROR_COUNT}
         *
         * @param level if null, default level is {@link MZTabErrorType.Level#Error}
         */

        public MZTabErrorList(Level level){
            this.level = level;
        }

        /**
         * A limit max capacity list, if contains a couple of {@link MZTabError} objects.
         * If overflow, system will raise {@link MZTabErrorOverflowException}. Besides this, during
         * add a new {@link MZTabError} object, it's {@link MZTabErrorType#level} SHOULD equal or
         * great than its level setting.
         *
         * @param error SHOULD NOT set null
         */

        public bool Add(MZTabError error){
            if (error == null){
                throw new NullReferenceException("Can not add a null error into list.");
            }

            if (error.Type.Level.CompareTo(level) < 0){
                return false;
            }

            if (_errorList.Count >= MZTabProperties.MAX_ERROR_COUNT){
                throw new MZTabErrorOverflowException();
            }

            if (_errorList.Any(x => x.Type == error.Type && x.Message == error.Message)){
                return false;
            }
            _errorList.Add(error);
            return true;
        }

        public void clear(){
            _errorList.Clear();
        }

        public int Size { get { return _errorList == null ? 0 : _errorList.Count; } }

        public MZTabError getError(int index){
            return _errorList[index];
        }

        public bool IsNullOrEmpty(){
            return _errorList == null || _errorList.Count == 0;
        }

        public void print(TextWriter stream){
            if (stream == null){
                throw new NullReferenceException("Output stream should be set first.");
            }
            foreach (MZTabError e in _errorList){
                stream.Write(e.ToString());
            }
        }

        /**
         * Print error list to string.
         */

        public override string ToString(){
            StringBuilder sb = new StringBuilder();

            foreach (MZTabError error in _errorList){
                sb.Append(error).Append(MZTabConstants.NEW_LINE);
            }

            return sb.ToString();
        }
    }
}