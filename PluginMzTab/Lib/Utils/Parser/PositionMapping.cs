using System.Collections.Generic;
using PluginMzTab.Lib.Model;

namespace PluginMzTab.Lib.Utils.Parser{
    /**
 * Create and maintain a couple of mappings between physical position and logical position.
 * Physical position: int, the position of mzTab file.
 * Logical position: string, the internal order of specification.
 */

    public class PositionMapping{
        // physicalPosition <--> logicalPosition
        private readonly Dictionary<int, string> mappings = new Dictionary<int, string>();

        public PositionMapping(MZTabColumnFactory factory, string headerLine) : this(factory, headerLine.Split('\t')){}

        public PositionMapping(MZTabColumnFactory factory, string[] headerList){
            for (int physicalPosition = 0; physicalPosition < headerList.Length; physicalPosition++){
                string header = headerList[physicalPosition];
                MZTabColumn column = factory.FindColumnByHeader(header);
                if (column != null){
                    put(physicalPosition, column.LogicPosition);
                }
            }
        }

        public void put(int physicalPosition, string logicalPosition){
            mappings.Add(physicalPosition, logicalPosition);
        }

        public bool isEmpty(){
            return mappings.Count == 0;
        }

        public int size(){
            return mappings.Count;
        }

        public bool containsKey(int key){
            return mappings.ContainsKey(key);
        }

        public IEnumerable<int> keySet(){
            return mappings.Keys;
        }

        public IEnumerable<string> values(){
            return mappings.Values;
        }

        public string get(int key){
            if (mappings.ContainsKey(key)){
                return mappings[key];
            }
            return null;
        }

        /**
         * Exchange key and value to "LogicalPosition, PhysicalPosition". This method used to simply the locate
         * operation by logical position to physical position.
         */

        public Dictionary<string, int> exchange(){
            Dictionary<string, int> exchangeMappings = new Dictionary<string, int>();

            foreach (int physicalPosition in mappings.Keys){
                if (mappings.ContainsKey(physicalPosition)){
                    string logicalPosition = mappings[physicalPosition];
                    exchangeMappings.Add(logicalPosition, physicalPosition);
                }
            }

            return exchangeMappings;
        }
    }
}