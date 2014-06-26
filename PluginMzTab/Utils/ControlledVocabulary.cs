using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using BaseLib.Util;

namespace PluginMzTab.utils{
    public class ControlledVocabulary{
        public static string Header { get { return StringUtils.Concat("\t", new[]{"accession", "name", "definition", "is a", "alias"}); } }

        private readonly List<string> _alias = new List<string>();
        private readonly List<string> _is_a = new List<string>();

        private string _cvLabel;
        public string CvLabel { get { return _cvLabel ?? (_cvLabel = Accession.Split(':')[0]); } }
        public string Accession { get; private set; }
        public string Name { get; private set; }
        public string Definition { get; set; }

        public static ControlledVocabulary Parse(string line){
            string[] items = line.Split('\t');

            ControlledVocabulary cv = new ControlledVocabulary{
                Accession = items[0],
                Name = items[1],
                Definition = items[2]
            };
            cv._is_a.AddRange(items[3].Split(';'));
            cv._alias.AddRange(items[4].Split(';'));

            return cv;
        }

        public ControlledVocabulary(string accession, string name, string def, IEnumerable<string> isA, IEnumerable<string> alias){
            Accession = accession;
            Name = name;
            Definition = def;
            if(isA != null) _is_a.AddRange(isA);
            if (alias != null) _alias.AddRange(alias);
        }

        protected ControlledVocabulary(){}

        public static ControlledVocabulary Parse(StreamReader reader){
            ControlledVocabulary controlledVocabulary = new ControlledVocabulary();
            Parse(reader, controlledVocabulary);

            if (controlledVocabulary.Accession == null || controlledVocabulary.Name == null){
                return null;
            }

            return controlledVocabulary;
        }

        public override string ToString(){
            IList<string> list = new List<string>();
            list.Add(Accession);
            list.Add(Name);
            list.Add(Definition);
            list.Add(_is_a.Count == 0 ? "" : StringUtils.Concat(";", _is_a));
            list.Add(_alias.Count == 0 ? "" : StringUtils.Concat(";", _alias));
            return StringUtils.Concat("\t", list);
        }

        private static void Parse(StreamReader reader, ControlledVocabulary controlledVocabulary){
            string term;
            while ((term = reader.ReadLine()) != null){
                if (String.IsNullOrEmpty(term)){
                    break;
                }
                if (term.StartsWith("id:")){
                    var e = term.Split(new[]{"id:"}, StringSplitOptions.RemoveEmptyEntries);
                    if (e.Length > 0){
                        controlledVocabulary.Accession = e[0].Trim();
                    }
                    continue;
                }
                if (term.StartsWith("name:")){
                    var e = term.Split(new[]{"name:"}, StringSplitOptions.RemoveEmptyEntries);
                    if (e.Length > 0){
                        controlledVocabulary.Name = e[0].Trim();
                    }
                }
                else if (term.StartsWith("def:") || (String.IsNullOrEmpty(controlledVocabulary.Definition) && !term.Contains(":"))){
                    var e = term.Split(new[]{"def:"}, StringSplitOptions.RemoveEmptyEntries);
                    if (e.Length > 0){
                        if (String.IsNullOrEmpty(controlledVocabulary.Definition)){
                            controlledVocabulary.Definition = e[0].Trim();
                        }
                        else{
                            controlledVocabulary.Definition += e[0].Trim();
                        }
                    }
                }
                /*else if (term.StartsWith("comment:") || (String.IsNullOrEmpty(controlledVocabulary.Definition) && !term.Contains(":"))){
                    var e = term.Split(new[]{"comment:"}, StringSplitOptions.RemoveEmptyEntries);
                    if (e.Length > 0){
                        if (String.IsNullOrEmpty(controlledVocabulary.Definition)){
                            controlledVocabulary.Definition = e[0].Trim();
                        }
                        else{
                            controlledVocabulary.Definition += e[0].Trim();
                        }
                    }
                }*/
                else if (term.StartsWith("is_a:")){
                    var e = term.Split(new[]{"is_a:", "!"}, StringSplitOptions.RemoveEmptyEntries);
                    if (e.Length > 0){
                        controlledVocabulary._is_a.Add(e[0].Trim());
                    }
                }
                else if (term.StartsWith("exact_synonym:") || term.StartsWith("synonym:")){
                    Regex regex = new Regex("\"(.*)\"");
                    if (regex.IsMatch(term)){
                        controlledVocabulary._alias.Add(regex.Match(term).Groups[1].Value);
                    }
                }
            }
        }

        public bool Match(string key){
            if (Name.Equals(key, StringComparison.CurrentCultureIgnoreCase)){
                return true;
            }
            if (Accession.Equals(key, StringComparison.CurrentCultureIgnoreCase)){
                return true;
            }
            return false;
        }

        public bool IsA(string term){
            return _is_a.Contains(term);
        }
    }
}