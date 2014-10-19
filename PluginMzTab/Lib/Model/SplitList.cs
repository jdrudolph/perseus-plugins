using System.Collections.Generic;
using System.Globalization;
using BaseLib.Util;

/**
 * This is list which each item split by a split char.
 */

namespace PluginMzTab.Lib.Model{
    public class SplitList<E> : List<E>{
        private char separator;

        public SplitList(char separator){
            this.separator = separator;
        }

        //_TODO
        public SplitList() : this(MZTabConstants.BAR){}

        public char Separator { set { separator = value; } }

        public override string ToString(){
            if (Count == 0){
                return "";
            }
            return StringUtils.Concat(separator.ToString(CultureInfo.InvariantCulture), this);
        }
    }
}