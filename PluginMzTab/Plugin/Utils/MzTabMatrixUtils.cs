using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using BaseLib.Mol;
using BaseLib.Param;
using BaseLib.Util;
using PerseusApi.Matrix;
using PluginMzTab.Lib.Model;
using PluginMzTab.Lib.Utils.Errors;
using PluginMzTab.Lib.Utils.Parser;
using PluginMzTab.Plugin.Extended;
using ComboBox = System.Windows.Forms.ComboBox;
using ListBox = System.Windows.Forms.ListBox;
using Modification = PluginMzTab.Lib.Model.Modification;

namespace PluginMzTab.Plugin.Utils{
    internal class MzTabMatrixUtils{
        public static int[] GetSelectedIndices(List<string> selection, ItemCollection values){
            if (selection == null || values == null){
                return null;
            }

            int[] result = new int[selection.Count];
            for (int i = 0; i < selection.Count; i++){
                if (selection[i] == null){
                    continue;
                }

                result[i] = GetSelectedIndex(selection[i], values);
            }

            return result;
        }

        public static int GetSelectedIndex(string selection, IList<string> values){
            return selection == null || values == null ? -1 : values.IndexOf(selection);
        }

        public static int GetSelectedIndex(string selection, ItemCollection values){
            return selection == null || values == null ? -1 : values.IndexOf(selection);
        }

        public static IList<string> Sort(IList<string> values){
            if (values == null){
                return null;
            }

            return values is List<string> ? SortList(values as List<string>)
                       : SortArray(values as string[]);
        }

        private static IList<string> SortArray(string[] values){
            if (values == null){
                return null;
            }
            Array.Sort(values);
            return values;
        }

        private static IList<string> SortList(List<string> values){
            if (values == null){
                return null;
            }
            values.Sort();
            return values;
        }

        public static int IndexOf(IList<Assay> assays, Assay assay){
            for (int i = 0; i < assays.Count; i++){
                if (Equals(assay, assays[i])){
                    return i;
                }
            }
            return -1;
        }

        private static bool Equals(SplitList<Lib.Model.Param> param1, SplitList<Lib.Model.Param> param2){
            if (param1 == null && param2 == null){
                return true;
            }

            if (param1 == null){
                return false;
            }

            if (param2 == null){
                return false;
            }

            if (param1.Count != param2.Count){
                return false;
            }

            param1.Sort();
            param2.Sort();

            return !param1.Where((t, i) => !t.Equals(param2[i])).Any();
        }

        private static bool Equals(Lib.Model.Param param1, Lib.Model.Param param2){
            if (param1 == null && param2 == null){
                return true;
            }

            if (param1 == null){
                return false;
            }

            if (param2 == null){
                return false;
            }

            if (!param1.Equals(param2)){
                return false;
            }

            return true;
        }

        public static bool Equals(MsRunImpl run1, MsRunImpl run2){
            if (run1 == null && run2 == null){
                return true;
            }

            if (run1 == null){
                return false;
            }

            if (run2 == null){
                return false;
            }

            if (!Equals(run1.Format, run2.Format)){
                return false;
            }

            if (!Equals(run1.IdFormat, run2.IdFormat)){
                return false;
            }

            if (!Equals(run1.FragmentationMethod, run2.FragmentationMethod)){
                return false;
            }

            return true;
        }

        public static bool Equals(Instrument instrument1, Instrument instrument2){
            if (instrument1 == null && instrument2 == null){
                return true;
            }

            if (instrument1 == null){
                return false;
            }

            if (instrument2 == null){
                return false;
            }


            if (!Equals(instrument1.Name, instrument2.Name)){
                return false;
            }

            if (!Equals(instrument1.Source, instrument2.Source)){
                return false;
            }

            if (!Equals(instrument1.Analyzer, instrument2.Analyzer)){
                return false;
            }

            if (!Equals(instrument1.Detector, instrument2.Detector)){
                return false;
            }

            return true;
        }

        public static bool Equals(Assay assay1, Assay assay2){
            if (assay1 == null && assay2 == null){
                return true;
            }

            if (assay1 == null){
                return false;
            }

            if (assay2 == null){
                return false;
            }


            if (!Equals(assay1.QuantificationReagent, assay2.QuantificationReagent)){
                return false;
            }

            return true;
        }

        public static bool Equals(Sample sample1, Sample sample2){
            if (sample1 == null && sample2 == null){
                return true;
            }

            if (sample1 == null){
                return false;
            }

            if (sample2 == null){
                return false;
            }

            if (!Equals(sample1.Description, sample2.Description)){
                return false;
            }

            if (!Equals(sample1.SpeciesList, sample2.SpeciesList)){
                return false;
            }

            if (!Equals(sample1.TissueList, sample2.TissueList)){
                return false;
            }

            if (!Equals(sample1.CellTypeList, sample2.CellTypeList)){
                return false;
            }

            if (!Equals(sample1.DiseaseList, sample2.DiseaseList)){
                return false;
            }

            return true;
        }

        public static IList<T> Unique<T>(IEnumerable<T> items){
            if (items == null){
                return null;
            }

            IList<T> unique = new List<T>();
            foreach (T item in items){
                if (unique.Count == 0){
                    unique.Add(item);
                    continue;
                }

                bool found = false;
                foreach (var r in unique){
                    if (r is MsRunImpl && item is MsRunImpl){
                        found = Equals(r as MsRunImpl, item as MsRunImpl);
                    }
                    if (r is Instrument && item is Instrument){
                        found = Equals(r as Instrument, item as Instrument);
                    }
                    if (r is Assay && item is Assay){
                        found = Equals(r as Assay, item as Assay);
                    }

                    if (found){
                        break;
                    }
                }

                if (!found){
                    unique.Add(item);
                }
            }
            return unique;
        }

        public static IList<string> GetAllColumnNames(IMatrixData mdata) {
            if (mdata == null) {
                return null;
            }
            List<string> columnNames = new List<string>(mdata.StringColumnNames);
            columnNames.AddRange(mdata.CategoryColumnNames);
            columnNames.AddRange(mdata.NumericColumnNames);
            columnNames.AddRange(mdata.ExpressionColumnNames);
            columnNames.AddRange(mdata.MultiNumericColumnNames);

            return columnNames;
        }

        public static double[] GetNumericValues(IMatrixData mdata, string columnname){
            if (columnname == null){
                return null;
            }
            if (mdata.NumericColumnNames.Contains(columnname)){
                int index = mdata.NumericColumnNames.IndexOf(columnname);
                return mdata.NumericColumns[index].ToArray();
            }

            return null;
        }

        public static void GetMatrixParam(IList<Parameter> list){
            IList<string> options = new List<string>{"Replace", "New"};

            SingleChoiceWithSubParams matrix = new SingleChoiceWithSubParams("Matrix"){
                ParamNameWidth = 50,
                TotalWidth = 400,
                Value = 0,
                Values = options,
                Help = null,
                SubParams = new Parameters[options.Count]
            };

            matrix.SubParams[0] = new Parameters(new Parameter[0]);
            matrix.SubParams[1] = new Parameters(new Parameter[]{new StringParam("Name")});
            list.Add(matrix);
        }

        public static int[] GetSelectedIndices(IList<string> selection, ListBox.ObjectCollection values){
            if (selection == null || values == null){
                return null;
            }
            int[] result = new int[selection.Count];
            for (int i = 0; i < selection.Count; i++){
                if (selection[i] == null){
                    continue;
                }

                result[i] = GetSelectedIndex(selection[i], values);
            }

            return result;
        }

        public static int[] GetSelectedIndices(IList<string> selection, ComboBox.ObjectCollection values){
            if (selection == null || values == null){
                return null;
            }
            int[] result = new int[selection.Count];
            for (int i = 0; i < selection.Count; i++){
                if (selection[i] == null){
                    continue;
                }

                result[i] = GetSelectedIndex(selection[i], values);
            }

            return result;
        }

        public static int[] GetSelectedIndices(IList<string> selection, IList<string> values){
            if (selection == null || values == null){
                return null;
            }

            int[] result = new int[selection.Count];
            for (int i = 0; i < selection.Count; i++){
                if (selection[i] == null){
                    continue;
                }

                result[i] = GetSelectedIndex(selection[i], values);
            }

            return result;
        }

        public static int GetSelectedIndex(string selection, ListBox.ObjectCollection values){
            if (values == null){
                return -1;
            }
            return selection == null ? -1 : values.IndexOf(selection);
        }

        public static int GetSelectedIndex(string selection, ComboBox.ObjectCollection values){
            if (values == null){
                return -1;
            }
            return selection == null ? -1 : values.IndexOf(selection);
        }

        public static object[] ConvertToObjectArray(IList<string> values, bool sort = true){
            if (values == null){
                return null;
            }

            object[] result = new object[values.Count];
            for (int i = 0; i < result.Length; i++){
                result[i] = values[i];
            }

            if (sort){
                Array.Sort(result);
            }

            return result;
        }

        public static void AddStringColumn(IMatrixData mdata, string name, string[] values){
            if (mdata.NumericColumnNames.Contains(name) || mdata.MultiNumericColumnNames.Contains(name)){
                return;
            }

            if (mdata.StringColumnNames.Contains(name)){
                if (values != null){
                    mdata.StringColumns[mdata.StringColumnNames.IndexOf(name)] = values;
                }
            }
            else{
                mdata.AddStringColumn(name, "", values ?? new string[mdata.RowCount]);
            }
        }

        public static void AddNumericColumn(IMatrixData mdata, string name, double[] values){
            if (mdata.StringColumnNames.Contains(name) || mdata.MultiNumericColumnNames.Contains(name)){
                return;
            }

            if (mdata.NumericColumnNames.Contains(name)){
                if (values != null){
                    mdata.NumericColumns[mdata.NumericColumnNames.IndexOf(name)] = values;
                }
            }
            else{
                mdata.AddNumericColumn(name, "", values ?? new double[mdata.RowCount]);
            }
        }

        public static Modification ConvertModificationToMzTab(BaseLib.Mol.Modification modification, Section section){
            Modification.ModificationType type = Modification.ModificationType.UNIMOD;
            string accession = modification.Unimod;
            if (accession == null){
                type = Modification.ModificationType.UNKNOWN;
                accession = modification.Name;
            }

            return new Modification(section, type, accession);
        }

        public static Metadata ParseMetadata(IMatrixData mdata) {
            MTDLineParser parser = new MTDLineParser();

            MZTabErrorList errorList = new MZTabErrorList();
            IList<string> items = new List<string>();

            for (int row = 0; row < mdata.RowCount; row++) {
                items.Clear();
                for (int col = 0; col < mdata.StringColumns.Count; col++) {
                    items.Add(mdata.StringColumns[col][row]);
                }
                string mtdLine = StringUtils.Concat("\t", items);
                try {
                    parser.Parse(row, mtdLine, errorList);
                } catch (Exception e) {
                    Console.Error.WriteLine(e.StackTrace);
                }
            }

            return parser.Metadata;
        }

        public static Dictionary<string, string> ConvertToParamDict(IMatrixData parameters) {
            Dictionary<string, string> paramDict = new Dictionary<string, string>();
            if (parameters != null) {
                int col1 = parameters.StringColumnNames.IndexOf("Parameter");
                int col2 = parameters.StringColumnNames.IndexOf("Value");
                for (int row = 0; row < parameters.RowCount; row++) {
                    string key = parameters.StringColumns[col1][row];
                    string value = parameters.StringColumns[col2][row];
                    if (paramDict.ContainsKey(key)) {
                        Console.Error.WriteLine("Key {0} is already in the Dictionary.", key);
                        continue;
                    }
                    paramDict.Add(key, value);
                }
            }
            return paramDict;
        }

        public static string[] ConvertToStringArray(string[][] categoricalColumn) {
            return categoricalColumn != null && categoricalColumn.Length > 0
                       ? categoricalColumn.Select(x => StringUtils.Concat(";", x)).ToArray()
                       : null;
        }

        public static IList<AssayQuantificationMod> GetQuantificationMod(string[] labels, int i, Assay assay) {
            if (labels == null) {
                return null;
            }
            IList<AssayQuantificationMod> list = new List<AssayQuantificationMod>();

            int n = 1;
            foreach (var label in labels[i].Split(';')) {
                if (!Tables.Modifications.ContainsKey(label)) {
                    continue;
                }
                BaseLib.Mol.Modification mod = Tables.Modifications[label];
                list.Add(new AssayQuantificationMod(assay, n++) {
                    Param = new UserParam(mod.Name, null),
                    Position = mod.Position.ToString(),
                    Site = StringUtils.Concat(";", mod.GetSiteArray())
                });
            }

            return list;
        }
    }
}