using System;
using System.IO;
using PluginMzTab.Lib.Model;
using PluginMzTab.Lib.Utils.Errors;

namespace PluginMzTab.Lib.Utils{
/**
 * User: Qingwei
 * Date: 29/01/13
 */

    public class MZTabProperties{
        //private static Logger logger = Logger.getLogger(MZTabProperties.class);

        private static Properties properties = SetProperties();

        private static Properties SetProperties(){
            const string mzTabProperties = "conf/mztab/mztab.properties";
            const string formatProperties = "conf/mztab/mztab_format_error.properties";
            const string logicalProperties = "conf/mztab/mztab_logical_error.properties";
            const string crosscheckProperties = "conf/mztab/mztab_crosscheck_error.properties";
            try{
                properties = new Properties();
                StreamReader reader = new StreamReader(mzTabProperties);
                properties.load(reader);
                reader.Close();

                reader = new StreamReader(formatProperties);
                properties.load(reader);
                reader.Close();

                reader = new StreamReader(logicalProperties);
                properties.load(reader);
                reader.Close();

                reader = new StreamReader(crosscheckProperties);
                properties.load(reader);
                reader.Close();

                return properties;
            }
            catch (FileNotFoundException e){
                Console.Error.WriteLine(e.Message);
            }
            catch (IOException e){
                Console.Error.WriteLine(e.Message);
            }

            return null;
        }

        public static string getProperty(string key){
            if (properties == null){
                properties = SetProperties();
            }
            if (properties.Property.ContainsKey(key)){
                return properties.Property[key];
            }
            return null;
        }

        public static readonly string MZTabExceptionMessage =
            "There exists errors in the metadata section or protein/peptide/small_molecule header section! Validation will stop, and ignore data table check!" +
            MZTabConstants.NEW_LINE;

        public static readonly string MZTabErrorOverflowExceptionMessage = "System error queue overflow!" +
                                                                           MZTabConstants.NEW_LINE;

        public static readonly string VERSION = getProperty("mztab.version") ?? "1.0 rc4";
        public static readonly string ENCODE = getProperty("mztab.encode") ?? "UTF-8";

        public static readonly int MAX_ERROR_COUNT = getProperty("mztab.max_error_count") == null
                                                         ? 200
                                                         : int.Parse(getProperty("mztab.max_error_count"));

        public static readonly Level LEVEL = getProperty("mztab.level") == null
                                                 ? Level.Warn
                                                 : MZTabErrorType.FindLevel(getProperty("mztab.level"));

        public static readonly bool CVPARAM_CHECK = getProperty("mztab.cvparam_webservice") != null &&
                                                    bool.Parse(getProperty("mztab.cvparam_webservice"));

//    public readonly static  boolean BUFFERED = bool.Parse(getProperty("mztab.buffered"));
    }
}