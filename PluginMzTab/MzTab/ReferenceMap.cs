using System;
using System.Collections.Generic;
using BaseLib.Param;
using BaseLibS.Util;
using PerseusApi.Matrix;

namespace PluginMzTab.mztab{
    public class RefMap{
        private readonly IList<string> nameList;

        readonly Dictionary<string, Table> map = new Dictionary<string, Table>();

        public RefMap(IList<string> nameList, IList<string> keyList, IList<IMatrixData> matrixList, string[][] columnNames)
        {
            this.nameList = nameList;
            int n = nameList.Count;

            for (int i = 0; i < keyList.Count; i++){
                string[] keys = CreateSection.GetValues(matrixList[i], keyList[i]);

                if (!map.ContainsKey(nameList[i])){
                    map.Add(nameList[i], new Table(keys, n));
                }
            }

            for (int i = 0; i < nameList.Count; i++){
                Add(nameList[i], i, columnNames[i], matrixList);
            }
        }

        private void Add(string name, int index, IList<string> columnnames, IList<IMatrixData> matrixList){
            int n = columnnames.Count;

            Table table;            
            if (map.ContainsKey(name)){
                table = map[name];
            }
            else{
                table = new Table(CreateSection.GetValues(matrixList[index], columnnames[index]), n);
                map.Add(name, table);
            }            
            
            for (int i = 0; i < columnnames.Count && i < matrixList.Count; i++){
                if (string.IsNullOrEmpty(columnnames[i])){
                    //TODO Add reference to from other tables
                }

                string[] values = CreateSection.GetValues(matrixList[i], columnnames[i]);
                if (values == null){
                    continue;
                }
                
                for (int row = 0; row < values.Length; row++){
                    foreach (var value in values[row].Split(';')){
                        if (!table.ContainsKey(value)){
                             table.Add(value, new Ref[n]);                           
                        }
                        
                        if (table[value][i] == null){
                            table[value][i] = new Ref();
                        }

                        if (!table[value][i].Contains(row)){
                            table[value][i].Add(row);
                        }
                    }
                }
            }
        }

        public IEnumerable<string> GetKeys(string name){
            if (map.ContainsKey(name)){
                return map[name].Keys;
            }
            return new string[0];
        }

        public IList<int> GetRows(string name, string id, string matrixId){
            int index = nameList.IndexOf(name);
            if (index != -1){
                if (map.ContainsKey(matrixId)){                    
                    if (map[matrixId].ContainsKey(id)){
                        if (map[matrixId][id][index] != null){
                            return map[matrixId][id][index];
                        }
                        else{
                            return new int[0];
                        }
                        
                    }
                }
            }
            return new int[0];
        }
    }

    public class Table : Dictionary<string, Ref[]>
    {
        public Table(string[] keys, int n){
            if (keys == null || keys.Length == 0){
                return;
            }

            foreach (var key in keys){
                if (ContainsKey(key)){
                    continue;
                }
                Add(key, new Ref[n]);
            }
        }
    }

    public class Ref : List<int>{
        
    }

    public class ReferenceMap{

        public TableReference ProteinGroups { get; set; }
        public TableReference Peptides { get; set; }
        public TableReference Msms { get; set; }

        public static TableReference GetMapping(string group, string name, Parameters param, IMatrixData matrix)
        {
            string[] values = null;
            try
            {
                int index = ArrayUtils.IndexOf(param.GetAllGroupHeadings(), group);
                SingleChoiceParam single = param.GetGroup(index).GetParam(name) as SingleChoiceParam;
                if (single != null)
                {
                    values = matrix.StringColumns[matrix.StringColumnNames.IndexOf(single.SelectedValue)];
                }
            }
            catch (Exception)
            {
                values = null;
            }

            TableReference result = new TableReference();
            if (values == null)
            {
                return result;
            }

            for (int row = 0; row < values.Length; row++)
            {
                foreach (var id in values[row].Split(';'))
                {
                    if (!result.ContainsKey(id))
                    {
                        result.Add(id, new List<int>());
                    }
                    result[id].Add(row);
                }
            }

            return result;
        }

        internal static void Complete(ReferenceMap protein, ReferenceMap peptide, ReferenceMap msms){
            protein.Peptides = Complete(protein.Peptides, protein.ProteinGroups, peptide.Peptides, peptide.ProteinGroups);
            protein.Msms = Complete(protein.Msms, protein.ProteinGroups, msms.Msms, msms.ProteinGroups);

            peptide.ProteinGroups = Complete(peptide.ProteinGroups, peptide.Peptides, protein.ProteinGroups, protein.Peptides);
            peptide.Msms = Complete(peptide.Msms, peptide.Peptides, msms.Msms, msms.Peptides);

            msms.ProteinGroups = Complete(msms.ProteinGroups, msms.Msms, protein.ProteinGroups, protein.Msms);
            msms.Peptides = Complete(msms.Peptides, msms.Msms, peptide.Peptides, peptide.Msms);
        }

        private static TableReference Complete(TableReference ref11, TableReference ref12, TableReference ref21,
                                               TableReference ref22){
            if (ref11 != null && ref11.Count > 0){
                return ref11;
            }

            if (ref21 == null || ref22 == null){
                return ref11;
            }

            ref11 = new TableReference();


            Dictionary<int, string> tmp = new Dictionary<int, string>();
            foreach (string key in ref12.Keys){
                foreach (var row in ref12[key]){
                    if (tmp.ContainsKey(row)){
                        continue;
                    }
                    tmp.Add(row, key);
                }
            }

            foreach (string key in ref22.Keys){
                foreach (var row in ref22[key]){
                    if (!tmp.ContainsKey(row)){
                        continue;
                    }
                    string newKey = tmp[row];
                    if (!ref11.ContainsKey(newKey)){
                        ref11.Add(newKey, new List<int>());
                    }
                    ref11[newKey].Add(row);
                }
            }
            tmp.Clear();
            GC.Collect();

            return ref11;
        }
    }

    public class TableReference : Dictionary<string, IList<int>>{

    }
}