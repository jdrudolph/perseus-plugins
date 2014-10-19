using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using PluginMzTab.Lib.Model;
using PluginMzTab.Lib.Utils.Errors;

namespace PluginMzTab.Lib.Utils.Parser{
    public class MZTabFileParser{
        private MZTabFile _mzTabFile;
        private string _tabFile;

        private readonly MZTabErrorList errorList;

        private void init(string tabFile){
            if (tabFile == null || !File.Exists(tabFile)){
                throw new FileNotFoundException("MZTab File not exists!");
            }

            _tabFile = tabFile;
        }

        public MZTabFileParser(string tabFile, TextWriter outstream) : this(tabFile, outstream, MZTabProperties.LEVEL){}

        public MZTabFileParser(string tabFile, TextWriter outstream, Level level){
            init(tabFile);

            try{
                errorList = new MZTabErrorList(level);
                check();
                refine();
            }
            catch (MZTabException e){
                outstream.Write(MZTabProperties.MZTabExceptionMessage);
                errorList.Add(e.Error);
            }
            catch (MZTabErrorOverflowException){
                outstream.Write(MZTabProperties.MZTabErrorOverflowExceptionMessage);
            }

            errorList.print(outstream);
            if (errorList.IsNullOrEmpty()){
                outstream.Write("not errors in " + tabFile + " file!" + MZTabConstants.NEW_LINE);
            }
        }

        public MZTabErrorList getErrorList(){
            return errorList;
        }

        public static Section getSection(string line){
            string[] items = line.Split(new[]{MZTabConstants.TAB}, StringSplitOptions.None);
            string section = items[0].Trim();
            return Section.findSection(section);
        }

        private StreamReader readFile(string tabFile){
            StreamReader reader;

            if (tabFile.EndsWith(".gz")){
                reader =
                    new StreamReader(new GZipStream(new FileStream(tabFile, FileMode.Open), CompressionMode.Decompress));
            }
            else{
                reader = new StreamReader(new FileStream(tabFile, FileMode.Open));
            }

            return reader;
        }

        public static string subString(string source){
            const int length = 20;

            if (length >= source.Length){
                return source;
            }
            return source.Substring(0, length - 1) + "...";
        }

        /**
         * refine all MZTabFile consistency correct.
         */

        private void refine(){
            if (_mzTabFile == null){
                return;
            }

            Metadata metadata = _mzTabFile.getMetadata();
            MZTabColumnFactory proteinFactory = _mzTabFile.getProteinColumnFactory();
            MZTabColumnFactory peptideFactory = _mzTabFile.getPeptideColumnFactory();
            //MZTabColumnFactory psmFactory = _mzTabFile.getPsmColumnFactory();
            MZTabColumnFactory smlFactory = _mzTabFile.getSmallMoleculeColumnFactory();

            // If mzTab-type is "Quantification", then at least one section with {protein|peptide|small_molecule}_abundance* columns MUST be present
            bool hasAbundance = false;
            if (metadata.MzTabType == MzTabType.Quantification){
                if (proteinFactory != null && proteinFactory.AbundanceColumnMapping.Count != 0){
                    hasAbundance = true;
                }
                if (peptideFactory != null && peptideFactory.AbundanceColumnMapping.Count != 0){
                    hasAbundance = true;
                }
                if (smlFactory != null && smlFactory.AbundanceColumnMapping.Count != 0){
                    hasAbundance = true;
                }
                if (! hasAbundance){
                    throw new MZTabException(new MZTabError(LogicalErrorType.QuantificationAbundance, -1));
                }
            }
        }

        private void check(){
            StreamReader reader = readFile(_tabFile);
            check(reader);
        }

        /**
         * Query {@link uk.ac.ebi.pride.jmztab.utils.errors.MZTabErrorList} to check exist errors or not.
         * @throws java.io.IOException
         * @throws uk.ac.ebi.pride.jmztab.utils.errors.MZTabException during parse metadata, protein/peptide/small_molecule header line, exists error.
         * @throws uk.ac.ebi.pride.jmztab.utils.errors.MZTabErrorOverflowException reference mztab.properties file mztab.max_error_count parameter.
         */

        public void check(StreamReader reader){
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

            string line;
            int highWaterMark = 1;
            int lineNumber = 0;
            while ((line = reader.ReadLine()) != null){
                lineNumber++;

                if (string.IsNullOrEmpty(line) || line.StartsWith("MTH") || line.StartsWith("#")){
                    continue;
                }

                if (line.StartsWith(Section.Comment.Prefix)){
                    comParser.Parse(lineNumber, line, errorList);
                    commentMap.Add(lineNumber, comParser.getComment());
                    continue;
                }

                Section section = getSection(line);
                MZTabError error;
                if (section == null){
                    error = new MZTabError(FormatErrorType.LinePrefix, lineNumber, subString(line));
                    throw new MZTabException(error);
                }
                if (section.Level < highWaterMark){
                    Section currentSection = Section.FindSection(highWaterMark);
                    error = new MZTabError(LogicalErrorType.LineOrder, lineNumber, currentSection.Name, section.Name);
                    throw new MZTabException(error);
                }

                highWaterMark = section.Level;
                // There exists errors during checking metadata section.
                if (highWaterMark == 1 && ! errorList.IsNullOrEmpty()){
                    break;
                }

                switch (highWaterMark){
                    case 1:
                        // metadata section.
                        mtdParser.Parse(lineNumber, line, errorList);
                        break;
                    case 2:
                        if (prhParser != null){
                            // header line only display once!
                            error = new MZTabError(LogicalErrorType.HeaderLine, lineNumber, subString(line));
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
                        if (prhParser == null){
                            // header line should be check first.
                            error = new MZTabError(LogicalErrorType.NoHeaderLine, lineNumber, subString(line));
                            throw new MZTabException(error);
                        }

                        if (prtParser == null){
                            prtParser = new PRTLineParser(prhParser.getFactory(), prtPositionMapping, mtdParser.Metadata,
                                                          errorList);
                        }
                        prtParser.Parse(lineNumber, line, errorList);
                        proteinMap.Add(lineNumber, prtParser.getRecord(line));

                        break;
                    case 4:
                        if (pehParser != null){
                            // header line only display once!
                            error = new MZTabError(LogicalErrorType.HeaderLine, lineNumber, subString(line));
                            throw new MZTabException(error);
                        }

                        if (mtdParser.Metadata.MzTabType == MzTabType.Identification){
                            errorList.Add(new MZTabError(LogicalErrorType.PeptideSection, lineNumber, subString(line)));
                        }

                        // peptide header section
                        pehParser = new PEHLineParser(mtdParser.Metadata);
                        pehParser.Parse(lineNumber, line, errorList);
                        pepPositionMapping = new PositionMapping(pehParser.getFactory(), line);

                        // tell system to continue check peptide data line.
                        highWaterMark = 5;
                        break;
                    case 5:
                        if (pehParser == null){
                            // header line should be check first.
                            error = new MZTabError(LogicalErrorType.NoHeaderLine, lineNumber, subString(line));
                            throw new MZTabException(error);
                        }

                        if (pepParser == null){
                            pepParser = new PEPLineParser(pehParser.getFactory(), pepPositionMapping, mtdParser.Metadata,
                                                          errorList);
                        }
                        pepParser.Parse(lineNumber, line, errorList);
                        peptideMap.Add(lineNumber, pepParser.getRecord(line));

                        break;
                    case 6:
                        if (pshParser != null){
                            // header line only display once!
                            error = new MZTabError(LogicalErrorType.HeaderLine, lineNumber, subString(line));
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
                        if (pshParser == null){
                            // header line should be check first.
                            error = new MZTabError(LogicalErrorType.NoHeaderLine, lineNumber, subString(line));
                            throw new MZTabException(error);
                        }

                        if (psmParser == null){
                            psmParser = new PSMLineParser(pshParser.getFactory(), psmPositionMapping, mtdParser.Metadata,
                                                          errorList);
                        }
                        psmParser.Parse(lineNumber, line, errorList);
                        psmMap.Add(lineNumber, psmParser.getRecord(line));

                        break;
                    case 8:
                        if (smhParser != null){
                            // header line only display once!
                            error = new MZTabError(LogicalErrorType.HeaderLine, lineNumber, subString(line));
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
                        if (smhParser == null){
                            // header line should be check first.
                            error = new MZTabError(LogicalErrorType.NoHeaderLine, lineNumber, subString(line));
                            throw new MZTabException(error);
                        }

                        if (smlParser == null){
                            smlParser = new SMLLineParser(smhParser.getFactory(), smlPositionMapping, mtdParser.Metadata,
                                                          errorList);
                        }
                        smlParser.Parse(lineNumber, line, errorList);
                        smallMoleculeMap.Add(lineNumber, smlParser.getRecord(line));

                        break;
                }
            }

            if (reader != null){
                reader.Close();
            }

            if (errorList.IsNullOrEmpty()){
                _mzTabFile = new MZTabFile(mtdParser.Metadata);
                foreach (int id in commentMap.Keys){
                    _mzTabFile.addComment(id, commentMap[id]);
                }

                if (prhParser != null){
                    MZTabColumnFactory proteinColumnFactory = prhParser.getFactory();
                    _mzTabFile.setProteinColumnFactory(proteinColumnFactory);
                    foreach (int id in proteinMap.Keys){
                        _mzTabFile.addProtein(id, proteinMap[id]);
                    }
                }

                if (pehParser != null){
                    MZTabColumnFactory peptideColumnFactory = pehParser.getFactory();
                    _mzTabFile.setPeptideColumnFactory(peptideColumnFactory);
                    foreach (int id in peptideMap.Keys){
                        _mzTabFile.addPeptide(id, peptideMap[id]);
                    }
                }

                if (pshParser != null){
                    MZTabColumnFactory psmColumnFactory = pshParser.getFactory();
                    _mzTabFile.setPSMColumnFactory(psmColumnFactory);
                    foreach (int id in psmMap.Keys){
                        _mzTabFile.addPSM(id, psmMap[id]);
                    }
                }

                if (smhParser != null){
                    MZTabColumnFactory smallMoleculeColumnFactory = smhParser.getFactory();
                    _mzTabFile.setSmallMoleculeColumnFactory(smallMoleculeColumnFactory);
                    foreach (int id in smallMoleculeMap.Keys){
                        _mzTabFile.addSmallMolecule(id, smallMoleculeMap[id]);
                    }
                }
            }
        }

        public MZTabFile getMZTabFile(){
            return _mzTabFile;
        }
    }
}