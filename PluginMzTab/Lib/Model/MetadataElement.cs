using System;
using System.Collections.Generic;
using System.Linq;

namespace PluginMzTab.Lib.Model{
    public class MetadataElement{
        public static MetadataElement MZTAB = new MetadataElement("mzTab");
        public static MetadataElement TITLE = new MetadataElement("title");
        public static MetadataElement DESCRIPTION = new MetadataElement("description");
        public static MetadataElement INSTRUMENT = new MetadataElement("instrument");
        public static MetadataElement SOFTWARE = new MetadataElement("software");
        public static MetadataElement FALSE_DISCOVERY_RATE = new MetadataElement("false_discovery_rate");
        public static MetadataElement PUBLICATION = new MetadataElement("publication");
        public static MetadataElement CONTACT = new MetadataElement("contact");
        public static MetadataElement URI = new MetadataElement("uri");
        public static MetadataElement FIXED_MOD = new MetadataElement("fixed_mod");
        public static MetadataElement VARIABLE_MOD = new MetadataElement("variable_mod");
        public static MetadataElement QUANTIFICATION_METHOD = new MetadataElement("quantification_method");
        public static MetadataElement PROTEIN = new MetadataElement("protein");
        public static MetadataElement PEPTIDE = new MetadataElement("peptide");
        public static MetadataElement SMALL_MOLECULE = new MetadataElement("small_molecule");
        public static MetadataElement MS_RUN = new MetadataElement("ms_run");
        public static MetadataElement CUSTOM = new MetadataElement("custom");
        public static MetadataElement SAMPLE = new MetadataElement("sample");
        public static MetadataElement SAMPLE_PROCESSING = new MetadataElement("sample_processing");
        public static MetadataElement ASSAY = new MetadataElement("assay");
        public static MetadataElement STUDY_VARIABLE = new MetadataElement("study_variable");
        public static MetadataElement CV = new MetadataElement("cv");
        public static MetadataElement COLUNIT = new MetadataElement("colunit");

        public static List<MetadataElement> All = new List<MetadataElement>{
            MZTAB,
            TITLE,
            DESCRIPTION,
            SAMPLE_PROCESSING,
            INSTRUMENT,
            SOFTWARE,
            FALSE_DISCOVERY_RATE,
            PUBLICATION,
            CONTACT,
            URI,
            FIXED_MOD,
            VARIABLE_MOD,
            QUANTIFICATION_METHOD,
            PROTEIN,
            PEPTIDE,
            SMALL_MOLECULE,
            MS_RUN,
            CUSTOM,
            SAMPLE,
            ASSAY,
            STUDY_VARIABLE,
            CV,
            COLUNIT
        };

        private readonly string _name;

        private MetadataElement(string name){
            _name = name;
        }

        public string Name { get { return _name; } }


        public override string ToString(){
            return _name;
        }

        public static MetadataElement findElement(string name){
            if (name == null){
                return null;
            }

            MetadataElement element;
            try{
                element = All.First(x => x.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
            }
            catch (ArgumentNullException){
                element = null;
            }
            catch (InvalidOperationException){
                element = null;
            }

            return element;
        }

        public bool Match(MetadataElement element){
            return element.Name.Equals(Name);
        }
    }
}