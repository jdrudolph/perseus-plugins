using System;
using PluginMzTab.Plugin.Utils;

namespace PluginMzTab.Lib.Model{
    public class PSM : MZTabRecord, ICloneable{
        private readonly Metadata metadata;

        public PSM(Metadata metadata)
            : base(MZTabColumnFactory.GetInstance(Section.Peptide)){
            this.metadata = metadata;
        }

        public PSM(MZTabColumnFactory factory, Metadata metadata)
            : base(factory){
            this.metadata = metadata;
        }

        public string Sequence { get { return getString(PSMColumn.SEQUENCE.LogicPosition); } set { setValue(PSMColumn.SEQUENCE.LogicPosition, MZTabUtils.ParseString(value)); } }

        public Integer PSM_ID { get { return getInteger(PSMColumn.PSM_ID.LogicPosition); } set { setValue(PSMColumn.PSM_ID.LogicPosition, value); } }

        public void SetPSM_ID(string psmIdLabel){
            PSM_ID = MZTabUtils.ParseInteger(psmIdLabel);
        }

        public string Accession { get { return getString(PSMColumn.ACCESSION.LogicPosition); } set { setValue(PSMColumn.ACCESSION.LogicPosition, MZTabUtils.ParseString(value)); } }

        public MZBoolean Unique { get { return getMZBoolean(PSMColumn.UNIQUE.LogicPosition); } set { setValue(PSMColumn.UNIQUE.LogicPosition, value); } }

        public void setUnique(string uniqueLabel){
            Unique = MZBoolean.FindBoolean(uniqueLabel);
        }

        public string Database { get { return getString(PSMColumn.DATABASE.LogicPosition); } set { setValue(PSMColumn.DATABASE.LogicPosition, MZTabUtils.ParseString(value)); } }

        public string DatabaseVersion { get { return getString(PSMColumn.DATABASE_VERSION.LogicPosition); } set { setValue(PSMColumn.DATABASE_VERSION.LogicPosition, MZTabUtils.ParseString(value)); } }

        public SplitList<Param> SearchEngine { get { return GetSplitList<Param>(PSMColumn.SEARCH_ENGINE.LogicPosition); } set { setValue(PSMColumn.SEARCH_ENGINE.LogicPosition, value); } }

        public bool AddSearchEngine(Param param){
            if (param == null){
                return false;
            }

            SplitList<Param> searchparams = SearchEngine;
            if (searchparams == null){
                searchparams = new SplitList<Param>(MZTabConstants.BAR);
                SearchEngine = searchparams;
                searchparams.Add(param);
            }
            else if (!searchparams.Contains(param)){
                searchparams.Add(param);
            }

            return true;
        }

        public bool AddSearchEngine(string paramLabel){
            return !string.IsNullOrEmpty(paramLabel) && AddSearchEngine(MZTabUtils.ParseParam(paramLabel));
        }

        public void setSearchEngine(string searchEngineLabel){
            SearchEngine = MZTabUtils.ParseParamList(searchEngineLabel);
        }

        /**
        * The search engine score for the given protein in the defined ms run. The type of score
        * MUST be defined in the metadata section. If the protein was not identified by the specified
        * search engine “null” must be reported
        *
        * @param id protein_search_engine_score[id] which MUST be defined in the metadata section.
        * @param msRun SHOULD NOT set null
        */
        public Double getSearchEngineScore(Integer id) {
            return getDouble(getLogicalPosition(PSMColumn.SEARCH_ENGINE_SCORE, id, null));
        }

        /**
         * The search engine score for the given protein in the defined ms run. The type of score
         * MUST be defined in the metadata section. If the protein was not identified by the specified
         * search engine “null” must be reported
         *
         * @param id protein_search_engine_score[id] which MUST be defined in the metadata section.
         * @param msRun SHOULD NOT set null
         */
        public void setSearchEngineScore(Integer id, double searchEngineScore) {
            setValue(getLogicalPosition(PSMColumn.SEARCH_ENGINE_SCORE, id, null), searchEngineScore);
        }

        [Obsolete]
        public SplitList<Param> SearchEngineScore { get { return GetSplitList<Param>(PSMColumn.SEARCH_ENGINE_SCORE.LogicPosition); } set { setValue(PSMColumn.SEARCH_ENGINE_SCORE.LogicPosition, value); } }

        [Obsolete]
        public bool AddSearchEngineScore(Param param){
            if (param == null){
                return false;
            }

            SplitList<Param> searchparams = SearchEngineScore;
            if (searchparams == null){
                searchparams = new SplitList<Param>(MZTabConstants.BAR);
                SearchEngineScore = searchparams;
            }

            searchparams.Add(param);
            return true;
        }

        [Obsolete]
        public bool AddSearchEngineScore(string paramLabel){
            return !string.IsNullOrEmpty(paramLabel) && AddSearchEngineScore(MZTabUtils.ParseParam(paramLabel));
        }

        [Obsolete]
        public void setSearchEngineScore(string searchEngineScoreLabel){
            SearchEngineScore = MZTabUtils.ParseParamList(searchEngineScoreLabel);
        }

        public Reliability Reliability { get { return getReliability(PSMColumn.RELIABILITY.LogicPosition); } set { setValue(PSMColumn.RELIABILITY.LogicPosition, value); } }

        public void setReliability(string reliabilityLabel){
            Reliability = Reliability.findReliability(reliabilityLabel);
        }

        public SplitList<Modification> Modifications { get { return GetSplitList<Modification>(PSMColumn.MODIFICATIONS.LogicPosition); } set { setValue(PSMColumn.MODIFICATIONS.LogicPosition, value); } }

        public bool AddModification(Modification modification){
            if (modification == null){
                return false;
            }

            SplitList<Modification> modList = Modifications;
            if (modList == null){
                modList = new SplitList<Modification>(MZTabConstants.COMMA);
                Modifications = modList;
            }

            modList.Add(modification);
            return true;
        }

        public void setModifications(string modificationsLabel){
            Modifications = MZTabUtils.ParseModificationList(Section.Peptide, modificationsLabel);
        }

        public SplitList<double> RetentionTime { get { return GetSplitList<double>(PSMColumn.RETENTION_TIME.LogicPosition); } set { setValue(PSMColumn.RETENTION_TIME.LogicPosition, value); } }

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

        public void setRetentionTime(string retentionTimeLabel){
            RetentionTime = MZTabUtils.ParseDoubleList(retentionTimeLabel);
        }

        public Integer Charge { get { return getInteger(PSMColumn.CHARGE.LogicPosition); } set { setValue(PSMColumn.CHARGE.LogicPosition, value); } }

        public void setCharge(string chargeLabel){
            Charge = MZTabUtils.ParseInteger(chargeLabel);
        }

        public double ExpMassToCharge { get { return getDouble(PSMColumn.EXP_MASS_TO_CHARGE.LogicPosition); } set { setValue(PSMColumn.EXP_MASS_TO_CHARGE.LogicPosition, value); } }

        public void SetExpMassToCharge(string expMassToChargeLabel){
            ExpMassToCharge = MZTabUtils.ParseDouble(expMassToChargeLabel);
        }

        public double CalcMassToCharge { get { return getDouble(PSMColumn.CALC_MASS_TO_CHARGE.LogicPosition); } set { setValue(PSMColumn.CALC_MASS_TO_CHARGE.LogicPosition, value); } }

        public void SetCalcMassToCharge(string calcMassToChargeLabel){
            CalcMassToCharge = MZTabUtils.ParseDouble(calcMassToChargeLabel);
        }

        public Uri URI { get { return getURI(PSMColumn.URI.LogicPosition); } set { setValue(PSMColumn.URI.LogicPosition, value); } }

        public void SetUri(string uriLabel){
            URI = MZTabUtils.ParseURI(uriLabel);
        }

        public SplitList<SpectraRef> SpectraRef { get { return GetSplitList<SpectraRef>(PSMColumn.SPECTRA_REF.LogicPosition); } set { setValue(PSMColumn.SPECTRA_REF.LogicPosition, value); } }

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

        public void SetSpectraRef(string spectraRefLabel){
            SpectraRef = MZTabUtils.ParseSpectraRefList(metadata, spectraRefLabel);
        }

        public string Pre { get { return getString(PSMColumn.PRE.LogicPosition); } set { setValue(PSMColumn.PRE.LogicPosition, MZTabUtils.ParseString(value)); } }

        public string Post { get { return getString(PSMColumn.POST.LogicPosition); } set { setValue(PSMColumn.POST.LogicPosition, MZTabUtils.ParseString(value)); } }

        public string Start { get { return getString(PSMColumn.START.LogicPosition); } set { setValue(PSMColumn.START.LogicPosition, MZTabUtils.ParseString(value)); } }

        public string End { get { return getString(PSMColumn.END.LogicPosition); } set { setValue(PSMColumn.END.LogicPosition, MZTabUtils.ParseString(value)); } }

        /**
         * PEP  value1  value2  value3  ...
         */

        public override string ToString(){
            return Section.PSM.Prefix + MZTabConstants.TAB + base.ToString();
        }

        public object Clone(){
            PSM psm = new PSM(factory, metadata);
            psm.Sequence = Sequence;
            psm.PSM_ID = PSM_ID;
            psm.Accession = Accession;
            psm.Unique = Unique;
            psm.Database = Database;
            psm.DatabaseVersion = DatabaseVersion;
            psm.SearchEngine = SearchEngine;
            psm.SearchEngineScore = SearchEngineScore;
            psm.Reliability = Reliability;
            psm.Modifications = Modifications;
            psm.RetentionTime = RetentionTime;
            psm.Charge = Charge;
            psm.ExpMassToCharge = ExpMassToCharge;
            psm.CalcMassToCharge = CalcMassToCharge;
            psm.URI = URI;
            psm.SpectraRef = SpectraRef;
            psm.Post = Post;
            psm.Pre = Pre;
            psm.Start = Start;
            psm.End = End;

            return psm;
        }
    }
}