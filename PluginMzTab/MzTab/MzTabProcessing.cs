using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using BaseLib.Param;
using BaseLib.Util;
using MzTabLibrary.model;
using MzTabLibrary.utils.errors;
using MzTabLibrary.utils.parser;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;
using PluginMzTab.extended;
using PluginMzTab.utils;

namespace PluginMzTab.mztab{
    public abstract class MzTabProcessing : IMatrixMultiProcessing{
        public int GetMaxThreads(Parameters parameters){
            return 1;
        }

        public abstract string Name { get; }
        public abstract float DisplayRank { get; }
        public abstract bool IsActive { get; }
        public abstract bool HasButton { get; }
        public abstract Bitmap DisplayImage { get; }
        public abstract string Description { get; }
        public string Heading { get { return "MzTab"; } }
        public abstract string HelpOutput { get; }
        public abstract string[] HelpSupplTables { get; }
        public abstract int NumSupplTables { get; }
        public abstract string[] HelpDocuments { get; }
        public abstract int NumDocuments { get; }
        public abstract int MinNumInput { get; }
        public abstract int MaxNumInput { get; }
		public string Url { get { return null; } }

        public abstract string[] Tables { get; }

        public string GetInputName(int index){
            if (index < 0 || index > Tables.Length){
                return null;
            }
            return Tables[index];
        }

        public abstract IMatrixData ProcessData(IMatrixData[] inputData, Parameters param, ref IMatrixData[] supplTables, ref IDocumentData[] documents, ProcessInfo processInfo);

        public abstract Parameters GetParameters(IMatrixData[] inputData, ref string errString);

        internal IMatrixData GetMatrixData(string name, IMatrixData[] inputData){
            int index = -1;
            for (int i = 0; i < MaxNumInput; i++){
                string temp = GetInputName(i);
                if (String.IsNullOrEmpty(temp)){
                    continue;
                }
                if (temp.Equals(name, StringComparison.CurrentCultureIgnoreCase)){
                    index = i;
                }
            }

            if (index == -1){
                return null;
            }

            return inputData[index];
        }

        internal string[] MatchProteinGroupIDs(IMatrixData baseMatrix, IMatrixData referenceMatrix, string proteinIdCol1, string proteinIdCol2, double[] ids, string[] database, string[] databaseVersion){
            string[] reference = referenceMatrix.StringColumns[referenceMatrix.StringColumnNames.IndexOf(proteinIdCol2)];

            string[] accessions = null;
            if (referenceMatrix.StringColumnNames.Contains("accession")){
                accessions = referenceMatrix.StringColumns[referenceMatrix.StringColumnNames.IndexOf("accession")];
            }
            else if (referenceMatrix.StringColumnNames.Contains("Majority protein IDs")){
                accessions =
                    referenceMatrix.StringColumns[referenceMatrix.StringColumnNames.IndexOf("Majority protein IDs")];
                accessions =
                    accessions.Select(
                        x => x.Contains(";") ? x.Substring(0, x.IndexOf(";", StringComparison.Ordinal)) : x).ToArray();
            }

            string[] databaseList = null;
            if (referenceMatrix.StringColumnNames.Contains(ProteinColumn.DATABASE.Name)){
                databaseList =
                    referenceMatrix.StringColumns[referenceMatrix.StringColumnNames.IndexOf(ProteinColumn.DATABASE.Name)
                        ];
            }
            string[] databaseVersionList = null;
            if (referenceMatrix.StringColumnNames.Contains(ProteinColumn.DATABASE_VERSION.Name)){
                databaseVersionList =
                    referenceMatrix.StringColumns[
                        referenceMatrix.StringColumnNames.IndexOf(ProteinColumn.DATABASE_VERSION.Name)];
            }


            SortedDictionary<string, int> lookup = new SortedDictionary<string, int>();
            if (accessions != null){
                for (int row = 0; row < reference.Length; row++){
                    if (!lookup.ContainsKey(reference[row])){
                        lookup.Add(reference[row], row);
                    }
                }
            }

            reference = baseMatrix.StringColumns[baseMatrix.StringColumnNames.IndexOf(proteinIdCol1)];
            string[] psmIDs = baseMatrix.StringColumns[baseMatrix.StringColumnNames.IndexOf("id")];
            string[] result = new string[baseMatrix.RowCount];
            for (int row = 0; row < reference.Length; row++){
                ids[row] = Double.Parse(psmIDs[row]);
                if (lookup.ContainsKey(reference[row])){
                    int r = lookup[reference[row]];
                    result[row] = accessions == null ? null : accessions[r];
                    database[row] = databaseList == null ? null : databaseList[r];
                    databaseVersion[row] = databaseVersionList == null ? null : databaseVersionList[r];
                }
                else{
                    result[row] = null;
                }
            }

            return result;
        }

        internal void AddAbundanceColumns(Metadata mtd, IMatrixData matrix, string section,
                                          SortedDictionary<int, IList<string>> msmslookup){
            if (mtd == null){
                return;
            }

            #region abundance studyvariable

            if (mtd.StudyVariableMap != null && mtd.StudyVariableMap.Count > 0){
                foreach (var value in mtd.StudyVariableMap.Values){
                    string header = string.Format("Intensity {0}", value.Description);
                    string name = string.Format("{0}_abundance_study_variable[{1}]", section, value.Id);

                    double[] temp = matrix.NumericColumnNames.Contains(header)
                                        ? matrix.NumericColumns[matrix.NumericColumnNames.IndexOf(header)]
                                        : null;

                    AddNumericColumn(matrix, name, temp);
                    AddNumericColumn(matrix, string.Format("{0}_abundance_stdev_study_variable[{1}]", section, value.Id), null);
                    AddNumericColumn(matrix, string.Format("{0}_abundance_std_error_study_variable[{1}]", section, value.Id), null);
                }
            }

            AddOptionalAbundanceColumns(mtd, matrix, proteingroups.ratio_HL, Constants.HeavyToLightRatio);
            AddOptionalAbundanceColumns(mtd, matrix, proteingroups.ratio_HL_Norm, Constants.HeavyToLightRatioNorm);
            AddOptionalAbundanceColumns(mtd, matrix, proteingroups.ratio_HL_Var, Constants.HeavyToLightRatioVar);
            AddOptionalAbundanceColumns(mtd, matrix, proteingroups.lfq_intensity, Constants.LfqIntensity);

            #endregion
        }

        private static void AddOptionalAbundanceColumns(Metadata mtd, IMatrixData matrix, Enum column, string col){
            string key = Constants.FirstColumnNameStartingWith(column, matrix.NumericColumnNames);
            if (key == null){
                return;
            }
            if (mtd.StudyVariableMap != null && mtd.StudyVariableMap.Count > 0){
                foreach (var value in mtd.StudyVariableMap.Values){
                    string name = value.Description;
                    string header =
                        matrix.NumericColumnNames.FirstOrDefault(x => x.Equals(string.Format("{0} {1}", key, name)));

                    if (header == null){
                        Regex regex = new Regex(@"[HL\s]*(.+)");
                        if (regex.IsMatch(name)){
                            name = regex.Match(name).Groups[1].Value;
                        }

                        header =
                            matrix.NumericColumnNames.FirstOrDefault(x => x.Equals(string.Format("{0} {1}", key, name)));
                    }

                    if (header != null){
                        double[] temp = matrix.NumericColumns[matrix.NumericColumnNames.IndexOf(header)];
                        AddNumericColumn(matrix, string.Format("opt_study_variable[{0}]_{1}", value.Id, col), temp);
                    }
                }
            }
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
                mdata.AddStringColumn(name, "", values ?? InitializeStringArray(mdata.RowCount));
            }
        }

        public static void ReplaceStringColumns(IMatrixData mdata, string newName, string oldName, string[] values){
            if (!newName.Equals(oldName)){
                RemoveStringColumn(mdata, oldName);
            }

            if (mdata.NumericColumnNames.Contains(newName) || mdata.MultiNumericColumnNames.Contains(newName)){
                return;
            }

            if (mdata.StringColumnNames.Contains(newName)){
                if (values != null){
                    mdata.StringColumns[mdata.StringColumnNames.IndexOf(newName)] = values;
                }
            }
            else{
                mdata.AddStringColumn(newName, "", values ?? InitializeStringArray(mdata.RowCount));
            }
        }

        public static void RemoveStringColumn(IMatrixData mdata, string column){
            if (column == null){
                return;
            }

            int index = mdata.StringColumnNames.IndexOf(column);
            if (index == -1){
                return;
            }

            mdata.StringColumnNames.RemoveAt(index);
            mdata.StringColumns.RemoveAt(index);

            GC.Collect();
        }

        public static string[] InitializeStringArray(int n, string value = ""){
            string[] result = new string[n];
            for (int i = 0; i < result.Length; i++){
                result[i] = value;
            }
            return result;
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
                mdata.AddNumericColumn(name, "", values ?? InitializeDoubleArray(mdata.RowCount));
            }
        }

        public static void RemoveNumericColumn(IMatrixData mdata, string column){
            if (column == null){
                return;
            }

            int index = mdata.NumericColumnNames.IndexOf(column);
            if (index == -1){
                return;
            }
            mdata.NumericColumnNames.RemoveAt(index);
            mdata.NumericColumns.RemoveAt(index);

            GC.Collect();
        }

        public static double[] InitializeDoubleArray(int n, double value = double.NaN){
            double[] result = new double[n];
            for (int i = 0; i < result.Length; i++){
                result[i] = value;
            }
            return result;
        }

        public static IList<string> GetAllColumnNames(IMatrixData mdata){
            if (mdata == null){
                return null;
            }
            List<string> columnNames = new List<string>(mdata.StringColumnNames);
            columnNames.AddRange(mdata.CategoryColumnNames);
            columnNames.AddRange(mdata.NumericColumnNames);
            columnNames.AddRange(mdata.ExpressionColumnNames);
            columnNames.AddRange(mdata.MultiNumericColumnNames);

            return columnNames;
        }

        public static Metadata ParseMetadata(IMatrixData mdata){
            MZTabErrorList errorList = null;
            return ParseMetadata(mdata, ref errorList);
        }

        public static Metadata ParseMetadata(IMatrixData mdata, ref MZTabErrorList errorList){
            MTDLineParser parser = new MTDLineParser();

            if (errorList == null){
                errorList = new MZTabErrorList();
            }

            IList<string> items = new List<string>();

            for (int row = 0; row < mdata.RowCount; row++){
                items.Clear();
                foreach (string[] column in mdata.StringColumns){
                    items.Add(column[row]);
                }
                string mtdLine = StringUtils.Concat("\t", items);
                if (mtdLine.StartsWith("#") || mtdLine.StartsWith("MTH")){
                    continue;
                }
                try{
                    parser.Parse(row, mtdLine, errorList);
                }
                catch (Exception e){
                    Console.Error.WriteLine(e.StackTrace);
                }
            }

            Metadata mtd = parser.Metadata;
            var temp = new SortedDictionary<int, MsRun>();
            foreach (var key in mtd.MsRunMap.Keys){
                MsRun value = mtd.MsRunMap[key];
                temp.Add(key, new MsRunImpl(value));
            }
            mtd.MsRunMap = temp;

            return mtd;
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
    }
}