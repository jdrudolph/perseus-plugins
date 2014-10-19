using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using BaseLibS.Mol;
using BaseLibS.Util;
using PluginMzTab.Lib.Model;

namespace PluginMzTab.Plugin.Utils{
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
            string confFolder = Path.Combine(FileUtils.GetConfigPath(), "mztab");
            if (Directory.Exists(confFolder)){
                string file = Path.Combine(confFolder, "cvs.txt");
                _controlledVocabularies = new Dictionary<string, IList<ControlledVocabulary>>();
                _controlledVocabularyHeaders = new Dictionary<string, ControlledVocabularyHeader>();

                if (File.Exists(file)){
                    StreamReader reader =
                        new StreamReader(File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read));
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
                        _controlledVocabularies["NEWT"].Add(new ControlledVocabulary(
                                                                string.Format("NEWT:{0}", db.Taxid), name, "",
                                                                new string[0], new string[0]));
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

        public Lib.Model.Param GetParam(string key, string cvLabel, string value = null){
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

        public IList<Lib.Model.Param> GetParamsOfTerm(string term, string cvLabel){
            IList<string> terms = GetNamesOfTerm(term, cvLabel);
            if (terms == null){
                return null;
            }
            List<Lib.Model.Param> result = new List<Lib.Model.Param>();

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

        public List<Lib.Model.Param> GetOnlyChildParamsOfTerm(string accession, string cvLabel){
            IList<string> terms = GetOnlyChildTermsOfTerm(accession, cvLabel);
            if (terms == null){
                return null;
            }
            List<Lib.Model.Param> result = new List<Lib.Model.Param>();
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

        public static IList<string> ExtractList(SortedDictionary<int, Lib.Model.Param> map){
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

        public Lib.Model.Param GetModificationParam(BaseLibS.Mol.Modification mod){
            return new UserParam(mod.Name, string.Format("{0}{1}", mod.DeltaMass < 0 ? "+" : "-", mod.DeltaMass));
        }
    }

    public class ControlledVocabulary {
        public static string Header { get { return StringUtils.Concat("\t", new[] { "accession", "name", "definition", "is a", "alias" }); } }

        private readonly List<string> _alias = new List<string>();
        private readonly List<string> _is_a = new List<string>();

        private string _cvLabel;
        public string CvLabel { get { return _cvLabel ?? (_cvLabel = Accession.Split(':')[0]); } }
        public string Accession { get; private set; }
        public string Name { get; private set; }
        public string Definition { get; set; }

        public static ControlledVocabulary Parse(string line) {
            string[] items = line.Split('\t');

            ControlledVocabulary cv = new ControlledVocabulary {
                Accession = items[0],
                Name = items[1],
                Definition = items[2]
            };
            cv._is_a.AddRange(items[3].Split(';'));
            cv._alias.AddRange(items[4].Split(';'));

            return cv;
        }

        public ControlledVocabulary(string accession, string name, string def, IEnumerable<string> isA,
                                    IEnumerable<string> alias) {
            Accession = accession;
            Name = name;
            Definition = def;
            if (isA != null) _is_a.AddRange(isA);
            if (alias != null) _alias.AddRange(alias);
        }

        protected ControlledVocabulary() { }

        public static ControlledVocabulary Parse(StreamReader reader) {
            ControlledVocabulary controlledVocabulary = new ControlledVocabulary();
            Parse(reader, controlledVocabulary);

            if (controlledVocabulary.Accession == null || controlledVocabulary.Name == null) {
                return null;
            }

            return controlledVocabulary;
        }

        public override string ToString() {
            IList<string> list = new List<string>();
            list.Add(Accession);
            list.Add(Name);
            list.Add(Definition);
            list.Add(_is_a.Count == 0 ? "" : StringUtils.Concat(";", _is_a));
            list.Add(_alias.Count == 0 ? "" : StringUtils.Concat(";", _alias));
            return StringUtils.Concat("\t", list);
        }

        private static void Parse(StreamReader reader, ControlledVocabulary controlledVocabulary) {
            string term;
            while ((term = reader.ReadLine()) != null) {
                if (String.IsNullOrEmpty(term)) {
                    break;
                }
                if (term.StartsWith("id:")) {
                    var e = term.Split(new[] { "id:" }, StringSplitOptions.RemoveEmptyEntries);
                    if (e.Length > 0) {
                        controlledVocabulary.Accession = e[0].Trim();
                    }
                    continue;
                }
                if (term.StartsWith("name:")) {
                    var e = term.Split(new[] { "name:" }, StringSplitOptions.RemoveEmptyEntries);
                    if (e.Length > 0) {
                        controlledVocabulary.Name = e[0].Trim();
                    }
                } else if (term.StartsWith("def:") ||
                           (String.IsNullOrEmpty(controlledVocabulary.Definition) && !term.Contains(":"))) {
                    var e = term.Split(new[] { "def:" }, StringSplitOptions.RemoveEmptyEntries);
                    if (e.Length > 0) {
                        if (String.IsNullOrEmpty(controlledVocabulary.Definition)) {
                            controlledVocabulary.Definition = e[0].Trim();
                        } else {
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
                  else if (term.StartsWith("is_a:")) {
                    var e = term.Split(new[] { "is_a:", "!" }, StringSplitOptions.RemoveEmptyEntries);
                    if (e.Length > 0) {
                        controlledVocabulary._is_a.Add(e[0].Trim());
                    }
                } else if (term.StartsWith("exact_synonym:") || term.StartsWith("synonym:")) {
                    Regex regex = new Regex("\"(.*)\"");
                    if (regex.IsMatch(term)) {
                        controlledVocabulary._alias.Add(regex.Match(term).Groups[1].Value);
                    }
                }
            }
        }

        public bool Match(string key) {
            if (Name.Equals(key, StringComparison.CurrentCultureIgnoreCase)) {
                return true;
            }
            if (Accession.Equals(key, StringComparison.CurrentCultureIgnoreCase)) {
                return true;
            }
            return false;
        }

        public bool IsA(string term) {
            return _is_a.Contains(term);
        }
    }

    public class ControlledVocabularyHeader : CV {
        public static string Header { get { return StringUtils.Concat("\t", new[] { "label", "fullname", "version", "url" }); } }

        public ControlledVocabularyHeader(string label)
            : base(1) {
            Label = label;
        }

        public override string ToString() {
            IList<string> items = new List<string>();

            items.Add(Label);
            items.Add(FullName);
            items.Add(Version);
            items.Add(Url);

            return StringUtils.Concat("\t", items);
        }

        public static ControlledVocabularyHeader Parse(string line) {
            if (string.IsNullOrEmpty(line)) {
                return null;
            }
            string[] items = line.Split('\t');

            return new ControlledVocabularyHeader(items[0]) {
                FullName = items[1],
                Version = items[2],
                Url = items[3]
            };
        }
    }
}