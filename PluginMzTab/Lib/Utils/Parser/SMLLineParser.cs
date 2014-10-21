using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using PluginMzTab.Lib.Model;
using PluginMzTab.Lib.Utils.Errors;

namespace PluginMzTab.Lib.Utils.Parser{
    public class SMLLineParser : MZTabDataLineParser{
        public SMLLineParser(MZTabColumnFactory factory, PositionMapping positionMapping,
                             Metadata metadata, MZTabErrorList errorList)
            : base(factory, positionMapping, metadata, errorList){}

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
                    if (columnName.Equals(SmallMoleculeColumn.IDENTIFIER.Name)){
                        checkIdentifier(column, target);
                    }
                    else if (columnName.Equals(SmallMoleculeColumn.CHEMICAL_FORMULA.Name)){
                        checkChemicalFormula(column, target);
                    }
                    else if (columnName.Equals(SmallMoleculeColumn.SMILES.Name)){
                        checkSmiles(column, target);
                    }
                    else if (columnName.Equals(SmallMoleculeColumn.INCHI_KEY.Name)){
                        checkInchiKey(column, target);
                    }
                    else if (columnName.Equals(SmallMoleculeColumn.DESCRIPTION.Name)){
                        checkDescription(column, target);
                    }
                    else if (columnName.Equals(SmallMoleculeColumn.EXP_MASS_TO_CHARGE.Name)){
                        checkExpMassToCharge(column, target);
                    }
                    else if (columnName.Equals(SmallMoleculeColumn.CALC_MASS_TO_CHARGE.Name)){
                        checkCalcMassToCharge(column, target);
                    }
                    else if (columnName.Equals(SmallMoleculeColumn.CHARGE.Name)){
                        checkCharge(column, target);
                    }
                    else if (columnName.Equals(SmallMoleculeColumn.RETENTION_TIME.Name)){
                        checkRetentionTime(column, target);
                    }
                    else if (columnName.Equals(SmallMoleculeColumn.TAXID.Name)){
                        checkTaxid(column, target);
                    }
                    else if (columnName.Equals(SmallMoleculeColumn.SPECIES.Name)){
                        checkSpecies(column, target);
                    }
                    else if (columnName.Equals(SmallMoleculeColumn.DATABASE.Name)){
                        checkDatabase(column, target);
                    }
                    else if (columnName.Equals(SmallMoleculeColumn.DATABASE_VERSION.Name)){
                        checkDatabaseVersion(column, target);
                    }
                    else if (columnName.Equals(SmallMoleculeColumn.RELIABILITY.Name)){
                        checkReliability(column, target);
                    }
                    else if (columnName.Equals(SmallMoleculeColumn.URI.Name)){
                        checkURI(column, target);
                    }
                    else if (columnName.Equals(SmallMoleculeColumn.SPECTRA_REF.Name)){
                        checkSpectraRef(column, target);
                    }
                    else if (columnName.Equals(SmallMoleculeColumn.SEARCH_ENGINE.Name)){
                        checkSearchEngine(column, target);
                    }
                    else if (columnName.Equals(SmallMoleculeColumn.BEST_SEARCH_ENGINE_SCORE.Name)){
                        checkBestSearchEngineScore(column, target);
                    }
                    else if (columnName.Equals(SmallMoleculeColumn.SEARCH_ENGINE_SCORE.Name)){
                        checkSearchEngineScore(column, target);
                    }
                    else if (columnName.Equals(SmallMoleculeColumn.MODIFICATIONS.Name)){
                        checkModifications(column, target);
                    }
                }
            }
        }

        protected override int loadStableData(MZTabRecord record, string line){
            _items = line.Split(new[]{MZTabConstants.TAB}, StringSplitOptions.None);
            _items[_items.Length - 1] = _items[_items.Length - 1].Trim();

            SmallMolecule smallMolecule = (SmallMolecule) record;

            SortedDictionary<string, MZTabColumn> columnMapping = factory.ColumnMapping;
            int physicalPosition = 1;

            string logicalPosition = positionMapping.get(physicalPosition);

            MZTabColumn column = null;
            if (columnMapping.ContainsKey(logicalPosition)){
                column = columnMapping[logicalPosition];
            }

            while (column != null && column is SmallMoleculeColumn){
                string target = _items[physicalPosition];
                string columnName = column.Name;
                if (columnName.Equals(SmallMoleculeColumn.IDENTIFIER.Name)){
                    smallMolecule.setIdentifier(target);
                }
                else if (columnName.Equals(SmallMoleculeColumn.CHEMICAL_FORMULA.Name)){
                    smallMolecule.ChemicalFormula = target;
                }
                else if (columnName.Equals(SmallMoleculeColumn.SMILES.Name)){
                    smallMolecule.Smiles = target;
                }
                else if (columnName.Equals(SmallMoleculeColumn.INCHI_KEY.Name)){
                    smallMolecule.InchiKey = target;
                }
                else if (columnName.Equals(SmallMoleculeColumn.DESCRIPTION.Name)){
                    smallMolecule.Description = target;
                }
                else if (columnName.Equals(SmallMoleculeColumn.EXP_MASS_TO_CHARGE.Name)){
                    smallMolecule.setExpMassToCharge(target);
                }
                else if (columnName.Equals(SmallMoleculeColumn.CALC_MASS_TO_CHARGE.Name)){
                    smallMolecule.setCalcMassToCharge(target);
                }
                else if (columnName.Equals(SmallMoleculeColumn.CHARGE.Name)){
                    smallMolecule.setCharge(target);
                }
                else if (columnName.Equals(SmallMoleculeColumn.RETENTION_TIME.Name)){
                    smallMolecule.setRetentionTime(target);
                }
                else if (columnName.Equals(SmallMoleculeColumn.TAXID.Name)){
                    smallMolecule.setTaxid(target);
                }
                else if (columnName.Equals(SmallMoleculeColumn.SPECIES.Name)){
                    smallMolecule.Species = target;
                }
                else if (columnName.Equals(SmallMoleculeColumn.DATABASE.Name)){
                    smallMolecule.Database = target;
                }
                else if (columnName.Equals(SmallMoleculeColumn.DATABASE_VERSION.Name)){
                    smallMolecule.DatabaseVersion = target;
                }
                else if (columnName.Equals(SmallMoleculeColumn.RELIABILITY.Name)){
                    smallMolecule.setReliability(target);
                }
                else if (columnName.Equals(SmallMoleculeColumn.URI.Name)){
                    smallMolecule.setURI(target);
                }
                else if (columnName.Equals(SmallMoleculeColumn.SPECTRA_REF.Name)){
                    smallMolecule.setSpectraRef(target);
                }
                else if (columnName.Equals(SmallMoleculeColumn.SEARCH_ENGINE.Name)){
                    smallMolecule.setSearchEngine(target);
                }
                else if (columnName.Equals(SmallMoleculeColumn.BEST_SEARCH_ENGINE_SCORE.Name)){
                    smallMolecule.setBestSearchEngineScore(target);
                }
                else if (columnName.Equals(SmallMoleculeColumn.SEARCH_ENGINE_SCORE.Name)){
                    smallMolecule.setSearchEngineScore(logicalPosition, target);
                }
                else if (columnName.Equals(SmallMoleculeColumn.MODIFICATIONS.Name)){
                    smallMolecule.setModifications(target);
                }

                physicalPosition++;
                logicalPosition = positionMapping.get(physicalPosition);
                column = logicalPosition != null && columnMapping.ContainsKey(logicalPosition)
                             ? columnMapping[logicalPosition]
                             : null;
            }

            return physicalPosition;
        }

        public SmallMolecule getRecord(string line){
            return (SmallMolecule) getRecord(Section.Small_Molecule, line);
        }

        /**
         * As these two ontologies are not applicable to small molecules, so-called CHEMMODs can also be defined.
         * CHEMMODs MUST NOT be used if the modification can be reported using a PSI-MOD or UNIMOD accession.
         * Mass deltas MUST NOT be used for CHEMMODs if the delta can be expressed through a known chemical formula .
         */

        protected SplitList<Modification> checkModifications(MZTabColumn column, string target){
            SplitList<Modification> modificationList = base.checkModifications(_section, column, target);

            foreach (Modification mod in modificationList){
                if (mod.Type == Modification.ModificationType.CHEMMOD){
                    if (target.Contains("-MOD:") || target.Contains("-UNIMOD:")){
                        _errorList.Add(new MZTabError(LogicalErrorType.CHEMMODS, _lineNumber, column.Header,
                                                      mod.ToString()));
                    }

                    if (parseChemmodAccession(mod.Accession) == null){
                        _errorList.Add(new MZTabError(FormatErrorType.CHEMMODSAccession, _lineNumber, column.Header,
                                                      mod.ToString()));
                        return null;
                    }
                }
            }

            return modificationList;
        }

        private string parseChemmodAccession(string accession){
            accession = MZTabUtils.ParseString(accession);

            Regex regex = new Regex("[+-](\\d+(.\\d+)?)?|(([A-Z][a-z]*)(\\d*))?");
            Match match = regex.Match(accession);

            if (match.Success){
                return accession;
            }
            return null;
        }
    }
}