using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using BaseLib.Util;

/**
 * User: Qingwei
 * Date: 23/05/13
 */

namespace PluginMzTab.Lib.Model{
    public class IndexedElement{
        private readonly MetadataElement element;
        private readonly int id;

        protected internal IndexedElement(MetadataElement element, int id){
            if (element == null){
                throw new ArgumentNullException("element");
            }
            if (id <= 0){
                throw new IndexOutOfRangeException("id value should great than 0!");
            }

            this.element = element;
            this.id = id;
        }

        public int Id { get { return id; } }

        public MetadataElement Element { get { return element; } }

        protected internal virtual string Reference { get { return string.Format("{0}[{1}]", element, id); } }

        private string Prefix { get { return Section.Metadata.Prefix; } }

        protected string TAB { get { return MZTabConstants.TAB.ToString(CultureInfo.InvariantCulture); } }

        /**
        * MTD  {element}[id]    {value.toString}
         */

        protected StringBuilder printPrefix(StringBuilder sb){
            return sb.Append(Prefix).Append(MZTabConstants.TAB);
        }

        /**
         * MTD  {element}[id]    {value.toString}
         */

        protected string printElement(Object value){
            return StringUtils.Concat(MZTabConstants.TAB.ToString(CultureInfo.InvariantCulture),
                                      new[]{Prefix, Reference, value});
        }

        /**
         * MTD  {element}[id]-{property}    {value.toString}
         */

        protected string printProperty(MetadataProperty property, Object value){
            string result = string.Format("{0}{1}{2}{3}{4}", Prefix, MZTabConstants.TAB, Reference, MZTabConstants.MINUS,
                                          property);

            if (value != null){
                result += string.Format("{0}{1}", MZTabConstants.TAB, value);
            }

            return result;
        }

        /**
         * print a list of metadata line.
         */

        protected StringBuilder printList(List<Param> list, MetadataProperty property, StringBuilder sb){
            foreach (Param param in list){
                sb.Append(printProperty(property, param)).Append(MZTabConstants.NEW_LINE);
            }

            return sb;
        }
    }
}