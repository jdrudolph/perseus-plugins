using System;
using PluginMzTab.Plugin.Utils;

namespace PluginMzTab.Lib.Model{
    public class PSMColumn : MZTabColumn{
        internal PSMColumn(string name, Type columnType, bool optional, string order)
            : base(name, columnType, optional, order) { } 

        internal PSMColumn(string name, Type columnType, bool optional, string order, Integer id)
            : base(name, columnType, optional, order, id){}

        public static PSMColumn SEQUENCE = new PSMColumn("sequence", typeof (string), false, "01");
        public static PSMColumn PSM_ID = new PSMColumn("PSM_ID", typeof (Integer), false, "02");
        public static PSMColumn ACCESSION = new PSMColumn("accession", typeof (string), false, "03");
        public static PSMColumn UNIQUE = new PSMColumn("unique", typeof (MZBoolean), false, "04");
        public static PSMColumn DATABASE = new PSMColumn("database", typeof (string), false, "05");
        public static PSMColumn DATABASE_VERSION = new PSMColumn("database_version", typeof (string), false, "06");
        public static PSMColumn SEARCH_ENGINE = new PSMColumn("search_engine", typeof (SplitList<Param>), false, "07");

        public static PSMColumn SEARCH_ENGINE_SCORE = new PSMColumn("search_engine_score", typeof (double),
                                                                    true, "08");

        public static PSMColumn RELIABILITY = new PSMColumn("reliability", typeof (Reliability), true, "09");

        public static PSMColumn MODIFICATIONS = new PSMColumn("modifications", typeof (SplitList<Modification>), false,
                                                              "10");

        public static PSMColumn RETENTION_TIME = new PSMColumn("retention_time", typeof (SplitList<double>), false, "11");
        public static PSMColumn CHARGE = new PSMColumn("charge", typeof (Integer), false, "12");
        public static PSMColumn EXP_MASS_TO_CHARGE = new PSMColumn("exp_mass_to_charge", typeof (double), false, "13");
        public static PSMColumn CALC_MASS_TO_CHARGE = new PSMColumn("calc_mass_to_charge", typeof (double), false, "14");
        public static PSMColumn URI = new PSMColumn("uri", typeof (Uri), true, "15");
        public static PSMColumn SPECTRA_REF = new PSMColumn("spectra_ref", typeof (SplitList<SpectraRef>), false, "16");
        public static PSMColumn PRE = new PSMColumn("pre", typeof (string), false, "17");
        public static PSMColumn POST = new PSMColumn("post", typeof (string), false, "18");
        public static PSMColumn START = new PSMColumn("start", typeof (string), false, "19");
        public static PSMColumn END = new PSMColumn("end", typeof (string), false, "20");
    }
}