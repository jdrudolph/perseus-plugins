using System;

namespace PluginMzTab.Lib.Model{
/**
 * {column header} = {parameter defining the unit}
 *
 * User: Qingwei
 * Date: 04/06/13
 */

    public class ColUnit{
        private readonly MZTabColumn column;
        private readonly Param value;

        /**
     * Defines the used unit for a column in the peptide/protein/small_molecule section.
     * The format of the value has to be {column name}={Parameter defining the unit}
     */

        internal ColUnit(MZTabColumn column, Param value){
            if (column == null){
                throw new NullReferenceException("MZTabColumn can not set null");
            }

            if (value == null){
                throw new NullReferenceException("Param can not set null");
            }

            if (column is PluginMzTab.Lib.Model.AbundanceColumn){
                throw new Exception("Colunit MUST NOT be used to define a unit for quantification columns.");
            }

            this.column = column;
            this.value = value;
        }

        /**
     * {column name}={Parameter defining the unit}
     */

        public override string ToString(){
            return string.Format("{0}={1}", column.Header, value);
        }
    }
}