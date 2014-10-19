using System;

namespace PluginMzTab.Plugin.Utils {
    public class Database {
        public string File { get; set; }
        public string Source { get; set; }
        public string Species { get; set; }
        public string Taxid { get; set; }
        public string Version { get; set; }
        public string SearchExpression { get; set; }
        public string Prefix { get; set; }


        public Database(string file, string version, string prefix, string source, string specie, string taxid, string expression) {
            if (file == null){
                throw new Exception("Database file is null!");
            }

            File = file;
            Prefix = prefix;
            Source = source;
            Species = specie;
            Taxid = taxid;
            SearchExpression = expression;
            Version = version;
        }

        public Database(string file, string version, string prefix):this(file, version, prefix, null, null, null, null){
            
        }
    }
}
