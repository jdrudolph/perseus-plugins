using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using BaseLibS.Mol;
using BaseLibS.Util;
using PluginMzTab.Lib.Model;
using PluginMzTab.Lib.Utils.Errors;
using PluginMzTab.Lib.Utils.Parser;
using PluginMzTab.Plugin.Extended;
using PluginMzTab.Plugin.Utils;
using Modification = PluginMzTab.Lib.Model.Modification;

namespace PluginMzTab.Plugin{
    public class Converter{
        protected readonly CVLookUp cv = new CVLookUp();

        private readonly Metadata _mtd;
        
        private readonly string _proteinGroupsFile;
        private readonly string _proteinSectionFile;

        private readonly string _peptidesFile;
        private readonly string _peptideSectionFile;

        private readonly string _msmsFile;
        private readonly string _mztabFile;
        private readonly string _psmSectionFile;

        private readonly Dictionary<string, MsmsContent> _msms_map;
        private readonly Dictionary<string, PeptideContent> _peptide_map;
        private readonly Dictionary<string, ProteinContent> _protein_map;

        private IList<OptionalColumn> _optionalColumns;

        public Converter(Stream databaseTable, Stream peaklistTable, Stream metadataFile, string proteinGroupsFile,
                         string peptidesFile, string msmsFile, string outputFolder){
            _proteinGroupsFile = proteinGroupsFile;
            _proteinSectionFile = Path.Combine(outputFolder, "section_protein.txt");

            _peptidesFile = peptidesFile;
            _peptideSectionFile = Path.Combine(outputFolder, "section_peptide.txt");

            _msmsFile = msmsFile;
            _psmSectionFile = Path.Combine(outputFolder, "section_psm.txt");

            _mztabFile = Path.Combine(outputFolder, "mztab.txt");

            MZTabErrorList errorlist = new MZTabErrorList();
            _mtd = ParseMetadata(metadataFile, ref errorlist);
            
            _msms_map = ReadMsmsTable(_msmsFile, peaklistTable);
            _peptide_map = ReadPeptidesTable(_peptidesFile);
            _protein_map = ReadProteinGroupsTable(_proteinGroupsFile, databaseTable);
        }
        
        public Action<string> Status { get; set; }
        public Action<int> Progress { get; set; }

        public void Start(){
            CreateProteinSection();
            CreatePeptideSection();
            CreatePSMSection();

            if (File.Exists(_mztabFile)) {
                File.Delete(_mztabFile);
            }

            StreamWriter writer = new StreamWriter(_mztabFile);
            writer.WriteLine(_mtd.ToString());

            string[] files = new[] { _proteinSectionFile, _peptideSectionFile, _psmSectionFile };

            foreach (var file in files) {
                StreamReader reader = new StreamReader(file);
                string line;

                while ((line = reader.ReadLine()) != null) {
                    if (line.StartsWith("#")) {
                        continue;
                    }
                    writer.WriteLine(line);
                }

                writer.WriteLine();
                writer.Flush();

                reader.Close();
            }

            writer.Close();
        }

        public void CreateProteinSection(){
            

            MZTabColumnFactory[] factories = new[]
            {MZTabColumnFactory.GetInstance(Section.Protein_Header), MZTabColumnFactory.GetInstance(Section.Protein)};

            AddOptionalColumnsToColumnFactory(factories);

            SplitList<Lib.Model.Param> searchengine = new SplitList<Lib.Model.Param>();
            IList<Integer> searchengineIds = new List<Integer>();
            foreach (Software software in _mtd.SoftwareMap.Values){
                if (software.Param.Name == "Andromeda"){
                    searchengine.Add(new CVParam("MS", "MS:1002337", "Andromeda", ""));
                    Integer id = new Integer(software.Id);
                    foreach (var factory in factories){
                        factory.AddBestSearchEngineScoreOptionalColumn(ProteinColumn.BEST_SEARCH_ENGINE_SCORE, id);
                    }
                    searchengineIds.Add(id);
                }
            }

            IList<string> rawfiles = new List<string>();
            foreach (MsRun msrun in _mtd.MsRunMap.Values){
                string location = msrun.Location.Value;
                if (location.EndsWith(".raw")){
                    if (location.Contains('/')){
                        location = location.Substring(location.LastIndexOf('/') + 1);
                    }
                    if (location.Contains(".")){
                        location = location.Substring(0, location.IndexOf('.'));
                    }
                    rawfiles.Add(location);
                    foreach (Integer id in searchengineIds){
                        foreach (var factory in factories){
                            factory.AddSearchEngineScoreOptionalColumn(ProteinColumn.SEARCH_ENGINE_SCORE, id, msrun);
                        }
                    }

                    foreach (var factory in factories){
                        factory.AddOptionalColumn(ProteinColumn.NUM_PSMS, msrun);
                    }
                }
            }

            StreamWriter writer = new StreamWriter(_proteinSectionFile);
            Stream stream = new FileStream(_proteinGroupsFile, FileMode.Open);
            StreamReader reader = new StreamReader(stream);

            int progress = (int)(stream.Position * 100 / stream.Length);
            Update(string.Format("Create Protein section ({0}%)", progress), progress);
            
            string line = reader.ReadLine();

            string[] header = line.Split('\t');
                        
            writer.WriteLine(factories[0].ToString());

            while ((line = reader.ReadLine()) != null){
                if (line.StartsWith("#")){
                    continue;
                }
                Protein protein = new Protein(factories[1]);
                string[] elem = line.Split('\t');

                int index = Constants.GetKeywordIndex(proteingroups.accession, header);
                if (index != -1){
                    protein.Accession = elem[index].Split(';')[0].Trim();
                    if (elem[index].Contains(';')){
                        protein.AmbiguityMembers = new SplitList<string>();
                        protein.AmbiguityMembers.AddRange(
                            elem[index].Substring(elem[index].IndexOf(';'))
                                       .Split(new[]{";"}, StringSplitOptions.RemoveEmptyEntries));
                    }
                }

                index = Constants.GetKeywordIndex(proteingroups.description, header);
                if (index != -1){
                    protein.Description = elem[index];
                }

                index = Constants.GetKeywordIndex(proteingroups.id, header);
                if (index != -1){
                    string key = elem[index];
                    if (_protein_map.ContainsKey(key)){                        
                        if (_protein_map[elem[index]].Database != null) {
                            DatabaseContent content = _protein_map[elem[index]].Database.FirstOrDefault();
                            protein.Taxid = string.IsNullOrEmpty(content.Taxid) ? null : Integer.Parse(content.Taxid);
                            protein.Species = content.Specie;
                            protein.Database = content.Source;
                            protein.DatabaseVersion = content.Version;
                        }
                        protein.Modifications = _protein_map[elem[index]].Modifications;
                    }
                    else{
                        Console.Error.WriteLine("Cannot find in ProteinTable a entry for " + header[index] + "=" + key);
                    }
                }

                protein.SearchEngine = searchengine;

                index = Constants.GetKeywordIndex(proteingroups.msms_IDs, header);
                if (index != -1){
                    double[] scores = lookupBestScore(elem[index], rawfiles, _msms_map);

                    if (scores != null){
                        foreach (Integer id in searchengineIds){
                            protein.setBestSearchEngineScore(id, ArrayUtils.Max(scores));
                            for (int i = 0; i < scores.Length; i++){
                                protein.setSearchEngineScore(id, _mtd.MsRunMap[i + 1], scores[i]);
                            }
                        }
                    }
                    int[] psms = lookupNumberOfPSMs(elem[index], rawfiles, _msms_map);
                    if (psms != null){
                        for (int i = 0; i < psms.Length; i++){
                            protein.setNumPSMs(_mtd.MsRunMap[i + 1], new Integer(psms[i]));
                        }
                    }
                }

                index = Constants.GetKeywordIndex(proteingroups.coverage, header);
                if (index != -1){
                    protein.ProteinCoverage = double.Parse(elem[index])/100d;
                }

                SetOptionalColumns(protein, header, elem);

                writer.WriteLine(protein.ToString());
            }

            reader.Close();
            writer.Close();
        }

        public void CreatePeptideSection(){
            MZTabColumnFactory[] factories = new[]
            {MZTabColumnFactory.GetInstance(Section.Peptide_Header), MZTabColumnFactory.GetInstance(Section.Peptide)};

            AddOptionalColumnsToColumnFactory(factories);

            SplitList<Lib.Model.Param> searchengine = new SplitList<Lib.Model.Param>();
            IList<Integer> searchengineIds = new List<Integer>();
            foreach (Software software in _mtd.SoftwareMap.Values){
                if (software.Param.Name == "Andromeda"){
                    searchengine.Add(new CVParam("MS", "MS:1002337", "Andromeda", ""));
                    Integer id = new Integer(software.Id);
                    foreach (var factory in factories){
                        factory.AddBestSearchEngineScoreOptionalColumn(PeptideColumn.BEST_SEARCH_ENGINE_SCORE, id);
                    }
                    searchengineIds.Add(id);
                }
            }

            IList<string> rawfiles = new List<string>();
            foreach (MsRun msrun in _mtd.MsRunMap.Values){
                string location = msrun.Location.Value;
                if (location.EndsWith(".raw")){
                    if (location.Contains('/')){
                        location = location.Substring(location.LastIndexOf('/') + 1);
                    }
                    if (location.Contains(".")){
                        location = location.Substring(0, location.IndexOf('.'));
                    }
                    rawfiles.Add(location);
                    foreach (Integer id in searchengineIds){
                        foreach (var factory in factories){
                            factory.AddSearchEngineScoreOptionalColumn(PeptideColumn.SEARCH_ENGINE_SCORE, id, msrun);
                        }
                    }
                }
            }


            StreamWriter writer = new StreamWriter(_peptideSectionFile);
            Stream stream = new FileStream(_peptidesFile, FileMode.Open);
            StreamReader reader = new StreamReader(stream);

            int progress = (int)(stream.Position * 100 / stream.Length);
            Update(string.Format("Create Peptide section ({0}%)", progress), progress);

            string line = reader.ReadLine();

            string[] header = line.Split('\t');

            writer.WriteLine(factories[0].ToString());

            List<ProteinContent> proteinAccessions = new List<ProteinContent>();
            List<Integer> charges = new List<Integer>();

            while ((line = reader.ReadLine()) != null){
                if (line.StartsWith("#")){
                    continue;
                }
                proteinAccessions.Clear();
                charges.Clear();

                Peptide peptide = new Peptide(factories[1], _mtd);
                string[] elem = line.Split('\t');

                int index = Constants.GetKeywordIndex(peptides.sequence, header);
                if (index != -1){
                    peptide.Sequence = elem[index].Trim();
                }

                index = Constants.GetKeywordIndex(peptides.unique, header);
                if (index != -1){
                    peptide.Unique = new MZBoolean(elem[index] == "yes" ? "1" : "0");
                }

                index = Constants.GetKeywordIndex(peptides.proteinGroup_IDs, header);
                if (index != -1){
                    proteinAccessions.AddRange(elem[index].Split(';').Select(id => _protein_map[id]));
                }

                peptide.SearchEngine = searchengine;

                index = Constants.GetKeywordIndex(peptides.msms_IDs, header);
                if (index != -1){
                    double[] scores = lookupBestScore(elem[index], rawfiles, _msms_map);

                    if (scores != null){
                        foreach (Integer id in searchengineIds){
                            peptide.setBestSearchEngineScore(id, ArrayUtils.Max(scores));
                            for (int i = 0; i < scores.Length; i++){
                                peptide.setSearchEngineScore(id, _mtd.MsRunMap[i + 1], scores[i]);
                            }
                        }
                    }
                }

                index = Constants.GetKeywordIndex(peptides.id, header);
                if (index != -1){
                    peptide.Modifications = _peptide_map[elem[index]].Modifications;
                }

                index = Constants.GetKeywordIndex(peptides.msms_IDs, header);
                if (index != -1){
                    peptide.RetentionTime = new SplitList<double>();
                    peptide.RetentionTimeWindow = new SplitList<double>();
                    foreach (string id in elem[index].Split(new []{";"}, StringSplitOptions.RemoveEmptyEntries)){
                        if (!_msms_map.ContainsKey(id)){
                            Console.Error.WriteLine("Cannot find in Peptides table a entry for " + header[index] + "=" + id);
                            continue;
                        }                        
                        if (!double.IsNaN(_msms_map[id].RetentionTime)){
                            peptide.RetentionTime.Add(_msms_map[id].RetentionTime);
                        }
                        if (!double.IsNaN(_msms_map[id].RetentionTimeWindow)){
                            peptide.RetentionTimeWindow.Add(_msms_map[id].RetentionTimeWindow);
                        }
                        if (!double.IsNaN(_msms_map[id].MassToCharge)){
                            peptide.MassToCharge = _msms_map[id].MassToCharge;
                        }
                        peptide.SpectraRef = _msms_map[id].SpectraRef;
                    }
                }

                index = Constants.GetKeywordIndex(peptides.charges, header);
                if (index != -1){
                    charges.AddRange(
                        elem[index].Split(new[] { ";", "," }, StringSplitOptions.RemoveEmptyEntries).Select(Integer.Parse));
                }

                SetOptionalColumns(peptide, header, elem);

                foreach (ProteinContent content in proteinAccessions){
                    peptide.Accession = content.Accession;
                    foreach (Integer charge in charges) {
                        peptide.Charge = charge;
                        if (content.Database == null){
                            Console.Error.WriteLine("Cannot find Protein Accession "+ content.Accession +" in Database");
                            writer.WriteLine(peptide.ToString());
                        }
                        else{
                            foreach (DatabaseContent db in content.Database){
                                peptide.Database = db.Source;
                                peptide.DatabaseVersion = db.Version;
                                writer.WriteLine(peptide.ToString());
                            }
                        }
                    }
                }
            }

            reader.Close();
            writer.Close();
        }

        public void CreatePSMSection(){
            MZTabColumnFactory[] factories = new[] { MZTabColumnFactory.GetInstance(Section.PSM_Header), MZTabColumnFactory.GetInstance(Section.PSM) };

            SplitList<Lib.Model.Param> searchengine = new SplitList<Lib.Model.Param>();
            IList<Integer> searchengineIds = new List<Integer>();
            foreach (Software software in _mtd.SoftwareMap.Values){
                if (software.Param.Name == "Andromeda"){
                    searchengine.Add(new CVParam("MS", "MS:1002337", "Andromeda", ""));
                    Integer id = new Integer(software.Id);
                    searchengineIds.Add(id);

                    foreach (var factory in factories){
                        factory.AddSearchEngineScoreOptionalColumn(PSMColumn.SEARCH_ENGINE_SCORE, id, null);    
                    }
                }
            }

            StreamWriter writer = new StreamWriter(_psmSectionFile);
            Stream stream = new FileStream(_msmsFile, FileMode.Open);
            StreamReader reader = new StreamReader(stream);

            int progress = (int)(stream.Position * 100 / stream.Length);
            Update(string.Format("Create PSM section ({0}%)", progress), progress);

            string line = reader.ReadLine();
            string[] header = line.Split('\t');


            writer.WriteLine(factories[0].ToString());

            List<ProteinContent> proteinAccessions = new List<ProteinContent>();
            List<int> charges = new List<int>();

            while ((line = reader.ReadLine()) != null){
                if (line.StartsWith("#")){
                    continue;
                }
                proteinAccessions.Clear();
                charges.Clear();

                PSM psm = new PSM(factories[1], _mtd);
                string[] elem = line.Split('\t');

                int index = Constants.GetKeywordIndex(msms.sequence, header);
                if (index != -1){
                    psm.Sequence = elem[index].Trim();
                }

                index = Constants.GetKeywordIndex(msms.id, header);
                if (index != -1){
                    psm.PSM_ID = Integer.Parse(elem[index].Trim());
                }


                index = Constants.GetKeywordIndex(msms.proteinGroup_IDs, header);
                if (index != -1){
                    proteinAccessions.AddRange(elem[index].Split(';').Select(id => _protein_map[id]));
                }

                index = Constants.GetKeywordIndex(msms.peptide_ID, header);
                if (index != -1){
                    psm.Unique = new MZBoolean(_peptide_map[elem[index]].Unique == "yes" ? "1" : "0");
                    psm.Start = _peptide_map[elem[index]].Start;
                    psm.End = _peptide_map[elem[index]].Stop;
                    psm.Pre = _peptide_map[elem[index]].Pre;
                    psm.Post = _peptide_map[elem[index]].Post;
                }

                psm.SearchEngine = searchengine;

                index = Constants.GetKeywordIndex(msms.score, header);
                if (index != -1){
                    foreach (Integer id in searchengineIds){
                        psm.setSearchEngineScore(id, Double.Parse(elem[index]));
                    }
                }

                index = Constants.GetKeywordIndex(msms.id, header);
                if (index != -1){
                    psm.Modifications = _msms_map[elem[index]].Modifications;
                    psm.SpectraRef = _msms_map[elem[index]].SpectraRef;
                }

                index = Constants.GetKeywordIndex(msms.retentiontime, header);
                if (index != -1){
                    psm.RetentionTime = new SplitList<double>{Double.Parse(elem[index])};
                }

                index = Constants.GetKeywordIndex(msms.charge, header);
                if (index != -1){
                    psm.Charge = Integer.Parse(elem[index]);

                    if (psm.Charge != null){
                        index = Constants.GetKeywordIndex(msms.mass, header);
                        psm.CalcMassToCharge = Molecule.ConvertToMz(Double.Parse(elem[index]), psm.Charge.ToInt());
                    }
                    else{
                        Console.Error.WriteLine("Cannot parse charge of PSM " + elem[index]);
                    }
                }

                index = Constants.GetKeywordIndex(msms.mz, header);
                if (index != -1){
                    psm.ExpMassToCharge = Double.Parse(elem[index]);
                }


                foreach (ProteinContent protein in proteinAccessions){
                    psm.Accession = protein.Accession;
                    if (protein.Database != null){
                        foreach (DatabaseContent content in protein.Database){
                            psm.Database = content.Source;
                            psm.DatabaseVersion = content.Version;
                        }
                    }
                    writer.WriteLine(psm.ToString());
                }
            }

            reader.Close();
            writer.Close();
        }

        private void FindOptionalAbundanceColumnNames(string[] header, Enum column) {
            Dictionary<int, string> map = Constants.GetKeyAndValues(column, header);

            if (_mtd.StudyVariableMap != null && _mtd.StudyVariableMap.Count > 0) {
                foreach (var element in _mtd.StudyVariableMap.Values) {
                    foreach (int key in map.Keys) {
                        string val = map[key];
                        if (val.Equals(element.Description)) {
                            if (_optionalColumns == null) {
                                _optionalColumns = new List<OptionalColumn>();
                            }
                            _optionalColumns.Add(new OptionalColumn(element, header[key], null));
                            break;
                        }
                    }
                }
            }
        }

        private void FindOptionalColumnNames(string[] header, IEnumerable<Enum> selectedColumns) {
            foreach (var column in selectedColumns) {
                Dictionary<int, string> map = Constants.GetKeyAndValues(column, header);
                if (map == null) {
                    continue;
                }

                if (_mtd.StudyVariableMap != null && _mtd.StudyVariableMap.Count > 0) {
                    foreach (var element in _mtd.StudyVariableMap.Values) {
                        foreach (var key in map.Keys) {
                            string val = map[key];
                            string name = element.Description;
                            if (name.StartsWith("H ") || name.StartsWith("L ")) {
                                name = name.Substring(2).Trim();
                            }
                            if (val.Equals(name)) {
                                if (_optionalColumns == null) {
                                    _optionalColumns = new List<OptionalColumn>();
                                }
                                _optionalColumns.Add(new OptionalColumn(element, header[key],
                                                                        header[key].Replace(name, "")));
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void AddOptionalColumnsToColumnFactory(MZTabColumnFactory[] factories){
            if (_optionalColumns != null){
                foreach (OptionalColumn column in _optionalColumns){
                    if (column.Element is Assay){
                        foreach (var factory in factories){
                            if (column.Description == null){
                                factory.AddAbundanceOptionalColumn(column.Element as Assay);
                            }
                            else{
                                factory.AddOptionalColumn(column.Element as Assay, column.Description, typeof (double));
                            }
                        }
                    }
                    if (column.Element is StudyVariable){
                        foreach (var factory in factories){
                            if (column.Description == null){
                                factory.AddAbundanceOptionalColumn(column.Element as StudyVariable);
                            }
                            else{
                                factory.AddOptionalColumn(column.Element as StudyVariable, column.Description,
                                                          typeof (double));
                            }
                        }
                    }
                }
            }
        }

        private void SetOptionalColumns(MZTabRecord record, string[] header, string[] elem){
            if (_optionalColumns != null){
                foreach (OptionalColumn column in _optionalColumns){
                    if (column.ColumnName == null){
                        continue;
                    }

                    int index = ArrayUtils.IndexOf(header, column.ColumnName);
                    if (index != -1){
                        if (column.Element is StudyVariable){
                            if (column.Description == null){
                                record.setAbundanceColumn(column.Element as StudyVariable, elem[index]);
                            }
                            else{
                                record.setOptionColumn(column.Element as StudyVariable, column.Description, elem[index]);
                            }
                        }
                        else if (column.Element is Assay){
                            if (column.Description == null){
                                record.setAbundanceColumn(column.Element as Assay, elem[index]);
                            }
                            else{
                                record.setOptionColumn(column.Element as Assay, column.Description, elem[index]);
                            }
                        }
                    }
                }
            }
        }



        private Dictionary<string, ProteinContent> ReadProteinGroupsTable(string file, Stream databaseTable){
            Dictionary<string, IList<DatabaseContent>> database_map = ParseDatabaseTable(databaseTable);

            Dictionary<string, ProteinContent> result = new Dictionary<string, ProteinContent>();

            Stream stream = new FileStream(file, FileMode.Open);
            StreamReader reader = new StreamReader(stream);

            int progress = (int)(stream.Position * 100 / stream.Length);
            Update(string.Format("Read ProteinGroups file ({0}%)", progress), progress);


            string line = reader.ReadLine();

            string[] header = line.Split('\t');

            FindOptionalAbundanceColumnNames(header, proteingroups.intensity);
            FindOptionalColumnNames(header,
                                    new Enum[]
                                    {proteingroups.ratio_HL, proteingroups.ratio_HL_Norm, proteingroups.ratio_HL_Var});
            
            Dictionary<int, Modification> modifications = new Dictionary<int, Modification>();
            Regex regex = new Regex("(.*) [sS]+ite [pP]+ositions");
            for (int col = 0; col < header.Length; col++) {
                if (regex.IsMatch(header[col])) {
                    string name = regex.Match(header[col]).Groups[1].Value;
                    Modification modification = ConvertModificationToMzTab(Tables.Modifications[name],
                                                                           Section.Protein);
                    modifications.Add(col, modification);
                }
            }

            int id_index = Constants.GetKeywordIndex(proteingroups.id, header);
            if (id_index == -1){
                throw new Exception("Cannot find the specified column");
            }

            int accession_index = Constants.GetKeywordIndex(proteingroups.accession, header);

            while ((line = reader.ReadLine()) != null){
                progress = (int)(stream.Position * 100 / stream.Length);
                Update(string.Format("Read ProteinGroups file ({0}%)", progress), progress);

                if (line.StartsWith("#")){
                    continue;
                }

                string[] elem = line.Split('\t');


                string id = elem[id_index];

                if (result.ContainsKey(id)){
                    Console.Error.WriteLine("No unique identifier [id]");
                    continue;
                }

                result.Add(id, new ProteinContent());
                ProteinContent content = result[id];
                content.Accession = elem[accession_index].Split(';')[0];

                foreach (var key in modifications.Keys) {
                    if (string.IsNullOrEmpty(elem[key])) {
                        continue;
                    }
                    Modification mod = (Modification)modifications[key].Clone();

                    mod.PositionMap.Clear();
                    foreach (string position in elem[key].Split(';')) {
                        mod.AddPosition(int.Parse(position), null);
                    }                    
                    content.AddModification(mod);
                }

                if (database_map.ContainsKey(content.Accession)){
                    content.Database = database_map[content.Accession];
                }else if (content.Accession.StartsWith("REV__")){
                    string accession = content.Accession.Replace("REV__", "");
                    if (database_map.ContainsKey(accession)){
                        content.Database = new List<DatabaseContent>{new DatabaseContent{Source = "Reverse"}};
                        database_map.Add(content.Accession, content.Database);
                    }
                    else{
                        Console.Error.WriteLine("Could not find reverse identifier in database map: " + content.Accession);    
                    }
                }
                else{
                    Console.Error.WriteLine("Could not find identifier in database map: " + content.Accession);
                }
            }

            reader.Close();
            stream.Close();

            return result;
        }

        private Dictionary<string, PeptideContent> ReadPeptidesTable(string peptidesFile){
            Dictionary<string, PeptideContent> result = new Dictionary<string, PeptideContent>();

            Stream stream  = new FileStream(peptidesFile, FileMode.Open);
            StreamReader reader = new StreamReader(stream);

            int progress = (int)(stream.Position * 100 / stream.Length);
            Update(string.Format("Read Peptides file ({0}%)", progress), progress);

            string line = reader.ReadLine();

            string[] header = line.Split('\t');

            Regex regex = new Regex("(.*) [sS]+ite [iI]+[dD]s");
            Dictionary<int, Modification> modifications = new Dictionary<int, Modification>();
            for (int col = 0; col < header.Length; col++) {
                if (regex.IsMatch(header[col])) {
                    string name = regex.Match(header[col]).Groups[1].Value;
                    Modification modification = ConvertModificationToMzTab(Tables.Modifications[name],
                                                                           Section.Protein);
                    modifications.Add(col, modification);
                }
            }

            int id_index = Constants.GetKeywordIndex(peptides.id, header);
            if (id_index == -1){
                throw new Exception("Cannot find the specified column");
            }

            int unique_index = Constants.GetKeywordIndex(peptides.unique, header);
            int start_index = Constants.GetKeywordIndex(peptides.start, header);
            int end_index = Constants.GetKeywordIndex(peptides.end, header);
            int pre_index = Constants.GetKeywordIndex(peptides.pre, header);
            int post_index = Constants.GetKeywordIndex(peptides.post, header);

            while ((line = reader.ReadLine()) != null){
                progress = (int)(stream.Position * 100 / stream.Length);
                Update(string.Format("Read Peptides file ({0}%)", progress), progress);

                if (line.StartsWith("#")){
                    continue;
                }

                string[] elem = line.Split('\t');


                string id = elem[id_index];

                if (result.ContainsKey(id)){
                    Console.Error.WriteLine("No unique identifier [id]");
                    continue;
                }

                result.Add(id, new PeptideContent());
                PeptideContent content = result[id];
                content.Unique = elem[unique_index];
                content.Start = elem[start_index];
                content.Stop = elem[end_index];
                content.Pre = elem[pre_index];
                content.Post = elem[post_index];

                foreach (var key in modifications.Keys) {
                    if (string.IsNullOrEmpty(elem[key])) {
                        continue;
                    }
                    
                    content.AddModification(modifications[key]);
                }
            }
            reader.Close();
            stream.Close();

            return result;
        }

        private Dictionary<string, MsmsContent> ReadMsmsTable(String file, Stream peaklistTable){
            SpectraRefMap spectraRef = ParseAndromedaPeakList(peaklistTable);

            Dictionary<string, MsmsContent> result = new Dictionary<string, MsmsContent>();

            Stream stream = new FileStream(file, FileMode.Open);
            StreamReader reader = new StreamReader(stream);

            int progress = (int)(stream.Position * 100 / stream.Length);
            Update(string.Format("Read MS/MS file ({0}%)", progress), progress);

            string line = reader.ReadLine();

            string[] header = line.Split('\t');

            int id_index = Constants.GetKeywordIndex(msms.id, header);
            if (id_index == -1){
                throw new Exception("Cannot find the specified column");
            }
            int rawfile_index = Constants.GetKeywordIndex(msms.rawfile, header);
            int scannumber_index = Constants.GetKeywordIndex(msms.scannumber, header);
            int charge_index = Constants.GetKeywordIndex(msms.charge, header);
            int score_index = Constants.GetKeywordIndex(msms.score, header);
            int retention_index = Constants.GetKeywordIndex(msms.retentiontime, header);
            int retention_win_index = Constants.GetKeywordIndex(msms.retentiontimeWindow, header);
            int mass_to_charge_index = Constants.GetKeywordIndex(msms.mz, header);
            int mod_name_index = Constants.GetKeywordIndex(msms.modifications, header);
            int mod_seq_index = Constants.GetKeywordIndex(msms.mod_sequence, header);
            var mod_prop_index = Constants.GetKeyAndValues(msms.mod_probabilities, header);

            while ((line = reader.ReadLine()) != null){
                if (line.StartsWith("#")){
                    continue;
                }

                string[] elem = line.Split('\t');


                string id = elem[id_index];

                if (result.ContainsKey(id)){
                    Console.Error.WriteLine("No unique identifier [id]");
                    continue;
                }

                result.Add(id, new MsmsContent());
                MsmsContent content = result[id];
                content.Score = double.Parse(elem[score_index]);
                content.Rawfile = elem[rawfile_index];
                content.Charge = elem[charge_index];
                content.Scannumber = elem[scannumber_index];
                content.RetentionTime = double.Parse(elem[retention_index]);
                content.RetentionTimeWindow = retention_win_index == -1
                                                  ? double.NaN
                                                  : Double.Parse(elem[retention_win_index]);
                content.MassToCharge = mass_to_charge_index == -1
                                           ? double.NaN
                                           : Double.Parse(elem[mass_to_charge_index]);

                content.SpectraRef = spectraRef.GetSpectraRef(content.Rawfile, content.Charge, content.Scannumber);


                content.Modifications = ParseModifications(elem[mod_name_index],
                                                           elem[mod_seq_index],
                                                           mod_prop_index.Keys.ToDictionary(key => mod_prop_index[key],
                                                                                            key => elem[key]));
            }

            reader.Close();
            stream.Close();

            return result;
        }

        private double[] lookupBestScore(string msmsIDs, IList<string> rawfiles, Dictionary<string, MsmsContent> msmsMap){
            string[] items = msmsIDs.Split(';');

            double[] result = new double[rawfiles.Count];
            for (int i = 0; i < rawfiles.Count; i++){
                result[i] = double.NaN;
            }

            for (int i = 0; i < items.Length; i++){
                if (msmsMap.ContainsKey(items[i])){
                    MsmsContent content = msmsMap[items[i]];
                    int n = rawfiles.IndexOf(content.Rawfile);
                    result[n] = double.IsNaN(result[n]) ? content.Score : Math.Max(result[n], content.Score);
                }
            }


            return result;
        }

        private int[] lookupNumberOfPSMs(string msmsIDs, IList<string> rawfiles, Dictionary<string, MsmsContent> msmsMap){
            string[] items = msmsIDs.Split(';');

            int[] result = new int[rawfiles.Count];

            for (int i = 0; i < items.Length; i++){
                if (msmsMap.ContainsKey(items[i])){
                    MsmsContent content = msmsMap[items[i]];
                    int n = rawfiles.IndexOf(content.Rawfile);
                    result[n] += 1;
                }
            }

            return result;
        }

        public Modification ConvertModificationToMzTab(BaseLibS.Mol.Modification modification, Section section){
            Modification.ModificationType type = Modification.ModificationType.UNIMOD;
            string accession = modification.Unimod == null ? modification.Name : modification.Unimod; //TODO
            if (accession == null){
                type = Modification.ModificationType.UNKNOWN;
                accession = modification.Name;
            }

            return new Modification(section, type, accession);
        }

        private SplitList<Modification> ParseModifications(string modColumn, string sequence,
                                                                  Dictionary<string, string> probabilityColumns){
            Regex num = new Regex(@"([\d]*)(.+)");
            Regex regex = new Regex(@"\([^)]*\)");

            if (modColumn == "Unmodified"){
                return null;
            }

            Dictionary<string, List<int>> positions = new Dictionary<string, List<int>>();
            while (regex.IsMatch(sequence)){
                Match match = regex.Match(sequence);

                string abbreviation = sequence.Substring(match.Index + 1, match.Length - 2);
                int position = match.Index - 1;

                if (!positions.ContainsKey(abbreviation)){
                    positions.Add(abbreviation, new List<int>());
                }

                positions[abbreviation].Add(position);

                sequence = sequence.Remove(match.Index, match.Length);
            }

            Dictionary<string, List<double>> probabilities = new Dictionary<string, List<double>>();
            Dictionary<string, List<int>> probPositions = new Dictionary<string, List<int>>();
            foreach (var key in probabilityColumns.Keys){
                string text = probabilityColumns[key];
                while (regex.IsMatch(text)){
                    Match match = regex.Match(text);

                    double prop;
                    double.TryParse(text.Substring(match.Index + 1, match.Length - 2), out prop);
                    int position = match.Index - 1;

                    if (!probabilities.ContainsKey(key)){
                        probabilities.Add(key, new List<double>());
                    }

                    if (!probPositions.ContainsKey(key)){
                        probPositions.Add(key, new List<int>());
                    }

                    probabilities[key].Add(prop);
                    probPositions[key].Add(position);

                    text = text.Remove(match.Index, match.Length);
                }
            }


            SplitList<Modification> result = new SplitList<Modification>();
            foreach (var mod in modColumn.Split(',')){
                Match match = num.Match(mod);
                int n = 1;
                if (!string.IsNullOrEmpty(match.Groups[1].Value)){
                    int.TryParse(match.Groups[1].Value, out n);
                }
                string title = match.Groups[2].Value;

                var m = Tables.ModificationList.FirstOrDefault(x => x.Name == title);

                if (m == null){
                    continue;
                }

                Modification modification = ConvertModificationToMzTab(m, Section.PSM);

                if (probabilities.ContainsKey(m.Name) && probPositions.ContainsKey(m.Name)){
                    List<double> prop = probabilities[m.Name];
                    List<int> pos = probPositions[m.Name];
                    for (int j = 0; j < prop.Count; j++){
                        modification.AddPosition(pos[j],
                                                 new CVParam("MS", "MS:1001876", "modifications probability",
                                                             prop[j].ToString(CultureInfo.InvariantCulture)));
                    }
                }
                else{
                    if (positions.ContainsKey(m.Abbreviation)){
                        foreach (var pos in positions[m.Abbreviation]){
                            modification.AddPosition(pos, null);
                        }
                    }
                }

                for (int j = 0; j < n; j++){
                    result.Add(modification);
                }
            }

            return result;
        }

        private SpectraRefMap ParseAndromedaPeakList(Stream peaklistTable){
            int progress = (int)(peaklistTable.Position * 100 / peaklistTable.Length);
            Update(string.Format("Create database reference map ({0}%)", progress), progress);

            SpectraRefMap result = new SpectraRefMap();

            StreamReader reader = new StreamReader(peaklistTable);

            String line = reader.ReadLine();
            string[] header = line.Split('\t');

            int rawfile_index = Constants.GetKeywordIndex(spectraRef.raw_file, header);
            int scannumber_index = Constants.GetKeywordIndex(spectraRef.scan_number, header);
            int location_index = Constants.GetKeywordIndex(spectraRef.location, header);
            int reference_index = Constants.GetKeywordIndex(spectraRef.index, header);
            int charge_index = Constants.GetKeywordIndex(spectraRef.charge, header);

            while ((line = reader.ReadLine()) != null){
                if (line.StartsWith("#")){
                    continue;
                }

                progress = (int)(peaklistTable.Position * 100 / peaklistTable.Length);
                Update(string.Format("Create spectra reference map ({0}%)", progress), progress);

                string[] items = line.Split('\t');
                string rawfile = items[rawfile_index];
                string scannumber = items[scannumber_index];
                string charge = items[charge_index];
                string location = items[location_index];
                string reference = "index=" + items[reference_index];

                if (!result.ContainsKey(rawfile)){
                    result.Add(rawfile, new Dictionary<string, Dictionary<string, SplitList<SpectraRef>>>());
                }

                if (!result[rawfile].ContainsKey(charge)){
                    result[rawfile].Add(charge, new Dictionary<string, SplitList<SpectraRef>>());
                }

                if (!result[rawfile][charge].ContainsKey(scannumber)){
                    result[rawfile][charge].Add(scannumber, new SplitList<SpectraRef>());
                }
                var msrun = _mtd.MsRunMap.Values.FirstOrDefault(x => x.Location.Value.EndsWith(location));
                if (msrun == null){
                    Console.Error.WriteLine("The apl file was not defined in Metadata " + location);
                }
                else{
                    result[rawfile][charge][scannumber].Add(new SpectraRef(msrun, reference));
                }
            }

            return result;
        }

        private Dictionary<string, IList<DatabaseContent>> ParseDatabaseTable(Stream databaseTable){
            int progress = (int)(databaseTable.Position * 100 / databaseTable.Length);

            Update(string.Format("Create database reference map ({0}%)", progress), progress);

            Dictionary<string, IList<DatabaseContent>> result = new Dictionary<string, IList<DatabaseContent>>();
            StreamReader reader = new StreamReader(databaseTable);

            string line = reader.ReadLine();

            string[] header = line.Split('\t');

            int identifier_index = Constants.GetKeywordIndex(databaseRef.identifier, header);
            int source_index = Constants.GetKeywordIndex(databaseRef.source, header);
            int version_index = Constants.GetKeywordIndex(databaseRef.version, header);
            int specie_index = Constants.GetKeywordIndex(databaseRef.specie, header);
            int taxid_index = Constants.GetKeywordIndex(databaseRef.taxid, header);

            while ((line = reader.ReadLine()) != null){
                if (line.StartsWith("#")){
                    continue;
                }
                progress = (int) (databaseTable.Position*100/databaseTable.Length);
                Update(string.Format("Create database reference table ({0}%)", progress), progress);
                
                string[] elem = line.Split('\t');

                string id = elem[identifier_index];
                if (!result.ContainsKey(id)){
                    result.Add(id, new List<DatabaseContent>());
                }
                result[id].Add(new DatabaseContent{
                    Source = elem[source_index],
                    Version = elem[version_index],
                    Specie = elem[specie_index],
                    Taxid = elem[taxid_index]
                });
            }

            reader.Close();

            return result;
        }

        private void Update(string status, int progress){
            if (Status != null){
                Status(status);
            }
            if (Progress != null){
                Progress(progress);
            }
        }

        private Metadata ParseMetadata(Stream metadataFile, ref MZTabErrorList errorList) {
            int progress = (int)(metadataFile.Position * 100 / metadataFile.Length);
            Update(string.Format("Read metadata section ({0}%)", progress), progress);

            MTDLineParser parser = new MTDLineParser();

            if (errorList == null) {
                errorList = new MZTabErrorList();
            }
            
            StreamReader reader = new StreamReader(metadataFile);
            string line;
            int row = 0;
            while ((line = reader.ReadLine()) != null) {
                string[] items = line.Split('\t');
                row++;

                progress = (int)(metadataFile.Position * 100 / metadataFile.Length);
                Update(string.Format("Read metadata section ({0}%)", progress), progress);

                string mtdLine = StringUtils.Concat("\t", items);
                if (mtdLine.StartsWith("#") || mtdLine.StartsWith("MTH")) {
                    continue;
                }
                try {
                    parser.Parse(row, mtdLine, errorList);
                } catch (Exception e) {
                    Console.Error.WriteLine(e.StackTrace);
                }
                
            }
            reader.Close();

            Metadata mtd = parser.Metadata;
            var temp = new SortedDictionary<int, MsRun>();
            foreach (var key in mtd.MsRunMap.Keys) {
                MsRun value = mtd.MsRunMap[key];
                temp.Add(key, new MsRunImpl(value));
            }
            mtd.MsRunMap = temp;

            return mtd;
        }
    }

    internal class MsmsContent{
        public double Score { get; set; }
        public string Rawfile { get; set; }
        public double RetentionTime { get; set; }
        public double RetentionTimeWindow { get; set; }
        public double MassToCharge { get; set; }
        public string Scannumber { get; set; }
        public string Charge { get; set; }

        public SplitList<SpectraRef> SpectraRef { get; set; }
        public SplitList<Modification> Modifications { get; set; }
    }

    internal class ProteinContent{
        public string Accession { get; set; }
        public IList<DatabaseContent> Database { get; set; }
        public SplitList<Modification> Modifications { get; set; }

        public void AddModification(Modification mod){
            if (Modifications == null){
                Modifications = new SplitList<Modification>();
            }
            Modifications.Add(mod);
        }
    }

    internal class PeptideContent{
        public string Unique { get; set; }
        public string Start { get; set; }
        public string Stop { get; set; }
        public string Pre { get; set; }
        public string Post { get; set; }

        public SplitList<Modification> Modifications { get; set; }

        public void AddModification(Modification mod){
            if (Modifications == null) {
                Modifications = new SplitList<Modification>();
            }
            Modifications.Add(mod);
        }
    }

    internal class DatabaseContent{
        public string Source { get; set; }
        public string Version { get; set; }
        public string Specie { get; set; }
        public string Taxid { get; set; }        
    }

    internal class OptionalColumn{
        public IndexedElement Element { get; private set; }
        public string ColumnName { get; private set; }
        public string Description { get; private set; }

        public OptionalColumn(IndexedElement element, string columnName, string description){
            Description = FormatOptionalColumnName(description);
            ColumnName = columnName;
            Element = element;
        }

        public static string FormatOptionalColumnName(string value){
            if (value != null){
                return value.Trim().Replace("H/L", "heavy_to_light").Replace(' ', '_').Replace('/', '-').ToLower();
            }
            return value;
        }
    }

    internal class SpectraRefMap : Dictionary<string, Dictionary<string, Dictionary<string, SplitList<SpectraRef>>>>{
        public SplitList<SpectraRef> GetSpectraRef(string rawfile, string charge, string scannumber){
            if (ContainsKey(rawfile)){
                if (this[rawfile].ContainsKey(charge)){
                    if (this[rawfile][charge].ContainsKey(scannumber)){
                        return this[rawfile][charge][scannumber];
                    }
                }
            }
            return null;
        }
    }
}