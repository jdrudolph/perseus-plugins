using System;

namespace PluginMzTab.Lib.Model{
/**
 * This must be supplied by the resource and has to be one of the following getStableColumns:
 * 1: high reliability
 * 2: medium reliability
 * 3: poor reliability
 */

    public class Reliability{
        public static Reliability High = new Reliability("high reliability", 1);
        public static Reliability Medium = new Reliability("medium reliability", 2);
        public static Reliability Poor = new Reliability("poor reliability", 3);

        private string name;
        private int level;

        private Reliability(string name, int level){
            this.name = name;
            this.level = level;
        }

        public int Level { get { return level; } }

        public string Name { get { return name; } }

        public override string ToString(){
            return "" + Level;
        }

        /**
     * @param reliabilityLabel 1, 2, 3
     */

        public static Reliability findReliability(string reliabilityLabel){
            reliabilityLabel = reliabilityLabel.Trim();
            try{
                int id = int.Parse(reliabilityLabel);
                Reliability reliability = null;
                switch (id){
                    case 1:
                        reliability = High;
                        break;
                    case 2:
                        reliability = Medium;
                        break;
                    case 3:
                        reliability = Poor;
                        break;
                }
                return reliability;
            }
            catch (Exception){
                return null;
            }
        }
    }
}