using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using PerseusApi.Generic;

namespace PluginProteomicRuler
{

    static class Constants
    {
        public static Dictionary<string, double> MonoisotopicMasses = new Dictionary<string, double>()
        {
            {"A", 71.03711},
            {"R",156.10111},
            {"N",114.04293},
            {"D",115.02694},
            {"C",103.00919},
            {"E",129.04259},
            {"Q",128.05858},
            {"G", 57.02146},
            {"H",137.05891},
            {"I",113.08406},
            {"L",113.08406},
            {"K",128.09496},
            {"M",131.04049},
            {"F",147.06841},
            {"P",97.05276},
            {"S",87.03203},
            {"T",101.04768},
            {"W",186.07931},
            {"Y",163.06333},
            {"V",99.06841},
            {"term",18.01056} // Proton on the N-terminus and OH on the C-terminus
        };
        public static Dictionary<string, double> AverageMasses = new Dictionary<string, double>()
        {
            {"A", 71.0788},
            {"R",156.1875},
            {"N",114.1038},
            {"D",115.0886},
            {"C",103.1388},
            {"E",129.1155},
            {"Q",128.1307},
            {"G", 57.0519},
            {"H",137.1411},
            {"I",113.1594},
            {"L",113.1594},
            {"K",128.1741},
            {"M",131.1926},
            {"F",147.1766},
            {"P", 97.1167},
            {"S", 87.0782},
            {"T",101.1051},
            {"W",186.2132},
            {"Y",163.176},
            {"V",99.1326},
            {"term",18.01528} // Proton on the N-terminus and OH on the C-terminus
        };

        public static Protease Trypsin = new Protease("Trypsin", new Regex(@"(.*?(?:K|R|$))"));
        public static Protease LysC = new Protease("LysC", new Regex(@"(.*?(?:K|$))"));
        public static Protease ArgC = new Protease("ArgC", new Regex(@"(.*?(?:R|$))"));
        public static Protease AspC = new Protease("AspC", new Regex(@"(.*?(?:D|$))"));
        public static Protease GluC = new Protease("GluC", new Regex(@"(.*?(?:E|$))"));
        public static Protease GluN = new Protease("GluN", new Regex(@"([E|^][^E]*)"));
        public static Protease AspN = new Protease("AspN", new Regex(@"([D|^][^D]*)"));


        public static Protease[] DefaultProteases = new [] { Trypsin, LysC, GluC, AspN, GluN, ArgC };

        public static List<string> DefaultProteasesNames()
        {
            List<string> names = new List<string>();
            foreach (Protease protease in DefaultProteases)
                names.Add(protease.Name);
            return names;
        }
    }


    public class Protease
    {
        public string Name;
        public Regex CleavageSpecificity;

        public Protease(string name, Regex regex)
        {
            Name = name;
            CleavageSpecificity = regex;
        }
    }




    public class PeptideSequence
    {
        private string _sequence = "";
        private int _length = 0;
        private double _molecularMassMonoisotopic, _molecularMassAverage;
        private bool _hasMolecularMassMonoisotopic, _hasMolecularMassAverage;
        public void SetSequence(string sequence)
        {
            sequence = sequence.ToUpper();
            _sequence = Regex.Replace(sequence, @"[^A-Z]", ""); // strip everything that is not an amino acid (ACDEFGHIKLMNPQRSTVWacdefghiklmnpqrstvw) plus other letters
            _length = _sequence.Length;

        }
        public void SetSequence(List<string> sequencePieces)
        {
            string sequence = "";
            foreach (string piece in sequencePieces)
                sequence += piece;
            SetSequence(sequence);
        }

        private void CalculateAverageMass()
        {
            foreach (char aa in _sequence)
                if (Constants.AverageMasses.ContainsKey(aa.ToString()))
                    _molecularMassAverage += Constants.AverageMasses[aa.ToString()];
            if (_molecularMassAverage != 0)
                _molecularMassAverage += Constants.AverageMasses["term"];
            _hasMolecularMassAverage = true;
        }

        private void CalculateMonoisotopicMass()
        {
            foreach (char aa in _sequence)
                if (Constants.MonoisotopicMasses.ContainsKey(aa.ToString()))
                    _molecularMassMonoisotopic += Constants.MonoisotopicMasses[aa.ToString()];
            if (_molecularMassMonoisotopic != 0)
                _molecularMassMonoisotopic += Constants.MonoisotopicMasses["term"];
            _hasMolecularMassMonoisotopic = true;
        }

        public string GetSequence()
        {
            return _sequence;
        }
        public int GetLength()
        {
            return _length;
        }
        public double GetAverageMolecularMass()
        {
            if (!_hasMolecularMassAverage)
                CalculateAverageMass();
            return _molecularMassAverage;
        }
        public double GetMonoisotopicMolecularMass()
        {
            if (!_hasMolecularMassMonoisotopic)
                CalculateMonoisotopicMass();
            return _molecularMassMonoisotopic;
        }
    }


    public class ProteinSequence : PeptideSequence
    {

        private string _header, _accession, _geneName, _species, _entryName, _proteinName, _consensusProteinName;

        public bool Equals(ProteinSequence otherSequence)
        {
            return GetAccession() == otherSequence.GetAccession() && GetSequence() == otherSequence.GetSequence();
        }



        private Dictionary<Protease, PeptideSequence[]> _theoreticalPeptides = new Dictionary<Protease, PeptideSequence[]>();

        // generic method; protease and margins can be defined
        private void CalculateTheoreticalPeptides(Protease protease, int minLength, int maxLength, double minWeight, double maxWeight)
        {
            MatchCollection peptideMatches = protease.CleavageSpecificity.Matches(GetSequence());
            List<PeptideSequence> theoreticalPeptides = new List<PeptideSequence>();
            foreach (Match match in peptideMatches)
            {
                PeptideSequence theoreticalPeptide = new PeptideSequence();
                theoreticalPeptide.SetSequence(match.Groups[1].Value);
                if (
                       theoreticalPeptide.GetLength() >= minLength
                    && theoreticalPeptide.GetLength() <= maxLength
                    && (
                    (minWeight > 0 && maxWeight < double.PositiveInfinity) // speed up calculations in case there are no weight limits
                    ||
                    (theoreticalPeptide.GetMonoisotopicMolecularMass() >= minWeight
                    && theoreticalPeptide.GetMonoisotopicMolecularMass() <= maxWeight)
                    )
                )
                    theoreticalPeptides.Add(theoreticalPeptide);
            }
            _theoreticalPeptides[protease] = theoreticalPeptides.ToArray();

            //_numberOfTheoreticalPeptides = theoreticalPeptides.Count;
            //_hasTheoreticalPeptides = true;
        }

        private void CalculateTheoreticalPeptides(Protease protease)
        {
            int minLength = 7;
            int maxLength = 30;
            double minWeight = 0;
            double maxWeight = double.PositiveInfinity;
            CalculateTheoreticalPeptides(protease, minLength, maxLength, minWeight, maxWeight);
        }

        private void CalculateTheoreticalPeptides()
        {
            CalculateTheoreticalPeptides(Constants.Trypsin);
        }

        public int GetNumberOfTheoreticalPeptides(Protease protease, int minLength, int maxLength)
        {
            if (!_theoreticalPeptides.ContainsKey(protease))
                CalculateTheoreticalPeptides(protease,minLength,maxLength,0,double.PositiveInfinity);
            return _theoreticalPeptides[protease].Count();
        }

        public int GetNumberOfTheoreticalPeptides(Protease protease)
        {
            if (!_theoreticalPeptides.ContainsKey(protease))
                CalculateTheoreticalPeptides(protease);
            return _theoreticalPeptides[protease].Count();
        }

        public int GetNumberOfTheoreticalPeptides()
        {
            Protease protease = Constants.Trypsin;
            if (!_theoreticalPeptides.ContainsKey(protease))
                CalculateTheoreticalPeptides(protease);
            return _theoreticalPeptides[protease].Count();
        }

        public PeptideSequence[] GetTheoreticalPeptides(Protease protease)
        {
            if (!_theoreticalPeptides.ContainsKey(protease))
                CalculateTheoreticalPeptides(protease);
            return _theoreticalPeptides[protease];
        }

        public PeptideSequence[] GetTheoreticalPeptides(Protease protease, int minLength, int maxLength)
        {
            if (!_theoreticalPeptides.ContainsKey(protease))
                CalculateTheoreticalPeptides(protease, minLength, maxLength, 0, double.PositiveInfinity);
            return _theoreticalPeptides[protease];
        }

        public PeptideSequence[] GetTheoreticalPeptides()
        {
            Protease protease = Constants.Trypsin;
            if (!_theoreticalPeptides.ContainsKey(protease))
                CalculateTheoreticalPeptides(protease);
            return _theoreticalPeptides[protease];
        }

        public string[] GetTheoreticalPeptideSequences(Protease protease)
        {
            PeptideSequence[] peptides = GetTheoreticalPeptides(protease);
            List<string> peptideSequences = new List<string>();
            foreach (PeptideSequence peptide in peptides)
                peptideSequences.Add(peptide.GetSequence());
            return peptideSequences.ToArray();
        }

        public string[] GetTheoreticalPeptideSequences(Protease protease, int minLength, int maxLength)
        {
            PeptideSequence[] peptides = GetTheoreticalPeptides(protease, minLength, maxLength);
            List<string> peptideSequences = new List<string>();
            foreach (PeptideSequence peptide in peptides)
                peptideSequences.Add(peptide.GetSequence());
            return peptideSequences.ToArray();
        }


        public void SetHeader(string header)
        {
            _header = header;
        }
        public string GetHeader()
        {
            return _header;
        }

        public void SetAccession(string accession)
        {
            _accession = accession;
        }
        public string GetAccession()
        {
            return _accession;
        }

        public void SetEntryName(string entryName)
        {
            _entryName = entryName;
        }
        public string GetEntryName()
        {
            return _entryName;
        }

        public void SetProteinName(string proteinName)
        {
            _proteinName = proteinName;
        }
        public string GetProteinName()
        {
            return _proteinName;
        }

        public void SetConsensusProteinName(string consensusProteinName)
        {
            _consensusProteinName = consensusProteinName;
        }
        public string GetConsensusProteinName()
        {
            return _consensusProteinName;
        }

        public void SetGeneName(string geneName)
        {
            _geneName = geneName;
        }
        public string GetGeneName()
        {
            return _geneName;
        }

        public void SetSpecies(string species)
        {
            _species = species;
        }
        public string GetSpecies()
        {
            return _species;
        }

    }


    class Fasta
    {
        private Regex _regexHeader = new Regex(@"^>.*");
        private Regex _regexAccession = new Regex(@"^>.*\|(.*)\|");
        private Regex _regexEntryName = new Regex(@"^>.*\|.*\|(\w*)");
        //private Regex _regexGeneName = new Regex(@"^>.*\|.*\|.*\sGN=(\w*)");
        private Regex _regexGeneName = new Regex(@"^>.*\|.*\|.*\sGN=(.*?)(?:\sPE=|\sSV=|$)"); // allow spaces in gene names
        private Regex _regexProteinName = new Regex(@"^>.*\|.*\|\w*\s(.*?)\s(?:OS|GN|PE|SV)=");
        private Regex _regexConsensusProteinName = new Regex(@"(?:Isoform [0-9]+ of )?(.*(?=( \(Fragment\)))|.*)");
        private Regex _regexSpecies = new Regex(@"^>.*\|.*\|.*\sOS=(.*?)\s(?:GN|PE|SV)=");

        public Dictionary<string, ProteinSequence>  Entries = new Dictionary<string, ProteinSequence>();

        public void ParseFile(string path, ProcessInfo processInfo)
        {
            processInfo.Status("Parsing " + path);

            string header = "";
            string accession = "";

            string line;
            int sequenceCounter = 0;
            List<string> sequencePieces = new List<string>();
            ProteinSequence protein = new ProteinSequence();

            try
            {
                System.IO.StreamReader file = new StreamReader(path);


                while ((line = file.ReadLine()) != null) // valid line
                {
                    processInfo.Status("Parsing " + path + ", reading sequence #" + sequenceCounter);

                    //line = line.Replace("\n", "").Replace("\r","");

                    bool lineIsHeader = _regexHeader.Match(line).Success;

                    // skip all lines until the first header is found
                    if (sequenceCounter == 0 && !lineIsHeader)
                        continue;

                    // line is a piece of a sequence
                    if (sequenceCounter > 0 && !lineIsHeader)
                    {
                        sequencePieces.Add(line);
                        continue;
                    }

                    //check if line is a fasta header
                    if (lineIsHeader)
                    {
                        if (sequenceCounter > 0)
                            // this is not the first header, i.e. the previous sequence is now completely read in
                        {
                            // add the previous protein  
                            protein.SetSequence(sequencePieces);
                            Entries.Add(accession, protein);
                            //Sequences.Add(accession, protein);
                        }
                        // initialize a new protein
                        protein = new ProteinSequence();
                        sequenceCounter++;
                        // then parse the new header
                        header = line;
                        accession = _regexAccession.Match(line).Groups[1].Value;
                        protein.SetHeader(header);
                        protein.SetAccession(accession);
                        protein.SetEntryName(_regexEntryName.Match(line).Groups[1].Value);
                        protein.SetProteinName(_regexProteinName.Match(line).Groups[1].Value);
                        protein.SetConsensusProteinName(_regexConsensusProteinName.Match(protein.GetProteinName()).Groups[1].Value);

                        if (_regexGeneName.Match(line).Success)
                            protein.SetGeneName(_regexGeneName.Match(line).Groups[1].Value);
                        protein.SetSpecies(_regexSpecies.Match(line).Groups[1].Value);
                        sequencePieces = new List<string>();
                    }
                } //end while
                //add the last protein
                if (sequenceCounter > 0) // make sure there is at least one sequence in the file
                {
                    protein.SetSequence(sequencePieces);
                    Entries.Add(accession, protein);
                    //Sequences.Add(accession, protein);
                }
            }
            catch (Exception)
            {
                processInfo.ErrString = "Something went wrong while opening the fasta file. Make sure the path is correct and the file is not opened in another application.";
            }

        }


        public ProteinSequence GetEntry(string accession)
        {
            if (Entries.ContainsKey(accession))
            {
                return Entries[accession];
            }
            return null;
        }





    }




}
