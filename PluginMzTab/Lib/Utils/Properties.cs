using System.Collections.Generic;
using System.IO;

namespace PluginMzTab.Lib.Utils{
    public class Properties{
        public Property Property { get; set; }

        public void load(StreamReader reader){
            if (Property == null){
                Property = new Property();
            }

            string line;
            while ((line = reader.ReadLine()) != null){
                if (string.IsNullOrEmpty(line) || line.StartsWith("#")){
                    continue;
                }
                string[] items = line.Split('=');
                if (items.Length == 2){
                    Property.Add(items[0].Trim(), items[1].Trim());
                }
            }
        }
    }

    public class Property : Dictionary<string, string>{}
}