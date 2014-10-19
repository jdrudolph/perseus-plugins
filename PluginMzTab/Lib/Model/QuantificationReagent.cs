using System.Collections.Generic;

namespace PluginMzTab.Lib.Model{
    public class QuantificationReagent : CVParam{
        private QuantificationReagent(string accession, string name, string value)
            : base("PRIDE", accession, name, value){}

        public static QuantificationReagent Unlabeled_sample = new QuantificationReagent("PRIDE:0000434",
                                                                                         "Unlabeled sample", null);

        public static QuantificationReagent ICAT_reagent = new QuantificationReagent("PRIDE:0000344", "ICAT reagent",
                                                                                     null);

        public static QuantificationReagent ICAT_heavy_reagent = new QuantificationReagent("PRIDE:0000346",
                                                                                           "ICAT heavy reagent", null);

        public static QuantificationReagent ICAT_light_reagent = new QuantificationReagent("PRIDE:0000345",
                                                                                           "ICAT light reagent", null);

        public static QuantificationReagent ICPL_reagent = new QuantificationReagent("PRIDE:0000347", "ICPL reagent",
                                                                                     null);

        public static QuantificationReagent ICPL_0_reagent = new QuantificationReagent("PRIDE:0000348", "ICPL 0 reagent",
                                                                                       null);

        public static QuantificationReagent ICPL_10_reagent = new QuantificationReagent("PRIDE:0000351",
                                                                                        "ICPL 10 reagent", null);

        public static QuantificationReagent ICPL_4_reagent = new QuantificationReagent("PRIDE:0000349", "ICPL 4 reagent",
                                                                                       null);

        public static QuantificationReagent ICPL_6_reagent = new QuantificationReagent("PRIDE:0000350", "ICPL 6 reagent",
                                                                                       null);

        public static QuantificationReagent SILAC_reagent = new QuantificationReagent("PRIDE:0000328", "SILAC reagent",
                                                                                      null);

        public static QuantificationReagent SILAC_heavy = new QuantificationReagent("PRIDE:0000328", "SILAC heavy", null);
        public static QuantificationReagent SILAC_light = new QuantificationReagent("PRIDE:0000328", "SILAC light", null);

        public static QuantificationReagent SILAC_medium = new QuantificationReagent("PRIDE:0000328", "SILAC medium",
                                                                                     null);


        public static QuantificationReagent TMT_reagent = new QuantificationReagent("PRIDE:0000337", "TMT reagent", null);

        public static QuantificationReagent TMT_reagent_126 = new QuantificationReagent("PRIDE:0000285",
                                                                                        "TMT reagent 126", null);

        public static QuantificationReagent TMT_reagent_127 = new QuantificationReagent("PRIDE:0000286",
                                                                                        "TMT reagent 127", null);

        public static QuantificationReagent TMT_reagent_128 = new QuantificationReagent("PRIDE:0000287",
                                                                                        "TMT reagent 128", null);

        public static QuantificationReagent TMT_reagent_129 = new QuantificationReagent("PRIDE:0000288",
                                                                                        "TMT reagent 129", null);

        public static QuantificationReagent TMT_reagent_130 = new QuantificationReagent("PRIDE:0000289",
                                                                                        "TMT reagent 130", null);

        public static QuantificationReagent TMT_reagent_131 = new QuantificationReagent("PRIDE:0000290",
                                                                                        "TMT reagent 131", null);


        public static QuantificationReagent iTRAQ_reagent = new QuantificationReagent("PRIDE:0000329", "iTRAQ reagent",
                                                                                      null);

        public static QuantificationReagent iTRAQ_reagent_113 = new QuantificationReagent("PRIDE:0000264",
                                                                                          "iTRAQ reagent 113", null);

        public static QuantificationReagent iTRAQ_reagent_114 = new QuantificationReagent("PRIDE:0000114",
                                                                                          "iTRAQ reagent 114", null);

        public static QuantificationReagent iTRAQ_reagent_115 = new QuantificationReagent("PRIDE:0000115",
                                                                                          "iTRAQ reagent 115", null);

        public static QuantificationReagent iTRAQ_reagent_116 = new QuantificationReagent("PRIDE:0000116",
                                                                                          "iTRAQ reagent 116", null);

        public static QuantificationReagent iTRAQ_reagent_117 = new QuantificationReagent("PRIDE:0000117",
                                                                                          "iTRAQ reagent 117", null);

        public static QuantificationReagent iTRAQ_reagent_118 = new QuantificationReagent("PRIDE:0000256",
                                                                                          "iTRAQ reagent 118", null);

        public static QuantificationReagent iTRAQ_reagent_119 = new QuantificationReagent("PRIDE:0000266",
                                                                                          "iTRAQ reagent 119", null);

        public static QuantificationReagent iTRAQ_reagent_121 = new QuantificationReagent("PRIDE:0000267",
                                                                                          "iTRAQ reagent 121", null);

        public static HashSet<QuantificationReagent> reagentSet(){
            HashSet<QuantificationReagent> reagentSet = new HashSet<QuantificationReagent>();
            reagentSet.Add(Unlabeled_sample);

            reagentSet.Add(ICAT_reagent);
            reagentSet.Add(ICAT_light_reagent);
            reagentSet.Add(ICAT_heavy_reagent);

            reagentSet.Add(ICPL_reagent);
            reagentSet.Add(ICPL_0_reagent);
            reagentSet.Add(ICPL_10_reagent);
            reagentSet.Add(ICPL_4_reagent);
            reagentSet.Add(ICPL_6_reagent);

            reagentSet.Add(SILAC_reagent);
            reagentSet.Add(SILAC_heavy);
            reagentSet.Add(SILAC_light);
            reagentSet.Add(SILAC_medium);

            reagentSet.Add(TMT_reagent);
            reagentSet.Add(TMT_reagent_126);
            reagentSet.Add(TMT_reagent_127);
            reagentSet.Add(TMT_reagent_128);
            reagentSet.Add(TMT_reagent_129);
            reagentSet.Add(TMT_reagent_130);
            reagentSet.Add(TMT_reagent_131);

            reagentSet.Add(iTRAQ_reagent);
            reagentSet.Add(iTRAQ_reagent_113);
            reagentSet.Add(iTRAQ_reagent_114);
            reagentSet.Add(iTRAQ_reagent_115);
            reagentSet.Add(iTRAQ_reagent_116);
            reagentSet.Add(iTRAQ_reagent_117);
            reagentSet.Add(iTRAQ_reagent_118);
            reagentSet.Add(iTRAQ_reagent_119);
            reagentSet.Add(iTRAQ_reagent_121);

            return reagentSet;
        }

        public static bool isReagent(string accession){
            foreach (QuantificationReagent reagent in reagentSet()){
                if (reagent.Accession.Equals(accession)){
                    return true;
                }
            }

            return false;
        }
    }
}