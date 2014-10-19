using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using PluginMzTab.Lib.Model;
using PluginMzTab.Lib.Utils.Errors;
using PluginMzTab.Plugin.Utils;

namespace PluginMzTab.Lib.Utils.Parser{
/**
 * A couple of common method used to parse a data line into {@link MZTabRecord} structure. There are two step
 * in data line parser process: step 1: data validate, these methods name start with "checkXXX()" focus on this.
 * step 2: after validate, we fill the cell data into {@link MZTabRecord}, and use "loadXXX()" to generate the
 * concrete record.
 *
 * NOTICE: {@link MZTabColumnFactory} maintain a couple of {@link MZTabColumn} which have internal logical
 * position and order. In physical mzTab file, we allow user not obey this logical position organized way,
 * and provide their date with own order. In order to distinguish them, we use physical position (a positive
 * integer) to record the column location in mzTab file. And use {@link PositionMapping} structure the maintain
 * the mapping between them.
 *
 * @see PRTLineParser
 * @see PEPLineParser
 * @see PSMLineParser
 * @see SMLLineParser
 */

    public abstract class MZTabDataLineParser : MZTabLineParser{
        protected MZTabColumnFactory factory;
        protected PositionMapping positionMapping;
        protected Dictionary<string, int> exchangeMapping; // reverse the key and value of positionMapping.

        protected SortedDictionary<int, MZTabColumn> mapping; // logical position --> offset
        protected Metadata metadata;

        /**
     * Generate a mzTab data line parser. A couple of common method used to parse a data line into
     * {@link MZTabRecord} structure.
     *
     * NOTICE: {@link MZTabColumnFactory} maintain a couple of {@link MZTabColumn} which have internal logical
     * position and order. In physical mzTab file, we allow user not obey this logical position organized way,
     * and provide their date with own order. In order to distinguish them, we use physical position (a positive
     * integer) to record the column location in mzTab file. And use {@link PositionMapping} structure the maintain
     * the mapping between them.
     *
     * @param factory SHOULD NOT set null
     * @param positionMapping SHOULD NOT set null
     * @param metadata SHOULD NOT set null
     */

        protected MZTabDataLineParser(MZTabColumnFactory factory, PositionMapping positionMapping,
                                      Metadata metadata, MZTabErrorList errorList){
            if (factory == null){
                throw new NullReferenceException("Column header factory should be create first.");
            }
            this.factory = factory;

            this.positionMapping = positionMapping;
            exchangeMapping = positionMapping.exchange();

            mapping = factory.GetOffsetColumnsMap();

            if (metadata == null){
                throw new NullReferenceException("Metadata should be parser first.");
            }
            this.metadata = metadata;
            _errorList = errorList ?? new MZTabErrorList();
        }

        /**
     * Validate the data line, if there exist errors, add them into {@link MZTabErrorList}.
     *
     * NOTICE: this step just do validate, not do convert operation. Convert the data line into
     * {@link MZTabRecord} implemented by {@link #getRecord(uk.ac.ebi.pride.jmztab.model.Section, string)}
     * method.
     */

        public new void Parse(int lineNumber, string line, MZTabErrorList errorList){
            base.Parse(lineNumber, line, errorList);
            checkCount();
            checkStableData();
            checkOptionalData();
        }

        /**
     * Check header line items size equals data line items size.
     * The number of Data line items does not match with the number of Header line items. Normally,
     * the user has not used the Unicode Horizontal Tab character (Unicode codepoint 0009) as the
     * column delimiter, there is a file encoding error, or the user has not provided the definition
     * of optional columns in the header line.
     */

        private void checkCount(){
            int headerCount = mapping.Count;
            int dataCount = _items.Length - 1;

            if (headerCount != dataCount){
                _errorList.Add(new MZTabError(FormatErrorType.CountMatch, _lineNumber, "" + dataCount, "" + headerCount));
            }
        }

        /**
     * Translate the data line to a {@link MZTabRecord}.
     *
     * NOTICE: Normally, we suggest user do convert operation after validate successfully.
     *
     * @see #parse(int, string, uk.ac.ebi.pride.jmztab.utils.errors.MZTabErrorList)
     */

        protected MZTabRecord getRecord(Section section, string line){
            MZTabRecord record = null;

            if (section.Equals(Section.Protein)){
                record = new Protein(factory);
            }
            else if (section.Equals(Section.Peptide)){
                record = new Peptide(factory, metadata);
            }
            else if (section.Equals(Section.PSM)){
                record = new PSM(factory, metadata);
            }
            else if (section.Equals(Section.Small_Molecule)){
                record = new SmallMolecule(factory, metadata);
            }

            int offset = loadStableData(record, line);
            if (offset == _items.Length - 1){
                return record;
            }

            loadOptionalData(record, offset);

            return record;
        }

        /**
     * Check and translate the stable columns and optional columns with stable order into mzTab elements.
     */
        protected abstract void checkStableData();

        /**
     * Load mzTab element and generate a {@link MZTabRecord}.
     */
        protected abstract int loadStableData(MZTabRecord record, string line);

        private void checkOptionalData(){
            for (int physicalPosition = 1; physicalPosition < _items.Length; physicalPosition++){
                MZTabColumn column = null;
                string key = positionMapping.get(physicalPosition);
                if (factory.ColumnMapping.ContainsKey(key)){
                    column = factory.ColumnMapping[key];
                }
                if (column != null){
                    string target = _items[physicalPosition];
                    Type dataType = column.GetType();
                    if (column is AbundanceColumn){
                        checkDouble(column, target);
                    }
                    else if (column is OptionColumn){
                        if (dataType == typeof (string)){
                            checkString(column, target);
                        }
                        else if (dataType == typeof (double)){
                            checkDouble(column, target);
                        }
                        else if (dataType == typeof (MZBoolean)){
                            checkMZBoolean(column, target);
                        }
                    }
                }
            }
        }

        private void loadOptionalData(MZTabRecord record, int physicalOffset){
            for (int physicalPosition = physicalOffset; physicalPosition < _items.Length; physicalPosition++){
                string target = _items[physicalPosition].Trim();
                MZTabColumn column = null;
                string key = positionMapping.get(physicalPosition);
                if (factory.ColumnMapping.ContainsKey(key)){
                    column = factory.ColumnMapping[key];
                }
                if (column != null){
                    Type dataType = column.Type;

                    if (dataType == typeof (string)){
                        record.setValue(column.LogicPosition, checkString(column, target));
                    }
                    else if (dataType == typeof (double)){
                        record.setValue(column.LogicPosition, checkDouble(column, target));
                    }
                    else if (dataType == typeof (MZBoolean)){
                        record.setValue(column.LogicPosition, checkMZBoolean(column, target));
                    }
                }
            }
        }

        /**
     * In the table-based sections (protein, peptide, and small molecule) there MUST NOT be any empty cells.
     * Some field not allow "null" value, for example unit_id, accession and so on. In "Complete" file, in
     * general "null" values SHOULD not be given.
     *
     * @param column SHOULD NOT set null
     * @param target SHOULD NOT be empty.
     */

        protected string checkData(MZTabColumn column, string target, bool allowNull){
            if (target == null){
                _errorList.Add(new MZTabError(LogicalErrorType.NULL, _lineNumber, column.Header));
                return null;
            }

            target = target.Trim();
            if (string.IsNullOrEmpty(target)){
                _errorList.Add(new MZTabError(LogicalErrorType.NULL, _lineNumber, column.Header));
                return null;
            }

            if (target.Equals("NULL", StringComparison.CurrentCultureIgnoreCase)){
                if (! allowNull || metadata.MzTabMode == MzTabMode.Complete){
                    _errorList.Add(new MZTabError(LogicalErrorType.NotNULL, _lineNumber, column.Header));
                }
            }

            return target;
        }

        /**
     * In the table-based sections (protein, peptide, and small molecule) there MUST NOT be any empty cells.
     * Some field not allow "null" value, for example unit_id, accession and so on. In "Complete" file, in
     * general "null" values SHOULD not be given.
     *
     * @param column SHOULD NOT set null
     * @param target SHOULD NOT be empty.
     */

        protected string checkString(MZTabColumn column, string target){
            return checkData(column, target, true);
        }

        /**
     * Check and translate target string into int. If parse incorrect, raise {@link FormatErrorType#int} error.
     *
     * @param column SHOULD NOT set null
     * @param target SHOULD NOT be empty.
     */

        protected Integer checkInteger(MZTabColumn column, string target) {
            string result = checkData(column, target, true);

            if (result == null || result.Equals("NULL", StringComparison.CurrentCultureIgnoreCase)){
                return null;
            }

            Integer value = MZTabUtils.ParseInteger(result);
            if (value == null){
                _errorList.Add(new MZTabError(FormatErrorType.Integer, _lineNumber, column.Header, target));
            }

            return value;
        }

        /**
     * Check and translate target string into double. If parse incorrect, raise {@link FormatErrorType#double} error.
     *
     * NOTICE: If ratios are included and the denominator is zero, the "INF" value MUST be used. If the result leads
     * to calculation errors (for example 0/0), this MUST be reported as "not a number" ("NaN").
     *
     * @param column SHOULD NOT set null
     * @param target SHOULD NOT be empty.
     */

        protected double checkDouble(MZTabColumn column, string target){
            string result = checkData(column, target, true);

            if (result == null || result.Equals("NULL", StringComparison.CurrentCultureIgnoreCase)){
                return double.MinValue;
            }

            double value = MZTabUtils.ParseDouble(result);
            if (value.Equals(double.MinValue)){
                _errorList.Add(new MZTabError(FormatErrorType.Double, _lineNumber, column.Header, target));
                return double.MinValue;
            }
            if (value.Equals(double.NaN) || double.IsPositiveInfinity(value)){
                return value;
            }

            return value;
        }

        /**
     * Check and translate target string into parameter list which split by '|' character..
     * If parse incorrect, raise {@link FormatErrorType#ParamList} error.
     *
     * @param column SHOULD NOT set null
     * @param target SHOULD NOT be empty.
     */

        protected SplitList<Param> checkParamList(MZTabColumn column, string target){
            string result = checkData(column, target, true);

            if (result == null || result.Equals("NULL", StringComparison.CurrentCultureIgnoreCase)){
                return new SplitList<Param>(MZTabConstants.BAR);
            }

            SplitList<Param> paramList = MZTabUtils.ParseParamList(result);
            if (paramList.Count == 0){
                _errorList.Add(new MZTabError(FormatErrorType.ParamList, _lineNumber, "Column " + column.Header, target));
            }

            return paramList;
        }

        /**
     * Check and translate target string into parameter list which split by splitChar character..
     * If parse incorrect, raise {@link FormatErrorType#StringList} error.
     *
     * @param column SHOULD NOT set null
     * @param target SHOULD NOT be empty.
     */

        protected SplitList<string> checkStringList(MZTabColumn column, string target, char splitChar){
            string result = checkData(column, target, true);

            if (result == null || result.Equals("NULL", StringComparison.CurrentCultureIgnoreCase)){
                return new SplitList<string>(splitChar);
            }

            SplitList<string> stringList = MZTabUtils.ParseStringList(splitChar, result);
            if (stringList.Count == 0){
                _errorList.Add(new MZTabError(FormatErrorType.StringList, _lineNumber, column.Header, result,
                                              "" + splitChar));
            }

            return stringList;
        }

        /**
     * Check and translate target to {@link MZBoolean}. Only "0" and "1" allow used in express Boolean (0/1).
     * If parse incorrect, raise {@link FormatErrorType#MZBoolean} error.
     *
     * @param column SHOULD NOT set null
     * @param target SHOULD NOT be empty.
     */

        protected MZBoolean checkMZBoolean(MZTabColumn column, string target){
            string result = checkData(column, target, true);

            if (result == null || result.Equals("NULL", StringComparison.CurrentCultureIgnoreCase)){
                return null;
            }

            MZBoolean value = MZBoolean.FindBoolean(result);
            if (value == null){
                _errorList.Add(new MZTabError(FormatErrorType.MZBoolean, _lineNumber, column.Header, result));
            }

            return value;
        }

        /**
     * Check target string. Normally, description can set "null". But
     * in "Complete" file, in general "null" values SHOULD not be given.
     *
     * @see #checkData(uk.ac.ebi.pride.jmztab.model.MZTabColumn, string, boolean)
     *
     * @param column SHOULD NOT set null
     * @param description SHOULD NOT be empty.
     */

        protected string checkDescription(MZTabColumn column, string description){
            return checkData(column, description, true);
        }

        /**
     * Check and translate taxid string into int. If exists error during parse, raise {@link FormatErrorType#int} error.
     * Normally, taxid can set "null", but in "Complete" file, in general "null" values SHOULD not be given.
     *
     * @param column SHOULD NOT set null
     * @param taxid SHOULD NOT be empty.
     */

        protected Integer checkTaxid(MZTabColumn column, string taxid){
            return checkInteger(column, taxid);
        }

        /**
     * Check target string. Normally, species can set "null". But
     * in "Complete" file, in general "null" values SHOULD not be given.
     *
     * @see #checkData(uk.ac.ebi.pride.jmztab.model.MZTabColumn, string, boolean)
     *
     * @param column SHOULD NOT set null
     * @param species SHOULD NOT be empty.
     */

        protected string checkSpecies(MZTabColumn column, string species){
            return checkData(column, species, true);
        }

        /**
     * Check target string. Normally, database can set "null". But
     * in "Complete" file, in general "null" values SHOULD not be given.
     *
     * @see #checkData(uk.ac.ebi.pride.jmztab.model.MZTabColumn, string, boolean)
     *
     * @param column SHOULD NOT set null
     * @param database SHOULD NOT be empty.
     */

        protected string checkDatabase(MZTabColumn column, string database){
            return checkData(column, database, true);
        }

        /**
     * Check target string. Normally, databaseVersion can set "null". But
     * in "Complete" file, in general "null" values SHOULD not be given.
     *
     * @see #checkData(uk.ac.ebi.pride.jmztab.model.MZTabColumn, string, boolean)
     *
     * @param column SHOULD NOT set null
     * @param databaseVersion SHOULD NOT be empty.
     */

        protected string checkDatabaseVersion(MZTabColumn column, string databaseVersion){
            return checkData(column, databaseVersion, true);
        }

        /**
     * Check and translate searchEngine string into parameter list which split by '|' character..
     * If parse incorrect, raise {@link FormatErrorType#ParamList} error.
     * Normally, searchEngine can set "null", but in "Complete" file, in general "null" values SHOULD not be given.
     *
     * @param column SHOULD NOT set null
     * @param searchEngine SHOULD NOT be empty.
     */

        protected SplitList<Param> checkSearchEngine(MZTabColumn column, string searchEngine){
            return checkParamList(column, searchEngine);
        }

        /**
     * Check and translate bestSearchEngineScore string into parameter list which split by '|' character..
     * If parse incorrect, raise {@link FormatErrorType#ParamList} error. If parameter is not {@link CVParam},
     * or parameter value is empty (score should be provide), system raise {@link FormatErrorType#SearchEngineScore}.
     * Normally, bestSearchEngineScore can set "null", but in "Complete" file, in general "null" values SHOULD not be given.
     *
     * @param column SHOULD NOT set null
     * @param bestSearchEngineScore SHOULD NOT be empty.
     */

        protected SplitList<Param> checkBestSearchEngineScore(MZTabColumn column, string bestSearchEngineScore){
            SplitList<Param> paramList = checkParamList(column, bestSearchEngineScore);

            foreach (Param param in paramList){
                if (! (param is CVParam) || string.IsNullOrEmpty(param.Value)){
                    _errorList.Add(new MZTabError(FormatErrorType.SearchEngineScore, _lineNumber, column.Header,
                                                  bestSearchEngineScore));
                }
            }

            return paramList;
        }

        /**
     * Check and translate searchEngineScore string into parameter list which split by '|' character..
     * If parse incorrect, raise {@link FormatErrorType#ParamList} error. If parameter is not {@link CVParam},
     * or parameter value is empty (score should be provide), system raise {@link FormatErrorType#SearchEngineScore}.
     * Normally, searchEngineScore can set "null", but in "Complete" file, in general "null" values SHOULD not be given.
     *
     * @param column SHOULD NOT set null
     * @param searchEngineScore SHOULD NOT be empty.
     */

        protected SplitList<Param> checkSearchEngineScore(MZTabColumn column, string searchEngineScore){
            SplitList<Param> paramList = checkParamList(column, searchEngineScore);

            foreach (Param param in paramList){
                if (!(param is CVParam) || string.IsNullOrEmpty(param.Value)){
                    _errorList.Add(new MZTabError(FormatErrorType.SearchEngineScore, _lineNumber, column.Header,
                                                  searchEngineScore));
                }
            }

            return paramList;
        }

        /**
     * Check and translate reliability string into {@link Reliability}. Currently, only "1", "2", "3" and "null" are
     * correct value, and others will raise {@link FormatErrorType#Reliability} error.
     * But in "Complete" file, in general "null" values SHOULD not be given.
     *
     * @param column SHOULD NOT set null
     * @param reliability SHOULD NOT be empty.
     */

        protected Reliability checkReliability(MZTabColumn column, string reliability){
            string result_reliaility = checkData(column, reliability, true);

            if (result_reliaility == null || result_reliaility.Equals("NULL", StringComparison.CurrentCultureIgnoreCase)){
                return null;
            }

            Reliability result = Reliability.findReliability(result_reliaility);
            if (result == null){
                _errorList.Add(new MZTabError(FormatErrorType.Reliability, _lineNumber, column.Header, result_reliaility));
            }

            return result;
        }

        /**
     * Check and translate numPSMs string into int. If exists error during parse, raise {@link FormatErrorType#int} error.
     * Normally, numPSMs can set "null", but in "Complete" file, in general "null" values SHOULD not be given.
     *
     * @param column SHOULD NOT set null
     * @param numPSMs SHOULD NOT be empty.
     */

        protected Integer checkNumPSMs(MZTabColumn column, string numPSMs) {
            return checkInteger(column, numPSMs);
        }

        /**
     * Check and translate numPeptidesDistinct string into int. If exists error during parse, raise {@link FormatErrorType#int} error.
     * Normally, numPeptidesDistinct can set "null", but in "Complete" file, in general "null" values SHOULD not be given.
     *
     * @param column SHOULD NOT set null
     * @param numPeptidesDistinct SHOULD NOT be empty.
     */

        protected Integer checkNumPeptidesDistinct(MZTabColumn column, string numPeptidesDistinct) {
            return checkInteger(column, numPeptidesDistinct);
        }

        /**
     * Check and translate numPeptidesUnique string into int. If exists error during parse, raise {@link FormatErrorType#int} error.
     * Normally, numPeptidesUnique can set "null", but in "Complete" file, in general "null" values SHOULD not be given.
     *
     * @param column SHOULD NOT set null
     * @param numPeptidesUnique SHOULD NOT be empty.
     */

        protected Integer checkNumPeptidesUnique(MZTabColumn column, string numPeptidesUnique) {
            return checkInteger(column, numPeptidesUnique);
        }

        /**
     * Check and translate target string into parameter list which split by ',' character..
     * If parse incorrect, raise {@link FormatErrorType#StringList} error.
     * Normally, ambiguityMembers can set "null", but in "Complete" file, in general "null" values SHOULD not be given.
     *
     * @param column SHOULD NOT set null
     * @param ambiguityMembers SHOULD NOT be empty.
     */

        protected SplitList<string> checkAmbiguityMembers(MZTabColumn column, string ambiguityMembers){
            return checkStringList(column, ambiguityMembers, MZTabConstants.COMMA);
        }

        /**
     * Check and translate target string into {@link Modification} list which split by ',' character..
     * If parse incorrect, raise {@link FormatErrorType#ModificationList} error.
     * Normally, ambiguityMembers can set "null", but in "Complete" file, in general "null" values SHOULD not be given.
     *
     * If software cannot determine protein-level modifications, "null" MUST be used.
     * If the software has determined that there are no modifications to a given protein "0" MUST be used.
     *
     * @param section SHOULD NOT set null
     * @param column SHOULD NOT set null
     * @param modificationsLabel SHOULD NOT be empty.
     */

        protected SplitList<Modification> checkModifications(Section section, MZTabColumn column,
                                                             string modificationsLabel){
            string result_modifications = checkData(column, modificationsLabel, true);

            if (result_modifications == null ||
                result_modifications.Equals("NULL", StringComparison.CurrentCultureIgnoreCase) ||
                result_modifications.Equals("0")){
                return new SplitList<Modification>(MZTabConstants.COMMA);
            }

            SplitList<Modification> modificationList = MZTabUtils.ParseModificationList(section, modificationsLabel);
            if (modificationList.Count == 0){
                _errorList.Add(new MZTabError(FormatErrorType.ModificationList, _lineNumber, column.Header,
                                              result_modifications));
            }

            return modificationList;
        }

        protected Uri checkURI(MZTabColumn column, string uri){
            string result_uri = checkData(column, uri, true);

            if (result_uri == null || result_uri.Equals("NULL", StringComparison.CurrentCultureIgnoreCase)){
                return null;
            }

            Uri result = MZTabUtils.ParseURI(result_uri);
            if (result == null){
                _errorList.Add(new MZTabError(FormatErrorType.URI, _lineNumber, "Column " + column.Header, result_uri));
            }

            return result;
        }

        /**
     * Check and translate spectraRef string into {@link SpectraRef} list.
     * If parse incorrect, or ms_run not defined in metadata raise {@link FormatErrorType#SpectraRef} error.
     * Normally, spectraRef can set "null", but in "Complete" file, in general "null" values SHOULD not be given.
     *
     * @param column SHOULD NOT set null
     * @param spectraRef SHOULD NOT be empty.
     */

        protected List<SpectraRef> checkSpectraRef(MZTabColumn column, string spectraRef){
            string result_spectraRef = checkData(column, spectraRef, true);

            if (result_spectraRef == null || result_spectraRef.Equals("NULL", StringComparison.CurrentCultureIgnoreCase)){
                return new List<SpectraRef>();
            }

            List<SpectraRef> refList = MZTabUtils.ParseSpectraRefList(metadata, result_spectraRef);
            if (refList.Count == 0){
                _errorList.Add(new MZTabError(FormatErrorType.SpectraRef, _lineNumber, column.Header, result_spectraRef));
            }
            else{
                foreach (SpectraRef reference in refList){
                    MsRun run = reference.MsRun;
                    if (run.Location == null){
                        _errorList.Add(new MZTabError(LogicalErrorType.SpectraRef, _lineNumber, column.Header,
                                                      result_spectraRef, "ms_run[" + run.Id + "]-location"));
                        refList.Clear();
                        break;
                    }
                }
            }

            return refList;
        }

        /**
     * Check target string. Normally, pre can set "null". But
     * in "Complete" file, in general "null" values SHOULD not be given.
     *
     * @see #checkData(uk.ac.ebi.pride.jmztab.model.MZTabColumn, string, boolean)
     *
     * @param column SHOULD NOT set null
     * @param pre SHOULD NOT be empty.
     */

        protected string checkPre(MZTabColumn column, string pre){
            return checkData(column, pre, true);
        }

        /**
     * Check target string. Normally, post can set "null". But
     * in "Complete" file, in general "null" values SHOULD not be given.
     *
     * @see #checkData(uk.ac.ebi.pride.jmztab.model.MZTabColumn, string, boolean)
     *
     * @param column SHOULD NOT set null
     * @param post SHOULD NOT be empty.
     */

        protected string checkPost(MZTabColumn column, string post){
            return checkData(column, post, true);
        }

        /**
     * Check target string. Normally, start can set "null". But
     * in "Complete" file, in general "null" values SHOULD not be given.
     *
     * @see #checkData(uk.ac.ebi.pride.jmztab.model.MZTabColumn, string, boolean)
     *
     * @param column SHOULD NOT set null
     * @param start SHOULD NOT be empty.
     */

        protected string checkStart(MZTabColumn column, string start){
            return checkData(column, start, true);
        }

        /**
     * Check target string. Normally, end can set "null". But
     * in "Complete" file, in general "null" values SHOULD not be given.
     *
     * @see #checkData(uk.ac.ebi.pride.jmztab.model.MZTabColumn, string, boolean)
     *
     * @param column SHOULD NOT set null
     * @param end SHOULD NOT be empty.
     */

        protected string checkEnd(MZTabColumn column, string end){
            return checkData(column, end, true);
        }

        /**
     * Check and translate target string into string list which split by ',' character..
     * If parse incorrect, raise {@link FormatErrorType#StringList} error. Besides, each item in list should be
     * start with "GO:", otherwise system raise {@link FormatErrorType#GOTermList} error.
     * Normally, go_terms can set "null", but in "Complete" file, in general "null" values SHOULD not be given.
     *
     * @param column SHOULD NOT set null
     * @param go_terms SHOULD NOT be empty.
     */

        protected SplitList<string> checkGOTerms(MZTabColumn column, string go_terms){
            string result_go_terms = checkData(column, go_terms, true);

            if (result_go_terms == null || result_go_terms.Equals("NULL", StringComparison.CurrentCultureIgnoreCase)){
                return new SplitList<string>(MZTabConstants.COMMA);
            }


            SplitList<string> stringList = MZTabUtils.ParseGoTermList(result_go_terms);
            if (stringList.Count == 0){
                _errorList.Add(new MZTabError(FormatErrorType.GOTermList, _lineNumber, column.Header, result_go_terms));
            }

            return stringList;
        }

        /**
     * Check and translate protein_coverage string into double. If parse incorrect, raise {@link FormatErrorType#double} error.
     * protein_coverage range should be in the [0, 1), otherwise raise {@link LogicalErrorType#ProteinCoverage} error.
     *
     * NOTICE: If ratios are included and the denominator is zero, the "INF" value MUST be used. If the result leads
     * to calculation errors (for example 0/0), this MUST be reported as "not a number" ("NaN").
     *
     * @param column SHOULD NOT set null
     * @param protein_coverage SHOULD NOT be empty.
     */

        protected double checkProteinCoverage(MZTabColumn column, string protein_coverage){
            double result = checkDouble(column, protein_coverage);

            if (result.Equals(double.MinValue)){
                return double.MinValue;
            }

            if (result < 0 || result > 1){
                _errorList.Add(new MZTabError(LogicalErrorType.ProteinCoverage, _lineNumber, column.Header,
                                              MZTabUtils.PrintDouble(result)));
                return double.MinValue;
            }

            return result;
        }

        /**
     * Check and translate peptide sequence. 'O' and 'U' are encoded by codons that are usually interpreted as stop codons,
     * which can not displayed in the sequence. So, if find it, system raise {@link FormatErrorType#Sequence} error.
     *
     * @param column SHOULD NOT set null
     * @param sequence SHOULD NOT be empty.
     */

        protected string checkSequence(MZTabColumn column, string sequence){
            string result = checkData(column, sequence, true);

            if (result == null){
                return null;
            }

            result = result.ToUpper();

            Regex regex = new Regex("[OU]");
            Match match = regex.Match(result);
            if (match.Success){
                _errorList.Add(new MZTabError(FormatErrorType.Sequence, _lineNumber, column.Header, sequence));
            }

            return result;
        }

        /**
     * Check and translate psm_id string into int. If exists error during parse, raise {@link FormatErrorType#int} error.
     * Normally, psm_id can set "null", but in "Complete" file, in general "null" values SHOULD not be given.
     *
     * @param column SHOULD NOT set null
     * @param psm_id SHOULD NOT be empty.
     */

        protected Integer checkPSMID(MZTabColumn column, string psm_id) {
            return checkInteger(column, psm_id);
        }

        /**
     * Check and translate unique to {@link MZBoolean}. Only "0" and "1" allow used in express Boolean (0/1).
     * If parse incorrect, raise {@link FormatErrorType#MZBoolean} error.
     *
     * @param column SHOULD NOT set null
     * @param unique SHOULD NOT be empty.
     */

        protected MZBoolean checkUnique(MZTabColumn column, string unique){
            return checkMZBoolean(column, unique);
        }

        /**
     * Check and translate charge string into int. If exists error during parse, raise {@link FormatErrorType#int} error.
     * Normally, charge can set "null", but in "Complete" file, in general "null" values SHOULD not be given.
     *
     * @param column SHOULD NOT set null
     * @param charge SHOULD NOT be empty.
     */

        protected Integer checkCharge(MZTabColumn column, string charge) {
            return checkInteger(column, charge);
        }

        /**
     * Check and translate mass_to_charge string into double. If parse incorrect, raise {@link FormatErrorType#double} error.
     *
     * NOTICE: If ratios are included and the denominator is zero, the "INF" value MUST be used. If the result leads
     * to calculation errors (for example 0/0), this MUST be reported as "not a number" ("NaN").
     *
     * @param column SHOULD NOT set null
     * @param mass_to_charge SHOULD NOT be empty.
     */

        protected double checkMassToCharge(MZTabColumn column, string mass_to_charge){
            return checkDouble(column, mass_to_charge);
        }

        /**
     * Check and translate exp_mass_to_charge string into double. If parse incorrect, raise {@link FormatErrorType#double} error.
     *
     * NOTICE: If ratios are included and the denominator is zero, the "INF" value MUST be used. If the result leads
     * to calculation errors (for example 0/0), this MUST be reported as "not a number" ("NaN").
     *
     * @param column SHOULD NOT set null
     * @param exp_mass_to_charge SHOULD NOT be empty.
     */

        protected double checkExpMassToCharge(MZTabColumn column, string exp_mass_to_charge){
            return checkDouble(column, exp_mass_to_charge);
        }

        /**
     * Check and translate calc_mass_to_charge string into double. If parse incorrect, raise {@link FormatErrorType#double} error.
     *
     * NOTICE: If ratios are included and the denominator is zero, the "INF" value MUST be used. If the result leads
     * to calculation errors (for example 0/0), this MUST be reported as "not a number" ("NaN").
     *
     * @param column SHOULD NOT set null
     * @param calc_mass_to_charge SHOULD NOT be empty.
     */

        protected double checkCalcMassToCharge(MZTabColumn column, string calc_mass_to_charge){
            return checkDouble(column, calc_mass_to_charge);
        }

        /**
     * Check and translate identifier string into string list which split by '|' character..
     * If parse incorrect, raise {@link FormatErrorType#StringList} error.
     * Normally, identifier can set "null", but in "Complete" file, in general "null" values SHOULD not be given.
     *
     * @param column SHOULD NOT set null
     * @param identifier SHOULD NOT be empty.
     */

        protected SplitList<string> checkIdentifier(MZTabColumn column, string identifier){
            return checkStringList(column, identifier, MZTabConstants.BAR);
        }

        /**
     * Check chemical_formula string. Normally, chemical_formula can set "null". But
     * in "Complete" file, in general "null" values SHOULD not be given.
     *
     * @see #checkData(uk.ac.ebi.pride.jmztab.model.MZTabColumn, string, boolean)
     *
     * @param column SHOULD NOT set null
     * @param chemical_formula SHOULD NOT be empty.
     */

        protected string checkChemicalFormula(MZTabColumn column, string chemical_formula){
            return checkData(column, chemical_formula, true);
        }

        /**
     * Check and translate smiles string into parameter list which split by '|' character..
     * If parse incorrect, raise {@link FormatErrorType#StringList} error.
     * Normally, smiles can set "null", but in "Complete" file, in general "null" values SHOULD not be given.
     *
     * @param column SHOULD NOT set null
     * @param smiles SHOULD NOT be empty.
     */

        protected SplitList<string> checkSmiles(MZTabColumn column, string smiles){
            return checkStringList(column, smiles, MZTabConstants.BAR);
        }

        /**
     * Check and translate inchi_key string into parameter list which split by '|' character..
     * If parse incorrect, raise {@link FormatErrorType#StringList} error.
     * Normally, inchi_key can set "null", but in "Complete" file, in general "null" values SHOULD not be given.
     *
     * @param column SHOULD NOT set null
     * @param inchi_key SHOULD NOT be empty.
     */

        protected SplitList<string> checkInchiKey(MZTabColumn column, string inchi_key){
            return checkStringList(column, inchi_key, MZTabConstants.BAR);
        }

        /**
     * Check and translate retention_time string into double list which split by '|' character..
     * If parse incorrect, raise {@link FormatErrorType#DoubleList} error.
     * Normally, retention_time can set "null", but in "Complete" file, in general "null" values SHOULD not be given.
     *
     * @param column SHOULD NOT set null
     * @param retention_time SHOULD NOT be empty.
     */

        protected SplitList<double> checkRetentionTime(MZTabColumn column, string retention_time){
            string result = checkData(column, retention_time, true);

            if (result == null || result.Equals("NULL", StringComparison.CurrentCultureIgnoreCase)){
                return new SplitList<double>(MZTabConstants.BAR);
            }

            SplitList<double> valueList = MZTabUtils.ParseDoubleList(result);
            if (valueList.Count == 0){
                _errorList.Add(new MZTabError(FormatErrorType.DoubleList, _lineNumber, column.Header, result,
                                              "" + MZTabConstants.BAR));
            }

            return valueList;
        }

        /**
     * Check and translate retention_time_window string into double list which split by '|' character..
     * If parse incorrect, raise {@link FormatErrorType#DoubleList} error.
     * Normally, retention_time_window can set "null", but in "Complete" file, in general "null" values SHOULD not be given.
     *
     * @param column SHOULD NOT set null
     * @param retention_time_window SHOULD NOT be empty.
     */

        protected SplitList<double> checkRetentionTimeWindow(MZTabColumn column, string retention_time_window){
            string result = checkData(column, retention_time_window, true);

            if (result == null || result.Equals("NULL", StringComparison.CurrentCultureIgnoreCase)){
                return new SplitList<double>(MZTabConstants.BAR);
            }

            SplitList<double> valueList = MZTabUtils.ParseDoubleList(result);
            if (valueList.Count == 0){
                _errorList.Add(new MZTabError(FormatErrorType.DoubleList, _lineNumber, column.Header, result,
                                              "" + MZTabConstants.BAR));
            }

            return valueList;
        }
    }
}