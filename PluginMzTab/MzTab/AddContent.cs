using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using BaseLib.Mol;
using BaseLib.Param;
using BaseLib.Util;
using MzTabLibrary.model;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;
using PerseusPluginLib.Rearrange;
using PluginMzTab.extended;
using PluginMzTab.utils;
using Modification = MzTabLibrary.model.Modification;

namespace PluginMzTab.mztab{
    public class AddContent : MzTabProcessing{
        public override string Name { get { return "Add content"; } }
        public override float DisplayRank { get { return 2; } }
        public override bool IsActive { get { return true; } }
        public override bool HasButton { get { return false; } }
        public override Bitmap DisplayImage { get { return null; } }
        public override string Description { get { return "Adds MzTab specific columns to the protein groups txt"; } }
        public override int MinNumInput { get { return 4; } }
        public override int MaxNumInput { get { return 4; } }
        public override string HelpOutput { get { return null; } }
        public override string[] HelpSupplTables { get { return ArrayUtils.SubArray(Tables, 1, Tables.Length); } }
        public override int NumSupplTables { get { return HelpSupplTables.Length; } }
        public override string[] HelpDocuments { get { return null; } }
        public override int NumDocuments { get { return 0; } }
        public override string[] Tables { get { return new[]{Matrix.MetadataSection, Matrix.ProteinGroups, Matrix.Peptides, Matrix.MsMs}; } }

        public override IMatrixData ProcessData(IMatrixData[] inputData, Parameters param, ref IMatrixData[] supplTables,
                                                ref IDocumentData[] documents, ProcessInfo processInfo){
            const int steps = 20;
            int n = 0;
            int col1, col2, col3;
            #region Clone matrix
            processInfo.Progress(n++*100/steps);
            processInfo.Status("Clone matrix");
            IMatrixData proteinGroups = GetMatrixData(Matrix.ProteinGroups, inputData);
            IMatrixData peptides = GetMatrixData(Matrix.Peptides, inputData);
            ReplaceCharacter(peptides, Constants.GetColumnName(utils.peptides.charges, peptides.StringColumnNames), ",",
                             ";");
            IMatrixData msms = GetMatrixData(Matrix.MsMs, inputData);
            IMatrixData metadata = GetMatrixData(Matrix.MetadataSection, inputData);
            Metadata mtd = ParseMetadata(metadata);
            IMatrixData metadataMatrix = (IMatrixData) metadata.Clone();
            IMatrixData proteinMatrix = (IMatrixData) proteinGroups.Clone();
            IMatrixData peptideMatrix = (IMatrixData) peptides.Clone();
            ExpandMatrix(peptideMatrix, Constants.GetColumnName(utils.peptides.charges, peptideMatrix.StringColumnNames));
            ExpandMatrix(peptideMatrix,
                         Constants.GetColumnName(utils.peptides.proteinGroup_IDs, peptideMatrix.StringColumnNames));
            IMatrixData msmsMatrix = (IMatrixData) msms.Clone();
            ExpandMatrix(msmsMatrix, Constants.GetColumnName(utils.msms.proteinGroup_IDs, msmsMatrix.StringColumnNames));
            #endregion
            #region Create X-References
            processInfo.Progress(n++*100/steps);
            processInfo.Status("Create X-References");
            IList<string> nameList = new List<string>();
            IList<string> keyList = new List<string>();
            IList<IMatrixData> matrixList = new[]{proteinMatrix, peptideMatrix, msmsMatrix};
            string[][] columnNames = new string[param.GroupCount][];
            for (int i = 0; i < param.GroupCount; i++){
                columnNames[i] = new string[param.GroupCount];
                ParameterGroup group = param.GetGroup(i);
                SingleChoiceParam single = group.GetParam("ID") as SingleChoiceParam;
                if (single != null){
                    nameList.Add(group.Name);
                    keyList.Add(single.SelectedValue);
                    for (int j = 0; j < param.GroupCount; j++){
                        if (i == j){
                            columnNames[i][j] = single.SelectedValue;
                        } else{
                            SingleChoiceParam refParam =
                                param.GetGroup(j).GetParam(string.Format("{0} Reference", group.Name)) as
                                SingleChoiceParam;
                            if (refParam != null){
                                columnNames[i][j] = refParam.SelectedValue;
                            }
                        }
                    }
                }
            }
            RefMap map = new RefMap(nameList, keyList, matrixList, columnNames);
            /*ReferenceMap protein = new ReferenceMap();
            protein.ProteinGroups = ReferenceMap.GetMapping(Matrix.ProteinGroups, "ID", param, proteinMatrix);
            protein.Peptides = ReferenceMap.GetMapping(Matrix.Peptides, Matrix.ProteinGroups + " Reference", param, peptideMatrix);
            protein.Msms = ReferenceMap.GetMapping(Matrix.MsMs, Matrix.ProteinGroups + " Reference", param, msmsMatrix);

            ReferenceMap peptide = new ReferenceMap();
            peptide.ProteinGroups = ReferenceMap.GetMapping(Matrix.ProteinGroups, Matrix.Peptides + " Reference", param, proteinMatrix);
            peptide.Peptides = ReferenceMap.GetMapping(Matrix.Peptides, "ID", param, peptideMatrix);
            peptide.Msms = ReferenceMap.GetMapping(Matrix.MsMs, Matrix.Peptides + " Reference", param, msmsMatrix);

            ReferenceMap msmsMap = new ReferenceMap();
            msmsMap.ProteinGroups = ReferenceMap.GetMapping(Matrix.ProteinGroups, Matrix.MsMs + " Reference", param, proteinMatrix);
            msmsMap.Peptides = ReferenceMap.GetMapping(Matrix.Peptides, Matrix.MsMs + " Reference", param, peptideMatrix);
            msmsMap.Msms = ReferenceMap.GetMapping(Matrix.MsMs, "ID", param, msmsMatrix);
            */
            PrepareMatrix(proteinMatrix, Section.Protein);
            PrepareMatrix(peptideMatrix, Section.Peptide);
            PrepareMatrix(msmsMatrix, Section.PSM);
            //ReferenceMap.Complete(protein, peptide, msmsMap);
            #endregion
            #region sequence
            processInfo.Progress(n++*100/steps);
            processInfo.Status("sequence");
            int index;
            if ((index = Constants.GetColumnIndex(utils.peptides.sequence, peptideMatrix.StringColumnNames)) != -1){
                AddStringColumn(peptideMatrix, PeptideColumn.SEQUENCE.Name, peptideMatrix.StringColumns[index]);
                if (!PeptideColumn.SEQUENCE.Name.Equals(peptideMatrix.StringColumnNames[index])){
                    RemoveStringColumn(peptideMatrix, peptideMatrix.StringColumnNames[index]);
                }
            }
            if ((index = Constants.GetColumnIndex(utils.msms.sequence, msmsMatrix.StringColumnNames)) != -1){
                AddStringColumn(msmsMatrix, PSMColumn.SEQUENCE.Name, msmsMatrix.StringColumns[index]);
                if (!PSMColumn.SEQUENCE.Name.Equals(msmsMatrix.StringColumnNames[index])){
                    RemoveStringColumn(msmsMatrix, msmsMatrix.StringColumnNames[index]);
                }
            }
            #endregion
            #region accession
            processInfo.Progress(n++*100/steps);
            processInfo.Status("accession");
            SetProteinAccession(proteinMatrix, param);
            if (proteinMatrix.StringColumnNames.Contains(ProteinColumn.ACCESSION.Name)){
                AddStringColumn(peptideMatrix, PeptideColumn.ACCESSION.Name, null);
                AddStringColumn(msmsMatrix, PSMColumn.ACCESSION.Name, null);
            }
            if (proteinMatrix.StringColumnNames.Contains(ProteinColumn.DATABASE.Name)){
                AddStringColumn(peptideMatrix, PeptideColumn.DATABASE.Name, null);
                AddStringColumn(msmsMatrix, PSMColumn.DATABASE.Name, null);
            }
            if (proteinMatrix.StringColumnNames.Contains(ProteinColumn.DATABASE_VERSION.Name)){
                AddStringColumn(peptideMatrix, PeptideColumn.DATABASE_VERSION.Name, null);
                AddStringColumn(msmsMatrix, PSMColumn.DATABASE_VERSION.Name, null);
            }
            int accession = proteinMatrix.StringColumnNames.IndexOf(ProteinColumn.ACCESSION.Name);
            int database = proteinMatrix.StringColumnNames.IndexOf(ProteinColumn.DATABASE.Name);
            int version = proteinMatrix.StringColumnNames.IndexOf(ProteinColumn.DATABASE_VERSION.Name);
            foreach (string protGroupId in map.GetKeys(Matrix.ProteinGroups)){
                foreach (var r in map.GetRows(Matrix.ProteinGroups, protGroupId, Matrix.ProteinGroups)){
                    var rows = map.GetRows(Matrix.Peptides, protGroupId, Matrix.ProteinGroups);
                    if (rows != null && rows.Count > 0){
                        col1 = peptideMatrix.StringColumnNames.IndexOf(PeptideColumn.ACCESSION.Name);
                        col2 = peptideMatrix.StringColumnNames.IndexOf(PeptideColumn.DATABASE.Name);
                        col3 = peptideMatrix.StringColumnNames.IndexOf(PeptideColumn.DATABASE_VERSION.Name);
                        foreach (int row in rows){
                            if (col1 != -1 && accession != -1){
                                peptideMatrix.StringColumns[col1][row] = proteinMatrix.StringColumns[accession][r];
                            }
                            if (col2 != -1 && database != -1){
                                peptideMatrix.StringColumns[col2][row] = proteinMatrix.StringColumns[database][r];
                            }
                            if (col3 != -1 && version != -1){
                                peptideMatrix.StringColumns[col3][row] = proteinMatrix.StringColumns[version][r];
                            }
                        }
                    }
                    rows = map.GetRows(Matrix.MsMs, protGroupId, Matrix.ProteinGroups);
                    if (rows != null && rows.Count > 0){
                        col1 = msmsMatrix.StringColumnNames.IndexOf(PSMColumn.ACCESSION.Name);
                        col2 = msmsMatrix.StringColumnNames.IndexOf(PSMColumn.DATABASE.Name);
                        col3 = msmsMatrix.StringColumnNames.IndexOf(PSMColumn.DATABASE_VERSION.Name);
                        foreach (int row in rows){
                            if (col1 != -1 && accession != -1){
                                msmsMatrix.StringColumns[col1][row] = proteinMatrix.StringColumns[accession][r];
                            }
                            if (col2 != -1 && database != -1){
                                msmsMatrix.StringColumns[col2][row] = proteinMatrix.StringColumns[database][r];
                            }
                            if (col3 != -1 && version != -1){
                                msmsMatrix.StringColumns[col3][row] = proteinMatrix.StringColumns[version][r];
                            }
                        }
                    }
                }
            }
            #endregion
            #region psm_id
            processInfo.Progress(n++*100/steps);
            processInfo.Status("psm_id");
            if ((index = Constants.GetColumnIndex(utils.msms.id, msmsMatrix.StringColumnNames)) != -1){
                AddNumericColumn(msmsMatrix, PSMColumn.PSM_ID.Name,
                                 ChangeStringToNumeric(msmsMatrix.StringColumns[index]));
            }
            #endregion
            #region unique
            processInfo.Progress(n++*100/steps);
            processInfo.Status("unique");
            if ((index = Constants.GetColumnIndex(utils.peptides.unique, peptideMatrix.StringColumnNames)) != -1){
                string[] temp = peptideMatrix.StringColumns[index];
                AddStringColumn(peptideMatrix, PeptideColumn.UNIQUE.Name,
                                temp.Select(x => x == "yes" ? "1" : "0").ToArray());
                GC.Collect();
            }
            if (peptideMatrix.StringColumnNames.Contains(PeptideColumn.UNIQUE.Name)){
                int col = peptideMatrix.StringColumnNames.IndexOf(PeptideColumn.UNIQUE.Name);
                AddStringColumn(msmsMatrix, PSMColumn.UNIQUE.Name, null);
                int unique_col = msmsMatrix.StringColumnNames.IndexOf(PSMColumn.UNIQUE.Name);
                foreach (string msmsId in map.GetKeys(Matrix.MsMs)){
                    var rows = map.GetRows(Matrix.Peptides, msmsId, Matrix.MsMs);
                    if (rows != null && rows.Count > 0){
                        bool unique = false;
                        foreach (int row in rows){
                            unique = peptideMatrix.StringColumns[col][row] != "0";
                        }
                        foreach (var row in map.GetRows(Matrix.MsMs, msmsId, Matrix.MsMs)){
                            msmsMatrix.StringColumns[unique_col][row] = unique ? "1" : "0";
                        }
                    }
                }
            }
            #endregion
            #region search engine
            processInfo.Progress(n++*100/steps);
            processInfo.Status("search engine");
            AddStringColumn(proteinMatrix, ProteinColumn.SEARCH_ENGINE.Name, null);
            AddStringColumn(peptideMatrix, PeptideColumn.SEARCH_ENGINE.Name, null);
            AddStringColumn(msmsMatrix, PSMColumn.SEARCH_ENGINE.Name, null);
            string search_engine = null;
            foreach (Software software in mtd.SoftwareMap.Values){
                if (software.Param.Name == "Andromeda"){
                    search_engine = new CVParam("MS", "MS:1002337", "Andromeda", "").ToString();
                }
            }
            col1 = msmsMatrix.StringColumnNames.IndexOf(PSMColumn.SEARCH_ENGINE.Name);
            col2 = peptideMatrix.StringColumnNames.IndexOf(PeptideColumn.SEARCH_ENGINE.Name);
            col3 = proteinMatrix.StringColumnNames.IndexOf(ProteinColumn.SEARCH_ENGINE.Name);
            foreach (string msmsId in map.GetKeys(Matrix.MsMs)){
                if (search_engine != null){
                    if (col1 != -1){
                        foreach (int row in map.GetRows(Matrix.MsMs, msmsId, Matrix.MsMs)){
                            msmsMatrix.StringColumns[col1][row] = search_engine;
                        }
                    }
                    if (col2 != -1){
                        foreach (int row in map.GetRows(Matrix.Peptides, msmsId, Matrix.MsMs)){
                            peptideMatrix.StringColumns[col2][row] = search_engine;
                        }
                    }
                    if (col3 != -1){
                        foreach (int row in map.GetRows(Matrix.ProteinGroups, msmsId, Matrix.MsMs)){
                            proteinMatrix.StringColumns[col3][row] = search_engine;
                        }
                    }
                }
            }
            #endregion
            #region score
            processInfo.Progress(n++*100/steps);
            processInfo.Status("score");
            CVParam score_accession = null;
            foreach (Software software in mtd.SoftwareMap.Values){
                if (software.Param.Name == "Andromeda"){
                    score_accession = new CVParam("MS", "MS:1002338", "Andromeda:score", "");
                }
            }
            int score_column = Constants.GetColumnIndex(utils.msms.score, msmsMatrix.NumericColumnNames);
            int rawfile_column = Constants.GetColumnIndex(utils.msms.rawfile, msmsMatrix.StringColumnNames);
            if (score_column != -1 && rawfile_column != -1 && score_accession != null){
                Dictionary<string, int> rawfileindex = RawfileIndex(mtd);
                #region for msms
                IEnumerable<string> values =
                    msmsMatrix.NumericColumns[score_column].Select(
                        x =>
                        new CVParam(score_accession.CvLabel, score_accession.Accession, score_accession.Name,
                                    x.ToString()).ToString());
                AddStringColumn(msmsMatrix, PSMColumn.SEARCH_ENGINE_SCORE.Name, values.ToArray());
                #endregion
                #region for proteinGroups
                AddStringColumn(proteinMatrix, ProteinColumn.BEST_SEARCH_ENGINE_SCORE.Name, null);
                int col = proteinMatrix.StringColumnNames.IndexOf(ProteinColumn.BEST_SEARCH_ENGINE_SCORE.Name);
                IList<int> cols = new List<int>();
                foreach (var filename in rawfileindex.Keys){
                    string name = string.Format("{0}_ms_run[{1}]", ProteinColumn.SEARCH_ENGINE_SCORE.Name,
                                                rawfileindex[filename]);
                    AddStringColumn(proteinMatrix, name, null);
                    cols.Add(proteinMatrix.StringColumnNames.IndexOf(name));
                }
                foreach (var proteinID in map.GetKeys(Matrix.ProteinGroups)){
                    foreach (int row in map.GetRows(Matrix.ProteinGroups, proteinID, Matrix.ProteinGroups)){
                        if (row == -1){
                            continue;
                        }
                        var rows = map.GetRows(Matrix.MsMs, proteinID, Matrix.ProteinGroups);
                        if (rows == null || rows.Count == 0){
                            continue;
                        }
                        double[] scores = new double[cols.Count];
                        for (int i = 0; i < scores.Length; i++){
                            scores[i] = double.MinValue;
                        }
                        foreach (int r in rows){
                            double score = msmsMatrix.NumericColumns[score_column][r];
                            int i = rawfileindex[msmsMatrix.StringColumns[rawfile_column][r]] - 1;
                            scores[i] = Math.Max(scores[i], score);
                        }
                        for (int i = 0; i < scores.Length; i++){
                            if (scores[i].Equals(double.MinValue)){
                                continue;
                            }
                            if (cols[i] != -1){
                                proteinMatrix.StringColumns[cols[i]][row] =
                                    new CVParam(score_accession.CvLabel, score_accession.Accession, score_accession.Name,
                                                scores[i].ToString(CultureInfo.InvariantCulture)).ToString();
                            }
                        }
                        if (col != -1){
                            double score = ArrayUtils.Max(scores);
                            proteinMatrix.StringColumns[col][row] =
                                new CVParam(score_accession.CvLabel, score_accession.Accession, score_accession.Name,
                                            score.ToString(CultureInfo.InvariantCulture)).ToString();
                        }
                    }
                }
                #endregion
                #region for peptides
                AddStringColumn(peptideMatrix, PeptideColumn.BEST_SEARCH_ENGINE_SCORE.Name, null);
                col = peptideMatrix.StringColumnNames.IndexOf(PeptideColumn.BEST_SEARCH_ENGINE_SCORE.Name);
                cols = new List<int>();
                foreach (var filename in rawfileindex.Keys){
                    string name = string.Format("{0}_ms_run[{1}]", PeptideColumn.SEARCH_ENGINE_SCORE.Name,
                                                rawfileindex[filename]);
                    AddStringColumn(peptideMatrix, name, null);
                    cols.Add(peptideMatrix.StringColumnNames.IndexOf(name));
                }
                foreach (var peptideID in map.GetKeys(Matrix.Peptides)){
                    foreach (int row in map.GetRows(Matrix.Peptides, peptideID, Matrix.Peptides)){
                        if (row == -1){
                            continue;
                        }
                        var rows = map.GetRows(Matrix.MsMs, peptideID, Matrix.Peptides);
                        if (rows == null || rows.Count == 0){
                            continue;
                        }
                        double[] scores = new double[cols.Count];
                        for (int i = 0; i < scores.Length; i++){
                            scores[i] = double.MinValue;
                        }
                        foreach (int r in rows){
                            double score = msmsMatrix.NumericColumns[score_column][r];
                            int i = rawfileindex[msmsMatrix.StringColumns[rawfile_column][r]] - 1;
                            scores[i] = Math.Max(scores[i], score);
                        }
                        for (int i = 0; i < scores.Length; i++){
                            if (scores[i].Equals(double.MinValue)){
                                continue;
                            }
                            if (cols[i] != -1){
                                peptideMatrix.StringColumns[cols[i]][row] =
                                    new CVParam(score_accession.CvLabel, score_accession.Accession, score_accession.Name,
                                                scores[i].ToString(CultureInfo.InvariantCulture)).ToString();
                            }
                        }
                        if (col != -1){
                            double score = ArrayUtils.Max(scores);
                            peptideMatrix.StringColumns[col][row] =
                                new CVParam(score_accession.CvLabel, score_accession.Accession, score_accession.Name,
                                            score.ToString(CultureInfo.InvariantCulture)).ToString();
                        }
                    }
                }
                cols.Clear();
                #endregion
                rawfileindex.Clear();
                GC.Collect();
            }
            #endregion
            #region num_psms
            processInfo.Progress(n++*100/steps);
            processInfo.Status("num_psms");
            if (rawfile_column != -1 && score_accession != null){
                Dictionary<string, int> rawfileindex = RawfileIndex(mtd);
                #region for proteinGroups
                IList<int> cols = new List<int>();
                foreach (var filename in rawfileindex.Keys){
                    string name = string.Format("{0}_ms_run[{1}]", ProteinColumn.NUM_PSMS.Name, rawfileindex[filename]);
                    AddNumericColumn(proteinMatrix, name, null);
                    cols.Add(proteinMatrix.NumericColumnNames.IndexOf(name));
                }
                foreach (var proteinID in map.GetKeys(Matrix.ProteinGroups)){
                    foreach (int row in map.GetRows(Matrix.ProteinGroups, proteinID, Matrix.ProteinGroups)){
                        if (row == -1){
                            continue;
                        }
                        var rows = map.GetRows(Matrix.MsMs, proteinID, Matrix.ProteinGroups);
                        if (rows == null || rows.Count == 0){
                            continue;
                        }
                        double[] num = new double[cols.Count];
                        for (int i = 0; i < num.Length; i++){
                            num[i] = 0;
                        }
                        foreach (int r in rows){
                            int i = rawfileindex[msmsMatrix.StringColumns[rawfile_column][r]] - 1;
                            num[i]++;
                        }
                        for (int i = 0; i < num.Length; i++){
                            if (cols[i] != -1){
                                proteinMatrix.NumericColumns[cols[i]][row] = num[i];
                            }
                        }
                    }
                }
                #endregion
                rawfileindex.Clear();
                GC.Collect();
            }
            #endregion
            #region modifications
            processInfo.Progress(n++*100/steps);
            processInfo.Status("modifications");
            #region proteinGroups
            AddStringColumn(proteinMatrix, ProteinColumn.MODIFICATIONS.Name, null);
            MultiChoiceParam sitePositions = param.GetMultiChoiceParam("Modification site positions");
            if (sitePositions != null){
                Regex regex = new Regex("(.*) [sS]+ite [pP]+ositions");
                int col = proteinMatrix.StringColumnNames.IndexOf(ProteinColumn.MODIFICATIONS.Name);
                foreach (string columnname in sitePositions.SelectedValues){
                    if (!regex.IsMatch(columnname)){
                        continue;
                    }
                    string name = regex.Match(columnname).Groups[1].Value;
                    if (!BaseLib.Mol.Tables.Modifications.ContainsKey(name)){
                        continue;
                    }
					Modification modification = ConvertModificationToMzTab(BaseLib.Mol.Tables.Modifications[name],
                                                                           Section.Protein);
                    string[] vals = proteinMatrix.StringColumns[proteinMatrix.StringColumnNames.IndexOf(columnname)];
                    for (int row = 0; row < proteinMatrix.RowCount; row++){
                        if (String.IsNullOrEmpty(vals[row])){
                            continue;
                        }
                        IList<string> list = new List<string>();
                        foreach (var item in vals[row].Split(';')){
                            modification.PositionMap.Clear();
                            int pos;
                            if (Int32.TryParse(item, out pos)){
                                modification.AddPosition(pos, null);
                                list.Add(modification.ToString());
                            }
                        }
                        if (String.IsNullOrEmpty(proteinMatrix.StringColumns[col][row])){
                            proteinMatrix.StringColumns[col][row] = StringUtils.Concat(",", list);
                        } else{
                            proteinMatrix.StringColumns[col][row] += "," + StringUtils.Concat(",", list);
                        }
                    }
                }
                GC.Collect();
            }
            #endregion
            #region msms
            AddModifications(param, msmsMatrix, Section.PSM);
            #endregion
            #region peptides
            //TODO:
            AddStringColumn(peptideMatrix, PeptideColumn.MODIFICATIONS.Name, null);
            #endregion
            #endregion
            #region retention time
            processInfo.Progress(n++*100/steps);
            processInfo.Status("retention time");
            if ((index = Constants.GetColumnIndex(utils.msms.retentiontime, msmsMatrix.NumericColumnNames)) != -1){
                AddStringColumn(msmsMatrix, PSMColumn.RETENTION_TIME.Name,
                                ChangeNumericToString(msmsMatrix.NumericColumns[index]));
                if (!PSMColumn.RETENTION_TIME.Name.Equals(msmsMatrix.NumericColumnNames[index])){
                    RemoveNumericColumn(msmsMatrix, msmsMatrix.NumericColumnNames[index]);
                }
            }
            if ((index = msmsMatrix.StringColumnNames.IndexOf(PSMColumn.RETENTION_TIME.Name)) != -1){
                AddStringColumn(peptideMatrix, PeptideColumn.RETENTION_TIME.Name, null);
                int rt_col = peptideMatrix.StringColumnNames.IndexOf(PeptideColumn.RETENTION_TIME.Name);
                SplitList<string> values = new SplitList<string>('|');
                foreach (string peptideId in map.GetKeys(Matrix.Peptides)){
                    var rows = map.GetRows(Matrix.MsMs, peptideId, Matrix.Peptides);
                    if (rows == null || rows.Count == 0){
                        continue;
                    }
                    values.Clear();
                    values.AddRange(rows.Select(r => msmsMatrix.StringColumns[index][r]));
                    foreach (int row in map.GetRows(Matrix.Peptides, peptideId, Matrix.Peptides)){
                        peptideMatrix.StringColumns[rt_col][row] = values.ToString();
                    }
                }
                values.Clear();
            }
            #endregion
            #region retention time window
            processInfo.Progress(n++*100/steps);
            processInfo.Status("retention time window");
            AddStringColumn(peptideMatrix, PeptideColumn.RETENTION_TIME_WINDOW.Name, null);
            #endregion
            #region charge
            processInfo.Progress(n++*100/steps);
            processInfo.Status("charge");
            if ((index = Constants.GetColumnIndex(utils.peptides.charges, peptideMatrix.StringColumnNames)) != -1){
                if (!peptideMatrix.StringColumnNames.Contains(PeptideColumn.CHARGE.Name)){
                    AddNumericColumn(peptideMatrix, PeptideColumn.CHARGE.Name,
                                     ChangeStringToNumeric(peptideMatrix.StringColumns[index]));
                }
            } else{
                AddNumericColumn(peptideMatrix, PeptideColumn.CHARGE.Name, null);
            }
            if ((index = Constants.GetColumnIndex(utils.msms.charge, msmsMatrix.NumericColumnNames)) != -1){
                AddNumericColumn(msmsMatrix, PSMColumn.CHARGE.Name, msmsMatrix.NumericColumns[index]);
            }
            #endregion
            #region mass to charge
            processInfo.Progress(n++*100/steps);
            processInfo.Status("mass to charge");
            if ((index = Constants.GetColumnIndex(utils.msms.mz, msmsMatrix.NumericColumnNames)) != -1){
                AddNumericColumn(msmsMatrix, PSMColumn.EXP_MASS_TO_CHARGE.Name, msmsMatrix.NumericColumns[index]);
            }
            if (msmsMatrix.NumericColumnNames.Contains(PSMColumn.EXP_MASS_TO_CHARGE.Name)){
                int col = msmsMatrix.NumericColumnNames.IndexOf(PSMColumn.EXP_MASS_TO_CHARGE.Name);
                AddNumericColumn(peptideMatrix, PeptideColumn.MASS_TO_CHARGE.Name, null);
                int mz_col = peptideMatrix.NumericColumnNames.IndexOf(PeptideColumn.MASS_TO_CHARGE.Name);
                foreach (string peptideId in map.GetKeys(Matrix.Peptides)){
                    var rows = map.GetRows(Matrix.MsMs, peptideId, Matrix.Peptides);
                    if (rows == null || rows.Count == 0){
                        continue;
                    }
                    double value = double.NaN;
                    foreach (int row in rows){
                        value = msmsMatrix.NumericColumns[col][row];
                        break;
                    }
                    foreach (int row in map.GetRows(Matrix.Peptides, peptideId, Matrix.Peptides)){
                        peptideMatrix.NumericColumns[mz_col][row] = value;
                    }
                }
            }
            #endregion
            #region calc_mass_to_charge
            processInfo.Progress(n++*100/steps);
            processInfo.Status("calc mass to charge");
            int c1 = Constants.GetColumnIndex(utils.msms.mass, msmsMatrix.NumericColumnNames);
            int c2 = Constants.GetColumnIndex(utils.msms.charge, msmsMatrix.NumericColumnNames);
            if (c1 != -1 && c2 != -1){
                AddNumericColumn(msmsMatrix, PSMColumn.CALC_MASS_TO_CHARGE.Name, null);
                int col = msmsMatrix.NumericColumnNames.IndexOf(PSMColumn.CALC_MASS_TO_CHARGE.Name);
                for (int row = 0; row < msmsMatrix.RowCount; row++){
                    msmsMatrix.NumericColumns[col][row] = Molecule.ConvertToMz(msmsMatrix.NumericColumns[c1][row],
                                                                              (int) msmsMatrix.NumericColumns[c2][row]);
                }
                GC.Collect();
            }
            #endregion
            #region spectra ref
            processInfo.Progress(n++*100/steps);
            processInfo.Status("spectra ref");
            Dictionary<string, string> spectraRef = SpectraRef(msmsMatrix, mtd);
            AddStringColumn(peptideMatrix, PeptideColumn.SPECTRA_REF.Name, new string[peptideMatrix.RowCount]);
            AddStringColumn(msmsMatrix, PSMColumn.SPECTRA_REF.Name, new string[msmsMatrix.RowCount]);
            if (spectraRef != null){
                foreach (string msmsId in spectraRef.Keys){
                    if (peptideMatrix.StringColumnNames.IndexOf(PeptideColumn.SPECTRA_REF.Name) != -1){
                        int col = peptideMatrix.StringColumnNames.IndexOf(PeptideColumn.SPECTRA_REF.Name);
                        var rows = map.GetRows(Matrix.Peptides, msmsId, Matrix.MsMs);
                        if (col != -1 && rows != null && rows.Count > 0){
                            foreach (int row in rows){
                                if (string.IsNullOrEmpty(peptideMatrix.StringColumns[col][row])){
                                    peptideMatrix.StringColumns[col][row] = spectraRef[msmsId];
                                } else{
                                    peptideMatrix.StringColumns[col][row] += "|" + spectraRef[msmsId];
                                }
                            }
                        }
                    }
                    if (msmsMatrix.StringColumnNames.IndexOf(PSMColumn.SPECTRA_REF.Name) != -1){
                        int col = msmsMatrix.StringColumnNames.IndexOf(PSMColumn.SPECTRA_REF.Name);
                        var rows = map.GetRows(Matrix.MsMs, msmsId, Matrix.MsMs);
                        if (col != -1 && rows != null && rows.Count > 0){
                            foreach (int row in rows){
                                if (string.IsNullOrEmpty(msmsMatrix.StringColumns[col][row])){
                                    msmsMatrix.StringColumns[col][row] = spectraRef[msmsId];
                                } else{
                                    msmsMatrix.StringColumns[col][row] += "|" + spectraRef[msmsId];
                                }
                            }
                        }
                    }
                }
                spectraRef.Clear();
                GC.Collect();
            }
            #endregion
            #region pre, post, start and end
            processInfo.Progress(n++*100/steps);
            processInfo.Status("pre, post, start and end");
            int pre_col = Constants.GetColumnIndex(utils.peptides.pre, peptideMatrix.StringColumnNames);
            int post_col = Constants.GetColumnIndex(utils.peptides.post, peptideMatrix.StringColumnNames);
            int start_col = Constants.GetColumnIndex(utils.peptides.start, peptideMatrix.StringColumnNames);
            int end_col = Constants.GetColumnIndex(utils.peptides.end, peptideMatrix.StringColumnNames);
            int pre_c = -1;
            AddStringColumn(msmsMatrix, PSMColumn.PRE.Name, null);
            if (pre_col != -1){
                pre_c = msmsMatrix.StringColumnNames.IndexOf(PSMColumn.PRE.Name);
            }
            int post_c = -1;
            AddStringColumn(msmsMatrix, PSMColumn.POST.Name, null);
            if (post_col != -1){
                post_c = msmsMatrix.StringColumnNames.IndexOf(PSMColumn.POST.Name);
            }
            int start_c = -1;
            AddStringColumn(msmsMatrix, PSMColumn.START.Name, null);
            if (start_col != -1){
                start_c = msmsMatrix.StringColumnNames.IndexOf(PSMColumn.START.Name);
            }
            int end_c = -1;
            AddStringColumn(msmsMatrix, PSMColumn.END.Name, null);
            if (end_col != -1){
                end_c = msmsMatrix.StringColumnNames.IndexOf(PSMColumn.END.Name);
            }
            string def = "";
            foreach (string msmsId in map.GetKeys(Matrix.MsMs)){
                var pep_Rows = map.GetRows(Matrix.Peptides, msmsId, Matrix.MsMs);
                if (pep_Rows == null || pep_Rows.Count == 0){
                    continue;
                }
                var msms_Rows = map.GetRows(Matrix.MsMs, msmsId, Matrix.MsMs);
                if (msms_Rows == null || msms_Rows.Count == 0){
                    continue;
                }
                string pre = null;
                string post = null;
                string start = null;
                string end = null;
                foreach (var row in pep_Rows){
                    if (pre_col != -1){
                        pre = peptideMatrix.StringColumns[pre_col][row];
                    }
                    if (post_col != -1){
                        post = peptideMatrix.StringColumns[pre_col][row];
                    }
                    if (start_col != -1){
                        start = peptideMatrix.StringColumns[start_col][row];
                    }
                    if (end_col != -1){
                        end = peptideMatrix.StringColumns[end_col][row];
                    }
                    break;
                }
                foreach (var row in msms_Rows){
                    if (pre_c != -1){
                        msmsMatrix.StringColumns[pre_c][row] = pre ?? def;
                    }
                    if (post_c != -1){
                        msmsMatrix.StringColumns[post_c][row] = post ?? def;
                    }
                    if (start_c != -1){
                        msmsMatrix.StringColumns[start_c][row] = start ?? def;
                    }
                    if (post_c != -1){
                        msmsMatrix.StringColumns[end_c][row] = end ?? def;
                    }
                }
            }
            #endregion
            #region protein coverage
            processInfo.Progress(n++*100/steps);
            processInfo.Status("protein coverage");
            int coverageCol = Constants.GetColumnIndex(proteingroups.coverage, proteinMatrix.NumericColumnNames);
            if (coverageCol != -1){
                AddNumericColumn(proteinMatrix, ProteinColumn.PROTEIN_COVERAGE.Name,
                                 proteinMatrix.NumericColumns[coverageCol].Select(x => x/100).ToArray());
                if (!ProteinColumn.PROTEIN_COVERAGE.Name.Equals(proteinMatrix.NumericColumnNames[coverageCol])){
                    RemoveNumericColumn(proteinMatrix, proteinMatrix.NumericColumnNames[coverageCol]);
                }
            }
            #endregion
            #region abundance
            processInfo.Progress(n++*100/steps);
            processInfo.Status("abundance");
            AddAbundanceColumns(mtd, peptideMatrix, "peptide", null);
            AddAbundanceColumns(mtd, proteinMatrix, "protein", null);
            #endregion
            #region apply supplymentary tables
            processInfo.Progress(n++*100/steps);
            processInfo.Status("apply supplymentary tables");
            if (supplTables == null){
                supplTables = new IMatrixData[NumSupplTables];
            }
            supplTables[0] = proteinMatrix;
            supplTables[1] = peptideMatrix;
            supplTables[2] = msmsMatrix;
            #endregion
            processInfo.Status("Done");
            processInfo.Progress(100);
            return metadataMatrix;
        }

        private static void PrepareMatrix(IMatrixData proteinMatrix, Section section){
            MZTabColumnFactory factory = MZTabColumnFactory.GetInstance(section);
            foreach (var column in factory.StableColumnMapping.Values){
                if (column.Type == typeof (int) || column.Type == typeof (double)){
                    AddNumericColumn(proteinMatrix, column.Name, null);
                } else{
                    AddStringColumn(proteinMatrix, column.Name, null);
                }
            }
            foreach (var column in factory.AbundanceColumnMapping.Values){
                if (column.Type == typeof (int) || column.Type == typeof (double)){
                    AddNumericColumn(proteinMatrix, column.Name, null);
                } else{
                    AddStringColumn(proteinMatrix, column.Name, null);
                }
            }
        }

        private void SetProteinAccession(IMatrixData matrix, Parameters param){
            AddStringColumn(matrix, ProteinColumn.ACCESSION.Name, null);
            int accession_col = matrix.StringColumnNames.IndexOf(ProteinColumn.ACCESSION.Name);
            AddStringColumn(matrix, ProteinColumn.AMBIGUITY_MEMBERS.Name, null);
            int members_col = matrix.StringColumnNames.IndexOf(ProteinColumn.AMBIGUITY_MEMBERS.Name);
            int index = Constants.GetColumnIndex(proteingroups.description, matrix.StringColumnNames);
            AddStringColumn(matrix, ProteinColumn.DESCRIPTION.Name, index != -1 ? matrix.StringColumns[index] : null);
            SingleChoiceParam accession = param.GetSingleChoiceParam("Accession");
            if (accession != null){
                string[] list = matrix.StringColumns[matrix.StringColumnNames.IndexOf(accession.SelectedValue)];
                for (int i = 0; i < list.Length; i++){
                    if (!list[i].Contains(";")){
                        matrix.StringColumns[accession_col][i] = list[i];
                        matrix.StringColumns[members_col][i] = "";
                    } else{
                        int p = list[i].IndexOf(";", StringComparison.Ordinal);
                        matrix.StringColumns[accession_col][i] = list[i].Substring(0, p);
                        matrix.StringColumns[members_col][i] = list[i].Substring(p + 1);
                    }
                }
                AddIdentifierOrigin(matrix.StringColumns[accession_col], param, matrix);
            }
            GC.Collect();
        }

        public Dictionary<string, IList<int>> GetMapping(string group, string name, Parameters param, IMatrixData matrix){
            string[] values = null;
            try{
                int index = ArrayUtils.IndexOf(param.GetAllGroupHeadings(), group);
                SingleChoiceParam single = param.GetGroup(index).GetParam(name) as SingleChoiceParam;
                if (single != null){
                    values = matrix.StringColumns[matrix.StringColumnNames.IndexOf(single.SelectedValue)];
                }
            } catch (Exception){
                values = null;
            }
            Dictionary<string, IList<int>> result = new Dictionary<string, IList<int>>();
            if (values == null){
                return result;
            }
            for (int row = 0; row < values.Length; row++){
                foreach (var id in values[row].Split(';')){
                    if (!result.ContainsKey(id)){
                        result.Add(id, new List<int>());
                    }
                    result[id].Add(row);
                }
            }
            return result;
        }

        private static Dictionary<string, int> RawfileIndex(Metadata mtd){
            if (mtd == null){
                return null;
            }
            Dictionary<string, int> result = new Dictionary<string, int>();
            foreach (var value in mtd.MsRunMap.Values){
                if (!(value is MsRunImpl)){
                    throw new Exception("Object of type MqMsRun expected.");
                }
                string file = (value as MsRunImpl).Description;
                if (file != null){
                    result.Add(file, value.Id);
                }
            }
            return result;
        }

        private static Dictionary<string, string> Rawfile(IMatrixData msmsMatrix){
            int c1 = Constants.GetColumnIndex(msms.id, msmsMatrix.StringColumnNames);
            int c2 = Constants.GetColumnIndex(msms.rawfile, msmsMatrix.StringColumnNames);
            if (c1 == -1 || c2 == -1){
                return null;
            }
            Dictionary<string, string> result = new Dictionary<string, string>();
            for (int row = 0; row < msmsMatrix.RowCount; row++){
                string id = msmsMatrix.StringColumns[c1][row];
                string rawfile = msmsMatrix.StringColumns[c2][row];
                if (!result.ContainsKey(id)){
                    result.Add(id, rawfile);
                }
            }
            return result;
        }

        private static Dictionary<string, string> SpectraRef(IMatrixData msmsMatrix, Metadata mtd){
            Dictionary<string, string> rawfile = Rawfile(msmsMatrix);
            if (rawfile == null){
                return null;
            }
            Dictionary<string, int> rawfileIndex = RawfileIndex(mtd);
            if (rawfileIndex == null){
                return null;
            }
            int idColumn = Constants.GetColumnIndex(msms.id, msmsMatrix.StringColumnNames);
            if (idColumn == -1){
                return null;
            }
            int fileColumn = Constants.GetColumnIndex(msms.rawfile, msmsMatrix.StringColumnNames);
            if (fileColumn == -1){
                return null;
            }
            int scanColumn = Constants.GetColumnIndex(msms.scannumber, msmsMatrix.NumericColumnNames);
            if (scanColumn == -1){
                return null;
            }
            Dictionary<string, string> result = new Dictionary<string, string>();
            for (int row = 0; row < msmsMatrix.RowCount; row++){
                string id = msmsMatrix.StringColumns[idColumn][row];
                if (result.ContainsKey(id)){
                    continue;
                }
                string file = msmsMatrix.StringColumns[fileColumn][row];
                double scan = msmsMatrix.NumericColumns[scanColumn][row];
                result.Add(id, string.Format("ms_run[{0}]:scannumber={1}", rawfileIndex[file], scan));
            }
            return result;
        }

        private void ReplaceCharacter(IMatrixData matrix, string column, string oldChar, string newChar){
            if (string.IsNullOrEmpty(column)){
                return;
            }
            if (matrix.StringColumnNames.Contains(column)){
                int col = matrix.StringColumnNames.IndexOf(column);
                for (int row = 0; row < matrix.RowCount; row++){
                    if (matrix.StringColumns[col][row].Contains(oldChar)){
                        matrix.StringColumns[col][row] = matrix.StringColumns[col][row].Replace(oldChar, newChar);
                    }
                }
            }
        }

        private void ExpandMatrix(IMatrixData matrix, string column){
            if (string.IsNullOrEmpty(column)){
                return;
            }
            if (!matrix.StringColumnNames.Contains(column) && !matrix.MultiNumericColumnNames.Contains(column)){
                return;
            }
            ExpandMultiNumeric expand = new ExpandMultiNumeric();
            IMatrixData[] supplTables = null;
            IDocumentData[] doucments = null;
            List<Parameter> list = new List<Parameter>{
                new MultiChoiceParam("String columns"){
                    Values = matrix.StringColumnNames,
                    Value =
                        matrix.StringColumnNames.Contains(column)
                            ? new[]{matrix.StringColumnNames.IndexOf(column)}
                            : new int[0]
                },
                new MultiChoiceParam("Multi-numeric columns"){
                    Values = matrix.MultiNumericColumnNames,
                    Value =
                        matrix.MultiNumericColumnNames.Contains(column)
                            ? new[]{matrix.MultiNumericColumnNames.IndexOf(column)}
                            : new int[0]
                }
            };
            expand.ProcessData(matrix, new Parameters(list), ref supplTables, ref doucments, null);
        }

        private double[] ChangeStringToNumeric(IList<string> values){
            if (values == null){
                return null;
            }
            double[] result = new double[values.Count];
            for (int i = 0; i < result.Length; i++){
                double number;
                if (double.TryParse(values[i], out number)){
                    result[i] = number;
                } else{
                    result[i] = double.NaN;
                }
            }
            return result;
        }

        private string[] ChangeNumericToString(IEnumerable<double> values){
            return values == null ? null : values.Select(x => x.ToString()).ToArray();
        }

        public override Parameters GetParameters(IMatrixData[] inputData, ref string errString){
            Parameters parameters = new Parameters();
            #region ProteinGroups group
            List<Parameter> temp = new List<Parameter>();
            IList<string> stringColumns =
                GetValuesPlusEmptyString(GetMatrixData(Matrix.ProteinGroups, inputData).StringColumnNames);
            temp.Add(new SingleChoiceParam("ID"){
                Values = stringColumns,
                SelectedValue = Constants.GetColumnName(proteingroups.id, stringColumns) ?? ""
            });
            temp.Add(new SingleChoiceParam(Matrix.Peptides + " Reference"){
                Values = stringColumns,
                SelectedValue = Constants.GetColumnName(proteingroups.peptide_IDs, stringColumns) ?? ""
            });
            temp.Add(new SingleChoiceParam(Matrix.MsMs + " Reference"){
                Values = stringColumns,
                SelectedValue = Constants.GetColumnName(proteingroups.msms_IDs, stringColumns) ?? ""
            });
            temp.Add(new SingleChoiceParam("Accession"){
                Values = stringColumns,
                SelectedValue = Constants.GetColumnName(proteingroups.accession, stringColumns) ?? ""
            });
            temp.Add(GetIdentifierParam(""));
            temp.Add(new MultiChoiceParam("Modification site positions", new[]{0}){
                Values = stringColumns.Where(x => x.ToLower().EndsWith("site positions")).ToArray()
            });
            parameters.AddParameterGroup(temp, Matrix.ProteinGroups, false);
            #endregion
            #region Peptides group
            temp = new List<Parameter>();
            stringColumns = GetValuesPlusEmptyString(GetMatrixData(Matrix.Peptides, inputData).StringColumnNames);
            temp.Add(new SingleChoiceParam("ID"){
                Values = stringColumns,
                SelectedValue = Constants.GetColumnName(peptides.id, stringColumns) ?? ""
            });
            temp.Add(new SingleChoiceParam(Matrix.ProteinGroups + " Reference"){
                Values = stringColumns,
                SelectedValue = Constants.GetColumnName(peptides.proteinGroup_IDs, stringColumns) ?? ""
            });
            temp.Add(new SingleChoiceParam(Matrix.MsMs + " Reference"){
                Values = stringColumns,
                SelectedValue = Constants.GetColumnName(peptides.msms_IDs, stringColumns) ?? ""
            });
            parameters.AddParameterGroup(temp, Matrix.Peptides, false);
            #endregion
            #region MS/MS group
            temp = new List<Parameter>();
            stringColumns = GetValuesPlusEmptyString(GetMatrixData(Matrix.MsMs, inputData).StringColumnNames);
            temp.Add(new SingleChoiceParam("ID"){
                Values = stringColumns,
                SelectedValue = Constants.GetColumnName(msms.id, stringColumns) ?? ""
            });
            temp.Add(new SingleChoiceParam(Matrix.ProteinGroups + " Reference"){
                Values = stringColumns,
                SelectedValue = Constants.GetColumnName(msms.proteinGroup_IDs, stringColumns) ?? ""
            });
            temp.Add(new SingleChoiceParam(Matrix.Peptides + " Reference"){
                Values = stringColumns,
                SelectedValue = Constants.GetColumnName(msms.peptide_ID, stringColumns) ?? ""
            });
            temp.AddRange(GetModificationsParam(GetMatrixData(Matrix.MsMs, inputData).StringColumnNames));
            parameters.AddParameterGroup(temp, Matrix.MsMs, false);
            #endregion
            return parameters;
        }

        private IList<string> GetValuesPlusEmptyString(IEnumerable<string> enumerable){
            return new List<string>(enumerable){""};
        }

        private Parameter GetIdentifierParam(string version){
            IList<string> dbList = new List<string>{
                FileUtils.GetContaminantFilePath(),
                @"M:\Fasta\UniProt\proteomes\2013_05_29\HUMAN.fasta",
                "",
                ""
            };
            SingleChoiceWithSubParams param = new SingleChoiceWithSubParams("Database"){
                Values = new string[dbList.Count],
                SubParams = new Parameters[dbList.Count]
            };
            param.TotalWidth = 700;
            param.ParamNameWidth = 100;
            for (int i = 0; i < dbList.Count; i++){
                param.Values[i] = (i + 1).ToString();
                List<Parameter> sub = new List<Parameter>();
                for (int j = 0; j <= i && j < dbList.Count; j++){
                    sub.AddRange(AddFasta(j + 1, dbList[j], version));
                }
                if (!string.IsNullOrEmpty(dbList[i])){
                    param.Value = i;
                }
                param.SubParams[i] = new Parameters(sub);
            }
            return param;
        }

        private static IEnumerable<Parameter> AddFasta(int n, string fastaValue, string version){
            IList<Parameter> list = new List<Parameter>();
            SingleChoiceParam type = new SingleChoiceParam(string.Format("Type {0}", n)){
                Values = new List<string>{"Specie", "Contaminants"}
            };
            if (fastaValue != null){
                string filename = Path.GetFileName(fastaValue);
                if (filename != null && filename.ToLower().Contains("contaminant")){
                    type.Value = type.Values.IndexOf("Contaminants");
                } else{
                    type.Value = type.Values.IndexOf("Specie");
                    version = "";
                }
            } else{
                type.Value = 0;
                version = "";
            }
            list.Add(type);
            FileParam fastaPath = new FileParam(string.Format("FASTA file {0}", n)){
                Filter = "Fasta (*.fasta)|*.fasta",
                Help = "Choose fasta file",
                Value = fastaValue
            };
            if (!File.Exists(fastaPath.Value)){
                fastaPath.Value = "";
            }
            list.Add(fastaPath);
            StringParam fastaVersion = new StringParam(string.Format("FASTA version {0}", n)){
                Help = "Enter the database version if known",
            };
            if (!string.IsNullOrEmpty(version)){
                fastaVersion.Value = version;
            } else if (!string.IsNullOrEmpty(fastaPath.Value)){
                string dir = Path.GetDirectoryName(fastaPath.Value);
                if (!string.IsNullOrEmpty(dir)){
                    string name = dir.Substring(dir.LastIndexOf(Path.DirectorySeparatorChar) + 1);
                    fastaVersion.Value = name.Replace("_", "-");
                }
            }
            list.Add(fastaVersion);
            return list;
        }

        private static void AddIdentifierOrigin(string[] accessions, Parameters parameters, IMatrixData mdata){
            AddStringColumn(mdata, ProteinColumn.SPECIES.Name, null);
            int species_col = mdata.StringColumnNames.IndexOf(ProteinColumn.SPECIES.Name);
            AddNumericColumn(mdata, ProteinColumn.TAXID.Name, null);
            int taxid_col = mdata.NumericColumnNames.IndexOf(ProteinColumn.TAXID.Name);
            mdata.NumericColumns[taxid_col] = mdata.NumericColumns[taxid_col].Select(x => Double.NaN).ToArray();
            AddStringColumn(mdata, ProteinColumn.DATABASE.Name, null);
            int db_col = mdata.StringColumnNames.IndexOf(ProteinColumn.DATABASE.Name);
            AddStringColumn(mdata, ProteinColumn.DATABASE_VERSION.Name, null);
            int db_version_col = mdata.StringColumnNames.IndexOf(ProteinColumn.DATABASE_VERSION.Name);
            SingleChoiceWithSubParams param = parameters.GetSingleChoiceWithSubParams("Database");
            int n = param.Values.IndexOf(param.SelectedValue);
            for (int c = 0; c < n + 1; c++){
                SingleChoiceParam type = param.SubParams[n].GetSingleChoiceParam(string.Format("Type {0}", c + 1));
                bool isContaminants = type.SelectedValue.ToLower().Contains("contaminant");
                FileParam file = param.SubParams[n].GetFileParam(string.Format("FASTA file {0}", c + 1));
                if (file == null){
                    continue;
                }
                string fastaFile = file.Value;
                if (string.IsNullOrEmpty(fastaFile)){
                    continue;
                }
                string database = Path.GetFileName(fastaFile);
                if (database == null || !BaseLib.Mol.Tables.Databases.ContainsKey(database)){
                    continue;
                }
                string version_Value = null;
                try{
                    StringParam versionParam =
                        param.SubParams[n].GetStringParam(string.Format("FASTA version {0}", c + 1));
                    if (versionParam != null){
                        version_Value = versionParam.Value;
                    }
                } catch (Exception){
                    version_Value = null;
                }
                SequenceDatabase db = BaseLib.Mol.Tables.Databases[database];
                if (db != null){
                    #region Extract Fasta identifier
                    Regex regex = new Regex(db.SearchExpression);
                    StreamReader reader = new StreamReader(fastaFile);
                    List<string> proteinIdentifier = new List<string>();
                    string line;
                    while ((line = reader.ReadLine()) != null){
                        if (regex.IsMatch(line)){
                            Match match = regex.Match(line);
                            string value = match.Groups[1].Value;
                            proteinIdentifier.Add(value);
                        }
                    }
                    #endregion
                    #region Match identifier
                    proteinIdentifier.Sort();
                    string[] array = proteinIdentifier.ToArray();
                    string version = string.IsNullOrEmpty(version_Value)
                                         ? string.Format("{0} entries", proteinIdentifier.Count)
                                         : string.Format("{0} ({1} entries)", version_Value, proteinIdentifier.Count);
                    var values = accessions;
                    for (int row = 0; row < values.Length; row++){
                        SetNumericValue(mdata, -1, taxid_col, row);
                        string id = values[row];
						string revPrefix = GlobalConstants.revPrefix;
                        if (id.StartsWith(revPrefix))
                        {
                            SetStringValue(mdata, "Reverse", db_col, row);
                            continue;
                        }
						string conPrefix = GlobalConstants.conPrefix;
                        if (isContaminants && id.StartsWith(conPrefix))
                        {
                            id = id.Replace(conPrefix, "");
                            SetStringValue(mdata, "Contaminants", db_col, row);
                        }
                        int index = Array.BinarySearch(array, id);
                        if (index >= 0){
                            double t;
                            if (!Double.TryParse(db.Taxid, out t)){
                                t = -1;
                            }
                            SetStringValue(mdata, db.Species, species_col, row);
                            SetNumericValue(mdata, t, taxid_col, row);
                            SetStringValue(mdata, db.Source, db_col, row);
                            SetStringValue(mdata, version, db_version_col, row);
                        }
                    }
                    #endregion
                }
            }
        }

        private static void SetStringValue(IMatrixData mdata, string value, int col, int row){
            if (string.IsNullOrEmpty(value)){
                return;
            }
            if (mdata.StringColumns[col][row].Equals(Constants.nullValue)){
                mdata.StringColumns[col][row] = value;
            } else{
                if (string.IsNullOrEmpty(mdata.StringColumns[col][row])){
                    mdata.StringColumns[col][row] = value;
                } else if (!mdata.StringColumns[col][row].Contains(value)){
                    mdata.StringColumns[col][row] += ", " + value;
                }
            }
        }

        private static void SetNumericValue(IMatrixData mdata, double value, int col, int row){
            if (double.IsNaN(value)){
                return;
            }
            if (double.IsNaN(mdata.NumericColumns[col][row]) || mdata.NumericColumns[col][row].Equals(-1)){
                mdata.NumericColumns[col][row] = value;
            }
        }

        private IEnumerable<Parameter> GetModificationsParam(List<string> columnNames){
            IList<Parameter> list = new List<Parameter>();
            string name = Constants.GetAll(msms.modifications).FirstOrDefault();
            if (name != null){
                list.Add(new SingleChoiceParam(name, columnNames.IndexOf(name)){Values = columnNames});
            }
            name = Constants.GetAll(msms.mod_sequence).FirstOrDefault();
            if (name != null){
                list.Add(new SingleChoiceParam(name, columnNames.IndexOf(name)){Values = columnNames});
            }
            name = Constants.GetAll(msms.mod_probabilities).FirstOrDefault();
            if (name != null){
                var values = columnNames.Where(x => x.ToLower().EndsWith(name.ToLower()));
                int[] selection = new int[values.Count()];
                for (int i = 0; i < selection.Length; i++){
                    selection[i] = i;
                }
                list.Add(new MultiChoiceParam(name, selection){Values = values.ToArray()});
            }
            return list;
        }

        private static void AddModifications(Parameters parameters, IMatrixData mdata, Section section){
            AddStringColumn(mdata, ProteinColumn.MODIFICATIONS.Name, null);
            int col = mdata.StringColumnNames.IndexOf(ProteinColumn.MODIFICATIONS.Name);
            string[] names = parameters.GetAllParameters().Select(x => x.Name).ToArray();
            SingleChoiceParam modParam =
                parameters.FindParameter(Constants.GetColumnName(msms.modifications, names)) as SingleChoiceParam;
            if (modParam == null){
                throw new NullReferenceException("Could not find Parameter <" +
                                                 Constants.GetColumnName(msms.modifications, names) + ">");
            }
            SingleChoiceParam modSeqParam =
                parameters.FindParameter(Constants.GetColumnName(msms.mod_sequence, names)) as SingleChoiceParam;
            if (modSeqParam == null){
                throw new NullReferenceException("Could not find Parameter <" +
                                                 Constants.GetColumnName(msms.mod_sequence, names) + ">");
            }
            MultiChoiceParam probabilityParam =
                parameters.FindParameter(Constants.GetColumnName(msms.mod_probabilities, names)) as MultiChoiceParam;
            if (probabilityParam == null){
                throw new NullReferenceException("Could not find Parameter <" +
                                                 Constants.GetColumnName(msms.mod_probabilities, names) + ">");
            }
            int index;
            string[] modColumn = CreateSection.GetValues(mdata, modParam.SelectedValue, out index);
            if (modColumn == null){
                return;
            }
            string[] modSequence = CreateSection.GetValues(mdata, modSeqParam.SelectedValue);
            if (modSequence == null){
                return;
            }
            Dictionary<string, string[]> probabilityColumns = new Dictionary<string, string[]>();
            foreach (string columnname in probabilityParam.SelectedValues){
                string key = columnname.Replace("Probabilities", "").Trim();
                var values = CreateSection.GetValues(mdata, columnname);
                if (values != null){
                    probabilityColumns.Add(key, values);
                }
            }
            Regex regex = new Regex(@"\([^)]*\)");
            Regex num = new Regex(@"([\d]*)(.+)");
            for (int i = 0; i < modColumn.Length; i++){
                if (modColumn[i] == "Unmodified"){
                    mdata.StringColumns[col][i] = "";
                    continue;
                }
                Dictionary<string, List<int>> positions = new Dictionary<string, List<int>>();
                string sequence = modSequence[i];
                while (regex.IsMatch(sequence)){
                    Match match = regex.Match(sequence);
                    string abbreviation = sequence.Substring(match.Index + 1, match.Length - 2);
                    int position = match.Index - 1;
                    if (!positions.ContainsKey(abbreviation)){
                        positions.Add(abbreviation, new List<int>());
                    }
                    positions[abbreviation].Add(position);
                    sequence = sequence.Remove(match.Index, match.Length);
                }
                Dictionary<string, List<double>> probabilities = new Dictionary<string, List<double>>();
                Dictionary<string, List<int>> probPositions = new Dictionary<string, List<int>>();
                foreach (var key in probabilityColumns.Keys){
                    string text = probabilityColumns[key][i];
                    while (regex.IsMatch(text)){
                        Match match = regex.Match(text);
                        double prop;
                        double.TryParse(text.Substring(match.Index + 1, match.Length - 2), out prop);
                        int position = match.Index - 1;
                        if (!probabilities.ContainsKey(key)){
                            probabilities.Add(key, new List<double>());
                        }
                        if (!probPositions.ContainsKey(key)){
                            probPositions.Add(key, new List<int>());
                        }
                        probabilities[key].Add(prop);
                        probPositions[key].Add(position);
                        text = text.Remove(match.Index, match.Length);
                    }
                }
                MzTabLibrary.model.ModificationList modifications = new MzTabLibrary.model.ModificationList();
                foreach (var mod in modColumn[i].Split(',')){
                    Match match = num.Match(mod);
                    int n = 1;
                    if (!string.IsNullOrEmpty(match.Groups[1].Value)){
                        int.TryParse(match.Groups[1].Value, out n);
                    }
                    string title = match.Groups[2].Value;
                    var m = BaseLib.Mol.Tables.ModificationList.FirstOrDefault(x => x.Name == title);
                    if (m == null){
                        continue;
                    }
                    Modification modification = ConvertModificationToMzTab(m, section);
                    if (probabilities.ContainsKey(m.Name) && probPositions.ContainsKey(m.Name)){
                        List<double> prop = probabilities[m.Name];
                        List<int> pos = probPositions[m.Name];
                        for (int j = 0; j < prop.Count; j++){
                            modification.AddPosition(pos[j],
                                                     new CVParam("MS", "MS:1001876", "modifications probability",
                                                                 prop[j].ToString(CultureInfo.InvariantCulture)));
                        }
                    } else{
                        if (positions.ContainsKey(m.Abbreviation)){
                            foreach (var pos in positions[m.Abbreviation]){
                                modification.AddPosition(pos, null);
                            }
                        }
                    }
                    for (int j = 0; j < n; j++){
                        modifications.Add(modification);
                    }
                }
                mdata.StringColumns[col][i] = modifications.ToString();
            }
            GC.Collect();
        }
    }
}