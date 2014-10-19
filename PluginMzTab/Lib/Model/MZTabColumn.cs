using System;
using System.Text;
using PluginMzTab.Plugin.Utils;

namespace PluginMzTab.Lib.Model{
/**
 * User: Qingwei
 * Date: 23/05/13
 */

    public class MZTabColumn{
        private readonly string _name;
        private readonly string _order;
        private readonly Integer _id;
        private string _header;
        private string _logicPosition;
        private readonly Type _type;
        private readonly bool _optional;

        private IndexedElement element;

        public MZTabColumn(string name, Type type, bool optional, string order, Integer id) {
            _name = name;
            if (type == null) {
                throw new NullReferenceException("Column data type should not set null!");
            }

            _type = type;
            _optional = optional;
            _order = order;
            _id = id;
            _header = generateHeader(name);
            _logicPosition = generateLogicalPosition();
        }

        public MZTabColumn(String name, Type dataType, bool optional, String order): this(name, dataType, optional, order, null) {
            
        }

        private String generateHeader(String name) {
            StringBuilder sb = new StringBuilder();

            sb.Append(name);
            if (_id != null) {
                sb.Append("[").Append(_id).Append("]");
            }

            return sb.ToString();
        }

        private String generateLogicalPosition() {
            StringBuilder sb = new StringBuilder();

            sb.Append(_order);
            if (_id != null) {
                // generate id string which length is 2. Eg. 12, return 12; 1, return 01
                sb.Append(String.Format("{0:00}", _id));
            } else {
                sb.Append("00");
            }

            if (element != null) {
                sb.Append(String.Format("{0:00}", element.Id));
            } else {
                sb.Append("00");
            }

            return sb.ToString();
        }

        public string Name { get { return _name; } }

        public string Order { get { return _order; } }

        public string Header { get { return _header; } }

        public string LogicPosition { get { return _logicPosition; } }

        public Type Type { get { return _type; } }

        public bool isOptional(){
            return _optional;
        }

        public IndexedElement Element{
            get { return element; }
            set{
                if (value == null) {
                    throw new NullReferenceException("Can not set null indexed element for optional column!");
                }
                element = value;
                _logicPosition = generateLogicalPosition();
                    
                    
                    _header = string.Format("{0}_{1}", _header, element.Reference);
                
            }
        }
        
        internal static MZTabColumn CreateOptionalColumn(Section section, MZTabColumn column, Integer id, IndexedElement element) {
            if (! column.isOptional()){
                throw new ArgumentException(column + " is not optional column!");
            }

            MZTabColumn optionColumn = null;

            if (section.Equals(Section.Protein_Header)){
                optionColumn = new ProteinColumn(column.Name, column.Type, column.isOptional(), column.Order, id);
            }
            else if (section.Equals(Section.Peptide_Header)){
                optionColumn = new PeptideColumn(column.Name, column.Type, column.isOptional(), column.Order, id);
            }
            else if (section.Equals(Section.PSM_Header)){
                optionColumn = new PSMColumn(column.Name, column.Type, column.isOptional(), column.Order, id);
            }
            else if (section.Equals(Section.Small_Molecule_Header)){
                optionColumn = new SmallMoleculeColumn(column.Name, column.Type, column.isOptional(), column.Order, id);
            }

            if (optionColumn != null && element != null){
                optionColumn.Element = element;
            }           

            return optionColumn;
        }

        public override string ToString(){
            return string.Format("MZTabColumn{{header='{0}', logicPosition='{1}', type='{2}', optional='{3}'}}", _header,
                                 _logicPosition, _type, _optional);
        }

        public override bool Equals(Object o){
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;

            MZTabColumn column = (MZTabColumn) o;

            if (_optional != column._optional) return false;
            if (_type != null ? !(_type == column._type) : column._type != null) return false;
            if (_header != null ? !_header.Equals(column._header) : column._header != null) return false;
            if (_logicPosition != null ? !_logicPosition.Equals(column._logicPosition) : column._logicPosition != null)
                return false;

            return true;
        }

        public override int GetHashCode(){
            int result = _header != null ? _header.GetHashCode() : 0;
            result = 31*result + (_logicPosition != null ? _logicPosition.GetHashCode() : 0);
            result = 31*result + (_type != null ? _type.GetHashCode() : 0);
            result = 31*result + (_optional ? 1 : 0);
            return result;
        }
    }
}