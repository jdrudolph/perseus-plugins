using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using PerseusApi.Document;
using PluginMzTab.Lib.Model;
using PluginMzTab.Lib.Utils;
using PluginMzTab.Lib.Utils.Errors;
using PluginMzTab.Lib.Utils.Parser;
using PluginMzTab.Plugin.Extended;

namespace PluginMzTab.Plugin.MzTab {
    class ValidateMzTab {

        public void ValidateFile(string file, IDocumentData doc, Action<int> progress, Action<string> status) {
            TextWriter outstream = doc != null ? new StreamWriter(new DocumentStream(doc)) : Console.Out;
            Stream stream = new FileStream(file, FileMode.Open);

            ValidateFile(stream, outstream, progress, status);
        }

        public void ValidateFile(Stream stream, TextWriter outstream, Action<int> progress, Action<string> status) {
            try {
                MZTabErrorList errorList = new MZTabErrorList(Level.Info);

                try {
                    validate(stream, outstream, errorList, progress, status);
                    //refine();
                } catch (MZTabException e) {
                    outstream.Write(MZTabProperties.MZTabExceptionMessage);
                    errorList.Add(e.Error);
                } catch (MZTabErrorOverflowException) {
                    outstream.Write(MZTabProperties.MZTabErrorOverflowExceptionMessage);
                }

                errorList.print(outstream);
                if (errorList.IsNullOrEmpty()) {
                    outstream.Write("No errors in this section!" + MZTabConstants.NEW_LINE);
                }

                outstream.Close();
                //stream.Close();
            } catch (Exception e) {
                MessageBox.Show(e.Message, e.StackTrace);
            }
        }

        private void validate(Stream stream, TextWriter outstream, MZTabErrorList errorList,
                          Action<int> progress, Action<string> status) {
            COMLineParser comParser = new COMLineParser();
            MTDLineParser mtdParser = new MTDLineParser();
            PRHLineParser prhParser = null;
            PRTLineParser prtParser = null;
            PEHLineParser pehParser = null;
            PEPLineParser pepParser = null;
            PSHLineParser pshParser = null;
            PSMLineParser psmParser = null;
            SMHLineParser smhParser = null;
            SMLLineParser smlParser = null;

            SortedDictionary<int, Comment> commentMap = new SortedDictionary<int, Comment>();
            SortedDictionary<int, Protein> proteinMap = new SortedDictionary<int, Protein>();
            SortedDictionary<int, Peptide> peptideMap = new SortedDictionary<int, Peptide>();
            SortedDictionary<int, PSM> psmMap = new SortedDictionary<int, PSM>();
            SortedDictionary<int, SmallMolecule> smallMoleculeMap = new SortedDictionary<int, SmallMolecule>();

            PositionMapping prtPositionMapping = null;
            PositionMapping pepPositionMapping = null;
            PositionMapping psmPositionMapping = null;
            PositionMapping smlPositionMapping = null;

            StreamReader reader = new StreamReader(stream);
            int highWaterMark = 1;
            int lineNumber = 0;
            try {
                string line;
                while ((line = reader.ReadLine()) != null) {
                    progress((int)(stream.Position * 100 / stream.Length));
                    status("Validate line " + lineNumber);
                    lineNumber++;

                    if (String.IsNullOrEmpty(line) || line.StartsWith("MTH") || line.StartsWith("#")) {
                        continue;
                    }

                    if (line.StartsWith(Section.Comment.Prefix)) {
                        comParser.Parse(lineNumber, line, errorList);
                        commentMap.Add(lineNumber, comParser.getComment());
                        continue;
                    }

                    Section section = MZTabFileParser.getSection(line);
                    MZTabError error;
                    if (section == null) {
                        error = new MZTabError(FormatErrorType.LinePrefix, lineNumber, MZTabFileParser.subString(line));
                        throw new MZTabException(error);
                    }
                    if (section.Level < highWaterMark) {
                        Section currentSection = Section.FindSection(highWaterMark);
                        error = new MZTabError(LogicalErrorType.LineOrder, lineNumber, currentSection.Name, section.Name);
                        throw new MZTabException(error);
                    }

                    highWaterMark = section.Level;
                    // There exists errors during checking metadata section.
                    if (highWaterMark == 1 && !errorList.IsNullOrEmpty()) {
                        break;
                    }

                    switch (highWaterMark) {
                        case 1:
                            // metadata section.
                            mtdParser.Parse(lineNumber, line, errorList);
                            break;
                        case 2:
                            if (prhParser != null) {
                                // header line only display once!
                                error = new MZTabError(LogicalErrorType.HeaderLine, lineNumber,
                                                       MZTabFileParser.subString(line));
                                throw new MZTabException(error);
                            }

                            // protein header section
                            prhParser = new PRHLineParser(mtdParser.Metadata);
                            prhParser.Parse(lineNumber, line, errorList);
                            prtPositionMapping = new PositionMapping(prhParser.getFactory(), line);

                            // tell system to continue check protein data line.
                            highWaterMark = 3;
                            break;
                        case 3:
                            if (prhParser == null) {
                                // header line should be check first.
                                error = new MZTabError(LogicalErrorType.NoHeaderLine, lineNumber,
                                                       MZTabFileParser.subString(line));
                                throw new MZTabException(error);
                            }

                            if (prtParser == null) {
                                prtParser = new PRTLineParser(prhParser.getFactory(), prtPositionMapping,
                                                              mtdParser.Metadata,
                                                              errorList);
                            }
                            prtParser.Parse(lineNumber, line, errorList);
                            proteinMap.Add(lineNumber, prtParser.getRecord(line));

                            break;
                        case 4:
                            if (pehParser != null) {
                                // header line only display once!
                                error = new MZTabError(LogicalErrorType.HeaderLine, lineNumber,
                                                       MZTabFileParser.subString(line));
                                throw new MZTabException(error);
                            }

                            if (mtdParser.Metadata.MzTabType == MzTabType.Identification) {
                                errorList.Add(new MZTabError(LogicalErrorType.PeptideSection, lineNumber,
                                                             MZTabFileParser.subString(line)));
                            }

                            // peptide header section
                            pehParser = new PEHLineParser(mtdParser.Metadata);
                            pehParser.Parse(lineNumber, line, errorList);
                            pepPositionMapping = new PositionMapping(pehParser.getFactory(), line);

                            // tell system to continue check peptide data line.
                            highWaterMark = 5;
                            break;
                        case 5:
                            if (pehParser == null) {
                                // header line should be check first.
                                error = new MZTabError(LogicalErrorType.NoHeaderLine, lineNumber,
                                                       MZTabFileParser.subString(line));
                                throw new MZTabException(error);
                            }

                            if (pepParser == null) {
                                pepParser = new PEPLineParser(pehParser.getFactory(), pepPositionMapping,
                                                              mtdParser.Metadata,
                                                              errorList);
                            }
                            pepParser.Parse(lineNumber, line, errorList);
                            peptideMap.Add(lineNumber, pepParser.getRecord(line));

                            break;
                        case 6:
                            if (pshParser != null) {
                                // header line only display once!
                                error = new MZTabError(LogicalErrorType.HeaderLine, lineNumber,
                                                       MZTabFileParser.subString(line));
                                throw new MZTabException(error);
                            }

                            // psm header section
                            pshParser = new PSHLineParser(mtdParser.Metadata);
                            pshParser.Parse(lineNumber, line, errorList);
                            psmPositionMapping = new PositionMapping(pshParser.getFactory(), line);

                            // tell system to continue check peptide data line.
                            highWaterMark = 7;
                            break;
                        case 7:
                            if (pshParser == null) {
                                // header line should be check first.
                                error = new MZTabError(LogicalErrorType.NoHeaderLine, lineNumber,
                                                       MZTabFileParser.subString(line));
                                throw new MZTabException(error);
                            }

                            if (psmParser == null) {
                                psmParser = new PSMLineParser(pshParser.getFactory(), psmPositionMapping,
                                                              mtdParser.Metadata,
                                                              errorList);
                            }
                            psmParser.Parse(lineNumber, line, errorList);
                            psmMap.Add(lineNumber, psmParser.getRecord(line));

                            break;
                        case 8:
                            if (smhParser != null) {
                                // header line only display once!
                                error = new MZTabError(LogicalErrorType.HeaderLine, lineNumber,
                                                       MZTabFileParser.subString(line));
                                throw new MZTabException(error);
                            }

                            // small molecule header section
                            smhParser = new SMHLineParser(mtdParser.Metadata);
                            smhParser.Parse(lineNumber, line, errorList);
                            smlPositionMapping = new PositionMapping(smhParser.getFactory(), line);

                            // tell system to continue check small molecule data line.
                            highWaterMark = 9;
                            break;
                        case 9:
                            if (smhParser == null) {
                                // header line should be check first.
                                error = new MZTabError(LogicalErrorType.NoHeaderLine, lineNumber,
                                                       MZTabFileParser.subString(line));
                                throw new MZTabException(error);
                            }

                            if (smlParser == null) {
                                smlParser = new SMLLineParser(smhParser.getFactory(), smlPositionMapping,
                                                              mtdParser.Metadata,
                                                              errorList);
                            }
                            smlParser.Parse(lineNumber, line, errorList);
                            smallMoleculeMap.Add(lineNumber, smlParser.getRecord(line));

                            break;
                    }
                }
            } catch (Exception e) {
                outstream.WriteLine("Line {0}: {1}", lineNumber, e.Message);
                errorList.Add(new ParserError(lineNumber, e.Message));
            }


            if (reader != null) {
                reader.Close();
            }

            if (errorList.IsNullOrEmpty()) {
                MZTabFile mzTabFile = new MZTabFile(mtdParser.Metadata);
                foreach (int id in commentMap.Keys) {
                    mzTabFile.addComment(id, commentMap[id]);
                }

                if (prhParser != null) {
                    MZTabColumnFactory proteinColumnFactory = prhParser.getFactory();
                    mzTabFile.setProteinColumnFactory(proteinColumnFactory);
                    foreach (int id in proteinMap.Keys) {
                        mzTabFile.addProtein(id, proteinMap[id]);
                    }
                }

                if (pehParser != null) {
                    MZTabColumnFactory peptideColumnFactory = pehParser.getFactory();
                    mzTabFile.setPeptideColumnFactory(peptideColumnFactory);
                    foreach (int id in peptideMap.Keys) {
                        mzTabFile.addPeptide(id, peptideMap[id]);
                    }
                }

                if (pshParser != null) {
                    MZTabColumnFactory psmColumnFactory = pshParser.getFactory();
                    mzTabFile.setPSMColumnFactory(psmColumnFactory);
                    foreach (int id in psmMap.Keys) {
                        mzTabFile.addPSM(id, psmMap[id]);
                    }
                }

                if (smhParser != null) {
                    MZTabColumnFactory smallMoleculeColumnFactory = smhParser.getFactory();
                    mzTabFile.setSmallMoleculeColumnFactory(smallMoleculeColumnFactory);
                    foreach (int id in smallMoleculeMap.Keys) {
                        mzTabFile.addSmallMolecule(id, smallMoleculeMap[id]);
                    }
                }
            }
        }
    }
    
    internal class ParserError : MZTabError {
        private readonly string _msg;

        public ParserError(int line, string msg)
            : base(MZTabErrorType.createError(Category.Logical, "Exception"), line) {
            _msg = msg;
        }

        public override string ToString() {
            return _msg;
        }
    }
}
