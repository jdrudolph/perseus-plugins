using System;
using System.Globalization;

namespace PluginMzTab.Plugin.Utils {
    public class Integer : IFormattable{
        private readonly int _i;
        public Integer(int i){
            _i = i;
        }

        public override string ToString(){
            return _i.ToString(CultureInfo.InvariantCulture);
        }

        public string ToString(string format, IFormatProvider formatProvider){
            return _i.ToString(format, formatProvider);
        }

        public static Integer Parse(string value){
            try{
                return new Integer(Int32.Parse(value));
            }
            catch (Exception){
                return null;
            }
        }

        public int ToInt(){
            return _i;
        }
    }
}
