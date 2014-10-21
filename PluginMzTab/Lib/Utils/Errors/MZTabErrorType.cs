using System;

namespace PluginMzTab.Lib.Utils.Errors{
    public enum Category{
        Format, // single field format error
        Logical, // exists logical error among fields value.
        CrossCheck // multiple documents cross parse error.
    }

    public enum Level{
        Warn = 3,
        Error = 2,
        Info = 1
    }

    public class MZTabErrorType{
        private readonly int _code;
        private readonly Category _category;
        private readonly Level _level;
        private readonly string _original;
        private readonly string _cause;

        protected MZTabErrorType(){}

        private MZTabErrorType(int code, Category category, Level level, string original, string cause){
            _code = code;

            /*if (category == null){
                throw new NullReferenceException("MZTabErrorType category can not set null!");
            }*/
            _category = category;

            _level = level;

            if (original == null || original.Trim().Length == 0){
                throw new Exception("Original " + original + " is empty!");
            }
            _original = original.Trim();
            _cause = cause;
        }

        public static MZTabErrorType createError(Category category, string keyword){
            return createMZTabError(category, Level.Error, keyword);
        }

        public static MZTabErrorType createWarn(Category category, string keyword){
            return createMZTabError(category, Level.Warn, keyword);
        }

        public static MZTabErrorType createInfo(Category category, string keyword){
            return createMZTabError(category, Level.Info, keyword);
        }

        /**
         *  In *_error.properties file, code_{keyword}, original_{keyword}, cause+{keyword} have
         *  stable format. Thus, this method used to load these properties and create a error.
         */

        private static MZTabErrorType createMZTabError(Category category, Level level, string keyword){
            if (string.IsNullOrEmpty(keyword)){
                throw new NullReferenceException(keyword + " can not empty!");
            }

            string prefix = null;
            switch (category){
                case Category.Format:
                    prefix = "f_";
                    break;
                case Category.Logical:
                    prefix = "l_";
                    break;
                case Category.CrossCheck:
                    prefix = "c_";
                    break;
            }

            int code = MZTabProperties.getProperty(prefix + "code_" + keyword) == null
                           ? 0
                           : int.Parse(MZTabProperties.getProperty(prefix + "code_" + keyword));
            string original = MZTabProperties.getProperty(prefix + "original_" + keyword) ?? "Some text";
            string cause = MZTabProperties.getProperty(prefix + "cause_" + keyword) ?? "Some text";

            return new MZTabErrorType(code, category, level, original, cause);
        }

        public int Code { get { return _code; } }

        public Category Category { get { return _category; } }

        public Level Level { get { return _level; } }

        public string Original { get { return _original; } }

        public string Cause { get { return _cause; } }

        public override string ToString(){
            return "    Code:\t" + _code + "\r\n" +
                   "Category:\t" + _category + "\r\n" +
                   "Original:\t" + _original + "\r\n" +
                   "   Cause:\t" + (_cause ?? "") + "\r\n";
        }

        public static Level FindLevel(string target){
            Level level;
            try{
                level = (Level) Enum.Parse(typeof (Level), target);
            }
            catch (ArgumentException){
                level = Level.Info;
            }

            return level;
        }
    }
}