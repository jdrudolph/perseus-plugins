using System.Collections.Generic;
using BaseLibS.Util;
using MzTabLibrary.model;

namespace PluginMzTab.utils{
    public class ControlledVocabularyHeader : CV{
        public static string Header { get { return StringUtils.Concat("\t", new[]{"label", "fullname", "version", "url"}); } }

        public ControlledVocabularyHeader(string label) : base(1){
            Label = label;
        }
        
        public override string ToString(){
            IList<string> items = new List<string>();

            items.Add(Label);
            items.Add(FullName);
            items.Add(Version);
            items.Add(Url);

            return StringUtils.Concat("\t", items);
        }

        public static ControlledVocabularyHeader Parse(string line){
            if (string.IsNullOrEmpty(line)){
                return null;
            }
            string[] items = line.Split('\t');

            return new ControlledVocabularyHeader(items[0]){
                FullName = items[1],
                Version = items[2],
                Url = items[3]
            };
        }
    }
}