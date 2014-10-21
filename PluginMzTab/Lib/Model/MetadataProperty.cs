using System;
using System.Collections.Generic;
using System.Linq;

namespace PluginMzTab.Lib.Model{
    public class MetadataProperty{
        public static MetadataProperty MZTAB_VERSION = new MetadataProperty(MetadataElement.MZTAB, "version");
        public static MetadataProperty MZTAB_MODE = new MetadataProperty(MetadataElement.MZTAB, "mode");
        public static MetadataProperty MZTAB_TYPE = new MetadataProperty(MetadataElement.MZTAB, "type");
        public static MetadataProperty MZTAB_ID = new MetadataProperty(MetadataElement.MZTAB, "ID");

        public static MetadataProperty INSTRUMENT_NAME = new MetadataProperty(MetadataElement.INSTRUMENT, "name");
        public static MetadataProperty INSTRUMENT_SOURCE = new MetadataProperty(MetadataElement.INSTRUMENT, "source");
        public static MetadataProperty INSTRUMENT_ANALYZER = new MetadataProperty(MetadataElement.INSTRUMENT, "analyzer");
        public static MetadataProperty INSTRUMENT_DETECTOR = new MetadataProperty(MetadataElement.INSTRUMENT, "detector");

        public static MetadataProperty SOFTWARE_SETTING = new MetadataProperty(MetadataElement.SOFTWARE, "setting");

        public static MetadataProperty CONTACT_NAME = new MetadataProperty(MetadataElement.CONTACT, "name");
        public static MetadataProperty CONTACT_AFFILIATION = new MetadataProperty(MetadataElement.CONTACT, "affiliation");
        public static MetadataProperty CONTACT_EMAIL = new MetadataProperty(MetadataElement.CONTACT, "email");

        public static MetadataProperty FIXED_MOD_SITE = new MetadataProperty(MetadataElement.FIXED_MOD, "site");
        public static MetadataProperty FIXED_MOD_POSITION = new MetadataProperty(MetadataElement.FIXED_MOD, "position");

        public static MetadataProperty VARIABLE_MOD_SITE = new MetadataProperty(MetadataElement.VARIABLE_MOD, "site");

        public static MetadataProperty VARIABLE_MOD_POSITION = new MetadataProperty(MetadataElement.VARIABLE_MOD,
                                                                                    "position");

        public static MetadataProperty PROTEIN_QUANTIFICATION_UNIT = new MetadataProperty(MetadataElement.PROTEIN,
                                                                                          "quantification_unit");

        public static MetadataProperty PEPTIDE_QUANTIFICATION_UNIT = new MetadataProperty(MetadataElement.PEPTIDE,
                                                                                          "quantification_unit");

        public static MetadataProperty SMALL_MOLECULE_QUANTIFICATION_UNIT =
            new MetadataProperty(MetadataElement.SMALL_MOLECULE, "quantification_unit");

        public static MetadataProperty MS_RUN_FORMAT = new MetadataProperty(MetadataElement.MS_RUN, "format");
        public static MetadataProperty MS_RUN_LOCATION = new MetadataProperty(MetadataElement.MS_RUN, "location");
        public static MetadataProperty MS_RUN_ID_FORMAT = new MetadataProperty(MetadataElement.MS_RUN, "id_format");

        public static MetadataProperty MS_RUN_FRAGMENTATION_METHOD = new MetadataProperty(MetadataElement.MS_RUN,
                                                                                          "fragmentation_method");

        public static MetadataProperty SAMPLE_SPECIES = new MetadataProperty(MetadataElement.SAMPLE, "species");
        public static MetadataProperty SAMPLE_TISSUE = new MetadataProperty(MetadataElement.SAMPLE, "tissue");
        public static MetadataProperty SAMPLE_CELL_TYPE = new MetadataProperty(MetadataElement.SAMPLE, "cell_type");
        public static MetadataProperty SAMPLE_DISEASE = new MetadataProperty(MetadataElement.SAMPLE, "disease");
        public static MetadataProperty SAMPLE_DESCRIPTION = new MetadataProperty(MetadataElement.SAMPLE, "description");
        public static MetadataProperty SAMPLE_CUSTOM = new MetadataProperty(MetadataElement.SAMPLE, "custom");

        public static MetadataProperty ASSAY_QUANTIFICATION_REAGENT = new MetadataProperty(MetadataElement.ASSAY,
                                                                                           "quantification_reagent");

        public static MetadataProperty ASSAY_SAMPLE_REF = new MetadataProperty(MetadataElement.ASSAY, "sample_ref");
        public static MetadataProperty ASSAY_MS_RUN_REF = new MetadataProperty(MetadataElement.ASSAY, "ms_run_ref");

        public static MetadataProperty ASSAY_QUANTIFICATION_MOD_SITE =
            new MetadataProperty(MetadataSubElement.ASSAY_QUANTIFICATION_MOD, "site");

        public static MetadataProperty ASSAY_QUANTIFICATION_MOD_POSITION =
            new MetadataProperty(MetadataSubElement.ASSAY_QUANTIFICATION_MOD, "position");

        public static MetadataProperty STUDY_VARIABLE_ASSAY_REFS = new MetadataProperty(MetadataElement.STUDY_VARIABLE,
                                                                                        "assay_refs");

        public static MetadataProperty STUDY_VARIABLE_SAMPLE_REFS = new MetadataProperty(
            MetadataElement.STUDY_VARIABLE, "sample_refs");

        public static MetadataProperty STUDY_VARIABLE_DESCRIPTION = new MetadataProperty(
            MetadataElement.STUDY_VARIABLE, "description");

        public static MetadataProperty CV_LABEL = new MetadataProperty(MetadataElement.CV, "label");
        public static MetadataProperty CV_FULL_NAME = new MetadataProperty(MetadataElement.CV, "full_name");
        public static MetadataProperty CV_VERSION = new MetadataProperty(MetadataElement.CV, "version");
        public static MetadataProperty CV_URL = new MetadataProperty(MetadataElement.CV, "url");

        public static MetadataProperty COLUNIT_PROTEIN = new MetadataProperty(MetadataElement.COLUNIT, "protein");
        public static MetadataProperty COLUNIT_PEPTIDE = new MetadataProperty(MetadataElement.COLUNIT, "peptide");
        public static MetadataProperty COLUNIT_PSM = new MetadataProperty(MetadataElement.COLUNIT, "psm");

        public static MetadataProperty COLUNIT_SMALL_MOLECULE = new MetadataProperty(MetadataElement.COLUNIT,
                                                                                     "small_molecule");

        #region All

        public static List<MetadataProperty> All = new List<MetadataProperty>{
            MZTAB_VERSION,
            MZTAB_MODE,
            MZTAB_TYPE,
            MZTAB_ID,
            INSTRUMENT_NAME,
            INSTRUMENT_SOURCE,
            INSTRUMENT_ANALYZER,
            INSTRUMENT_DETECTOR,
            SOFTWARE_SETTING,
            CONTACT_NAME,
            CONTACT_AFFILIATION,
            CONTACT_EMAIL,
            FIXED_MOD_SITE,
            FIXED_MOD_POSITION,
            VARIABLE_MOD_SITE,
            VARIABLE_MOD_POSITION,
            PROTEIN_QUANTIFICATION_UNIT,
            PEPTIDE_QUANTIFICATION_UNIT,
            SMALL_MOLECULE_QUANTIFICATION_UNIT,
            MS_RUN_FORMAT,
            MS_RUN_LOCATION,
            MS_RUN_ID_FORMAT,
            MS_RUN_FRAGMENTATION_METHOD,
            SAMPLE_SPECIES,
            SAMPLE_TISSUE,
            SAMPLE_CELL_TYPE,
            SAMPLE_DISEASE,
            SAMPLE_DESCRIPTION,
            SAMPLE_CUSTOM,
            ASSAY_QUANTIFICATION_REAGENT,
            ASSAY_SAMPLE_REF,
            ASSAY_MS_RUN_REF,
            ASSAY_QUANTIFICATION_MOD_SITE,
            ASSAY_QUANTIFICATION_MOD_POSITION,
            STUDY_VARIABLE_ASSAY_REFS,
            STUDY_VARIABLE_SAMPLE_REFS,
            STUDY_VARIABLE_DESCRIPTION,
            CV_LABEL,
            CV_FULL_NAME,
            CV_VERSION,
            CV_URL,
            COLUNIT_PROTEIN,
            COLUNIT_PEPTIDE,
            COLUNIT_PSM,
            COLUNIT_SMALL_MOLECULE
        };

        #endregion

        private readonly string _name;
        private readonly MetadataElement _element;
        private readonly MetadataSubElement _subElement;

        private MetadataProperty(MetadataElement element, string name){
            _element = element;
            _subElement = null;
            _name = name;
        }

        private MetadataProperty(MetadataSubElement subElement, string name){
            _element = subElement.Element;
            _subElement = subElement;
            _name = name;
        }


        public MetadataElement Element { get { return _element; } }

        public MetadataSubElement SubElement { get { return _subElement; } }

        public string Name { get { return _name; } }

        public override string ToString(){
            return _name;
        }

        /**
         * element.getName_propertyName.
	     * @see uk.ac.ebi.pride.jmztab.model.MetadataElement#getName()
	     */

        public static MetadataProperty FindProperty(MetadataElement element, string propertyName){
            if (element == null || propertyName == null){
                return null;
            }
            MetadataProperty property;
            try{
                property =
                    All.First(
                        x =>
                        x.Element.Name.Equals(element.Name, StringComparison.CurrentCultureIgnoreCase) &&
                        x.Name.Equals(propertyName, StringComparison.CurrentCultureIgnoreCase));
            }
            catch (ArgumentNullException){
                property = null;
            }
            catch (InvalidOperationException){
                property = null;
            }
            return property;
        }

        /**
         * subElement.getName_propertyName
         * @see uk.ac.ebi.pride.jmztab.model.MetadataSubElement#getName()
         */

        public static MetadataProperty FindProperty(MetadataSubElement subElement, string propertyName){
            if (subElement == null || propertyName == null){
                return null;
            }

            MetadataProperty property = null;
            try{
                if (All.Any(x => x.SubElement != null)){
                    property =
                        All.FirstOrDefault(
                            x =>
                            x.SubElement != null && x.SubElement.Name.Equals(subElement.Name) &&
                            x.Name.Equals(propertyName, StringComparison.CurrentCultureIgnoreCase));
                }
            }
            catch (ArgumentNullException){
                property = null;
            }
            catch (InvalidOperationException){
                property = null;
            }
            return property;
        }

        public bool Match(MetadataProperty property){
            return property.Name.Equals(Name);
        }
    }
}