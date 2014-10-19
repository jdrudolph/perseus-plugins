using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using PerseusApi.Matrix;

namespace PluginMzTab.Plugin.Utils{
    public enum experimentalDesign{
        rawfile,
        variable
    }

    public enum summary{
        rawfile,
        labels0,
        labels1,
        labels2,
        labels3,
        multiplicity,
        instrument
    }

    public enum parameters{
        version,
        fixedMod,
        variableMod,
        protein_fdr,
        psm_fdr,
        site_fdr
    }

    public enum proteingroups{
        accession,
        description,
        id,
        peptide_IDs,
        msms_IDs,
        coverage,
        ratio_HL,
        ratio_HL_Norm,
        ratio_HL_Var,
        intensity,
        lfq_intensity
    }

    public enum peptides{
        id,
        proteinGroup_IDs,
        msms_IDs,
        charges,
        sequence,
        unique,
        pre,
        post,
        start,
        end
    }

    public enum msms{
        id,
        proteinGroup_IDs,
        peptide_ID,
        sequence,
        rawfile,
        scannumber,
        charge,
        mass,
        mz,
        score,
        retentiontime,
        retentiontimeWindow, //TODO for Jürgen  
        modifications,
        mod_sequence,
        mod_probabilities
    }

    public enum spectraRef{
        location,
        format,
        id_format,
        index,
        raw_file,
        scan_number,
        charge,
        fragmentation,
        mz
    }

    public enum databaseRef{
        file,
        source,
        version,
        identifier,
        specie,
        taxid
    }

    public class Keyword{
        public string Table { get; set; }
        public string Key { get; set; }
        public string Pattern { get; set; }
        public bool Mandatory { get; set; }
        public bool IsColumn { get; set; }

        protected Keyword(){}

        public override bool Equals(object obj){
            Keyword other = obj as Keyword;
            if (other == null){
                return false;
            }

            if (other.Key != Key){
                return false;
            }
            return true;
        }

        public override int GetHashCode(){
            return Key.GetHashCode();
        }

        public static Keyword CreateInstance(string line){
            Keyword result = new Keyword();
            try{
                string[] items = line.Split('\t');
                result.Table = items[0].Trim();
                result.Key = items[1].Trim();
                result.Pattern = items[2].Trim();
                result.Mandatory = Boolean.Parse(items[3].Trim());
                result.IsColumn = result.Key.GetType().Name != "parameters";
            }
            catch (Exception){
                result = null;
            }
            return result;
        }
    }

    public static class Constants{
        public static readonly IList<string> versions = new List<string>{"1.0 rc4"};

        public const int LabelHeight = 25;
        public const int TextBoxHeight = 25;
        public const int ComboBoxHeight = 25;
        public const int ListSelectorHeight = 90;
        public const int MultiListSelectorHeight = 400;
        public const int height = 130;
        public const int puffer = 12;

        private static IList<Keyword> _keywords;

        private static IList<Keyword> Keywords{
            get{
                if (_keywords == null){
                    _keywords = new List<Keyword>();
                    const string location = "conf/mztab/maxquant.columns.txt";
                    StreamReader reader = null;
                    try{
                        reader = new StreamReader(location);
                        string line;
                        while ((line = reader.ReadLine()) != null){
                            if (string.IsNullOrEmpty(line) || line.StartsWith("#")){
                                continue;
                            }
                            Keyword keyword = Keyword.CreateInstance(line);
                            if (keyword == null){
                                continue;
                            }
                            _keywords.Add(keyword);
                        }
                    }
                    catch (FileNotFoundException){
                        throw new Exception("Could not find file: " + location);
                    }
                    catch (IOException ex){
                        throw new Exception("Could not read file: " + location);
                    }
                    finally{
                        if (reader != null){
                            reader.Dispose();
                        }
                    }
                }
                return _keywords;
            }
        }

        public static Dictionary<int, string> GetKeyAndValues(Enum column, IList<string> names){
            if (names == null || Keywords == null){
                return null;
            }

            Keyword tmp = Keywords.FirstOrDefault(x => x.Key.Equals(column));

            if (tmp == null){
                return null;
            }

            string pattern = tmp.Pattern;

            if (pattern == null){
                return null;
            }

            Dictionary<int, string> result = new Dictionary<int, string>();

            Regex regex = new Regex(pattern);
            foreach (string col in names.Where(x => regex.IsMatch(x))){
                int key = names.IndexOf(col);
                string value = regex.Match(col).Groups[1].Value;
                result.Add(key, value);
            }

            return result;
        }

        public static int GetKeywordIndex(Enum column, IList<string> names){
            string name = GetKeywordName(column, names);
            if (name == null){
                return -1;
            }

            return names.IndexOf(name);
        }

        public static string GetKeywordName(Enum column, IList<string> names){
            if (names == null || Keywords == null){
                return null;
            }

            Keyword tmp = GetKeyword(column);

            if (tmp == null){
                return null;
            }

            Regex regex = new Regex(tmp.Pattern);
            return names.FirstOrDefault(regex.IsMatch);
        }

        public static Keyword GetKeyword(Enum key){
            return Keywords.FirstOrDefault(x => x.Table.Equals(key.GetType().Name) && x.Key.Equals(key.ToString()));
        }

        internal static string GetPattern(Enum key){
            Keyword keyword = GetKeyword(key);
            if (keyword == null){
                throw new NullReferenceException("No pattern specified for table " + key.GetType().Name + " column " +
                                                 key);
            }

            return keyword.Pattern;
        }

        public static string getRowName(Enum row, Dictionary<string, string> parameters){
            return GetKeywordName(row, new List<string>(parameters.Keys));
        }

        public static void ValidateColumnNames(IMatrixData matrix, ref string errString){
            try{
                if (matrix.Origin == null){
                    throw new Exception("Could not find matrix origin");
                }

                string filename = Path.GetFileNameWithoutExtension(matrix.Origin);
                IList<string> names = filename == "parameters"
                                          ? matrix.StringColumns[0]
                                          : MzTabMatrixUtils.GetAllColumnNames(matrix);

                ValidateColumnNames(filename, names);
            }
            catch (Exception e){
                string msg = "Validation of loaded matrix " + matrix.Name +
                             " was not successful. All mandatory columnnames are specified in the configuration file (/conf/mztab/maxquant.columns.txt). Error: " +
                             e.Message;
                errString = msg;
                throw new Exception(msg);
            }
        }

        public static void ValidateColumnNames(string file, Action<string> status, bool columns){
            try{
                string filename = Path.GetFileNameWithoutExtension(file);
                string[] names;
                try{
                    string[] lines = File.ReadAllLines(file);
                    if (columns){
                        names = lines.FirstOrDefault().Split('\t');
                    }
                    else{
                        names =
                            lines.Where(x => !string.IsNullOrEmpty(x) && x.Contains('\t') && !x.StartsWith("#"))
                                 .Select(x => x.Split('\t')[0])
                                 .ToArray();
                    }
                }
                catch (Exception){
                    throw new Exception("Could not read file " + file);
                }
                ValidateColumnNames(filename, names);
            }
            catch (Exception e){
                string msg = "ERROR: Validation of selected file " + file + " was not successful. " +
                             "All mandatory columnnames are specified in the configuration file (../conf/mztab/maxquant.columns.txt).\n" +
                             "Error: " + e.Message;
                status(msg);
                throw new Exception(msg);
            }
        }

        private static void ValidateColumnNames(string filename, IList<string> names){
            var collection =
                Keywords.Where(x => x.Table.Equals(filename, StringComparison.CurrentCultureIgnoreCase)).ToArray();
            if (!collection.Any()){
                return;
            }

            foreach (Keyword keyword in collection){
                if (keyword.Mandatory){
                    Regex regex = new Regex(keyword.Pattern);
                    if (!names.Any(regex.IsMatch)){
                        throw new Exception(
                            string.Format(
                                "Could not find a columname for the mandatory column {0} in table {1}.",
                                keyword.Key, keyword.Table));
                    }
                }
            }
        }
    }
}