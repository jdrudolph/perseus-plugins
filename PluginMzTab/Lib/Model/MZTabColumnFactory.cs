using System;
using System.Collections.Generic;
using System.Linq;
using PluginMzTab.Plugin.Utils;

namespace PluginMzTab.Lib.Model{
/**
 * This is a static factory class which used to generate a couple of MZTabColumn, and organized them
 * <position, MZTabColumn> pair.
 *
 * Currently, mzTab table including two parts:
 * <ol>
 *     <li>
 *         stable columns: based on protein, peptide, and small molecular, there are a couple of mandatory column
 *         and stable ordered columns.
 *     </li>
 *     <li>
 *         optional columns: these columns MAY be present in the table. These columns will add to the last column of
 *         table, which position based on the stable columns scale.
 *         @see AbundanceColumn
 *         @see OptionColumn
 *         @see CVParamOptionColumn
 *     </li>
 * </ol>
 *
 * User: Qingwei
 * Date: 23/05/13
 */

    public class MZTabColumnFactory{
        /**
     * maintain the position and MZTabColumn ordered pairs. Notice: the position start with 1.
     */

        private readonly SortedDictionary<string, MZTabColumn> stableColumnMapping =
            new SortedDictionary<string, MZTabColumn>();

        private readonly SortedDictionary<string, MZTabColumn> optionalColumnMapping =
            new SortedDictionary<string, MZTabColumn>();

        private readonly SortedDictionary<String, MZTabColumn> abundanceColumnMapping =
            new SortedDictionary<String, MZTabColumn>();

        private readonly SortedDictionary<string, MZTabColumn> columnMapping =
            new SortedDictionary<string, MZTabColumn>();

        /**
     * There are three type of table: protein, peptide or small molecular.
     */
        private Section section;

        private MZTabColumnFactory(){}

        private static void AddStableColumn(MZTabColumnFactory factory, MZTabColumn column){
            factory.stableColumnMapping.Add(column.LogicPosition, column);
        }

        /**
         * Based on {@link #section} to generate stable columns, and store them into {@link #stableColumnMapping} and
         * {@link #columnMapping} which maintain all columns in the factory.
         *
         * @param section SHOULD be {@link Section#Protein_Header}, {@link Section#Peptide_Header} {@link Section#PSM_Header}
         *                or {@link Section#Small_Molecule_Header}.
         */

        public static MZTabColumnFactory GetInstance(Section section){
            section = Section.toHeaderSection(section);

            if (section == null){
                throw new ArgumentException(
                    "Section should use Protein_Header, Peptide_Header, PSM_Header or Small_Molecule_Header.");
            }

            MZTabColumnFactory factory = new MZTabColumnFactory();


            if (section.Equals(Section.Protein_Header)){
                AddStableColumn(factory, ProteinColumn.ACCESSION);
                AddStableColumn(factory, ProteinColumn.DESCRIPTION);
                AddStableColumn(factory, ProteinColumn.TAXID);
                AddStableColumn(factory, ProteinColumn.SPECIES);
                AddStableColumn(factory, ProteinColumn.DATABASE);
                AddStableColumn(factory, ProteinColumn.DATABASE_VERSION);
                AddStableColumn(factory, ProteinColumn.SEARCH_ENGINE);
                AddStableColumn(factory, ProteinColumn.AMBIGUITY_MEMBERS);
                AddStableColumn(factory, ProteinColumn.MODIFICATIONS);
                AddStableColumn(factory, ProteinColumn.PROTEIN_COVERAGE);
            }
            else if (section.Equals(Section.Peptide_Header)){
                AddStableColumn(factory, PeptideColumn.SEQUENCE);
                AddStableColumn(factory, PeptideColumn.ACCESSION);
                AddStableColumn(factory, PeptideColumn.UNIQUE);
                AddStableColumn(factory, PeptideColumn.DATABASE);
                AddStableColumn(factory, PeptideColumn.DATABASE_VERSION);
                AddStableColumn(factory, PeptideColumn.SEARCH_ENGINE);
                AddStableColumn(factory, PeptideColumn.MODIFICATIONS);
                AddStableColumn(factory, PeptideColumn.RETENTION_TIME);
                AddStableColumn(factory, PeptideColumn.RETENTION_TIME_WINDOW);
                AddStableColumn(factory, PeptideColumn.CHARGE);
                AddStableColumn(factory, PeptideColumn.MASS_TO_CHARGE);
                AddStableColumn(factory, PeptideColumn.SPECTRA_REF);
            }
            else if (section.Equals(Section.PSM_Header)){
                AddStableColumn(factory, PSMColumn.SEQUENCE);
                AddStableColumn(factory, PSMColumn.PSM_ID);
                AddStableColumn(factory, PSMColumn.ACCESSION);
                AddStableColumn(factory, PSMColumn.UNIQUE);
                AddStableColumn(factory, PSMColumn.DATABASE);
                AddStableColumn(factory, PSMColumn.DATABASE_VERSION);
                AddStableColumn(factory, PSMColumn.SEARCH_ENGINE);
                AddStableColumn(factory, PSMColumn.MODIFICATIONS);
                AddStableColumn(factory, PSMColumn.RETENTION_TIME);
                AddStableColumn(factory, PSMColumn.CHARGE);
                AddStableColumn(factory, PSMColumn.EXP_MASS_TO_CHARGE);
                AddStableColumn(factory, PSMColumn.CALC_MASS_TO_CHARGE);
                AddStableColumn(factory, PSMColumn.SPECTRA_REF);
                AddStableColumn(factory, PSMColumn.PRE);
                AddStableColumn(factory, PSMColumn.POST);
                AddStableColumn(factory, PSMColumn.START);
                AddStableColumn(factory, PSMColumn.END);
            }
            else if (section.Equals(Section.Small_Molecule_Header)){
                AddStableColumn(factory, SmallMoleculeColumn.IDENTIFIER);
                AddStableColumn(factory, SmallMoleculeColumn.CHEMICAL_FORMULA);
                AddStableColumn(factory, SmallMoleculeColumn.SMILES);
                AddStableColumn(factory, SmallMoleculeColumn.INCHI_KEY);
                AddStableColumn(factory, SmallMoleculeColumn.DESCRIPTION);
                AddStableColumn(factory, SmallMoleculeColumn.EXP_MASS_TO_CHARGE);
                AddStableColumn(factory, SmallMoleculeColumn.CALC_MASS_TO_CHARGE);
                AddStableColumn(factory, SmallMoleculeColumn.CHARGE);
                AddStableColumn(factory, SmallMoleculeColumn.RETENTION_TIME);
                AddStableColumn(factory, SmallMoleculeColumn.TAXID);
                AddStableColumn(factory, SmallMoleculeColumn.SPECIES);
                AddStableColumn(factory, SmallMoleculeColumn.DATABASE);
                AddStableColumn(factory, SmallMoleculeColumn.DATABASE_VERSION);
                AddStableColumn(factory, SmallMoleculeColumn.SPECTRA_REF);
                AddStableColumn(factory, SmallMoleculeColumn.SEARCH_ENGINE);
                AddStableColumn(factory, SmallMoleculeColumn.BEST_SEARCH_ENGINE_SCORE);
                AddStableColumn(factory, SmallMoleculeColumn.MODIFICATIONS);
            }
            else{
                throw new NotImplementedException("Section should be Protein, Peptide or " +
                                                  "Small_Molecule. Others can not setting. ");
            }

            foreach (var column in factory.StableColumnMapping){
                factory.ColumnMapping.Add(column.Key, column.Value);
            }
            factory.section = section;

            return factory;
        }

        /**
        * Get the table-based section. {@link Section#Protein_Header}, {@link Section#Peptide_Header} {@link Section#PSM_Header}
        * or {@link Section#Small_Molecule_Header}
        */
        public Section Section { get { return section; } }

        /**
         * Get stable columns mapping. Key is logical position, and value is MZTabColumn object.
         * Stable column with stable order: header name, data type, logical position and order are stable in these columns.
         * All of them have been defined in {@link ProteinColumn}, {@link PeptideColumn}, {@link PSMColumn} and
         * {@link SmallMoleculeColumn}.
         */
        public SortedDictionary<string, MZTabColumn> StableColumnMapping { get { return stableColumnMapping; } }

        /**
         * Get all abundance columns. Key is logical position, and value is MZTabColumn object.
         * Abundance Columns always stay in the end of the table-based section.
         *
         * @see AbundanceColumn
         */
        public SortedDictionary<string, MZTabColumn> AbundanceColumnMapping { get { return abundanceColumnMapping; } }

        /**
         * Get all optional columns, including option column with stable order and name, abundance columns, optional columns
         * and cv param optional columns. Key is logical position, and value is MZTabColumn object.
         *
         * @see AbundanceColumn
         * @see OptionColumn
         * @see CVParamOptionColumn
         */
        public SortedDictionary<string, MZTabColumn> OptionalColumnMapping { get { return optionalColumnMapping; } }

        /**
         * Get all columns in the factory. In this class, we maintain the following constraint at any time:
         * columnMapping.size == optionalColumnMapping.size + stableColumnMapping.size
         */
        public SortedDictionary<string, MZTabColumn> ColumnMapping { get { return columnMapping; } }

        /**
         * Add a optional column which has stable order and name, into {@link #optionalColumnMapping} and {@link #columnMapping}.
         *
         * @see MZTabColumn#createOptionalColumn(Section, MZTabColumn, IndexedElement)
         *
         * @exception ArgumentException: If user would like to add duplicate optional columns.
         * @param column SHOULD NOT set null.
         * @param msRun SHOULD NOT set null.
         */

        public void AddOptionalColumn(MZTabColumn column, MsRun msRun){
            string position = column.LogicPosition;
            if (optionalColumnMapping.ContainsKey(position)){
                throw new ArgumentException("There exists column " + optionalColumnMapping[position] + " in position " +
                                            position);
            }

            MZTabColumn newColumn = null;
            /*if (section.Equals(Section.Protein_Header)){
                if (position.Equals("09") || position.Equals("11") || position.Equals("12") || position.Equals("13")){
                    newColumn = MZTabColumn.CreateOptionalColumn(section, column, msRun);
                }
            }
            else if (section.Equals(Section.Peptide_Header)){
                if (position.Equals("08")){
                    newColumn = MZTabColumn.CreateOptionalColumn(section, column, msRun);
                }
            }
            else if (section.Equals(Section.Small_Molecule_Header)){
                if (position.Equals("19")){
                    newColumn = MZTabColumn.CreateOptionalColumn(section, column, msRun);
                }
            }*/

            if (section.Equals(Section.Protein_Header)) {
                if (column.Name.Equals(ProteinColumn.NUM_PSMS.Name) || column.Name.Equals(ProteinColumn.NUM_PEPTIDES_DISTINCT.Name) || column.Name.Equals(ProteinColumn.NUM_PEPTIDES_UNIQUE.Name)) {
                    newColumn = MZTabColumn.CreateOptionalColumn(section, column, null, msRun);
                }
            }

            if (newColumn != null){
                optionalColumnMapping.Add(newColumn.LogicPosition, newColumn);
                columnMapping.Add(newColumn.LogicPosition, newColumn);
            }
        }


        /**
         * Add the best {@link SearchEngineScore} across all replicates reported into {@link #optionalColumnMapping} and
         * {@link #columnMapping}. This column is not available for the PSM section
         *
         * @param column best search_engine_score column to add. SHOULD NOT set null.
         * @param id of the {section}_search_engine_score[id] param defined in {@link Metadata} for this column. SHOULD NOT set null.
         *
         * The header of the column will be represented as: best_search_engine_score[id]
         */
        public void AddBestSearchEngineScoreOptionalColumn(MZTabColumn column, Integer id) {
            String position = column.LogicPosition;
            if (optionalColumnMapping.ContainsKey(position)) {
                throw new ArgumentOutOfRangeException("There exists column " + optionalColumnMapping[position] + " in position " + position);
            }

            MZTabColumn newColumn = null;

            if (section.Equals(Section.Protein_Header)){
                if (column.Name.Equals(ProteinColumn.BEST_SEARCH_ENGINE_SCORE.Name)) {
                    newColumn = MZTabColumn.CreateOptionalColumn(section, column, id, null);
                }
            }else if (section.Equals(Section.Peptide_Header)){
                if (column.Name.Equals(PeptideColumn.BEST_SEARCH_ENGINE_SCORE.Name)) {
                    newColumn = MZTabColumn.CreateOptionalColumn(section, column, id, null);
                }
            }else if(section.Equals(Section.Small_Molecule_Header)){
                if (column.Name.Equals(SmallMoleculeColumn.BEST_SEARCH_ENGINE_SCORE.Name)) {
                    newColumn = MZTabColumn.CreateOptionalColumn(section, column, id, null);
                }
            }
                // TODO add a warning for PSMs, combination not possible
            

            if (newColumn != null) {
                optionalColumnMapping.Add(newColumn.LogicPosition, newColumn);
                columnMapping.Add(newColumn.LogicPosition, newColumn);
            }
        }

        /** Add {@link SearchEngineScore} followed by {@link MsRun} (MsRun will be null in the PSM section) which has stable order and name,
         * into {@link #optionalColumnMapping} and {@link #columnMapping}.
         *
         * @param column search_engine_score column to add. SHOULD NOT set null.
         * @param id of the {section}_search_engine_score[id] param defined in {@link Metadata} for this column. SHOULD NOT set null.
         * @param msRun {@link MsRun} for this search_engine_score
         *
         * The header will be represented as: search_engine_score[id]{_ms_run[1-n]}
         */
        public void AddSearchEngineScoreOptionalColumn(MZTabColumn column, Integer id, MsRun msRun) {
            String position = column.LogicPosition;
            if (optionalColumnMapping.ContainsKey(position)) {
                throw new ArgumentOutOfRangeException("There exists column " + optionalColumnMapping[position] + " in position " + position);
            }

            MZTabColumn newColumn = null;

            if (section.Equals(Section.Protein_Header)){
                if (column.Name.Equals(ProteinColumn.SEARCH_ENGINE_SCORE.Name)) {
                    newColumn = MZTabColumn.CreateOptionalColumn(section, column, id, msRun);
                }
            } else if (section.Equals(Section.Peptide_Header)){
                if (column.Name.Equals(PeptideColumn.SEARCH_ENGINE_SCORE.Name)) {
                    newColumn = MZTabColumn.CreateOptionalColumn(section, column, id, msRun);
                }
            }else if (section.Equals(Section.Small_Molecule_Header)){
                if (column.Name.Equals(SmallMoleculeColumn.SEARCH_ENGINE_SCORE.Name)) {
                    newColumn = MZTabColumn.CreateOptionalColumn(section, column, id, msRun);
                }
            }else if (section.Equals(Section.PSM_Header)){
                if (column.Name.Equals(PSMColumn.SEARCH_ENGINE_SCORE.Name)) {
                    newColumn = MZTabColumn.CreateOptionalColumn(section, column, id, null);
                }
            }

            if (newColumn != null) {
                optionalColumnMapping.Add(newColumn.LogicPosition, newColumn);
                columnMapping.Add(newColumn.LogicPosition, newColumn);
            }
        }

        public static int ParseColumnOrder(string position){
            return int.Parse(position.Substring(0, 2));
        }

        public static int ParseColumnId(string position){
            return position.Length == 2 ? 0 : int.Parse(position.Substring(2));
        }

        /**
         * Add {@link ProteinColumn#GO_TERMS} into {@link #optionalColumnMapping} and {@link #columnMapping}.
         *
         * Notice: this function only used in {@link Section#Protein_Header}
         */

        public void addGoTermsOptionalColumn(){
            if (section != Section.Protein_Header){
                throw new Exception("go_terms optional column only add into the protein section.");
            }

            MZTabColumn column = ProteinColumn.GO_TERMS;
            optionalColumnMapping.Add(column.LogicPosition, column);
            columnMapping.Add(column.LogicPosition, column);
        }

        /**
         * Add Reliability optional column into {@link #optionalColumnMapping} and {@link #columnMapping}.
         */

        public void addReliabilityOptionalColumn(){
            MZTabColumn column = null;
            if (section.Equals(Section.Protein_Header)){
                column = ProteinColumn.RELIABILITY;
            }
            else if (section.Equals(Section.Peptide_Header)){
                column = PeptideColumn.RELIABILITY;
            }
            else if (section.Equals(Section.Small_Molecule_Header)){
                column = SmallMoleculeColumn.RELIABILITY;
            }
            else if (section.Equals(Section.PSM_Header)){
                column = PSMColumn.RELIABILITY;
            }

            if (column != null){
                optionalColumnMapping.Add(column.LogicPosition, column);
                columnMapping.Add(column.LogicPosition, column);
            }
        }

        /**
         * Add URI optional column into {@link #optionalColumnMapping} and {@link #columnMapping}.
         */

        public void addURIOptionalColumn(){
            MZTabColumn column = null;
            if (section.Equals(Section.Protein_Header)){
                column = ProteinColumn.URI;
            }
            else if (section.Equals(Section.Peptide_Header)){
                column = PeptideColumn.URI;
            }
            else if (section.Equals(Section.Small_Molecule_Header)){
                column = SmallMoleculeColumn.URI;
            }
            else if (section.Equals(Section.PSM_Header)){
                column = PSMColumn.URI;
            }


            if (column != null){
                optionalColumnMapping.Add(column.LogicPosition, column);
                columnMapping.Add(column.LogicPosition, column);
            }
        }

        private string AddOptionColumn(MZTabColumn column){
            string logicalPosition = column.LogicPosition;

            optionalColumnMapping.Add(logicalPosition, column);
            columnMapping.Add(column.LogicPosition, column);

            return logicalPosition;
        }

        /**
         * Add global {@link OptionColumn} into {@link #optionalColumnMapping} and {@link #columnMapping}.
         * The header like: opt_global_{name}
         *
         * @param name SHOULD NOT empty.
         * @param columnType SHOULD NOT empty.
         */

        public String AddOptionalColumn(String name, Type columnType){
            MZTabColumn column = new OptionColumn(null, name, columnType, ParseColumnOrder(columnMapping.Last().Key));
            return AddOptionColumn(column);
        }

        /**
     * Add {@link OptionColumn} followed by assay into {@link #optionalColumnMapping} and {@link #columnMapping}.
     * The header like: opt_assay[1]_{name}
     *
     * @param assay SHOULD NOT empty.
     * @param name SHOULD NOT empty.
     * @param columnType SHOULD NOT empty.
     */

        public string AddOptionalColumn(Assay assay, string name, Type columnType){
            MZTabColumn column = new OptionColumn(assay, name, columnType, ParseColumnOrder(columnMapping.Last().Key));
            return AddOptionColumn(column);
        }

        /**
          * Add {@link OptionColumn} followed by study variable into {@link #optionalColumnMapping} and {@link #columnMapping}.
          * The header like: opt_study_variable[1]_{name}
          *
          * @param studyVariable SHOULD NOT empty.
          * @param name SHOULD NOT empty.
          * @param columnType SHOULD NOT empty.
          */

        public string AddOptionalColumn(StudyVariable studyVariable, string name, Type columnType){
            MZTabColumn column = new OptionColumn(studyVariable, name, columnType,
                                                  ParseColumnOrder(columnMapping.Last().Key));
            return AddOptionColumn(column);
        }

        /**
         * Add {@link OptionColumn} followed by ms_run into {@link #optionalColumnMapping} and {@link #columnMapping}.
         * The header like: opt_ms_run[1]_{name}
         *
         * @param msRun SHOULD NOT empty.
         * @param name SHOULD NOT empty.
         * @param columnType SHOULD NOT empty.
         */

        public string AddOptionalColumn(MsRun msRun, String name, Type columnType){
            MZTabColumn column = new OptionColumn(msRun, name, columnType, ParseColumnOrder(columnMapping.Last().Key));
            return AddOptionColumn(column);
        }

        /**
         * Add global {@link CVParamOptionColumn} into {@link #optionalColumnMapping} and {@link #columnMapping}.
         * The header like: opt_global_cv_{accession}_{parameter name}
         *
         * @param param SHOULD NOT empty.
         * @param columnType SHOULD NOT empty.
         */

        public string AddOptionalColumn(CVParam param, Type columnType){
            MZTabColumn column = new CVParamOptionColumn(null, param, columnType,
                                                         ParseColumnOrder(columnMapping.Last().Key));
            return AddOptionColumn(column);
        }

        /**
         * Add {@link CVParamOptionColumn} followed by assay into {@link #optionalColumnMapping} and {@link #columnMapping}.
         * The header like: opt_assay[1]_cv_{accession}_{parameter name}
         *
         * @param assay SHOULD NOT empty.
         * @param param SHOULD NOT empty.
         * @param columnType SHOULD NOT empty.
         */

        public string AddOptionalColumn(Assay assay, CVParam param, Type columnType){
            MZTabColumn column = new CVParamOptionColumn(assay, param, columnType,
                                                         ParseColumnOrder(columnMapping.Last().Key));
            return AddOptionColumn(column);
        }

        /**
         * Add {@link CVParamOptionColumn} followed by study variable into {@link #optionalColumnMapping} and {@link #columnMapping}.
         * The header like: opt_study_variable[1]_cv_{accession}_{parameter name}
         *
         * @param studyVariable SHOULD NOT empty.
         * @param param SHOULD NOT empty.
         * @param columnType SHOULD NOT empty.
         */

        public string AddOptionalColumn(StudyVariable studyVariable, CVParam param, Type columnType){
            MZTabColumn column = new CVParamOptionColumn(studyVariable, param, columnType,
                                                         ParseColumnOrder(columnMapping.Last().Key));
            return AddOptionColumn(column);
        }

        /**
         * Add {@link CVParamOptionColumn} followed by ms_run into {@link #optionalColumnMapping} and {@link #columnMapping}.
         * The header like: opt_ms_run[1]_cv_{accession}_{parameter name}
         *
         * @param msRun SHOULD NOT empty.
         * @param param SHOULD NOT empty.
         * @param columnType SHOULD NOT empty.
         */

        public string AddOptionalColumn(MsRun msRun, CVParam param, Type columnType){
            MZTabColumn column = new CVParamOptionColumn(msRun, param, columnType,
                                                         ParseColumnOrder(columnMapping.Last().Key));
            return AddOptionColumn(column);
        }

        /**
         * Add {@link AbundanceColumn} into {@link AbundanceColumn}, {@link #optionalColumnMapping} and {@link #columnMapping}.
         * The header like: {Section}_abundance_assay[1]
         *
         * @see AbundanceColumn#createOptionalColumn(Section, Assay, int)
         *
         * @param assay SHOULD NOT empty.
         */

        public string AddAbundanceOptionalColumn(Assay assay){
            MZTabColumn column = AbundanceColumn.createOptionalColumn(section, assay,
                                                                                            ParseColumnOrder(
                                                                                                columnMapping.Last().Key));
            abundanceColumnMapping.Add(column.LogicPosition, column);
            return AddOptionColumn(column);
        }

        /**
         * Add three {@link AbundanceColumn} into {@link AbundanceColumn}, {@link #optionalColumnMapping} and {@link #columnMapping} at one time.
         * The header like: {Section}_abundance_study_variable[1], {Section}_abundance_stdev_study_variable[1],
         * {Section}_abundance_std_error_study_variable[1].
         *
         * @see AbundanceColumn#createOptionalColumns(Section, StudyVariable, String)
         *
         * @param studyVariable SHOULD NOT empty.
         */

        public string AddAbundanceOptionalColumn(StudyVariable studyVariable){
            SortedDictionary<string, MZTabColumn> columns =
                AbundanceColumn.createOptionalColumns(section, studyVariable,
                                                                            ParseColumnOrder(columnMapping.Last().Key));
            foreach (var column in columns){
                abundanceColumnMapping.Add(column.Key, column.Value);
                optionalColumnMapping.Add(column.Key, column.Value);
                columnMapping.Add(column.Key, column.Value);
            }
            return columns.Last().Key;
        }

        /**
     * @return tab split column header string list.
     */

        public SplitList<string> GetHeaderList(){
            SplitList<string> headerList = new SplitList<string>(MZTabConstants.TAB);

            foreach (MZTabColumn mzTabColumn in columnMapping.Values){
                headerList.Add(mzTabColumn.Header);
            }

            return headerList;
        }

        /**
     * [PRH|PEH/SMH]    header1 header2 ...
     */

        public override string ToString(){
            return section.Prefix + MZTabConstants.TAB + GetHeaderList();
        }

        /**
     * @return map(offset, MZTabColumn), the offset record the position of MZTabColumn in header line.
     */

        public SortedDictionary<int, MZTabColumn> GetOffsetColumnsMap(){
            SortedDictionary<int, MZTabColumn> map = new SortedDictionary<int, MZTabColumn>();

            int offset = 1;
            foreach (MZTabColumn column in columnMapping.Values){
                map.Add(offset++, column);
            }

            return map;
        }

        public bool IsOptionalColumn(string header){
            if (section.Equals(Section.Protein_Header)){
                if (header.StartsWith(ProteinColumn.SEARCH_ENGINE_SCORE.Name) ||
                    header.StartsWith(ProteinColumn.NUM_PSMS.Name) ||
                    header.StartsWith(ProteinColumn.NUM_PEPTIDES_DISTINCT.Name) ||
                    header.StartsWith(ProteinColumn.NUM_PEPTIDES_UNIQUE.Name) ||
                    header.StartsWith("protein_abundance_assay") ||
                    header.StartsWith("protein_abundance_study_variable") ||
                    header.StartsWith("protein_abundance_stdev_study_variable") ||
                    header.StartsWith("protein_abundance_std_error_study_variable")){
                    return true;
                }
            }
            else if (section.Equals(Section.Peptide_Header)){
                if (header.StartsWith(PeptideColumn.SEARCH_ENGINE_SCORE.Name) ||
                    header.StartsWith("peptide_abundance_assay") ||
                    header.StartsWith("peptide_abundance_study_variable") ||
                    header.StartsWith("peptide_abundance_stdev_study_variable") ||
                    header.StartsWith("peptide_abundance_std_error_study_variable")){
                    return true;
                }
            }
            else if (section.Equals(Section.Small_Molecule_Header)){
                if (header.StartsWith(SmallMoleculeColumn.SEARCH_ENGINE_SCORE.Name) ||
                    header.StartsWith("smallmolecule_abundance_assay") ||
                    header.StartsWith("smallmolecule_abundance_study_variable") ||
                    header.StartsWith("smallmolecule_abundance_stdev_study_variable") ||
                    header.StartsWith("smallmolecule_abundance_std_error_study_variable")){
                    return true;
                }
            }


            return header.StartsWith("opt_");
        }

        /**
     * Based on header name to query the MZTabColumn.
     * Notice: for optional columns, header name maybe flexible. For example,
     * num_peptides_distinct_ms_file[1].
     * At this time, user SHOULD BE provide the concrete header name to query
     * MZTabColumn. If just provide num_peptides_distinct, return null.
     *
     * @see
     */

        public MZTabColumn FindColumnByHeader(string header){
            header = header.Trim();
            foreach (MZTabColumn column in columnMapping.Values){
                if (header.Equals(column.Header, StringComparison.CurrentCultureIgnoreCase)){
                    return column;
                }
            }

            return null;
        }

        /**
         * Query the MZTabColumn in factory, based on column logical position.
         */

        public MZTabColumn FindColumnByPosition(string logicalPosition){
            return columnMapping[logicalPosition];
        }
    }
}