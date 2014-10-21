using System;
using PluginMzTab.Plugin.Utils;

namespace PluginMzTab.Lib.Model{
    public class SmallMoleculeColumn : MZTabColumn{
        internal SmallMoleculeColumn(string name, Type columnType, bool optional, string order)
            : base(name, columnType, optional, order){}

        internal SmallMoleculeColumn(string name, Type columnType, bool optional, string order,Integer id)
            : base(name, columnType, optional, order, id) { }

        public static SmallMoleculeColumn IDENTIFIER = new SmallMoleculeColumn("identifier", typeof (SplitList<>), false,
                                                                               "01");

        public static SmallMoleculeColumn CHEMICAL_FORMULA = new SmallMoleculeColumn("chemical_formula", typeof (string),
                                                                                     false, "02");

        public static SmallMoleculeColumn SMILES = new SmallMoleculeColumn("smiles", typeof (SplitList<>), false, "03");

        public static SmallMoleculeColumn INCHI_KEY = new SmallMoleculeColumn("inchi_key", typeof (SplitList<>), false,
                                                                              "04");

        public static SmallMoleculeColumn DESCRIPTION = new SmallMoleculeColumn("description", typeof (string), false,
                                                                                "05");

        public static SmallMoleculeColumn EXP_MASS_TO_CHARGE = new SmallMoleculeColumn("exp_mass_to_charge",
                                                                                       typeof (double), false, "06");

        public static SmallMoleculeColumn CALC_MASS_TO_CHARGE = new SmallMoleculeColumn("calc_mass_to_charge",
                                                                                        typeof (double), false, "07");

        public static SmallMoleculeColumn CHARGE = new SmallMoleculeColumn("charge", typeof (int), false, "08");

        public static SmallMoleculeColumn RETENTION_TIME = new SmallMoleculeColumn("retention_time",
                                                                                   typeof (SplitList<double>), false,
                                                                                   "09");

        public static SmallMoleculeColumn TAXID = new SmallMoleculeColumn("taxid", typeof (int), false, "10");
        public static SmallMoleculeColumn SPECIES = new SmallMoleculeColumn("species", typeof (string), false, "11");
        public static SmallMoleculeColumn DATABASE = new SmallMoleculeColumn("database", typeof (string), false, "12");

        public static SmallMoleculeColumn DATABASE_VERSION = new SmallMoleculeColumn("database_version", typeof (string),
                                                                                     false, "13");

        public static SmallMoleculeColumn RELIABILITY = new SmallMoleculeColumn("reliability", typeof (Reliability),
                                                                                true, "14");

        public static SmallMoleculeColumn URI = new SmallMoleculeColumn("uri", typeof (Uri), true, "15");

        public static SmallMoleculeColumn SPECTRA_REF = new SmallMoleculeColumn("spectra_ref",
                                                                                typeof (SplitList<SpectraRef>), false,
                                                                                "16");

        public static SmallMoleculeColumn SEARCH_ENGINE = new SmallMoleculeColumn("search_engine",
                                                                                  typeof (SplitList<Param>), false, "17");

        public static SmallMoleculeColumn BEST_SEARCH_ENGINE_SCORE = new SmallMoleculeColumn(
            "best_search_engine_score", typeof (SplitList<Param>), false, "18");

        public static SmallMoleculeColumn SEARCH_ENGINE_SCORE = new SmallMoleculeColumn("search_engine_score",
                                                                                        typeof (SplitList<Param>), true,
                                                                                        "19");

        public static SmallMoleculeColumn MODIFICATIONS = new SmallMoleculeColumn("modifications",
                                                                                  typeof (SplitList<Modification>),
                                                                                  false, "20");
    }
}