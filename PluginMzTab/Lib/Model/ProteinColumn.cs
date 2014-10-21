using System;
using PluginMzTab.Plugin.Utils;

namespace PluginMzTab.Lib.Model{
    public class ProteinColumn : MZTabColumn{
        internal ProteinColumn(string name, Type columnType, bool optional, string order)
            : base(name, columnType, optional, order) { }

        internal ProteinColumn(string name, Type columnType, bool optional, string order, Integer id)
            : base(name, columnType, optional, order, id){}

        public static readonly ProteinColumn ACCESSION = new ProteinColumn("accession", typeof (string), false, "01");
        public static readonly ProteinColumn DESCRIPTION = new ProteinColumn("description", typeof (string), false, "02");
        public static readonly ProteinColumn TAXID = new ProteinColumn("taxid", typeof (Integer), false, "03");
        public static readonly ProteinColumn SPECIES = new ProteinColumn("species", typeof (string), false, "04");
        public static readonly ProteinColumn DATABASE = new ProteinColumn("database", typeof (string), false, "05");

        public static readonly ProteinColumn DATABASE_VERSION = new ProteinColumn("database_version", typeof (string),
                                                                                  false, "06");

        public static readonly ProteinColumn SEARCH_ENGINE = new ProteinColumn("search_engine",
                                                                               typeof (SplitList<Param>), false, "07");

        public static readonly ProteinColumn BEST_SEARCH_ENGINE_SCORE = new ProteinColumn("best_search_engine_score",
                                                                                          typeof (double),
                                                                                          true, "08");

        public static readonly ProteinColumn SEARCH_ENGINE_SCORE = new ProteinColumn("search_engine_score",
                                                                                     typeof (double), true,
                                                                                     "09");

        public static readonly ProteinColumn RELIABILITY = new ProteinColumn("reliability", typeof (Reliability), true,
                                                                             "10");

        public static readonly ProteinColumn NUM_PSMS = new ProteinColumn("num_psms", typeof (Integer), true, "11");

        public static readonly ProteinColumn NUM_PEPTIDES_DISTINCT = new ProteinColumn("num_peptides_distinct",
                                                                                       typeof(Integer), true, "12");

        public static readonly ProteinColumn NUM_PEPTIDES_UNIQUE = new ProteinColumn("num_peptides_unique", typeof (int),
                                                                                     true, "13");

        public static readonly ProteinColumn AMBIGUITY_MEMBERS = new ProteinColumn("ambiguity_members",
                                                                                   typeof (SplitList<string>), false,
                                                                                   "14");

        public static readonly ProteinColumn MODIFICATIONS = new ProteinColumn("modifications",
                                                                               typeof (SplitList<Modification>), false,
                                                                               "15");

        public static readonly ProteinColumn URI = new ProteinColumn("uri", typeof (Uri), true, "16");

        public static readonly ProteinColumn GO_TERMS = new ProteinColumn("go_terms", typeof (SplitList<Param>), true,
                                                                          "17");

        public static readonly ProteinColumn PROTEIN_COVERAGE = new ProteinColumn("protein_coverage", typeof (double),
                                                                                  false, "18");
    }
}