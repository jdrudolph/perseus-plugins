using System;
using System.Collections.Generic;
using PluginMzTab.Lib.Model;
using PluginMzTab.Lib.Utils.Errors;

namespace PluginMzTab.Lib.Utils.Parser{
    public class PSMLineParser : MZTabDataLineParser{
        public PSMLineParser(MZTabColumnFactory factory, PositionMapping positionMapping, Metadata metadata,
                             MZTabErrorList errorList) : base(factory, positionMapping, metadata, errorList){}

        protected override void checkStableData(){
            for (int physicalPosition = 1; physicalPosition < _items.Length; physicalPosition++){
                string key = positionMapping.get(physicalPosition);
                MZTabColumn column = null;
                if (factory.ColumnMapping.ContainsKey(key)){
                    column = factory.ColumnMapping[key];
                }

                if (column != null){
                    string columnName = column.Name;
                    string target = _items[physicalPosition];
                    if (columnName.Equals(PSMColumn.SEQUENCE.Name)){
                        checkSequence(column, target);
                    }
                    else if (columnName.Equals(PSMColumn.PSM_ID.Name)){
                        checkPSMID(column, target);
                    }
                    else if (columnName.Equals(PSMColumn.ACCESSION.Name)){
                        checkAccession(column, target);
                    }
                    else if (columnName.Equals(PSMColumn.UNIQUE.Name)){
                        checkUnique(column, target);
                    }
                    else if (columnName.Equals(PSMColumn.DATABASE.Name)){
                        checkDatabase(column, target);
                    }
                    else if (columnName.Equals(PSMColumn.DATABASE_VERSION.Name)){
                        checkDatabaseVersion(column, target);
                    }
                    else if (columnName.Equals(PSMColumn.SEARCH_ENGINE.Name)){
                        checkSearchEngine(column, target);
                    }
                    else if (columnName.Equals(PSMColumn.SEARCH_ENGINE_SCORE.Name)){
                        checkSearchEngineScore(column, target);
                    }
                    else if (columnName.Equals(PSMColumn.RELIABILITY.Name)){
                        checkReliability(column, target);
                    }
                    else if (columnName.Equals(PSMColumn.MODIFICATIONS.Name)){
                        string sequence = null;
                        if (exchangeMapping.ContainsKey(PSMColumn.SEQUENCE.LogicPosition)){
                            sequence = _items[exchangeMapping[PSMColumn.SEQUENCE.LogicPosition]];
                        }
                        checkModifications(column, sequence, target);
                    }
                    else if (columnName.Equals(PSMColumn.RETENTION_TIME.Name)){
                        checkRetentionTime(column, target);
                    }
                    else if (columnName.Equals(PSMColumn.CHARGE.Name)){
                        checkCharge(column, target);
                    }
                    else if (columnName.Equals(PSMColumn.EXP_MASS_TO_CHARGE.Name)){
                        checkExpMassToCharge(column, target);
                    }
                    else if (columnName.Equals(PSMColumn.CALC_MASS_TO_CHARGE.Name)){
                        checkCalcMassToCharge(column, target);
                    }
                    else if (columnName.Equals(PSMColumn.URI.Name)){
                        checkURI(column, target);
                    }
                    else if (columnName.Equals(PSMColumn.SPECTRA_REF.Name)){
                        checkSpectraRef(column, target);
                    }
                    else if (columnName.Equals(PSMColumn.PRE.Name)){
                        checkPre(column, target);
                    }
                    else if (columnName.Equals(PSMColumn.POST.Name)){
                        checkPost(column, target);
                    }
                    else if (columnName.Equals(PSMColumn.START.Name)){
                        checkStart(column, target);
                    }
                    else if (columnName.Equals(PSMColumn.END.Name)){
                        checkEnd(column, target);
                    }
                }
            }
        }

        protected override int loadStableData(MZTabRecord record, string line){
            _items = line.Split(new[]{MZTabConstants.TAB}, StringSplitOptions.None);
            _items[_items.Length - 1] = _items[_items.Length - 1].Trim();

            PSM psm = (PSM) record;

            SortedDictionary<string, MZTabColumn> columnMapping = factory.ColumnMapping;
            int physicalPosition = 1;

            string logicalPosition = positionMapping.get(physicalPosition);

            MZTabColumn column = null;
            if (columnMapping.ContainsKey(logicalPosition)){
                column = columnMapping[logicalPosition];
            }

            while (column != null && column is PSMColumn){
                string target;
                try{
                    target = _items[physicalPosition];
                }
                catch (IndexOutOfRangeException e){
                    Console.Error.WriteLine(_lineNumber + ":\t" + line);
                    throw e;
                }
                string columnName = column.Name;
                if (columnName.Equals(PSMColumn.SEQUENCE.Name)){
                    psm.Sequence = target;
                }
                else if (columnName.Equals(PSMColumn.PSM_ID.Name)){
                    psm.SetPSM_ID(target);
                }
                else if (columnName.Equals(PSMColumn.ACCESSION.Name)){
                    psm.Accession = target;
                }
                else if (columnName.Equals(PSMColumn.UNIQUE.Name)){
                    psm.setUnique(target);
                }
                else if (columnName.Equals(PSMColumn.DATABASE.Name)){
                    psm.Database = target;
                }
                else if (columnName.Equals(PSMColumn.DATABASE_VERSION.Name)){
                    psm.DatabaseVersion = target;
                }
                else if (columnName.Equals(PSMColumn.SEARCH_ENGINE.Name)){
                    psm.setSearchEngine(target);
                }
                else if (columnName.Equals(PSMColumn.SEARCH_ENGINE_SCORE.Name)){
                    psm.setSearchEngineScore(target);
                }
                else if (columnName.Equals(PSMColumn.RELIABILITY.Name)){
                    psm.setReliability(target);
                }
                else if (columnName.Equals(PSMColumn.MODIFICATIONS.Name)){
                    psm.setModifications(target);
                }
                else if (columnName.Equals(PSMColumn.RETENTION_TIME.Name)){
                    psm.setRetentionTime(target);
                }
                else if (columnName.Equals(PSMColumn.CHARGE.Name)){
                    psm.setCharge(target);
                }
                else if (columnName.Equals(PSMColumn.EXP_MASS_TO_CHARGE.Name)){
                    psm.SetExpMassToCharge(target);
                }
                else if (columnName.Equals(PSMColumn.CALC_MASS_TO_CHARGE.Name)){
                    psm.SetCalcMassToCharge(target);
                }
                else if (columnName.Equals(PSMColumn.URI.Name)){
                    psm.SetUri(target);
                }
                else if (columnName.Equals(PSMColumn.SPECTRA_REF.Name)){
                    psm.SetSpectraRef(target);
                }
                else if (columnName.Equals(PSMColumn.PRE.Name)){
                    psm.Pre = target;
                }
                else if (columnName.Equals(PSMColumn.POST.Name)){
                    psm.Post = target;
                }
                else if (columnName.Equals(PSMColumn.START.Name)){
                    psm.Start = target;
                }
                else if (columnName.Equals(PSMColumn.END.Name)){
                    psm.End = target;
                }

                physicalPosition++;
                logicalPosition = positionMapping.get(physicalPosition);
                column = logicalPosition != null && columnMapping.ContainsKey(logicalPosition)
                             ? columnMapping[logicalPosition]
                             : null;

                if (column == null){
                    break;
                }
            }

            return physicalPosition;
        }

        private string checkAccession(MZTabColumn column, string target){
            return checkData(column, target, true);
        }

        public PSM getRecord(string line){
            return (PSM) getRecord(Section.PSM, line);
        }

        /**
     * For proteins and peptides modifications SHOULD be reported using either UNIMOD or PSI-MOD accessions.
     * As these two ontologies are not applicable to small molecules, so-called CHEMMODs can also be defined.
     */

        protected SplitList<Modification> checkModifications(MZTabColumn column, string sequence, string target){
            SplitList<Modification> modificationList = checkModifications(_section, column, target);

            int terminal_position = sequence.Length + 1;
            foreach (Modification mod in modificationList){
                foreach (int position in mod.PositionMap.Keys){
                    if (position > terminal_position || position < 0){
                        _errorList.Add(new MZTabError(LogicalErrorType.ModificationPosition, _lineNumber, column.Header,
                                                      mod.ToString(), sequence));
                        return null;
                    }
                }

                if (mod.Type == Modification.ModificationType.CHEMMOD){
                    _errorList.Add(new MZTabError(LogicalErrorType.CHEMMODS, _lineNumber, column.Header, mod.ToString()));
                }
            }

            return modificationList;
        }
    }
}