using System;
using PluginMzTab.Plugin.Utils;

namespace PluginMzTab.Lib.Model{
    public class SmallMolecule : MZTabRecord{
        private readonly Metadata metadata;

        public SmallMolecule(Metadata metadata) : base(MZTabColumnFactory.GetInstance(Section.Small_Molecule)){
            this.metadata = metadata;
        }

        public SmallMolecule(MZTabColumnFactory factory, Metadata metadata) : base(factory){
            this.metadata = metadata;
        }

        public SplitList<string> Identifier { get { return GetSplitList<string>(SmallMoleculeColumn.IDENTIFIER.Order); } set { setValue(SmallMoleculeColumn.IDENTIFIER.Order, value); } }

        public void addIdentifier(string identifier){
            if (string.IsNullOrEmpty(identifier)){
                return;
            }

            SplitList<string> identifierList = Identifier;
            if (identifierList == null){
                identifierList = new SplitList<string>(MZTabConstants.BAR);
                Identifier = identifierList;
            }
            identifierList.Add(identifier);
        }

        public void setIdentifier(string identifierLabel){
            Identifier = MZTabUtils.ParseStringList(MZTabConstants.BAR, identifierLabel);
        }

        public string ChemicalFormula { get { return getString(SmallMoleculeColumn.CHEMICAL_FORMULA.Order); } set { setValue(SmallMoleculeColumn.CHEMICAL_FORMULA.Order, MZTabUtils.ParseString(value)); } }


        public string Smiles { get { return getString(SmallMoleculeColumn.SMILES.Order); } set { setValue(SmallMoleculeColumn.SMILES.Order, MZTabUtils.ParseStringList(MZTabConstants.BAR, value)); } }

        public string InchiKey { get { return getString(SmallMoleculeColumn.INCHI_KEY.Order); } set { setValue(SmallMoleculeColumn.INCHI_KEY.Order, MZTabUtils.ParseStringList(MZTabConstants.BAR, value)); } }

        public string Description { get { return getString(SmallMoleculeColumn.DESCRIPTION.Order); } set { setValue(SmallMoleculeColumn.DESCRIPTION.Order, MZTabUtils.ParseString(value)); } }

        public double ExpMassToCharge { get { return getDouble(SmallMoleculeColumn.EXP_MASS_TO_CHARGE.Order); } set { setValue(SmallMoleculeColumn.EXP_MASS_TO_CHARGE.Order, value); } }

        public void setExpMassToCharge(string expMassToChargeLabel){
            ExpMassToCharge = MZTabUtils.ParseDouble(expMassToChargeLabel);
        }

        public double CalcMassToCharge { get { return getDouble(SmallMoleculeColumn.CALC_MASS_TO_CHARGE.Order); } set { setValue(SmallMoleculeColumn.CALC_MASS_TO_CHARGE.Order, value); } }

        public void setCalcMassToCharge(string calcMassToChargeLabel){
            CalcMassToCharge = MZTabUtils.ParseDouble(calcMassToChargeLabel);
        }

        public Integer Charge { get { return getInteger(SmallMoleculeColumn.CHARGE.Order); } set { setValue(SmallMoleculeColumn.CHARGE.Order, value); } }

        public void setCharge(string chargeLabel){
            Charge = MZTabUtils.ParseInteger(chargeLabel);
        }

        public SplitList<double> RetentionTime { get { return GetSplitList<double>(SmallMoleculeColumn.RETENTION_TIME.Order); } set { setValue(SmallMoleculeColumn.RETENTION_TIME.Order, value); } }

        public bool addRetentionTime(double rt){
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

        public bool addRetentionTime(string rtLabel){
            return !string.IsNullOrEmpty(rtLabel) && addRetentionTime(MZTabUtils.ParseDouble(rtLabel));
        }

        public void setRetentionTime(string retentionTimeLabel){
            RetentionTime = MZTabUtils.ParseDoubleList(retentionTimeLabel);
        }

        public Integer Taxid { get { return getInteger(SmallMoleculeColumn.TAXID.Order); } set { setValue(SmallMoleculeColumn.TAXID.Order, value); } }

        public void setTaxid(string taxidLabel){
            Taxid = MZTabUtils.ParseInteger(taxidLabel);
        }

        public string Species { get { return getString(SmallMoleculeColumn.SPECIES.Order); } set { setValue(SmallMoleculeColumn.SPECIES.Order, MZTabUtils.ParseString(value)); } }

        public string Database { get { return getString(SmallMoleculeColumn.DATABASE.Order); } set { setValue(SmallMoleculeColumn.DATABASE.Order, MZTabUtils.ParseString(value)); } }

        public string DatabaseVersion { get { return getString(SmallMoleculeColumn.DATABASE_VERSION.Order); } set { setValue(SmallMoleculeColumn.DATABASE_VERSION.Order, MZTabUtils.ParseString(value)); } }

        public Reliability Reliability { get { return getReliability(SmallMoleculeColumn.RELIABILITY.Order); } set { setValue(SmallMoleculeColumn.RELIABILITY.Order, value); } }


        public void setReliability(string reliabilityLabel){
            Reliability = Reliability.findReliability(reliabilityLabel);
        }

        public Uri URI { get { return getURI(SmallMoleculeColumn.URI.Order); } set { setValue(SmallMoleculeColumn.URI.Order, value); } }

        public void setURI(string uriLabel){
            URI = MZTabUtils.ParseURI(uriLabel);
        }

        public SplitList<SpectraRef> SpectraRef { get { return GetSplitList<SpectraRef>(SmallMoleculeColumn.SPECTRA_REF.Order); } set { setValue(SmallMoleculeColumn.SPECTRA_REF.Order, value); } }

        public bool addSpectraRef(SpectraRef specRef){
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

        public void setSpectraRef(string spectraRefLabel){
            SpectraRef = MZTabUtils.ParseSpectraRefList(metadata, spectraRefLabel);
        }

        public SplitList<Param> SearchEngine { get { return GetSplitList<Param>(SmallMoleculeColumn.SEARCH_ENGINE.Order); } set { setValue(SmallMoleculeColumn.SEARCH_ENGINE.Order, value); } }

        public bool addSearchEngineParam(Param param){
            if (param == null){
                return false;
            }

            SplitList<Param> paramList = SearchEngine;
            if (paramList == null){
                paramList = new SplitList<Param>(MZTabConstants.BAR);
                SearchEngine = paramList;
            }

            paramList.Add(param);
            return true;
        }

        public bool addSearchEngineParam(string paramLabel){
            return !string.IsNullOrEmpty(paramLabel) && addSearchEngineParam(MZTabUtils.ParseParam(paramLabel));
        }


        public void setSearchEngine(string searchEngineLabel){
            SearchEngine = MZTabUtils.ParseParamList(searchEngineLabel);
        }

        public SplitList<Param> BestSearchEngineScore { get { return GetSplitList<Param>(SmallMoleculeColumn.BEST_SEARCH_ENGINE_SCORE.Order); } set { setValue(SmallMoleculeColumn.BEST_SEARCH_ENGINE_SCORE.Order, value); } }

        public bool addBestSearchEngineScoreParam(Param param){
            if (param == null){
                return false;
            }

            SplitList<Param> paramList = BestSearchEngineScore;
            if (paramList == null){
                paramList = new SplitList<Param>(MZTabConstants.BAR);
                BestSearchEngineScore = paramList;
            }

            paramList.Add(param);
            return true;
        }

        public bool addBestSearchEngineScoreParam(string paramLabel){
            return !string.IsNullOrEmpty(paramLabel) && addBestSearchEngineScoreParam(MZTabUtils.ParseParam(paramLabel));
        }

        public void setBestSearchEngineScore(string bestSearchEngineScoreLabel){
            BestSearchEngineScore = MZTabUtils.ParseParamList(bestSearchEngineScoreLabel);
        }


        public SplitList<Param> getSearchEngineScore(MsRun msRun){
            return GetSplitList<Param>(getPosition(SmallMoleculeColumn.SEARCH_ENGINE_SCORE, msRun));
        }

        public void setSearchEngineScore(MsRun msRun, SplitList<Param> searchEngineScore){
            setValue(getPosition(SmallMoleculeColumn.SEARCH_ENGINE_SCORE, msRun), searchEngineScore);
        }

        public void setSearchEngineScore(string logicalPosition, SplitList<Param> searchEngineScore){
            setValue(logicalPosition, searchEngineScore);
        }

        public void setSearchEngineScore(string logicalPosition, string paramsLabel){
            setSearchEngineScore(logicalPosition, MZTabUtils.ParseParamList(paramsLabel));
        }

        public bool addSearchEngineScore(MsRun msRun, CVParam param){
            if (param == null){
                return false;
            }

            SplitList<Param> paramList = getSearchEngineScore(msRun);
            if (paramList == null){
                paramList = new SplitList<Param>(MZTabConstants.BAR);
                setSearchEngineScore(msRun, paramList);
            }
            paramList.Add(param);

            return true;
        }

        public void setSearchEngineScore(MsRun msRun, string paramsLabel){
            setSearchEngineScore(msRun, MZTabUtils.ParseParamList(paramsLabel));
        }


        public SplitList<Modification> Modifications { get { return GetSplitList<Modification>(SmallMoleculeColumn.MODIFICATIONS.Order); } set { setValue(SmallMoleculeColumn.MODIFICATIONS.Order, value); } }

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
            return true;
        }


        public void setModifications(string modificationsLabel){
            Modifications = MZTabUtils.ParseModificationList(Section.Small_Molecule, modificationsLabel);
        }

        /**
     * SML  value1  value2  value3  ...
     */

        public override string ToString(){
            return Section.Small_Molecule.Prefix + MZTabConstants.TAB + base.ToString();
        }
    }
}