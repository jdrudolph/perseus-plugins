using System.Collections.Generic;

namespace PluginMzTab.Lib.Model{
/**
 * Quantitative technologies generally result in some kind of abundance measurement of the identified analyze.
 * Several of the available techniques, furthermore, allow/require multiple similar samples to be multiplexed
 * and analyzed in a single MS run – for example in label-based techniques, such as SILAC/N15 where
 * quantification occurs on MS1 data or in tag-based techniques, such as iTRAQ/TMT where quantification occurs
 * in MS2 data.
 *
 * NOTICE: colunit columns MUST NOT be used to define a unit for quantification columns.
 * In mzTab package, these quantification columns are AbundanceColumn.
 *
 * @see ColUnit
 *
 * User: Qingwei
 * Date: 23/05/13
 */

    public class Field{
        public static Field ABUNDANCE = new Field("abundance", typeof (double), 1);
        public static Field ABUNDANCE_STDEV = new Field("abundance_stdev", typeof (double), 2);
        public static Field ABUNDANCE_STD_ERROR = new Field("abundance_std_error", typeof (double), 3);

        internal readonly string name;
        internal readonly System.Type columnType;
        internal readonly int position;

        private Field(string name, System.Type columnType, int position){
            this.name = name;
            this.columnType = columnType;
            this.position = position;
        }

        public override string ToString(){
            return name;
        }
    }

    public class AbundanceColumn : MZTabColumn{
        /**
     * This is a temporary method, which face small molecule abundance column:
     * translate small_molecule --> smallmolecule
     * @see uk.ac.ebi.pride.jmztab.utils.parser.MZTabHeaderLineParser#translate(string)
     */

        private static string translate(string oldName){
            return oldName.Equals("small_molecule") ? "smallmolecule" : oldName;
        }

        private AbundanceColumn(Section section, Field field, IndexedElement element, int offset)
            : base(translate(section.Name) + "_" + field.name, field.columnType, true, offset + field.position + ""){
            Element = element;
        }

        public static MZTabColumn createOptionalColumn(Section section, Assay assay, int offset){
            return new AbundanceColumn(Section.toDataSection(section), Field.ABUNDANCE, assay, offset);
        }

        public static SortedDictionary<string, MZTabColumn> createOptionalColumns(Section section,
                                                                                  StudyVariable studyVariable,
                                                                                  int offset){
            SortedDictionary<string, MZTabColumn> columns = new SortedDictionary<string, MZTabColumn>();
            Section dataSection = Section.toDataSection(section);

            AbundanceColumn column = new AbundanceColumn(dataSection, Field.ABUNDANCE, studyVariable, offset);
            columns.Add(column.LogicPosition, column);
            column = new AbundanceColumn(dataSection, Field.ABUNDANCE_STDEV, studyVariable, offset);
            columns.Add(column.LogicPosition, column);
            column = new AbundanceColumn(dataSection, Field.ABUNDANCE_STD_ERROR, studyVariable, offset);
            columns.Add(column.LogicPosition, column);

            return columns;
        }
    }
}