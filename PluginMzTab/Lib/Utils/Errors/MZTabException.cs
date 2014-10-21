using System;

namespace PluginMzTab.Lib.Utils.Errors{
    public class MZTabException : Exception{
        private readonly MZTabError _error;

        public MZTabException(string message)
            : base(message){}

        public MZTabException(MZTabError error) : base(error.ToString()){
            _error = error;
        }

        public MZTabError Error { get { return _error; } }
    }
}