using System;
using System.Collections.Generic;
using System.Security.Policy;
using System.Text.RegularExpressions;
using PluginMzTab.Lib.Model;
using PluginMzTab.Lib.Utils.Errors;

namespace PluginMzTab.Lib.Utils.Parser{
/**
* Metadata Element start with MTD, and structure like:
* MTD  {MetadataElement}([id])(-{MetadataProperty})
*
* @see MetadataElement  : Mandatory
* @see MetadataProperty : Optional.
*/

    public class MTDLineParser : MZTabLineParser{
        private const string Error_Header = "Metadata ";

        private readonly Metadata _metadata = new Metadata();

        private readonly Dictionary<string, string> _colUnitMap = new Dictionary<string, string>();

        /**
     * For facing colunit definition line, for example:
     * MTD  colunit-protein retention_time=[UO, UO:000031, minute, ]
     * after parse metadata and header lines, need calling
     * {@link #refineColUnit(uk.ac.ebi.pride.jmztab.model.MZTabColumnFactory)} manually.
     */

        public new void Parse(int lineNumber, string mtdLine, MZTabErrorList errorList){
            base.Parse(lineNumber, mtdLine, errorList);

            if (_items.Length != 3){
                MZTabError error = new MZTabError(FormatErrorType.MTDLine, lineNumber, mtdLine);
                throw new MZTabException(error);
            }

            string defineLabel = _items[1].Trim().ToLower();
            string valueLabel = _items[2].Trim();

            if (defineLabel.Contains("colunit")){
                // ignore colunit parse. In the stage, just store them into colUnitMap<defineLabel, valueLabel>.
                // after table section columns created, call checkColUnit manually.
                _colUnitMap.Add(defineLabel, valueLabel);

                if (! defineLabel.Equals("colunit-protein") &&
                    ! defineLabel.Equals("colunit-peptide") &&
                    ! defineLabel.Equals("colunit-psm") &&
                    ! defineLabel.Equals("colunit-small_molecule")){
                    MZTabError error = new MZTabError(FormatErrorType.MTDDefineLabel, lineNumber, defineLabel);
                    throw new MZTabException(error);
                }
            }
            else{
                parseNormalMetadata(defineLabel, valueLabel);
            }
        }

        private string checkEmail(string defineLabel, string valueLabel){
            string email = MZTabUtils.ParseEmail(valueLabel);

            if (email == null){
                MZTabError error = new MZTabError(FormatErrorType.Email, _lineNumber, Error_Header + defineLabel,
                                                  valueLabel);
                throw new MZTabException(error);
            }

            return email;
        }

        private MetadataProperty checkProperty(MetadataElement element, string propertyName){
            if (string.IsNullOrEmpty(propertyName)){
                return null;
            }

            MetadataProperty property = MetadataProperty.FindProperty(element, propertyName);
            if (property == null){
                MZTabError error = new MZTabError(FormatErrorType.MTDDefineLabel, _lineNumber,
                                                  element.Name + "-" + propertyName);
                throw new MZTabException(error);
            }

            return property;
        }

        private MetadataProperty checkProperty(MetadataSubElement subElement, string propertyName){
            if (string.IsNullOrEmpty(propertyName)){
                return null;
            }

            MetadataProperty property = MetadataProperty.FindProperty(subElement, propertyName);
            if (property == null){
                MZTabError error = new MZTabError(FormatErrorType.MTDDefineLabel, _lineNumber,
                                                  subElement.Name + "-" + propertyName);
                throw new MZTabException(error);
            }

            return property;
        }

        private MzTabMode checkMZTabMode(string defineLabel, string valueLabel){
            try{
                return (MzTabMode) Enum.Parse(typeof (MzTabMode), valueLabel);
            }
            catch (ArgumentException){
                MZTabError error = new MZTabError(FormatErrorType.MZTabMode, _lineNumber, Error_Header + defineLabel,
                                                  valueLabel);
                throw new MZTabException(error);
            }
        }

        private MzTabType checkMZTabType(string defineLabel, string valueLabel){
            try{
                return (MzTabType) Enum.Parse(typeof (MzTabType), valueLabel);
            }
            catch (ArgumentException){
                MZTabError error = new MZTabError(FormatErrorType.MZTabType, _lineNumber, Error_Header + defineLabel,
                                                  valueLabel);
                throw new MZTabException(error);
            }
        }

        private Param checkParam(string defineLabel, string valueLabel){
            Param param = MZTabUtils.ParseParam(valueLabel);
            if (param == null){
                MZTabError error = new MZTabError(FormatErrorType.Param, _lineNumber, Error_Header + defineLabel,
                                                  valueLabel);
                throw new MZTabException(error);
            }

            return param;
        }

        private SplitList<Param> checkParamList(string defineLabel, string valueLabel){
            SplitList<Param> paramList = MZTabUtils.ParseParamList(valueLabel);
            if (paramList == null || paramList.Count == 0){
                MZTabError error = new MZTabError(FormatErrorType.ParamList, _lineNumber, Error_Header + defineLabel,
                                                  valueLabel);
                throw new MZTabException(error);
            }

            return paramList;
        }

        private SplitList<PublicationItem> checkPublication(string defineLabel, string valueLabel){
            SplitList<PublicationItem> publications = MZTabUtils.ParsePublicationItems(valueLabel);
            if (publications.Count == 0){
                MZTabError error = new MZTabError(FormatErrorType.Publication, _lineNumber, Error_Header + defineLabel,
                                                  valueLabel);
                throw new MZTabException(error);
            }

            return publications;
        }

        private Uri checkURI(string defineLabel, string valueLabel){
            Uri uri = MZTabUtils.ParseURI(valueLabel);
            if (uri == null){
                MZTabError error = new MZTabError(FormatErrorType.URI, _lineNumber, Error_Header + defineLabel,
                                                  valueLabel);
                throw new MZTabException(error);
            }

            return uri;
        }

        private Url checkURL(string defineLabel, string valueLabel){
            Url url = MZTabUtils.ParseUrl(valueLabel);
            if (url == null){
                MZTabError error = new MZTabError(FormatErrorType.URL, _lineNumber, Error_Header + defineLabel,
                                                  valueLabel);
                throw new MZTabException(error);
            }

            return url;
        }

        // the id is not correct number in the define label.
        private int checkIndex(string defineLabel, string id){
            try{
                int index = int.Parse(id);
                if (index < 1){
                    throw new FormatException();
                }

                return index;
            }
            catch (FormatException){
                MZTabError error = new MZTabError(LogicalErrorType.IdNumber, _lineNumber, Error_Header + defineLabel, id);
                throw new MZTabException(error);
            }
        }

        private IndexedElement checkIndexedElement(string defineLabel, string valueLabel, MetadataElement element){
            IndexedElement indexedElement = MZTabUtils.ParseIndexedElement(valueLabel, element);
            if (indexedElement == null){
                MZTabError error = new MZTabError(FormatErrorType.IndexedElement, _lineNumber,
                                                  Error_Header + defineLabel, valueLabel);
                throw new MZTabException(error);
            }

            return indexedElement;
        }

        private List<IndexedElement> checkIndexedElementList(string defineLabel, string valueLabel,
                                                             MetadataElement element){
            List<IndexedElement> indexedElementList = MZTabUtils.ParseIndexedElementList(valueLabel, element);
            if (indexedElementList == null || indexedElementList.Count == 0){
                MZTabError error = new MZTabError(FormatErrorType.IndexedElement, _lineNumber,
                                                  Error_Header + defineLabel, valueLabel);
                throw new MZTabException(error);
            }

            return indexedElementList;
        }

        /**
     * The metadata line including three parts:
     * MTD  {define}    {value}
     *
     * In normal, define label structure like:
     * {element}([{id}])(-property)
     *
     * ([{id}]) and {-property} are optional.
     *
     * parse label and generate Unit, MetadataElement, id, MetadataProperty objects.
     * If optional item not exists, return null.
     */

        private void parseNormalMetadata(string defineLabel, string valueLabel){
            Regex regex = new Regex("(\\w+)(\\[(\\w+)\\])?(-(\\w+)(\\[(\\w+)\\])?)?(-(\\w+))?");

            if (regex.IsMatch(defineLabel)){
                Match match = regex.Match(defineLabel);

                // Stage 1: create Unit.
                MetadataElement element = MetadataElement.findElement(match.Groups[1].Value);

                int id;
                MetadataProperty property;
                Param param;
                SplitList<Param> paramList;
                IndexedElement indexedElement;
                List<IndexedElement> indexedElementList;
                if (element.Equals(MetadataElement.MZTAB)){
                    property = checkProperty(element, match.Groups[5].Value);
                    if (property.Equals(MetadataProperty.MZTAB_VERSION)){
                        _metadata.MZTabVersion = valueLabel;
                    }
                    else if (property.Equals(MetadataProperty.MZTAB_MODE)){
                        _metadata.MzTabMode = checkMZTabMode(defineLabel, valueLabel);
                    }
                    else if (property.Equals(MetadataProperty.MZTAB_TYPE)){
                        _metadata.MzTabType = checkMZTabType(defineLabel, valueLabel);
                    }
                    else if (property.Equals(MetadataProperty.MZTAB_ID)){
                        if (_metadata.MZTabID != null){
                            throw new MZTabException(new MZTabError(LogicalErrorType.DuplicationDefine, _lineNumber,
                                                                    defineLabel));
                        }
                        _metadata.MZTabID = valueLabel;
                    }
                }
                else if (element.Equals(MetadataElement.TITLE)){
                    if (_metadata.Title != null){
                        throw new MZTabException(new MZTabError(LogicalErrorType.DuplicationDefine, _lineNumber,
                                                                defineLabel));
                    }
                    _metadata.Title = valueLabel;
                }
                else if (element.Equals(MetadataElement.DESCRIPTION)){
                    if (_metadata.Description != null){
                        throw new MZTabException(new MZTabError(LogicalErrorType.DuplicationDefine, _lineNumber,
                                                                defineLabel));
                    }
                    _metadata.Description = valueLabel;
                }
                else if (element.Equals(MetadataElement.SAMPLE_PROCESSING)){
                    id = checkIndex(defineLabel, match.Groups[3].Value);
                    _metadata.AddSampleProcessing(id, checkParamList(defineLabel, valueLabel));
                }
                else if (element.Equals(MetadataElement.INSTRUMENT)){
                    id = checkIndex(defineLabel, match.Groups[3].Value);
                    property = checkProperty(element, match.Groups[5].Value);
                    param = checkParam(defineLabel, valueLabel);

                    if (property.Equals(MetadataProperty.INSTRUMENT_NAME)){
                        _metadata.AddInstrumentName(id, param);
                    }
                    else if (property.Equals(MetadataProperty.INSTRUMENT_SOURCE)){
                        _metadata.AddInstrumentSource(id, param);
                    }
                    else if (property.Equals(MetadataProperty.INSTRUMENT_ANALYZER)){
                        _metadata.AddInstrumentAnalyzer(id, param);
                    }
                    else if (property.Equals(MetadataProperty.INSTRUMENT_DETECTOR)){
                        _metadata.AddInstrumentDetector(id, param);
                    }
                }
                else if (element.Equals(MetadataElement.SOFTWARE)){
                    id = checkIndex(defineLabel, match.Groups[3].Value);
                    property = checkProperty(element, match.Groups[5].Value);
                    if (property == null){
                        param = checkParam(defineLabel, valueLabel);
                        if (param.Value == null || param.Value.Trim().Length == 0){
                            // this is a warn.
                            _errorList.Add(new MZTabError(LogicalErrorType.SoftwareVersion, _lineNumber, valueLabel));
                        }
                        _metadata.AddSoftwareParam(id, param);
                    }
                    else if (property.Equals(MetadataProperty.SOFTWARE_SETTING)){
                        _metadata.AddSoftwareSetting(id, valueLabel);
                    }
                }
                else if (element.Equals(MetadataElement.FALSE_DISCOVERY_RATE)){
                    if (_metadata.FalseDiscoveryRate.Count > 0){
                        throw new MZTabException(new MZTabError(LogicalErrorType.DuplicationDefine, _lineNumber,
                                                                defineLabel));
                    }
                    paramList = checkParamList(defineLabel, valueLabel);
                    _metadata.FalseDiscoveryRate = paramList;
                }
                else if (element.Equals(MetadataElement.PUBLICATION)){
                    id = checkIndex(defineLabel, match.Groups[3].Value);
                    _metadata.AddPublicationItems(id, checkPublication(defineLabel, valueLabel));
                }
                else if (element.Equals(MetadataElement.CONTACT)){
                    id = checkIndex(defineLabel, match.Groups[3].Value);
                    property = checkProperty(element, match.Groups[5].Value);
                    if (property.Equals(MetadataProperty.CONTACT_NAME)){
                        _metadata.AddContactName(id, valueLabel);
                    }
                    else if (property.Equals(MetadataProperty.CONTACT_NAME)){
                        _metadata.AddContactAffiliation(id, valueLabel);
                    }
                    else if (property.Equals(MetadataProperty.CONTACT_NAME)){
                        _metadata.AddContactEmail(id, checkEmail(defineLabel, valueLabel));
                    }
                }
                else if (element.Equals(MetadataElement.URI)){
                    _metadata.AddUri(checkURI(defineLabel, valueLabel));
                }
                else if (element.Equals(MetadataElement.FIXED_MOD)){
                    id = checkIndex(defineLabel, match.Groups[3].Value);
                    property = checkProperty(element, match.Groups[5].Value);
                    if (property == null){
                        param = checkParam(defineLabel, valueLabel);
                        if (param == null){
                            // fixed modification parameter should be setting.
                            _errorList.Add(new MZTabError(FormatErrorType.Param, _lineNumber, valueLabel));
                        }
                        else{
                            _metadata.AddFixedModParam(id, param);
                        }
                    }
                    else{
                        if (property.Equals(MetadataProperty.FIXED_MOD_POSITION)){
                            _metadata.AddFixedModPosition(id, valueLabel);
                        }
                        else if (property.Equals(MetadataProperty.FIXED_MOD_SITE)){
                            _metadata.AddFixedModSite(id, valueLabel);
                        }
                    }
                }
                else if (element.Equals(MetadataElement.VARIABLE_MOD)){
                    id = checkIndex(defineLabel, match.Groups[3].Value);
                    property = checkProperty(element, match.Groups[5].Value);
                    if (property == null){
                        param = checkParam(defineLabel, valueLabel);
                        if (param == null){
                            // variable modification parameter should be setting.
                            _errorList.Add(new MZTabError(FormatErrorType.Param, _lineNumber, valueLabel));
                        }
                        else{
                            _metadata.AddVariableModParam(id, param);
                        }
                    }
                    else{
                        if (property.Equals(MetadataProperty.FIXED_MOD_POSITION)){
                            _metadata.AddVariableModPosition(id, valueLabel);
                        }
                        else if (property.Equals(MetadataProperty.FIXED_MOD_SITE)){
                            _metadata.AddVariableModSite(id, valueLabel);
                        }
                    }
                }
                else if (element.Equals(MetadataElement.QUANTIFICATION_METHOD)){
                    if (_metadata.QuantificationMethod != null){
                        throw new MZTabException(new MZTabError(LogicalErrorType.DuplicationDefine, _lineNumber,
                                                                defineLabel));
                    }
                    _metadata.QuantificationMethod = checkParam(defineLabel, valueLabel);
                }
                else if (element.Equals(MetadataElement.PROTEIN)){
                    property = checkProperty(element, match.Groups[5].Value);
                    if (property.Equals(MetadataProperty.PROTEIN_QUANTIFICATION_UNIT)){
                        if (_metadata.ProteinQuantificationUnit != null){
                            throw new MZTabException(new MZTabError(LogicalErrorType.DuplicationDefine, _lineNumber,
                                                                    defineLabel));
                        }
                        _metadata.ProteinQuantificationUnit = checkParam(defineLabel, valueLabel);
                    }
                }
                else if (element.Equals(MetadataElement.PEPTIDE)){
                    property = checkProperty(element, match.Groups[5].Value);
                    if (property.Equals(MetadataProperty.PEPTIDE_QUANTIFICATION_UNIT)){
                        if (_metadata.PeptideQuantificationUnit != null){
                            throw new MZTabException(new MZTabError(LogicalErrorType.DuplicationDefine, _lineNumber,
                                                                    defineLabel));
                        }
                        _metadata.PeptideQuantificationUnit = checkParam(defineLabel, valueLabel);
                    }
                }
                else if (element.Equals(MetadataElement.SMALL_MOLECULE)){
                    property = checkProperty(element, match.Groups[5].Value);
                    if (property.Equals(MetadataProperty.SMALL_MOLECULE_QUANTIFICATION_UNIT)){
                        if (_metadata.SmallMoleculeQuantificationUnit != null){
                            throw new MZTabException(new MZTabError(LogicalErrorType.DuplicationDefine, _lineNumber,
                                                                    defineLabel));
                        }
                        _metadata.SmallMoleculeQuantificationUnit = checkParam(defineLabel, valueLabel);
                    }
                }
                else if (element.Equals(MetadataElement.MS_RUN)){
                    id = checkIndex(defineLabel, match.Groups[3].Value);
                    property = checkProperty(element, match.Groups[5].Value);
                    if (property.Equals(MetadataProperty.MS_RUN_FORMAT)){
                        _metadata.AddMsRunFormat(id, checkParam(defineLabel, valueLabel));
                    }
                    else if (property.Equals(MetadataProperty.MS_RUN_LOCATION)){
                        _metadata.AddMsRunLocation(id, checkURL(defineLabel, valueLabel));
                    }
                    else if (property.Equals(MetadataProperty.MS_RUN_ID_FORMAT)){
                        _metadata.AddMsRunIdFormat(id, checkParam(defineLabel, valueLabel));
                    }
                    else if (property.Equals(MetadataProperty.MS_RUN_FRAGMENTATION_METHOD)){
                        _metadata.AddMsRunFragmentationMethod(id, checkParam(defineLabel, valueLabel));
                    }
                }
                else if (element.Equals(MetadataElement.CUSTOM)){
                    _metadata.AddCustom(checkParam(defineLabel, valueLabel));
                }
                else if (element.Equals(MetadataElement.SAMPLE)){
                    id = checkIndex(defineLabel, match.Groups[3].Value);
                    property = checkProperty(element, match.Groups[5].Value);
                    if (property.Equals(MetadataProperty.SAMPLE_SPECIES)){
                        _metadata.AddSampleSpecies(id, checkParam(defineLabel, valueLabel));
                    }
                    else if (property.Equals(MetadataProperty.SAMPLE_TISSUE)){
                        _metadata.AddSampleTissue(id, checkParam(defineLabel, valueLabel));
                    }
                    else if (property.Equals(MetadataProperty.SAMPLE_CELL_TYPE)){
                        _metadata.AddSampleCellType(id, checkParam(defineLabel, valueLabel));
                    }
                    else if (property.Equals(MetadataProperty.SAMPLE_DISEASE)){
                        _metadata.AddSampleDisease(id, checkParam(defineLabel, valueLabel));
                    }
                    else if (property.Equals(MetadataProperty.SAMPLE_DESCRIPTION)){
                        _metadata.AddSampleDescription(id, valueLabel);
                    }
                    else if (property.Equals(MetadataProperty.SAMPLE_CUSTOM)){
                        _metadata.AddSampleCustom(id, checkParam(defineLabel, valueLabel));
                    }
                }
                else if (element.Equals(MetadataElement.ASSAY)){
                    if (string.IsNullOrEmpty(match.Groups[6].Value)){
                        // no quantification modification. For example: assay[1-n]-quantification_reagent
                        id = checkIndex(defineLabel, match.Groups[3].Value);
                        property = checkProperty(element, match.Groups[5].Value);
                        if (property.Equals(MetadataProperty.ASSAY_QUANTIFICATION_REAGENT)){
                            _metadata.AddAssayQuantificationReagent(id, checkParam(defineLabel, valueLabel));
                        }
                        else if (property.Equals(MetadataProperty.ASSAY_SAMPLE_REF)){
                            indexedElement = checkIndexedElement(defineLabel, valueLabel, MetadataElement.SAMPLE);
                            Sample sample = _metadata.SampleMap[indexedElement.Id];
                            if (sample == null){
                                throw new MZTabException(new MZTabError(LogicalErrorType.MsRunNotDefined, _lineNumber,
                                                                        valueLabel));
                            }
                            _metadata.AddAssaySample(id, sample);
                        }
                        else if (property.Equals(MetadataProperty.ASSAY_MS_RUN_REF)){
                            indexedElement = checkIndexedElement(defineLabel, valueLabel, MetadataElement.MS_RUN);
                            MsRun msRun = _metadata.MsRunMap[indexedElement.Id];
                            if (msRun == null){
                                throw new MZTabException(new MZTabError(LogicalErrorType.MsRunNotDefined, _lineNumber,
                                                                        valueLabel));
                            }
                            _metadata.AddAssayMsRun(id, msRun);
                        }
                    }
                    else{
                        // quantification modification. For example: assay[1]-quantification_mod[1], assay[1]-quantification_mod[1]-site
                        id = checkIndex(defineLabel, match.Groups[3].Value);
                        MetadataSubElement subElement = MetadataSubElement.FindSubElement(element, match.Groups[5].Value);
                        if (subElement.Equals(MetadataSubElement.ASSAY_QUANTIFICATION_MOD)){
                            int modId = checkIndex(defineLabel, match.Groups[7].Value);
                            property = checkProperty(subElement, match.Groups[9].Value);
                            if (property == null){
                                _metadata.AddAssayQuantificationModParam(id, modId, checkParam(defineLabel, valueLabel));
                            }
                            else{
                                if (property.Equals(MetadataProperty.ASSAY_QUANTIFICATION_MOD_SITE)){
                                    _metadata.AddAssayQuantificationModSite(id, modId, valueLabel);
                                }
                                else if (property.Equals(MetadataProperty.ASSAY_QUANTIFICATION_MOD_POSITION)){
                                    _metadata.AddAssayQuantificationModPosition(id, modId, valueLabel);
                                }
                            }
                        }
                    }
                }
                else if (element.Equals(MetadataElement.STUDY_VARIABLE)){
                    id = checkIndex(defineLabel, match.Groups[3].Value);
                    property = checkProperty(element, match.Groups[5].Value);
                    if (property.Equals(MetadataProperty.STUDY_VARIABLE_ASSAY_REFS)){
                        indexedElementList = checkIndexedElementList(defineLabel, valueLabel, MetadataElement.ASSAY);
                        foreach (IndexedElement e in indexedElementList){
                            if (! _metadata.AssayMap.ContainsKey(e.Id)){
                                // can not find assay[id] in metadata.
                                throw new MZTabException(new MZTabError(LogicalErrorType.MsRunNotDefined, _lineNumber,
                                                                        valueLabel));
                            }
                            _metadata.AddStudyVariableAssay(id, _metadata.AssayMap[e.Id]);
                        }
                    }
                    else if (property.Equals(MetadataProperty.STUDY_VARIABLE_SAMPLE_REFS)){
                        indexedElementList = checkIndexedElementList(defineLabel, valueLabel, MetadataElement.SAMPLE);
                        foreach (IndexedElement e in indexedElementList){
                            if (! _metadata.SampleMap.ContainsKey(e.Id)){
                                // can not find assay[id] in metadata.
                                throw new MZTabException(new MZTabError(LogicalErrorType.MsRunNotDefined, _lineNumber,
                                                                        valueLabel));
                            }
                            _metadata.AddStudyVariableSample(id, _metadata.SampleMap[e.Id]);
                        }
                    }
                    else if (property.Equals(MetadataProperty.STUDY_VARIABLE_DESCRIPTION)){
                        _metadata.AddStudyVariableDescription(id, valueLabel);
                    }
                }
                else if (element.Equals(MetadataElement.CV)){
                    id = checkIndex(defineLabel, match.Groups[3].Value);
                    property = checkProperty(element, match.Groups[5].Value);
                    if (property.Equals(MetadataProperty.CV_LABEL)){
                        _metadata.AddCVLabel(id, valueLabel);
                    }
                    else if (property.Equals(MetadataProperty.CV_FULL_NAME)){
                        _metadata.AddCVFullName(id, valueLabel);
                    }
                    else if (property.Equals(MetadataProperty.CV_VERSION)){
                        _metadata.AddCVVersion(id, valueLabel);
                    }
                    else if (property.Equals(MetadataProperty.CV_URL)){
                        _metadata.AddCVURL(id, valueLabel);
                    }
                }
            }
            else{
                throw new MZTabException(new MZTabError(FormatErrorType.MTDLine, _lineNumber, _line));
            }
        }

        /**
     * Refine the metadata, and check whether missing some important information.
     * fixed_mode, variable_mode must provide in the Complete file.
     * Detail information see specification 5.5
     */

        public void refineNormalMetadata(){
            MzTabMode mode = _metadata.MzTabMode;
            MzTabType type = _metadata.MzTabType;

            SortedDictionary<int, StudyVariable> svMap = _metadata.StudyVariableMap;
            SortedDictionary<int, Assay> assayMap = _metadata.AssayMap;
            SortedDictionary<int, MsRun> runMap = _metadata.MsRunMap;

            if (mode == MzTabMode.Complete){
                if (_metadata.SoftwareMap.Count == 0){
                    throw new MZTabException(new MZTabError(LogicalErrorType.NotDefineInMetadata, _lineNumber,
                                                            "software[1-n]", mode.ToString(), type.ToString()));
                }

                if (type == MzTabType.Quantification){
                    if (_metadata.QuantificationMethod == null){
                        throw new MZTabException(new MZTabError(LogicalErrorType.NotDefineInMetadata, _lineNumber,
                                                                "quantification_method", mode.ToString(),
                                                                type.ToString()));
                    }
                    foreach (int id in assayMap.Keys){
                        if (assayMap[id].MsRun == null){
                            throw new MZTabException(new MZTabError(LogicalErrorType.NotDefineInMetadata, _lineNumber,
                                                                    "assay[" + id + "]-ms_run_ref", mode.ToString(),
                                                                    type.ToString()));
                        }
                        if (assayMap[id].QuantificationReagent == null){
                            throw new MZTabException(new MZTabError(LogicalErrorType.NotDefineInMetadata, _lineNumber,
                                                                    "assay[" + id + "]-quantification_reagent",
                                                                    mode.ToString(), type.ToString()));
                        }
                    }
                    if (svMap.Count > 0 && assayMap.Count > 0){
                        foreach (int id in svMap.Keys){
                            if (svMap[id].AssayMap.Count == 0){
                                throw new MZTabException(new MZTabError(LogicalErrorType.AssayRefs, _lineNumber,
                                                                        "study_variable[" + id + "]-assay_refs"));
                            }
                        }
                    }
                }
            }

            // Complete and Summary should provide following information.
            // mzTab-version, mzTab-mode and mzTab-type have default values in create metadata. Not check here.
            if (_metadata.Description == null){
                throw new MZTabException(new MZTabError(LogicalErrorType.NotDefineInMetadata, _lineNumber, "description",
                                                        mode.ToString(), type.ToString()));
            }
            foreach (int id in runMap.Keys){
                if (runMap[id].Location == null){
                    throw new MZTabException(new MZTabError(LogicalErrorType.NotDefineInMetadata, _lineNumber,
                                                            "ms_run[" + id + "]-location", mode.ToString(),
                                                            type.ToString()));
                }
            }

            if (type == MzTabType.Quantification){
                foreach (int id in svMap.Keys){
                    if (svMap[id].Description == null){
                        throw new MZTabException(new MZTabError(LogicalErrorType.NotDefineInMetadata, _lineNumber,
                                                                "study_variable[" + id + "]-description",
                                                                mode.ToString(), type.ToString()));
                    }
                }
            }
        }

        /**
     * valueLabel pattern like: column_name=param_string
     */

        private MZTabError checkColUnit(string valueLabel, MZTabColumnFactory factory){
            string[] items = valueLabel.Split('=');
            string columnName = items[0].Trim();
            string value = items[1].Trim();

            MZTabColumn column = factory.FindColumnByHeader(columnName);
            if (column == null){
                // column_name not exists in the factory.
                return new MZTabError(FormatErrorType.ColUnit, _lineNumber, valueLabel, columnName);
            }
            Param param = MZTabUtils.ParseParam(value);
            if (param == null){
                return new MZTabError(FormatErrorType.Param, _lineNumber, valueLabel, value);
            }
            if (factory.Section.Equals(Section.Protein_Header)){
                _metadata.AddProteinColUnit(column, param);
            }
            else if (factory.Section.Equals(Section.Peptide_Header)){
                _metadata.AddPeptideColUnit(column, param);
            }
            else if (factory.Section.Equals(Section.PSM_Header)){
                _metadata.AddPSMColUnit(column, param);
            }
            else if (factory.Section.Equals(Section.Small_Molecule_Header)){
                _metadata.AddSmallMoleculeColUnit(column, param);
            }

            return null;
        }

        /**
     * Based on factory, navigate colUnitMap<defineLabel, valueLabel>
     * and refine colunit columns are correct or not.
     *
     * Notice: after refined phase, colunit definition can be record in the metadata.
     */

        public void refineColUnit(MZTabColumnFactory factory){
            foreach (string defineLabel in _colUnitMap.Keys){
                if (defineLabel.Equals("colunit-" + Section.toDataSection(factory.Section).Name,
                                       StringComparison.InvariantCultureIgnoreCase)){
                    string valueLabel = _colUnitMap[defineLabel];
                    MZTabError error = checkColUnit(valueLabel, factory);

                    if (error != null){
                        throw new MZTabException(error);
                    }
                }
            }
        }

        public Metadata Metadata { get { return _metadata; } }
    }
}