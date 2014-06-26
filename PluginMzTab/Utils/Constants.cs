using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PluginMzTab.utils{
    public enum experiment
    {
        rawfile,
        variable
    }

    public enum summary
    {
        rawfile,
        labels0,
        labels1,
        labels2,
        labels3,
        multiplicity,
        instrument
    }

    public enum parameters
    {
        version,
        fixedMod,
        variableMod,
        protein_fdr,
        psm_fdr,
        site_fdr
    }

    public enum proteingroups
    {
        accession,
        description,
        id,
        peptide_IDs,
        msms_IDs,
        coverage,
        ratio_HL,
        ratio_HL_Norm,
        ratio_HL_Var,
        lfq_intensity
    }

    public enum peptides
    {
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

    public enum msms
    {
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
        retentiontimeWindow,//TODO for Jürgen  
        modifications,
        mod_sequence,
        mod_probabilities
    }

    internal static class Constants{
        public static readonly IList<string> versions = new List<string>{"1.0 rc4"};
        
        public const int LabelHeight = 25;
        public const int TextBoxHeight = 25;
        public const int ComboBoxHeight = 25;
        public const int ListSelectorHeight = 90;
        public const int MultiListSelectorHeight = 400;
        public const int height = 130;
        public const int puffer = 12;

        public const string nullValue = "null";

        private static Dictionary<string, IList<string>> properties;

        private static Dictionary<string, IList<string>> Properties{
            get{
                if (properties == null){
                    try{
                        properties = new Dictionary<string, IList<string>>();
                        StreamReader reader = null;
                        try{
                            reader = new StreamReader("conf/mztab/maxquant.columns.txt");
                            string line;
                            while ((line = reader.ReadLine()) != null){
                                if (string.IsNullOrEmpty(line) || line.StartsWith("#")){
                                    continue;
                                }

                                string[] items = line.Split('=');
                                if (items.Length == 2){
                                    string key = items[0].Trim();
                                    if (!properties.ContainsKey(key)){
                                        properties.Add(key, new List<string>());
                                    }
                                    properties[key].Add(items[1].Trim());
                                }
                            }
                        }
                        catch (Exception){}
                        finally{
                            if (reader != null){
                                reader.Dispose();
                            }
                        }
                    }
                    catch (Exception e){
                        Console.Error.WriteLine(e.Message);
                    }
                }
                return properties;
            }
        }

        public static string HeavyToLightRatio{ get { return "ratio_heavy_to_light"; } }

        public static string HeavyToLightRatioNorm { get { return "ratio_heavy_to_light_normalized"; } }

        public static string HeavyToLightRatioVar { get { return "ratio_heavy_to_light_variability"; } }

        public static string LfqIntensity { get { return "lfq_intensity"; } }

        public static int GetColumnIndex(Enum column, IList<string> names){
            string name = GetColumnName(column, names);
            if (name == null || !names.Contains(name)){
                return -1;
            }

            return names.IndexOf(name);
        }

        public static string GetColumnName(Enum column, IList<string> names){
            string result = null;

            if (names == null || Properties == null){
                return null;
            }

            string key = column.GetType().Name + "_" + column;
            if (Properties.ContainsKey(key))
            {
                foreach (var name in properties[key])
                {
                    if (names.Contains(name)){
                        result = name;
                        break;
                    }
                }
            }

            return result;
        }

        public static string FirstColumnNameStartingWith(Enum column, IList<string> names)
        {
            string result = null;

            if (names == null || Properties == null)
            {
                return null;
            }

            string key = column.GetType().Name + "_" + column;
            if (Properties.ContainsKey(key))
            {
                foreach (var value in properties[key])
                {
                    if (names.Any(x => x.StartsWith(value)))
                    {
                        result = value;
                        break;
                    }
                }
            }

            return result;
        }

        public static IList<string> GetAll(Enum column){
            if (Properties == null)
            {
                return null;
            }

            string key = column.GetType().Name + "_" + column;
            if (Properties.ContainsKey(key)){
                return Properties[key];
            }
            return null;
        }
    }
}