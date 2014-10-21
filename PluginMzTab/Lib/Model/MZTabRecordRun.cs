using System;
using System.IO;
using System.Security.Policy;
using PluginMzTab.Plugin.Utils;

namespace PluginMzTab.Lib.Model{
    public class MZTabRecordRun{
        private void addProteinValue(){
            MsRun msRun1 = new MsRun(1);
            MsRun msRun2 = new MsRun(2);
            Assay assay1 = new Assay(1);
            Assay assay2 = new Assay(2);
            StudyVariable studyVariable1 = new StudyVariable(1);

            MZTabColumnFactory factory = MZTabColumnFactory.GetInstance(Section.Protein_Header);
            factory.AddOptionalColumn(ProteinColumn.SEARCH_ENGINE_SCORE, msRun1);
            factory.AddOptionalColumn(ProteinColumn.NUM_PSMS, msRun1);
            factory.AddOptionalColumn(ProteinColumn.NUM_PEPTIDES_DISTINCT, msRun1);
            factory.AddOptionalColumn(ProteinColumn.NUM_PEPTIDES_UNIQUE, msRun1);
            factory.AddOptionalColumn(ProteinColumn.NUM_PSMS, msRun2);
            factory.AddOptionalColumn(ProteinColumn.NUM_PEPTIDES_DISTINCT, msRun2);

            factory.AddAbundanceOptionalColumn(assay1);
            factory.AddAbundanceOptionalColumn(studyVariable1);
            factory.AddAbundanceOptionalColumn(assay2);

            factory.AddOptionalColumn(assay1, "my_value", typeof (string));
            CVParam param = new CVParam("MS", "MS:1002217", "decoy peptide", null);
            factory.AddOptionalColumn(param, typeof (string));

            Console.WriteLine(factory);

            Protein protein = new Protein(factory);

            // set stable columns data.
            protein.Accession = "P12345";
            protein.Description = "Aspartate aminotransferase, mitochondrial";
            protein.SetTaxid("10116");
            protein.Species = "Rattus norvegicus (Rat)";
            protein.Database = "UniProtKB";
            protein.DatabaseVersion = "2011_11";
            protein.SetSearchEngine("[MS,MS:1001207,Mascot,]");
            protein.AddSearchEngine("[MS,MS:1001208,Sequest,]");
            protein.SetBestSearchEngineScore("[MS,MS:1001171,Mascot score,50]|[MS,MS:1001155,Sequest:xcorr,2]");
            protein.Reliability = Reliability.High;
            protein.SetAmbiguityMembers("P12347,P12348");
            protein.SetModifications("3|4|8-MOD:00412, 3|4|8-MOD:00412");
            protein.SetURI("http://www.ebi.ac.uk/pride/url/to/P12345");
            protein.SetGOTerms("GO:0006457|GO:0005759|GO:0005886|GO:0004069");
            protein.SetProteinCoverage("0.4");
            Console.WriteLine(protein);

            // set optional columns which have stable order.
            protein.setSearchEngineScore(msRun1, "[MS,MS:1001171,Mascot score,50]|[MS,MS:1001155,Sequest:xcorr,2]");
            protein.setNumPSMs(msRun1, 4);
            protein.setNumPSMs(msRun2, 2);
            protein.setNumPeptidesDistinct(msRun1, 3);
            protein.setNumPeptidesUnique(msRun1, 2);
            Console.WriteLine(protein);

            // set abundance columns
            protein.setAbundanceColumn(assay1, "0.4");
            protein.setAbundanceColumn(assay2, "0.2");

            protein.setAbundanceColumn(studyVariable1, "0.4");
            protein.setAbundanceStdevColumn(studyVariable1, "0.3");
            protein.setAbundanceStdErrorColumn(studyVariable1, "0.2");
            Console.WriteLine(protein);

            // set user defined optional columns
            protein.setOptionColumn(assay1, "my_value", "My value about assay[1]");
            protein.setOptionColumn(param, "TOM value");

            Console.WriteLine(protein);
        }

        private void addPeptideValue(){
            MsRun msRun1 = new MsRun(1);
            Assay assay1 = new Assay(1);
            Assay assay2 = new Assay(2);
            StudyVariable studyVariable1 = new StudyVariable(1);

            MZTabColumnFactory factory = MZTabColumnFactory.GetInstance(Section.Peptide_Header);
            factory.AddOptionalColumn(PeptideColumn.SEARCH_ENGINE_SCORE, msRun1);
            factory.AddAbundanceOptionalColumn(assay1);
            factory.AddAbundanceOptionalColumn(studyVariable1);
            factory.AddAbundanceOptionalColumn(assay2);
            factory.AddOptionalColumn(msRun1, "my_value", typeof (string));
            CVParam param = new CVParam("MS", "MS:1002217", "decoy peptide", null);
            factory.AddOptionalColumn(param, typeof (string));

            Metadata metadata = new Metadata();
            metadata.AddMsRunLocation(2, new Url("file://C:\\path\\to\\my\\file"));

            Console.WriteLine(factory);

            Peptide peptide = new Peptide(factory, metadata);

            peptide.Sequence = "KVPQVSTPTLVEVSR";
            peptide.Accession = "P02768";
            peptide.SetUnique("0");
            peptide.Database = "UniProtKB";
            peptide.DatabaseVersion = "2011_11";
            peptide.SetSearchEngine("[MS,MS:1001207,Mascot,]|[MS,MS:1001208,Sequest,]");
            peptide.SetBestSearchEngineScore("[MS,MS:1001155,Sequest:xcorr,2]");
            peptide.Reliability = Reliability.findReliability("3");
            peptide.SetModifications(
                "3[MS,MS:1001876, modification probability, 0.8]|4[MS,MS:1001876, modification probability, 0.2]-MOD:00412,8[MS,MS:1001876, modification probability, 0.3]-MOD:00412");
            peptide.AddRetentionTime(10.2);
            peptide.AddRetentionTimeWindow(1123.2);
            peptide.AddRetentionTimeWindow(1145.3);
            peptide.Charge = new Integer(2);
            peptide.MassToCharge = 1234.4;
            peptide.URI = new Uri("http://www.ebi.ac.uk/pride/link/to/peptide");
            peptide.SetSpectraRef("ms_run[2]:index=7|ms_run[2]:index=9");
            Console.WriteLine(peptide);
        }

        private void addPSMValue(){
            Assay assay1 = new Assay(1);

            MZTabColumnFactory factory = MZTabColumnFactory.GetInstance(Section.PSM_Header);
            factory.AddOptionalColumn(assay1, "my_value", typeof (string));
            CVParam param = new CVParam("MS", "MS:1002217", "decoy peptide", null);
            factory.AddOptionalColumn(param, typeof (string));

            Metadata metadata = new Metadata();
            metadata.AddMsRunLocation(2, new Url("file://C:\\path\\to\\my\\file"));

            Console.WriteLine(factory);
            PSM psm = new PSM(factory, metadata);

            psm.Sequence = "KVPQVSTPTLVEVSR";
            psm.SetPSM_ID("1");
            psm.Accession = "P02768";
            psm.Unique = MZBoolean.False;
            psm.Database = "UniProtKB";
            psm.DatabaseVersion = "2011_11";
            psm.setSearchEngine("[MS,MS:1001207,Mascot,]|[MS,MS:1001208,Sequest,]");
            psm.setSearchEngineScore("[MS,MS:1001155,Sequest:xcorr,2]");
            psm.setReliability("3");
            psm.setModifications("CHEMMOD:+159.93");
            psm.AddRetentionTime(10.2);
            psm.Charge = new Integer(2);
            psm.SetExpMassToCharge("1234.4");
            psm.SetCalcMassToCharge("123.4");
            psm.SetUri("http://www.ebi.ac.uk/pride/link/to/peptide");
            psm.SetSpectraRef("ms_run[2]:index=7|ms_run[2]:index=9");
            psm.Pre = "K";
            psm.Post = "D";
            psm.Start = "45";
            psm.End = "57";
            Console.WriteLine(psm);
        }

        private void addSmallMoleculeValue(){
            MsRun msRun1 = new MsRun(1);
            Assay assay1 = new Assay(1);
            Assay assay2 = new Assay(2);
            StudyVariable studyVariable1 = new StudyVariable(1);

            MZTabColumnFactory factory = MZTabColumnFactory.GetInstance(Section.Small_Molecule);
            factory.AddAbundanceOptionalColumn(assay1);
            factory.AddAbundanceOptionalColumn(studyVariable1);
            factory.AddAbundanceOptionalColumn(assay2);
            factory.AddOptionalColumn(msRun1, "my_value", typeof (string));
            CVParam param = new CVParam("MS", "MS:1002217", "decoy peptide", null);
            factory.AddOptionalColumn(param, typeof (string));

            Metadata metadata = new Metadata();
            metadata.AddMsRunLocation(2, new Url("file://C:\\path\\to\\my\\file"));

            Console.WriteLine(factory);
            /*TODO: SmallMolecule sm = new SmallMolecule(factory, metadata);
        sm.setIdentifier("CID:00027395");
        sm.setChemicalFormula("C17H20N4O2");
        sm.setSmiles("C1=CC=C(C=C1)CCNC(=O)CCNNC(=O)C2=CC=NC=C2");
        sm.setInchiKey("QXBMEGUKVLFJAM-UHFFFAOYSA-N");
        sm.setDescription("N-(2-phenylethyl)-3-[2-(pyridine-4-carbonyl)hydrazinyl]propanamide");
        sm.setExpMassToCharge("1234.4");
        sm.setCalcMassToCharge("1234.5");
        sm.setCharge("2");
        sm.setRetentionTime("10.2|11.5");
        sm.setTaxid("10116");
        sm.setSpecies("Rattus norvegicus (Rat)");
        sm.setDatabase("UniProtKB");
        sm.setDatabaseVersion("2011_11");
        sm.setReliability("2");
        sm.setURI("http://www.ebi.ac.uk/pride/link/to/identification");
        sm.setSpectraRef("ms_run[2]:index=7|ms_run[2]:index=9");
        sm.setSearchEngine("[MS, MS:1001477, SpectraST,]");
        sm.setBestSearchEngineScore("[MS, MS:1001419, SpectraST:discriminant score F, 0.7]");
        sm.setModifications("CHEMMOD:+Na-H");

        Console.WriteLine(sm);*/
        }

        public static void main(string[] args, string file){
            StreamWriter writer;
            if (!string.IsNullOrEmpty(file)){
                writer = new StreamWriter(File.Open(file, FileMode.OpenOrCreate, FileAccess.Write));
                Console.SetOut(writer);
            }
            MZTabRecordRun run = new MZTabRecordRun();

            Console.WriteLine("Fill protein record.");
            run.addProteinValue();
            Console.WriteLine("\n\n");

            Console.WriteLine("Fill peptide record.");
            run.addPeptideValue();
            Console.WriteLine("\n\n");

            Console.WriteLine("Fill PSM record.");
            run.addPSMValue();
            Console.WriteLine("\n\n");

            Console.WriteLine("Fill small molecule record.");
            run.addSmallMoleculeValue();
            Console.WriteLine("\n\n");
        }
    }
}