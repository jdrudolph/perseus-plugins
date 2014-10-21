using System;
using System.Collections.Generic;
using System.Linq;

namespace PluginMzTab.Lib.Model{
    /**
	 * User: qingwei
	 * Date: 14/10/13
	 */

    public class MetadataSubElement{
        public static MetadataSubElement ASSAY_QUANTIFICATION_MOD = new MetadataSubElement(MetadataElement.ASSAY,
                                                                                           "quantification_mod");

        public static IList<MetadataSubElement> All = new List<MetadataSubElement>{ASSAY_QUANTIFICATION_MOD};

        private readonly MetadataElement _element;
        private readonly string _subName;

        private MetadataSubElement(MetadataElement element, string subName){
            _element = element;
            _subName = subName;
        }

        public string Name { get { return _element.Name + "_" + _subName; } }

        public string SubName { get { return _subName; } }

        public MetadataElement Element { get { return _element; } }


        public override string ToString(){
            return SubName;
        }

        /**
	     * subElementName should include elementName.
	     * For example: assay_quantification_mod
	     */

        public static MetadataSubElement FindSubElement(MetadataElement element, string subElementName){
            if (element == null || subElementName == null){
                return null;
            }


            MetadataSubElement subElement;
            try{
                subElement =
                    All.First(
                        x =>
                        x.Element.Name.Equals(element.Name, StringComparison.CurrentCultureIgnoreCase) &&
                        x.SubName.Equals(subElementName, StringComparison.CurrentCultureIgnoreCase));
            }
            catch (ArgumentNullException){
                subElement = null;
            }
            catch (InvalidOperationException){
                subElement = null;
            }

            return subElement;
        }
    }
}