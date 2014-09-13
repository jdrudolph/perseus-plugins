using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BaseLib.Mol;
using BaseLib.Util;
using BaseLibS.Util;
using MzTabLibrary.model;

namespace PluginMzTab.utils{
    public class CVLookUp{
        private Dictionary<string, IList<ControlledVocabulary>> _controlledVocabularies;
        private Dictionary<string, ControlledVocabularyHeader> _controlledVocabularyHeaders;

        private Dictionary<string, IList<ControlledVocabulary>> ControlledVocabularies{
            get{
                if (_controlledVocabularies == null){
                    ReadFile();
                }
                return _controlledVocabularies;
            }
        }
        private Dictionary<string, ControlledVocabularyHeader> ControlledVocabularyHeaders{
            get{
                if (_controlledVocabularyHeaders == null){
                    ReadFile();
                }
                return _controlledVocabularyHeaders;
            }
        }

        public IList<ControlledVocabularyHeader> Headers { get { return new List<ControlledVocabularyHeader>(ControlledVocabularyHeaders.Values); } }

        private void ReadFile(){
            string confFolder = Path.Combine(FileUtils2.GetConfigPath(), "mztab");
            if (Directory.Exists(confFolder)){
                string file = Path.Combine(confFolder, "cvs.txt");
                _controlledVocabularies = new Dictionary<string, IList<ControlledVocabulary>>();
                _controlledVocabularyHeaders = new Dictionary<string, ControlledVocabularyHeader>();

                if (File.Exists(file)){
                    StreamReader reader = new StreamReader(File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read));
                    string line;
                    bool header = false;
                    while ((line = reader.ReadLine()) != null){
                        if (line.StartsWith("#headers")){
                            header = true;
                            continue;
                        }
                        if (line.StartsWith("#terms")){
                            header = false;
                            continue;
                        }

                        if (header){
                            ControlledVocabularyHeader o = ControlledVocabularyHeader.Parse(line);
                            if (o == null){
                                continue;
                            }
                            _controlledVocabularyHeaders.Add(o.Label, o);
                        }
                        else{
                            ControlledVocabulary o = ControlledVocabulary.Parse(line);
                            string key = o.CvLabel.ToUpper();
                            if (!_controlledVocabularies.ContainsKey(key)){
                                _controlledVocabularies.Add(key, new List<ControlledVocabulary>());
                            }
                            _controlledVocabularies[key].Add(o);
                        }
                    }
                    reader.Close();
                }

                try{
                    if (!_controlledVocabularies.ContainsKey("NEWT")){
                        _controlledVocabularies.Add("NEWT", new List<ControlledVocabulary>());
                    }
                    foreach (var db in Tables.Databases.Values){
                        if (string.IsNullOrEmpty(db.Species)){
                            continue;
                        }
                        var name = db.Species;
                        if (_controlledVocabularies["NEWT"].Any(x => x.Name == name)){
                            continue;
                        }
                        _controlledVocabularies["NEWT"].Add(new ControlledVocabulary(string.Format("NEWT:{0}", db.Taxid), name, "", new string[0], new string[0]));
                    }
                }
                catch (Exception){
                    Console.Out.WriteLine("Can not read the database.xml");
                }
            }
        }
        
        public string GetNameOfTerm(string key, string cvLabel){
            ControlledVocabulary result = GetCvOfTerm(key, cvLabel);
            if (result == null){
                return null;
            }

            return result.Name;
        }

        private ControlledVocabulary GetCvOfTerm(string key, string cvLabel){
            if (ControlledVocabularies == null){
                return null;
            }
            if (String.IsNullOrEmpty(key) || !ControlledVocabularies.ContainsKey(cvLabel)){
                return null;
            }

            //lookup key in ontologies
            return ControlledVocabularies[cvLabel].FirstOrDefault(x => x.Match(key));
        }

        public Param GetParam(string key, string cvLabel, string value = null){
            if (key == null){
                return null;
            }
            ControlledVocabulary result = GetCvOfTerm(key, cvLabel);

            //if nothing found in lookup tables use a UserParam
            if (result == null){
                return new UserParam(key, value);
            }

            return new CVParam(result.CvLabel, result.Accession, result.Name, value);
        }

        private IList<string> GetTermList(string cvLabel, bool name = true){
            if (cvLabel == null){
                return null;
            }
            List<string> result = new List<string>();
            if (ControlledVocabularies.ContainsKey(cvLabel)){
                result.AddRange(name
                                    ? ControlledVocabularies[cvLabel].Select(x => x.Name)
                                    : ControlledVocabularies[cvLabel].Select(x => x.Accession));
            }
            return result;
        }

        public IList<Param> GetParamsOfTerm(string term, string cvLabel){
            IList<string> terms = GetNamesOfTerm(term, cvLabel);
            if (terms == null){
                return null;
            }
            List<Param> result = new List<Param>();

            if (terms.Count > 0){
                result.AddRange(terms.Select(x => GetParam(x, cvLabel)));
            }
            return result;
        }

        public IList<string> GetNamesOfTerm(string term, string cvLabel){
            if (term == null || cvLabel == null){
                return null;
            }

            if (ControlledVocabularies.ContainsKey(cvLabel)){
                IList<ControlledVocabulary> sub = ControlledVocabularies[cvLabel].Where(x => x.IsA(term)).ToList();

                if (!sub.Any()){
                    return new List<string>();
                }

                return sub.Select(x => x.Name).ToList();
            }

            return new List<string>();
        }

        public List<Param> GetOnlyChildParamsOfTerm(string accession, string cvLabel){
            IList<string> terms = GetOnlyChildTermsOfTerm(accession, cvLabel);
            if (terms == null){
                return null;
            }
            List<Param> result = new List<Param>();
            if (terms.Count > 0){
                result.AddRange(terms.Select(x => GetParam(x, cvLabel)));
            }
            return result;
        }

        public List<string> GetOnlyChildNamesOfTerm(string accession, string cvLabel){
            IList<string> terms = GetOnlyChildTermsOfTerm(accession, cvLabel);
            if (terms == null){
                return null;
            }
            List<string> result = new List<string>();
            if (terms.Count > 0){
                result.AddRange(from term in terms
                                select ControlledVocabularies[cvLabel].FirstOrDefault(x => x.Accession.Equals(term))
                                into temp where temp != null select temp.Name);
            }
            return result;
        }

        private List<string> GetOnlyChildTermsOfTerm(string accession, string cvLabel){
            if (cvLabel == null || ControlledVocabularies == null){
                return null;
            }
            List<string> result = new List<string>();
            if (ControlledVocabularies.ContainsKey(cvLabel)){
                IList<ControlledVocabulary> sub = ControlledVocabularies[cvLabel].Where(x => x.IsA(accession)).ToList();
                if (!sub.Any()){
                    result.Add(accession);
                }
                else{
                    foreach (ControlledVocabulary ontology in sub){
                        result.AddRange(GetOnlyChildTermsOfTerm(ontology.Accession, cvLabel));
                    }
                }
            }
            return result;
        }

        public static IList<T> ExtractParamList<T>(SortedDictionary<int, T> map){
            if (map.Count == 0){
                return new List<T>();
            }
            return map.Keys.Select(x => map[x]).ToList();
        }

        public static IList<string> ExtractList(SortedDictionary<int, Param> map){
            if (map.Count == 0){
                return new List<string>();
            }
            return map.Keys.Select(x => map[x].Name).ToList();
        }

        public IList<string> GetSpecies(string cvLabel){
            return GetTermList(cvLabel);
        }

        public IList<string> GetTaxids(string cvLabel){
            return GetTermList(cvLabel, false);
        }

        public IList<string> GetDatabases(){
            return new List<string>{"UniprotKB"};
        }

        public IList<string> GetAllModificationsAsParam(string cvLabel){
            return GetTermList(cvLabel);
        }

        public IList<string> GetTissues(string cvLabel){
            return GetTermList(cvLabel);
        }

        public IList<string> GetCellTypes(string cvLabel){
            return GetTermList(cvLabel);
        }

        public IList<string> GetDiseases(string cvLabel){
            return GetTermList(cvLabel);
        }

        public IList<string> GetQuantReagents(){
            return new List<string>(GetOnlyChildNamesOfTerm("PRIDE:0000324", "PRIDE"));
        }

        public List<string> GetSampleProcessing(){
            List<string> result = new List<string>();
            result.AddRange(GetOnlyChildNamesOfTerm("sep:00101", "SEP"));
            //result.AddRange(GetTermList("MS"));
            return result;
        }

        public Param GetModificationParam(BaseLib.Mol.Modification mod){
            return new UserParam(mod.Name, string.Format("{0}{1}", mod.DeltaMass < 0 ? "+" : "-", mod.DeltaMass));
        }
    }
}