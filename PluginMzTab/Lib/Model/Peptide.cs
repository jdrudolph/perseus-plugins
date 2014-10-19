using System;
using System.Linq;
using PluginMzTab.Plugin.Utils;

namespace PluginMzTab.Lib.Model{
    public class Peptide : MZTabRecord, ICloneable{
        private readonly Metadata metadata;

        public Peptide(Metadata metadata) : base(MZTabColumnFactory.GetInstance(Section.Peptide)){
            this.metadata = metadata;
        }

        public Peptide(MZTabColumnFactory factory, Metadata metadata) : base(factory){
            this.metadata = metadata;
        }

        public string Sequence { get { return getString(PeptideColumn.SEQUENCE.LogicPosition); } set { setValue(PeptideColumn.SEQUENCE.LogicPosition, MZTabUtils.ParseString(value)); } }

        public string Accession { get { return getString(PeptideColumn.ACCESSION.LogicPosition); } set { setValue(PeptideColumn.ACCESSION.LogicPosition, MZTabUtils.ParseString(value)); } }

        public MZBoolean Unique { get { return getMZBoolean(PeptideColumn.UNIQUE.LogicPosition); } set { setValue(PeptideColumn.UNIQUE.LogicPosition, value); } }

        public void SetUnique(string value){
            Unique = MZBoolean.FindBoolean(value);
        }

        public string Database { get { return getString(PeptideColumn.DATABASE.LogicPosition); } set { setValue(PeptideColumn.DATABASE.LogicPosition, MZTabUtils.ParseString(value)); } }

        public string DatabaseVersion { get { return getString(PeptideColumn.DATABASE_VERSION.LogicPosition); } set { setValue(PeptideColumn.DATABASE_VERSION.LogicPosition, MZTabUtils.ParseString(value)); } }

        public Reliability Reliability { get { return getReliability(PeptideColumn.RELIABILITY.LogicPosition); } set { setValue(PeptideColumn.RELIABILITY.LogicPosition, value); } }

        public void SetReliability(string target){
            Reliability = Reliability.findReliability(target);
        }

        public double MassToCharge { get { return getDouble(PeptideColumn.MASS_TO_CHARGE.LogicPosition); } set { setValue(PeptideColumn.MASS_TO_CHARGE.LogicPosition, value); } }

        public bool SetMassToCharge(string target){
            if (string.IsNullOrEmpty(target)){
                return false;
            }
            double value = MZTabUtils.ParseDouble(target);
            if (value.Equals(double.MinValue)){
                MassToCharge = double.NaN;
                return false;
            }
            MassToCharge = value;
            return true;
        }


        public Uri URI { get { return getURI(PeptideColumn.URI.LogicPosition); } set { setValue(PeptideColumn.URI.LogicPosition, value); } }

        public bool SetURI(string target){
            if (string.IsNullOrEmpty(target)){
                return false;
            }

            URI = new Uri(target);

            return true;
        }

        public SplitList<Param> SearchEngine { get { return GetSplitList<Param>(PeptideColumn.SEARCH_ENGINE.LogicPosition); } set { setValue(PeptideColumn.SEARCH_ENGINE.LogicPosition, value); } }

        public void SetSearchEngine(string value){
            SearchEngine = MZTabUtils.ParseParamList(value);
        }

        public bool AddSearchEngine(Param param){
            if (param == null){
                return false;
            }

            SplitList<Param> searchParams = SearchEngine;
            if (searchParams == null){
                searchParams = new SplitList<Param>(MZTabConstants.BAR);
                SearchEngine = searchParams;
                searchParams.Add(param);
            }
            else if (! searchParams.Contains(param)){
                searchParams.Add(param);
            }

            return true;
        }

        public bool AddSearchEngine(string paramLabel){
            return !string.IsNullOrEmpty(paramLabel) && AddSearchEngine(MZTabUtils.ParseParam(paramLabel));
        }

        /**
 * The best search engine score (for this type of score) for the given protein across
 * all replicates reported. The type of score MUST be defined in the metadata section.
 * If the protein was not identified by the specified search engine “null” must be reported
 *
 * @param id protein_search_engine_score[id] which MUST be defined in the metadata section.
 */
        public Double getBestSearchEngineScore(Integer id) {
            return getDouble(getLogicalPosition(PeptideColumn.BEST_SEARCH_ENGINE_SCORE, id, null));
        }

        /**
         * The best search engine score (for this type of score) for the given protein across
         * all replicates reported. The type of score MUST be defined in the metadata section.
         * If the protein was not identified by the specified search engine “null” must be reported
         *
         * @param id protein_search_engine_score[id] which MUST be defined in the metadata section.
         */
        public void setBestSearchEngineScore(Integer id, double bestSearchEngineScore) {
            setValue(getLogicalPosition(PeptideColumn.BEST_SEARCH_ENGINE_SCORE, id, null), bestSearchEngineScore);
        }

        /**
        * The search engine score for the given protein in the defined ms run. The type of score
        * MUST be defined in the metadata section. If the protein was not identified by the specified
        * search engine “null” must be reported
        *
        * @param id protein_search_engine_score[id] which MUST be defined in the metadata section.
        * @param msRun SHOULD NOT set null
        */
        public Double getSearchEngineScore(Integer id, MsRun msRun) {
            return getDouble(getLogicalPosition(PeptideColumn.SEARCH_ENGINE_SCORE, id, msRun));
        }

        /**
         * The search engine score for the given protein in the defined ms run. The type of score
         * MUST be defined in the metadata section. If the protein was not identified by the specified
         * search engine “null” must be reported
         *
         * @param id protein_search_engine_score[id] which MUST be defined in the metadata section.
         * @param msRun SHOULD NOT set null
         */
        public void setSearchEngineScore(Integer id, MsRun msRun, double searchEngineScore) {
            setValue(getLogicalPosition(PeptideColumn.SEARCH_ENGINE_SCORE, id, msRun), searchEngineScore);
        }

        [Obsolete]
        public SplitList<Param> BestSearchEngineScore { get { return GetSplitList<Param>(PeptideColumn.BEST_SEARCH_ENGINE_SCORE.LogicPosition); } set { setValue(PeptideColumn.BEST_SEARCH_ENGINE_SCORE.LogicPosition, value); } }

        [Obsolete]
        public void SetBestSearchEngineScore(string value){
            BestSearchEngineScore = MZTabUtils.ParseParamList(value);
        }

        [Obsolete]
        public bool AddBestSearchEngineScore(Param param){
            if (param == null){
                return false;
            }

            SplitList<Param> searchparams = BestSearchEngineScore;
            if (searchparams == null){
                searchparams = new SplitList<Param>(MZTabConstants.BAR);
                BestSearchEngineScore = (searchparams);
            }

            searchparams.Add(param);
            return true;
        }

        [Obsolete]
        public bool AddBestSearchEngineScore(string paramLabel){
            return !string.IsNullOrEmpty(paramLabel) && AddBestSearchEngineScore(MZTabUtils.ParseParam(paramLabel));
        }

        [Obsolete]
        public SplitList<Param> getSearchEngineScore(MsRun msRun){
            return GetSplitList<Param>(getPosition(PeptideColumn.SEARCH_ENGINE_SCORE, msRun));
        }

        [Obsolete]
        public void SetSearchEngineScore(string logicalPosition, SplitList<Param> searchEngineScore){
            setValue(logicalPosition, searchEngineScore);
        }

        [Obsolete]
        public void SetSearchEngineScore(MsRun msRun, SplitList<Param> searchEngineScore){
            SetSearchEngineScore(getPosition(PeptideColumn.SEARCH_ENGINE_SCORE, msRun), searchEngineScore);
        }

        [Obsolete]
        public void SetSearchEngineScore(MsRun msRun, string paramsLabel){
            SetSearchEngineScore(msRun, MZTabUtils.ParseParamList(paramsLabel));
        }

        [Obsolete]
        public void SetSearchEngineScore(string logicalPosition, string paramsLabel){
            SetSearchEngineScore(logicalPosition, MZTabUtils.ParseParamList(paramsLabel));
        }

        [Obsolete]
        public bool AddSearchEngineScore(MsRun msRun, CVParam param){
            if (param == null){
                return false;
            }

            SplitList<Param> searchparams = getSearchEngineScore(msRun);
            if (searchparams == null){
                searchparams = new SplitList<Param>(MZTabConstants.BAR);
                SetSearchEngineScore(msRun, searchparams);
            }
            searchparams.Add(param);

            return true;
        }


        public SplitList<Modification> Modifications { get { return GetSplitList<Modification>(PeptideColumn.MODIFICATIONS.LogicPosition); } set { setValue(PeptideColumn.MODIFICATIONS.LogicPosition, value); } }

        public void SetModifications(string target){
            Modifications = MZTabUtils.ParseModificationList(Section.Peptide, target);
        }

        public bool AddModification(Modification modification){
            if (modification == null){
                return false;
            }

            if (Modifications == null){
                Modifications = new SplitList<Modification>(MZTabConstants.COMMA);
            }

            if (Modifications.All(x => x.Accession != modification.Accession)){
                Modifications.Add(modification);
            }
            else{
                var m = Modifications.First(x => x.Accession == modification.Accession);
                foreach (int key in modification.PositionMap.Keys){
                    if (m.PositionMap.ContainsKey(key)){
                        continue;
                    }
                    m.AddPosition(key, modification.PositionMap[key]);
                }
            }
            return true;
        }


        public SplitList<double> RetentionTime { get { return GetSplitList<double>(PeptideColumn.RETENTION_TIME.LogicPosition); } set { setValue(PeptideColumn.RETENTION_TIME.LogicPosition, value); } }

        public void SetRetentionTime(string target){
            AddRetentionTime(target);
        }

        public bool AddRetentionTime(double rt){
            if (rt.Equals(double.MinValue)){
                return false;
            }

            SplitList<double> rtList = RetentionTime;
            if (rtList == null){
                rtList = new SplitList<double>(MZTabConstants.BAR);
                RetentionTime = rtList;
            }

            rtList.Add(rt);
            return true;
        }

        public bool AddRetentionTime(string rtLabel){
            return !string.IsNullOrEmpty(rtLabel) && AddRetentionTime(MZTabUtils.ParseDouble(rtLabel));
        }


        public SplitList<double> RetentionTimeWindow { get { return GetSplitList<double>(PeptideColumn.RETENTION_TIME_WINDOW.LogicPosition); } set { setValue(PeptideColumn.RETENTION_TIME_WINDOW.LogicPosition, value); } }

        public void SetRetentionTimeWindow(string retentionTimeWindowLabel){
            RetentionTimeWindow = MZTabUtils.ParseDoubleList(retentionTimeWindowLabel);
        }

        public bool AddRetentionTimeWindow(double rtw){
            SplitList<double> rtwList = RetentionTimeWindow;
            if (rtwList == null){
                rtwList = new SplitList<double>(MZTabConstants.BAR);
                RetentionTimeWindow = rtwList;
            }

            if (rtw.Equals(double.MinValue) || rtwList.Contains(rtw)){
                return false;
            }
            rtwList.Add(rtw);
            return true;
        }

        public bool AddRetentionTimeWindow(string rtwLabel){
            return !string.IsNullOrEmpty(rtwLabel) && AddRetentionTimeWindow(MZTabUtils.ParseDouble(rtwLabel));
        }

        public Integer Charge { get { return getInteger(PeptideColumn.CHARGE.LogicPosition); } set { setValue(PeptideColumn.CHARGE.LogicPosition, value); } }

        public void SetCharge(string value){
            Charge = MZTabUtils.ParseInteger(value);
        }


        public SplitList<SpectraRef> SpectraRef { get { return GetSplitList<SpectraRef>(PeptideColumn.SPECTRA_REF.LogicPosition); } set { setValue(PeptideColumn.SPECTRA_REF.LogicPosition, value); } }

        public void SetSpectraRef(string target){
            SpectraRef = MZTabUtils.ParseSpectraRefList(metadata, target);
        }

        public bool AddSpectraRef(SpectraRef specRef){
            if (specRef == null){
                return false;
            }

            SplitList<SpectraRef> specRefs = SpectraRef;
            if (specRefs == null){
                specRefs = new SplitList<SpectraRef>(MZTabConstants.BAR);
                SpectraRef = specRefs;
            }

            specRefs.Add(specRef);
            return true;
        }


        /**
     * PEP  value1  value2  value3  ...
     */

        public override string ToString(){
            return Section.Peptide.Prefix + MZTabConstants.TAB + base.ToString();
        }

        public object Clone(){
            Peptide peptide = new Peptide(base.factory, this.metadata);
            peptide.Sequence = Sequence;
            peptide.Accession = Accession;
            peptide.Unique = Unique;
            peptide.Database = Database;
            peptide.DatabaseVersion = DatabaseVersion;
            peptide.SearchEngine = SearchEngine;
            peptide.BestSearchEngineScore = BestSearchEngineScore;
            //TODO: peptide.setSearchEngineScore();
            peptide.Reliability = Reliability;
            peptide.Modifications = Modifications;
            peptide.RetentionTime = RetentionTime;
            peptide.RetentionTimeWindow = RetentionTimeWindow;
            peptide.Charge = Charge;
            peptide.MassToCharge = MassToCharge;
            peptide.URI = URI;
            peptide.SpectraRef = SpectraRef;

            return peptide;
        }
    }
}