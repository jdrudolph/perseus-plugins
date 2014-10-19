using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using BaseLib.Util;
using PluginMzTab.Plugin.Utils;

namespace PluginMzTab.Lib.Model{
/**
 * MZTabRecord used to store a row record of the table.
 *
 * User: Qingwei
 * Date: 23/05/13
 */

    public class MZTabRecord{
        protected MZTabColumnFactory factory;

        private readonly Dictionary<string, object> record = new Dictionary<string, object>();

        public MZTabRecord(MZTabColumnFactory factory){
            if (factory == null){
                throw new NullReferenceException("Not create MZTabColumn by using MZTabColumnFactory yet.");
            }

            this.factory = factory;
            foreach (string position in factory.ColumnMapping.Keys){
                setValue(position, null);
            }
        }

        /**
     * validate the mzTabColumn's dataType match with the data's valueType.
     *
     * @see uk.ac.ebi.pride.jmztab.model.MZTabColumn#getDataType()
     */

        private bool isMatch(string position, Type valueType){
            MZTabColumn column = factory.ColumnMapping[position];
            if (column == null){
                return false;
            }

            Type columnMzTabType = column.Type;
            return valueType == columnMzTabType;
        }

        public void Add(string position, object value){
            if (!record.ContainsKey(position)){
                record.Add(position, value);
            }
            //if (record[position] == null){
            record[position] = value;
            //}
            //else{
            //TODO: throw new Exception("Could not add key twice.");
            //}
        }

        public bool setValue(string position, Object value){
            if (value == null){
                Add(position, value);
                return true;
            }

            if (isMatch(position, value.GetType())){
                Add(position, value);
                return true;
            }
            return false;
        }

        public Object getValue(string position){
            return record[position];
        }

        private Object translateValue(Object value){
            if (value == null){
                return null;
            }
            if (value is double){
                double v = (double) value;
                if (double.IsNaN(v)){
                    return MZTabConstants.CALCULATE_ERROR;
                }
                if (double.IsInfinity(v)){
                    return MZTabConstants.INFINITY;
                }
                return value;
            }
            /*TODO:if (value.GetType().GetGenericTypeDefinition() == typeof(List<>)){
            var v = value as List<>;
            if (((List)value).string.IsNullOrEmpty()) {
                return null;
            }
            return value;
        }*/
            return value;
        }

        /**
     * Tab split string.
     * value1   value2  value3  ...
     */

        public override string ToString(){
            IList<object> values = record.Values.Select(translateValue).ToList();
            for (int i = 0; i < values.Count(); i++){
                if (values[i] == null){
                    values[i] = "NULL";
                }
            }
            return StringUtils.Concat(MZTabConstants.TAB.ToString(CultureInfo.InvariantCulture), values);
        }

        protected string getString(string position){
            if (! isMatch(position, typeof (string))){
                return null;
            }

            return (string) getValue(position);
        }

        protected Integer getInteger(string position){
            if (! isMatch(position, typeof (Integer))){
                return null;
            }

            return (Integer)getValue(position);
        }

        protected double getDouble(string position){
            if (! isMatch(position, typeof (double))){
                return double.MinValue;
            }

            return (double) getValue(position);
        }

        protected SplitList<T> GetSplitList<T>(string position){
            if (!isMatch(position, typeof (SplitList<T>))){
                return null;
            }

            return (SplitList<T>) getValue(position);
        }

        protected Uri getURI(string position){
            if (! isMatch(position, typeof (Uri))){
                return null;
            }

            return (Uri) getValue(position);
        }

        protected Reliability getReliability(string position){
            if (! isMatch(position, typeof (Reliability))){
                return null;
            }

            return (Reliability) getValue(position);
        }

        protected MZBoolean getMZBoolean(string position){
            if (! isMatch(position, typeof (MZBoolean))){
                return null;
            }

            return (MZBoolean) getValue(position);
        }

        /**
        * Get logical position based on column's order and element id.
        *
        * order + id + element.id
        */
        protected String getLogicalPosition(MZTabColumn column, Integer id, IndexedElement element){
            StringBuilder sb = new StringBuilder();

            sb.Append(column.Order);
            if (id != null){
                // generate id string which length is 2. Eg. 12, return 12; 1, return 01
                sb.Append(string.Format("{0:00}", id));
            }
            else{
                sb.Append("00");
            }
            if (element != null){
                sb.Append(string.Format("{0:00}", element.Id));
            }
            else{
                sb.Append("00");
            }

            return sb.ToString();
        }

        protected string getPosition(MZTabColumn column, IndexedElement element){
            return column.Order + element.Id;
        }

        private MZTabColumn getColumn(string tag, IndexedElement element){
            Section dataSection = Section.toDataSection(factory.Section);
            string header = dataSection.Name + tag + element.Reference;

            return factory.FindColumnByHeader(header);
        }

        public double getAbundanceColumn(Assay assay){
            MZTabColumn column = getColumn("_abundance_", assay);
            if (column == null){
                return double.MinValue;
            }
            return getDouble(column.LogicPosition);
        }

        public void setAbundanceColumn(Assay assay, double value){
            MZTabColumn column = getColumn("_abundance_", assay);
            if (column != null){
                setValue(column.LogicPosition, value);
            }
        }

        public void setAbundanceColumn(Assay assay, string valueLabel){
            setAbundanceColumn(assay, MZTabUtils.ParseDouble(valueLabel));
        }

        public double getAbundanceColumn(StudyVariable studyVariable){
            MZTabColumn column = getColumn("_abundance_", studyVariable);
            if (column == null){
                return double.MinValue;
            }
            return getDouble(column.LogicPosition);
        }

        public void setAbundanceColumn(StudyVariable studyVariable, double value){
            MZTabColumn column = getColumn("_abundance_", studyVariable);
            if (column != null){
                setValue(column.LogicPosition, value);
            }
        }

        public void setAbundanceColumn(StudyVariable studyVariable, string valueLabel){
            setAbundanceColumn(studyVariable, MZTabUtils.ParseDouble(valueLabel));
        }

        public double getAbundanceStdevColumn(StudyVariable studyVariable){
            MZTabColumn column = getColumn("_abundance_stdev_", studyVariable);
            if (column == null){
                return double.MinValue;
            }
            return getDouble(column.LogicPosition);
        }

        public void setAbundanceStdevColumn(StudyVariable studyVariable, double value){
            MZTabColumn column = getColumn("_abundance_stdev_", studyVariable);
            if (column != null){
                setValue(column.LogicPosition, value);
            }
        }

        public void setAbundanceStdevColumn(StudyVariable studyVariable, string valueLabel){
            setAbundanceStdevColumn(studyVariable, MZTabUtils.ParseDouble(valueLabel));
        }

        public double getAbundanceStdErrorColumn(StudyVariable studyVariable){
            MZTabColumn column = getColumn("_abundance_std_error_", studyVariable);
            if (column == null){
                return double.MinValue;
            }
            return getDouble(column.LogicPosition);
        }

        public void setAbundanceStdErrorColumn(StudyVariable studyVariable, double value){
            MZTabColumn column = getColumn("_abundance_std_error_", studyVariable);
            if (column != null){
                setValue(column.LogicPosition, value);
            }
        }

        public void setAbundanceStdErrorColumn(StudyVariable studyVariable, string valueLabel){
            setAbundanceStdErrorColumn(studyVariable, MZTabUtils.ParseDouble(valueLabel));
        }

        /**
     * return user defined optional column,
     * opt_{ASSAY_ID}_name column's record value.
     *
     * If not find the optional column, return null;
     */

        public string getOptionColumn(Assay assay, string name){
            string header = OptionColumn.GetHeader(assay, name);
            MZTabColumn column = factory.FindColumnByHeader(header);
            return column == null ? null : getString(column.LogicPosition);
        }

        public void setOptionColumn(Assay assay, string name, string value){
            string header = OptionColumn.GetHeader(assay, name);
            MZTabColumn column = factory.FindColumnByHeader(header);
            if (column != null){
                setValue(column.LogicPosition, value);
            }
        }

        public string getOptionColumn(Assay assay, CVParam param){
            string header = CVParamOptionColumn.GetHeader(assay, param);
            MZTabColumn column = factory.FindColumnByHeader(header);
            return column == null ? null : getString(column.LogicPosition);
        }

        public void setOptionColumn(Assay assay, CVParam param, string value){
            string header = CVParamOptionColumn.GetHeader(assay, param);
            MZTabColumn column = factory.FindColumnByHeader(header);
            if (column != null){
                setValue(column.LogicPosition, value);
            }
        }

        /**
     * return user defined optional column,
     * opt_{STUDY_VARIABLE_ID}_name column's record value.
     *
     * If not find the optional column, return null;
     */

        public string getOptionColumn(StudyVariable studyVariable, string name){
            string header = OptionColumn.GetHeader(studyVariable, name);
            MZTabColumn column = factory.FindColumnByHeader(header);
            return column == null ? null : getString(column.LogicPosition);
        }

        public void setOptionColumn(StudyVariable studyVariable, string name, string value){
            string header = OptionColumn.GetHeader(studyVariable, name);
            MZTabColumn column = factory.FindColumnByHeader(header);
            if (column != null){
                setValue(column.LogicPosition, value);
            }
        }

        public string getOptionColumn(StudyVariable studyVariable, CVParam param){
            string header = CVParamOptionColumn.GetHeader(studyVariable, param);
            MZTabColumn column = factory.FindColumnByHeader(header);
            return column == null ? null : getString(column.LogicPosition);
        }

        public void setOptionColumn(StudyVariable studyVariable, CVParam param, string value){
            string header = CVParamOptionColumn.GetHeader(studyVariable, param);
            MZTabColumn column = factory.FindColumnByHeader(header);
            if (column != null){
                setValue(column.LogicPosition, value);
            }
        }

        /**
     * return user defined optional column,
     * opt_{MS_FILE_ID}_name column's record value.
     *
     * If not find the optional column, return null;
     */

        public string getOptionColumn(MsRun msRun, string name){
            string header = OptionColumn.GetHeader(msRun, name);
            MZTabColumn column = factory.FindColumnByHeader(header);
            return column == null ? null : getString(column.LogicPosition);
        }

        public void setOptionColumn(MsRun msRun, string name, string value){
            string header = OptionColumn.GetHeader(msRun, name);
            MZTabColumn column = factory.FindColumnByHeader(header);
            if (column != null){
                setValue(column.LogicPosition, value);
            }
        }

        public string getOptionColumn(MsRun msRun, CVParam param){
            string header = CVParamOptionColumn.GetHeader(msRun, param);
            MZTabColumn column = factory.FindColumnByHeader(header);
            return column == null ? null : getString(column.LogicPosition);
        }

        public void setOptionColumn(MsRun msRun, CVParam param, string value){
            string header = CVParamOptionColumn.GetHeader(msRun, param);
            MZTabColumn column = factory.FindColumnByHeader(header);
            if (column != null){
                setValue(column.LogicPosition, value);
            }
        }

        /**
     * return user defined optional column,
     * opt_global_name column's record value.
     *
     * If not find the optional column, return null;
     */

        public string getOptionColumn(string name){
            string header = OptionColumn.GetHeader(null, name);
            MZTabColumn column = factory.FindColumnByHeader(header);
            return column == null ? null : getString(column.LogicPosition);
        }

        public void setOptionColumn(string name, Object value){
            string header = OptionColumn.GetHeader(null, name);
            MZTabColumn column = factory.FindColumnByHeader(header);
            if (column != null){
                setValue(column.LogicPosition, value);
            }
        }

        public string getOptionColumn(CVParam param){
            string header = CVParamOptionColumn.GetHeader(null, param);
            MZTabColumn column = factory.FindColumnByHeader(header);
            return column == null ? null : getString(column.LogicPosition);
        }

        public void setOptionColumn(CVParam param, Object value){
            string header = CVParamOptionColumn.GetHeader(null, param);
            MZTabColumn column = factory.FindColumnByHeader(header);
            if (column != null){
                setValue(column.LogicPosition, value);
            }
        }
    }
}