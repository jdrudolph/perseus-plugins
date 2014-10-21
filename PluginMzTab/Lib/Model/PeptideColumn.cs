using System;
using PluginMzTab.Plugin.Utils;

namespace PluginMzTab.Lib.Model{
    public class PeptideColumn : MZTabColumn{
        internal PeptideColumn(string name, Type columnType, bool optional, string order)
            : base(name, columnType, optional, order){}

        internal PeptideColumn(string name, Type columnType, bool optional, string order, Integer id)
            : base(name, columnType, optional, order, id) { }

        public static PeptideColumn SEQUENCE = new PeptideColumn("sequence", typeof (string), false, "01");
        public static PeptideColumn ACCESSION = new PeptideColumn("accession", typeof (string), false, "02");
        public static PeptideColumn UNIQUE = new PeptideColumn("unique", typeof (MZBoolean), false, "03");
        public static PeptideColumn DATABASE = new PeptideColumn("database", typeof (string), false, "04");

        public static PeptideColumn DATABASE_VERSION = new PeptideColumn("database_version", typeof (string), false,
                                                                         "05");

        public static PeptideColumn SEARCH_ENGINE = new PeptideColumn("search_engine", typeof (SplitList<Param>), false,
                                                                      "06");

        public static PeptideColumn BEST_SEARCH_ENGINE_SCORE = new PeptideColumn("best_search_engine_score",
                                                                                 typeof (double), true, "07");

        public static PeptideColumn SEARCH_ENGINE_SCORE = new PeptideColumn("search_engine_score",
                                                                            typeof (double), true, "08");

        public static PeptideColumn RELIABILITY = new PeptideColumn("reliability", typeof (Reliability), true, "09");

        public static PeptideColumn MODIFICATIONS = new PeptideColumn("modifications", typeof (SplitList<Modification>),
                                                                      false, "10");

        public static PeptideColumn RETENTION_TIME = new PeptideColumn("retention_time", typeof (SplitList<double>),
                                                                       false, "11");

        public static PeptideColumn RETENTION_TIME_WINDOW = new PeptideColumn("retention_time_window",
                                                                              typeof (SplitList<double>), false, "12");

        public static PeptideColumn CHARGE = new PeptideColumn("charge", typeof (Integer), false, "13");
        public static PeptideColumn MASS_TO_CHARGE = new PeptideColumn("mass_to_charge", typeof (double), false, "14");
        public static PeptideColumn URI = new PeptideColumn("uri", typeof (Uri), true, "15");

        public static PeptideColumn SPECTRA_REF = new PeptideColumn("spectra_ref", typeof (SplitList<SpectraRef>), false,
                                                                    "16");
    }
}