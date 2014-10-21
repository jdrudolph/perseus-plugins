using System;
using System.Collections.Generic;
using PluginMzTab.Lib.Model;
using PluginMzTab.Lib.Utils.Errors;

namespace PluginMzTab.Lib.Utils.Parser{
    public class PEPLineParser : MZTabDataLineParser{
        public PEPLineParser(MZTabColumnFactory factory, PositionMapping positionMapping, Metadata metadata,
                             MZTabErrorList errorList) : base(factory, positionMapping, metadata, errorList){}

        protected override void checkStableData(){
            for (int physicalPosition = 1; physicalPosition < _items.Length; physicalPosition++){
                MZTabColumn column = null;
                string key = positionMapping.get(physicalPosition);
                if (factory.ColumnMapping.ContainsKey(key)){
                    column = factory.ColumnMapping[key];
                }
                if (column != null){
                    string columnName = column.Name;
                    string target = _items[physicalPosition];
                    if (columnName.Equals(PeptideColumn.SEQUENCE.Name)){
                        checkSequence(column, target);
                    }
                    else if (columnName.Equals(PeptideColumn.ACCESSION.Name)){
                        checkAccession(column, target);
                    }
                    else if (columnName.Equals(PeptideColumn.UNIQUE.Name)){
                        checkUnique(column, target);
                    }
                    else if (columnName.Equals(PeptideColumn.DATABASE.Name)){
                        checkDatabase(column, target);
                    }
                    else if (columnName.Equals(PeptideColumn.DATABASE_VERSION.Name)){
                        checkDatabaseVersion(column, target);
                    }
                    else if (columnName.Equals(PeptideColumn.SEARCH_ENGINE.Name)){
                        checkSearchEngine(column, target);
                    }
                    else if (columnName.Equals(PeptideColumn.BEST_SEARCH_ENGINE_SCORE.Name)){
                        checkBestSearchEngineScore(column, target);
                    }
                    else if (columnName.Equals(PeptideColumn.SEARCH_ENGINE_SCORE.Name)){
                        checkSearchEngineScore(column, target);
                    }
                    else if (columnName.Equals(PeptideColumn.RELIABILITY.Name)){
                        checkReliability(column, target);
                    }
                    else if (columnName.Equals(PeptideColumn.MODIFICATIONS.Name)){
                        string sequence = null;
                        if (exchangeMapping.ContainsKey(PeptideColumn.SEQUENCE.LogicPosition)){
                            sequence = _items[exchangeMapping[PeptideColumn.SEQUENCE.LogicPosition]];
                        }
                        checkModifications(column, sequence, target);
                    }
                    else if (columnName.Equals(PeptideColumn.RETENTION_TIME.Name)){
                        checkRetentionTime(column, target);
                    }
                    else if (columnName.Equals(PeptideColumn.RETENTION_TIME_WINDOW.Name)){
                        checkRetentionTimeWindow(column, target);
                    }
                    else if (columnName.Equals(PeptideColumn.CHARGE.Name)){
                        checkCharge(column, target);
                    }
                    else if (columnName.Equals(PeptideColumn.MASS_TO_CHARGE.Name)){
                        checkMassToCharge(column, target);
                    }
                    else if (columnName.Equals(PeptideColumn.URI.Name)){
                        checkURI(column, target);
                    }
                    else if (columnName.Equals(PeptideColumn.SPECTRA_REF.Name)){
                        checkSpectraRef(column, target);
                    }
                }
            }
        }

        protected override int loadStableData(MZTabRecord record, string line){
            _items = line.Split(new[]{MZTabConstants.TAB}, StringSplitOptions.None);
            _items[_items.Length - 1] = _items[_items.Length - 1].Trim();

            Peptide peptide = (Peptide) record;
            SortedDictionary<string, MZTabColumn> columnMapping = factory.ColumnMapping;
            int physicalPosition = 1;

            string logicalPosition = positionMapping.get(physicalPosition);
            MZTabColumn column = null;
            if (columnMapping.ContainsKey(logicalPosition)){
                column = columnMapping[logicalPosition];
            }
            while (column != null && column is PeptideColumn){
                string target = _items[physicalPosition];
                string columnName = column.Name;
                if (columnName.Equals(PeptideColumn.SEQUENCE.Name)){
                    peptide.Sequence = target;
                }
                else if (columnName.Equals(PeptideColumn.ACCESSION.Name)){
                    peptide.Accession = target;
                }
                else if (columnName.Equals(PeptideColumn.UNIQUE.Name)){
                    peptide.SetUnique(target);
                }
                else if (columnName.Equals(PeptideColumn.DATABASE.Name)){
                    peptide.Database = target;
                }
                else if (columnName.Equals(PeptideColumn.DATABASE_VERSION.Name)){
                    peptide.DatabaseVersion = target;
                }
                else if (columnName.Equals(PeptideColumn.SEARCH_ENGINE.Name)){
                    peptide.SetSearchEngine(target);
                }
                else if (columnName.Equals(PeptideColumn.BEST_SEARCH_ENGINE_SCORE.Name)){
                    peptide.SetBestSearchEngineScore(target);
                }
                else if (columnName.Equals(PeptideColumn.SEARCH_ENGINE_SCORE.Name)){
                    peptide.SetSearchEngineScore(logicalPosition, target);
                }
                else if (columnName.Equals(PeptideColumn.RELIABILITY.Name)){
                    peptide.SetReliability(target);
                }
                else if (columnName.Equals(PeptideColumn.MODIFICATIONS.Name)){
                    peptide.SetModifications(target);
                }
                else if (columnName.Equals(PeptideColumn.RETENTION_TIME.Name)){
                    peptide.SetRetentionTime(target);
                }
                else if (columnName.Equals(PeptideColumn.RETENTION_TIME_WINDOW.Name)){
                    peptide.SetRetentionTimeWindow(target);
                }
                else if (columnName.Equals(PeptideColumn.CHARGE.Name)){
                    peptide.SetCharge(target);
                }
                else if (columnName.Equals(PeptideColumn.MASS_TO_CHARGE.Name)){
                    peptide.SetMassToCharge(target);
                }
                else if (columnName.Equals(PeptideColumn.URI.Name)){
                    peptide.SetURI(target);
                }
                else if (columnName.Equals(PeptideColumn.SPECTRA_REF.Name)){
                    peptide.SetSpectraRef(target);
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

        public Peptide getRecord(string line){
            return (Peptide) getRecord(Section.Peptide, line);
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
                    // this is warn
                    _errorList.Add(new MZTabError(LogicalErrorType.CHEMMODS, _lineNumber, column.Header, mod.ToString()));
                }
            }

            return modificationList;
        }
    }
}