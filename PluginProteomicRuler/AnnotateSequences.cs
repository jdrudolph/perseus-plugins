using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using BaseLib.Param;
using BaseLibS.Param;
using BaseLibS.Util;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;

namespace PluginProteomicRuler
{
    public class AnnotateSequences : IMatrixProcessing
    {
        public bool HasButton { get { return false; } }

        public Bitmap DisplayImage { get { return null; } }

        public string Description
        {
            get { return "Annotate proteins using information extracted from a (uniprot) fasta file.\n" +
                "This plugin extracts gene names, protein names, entry names, species names from uniprot fasta headers.\n" +
                "It calculates the length of the amino acid sequences, monoisotopic and average molecular masses, optionally for the leading IDs or as the median of all IDs.\n" +
                "Finally, the numbers of theoretical peptides can be calculated for a range of proteases.\n"; }
        }

        public string HelpOutput { get { return "A series of categorical or numeric annotation columns are added depending on the user selection."; } }
        public DocumentType HelpDescriptionType { get { return DocumentType.PlainText; } }
        public DocumentType HelpOutputType { get { return DocumentType.PlainText; } }
        public DocumentType[] HelpSupplTablesType { get { return new DocumentType[0]; } }
        public string[] HelpSupplTables { get { return new string[0]; } }
        public int NumSupplTables { get { return 0; } }
        public string Name { get { return "Annotate proteins (fasta headers, sequence features, ...)"; } }
        public string Heading { get { return "Proteomic ruler"; } }
        public bool IsActive { get { return true; } }
        public float DisplayRank { get { return 0; } }
        public string[] HelpDocuments { get { return new string[0]; } }
        public DocumentType[] HelpDocumentTypes { get { return new DocumentType[0]; } }
        public int NumDocuments { get { return 0; } }
        public int GetMaxThreads(Parameters parameters) { return 1; }
        public string Url { get { return "http://141.61.102.17/perseus_doku/doku.php?id=perseus:plugins:proteomicruler:annotateproteins"; } }

        public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables,
                                ref IDocumentData[] documents, ProcessInfo processInfo)
        {

            int proteinIdColumnInd = param.GetSingleChoiceParam("Protein IDs").Value;
            string[][] proteinIds = new string[mdata.RowCount][];
            string[][] leadingIds = new string[mdata.RowCount][];
            List<string> allIds = new List<string>();
            for (int row = 0; row < mdata.RowCount; row++)
            {
                proteinIds[row] = mdata.StringColumns[proteinIdColumnInd][row].Split(new char[] {';'});
                leadingIds[row] = new []{proteinIds[row][0]};
                allIds.AddRange(proteinIds[row]);
            }

            string fastaFilePath = param.GetFileParam("Fasta file").Value;
            Fasta fasta = new Fasta();
            fasta.ParseFile(fastaFilePath, processInfo);


            // Text annotations
            processInfo.Status("Adding fasta header annotations.");
            int[] selection = param.GetSingleChoiceWithSubParams("Fasta header annotations").GetSubParameters().GetMultiChoiceParam("Annotations").Value;
            string[][] idsToBeAnnotated = (param.GetSingleChoiceWithSubParams("Fasta header annotations").Value == 0) ? proteinIds : leadingIds;

            ProteinSequence[][] fastaEntries = new ProteinSequence[mdata.RowCount][];
            for (int row = 0; row < mdata.RowCount; row++)
            {
                List<ProteinSequence> rowEntries = new List<ProteinSequence>();
                foreach (string id in idsToBeAnnotated[row])
                {
                    ProteinSequence entry = fasta.GetEntry(id);
                    if (entry == null) { continue; }
                    rowEntries.Add(entry);
                }
                fastaEntries[row] = rowEntries.ToArray();
            }

            
            if (ArrayUtils.Contains(selection,0)) // Entry name
            {
                string[] annotationColumn = new string[mdata.RowCount];
                for (int row = 0; row < mdata.RowCount; row++)
                {
                    List<string> rowAnnotations = new List<string>();
                    foreach (ProteinSequence entry in fastaEntries[row])
                    {
                        string entryName = entry.GetEntryName();
                        if (entryName != null && !ArrayUtils.Contains(rowAnnotations, entryName))
                            rowAnnotations.Add(entryName);
                    }
                    annotationColumn[row] = String.Join(";",rowAnnotations.ToArray());
                }
                mdata.AddStringColumn("Entry name","",annotationColumn);
            }

            if (ArrayUtils.Contains(selection, 1)) // Gene name
            {
                string[] annotationColumn = new string[mdata.RowCount];
                for (int row = 0; row < mdata.RowCount; row++)
                {
                    List<string> rowAnnotations = new List<string>();
                    foreach (ProteinSequence entry in fastaEntries[row])
                    {
                        string geneName = entry.GetGeneName();
                        if (geneName != null && !ArrayUtils.Contains(rowAnnotations,geneName))
                            rowAnnotations.Add(geneName);
                    }
                    annotationColumn[row] = String.Join(";", rowAnnotations.ToArray());
                }
                mdata.AddStringColumn("Gene name", "", annotationColumn);
            }


            if (ArrayUtils.Contains(selection, 2)) // Verbose protein name, i.e. all protein names annotated in all fasta headers, including the 'Isoform x of...' prefixes and '(Fragment)' suffixes
            {
                string[] annotationColumn = new string[mdata.RowCount];
                for (int row = 0; row < mdata.RowCount; row++)
                {
                    List<string> rowAnnotations = new List<string>();
                    foreach (ProteinSequence entry in fastaEntries[row])
                    {
                        string proteinName = entry.GetProteinName();
                        if (proteinName != null && !ArrayUtils.Contains(rowAnnotations, proteinName))
                            rowAnnotations.Add(proteinName);
                    }
                    annotationColumn[row] = String.Join(";", rowAnnotations.ToArray());
                }
                mdata.AddStringColumn("Protein name (verbose)", "", annotationColumn);   
            }

            if (ArrayUtils.Contains(selection, 3)) // Consensus protein name
            {
                string[] annotationColumn = new string[mdata.RowCount];
                for (int row = 0; row < mdata.RowCount; row++)
                {
                    List<string> rowAnnotations = new List<string>();
                    foreach (ProteinSequence entry in fastaEntries[row])
                    {
                        string proteinName = entry.GetConsensusProteinName();
                        if (proteinName != null && !ArrayUtils.Contains(rowAnnotations, proteinName))
                            rowAnnotations.Add(proteinName);
                    }
                    annotationColumn[row] = String.Join(";", rowAnnotations.ToArray());
                }
                mdata.AddStringColumn("Protein name", "", annotationColumn);
            }

            if (ArrayUtils.Contains(selection, 4)) // Species
            {
                string[] annotationColumn = new string[mdata.RowCount];
                for (int row = 0; row < mdata.RowCount; row++)
                {
                    List<string> rowAnnotations = new List<string>();
                    foreach (ProteinSequence entry in fastaEntries[row])
                    {
                        string speciesName = entry.GetSpecies();
                        if (speciesName != null && !ArrayUtils.Contains(rowAnnotations, speciesName))
                            rowAnnotations.Add(speciesName);
                    }
                    annotationColumn[row] = String.Join(";", rowAnnotations.ToArray());
                }
                mdata.AddStringColumn("Species", "", annotationColumn);
            }


            // Numeric annotations
            processInfo.Status("Adding numeric annotations.");
            selection = param.GetSingleChoiceWithSubParams("Numeric annotations").GetSubParameters().GetMultiChoiceParam("Annotations").Value;
            bool annotateLeadingId = (param.GetSingleChoiceWithSubParams("Numeric annotations").Value == 1);

            if (ArrayUtils.Contains(selection, 0)) // Sequence length
            {
                double[] annotationColumn = new double[mdata.RowCount];
                for (int row = 0; row < mdata.RowCount; row++)
                {
                    List<double> rowAnnotations = new List<double>();
                    foreach (ProteinSequence entry in fastaEntries[row])
                    {
                        double sequenceLength = entry.GetSequence().Length;
                        rowAnnotations.Add(sequenceLength);
                        if (annotateLeadingId && rowAnnotations.Count > 0)
                            break;
                    }
                    annotationColumn[row] = ArrayUtils.Median(rowAnnotations.ToArray());
                }
                mdata.AddNumericColumn("Sequence length", "", annotationColumn);
            }

            if (ArrayUtils.Contains(selection, 1)) // Monoisotopic molecular mass
            {
                double[] annotationColumn = new double[mdata.RowCount];
                for (int row = 0; row < mdata.RowCount; row++)
                {
                    List<double> rowAnnotations = new List<double>();
                    foreach (ProteinSequence entry in fastaEntries[row])
                    {
                        double monoisotopicMass = entry.GetMonoisotopicMolecularMass();
                        rowAnnotations.Add(monoisotopicMass);
                        if (annotateLeadingId && rowAnnotations.Count > 0)
                            break;
                    }
                    annotationColumn[row] = ArrayUtils.Median(rowAnnotations.ToArray());
                }
                mdata.AddNumericColumn("Monoisotopic molecular mass", "", annotationColumn);
            }

            if (ArrayUtils.Contains(selection, 2)) // Average molecular mass
            {
                double[] annotationColumn = new double[mdata.RowCount];
                for (int row = 0; row < mdata.RowCount; row++)
                {
                    List<double> rowAnnotations = new List<double>();
                    foreach (ProteinSequence entry in fastaEntries[row])
                    {
                        double averageMass = entry.GetAverageMolecularMass();
                        rowAnnotations.Add(averageMass);
                        if (annotateLeadingId && rowAnnotations.Count > 0)
                            break;
                    }
                    annotationColumn[row] = ArrayUtils.Median(rowAnnotations.ToArray());
                }
                mdata.AddNumericColumn("Average molecular mass", "", annotationColumn);
            }


            // Theoretical peptides
            processInfo.Status("Calculating theoretical peptides.");
            annotateLeadingId = (param.GetSingleChoiceWithSubParams("Calculate theoretical peptides").Value == 1);
            Protease[] proteases = ArrayUtils.SubArray(Constants.DefaultProteases, param.GetSingleChoiceWithSubParams("Calculate theoretical peptides").GetSubParameters().GetMultiChoiceParam("Proteases").Value);
            double minLength = param.GetSingleChoiceWithSubParams("Calculate theoretical peptides").GetSubParameters().GetDoubleParam("Min. peptide length").Value;
            double maxLength = param.GetSingleChoiceWithSubParams("Calculate theoretical peptides").GetSubParameters().GetDoubleParam("Max. peptide length").Value;
            bool displayPeptideSequences = annotateLeadingId && param.GetSingleChoiceWithSubParams("Calculate theoretical peptides").GetSubParameters().GetBoolParam("Show sequences").Value;

            foreach (Protease protease in proteases)
            {
                double[] annotationColumn = new double[mdata.RowCount];
                string[] peptideColumn = new string[mdata.RowCount];
                for (int row = 0; row < mdata.RowCount; row++)
                {
                    List<double> rowAnnotations = new List<double>();
                    List<string> rowPeptides = new List<string>();
                    foreach (ProteinSequence entry in fastaEntries[row])
                    {
                        double nTheoreticalPeptides = entry.GetNumberOfTheoreticalPeptides(protease, (int) minLength, (int) maxLength);
                        rowAnnotations.Add(nTheoreticalPeptides);
                        if (displayPeptideSequences)
                            rowPeptides.AddRange(entry.GetTheoreticalPeptideSequences(protease, (int) minLength, (int) maxLength));
                        if (annotateLeadingId && rowAnnotations.Count > 0)
                            break;
                    }
                    annotationColumn[row] = ArrayUtils.Median(rowAnnotations.ToArray());
                    peptideColumn[row] = String.Join(";", rowPeptides);
                }
                mdata.AddNumericColumn("Number of theoretical peptides (" + protease.Name + ", " + minLength + "-" + maxLength + ")", "", annotationColumn);
                if (displayPeptideSequences)
                    mdata.AddStringColumn("Theoretical peptide sequences (" + protease.Name + ", " + minLength + "-" + maxLength + ")", "", peptideColumn);
            }


            // Sequence features
            processInfo.Status("Counting sequence features.");
            annotateLeadingId = (param.GetSingleChoiceWithSubParams("Count sequence features").Value == 1);
            bool normalizeBySequenceLength = param.GetSingleChoiceWithSubParams("Count sequence features").GetSubParameters().GetBoolParam("Normalize by sequence length").Value;

            if (
                param.GetSingleChoiceWithSubParams("Count sequence features")
                     .GetSubParameters()
                     .GetStringParam("Regex")
                     .Value != "")
            {

                Regex regex = new Regex("^.*$");
                try
                {
                    regex =
                        new Regex(
                            param.GetSingleChoiceWithSubParams("Count sequence features")
                                 .GetSubParameters()
                                 .GetStringParam("Regex")
                                 .Value);
                }
                catch (ArgumentException)
                {
                    processInfo.ErrString = "The regular expression you provided has invalid syntax.";
                    return;
                }

                double[] sequenceFeatureColumn = new double[mdata.RowCount];
                for (int row = 0; row < mdata.RowCount; row++)
                {
                    List<double> featureCount = new List<double>();
                    foreach (ProteinSequence entry in fastaEntries[row])
                    {
                        double nFeatures = regex.Matches(entry.GetSequence()).Count;
                        featureCount.Add(normalizeBySequenceLength ? nFeatures/entry.GetLength() : nFeatures);
                        if (annotateLeadingId)
                            break;
                    }
                    sequenceFeatureColumn[row] = ArrayUtils.Median(featureCount.ToArray());
                }
                mdata.AddNumericColumn((normalizeBySequenceLength ? "Normalized feature count (" : "Feature count (") + regex.ToString() + ")","",sequenceFeatureColumn);
            }


            processInfo.Status("Done.");

        }




        public Parameters GetParameters(IMatrixData mdata, ref string errorString)
        {
            return
                new Parameters(new Parameter[]
                    {
                        new SingleChoiceParam("Protein IDs")
                            {
                                Help = "Specify the column containing the protein IDs",
                                Values = mdata.StringColumnNames,
                                Value =
                                    ProteomicRulerUtils.Match(mdata.StringColumnNames.ToArray(), new[] {"majority"}, false, true, true)[0],
                            },
                        new FileParam("Fasta file")
                            {
                                Filter = FileUtils.fastaFilter,
                                Help = "Select the fasta file used for the database search of this dataset. The software will assume uniprot-formatted headers for extracting accession IDs and metadata. As fallback position, everything after the > will be taken as ID, but no header metadata can be extracted from non-uniprot headers."
                            },
                        new SingleChoiceWithSubParams("Fasta header annotations")
                            {
                                paramNameWidth = 120,
                                totalWidth = 500,
                                Help = "Specify the annotations to be extracted from uniprot fasta headers",
                                Values = new string[] {"for all IDs", "for the leading ID"},
                                Value = 0,
                                SubParams = new List<Parameters>(){
								new Parameters(new Parameter[]
                                {
									new MultiChoiceParam("Annotations")
									    {
									        Values = new string[]{"Entry name", "Gene name", "Protein name (verbose)", "Protein name (consensus)" , "Species"},
                                            Value = new int[]{1,3},
									    }
								}),
								new Parameters(new Parameter[]
								{
								    new MultiChoiceParam("Annotations")
									    {
									        Values = new string[]{"Entry name", "Gene name", "Protein name (verbose)" , "Protein name (consensus)" , "Species"},
                                            Value = new int[]{1,3},
									    }
								})
							    }
                            },
                        new SingleChoiceWithSubParams("Numeric annotations")
                            {
                                paramNameWidth = 120,
                                totalWidth = 500,
                                Help = "Specify the annotations to be mapped as numeric annotations",
                                Values = new string[] {"median of all IDs", "for the leading ID"},
                                Value = 0,
                                SubParams = new List<Parameters>(){
								new Parameters(new Parameter[]
                                {
									new MultiChoiceParam("Annotations")
									    {
									        Values = new string[]{"Sequence length","Monoisotopic molecular mass","Average molecular mass"},
                                            Value = new int[]{0,2}
									    }
								}),
								new Parameters(new Parameter[]
								{
								    new MultiChoiceParam("Annotations")
									    {
									        Values = new string[]{"Sequence length","Monoisotopic molecular mass","Average molecular mass"},
                                            Value = new int[]{0,2}
									    }
								})
							    }
                            },
                        new SingleChoiceWithSubParams("Calculate theoretical peptides")
                            {
                                paramNameWidth = 120,
                                totalWidth = 500,
                                Help = "Calculate the numbers of theoretical peptides (without miscleavages) by in silico digestion.",
                                Values = new string[] {"median of all IDs", "for the leading ID"},
                                Value = 0,
                                SubParams = new List<Parameters>(){
								new Parameters(new Parameter[]
                                {
									new MultiChoiceParam("Proteases")
									    {
									        Values = Constants.DefaultProteasesNames(),
                                            Value = new int[]{0}
									    },
                                    new DoubleParam("Min. peptide length",7),
                                    new DoubleParam("Max. peptide length",30)
								}),
								new Parameters(new Parameter[]
								{
								    new MultiChoiceParam("Proteases")
									    {
									        Values = Constants.DefaultProteasesNames(),
                                            Value = new int[]{0}
									    },
                                    new DoubleParam("Min. peptide length",7),
                                    new DoubleParam("Max. peptide length",30),
                                    new BoolParam("Show sequences",false), 
								})
							    }
                            },
                        new SingleChoiceWithSubParams("Count sequence features")
                            {
                                paramNameWidth = 180,
                                totalWidth = 500,
                                Help = "Count the number of matches to a given regular expression in the amino acid sequence (optionally normalized by sequence length).\n\nExamples:\n[KR] tryptic cleavage sites\nN[^P][ST][^P] N-glysosylation motifs",
                                Values = new string[] {"median of all IDs", "for the leading ID"},
                                Value = 0,

                                SubParams = new List<Parameters>()
                                {
                                    new Parameters(new Parameter[]
                                        {
                                            new StringParam("Regex"), 
                                            new BoolParam("Normalize by sequence length", true), 
                                        }),
                                    new Parameters(new Parameter[]
                                        {
                                            new StringParam("Regex"), 
                                            new BoolParam("Normalize by sequence length", false), 
                                        })
                                }
                            }
                    });
        }
    }

}
