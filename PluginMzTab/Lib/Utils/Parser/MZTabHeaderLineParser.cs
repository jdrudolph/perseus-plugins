using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PluginMzTab.Lib.Model;
using PluginMzTab.Lib.Utils.Errors;

namespace PluginMzTab.Lib.Utils.Parser{
    /**
* A couple of common method used to parse a header line into {@link MZTabColumnFactory} structure.
 *
 * NOTICE: {@link MZTabColumnFactory} maintain a couple of {@link MZTabColumn} which have internal logical
 * position and order. In physical mzTab file, we allow user not obey this logical position organized way,
 * and provide their date with own order. In order to distinguish them, we use physical position (a positive
 * integer) to record the column location in mzTab file. And use {@link PositionMapping} structure the maintain
 * the mapping between them.
 *
 * @see PRHLineParser
 * @see PEHLineParser
 * @see PSHLineParser
 * @see SMHLineParser
*/

    public abstract class MZTabHeaderLineParser : MZTabLineParser{
        protected MZTabColumnFactory factory;
        protected Metadata metadata;

        /**
     * Parse a header line into {@link MZTabColumnFactory} structure.
     *
     * @param factory SHOULD NOT set null
     * @param metadata SHOULD NOT set null
     */

        protected MZTabHeaderLineParser(MZTabColumnFactory factory, Metadata metadata){
            if (factory == null){
                throw new NullReferenceException("Header line should be parse first!");
            }
            this.factory = factory;

            if (metadata == null){
                throw new NullReferenceException("Metadata should be create first!");
            }
            this.metadata = metadata;
        }

        /**
     * Some validate operation need to be done after the whole {@link MZTabColumnFactory} created.
     * Thus, user can add them, and called at the end of the
     * {@link #parse(int, string, uk.ac.ebi.pride.jmztab.utils.errors.MZTabErrorList)} method.
     */
        protected abstract void refine();

        /**
     * Refine optional columns based one {@link MZTabDescription.Mode} and {@link MZTabDescription.Type}
     * These re-validate operation will called in {@link #refine()} method.
     */

        protected void refineOptionalColumn(MzTabMode mode, MzTabType type, string columnHeader){
            if (factory.FindColumnByHeader(columnHeader) == null){
                throw new MZTabException(new MZTabError(LogicalErrorType.NotDefineInHeader, _lineNumber, columnHeader,
                                                        mode.ToString(), type.ToString()));
            }
        }

        /**
     * Parse a header line into {@link MZTabColumnFactory} structure. There are two steps in this method:
     * Step 1: {@link #parseStableOrderColumns()} focus on validate and parse for stable columns and optional
     * columns which have stable order; and Step 2: {@link #parseOptionalColumns(int)} focus on {@link AbundanceColumn},
     * {@link OptionColumn} and {@link CVParamOptionColumn} parse and validation.
     */

        protected new void Parse(int lineNumber, string line, MZTabErrorList errorList){
            base.Parse(lineNumber, line, errorList);

            int offset = parseStableOrderColumns();
            if (offset < _items.Length){
                parseOptionalColumns(offset);
            }

            refine();
        }

        /**
     * Focus on validate and parse for stable columns and optional columns which have stable order;
     * All of them defined in the {@link ProteinColumn}, {@link PeptideColumn}, {@link PSMColumn}
     * or {@link SmallMoleculeColumn}.
     *
     * NOTICE: there not exist optional columns with stable order in {@link PSMColumn}
     */

        private int parseStableOrderColumns(){
            List<string> headerList = _items.ToList();

            // step 1: confirm stable columns have been included in the header line.
            foreach (MZTabColumn column in factory.StableColumnMapping.Values){
                if (! headerList.Contains(column.Header)){
                    MZTabError error = new MZTabError(FormatErrorType.StableColumn, _lineNumber, column.Header);
                    throw new MZTabException(error);
                }
            }

            // step 2: checking some optional columns which have stable order.
            foreach (string header in headerList){
                if (header.Equals(ProteinColumn.GO_TERMS.Header)){
                    factory.addGoTermsOptionalColumn();
                }
                else if (header.Equals(ProteinColumn.RELIABILITY.Header)){
                    factory.addReliabilityOptionalColumn();
                }
                else if (header.Equals(ProteinColumn.URI.Header)){
                    factory.addURIOptionalColumn();
                }
            }

            // step 3: checking some flexible optional columns which have stable order.
            if (_items != null){
                MZTabColumn column = null;
                Regex regex = new Regex("(\\w+)_ms_run\\[(\\d+)\\]");


                for (int i = 1; i < _items.Length; i++){
                    string header = _items[i];

                    Match match = regex.Match(header);
                    if (regex.IsMatch(header)){
                        int id = checkIndex(header, match.Groups[2].Value);

                        MsRun msRun = null;
                        if (metadata.MsRunMap.ContainsKey(id)){
                            msRun = metadata.MsRunMap[id];
                        }
                        if (msRun == null){
                            throw new MZTabException(new MZTabError(LogicalErrorType.MsRunNotDefined, _lineNumber,
                                                                    header));
                        }

                        if (_section.Equals(Section.Protein_Header)){
                            if (header.StartsWith(ProteinColumn.SEARCH_ENGINE_SCORE.Name)){
                                column = ProteinColumn.SEARCH_ENGINE_SCORE;
                            }
                            else if (header.StartsWith(ProteinColumn.NUM_PSMS.Name)){
                                column = ProteinColumn.NUM_PSMS;
                            }
                            else if (header.StartsWith(ProteinColumn.NUM_PEPTIDES_DISTINCT.Name)){
                                column = ProteinColumn.NUM_PEPTIDES_DISTINCT;
                            }
                            else if (header.StartsWith(ProteinColumn.NUM_PEPTIDES_UNIQUE.Name)){
                                column = ProteinColumn.NUM_PEPTIDES_UNIQUE;
                            }
                            else if (header.StartsWith("opt_")){
                                // ignore opt_ms_run....
                                // This kind of optional columns will be processed in the parseOptionalColumns() method.
                            }
                            else{
                                throw new MZTabException(new MZTabError(FormatErrorType.MsRunOptionalColumn,
                                                                        _lineNumber,
                                                                        header, _section.Name));
                            }
                        }
                        else if (_section.Equals(Section.Peptide_Header)){
                            if (header.StartsWith(PeptideColumn.SEARCH_ENGINE_SCORE.Name)){
                                column = PeptideColumn.SEARCH_ENGINE_SCORE;
                            }
                            else if (header.StartsWith("opt_")){
                                // ignore opt_ms_run....
                                // This kind of optional columns will be processed in the parseOptionalColumns() method.
                            }
                            else{
                                throw new MZTabException(new MZTabError(FormatErrorType.MsRunOptionalColumn,
                                                                        _lineNumber,
                                                                        header, _section.Name));
                            }
                        }
                        else if (_section.Equals(Section.Small_Molecule_Header)){
                            if (header.StartsWith(SmallMoleculeColumn.SEARCH_ENGINE_SCORE.Name)){
                                column = SmallMoleculeColumn.SEARCH_ENGINE_SCORE;
                            }
                            else if (header.StartsWith("opt_")){
                                // ignore opt_ms_run....
                                // This kind of optional columns will be processed in the parseOptionalColumns() method.
                            }
                            else{
                                throw new MZTabException(new MZTabError(FormatErrorType.MsRunOptionalColumn,
                                                                        _lineNumber,
                                                                        header, _section.Name));
                            }
                        }

                        if (column != null){
                            factory.AddOptionalColumn(column, msRun);
                        }
                    }
                }
            }

            return factory.StableColumnMapping.Values.Count;
        }

        /**
     * Iterative parse optional columns with dynamic logical position and order. These optional column including:
     * {@link AbundanceColumn}, {@link OptionColumn} and {@link CVParamOptionColumn} parse and validation.
     * All of these optional columns always put the end of the table, thus, we use offset to locate the last
     * column position.
     *
     * @param offset the cursor to locate the optional columns locate the end of the table.
     */

        private void parseOptionalColumns(int offset){
            string columnName = _items[offset].Trim();

            if (columnName.StartsWith("opt_")){
                checkOptColumnName(columnName);
            }
            else if (columnName.Contains("abundance")){
                offset = checkAbundanceColumns(offset);
            }

            if (offset < _items.Length - 1){
                offset++;
                parseOptionalColumns(offset);
            }
        }

        /**
     * Some {@link CVParamOptionColumn}, their data type have defined. Currently, we provide two {@link CVParam}
     * which defined in the mzTab specification. One is "emPAI value" (MS:1001905), data type is Double;
     * another is "decoy peptide" (MS:1002217), the data type is Boolean (0/1). Besides them, "opt_" start optional
     * column data type is string.
     *
     * @see #checkOptColumnName(string)
     */

        private Type getDataType(CVParam param){
            Type dataType;

            if (param == null){
                dataType = typeof (string);
            }
            else if (param.Accession.Equals("MS:1001905")){
                dataType = typeof (double);
            }
            else if (param.Accession.Equals("MS:1002217")){
                dataType = typeof (MZBoolean);
            }
            else{
                dataType = typeof (string);
            }

            return dataType;
        }

        /**
     * Additional columns can be added to the end of the protein table. These column headers MUST start with the prefix "opt_".
     * Column names MUST only contain the following characters: 'A’-'Z’, 'a’-'z’, '0’-'9’, '_’, '-’, '[’, ']’, and ':’.
     *
     * the format: opt_{IndexedElement[id]}_{value}. Spaces within the parameter's name MUST be replaced by '_'.
     */

        private bool checkOptColumnName(string nameLabel){
            nameLabel = nameLabel.Trim();

            const string regexp = "opt_((assay|study_variable|ms_run)\\[(\\w+)\\]|global)_([A-Za-z0-9_\\-\\[\\]:\\.]+)";
            Regex regex = new Regex(regexp);
            Match match = regex.Match(nameLabel);

            if (match.Success){
                string object_id = match.Groups[1].Value;
                string value = match.Groups[4].Value;

                CVParam param = null;
                if (value.StartsWith("cv_")){
                    param = checkCVParamOptColumnName(nameLabel, value);
                }

                Type dataType = getDataType(param);

                if (object_id.Contains("global")){
                    if (param == null){
                        factory.AddOptionalColumn(value, dataType);
                    }
                    else{
                        factory.AddOptionalColumn(param, dataType);
                    }
                }
                else{
                    int id = checkIndex(nameLabel, match.Groups[3].Value);

                    MZTabError error;
                    if (object_id.Contains("assay")){
                        Assay element = null;
                        if (metadata.AssayMap.ContainsKey(id)){
                            element = metadata.AssayMap[id];
                        }
                        // not found assay_id in metadata.
                        if (element == null){
                            error = new MZTabError(LogicalErrorType.AssayNotDefined, _lineNumber, nameLabel);
                            throw new MZTabException(error);
                        }
                        if (param == null){
                            factory.AddOptionalColumn(element, value, dataType);
                        }
                        else{
                            factory.AddOptionalColumn(element, param, dataType);
                        }
                    }
                    else if (object_id.Contains("study_variable")){
                        StudyVariable element = null;
                        if (metadata.StudyVariableMap.ContainsKey(id)){
                            element = metadata.StudyVariableMap[id];
                        }
                        // not found study_variable_id in metadata.
                        if (element == null){
                            error = new MZTabError(LogicalErrorType.StudyVariableNotDefined, _lineNumber, nameLabel);
                            throw new MZTabException(error);
                        }
                        if (param == null){
                            factory.AddOptionalColumn(element, value, dataType);
                        }
                        else{
                            factory.AddOptionalColumn(element, param, dataType);
                        }
                    }
                    else if (object_id.Contains("ms_run")){
                        MsRun element = null;
                        if (metadata.MsRunMap.ContainsKey(id)){
                            element = metadata.MsRunMap[id];
                        }
                        // not found ms_run_id in metadata.
                        if (element == null){
                            error = new MZTabError(LogicalErrorType.MsRunNotDefined, _lineNumber, nameLabel);
                            throw new MZTabException(error);
                        }
                        if (param == null){
                            factory.AddOptionalColumn(element, value, dataType);
                        }
                        else{
                            factory.AddOptionalColumn(element, param, dataType);
                        }
                    }
                }

                return true;
            }
            throw new MZTabException(new MZTabError(FormatErrorType.OptionalCVParamColumn, _lineNumber, nameLabel));
        }

        /**
     * An kind of {@link CVParamOptionColumn} which use CV parameter accessions in following the format:
     * opt_{OBJECT_ID}_cv_{accession}_{parameter name}. Spaces within the parameter' s name MUST be replaced by '_'.
     */

        private CVParam checkCVParamOptColumnName(string nameLabel, string valueLabel){
            nameLabel = nameLabel.Trim();
            valueLabel = valueLabel.Trim();

            const string regexp = "cv(_([A-Za-z0-9\\-\\[\\]:\\.]+))?(_([A-Za-z0-9_\\-\\[\\]:\\.]+)*)";
            Regex regex = new Regex(regexp);
            Match match = regex.Match(valueLabel);

            if (! match.Success || match.Length != valueLabel.Length){
                throw new MZTabException(new MZTabError(FormatErrorType.OptionalCVParamColumn, _lineNumber, nameLabel));
            }
            string accession = match.Groups[2].Value;
            string name = match.Groups[4].Value;
            if (name == null || name.Trim().Length == 0){
                throw new MZTabException(new MZTabError(FormatErrorType.OptionalCVParamColumn, _lineNumber,
                                                        nameLabel));
            }

            CVParam param = match.Groups[4].Value == null ? null : new CVParam(null, accession, name, null);

            return param;
        }

        /**
     * Abundance_assay[id], abundance_study_variable[id] abundance_stdev_sub_study_variable[id],
     * abundance_std_error_study_variable[id]. The last three columns should be display together.
     * Thus, this method will parse three abundance columns as a group, and return the offset of next
     */

        private int checkAbundanceColumns(int offset){
            if (_items[offset].Contains("abundance_assay")){
                checkAbundanceAssayColumn(_items[offset]);

                return offset;
            }
            string abundanceHeader = "";
            string abundanceStdevHeader = "";
            string abundanceStdErrorHeader = "";

            try{
                abundanceHeader = _items[offset++];
                abundanceStdevHeader = _items[offset++];
                abundanceStdErrorHeader = _items[offset];
            }
            catch (ArgumentOutOfRangeException){
                // do nothing.
            }

            checkAbundanceStudyVariableColumns(abundanceHeader, abundanceStdevHeader, abundanceStdErrorHeader);

            return offset;
        }

        /**
     * This is a temporary method which face smallmolecule in SmallMolecule header line.
     * translate smallmolecule --> small_molecule.
     * @see AbundanceColumn#translate(string)
     */

        private string translate(string oldName){
            if (oldName.Equals("smallmolecule")){
                return "small_molecule";
            }
            return oldName;
        }

        /**
     * Parse header to a index id number.
     * If exists parse error, stop validate and throw {@link MZTabException} directly.
     */

        private int checkIndex(string header, string id){
            try{
                int index = int.Parse(id);
                if (index < 1){
                    throw new FormatException();
                }

                return index;
            }
            catch (FormatException){
                MZTabError error = new MZTabError(LogicalErrorType.IdNumber, _lineNumber, header, id);
                throw new MZTabException(error);
            }
        }

        /**
     * Check (protein|peptide|smallmolecule)_abundance is correct, and return object value label.
     * For example, protein_abundance_std_error_study_variable[id], return study_variable[id].
     */

        private string checkAbundanceSection(string abundanceHeader){
            abundanceHeader = abundanceHeader.Trim().ToLower();

            Regex regex = new Regex("(protein|peptide|smallmolecule)_abundance_(.+)");
            Match match = regex.Match(abundanceHeader);

            if (match.Success){
                string sectionName = translate(match.Groups[1].Value);
                if (sectionName != null &&
                    !(sectionName.Equals(Section.Protein.Name) && _section != Section.Protein_Header) &&
                    !(sectionName.Equals(Section.Peptide.Name) && _section != Section.Peptide_Header) &&
                    !(sectionName.Equals(Section.Small_Molecule.Name) && _section != Section.Small_Molecule_Header)){
                    return match.Groups[2].Value;
                }

                MZTabError error = new MZTabError(FormatErrorType.AbundanceColumn, _lineNumber, abundanceHeader);
                throw new MZTabException(error);
            }
            else{
                MZTabError error = new MZTabError(FormatErrorType.AbundanceColumn, _lineNumber, abundanceHeader);
                throw new MZTabException(error);
            }
        }

        /**
     * Check XXXX_abundance_assay[id] column header. If parse error, stop validate and raise
     * {@link MZTabException}.
     */

        private void checkAbundanceAssayColumn(string abundanceHeader){
            string valueLabel = checkAbundanceSection(abundanceHeader);
            Regex regex = new Regex("assay\\[(\\d+)\\]");
            Match match = regex.Match(valueLabel);
            if (! match.Success){
                MZTabError error = new MZTabError(FormatErrorType.AbundanceColumn, _lineNumber, abundanceHeader);
                throw new MZTabException(error);
            }

            int id = checkIndex(abundanceHeader, match.Groups[1].Value);
            Assay assay = null;
            if (metadata.AssayMap.ContainsKey(id)){
                assay = metadata.AssayMap[id];
            }
            if (assay == null){
                MZTabError error = new MZTabError(LogicalErrorType.AssayNotDefined, _lineNumber, abundanceHeader);
                throw new MZTabException(error);
            }

            factory.AddAbundanceOptionalColumn(assay);
        }

        /**
     * Check XXXX_abundance_study_variable[id], XXXX_abundance_stdev_study_variable[id], XXXX_abundance_std_error_study_variable[id]
     * column header. If parse error, stop validate and raise {@link MZTabException}.
     */

        private StudyVariable checkAbundanceStudyVariableColumn(string abundanceHeader){
            string valueLabel = checkAbundanceSection(abundanceHeader);

            Regex regex = new Regex("study_variable\\[(\\d+)\\]");
            Match match = regex.Match(valueLabel);

            if (! match.Success){
                MZTabError error = new MZTabError(FormatErrorType.AbundanceColumn, _lineNumber, abundanceHeader);
                throw new MZTabException(error);
            }

            int id = checkIndex(abundanceHeader, match.Groups[1].Value);
            StudyVariable studyVariable = null;
            if (metadata.StudyVariableMap.ContainsKey(id)){
                studyVariable = metadata.StudyVariableMap[id];
            }

            if (studyVariable == null){
                MZTabError error = new MZTabError(LogicalErrorType.StudyVariableNotDefined, _lineNumber, abundanceHeader);
                throw new MZTabException(error);
            }

            return studyVariable;
        }

        /**
     * Check XXXX_abundance_study_variable[id], XXXX_abundance_stdev_study_variable[id], XXXX_abundance_std_error_study_variable[id]
     * column header. If parse error, stop validate and raise {@link MZTabException}.
     */

        private void checkAbundanceStudyVariableColumns(string abundanceHeader, string abundanceStdevHeader,
                                                        string abundanceStdErrorHeader){
            abundanceHeader = abundanceHeader.Trim().ToLower();
            abundanceStdevHeader = abundanceStdevHeader.Trim().ToLower();
            abundanceStdErrorHeader = abundanceStdErrorHeader.Trim().ToLower();

            if (! abundanceHeader.Contains("_abundance_study_variable")){
                string missHeader = Section.toDataSection(_section).Name + "_abundance_study_variable";

                MZTabError error = new MZTabError(LogicalErrorType.AbundanceColumnTogether, _lineNumber, missHeader);
                throw new MZTabException(error);
            }

            if (! abundanceStdevHeader.Contains("_abundance_stdev_study_variable")){
                string missHeader = Section.toDataSection(_section).Name + "_abundance_stdev_study_variable";

                MZTabError error = new MZTabError(LogicalErrorType.AbundanceColumnTogether, _lineNumber, missHeader);
                throw new MZTabException(error);
            }

            if (! abundanceStdErrorHeader.Contains("_abundance_std_error_study_variable")){
                string missHeader = Section.toDataSection(_section).Name + "_abundance_std_error_study_variable";

                MZTabError error = new MZTabError(LogicalErrorType.AbundanceColumnTogether, _lineNumber, missHeader);
                throw new MZTabException(error);
            }

            StudyVariable abundanceStudyVariable = checkAbundanceStudyVariableColumn(abundanceHeader);
            StudyVariable abundanceStdevStudyVariable = checkAbundanceStudyVariableColumn(abundanceStdevHeader);
            StudyVariable abundanceStdErrorStudyVariable = checkAbundanceStudyVariableColumn(abundanceStdErrorHeader);

            if (abundanceStudyVariable != abundanceStdevStudyVariable ||
                abundanceStudyVariable != abundanceStdErrorStudyVariable){
                MZTabError error = new MZTabError(LogicalErrorType.AbundanceColumnSameId, _lineNumber, abundanceHeader,
                                                  abundanceStdevHeader, abundanceStdErrorHeader);
                throw new MZTabException(error);
            }

            factory.AddAbundanceOptionalColumn(abundanceStudyVariable);
        }

        public MZTabColumnFactory getFactory(){
            return factory;
        }
    }
}