using System;
using PluginMzTab.Plugin.Utils;

namespace PluginMzTab.Lib.Model{
    public class Protein : MZTabRecord{
        public Protein() : base(MZTabColumnFactory.GetInstance(Section.Protein)){}

        public Protein(MZTabColumnFactory factory) : base(factory){}

        public string Accession { get { return getString(ProteinColumn.ACCESSION.LogicPosition); } set { setValue(ProteinColumn.ACCESSION.LogicPosition, MZTabUtils.ParseString(value)); } }

        public string Description { get { return getString(ProteinColumn.DESCRIPTION.LogicPosition); } set { setValue(ProteinColumn.DESCRIPTION.LogicPosition, MZTabUtils.ParseString(value)); } }

        public Integer Taxid { get { return getInteger(ProteinColumn.TAXID.LogicPosition); } set { setValue(ProteinColumn.TAXID.LogicPosition, value); } }

        public void SetTaxid(string value){
            Taxid = MZTabUtils.ParseInteger(value);
        }

        public string Species { get { return getString(ProteinColumn.SPECIES.LogicPosition); } set { setValue(ProteinColumn.SPECIES.LogicPosition, MZTabUtils.ParseString(value)); } }

        public string Database { get { return getString(ProteinColumn.DATABASE.LogicPosition); } set { setValue(ProteinColumn.DATABASE.LogicPosition, MZTabUtils.ParseString(value)); } }

        public string DatabaseVersion { get { return getString(ProteinColumn.DATABASE_VERSION.LogicPosition); } set { setValue(ProteinColumn.DATABASE_VERSION.LogicPosition, MZTabUtils.ParseString(value)); } }


        public SplitList<Param> SearchEngine { get { return GetSplitList<Param>(ProteinColumn.SEARCH_ENGINE.LogicPosition); } set { setValue(ProteinColumn.SEARCH_ENGINE.LogicPosition, value); } }

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
            }

            searchParams.Add(param);
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
            return getDouble(getLogicalPosition(ProteinColumn.BEST_SEARCH_ENGINE_SCORE, id, null));
        }

        /**
         * The best search engine score (for this type of score) for the given protein across
         * all replicates reported. The type of score MUST be defined in the metadata section.
         * If the protein was not identified by the specified search engine “null” must be reported
         *
         * @param id protein_search_engine_score[id] which MUST be defined in the metadata section.
         */
        public void setBestSearchEngineScore(Integer id, double bestSearchEngineScore) {
            setValue(getLogicalPosition(ProteinColumn.BEST_SEARCH_ENGINE_SCORE, id, null), bestSearchEngineScore);
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
            return getDouble(getLogicalPosition(ProteinColumn.SEARCH_ENGINE_SCORE, id, msRun));
        }

        /**
         * The search engine score for the given protein in the defined ms run. The type of score
         * MUST be defined in the metadata section. If the protein was not identified by the specified
         * search engine “null” must be reported
         *
         * @param id protein_search_engine_score[id] which MUST be defined in the metadata section.
         * @param msRun SHOULD NOT set null
         */
        public void setSearchEngineScore(Integer id, MsRun msRun, Double searchEngineScore) {
            setValue(getLogicalPosition(ProteinColumn.SEARCH_ENGINE_SCORE, id, msRun), searchEngineScore);
        }

        [Obsolete]
        public SplitList<Param> BestSearchEngineScore { get { return GetSplitList<Param>(ProteinColumn.BEST_SEARCH_ENGINE_SCORE.LogicPosition); } set { setValue(ProteinColumn.BEST_SEARCH_ENGINE_SCORE.LogicPosition, value); } }
        
        [Obsolete]
        public void SetBestSearchEngineScore(string value){
            BestSearchEngineScore = MZTabUtils.ParseParamList(value);
        }

        [Obsolete]
        public bool addBestSearchEngineScoreParam(Param param){
            if (param == null){
                return false;
            }

            SplitList<Param> searchparam = BestSearchEngineScore;
            if (searchparam == null){
                searchparam = new SplitList<Param>(MZTabConstants.BAR);
                BestSearchEngineScore = searchparam;
            }

            searchparam.Add(param);
            return true;
        }

        [Obsolete]
        public bool addBestSearchEngineScoreParam(string paramLabel){
            return !string.IsNullOrEmpty(paramLabel) && addBestSearchEngineScoreParam(MZTabUtils.ParseParam(paramLabel));
        }

        [Obsolete]
        public SplitList<Param> getSearchEngineScore(MsRun msRun){
            return GetSplitList<Param>(getPosition(ProteinColumn.SEARCH_ENGINE_SCORE, msRun));
        }

        [Obsolete]
        public void setSearchEngineScore(string logicalPosition, SplitList<Param> searchEngineScore){
            setValue(logicalPosition, searchEngineScore);
        }

        [Obsolete]
        public void setSearchEngineScore(MsRun msRun, SplitList<Param> searchEngineScore){
            setSearchEngineScore(getPosition(ProteinColumn.SEARCH_ENGINE_SCORE, msRun), searchEngineScore);
        }

        [Obsolete]
        public void setSearchEngineScore(string logicalPosition, string paramsLabel){
            setValue(logicalPosition, MZTabUtils.ParseParamList(paramsLabel));
        }

        [Obsolete]
        public void setSearchEngineScore(MsRun msRun, string paramsLabel){
            setSearchEngineScore(msRun, MZTabUtils.ParseParamList(paramsLabel));
        }

        [Obsolete]
        public bool addSearchEngineScoreParam(MsRun msRun, CVParam param){
            if (param == null){
                return false;
            }

            SplitList<Param> searchParams = getSearchEngineScore(msRun);
            if (searchParams == null){
                searchParams = new SplitList<Param>(MZTabConstants.BAR);
                setSearchEngineScore(msRun, searchParams);
            }
            searchParams.Add(param);

            return true;
        }


        public Reliability Reliability { get { return getReliability(ProteinColumn.RELIABILITY.LogicPosition); } set { setValue(ProteinColumn.RELIABILITY.LogicPosition, value); } }

        public void SetReliability(string value){
            Reliability = Reliability.findReliability(value);
        }


        public Integer getNumPSMs(MsRun msRun){
            return getInteger(getPosition(ProteinColumn.NUM_PSMS, msRun));
        }

        public void setNumPSMs(string logicalPosition, Integer numPSMs){
            setValue(logicalPosition, numPSMs);
        }

        public void setNumPSMs(MsRun msRun, Integer numPSMs) {
            setNumPSMs(getLogicalPosition(ProteinColumn.NUM_PSMS, null, msRun), numPSMs.ToString());
        }

        [Obsolete]
        public void setNumPSMs(MsRun msRun, int numPSMs){
            setNumPSMs(getPosition(ProteinColumn.NUM_PSMS, msRun), numPSMs);
        }

        [Obsolete]
        public void setNumPSMs(string logicalPosition, string numPSMsLabel){
            setNumPSMs(logicalPosition, MZTabUtils.ParseInteger(numPSMsLabel));
        }

        [Obsolete]
        public void setNumPSMs(string logicalPosition, int numPSMs) {
            setValue(logicalPosition, numPSMs);
        }

        [Obsolete]
        public void setNumPSMs(MsRun msRun, string numPSMsLabel){
            setNumPSMs(msRun, MZTabUtils.ParseInteger(numPSMsLabel));
        }


        public Integer getNumPeptidesDistinct(MsRun msRun) {
            return getInteger(getPosition(ProteinColumn.NUM_PEPTIDES_DISTINCT, msRun));
        }

        public void setNumPeptidesDistinct(string logicalPosition, Integer numPeptidesDistinct){
            setValue(logicalPosition, numPeptidesDistinct);
        }

        public void setNumPeptidesDistinct(MsRun msRun, Integer numPeptidesDistinct) {
            setNumPeptidesDistinct(getPosition(ProteinColumn.NUM_PEPTIDES_DISTINCT, msRun), numPeptidesDistinct);
        }

        [Obsolete]
        public void setNumPeptidesDistinct(MsRun msRun, int numPeptidesDistinct){
            setNumPeptidesDistinct(getPosition(ProteinColumn.NUM_PEPTIDES_DISTINCT, msRun), numPeptidesDistinct);
        }

        public void setNumPeptidesDistinct(MsRun msRun, string numPeptidesDistinct){
            setNumPeptidesDistinct(msRun, MZTabUtils.ParseInteger(numPeptidesDistinct));
        }

        [Obsolete]
        public void setNumPeptidesDistinct(string logicalPosition, int numPeptidesDistinct) {
            setValue(logicalPosition, numPeptidesDistinct);
        }

        public void setNumPeptidesDistinct(string logicalPosition, string numPeptidesDistinct){
            setNumPeptidesDistinct(logicalPosition, MZTabUtils.ParseInteger(numPeptidesDistinct));
        }


        public Integer getNumPeptidesUnique(MsRun msRun) {
            return getInteger(getPosition(ProteinColumn.NUM_PEPTIDES_UNIQUE, msRun));
        }

        public void setNumPeptidesUnique(string logicalPosition, Integer numPeptidesUnique) {
            setValue(logicalPosition, numPeptidesUnique);
        }

        [Obsolete]
        public void setNumPeptidesUnique(string logicalPosition, int numPeptidesUnique){
            setValue(logicalPosition, numPeptidesUnique);
        }

        public void setNumPeptidesUnique(MsRun msRun, Integer numPeptidesUnique){
            setNumPeptidesUnique(getPosition(ProteinColumn.NUM_PEPTIDES_UNIQUE, msRun), numPeptidesUnique);
        }

        public void setNumPeptidesUnique(string logicalPosition, string numPeptidesUnique){
            setNumPeptidesUnique(logicalPosition, MZTabUtils.ParseInteger(numPeptidesUnique));
        }

        [Obsolete]
        public void setNumPeptidesUnique(MsRun msRun, int numPeptidesUnique) {
            setNumPeptidesUnique(getPosition(ProteinColumn.NUM_PEPTIDES_UNIQUE, msRun), numPeptidesUnique);
        }

        public void setNumPeptidesUnique(MsRun msRun, string numPeptidesUnique){
            setNumPeptidesUnique(msRun, MZTabUtils.ParseInteger(numPeptidesUnique));
        }


        public SplitList<string> AmbiguityMembers { get { return GetSplitList<string>(ProteinColumn.AMBIGUITY_MEMBERS.LogicPosition); } set { setValue(ProteinColumn.AMBIGUITY_MEMBERS.LogicPosition, value); } }

        public void SetAmbiguityMembers(string value){
            AmbiguityMembers = MZTabUtils.ParseStringList(MZTabConstants.COMMA, value);
        }

        public bool addAmbiguityMembers(string member){
            if (string.IsNullOrEmpty(member)){
                return false;
            }

            SplitList<string> ambiguityMembers = AmbiguityMembers;
            if (ambiguityMembers == null){
                ambiguityMembers = new SplitList<string>(MZTabConstants.COMMA);
                AmbiguityMembers = ambiguityMembers;
            }

            ambiguityMembers.Add(member);
            return true;
        }


        public SplitList<Modification> Modifications { get { return GetSplitList<Modification>(ProteinColumn.MODIFICATIONS.LogicPosition); } set { setValue(ProteinColumn.MODIFICATIONS.LogicPosition, value); } }

        public void SetModifications(string value){
            Modifications = MZTabUtils.ParseModificationList(Section.Protein, value);
        }

        public bool addModification(Modification modification){
            if (modification == null){
                return false;
            }

            SplitList<Modification> modList = Modifications;
            if (modList == null){
                modList = new SplitList<Modification>(MZTabConstants.COMMA);
                Modifications = modList;
            }

            modList.Add(modification);
            return true; //TODO: Check
        }


        public Uri URI { get { return getURI(ProteinColumn.URI.LogicPosition); } set { setValue(ProteinColumn.URI.LogicPosition, value); } }

        public void SetURI(string value){
            URI = MZTabUtils.ParseURI(value);
        }


        public SplitList<string> GOTerms { get { return GetSplitList<string>(ProteinColumn.GO_TERMS.LogicPosition); } set { setValue(ProteinColumn.GO_TERMS.LogicPosition, value); } }

        public void SetGOTerms(string value){
            GOTerms = MZTabUtils.ParseStringList(MZTabConstants.BAR, value);
        }

        public bool addGOTerm(string term){
            if (string.IsNullOrEmpty(term)){
                return false;
            }

            SplitList<string> goTerms = GOTerms;
            if (goTerms == null){
                goTerms = new SplitList<string>(MZTabConstants.BAR);
                GOTerms = goTerms;
            }

            goTerms.Add(term);
            return true;
        }


        public double ProteinCoverage { get { return getDouble(ProteinColumn.PROTEIN_COVERAGE.LogicPosition); } set { setValue(ProteinColumn.PROTEIN_COVERAGE.LogicPosition, value); } }

        public void SetProteinCoverage(string value){
            ProteinCoverage = MZTabUtils.ParseDouble(value);
        }

        /**
        * PRT  value1  value2  value3  ...
        */
        public override string ToString(){
            return Section.Protein.Prefix + MZTabConstants.TAB + base.ToString();
        }
    }
}