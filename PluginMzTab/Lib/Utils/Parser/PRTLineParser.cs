using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using PluginMzTab.Lib.Model;
using PluginMzTab.Lib.Utils.Errors;

namespace PluginMzTab.Lib.Utils.Parser{
    public class PRTLineParser : MZTabDataLineParser{
        private readonly HashSet<string> accessionSet = new HashSet<string>();

        public PRTLineParser(MZTabColumnFactory factory, PositionMapping positionMapping, Metadata metadata,
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
                    if (columnName.Equals(ProteinColumn.ACCESSION.Name)){
                        checkAccession(column, target);
                    }
                    else if (columnName.Equals(ProteinColumn.DESCRIPTION.Name)){
                        checkDescription(column, target);
                    }
                    else if (columnName.Equals(ProteinColumn.TAXID.Name)){
                        checkTaxid(column, target);
                    }
                    else if (columnName.Equals(ProteinColumn.SPECIES.Name)){
                        checkSpecies(column, target);
                    }
                    else if (columnName.Equals(ProteinColumn.DATABASE.Name)){
                        checkDatabase(column, target);
                    }
                    else if (columnName.Equals(ProteinColumn.DATABASE_VERSION.Name)){
                        checkDatabaseVersion(column, target);
                    }
                    else if (columnName.Equals(ProteinColumn.SEARCH_ENGINE.Name)){
                        checkSearchEngine(column, target);
                    }
                    else if (columnName.Equals(ProteinColumn.BEST_SEARCH_ENGINE_SCORE.Name)){
                        checkBestSearchEngineScore(column, target);
                    }
                    else if (columnName.Equals(ProteinColumn.SEARCH_ENGINE_SCORE.Name)){
                        checkSearchEngineScore(column, target);
                    }
                    else if (columnName.Equals(ProteinColumn.RELIABILITY.Name)){
                        checkReliability(column, target);
                    }
                    else if (columnName.Equals(ProteinColumn.NUM_PSMS.Name)){
                        checkNumPSMs(column, target);
                    }
                    else if (columnName.Equals(ProteinColumn.NUM_PEPTIDES_DISTINCT.Name)){
                        checkNumPeptidesDistinct(column, target);
                    }
                    else if (columnName.Equals(ProteinColumn.NUM_PEPTIDES_UNIQUE.Name)){
                        checkNumPeptidesUnique(column, target);
                    }
                    else if (columnName.Equals(ProteinColumn.AMBIGUITY_MEMBERS.Name)){
                        checkAmbiguityMembers(column, target);
                    }
                    else if (columnName.Equals(ProteinColumn.MODIFICATIONS.Name)){
                        checkModifications(column, target);
                    }
                    else if (columnName.Equals(ProteinColumn.URI.Name)){
                        checkURI(column, target);
                    }
                    else if (columnName.Equals(ProteinColumn.GO_TERMS.Name)){
                        checkGOTerms(column, target);
                    }
                    else if (columnName.Equals(ProteinColumn.PROTEIN_COVERAGE.Name)){
                        checkProteinCoverage(column, target);
                    }
                }
            }
        }

        protected override int loadStableData(MZTabRecord record, string line){
            _items = line.Split(new[]{MZTabConstants.TAB}, StringSplitOptions.None);
            _items[_items.Length - 1] = _items[_items.Length - 1].Trim();

            Protein protein = (Protein) record;
            MZTabColumn column = null;
            SortedDictionary<string, MZTabColumn> columnMapping = factory.ColumnMapping;
            int physicalPosition = 1;

            string logicalPosition = positionMapping.get(physicalPosition);
            if (columnMapping.ContainsKey(logicalPosition)){
                column = columnMapping[logicalPosition];
            }
            while (column != null && column is ProteinColumn){
                string target = _items[physicalPosition].Trim();
                string columnName = column.Name;
                if (columnName.Equals(ProteinColumn.ACCESSION.Name)){
                    protein.Accession = target;
                }
                else if (columnName.Equals(ProteinColumn.DESCRIPTION.Name)){
                    protein.Description = target;
                }
                else if (columnName.Equals(ProteinColumn.TAXID.Name)){
                    protein.SetTaxid(target);
                }
                else if (columnName.Equals(ProteinColumn.SPECIES.Name)){
                    protein.Species = target;
                }
                else if (columnName.Equals(ProteinColumn.DATABASE.Name)){
                    protein.Database = target;
                }
                else if (columnName.Equals(ProteinColumn.DATABASE_VERSION.Name)){
                    protein.DatabaseVersion = target;
                }
                else if (columnName.Equals(ProteinColumn.SEARCH_ENGINE.Name)){
                    protein.SetSearchEngine(target);
                }
                else if (columnName.Equals(ProteinColumn.BEST_SEARCH_ENGINE_SCORE.Name)){
                    protein.SetBestSearchEngineScore(target);
                }
                else if (columnName.Equals(ProteinColumn.SEARCH_ENGINE_SCORE.Name)){
                    protein.setSearchEngineScore(logicalPosition, target);
                }
                else if (columnName.Equals(ProteinColumn.RELIABILITY.Name)){
                    protein.SetReliability(target);
                }
                else if (columnName.Equals(ProteinColumn.NUM_PSMS.Name)){
                    protein.setNumPSMs(logicalPosition, target);
                }
                else if (columnName.Equals(ProteinColumn.NUM_PEPTIDES_DISTINCT.Name)){
                    protein.setNumPeptidesDistinct(logicalPosition, target);
                }
                else if (columnName.Equals(ProteinColumn.NUM_PEPTIDES_UNIQUE.Name)){
                    protein.setNumPeptidesUnique(logicalPosition, target);
                }
                else if (columnName.Equals(ProteinColumn.AMBIGUITY_MEMBERS.Name)){
                    protein.SetAmbiguityMembers(target);
                }
                else if (columnName.Equals(ProteinColumn.MODIFICATIONS.Name)){
                    protein.SetModifications(target);
                }
                else if (columnName.Equals(ProteinColumn.URI.Name)){
                    protein.SetURI(target);
                }
                else if (columnName.Equals(ProteinColumn.GO_TERMS.Name)){
                    protein.SetGOTerms(target);
                }
                else if (columnName.Equals(ProteinColumn.PROTEIN_COVERAGE.Name)){
                    protein.SetProteinCoverage(target);
                }

                physicalPosition++;
                logicalPosition = positionMapping.get(physicalPosition);
                column = logicalPosition == null
                             ? null
                             : columnMapping.ContainsKey(logicalPosition) ? columnMapping[logicalPosition] : null;

                if (column == null){
                    break;
                }
            }

            return physicalPosition;
        }

        public Protein getRecord(string line){
            return (Protein) getRecord(Section.Protein, line);
        }

        /**
     * accession should not null.
     * accession MUST be unique, otherwise raise {@link LogicalErrorType#DuplicationAccession} error.
     *
     * If check error return null, else return accession string.
     */

        protected string checkAccession(MZTabColumn column, string accession){
            string result_accession = checkData(column, accession, false);

            if (result_accession == null){
                return result_accession;
            }

            if (! accessionSet.Add(result_accession)){
                _errorList.Add(new MZTabError(LogicalErrorType.DuplicationAccession, _lineNumber, column.Header,
                                              result_accession));
                return null;
            }

            return result_accession;
        }

        /**
     * For proteins and peptides modifications SHOULD be reported using either UNIMOD or PSI-MOD accessions.
     * As these two ontologies are not applicable to small molecules, so-called CHEMMODs can also be defined.
     *
     * Ambiguity of modification position MUST NOT be reported at the Protein level.
     */

        protected SplitList<Modification> checkModifications(MZTabColumn column, string target){
            SplitList<Modification> modificationList = checkModifications(_section, column, target);

            foreach (Modification mod in modificationList){
                if (mod.PositionMap.Count > 1){
                    // this is warn
                    _errorList.Add(new MZTabError(LogicalErrorType.AmbiguityMod, _lineNumber, column.Header,
                                                  mod.ToString()));
                }

                if (mod.Type == Modification.ModificationType.CHEMMOD){
                    // this is warn
                    _errorList.Add(new MZTabError(LogicalErrorType.CHEMMODS, _lineNumber, column.Header, mod.ToString()));
                }

                if (mod.Type == Modification.ModificationType.SUBST &&
                    parseSubstitutionIdentifier(mod.Accession) != null){
                    _errorList.Add(new MZTabError(LogicalErrorType.SubstituteIdentifier, _lineNumber, column.Header,
                                                  mod.ToString()));
                    return null;
                }
            }

            return modificationList;
        }

        /**
     * In SUBST cases, the "sequence" column MUST contain the original, unaltered sequence.
     */

        private string parseSubstitutionIdentifier(string identifier){
            identifier = MZTabUtils.ParseString(identifier);
            if (identifier == null){
                return null;
            }

            Regex regex = new Regex("\"[^BJOUXZ]+\"");
            Match match = regex.Match(identifier);

            if (match.Success && match.Index == 0 && match.Length == identifier.Length){
                return identifier;
            }
            return null;
        }
    }
}