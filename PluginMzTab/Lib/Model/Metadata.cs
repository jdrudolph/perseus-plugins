using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;

namespace PluginMzTab.Lib.Model{
    /**
 * User: Qingwei
 * Date: 23/05/13
 */

    public class Metadata{
        private MZTabDescription _tabDescription;
        private string _title;
        private string _description;

        private SortedDictionary<int, SplitList<Param>> _sampleProcessingMap =
            new SortedDictionary<int, SplitList<Param>>();

        private SortedDictionary<int, Instrument> _instrumentMap = new SortedDictionary<int, Instrument>();
        private SortedDictionary<int, Software> _softwareMap = new SortedDictionary<int, Software>();
        private SplitList<Param> _falseDiscoveryRate = new SplitList<Param>(MZTabConstants.BAR);
        private SortedDictionary<int, Publication> _publicationMap = new SortedDictionary<int, Publication>();
        private SortedDictionary<int, Contact> _contactMap = new SortedDictionary<int, Contact>();
        private List<Uri> _uriList = new List<Uri>();
        private SortedDictionary<int, FixedMod> _fixedModMap = new SortedDictionary<int, FixedMod>();
        private SortedDictionary<int, VariableMod> _variableModMap = new SortedDictionary<int, VariableMod>();
        private Param _quantificationMethod;
        private Param _proteinQuantificationUnit;
        private Param _peptideQuantificationUnit;
        private Param _smallMoleculeQuantificationUnit;
        private SortedDictionary<int, MsRun> _msRunMap = new SortedDictionary<int, MsRun>();
        private List<Param> _customList = new List<Param>();
        private SortedDictionary<int, Sample> _sampleMap = new SortedDictionary<int, Sample>();
        private SortedDictionary<int, Assay> _assayMap = new SortedDictionary<int, Assay>();

        private SortedDictionary<int, StudyVariable> _studyVariableMap =
            new SortedDictionary<int, StudyVariable>();

        private SortedDictionary<int, CV> _cvMap = new SortedDictionary<int, CV>();
        private List<ColUnit> _proteinColUnitList = new List<ColUnit>();
        private List<ColUnit> _peptideColUnitList = new List<ColUnit>();
        private List<ColUnit> _psmColUnitList = new List<ColUnit>();
        private List<ColUnit> _smallMoleculeColUnitList = new List<ColUnit>();

        public Metadata() : this(new MZTabDescription(MzTabMode.Summary, MzTabType.Identification)){}

        public Metadata(MZTabDescription tabDescription){
            if (tabDescription == null){
                throw new Exception(@"Should define mz-tab description first.");
            }

            _tabDescription = tabDescription;
        }

        private StringBuilder printPrefix(StringBuilder sb){
            sb.Append(Section.Metadata.Prefix).Append(MZTabConstants.TAB);

            return sb;
        }

        /**
     * Multi-lines output. One line like following:
     * item[{map.key}]    {map.value}
     */

        private StringBuilder printMap(IEnumerable<IndexedElement> values, string item, StringBuilder sb){
            foreach (IndexedElement value in values){
                sb.Append(value);

                if (item.Equals(MetadataElement.SAMPLE_PROCESSING.Name)){
                    sb.Append(MZTabConstants.NEW_LINE);
                }
            }
            return sb;
        }

        protected StringBuilder printMap(SortedDictionary<int, Instrument> map, string item, StringBuilder sb){
            return printMap(map.Values.Select(x => x), item, sb);
        }

        protected StringBuilder printMap(SortedDictionary<int, Software> map, string item, StringBuilder sb){
            return printMap(map.Values.Select(x => x), item, sb);
        }

        protected StringBuilder printMap(SortedDictionary<int, Publication> map, string item, StringBuilder sb){
            return printMap(map.Values.Select(x => x), item, sb);
        }

        protected StringBuilder printMap(SortedDictionary<int, Contact> map, string item, StringBuilder sb){
            return printMap(map.Values.Select(x => x), item, sb);
        }

        protected StringBuilder printMap(SortedDictionary<int, MsRun> map, string item, StringBuilder sb){
            return printMap(map.Values.Select(x => x), item, sb);
        }

        protected StringBuilder printMap(SortedDictionary<int, Sample> map, string item, StringBuilder sb){
            return printMap(map.Values.Select(x => x), item, sb);
        }

        protected StringBuilder printMap(SortedDictionary<int, Assay> map, string item, StringBuilder sb){
            return printMap(map.Values.Select(x => x), item, sb);
        }

        protected StringBuilder printMap(SortedDictionary<int, StudyVariable> map, string item, StringBuilder sb){
            return printMap(map.Values.Select(x => x), item, sb);
        }

        protected StringBuilder printMap(SortedDictionary<int, CV> map, string item, StringBuilder sb){
            return printMap(map.Values.Select(x => x), item, sb);
        }

        protected StringBuilder printMap(SortedDictionary<int, FixedMod> map, string item, StringBuilder sb){
            return printMap(map.Values.Select(x => x), item, sb);
        }

        protected StringBuilder printMap(SortedDictionary<int, VariableMod> map, string item, StringBuilder sb){
            return printMap(map.Values.Select(x => x), item, sb);
        }

        protected StringBuilder printMap(SortedDictionary<int, SplitList<Param>> map, string item, StringBuilder sb){
            foreach (int id in map.Keys){
                SplitList<Param> value = map[id];
                printPrefix(sb).Append(item).Append(string.Format("[{0}]", id)).Append(MZTabConstants.TAB).Append(value);

                if (item.Equals(MetadataElement.SAMPLE_PROCESSING.Name)){
                    sb.Append(MZTabConstants.NEW_LINE);
                }
            }
            return sb;
        }

        public override string ToString(){
            StringBuilder sb = new StringBuilder();

            sb.Append(_tabDescription);

            if (_title != null){
                printPrefix(sb)
                    .Append(MetadataElement.TITLE)
                    .Append(MZTabConstants.TAB)
                    .Append(_title)
                    .Append(MZTabConstants.NEW_LINE);
            }

            if (_description != null){
                printPrefix(sb)
                    .Append(MetadataElement.DESCRIPTION)
                    .Append(MZTabConstants.TAB)
                    .Append(_description)
                    .Append(MZTabConstants.NEW_LINE);
            }
            sb = printMap(_sampleProcessingMap, MetadataElement.SAMPLE_PROCESSING.ToString(), sb);
            sb = printMap(_instrumentMap, MetadataElement.INSTRUMENT.ToString(), sb);
            sb = printMap(_softwareMap, MetadataElement.SOFTWARE.ToString(), sb);

            if (_falseDiscoveryRate.Count != 0){
                printPrefix(sb)
                    .Append(MetadataElement.FALSE_DISCOVERY_RATE)
                    .Append(MZTabConstants.TAB)
                    .Append(_falseDiscoveryRate)
                    .Append(MZTabConstants.NEW_LINE);
            }

            sb = printMap(_publicationMap, MetadataElement.PUBLICATION.ToString(), sb);
            sb = printMap(_contactMap, MetadataElement.CONTACT.ToString(), sb);

            foreach (Uri uri in _uriList){
                printPrefix(sb)
                    .Append(MetadataElement.URI)
                    .Append(MZTabConstants.TAB)
                    .Append(uri)
                    .Append(MZTabConstants.NEW_LINE);
            }

            if (_fixedModMap.Count != 0){
                sb = printMap(_fixedModMap, MetadataElement.FIXED_MOD.ToString(), sb);
            }

            if (_variableModMap.Count != 0){
                sb = printMap(_variableModMap, MetadataElement.VARIABLE_MOD.ToString(), sb);
            }

            if (_quantificationMethod != null){
                printPrefix(sb)
                    .Append(MetadataElement.QUANTIFICATION_METHOD)
                    .Append(MZTabConstants.TAB)
                    .Append(_quantificationMethod)
                    .Append(MZTabConstants.NEW_LINE);
            }
            if (_proteinQuantificationUnit != null){
                printPrefix(sb)
                    .Append(MetadataElement.PROTEIN)
                    .Append(MZTabConstants.MINUS)
                    .Append(MetadataProperty.PROTEIN_QUANTIFICATION_UNIT)
                    .Append(MZTabConstants.TAB)
                    .Append(_proteinQuantificationUnit)
                    .Append(MZTabConstants.NEW_LINE);
            }
            if (_peptideQuantificationUnit != null){
                printPrefix(sb)
                    .Append(MetadataElement.PEPTIDE)
                    .Append(MZTabConstants.MINUS)
                    .Append(MetadataProperty.PEPTIDE_QUANTIFICATION_UNIT)
                    .Append(MZTabConstants.TAB)
                    .Append(_peptideQuantificationUnit)
                    .Append(MZTabConstants.NEW_LINE);
            }
            if (_smallMoleculeQuantificationUnit != null){
                printPrefix(sb)
                    .Append(MetadataElement.SMALL_MOLECULE)
                    .Append(MZTabConstants.MINUS)
                    .Append(MetadataProperty.SMALL_MOLECULE_QUANTIFICATION_UNIT)
                    .Append(MZTabConstants.TAB)
                    .Append(_smallMoleculeQuantificationUnit)
                    .Append(MZTabConstants.NEW_LINE);
            }

            sb = printMap(_msRunMap, MetadataElement.MS_RUN.ToString(), sb);

            foreach (Param custom in _customList){
                printPrefix(sb)
                    .Append(MetadataElement.CUSTOM)
                    .Append(MZTabConstants.TAB)
                    .Append(custom)
                    .Append(MZTabConstants.NEW_LINE);
            }

            sb = printMap(_sampleMap, MetadataElement.SAMPLE.ToString(), sb);
            sb = printMap(_assayMap, MetadataElement.ASSAY.ToString(), sb);
            sb = printMap(_studyVariableMap, MetadataElement.STUDY_VARIABLE.ToString(), sb);
            sb = printMap(_cvMap, MetadataElement.CV.ToString(), sb);

            foreach (ColUnit colUnit in _proteinColUnitList){
                printPrefix(sb)
                    .Append(MetadataElement.COLUNIT)
                    .Append(MZTabConstants.MINUS)
                    .Append(MetadataProperty.COLUNIT_PROTEIN);
                sb.Append(MZTabConstants.TAB).Append(colUnit).Append(MZTabConstants.NEW_LINE);
            }
            foreach (ColUnit colUnit in _peptideColUnitList){
                printPrefix(sb)
                    .Append(MetadataElement.COLUNIT)
                    .Append(MZTabConstants.MINUS)
                    .Append(MetadataProperty.COLUNIT_PEPTIDE);
                sb.Append(MZTabConstants.TAB).Append(colUnit).Append(MZTabConstants.NEW_LINE);
            }
            foreach (ColUnit colUnit in _psmColUnitList){
                printPrefix(sb)
                    .Append(MetadataElement.COLUNIT)
                    .Append(MZTabConstants.MINUS)
                    .Append(MetadataProperty.COLUNIT_PSM);
                sb.Append(MZTabConstants.TAB).Append(colUnit).Append(MZTabConstants.NEW_LINE);
            }
            foreach (ColUnit colUnit in _smallMoleculeColUnitList){
                printPrefix(sb)
                    .Append(MetadataElement.COLUNIT)
                    .Append(MZTabConstants.MINUS)
                    .Append(MetadataProperty.COLUNIT_SMALL_MOLECULE);
                sb.Append(MZTabConstants.TAB).Append(colUnit).Append(MZTabConstants.NEW_LINE);
            }

            return sb.ToString();
        }

        public MZTabDescription TabDescription{
            get { return _tabDescription; }
            set{
                if (value == null){
                    throw new Exception("MzTabDescription should not set null!");
                }
                _tabDescription = value;
            }
        }

        public string MZTabVersion { get { return _tabDescription.Version; } set { _tabDescription.Version = value; } }

        public string MZTabID { get { return _tabDescription.Id; } set { _tabDescription.Id = value; } }

        public MzTabMode MzTabMode { get { return _tabDescription.MzTabMode; } set { _tabDescription.MzTabMode = value; } }

        public MzTabType MzTabType { get { return _tabDescription.MzTabType; } set { _tabDescription.MzTabType = value; } }

        public string Title { get { return _title; } set { _title = value; } }

        public string Description { get { return _description; } set { _description = value; } }

        public SortedDictionary<int, SplitList<Param>> SampleProcessingMap{
            get { return _sampleProcessingMap; }
            set{
                if (value == null){
                    _sampleProcessingMap = new SortedDictionary<int, SplitList<Param>>();
                }
                _sampleProcessingMap = value;
            }
        }

        public SortedDictionary<int, Instrument> InstrumentMap{
            get { return _instrumentMap; }
            set{
                if (value == null){
                    _instrumentMap = new SortedDictionary<int, Instrument>();
                }
                _instrumentMap = value;
            }
        }

        public SortedDictionary<int, Software> SoftwareMap{
            get { return _softwareMap; }
            set{
                if (value == null){
                    _softwareMap = new SortedDictionary<int, Software>();
                }
                _softwareMap = value;
            }
        }

        public SplitList<Param> FalseDiscoveryRate{
            get { return _falseDiscoveryRate; }
            set{
                if (value == null){
                    _falseDiscoveryRate = new SplitList<Param>(MZTabConstants.BAR);
                }
                _falseDiscoveryRate = value;
            }
        }

        public SortedDictionary<int, Publication> PublicationMap{
            get { return _publicationMap; }
            set{
                if (value == null){
                    _publicationMap = new SortedDictionary<int, Publication>();
                }
                _publicationMap = value;
            }
        }

        public SortedDictionary<int, Contact> ContactMap{
            get { return _contactMap; }
            set{
                if (value == null){
                    _contactMap = new SortedDictionary<int, Contact>();
                }
                _contactMap = value;
            }
        }

        public List<Uri> UriList{
            get { return _uriList; }
            set{
                if (value == null){
                    _uriList = new List<Uri>();
                }
                _uriList = value;
            }
        }

        public SortedDictionary<int, FixedMod> FixedModMap{
            get { return _fixedModMap; }
            set{
                if (value == null){
                    _fixedModMap = new SortedDictionary<int, FixedMod>();
                }
                _fixedModMap = value;
            }
        }

        public SortedDictionary<int, VariableMod> VariableModMap{
            get { return _variableModMap; }
            set{
                if (value == null){
                    _variableModMap = new SortedDictionary<int, VariableMod>();
                }
                _variableModMap = value;
            }
        }

        public Param QuantificationMethod { get { return _quantificationMethod; } set { _quantificationMethod = value; } }

        public Param ProteinQuantificationUnit { get { return _proteinQuantificationUnit; } set { _proteinQuantificationUnit = value; } }

        public Param PeptideQuantificationUnit { get { return _peptideQuantificationUnit; } set { _peptideQuantificationUnit = value; } }

        public Param SmallMoleculeQuantificationUnit { get { return _smallMoleculeQuantificationUnit; } set { _smallMoleculeQuantificationUnit = value; } }

        public SortedDictionary<int, MsRun> MsRunMap{
            get { return _msRunMap; }
            set{
                if (value == null){
                    _msRunMap = new SortedDictionary<int, MsRun>();
                }
                _msRunMap = value;
            }
        }

        public List<Param> CustomList{
            get { return _customList; }
            set{
                if (value == null){
                    _customList = new List<Param>();
                }
                _customList = value;
            }
        }

        public SortedDictionary<int, Sample> SampleMap{
            get { return _sampleMap; }
            set{
                if (value == null){
                    _sampleMap = new SortedDictionary<int, Sample>();
                }
                _sampleMap = value;
            }
        }

        public SortedDictionary<int, Assay> AssayMap{
            get { return _assayMap; }
            set{
                if (value == null){
                    _assayMap = new SortedDictionary<int, Assay>();
                }
                _assayMap = value;
            }
        }

        public SortedDictionary<int, StudyVariable> StudyVariableMap{
            get { return _studyVariableMap; }
            set{
                if (value == null){
                    _studyVariableMap = new SortedDictionary<int, StudyVariable>();
                }
                _studyVariableMap = value;
            }
        }

        public SortedDictionary<int, CV> CvMap{
            get { return _cvMap; }
            set{
                if (value == null){
                    _cvMap = new SortedDictionary<int, CV>();
                }
                _cvMap = value;
            }
        }

        public List<ColUnit> ProteinColUnitList{
            get { return _proteinColUnitList; }
            set{
                if (value == null){
                    _proteinColUnitList = new List<ColUnit>();
                }
                _proteinColUnitList = value;
            }
        }

        public List<ColUnit> PeptideColUnitList{
            get { return _peptideColUnitList; }
            set{
                if (value == null){
                    _peptideColUnitList = new List<ColUnit>();
                }
                _peptideColUnitList = value;
            }
        }

        public List<ColUnit> PsmColUnitList{
            get { return _psmColUnitList; }
            set{
                if (value == null){
                    _psmColUnitList = new List<ColUnit>();
                }
                _psmColUnitList = value;
            }
        }

        public List<ColUnit> SmallMoleculeColUnitList{
            get { return _smallMoleculeColUnitList; }
            set{
                if (value == null){
                    _smallMoleculeColUnitList = new List<ColUnit>();
                }
                _smallMoleculeColUnitList = value;
            }
        }


        public bool SetTitle(string title){
            if (Title != null){
                return false;
            }
            Title = title;
            return true;
        }

        public bool SetDescription(string description){
            if (Description != null){
                return false;
            }
            Description = description;
            return true;
        }

        public bool AddSampleProcessing(int id, SplitList<Param> sampleProcessing){
            if (_sampleProcessingMap.ContainsKey(id)){
                return false;
            }
            sampleProcessing.Separator = MZTabConstants.BAR;
            _sampleProcessingMap.Add(id, sampleProcessing);
            return true;
        }

        public bool AddSampleProcessingParam(int id, Param param){
            if (!_sampleProcessingMap.ContainsKey(id)){
                _sampleProcessingMap.Add(id, new SplitList<Param>(MZTabConstants.BAR){param});
            }
            else{
                _sampleProcessingMap[id].Add(param);
            }

            return true;
        }

        public bool AddInstrumentName(int id, Param name){
            Instrument instrument;
            if (!_instrumentMap.ContainsKey(id)){
                instrument = new Instrument(id){Name = name};
                _instrumentMap.Add(id, instrument);
                return true;
            }
            instrument = _instrumentMap[id];
            if (instrument.Name != null){
                return false;
            }
            instrument.Name = name;
            return true;
        }

        public bool AddInstrumentSource(int id, Param source){
            Instrument instrument;
            if (!_instrumentMap.ContainsKey(id)){
                instrument = new Instrument(id){Source = source};
                _instrumentMap.Add(id, instrument);
                return true;
            }
            instrument = _instrumentMap[id];
            if (instrument.Source != null){
                return false;
            }
            instrument.Source = source;
            return true;
        }

        public bool AddInstrumentAnalyzer(int id, Param analyzer){
            Instrument instrument;
            if (!_instrumentMap.ContainsKey(id)){
                instrument = new Instrument(id){Analyzer = analyzer};
                _instrumentMap.Add(id, instrument);
                return true;
            }
            instrument = _instrumentMap[id];
            if (instrument.Analyzer != null){
                return false;
            }
            instrument.Analyzer = analyzer;
            return true;
        }

        public bool AddInstrumentDetector(int id, Param detector){
            Instrument instrument;
            if (!_instrumentMap.ContainsKey(id)){
                instrument = new Instrument(id){Detector = detector};
                _instrumentMap.Add(id, instrument);
                return true;
            }
            instrument = _instrumentMap[id];
            if (instrument.Detector != null){
                return false;
            }
            instrument.Detector = detector;
            return true;
        }

        public bool AddSoftwareParam(int id, Param param){
            Software software;
            if (!_softwareMap.ContainsKey(id)){
                software = new Software(id){Param = param};
                _softwareMap.Add(id, software);
                return true;
            }
            software = _softwareMap[id];
            if (software.Param != null){
                return false;
            }
            software.Param = param;
            return true;
        }

        public void AddSoftwareSetting(int id, string setting){
            Software software;
            if (!_softwareMap.ContainsKey(id)){
                software = new Software(id);

                _softwareMap.Add(id, software);
            }
            else{
                software = _softwareMap[id];
            }

            software.AddSetting(setting);
        }

        public void AddFalseDiscoveryRateParam(Param param){
            _falseDiscoveryRate.Add(param);
        }

        public void AddPublicationItem(int id, PublicationType type, string accession){
            Publication publication;
            if (!_publicationMap.ContainsKey(id)){
                publication = new Publication(id);
                _publicationMap.Add(id, publication);
            }
            else{
                publication = _publicationMap[id];
            }

            publication.AddPublicationItem(new PublicationItem(type, accession));
        }

        public void AddPublicationItems(int id, IEnumerable<PublicationItem> items){
            Publication publication;
            if (!_publicationMap.ContainsKey(id)){
                publication = new Publication(id);
                _publicationMap.Add(id, publication);
            }
            else{
                publication = _publicationMap[id];
            }
            publication.AddPublicationItems(items);
        }

        public bool AddContactName(int id, string name){
            Contact contact;
            if (!_contactMap.ContainsKey(id)){
                contact = new Contact(id){Name = name};
                _contactMap.Add(id, contact);
                return true;
            }
            contact = _contactMap[id];
            if (contact.Name != null){
                return false;
            }
            contact.Name = name;
            return true;
        }

        public bool AddContactAffiliation(int id, string affiliation){
            Contact contact;
            if (!_contactMap.ContainsKey(id)){
                contact = new Contact(id){Affiliation = affiliation};
                _contactMap.Add(id, contact);
                return true;
            }
            contact = _contactMap[id];
            if (contact.Affiliation != null){
                return false;
            }
            contact.Affiliation = affiliation;
            return true;
        }

        public bool AddContactEmail(int id, string email){
            Contact contact;
            if (!_contactMap.ContainsKey(id)){
                contact = new Contact(id){Email = email};
                _contactMap.Add(id, contact);
                return true;
            }
            contact = _contactMap[id];
            if (contact.Email != null){
                return false;
            }
            contact.Email = email;
            return true;
        }

        public void AddUri(Uri uri){
            _uriList.Add(uri);
        }

        public bool AddFixedModParam(int id, Param param){
            FixedMod mod;
            if (_fixedModMap.ContainsKey(id)){
                mod = _fixedModMap[id];
                if (mod.Param != null){
                    return false;
                }
                mod.Param = param;
                return true;
            }
            mod = new FixedMod(id);
            _fixedModMap.Add(id, mod);

            mod.Param = param;
            return true;
        }

        public void AddFixedModSite(int id, string site){
            FixedMod mod;
            if (_fixedModMap.ContainsKey(id)){
                mod = _fixedModMap[id];
                mod.Site = site;
            }
            else{
                mod = new FixedMod(id);
                _fixedModMap.Add(id, mod);
                mod.Site = site;
            }
        }

        public void AddFixedModPosition(int id, string position){
            FixedMod mod;
            if (_fixedModMap.ContainsKey(id)){
                mod = _fixedModMap[id];
                mod.Position = position;
            }
            else{
                mod = new FixedMod(id);
                _fixedModMap.Add(id, mod);
                mod.Position = position;
            }
        }

        public bool AddVariableModParam(int id, Param param){
            VariableMod mod;
            if (_variableModMap.ContainsKey(id)){
                mod = _variableModMap[id];
                if (mod.Param != null){
                    return false;
                }
                mod.Param = param;
                return true;
            }
            mod = new VariableMod(id);
            _variableModMap.Add(id, mod);

            mod.Param = param;
            return true;
        }

        public void AddVariableModSite(int id, string site){
            VariableMod mod;
            if (!_variableModMap.ContainsKey(id)){
                mod = new VariableMod(id);
                _variableModMap.Add(id, mod);
                mod.Site = site;
            }
            else{
                mod = _variableModMap[id];
                mod.Site = site;
            }
        }

        public void AddVariableModPosition(int id, string position){
            VariableMod mod;
            if (!_variableModMap.ContainsKey(id)){
                mod = new VariableMod(id);
                _variableModMap.Add(id, mod);
                mod.Position = position;
            }
            else{
                mod = _variableModMap[id];
                mod.Position = position;
            }
        }

        public void AddMsRun(MsRun msRun){
            _msRunMap.Add(msRun.Id, msRun);
        }

        public bool AddMsRunFormat(int id, Param format){
            MsRun msRun;
            if (!_msRunMap.ContainsKey(id)){
                msRun = new MsRun(id){Format = format};
                _msRunMap.Add(id, msRun);
                return true;
            }
            msRun = _msRunMap[id];
            if (msRun.Format != null){
                return false;
            }
            msRun.Format = format;
            return true;
        }

        public bool AddMsRunLocation(int id, Url location){
            MsRun msRun;
            if (!_msRunMap.ContainsKey(id)){
                msRun = new MsRun(id){Location = location};
                _msRunMap.Add(id, msRun);
                return true;
            }
            msRun = _msRunMap[id];
            if (msRun.Location != null){
                return false;
            }
            msRun.Location = location;
            return true;
        }

        public bool AddMsRunIdFormat(int id, Param idFormat){
            MsRun msRun;
            if (!_msRunMap.ContainsKey(id)){
                msRun = new MsRun(id){IdFormat = idFormat};
                _msRunMap.Add(id, msRun);
                return true;
            }
            msRun = _msRunMap[id];
            if (msRun.IdFormat != null){
                return false;
            }
            msRun.IdFormat = idFormat;
            return true;
        }

        public bool AddMsRunFragmentationMethod(int id, Param fragmentationMethod){
            MsRun msRun;
            if (!_msRunMap.ContainsKey(id)){
                msRun = new MsRun(id){FragmentationMethod = fragmentationMethod};
                _msRunMap.Add(id, msRun);
                return true;
            }

            msRun = _msRunMap[id];
            if (msRun.FragmentationMethod != null){
                return false;
            }
            msRun.FragmentationMethod = fragmentationMethod;
            return true;
        }

        public void AddCustom(Param custom){
            _customList.Add(custom);
        }

        public void AddSample(Sample sample){
            if (sample == null){
                throw new Exception("Sample should not be null");
            }
            _sampleMap.Add(sample.Id, sample);
        }

        public bool AddSampleSpecies(int id, Param species){
            Sample sample;
            if (!_sampleMap.ContainsKey(id)){
                sample = new Sample(id);
                sample.AddSpecies(species);
                _sampleMap.Add(id, sample);
                return true;
            }
            sample = _sampleMap[id];

            sample.AddSpecies(species);
            return true;
        }

        public bool AddSampleTissue(int id, Param tissue){
            Sample sample;
            if (!_sampleMap.ContainsKey(id)){
                sample = new Sample(id);
                sample.AddTissue(tissue);
                _sampleMap.Add(id, sample);
                return true;
            }
            sample = _sampleMap[id];

            sample.AddTissue(tissue);
            return true;
        }

        public bool AddSampleCellType(int id, Param cellType){
            Sample sample;
            if (!_sampleMap.ContainsKey(id)){
                sample = new Sample(id);
                sample.AddCellType(cellType);
                _sampleMap.Add(id, sample);
                return true;
            }
            sample = _sampleMap[id];

            if (sample.CellTypeList.Contains(cellType)){
                return false;
            }

            sample.AddCellType(cellType);
            return true;
        }

        public bool AddSampleDisease(int id, Param disease){
            Sample sample;
            if (!_sampleMap.ContainsKey(id)){
                sample = new Sample(id);
                sample.AddDisease(disease);
                _sampleMap.Add(id, sample);
                return true;
            }
            sample = _sampleMap[id];

            sample.AddDisease(disease);
            return true;
        }

        public bool AddSampleDescription(int id, string description){
            Sample sample;
            if (!_sampleMap.ContainsKey(id)){
                sample = new Sample(id){Description = description};
                _sampleMap.Add(id, sample);
                return true;
            }
            sample = _sampleMap[id];
            sample.Description = description;
            return true;
        }

        public bool AddSampleCustom(int id, Param custom){
            Sample sample;
            if (!_sampleMap.ContainsKey(id)){
                sample = new Sample(id);
                sample.AddCustom(custom);
                _sampleMap.Add(id, sample);
                return true;
            }
            sample = _sampleMap[id];
            sample.AddCustom(custom);
            return true;
        }

        public void AddAssay(Assay assay){
            _assayMap.Add(assay.Id, assay);
        }

        public bool AddAssayQuantificationReagent(int id, Param quantificationReagent){
            Assay assay;
            if (!_assayMap.ContainsKey(id)){
                assay = new Assay(id){QuantificationReagent = quantificationReagent};
                _assayMap.Add(id, assay);
                return true;
            }
            assay = _assayMap[id];
            if (assay.QuantificationReagent != null){
                return false;
            }
            assay.QuantificationReagent = quantificationReagent;
            return true;
        }

        public bool AddAssaySample(int id, Sample sample){
            Assay assay;
            if (!_assayMap.ContainsKey(id)){
                assay = new Assay(id){Sample = sample};
                _assayMap.Add(id, assay);
                return true;
            }
            assay = _assayMap[id];
            assay.Sample = sample;
            return true;
        }

        public bool AddAssayMsRun(int id, MsRun msRun){
            Assay assay;
            if (!_assayMap.ContainsKey(id)){
                assay = new Assay(id){MsRun = msRun};
                _assayMap.Add(id, assay);
                return true;
            }
            assay = _assayMap[id];
            assay.MsRun = msRun;
            return true;
        }

        public void AddAssayQuantificationModParam(int assayId, int quanModId, Param param){
            Assay assay = _assayMap[assayId];
            if (assay == null){
                assay = new Assay(assayId);
                assay.addQuantificationModParam(quanModId, param);
                _assayMap.Add(assayId, assay);
            }
            else{
                assay.addQuantificationModParam(quanModId, param);
            }
        }

        public void AddAssayQuantificationModSite(int assayId, int quanModId, String site){
            Assay assay = _assayMap[assayId];
            if (assay == null){
                assay = new Assay(assayId);
                assay.addQuantificationModSite(quanModId, site);
                _assayMap.Add(assayId, assay);
            }
            else{
                assay.addQuantificationModSite(quanModId, site);
            }
        }

        public void AddAssayQuantificationModPosition(int assayId, int quanModId, String position){
            Assay assay = _assayMap[assayId];
            if (assay == null){
                assay = new Assay(assayId);
                assay.addQuantificationModPosition(quanModId, position);
                _assayMap.Add(assayId, assay);
            }
            else{
                assay.addQuantificationModPosition(quanModId, position);
            }
        }

        public void AddStudyVariable(StudyVariable studyVariable){
            _studyVariableMap.Add(studyVariable.Id, studyVariable);
        }

        public bool AddStudyVariableAssay(int id, Assay assay){
            StudyVariable studyVariable;
            if (!_studyVariableMap.ContainsKey(id)){
                studyVariable = new StudyVariable(id);
                studyVariable.AddAssay(assay.Id, assay);
                _studyVariableMap.Add(id, studyVariable);
                return true;
            }
            studyVariable = _studyVariableMap[id];
            studyVariable.AddAssay(assay.Id, assay);
            return true;
        }

        public bool AddStudyVariableSample(int id, Sample sample){
            StudyVariable studyVariable;
            if (!_studyVariableMap.ContainsKey(id)){
                studyVariable = new StudyVariable(id);
                studyVariable.AddSample(sample.Id, sample);
                _studyVariableMap.Add(id, studyVariable);
                return true;
            }
            studyVariable = _studyVariableMap[id];
            studyVariable.AddSample(sample.Id, sample);
            return true;
        }

        public bool AddStudyVariableDescription(int id, string description){
            StudyVariable studyVariable;
            if (!_studyVariableMap.ContainsKey(id)){
                studyVariable = new StudyVariable(id){Description = description};
                //TODO: add return value
                _studyVariableMap.Add(id, studyVariable);
                return true;
            }
            studyVariable = _studyVariableMap[id];
            studyVariable.Description = description;
            //TODO: Moved it to the upper if studyVariableMap.Add(id, studyVariable);
            return true;
        }

        public void AddCVLabel(int id, string label){
            if (!_cvMap.ContainsKey(id)){
                _cvMap.Add(id, new CV(id));
            }
            CV cv = _cvMap[id];
            cv.Label = label;
        }

        public void AddCVFullName(int id, string fullName){
            if (!_cvMap.ContainsKey(id)){
                _cvMap.Add(id, new CV(id));
            }
            CV cv = _cvMap[id];
            cv.FullName = fullName;
        }

        public void AddCVVersion(int id, string version){
            if (!_cvMap.ContainsKey(id)){
                _cvMap.Add(id, new CV(id));
            }
            CV cv = _cvMap[id];
            cv.Version = version;
        }

        public void AddCVURL(int id, string url){
            if (!_cvMap.ContainsKey(id)){
                _cvMap.Add(id, new CV(id));
            }
            CV cv = _cvMap[id];
            cv.Url = url;
        }

        public void AddProteinColUnit(MZTabColumn column, Param param){
            _proteinColUnitList.Add(new ColUnit(column, param));
        }

        public void AddPeptideColUnit(MZTabColumn column, Param param){
            _peptideColUnitList.Add(new ColUnit(column, param));
        }

        public void AddPSMColUnit(MZTabColumn column, Param param){
            _psmColUnitList.Add(new ColUnit(column, param));
        }

        public void AddSmallMoleculeColUnit(MZTabColumn column, Param param){
            _smallMoleculeColUnitList.Add(new ColUnit(column, param));
        }
    }
}