using System;
using System.Security.Policy;

namespace PluginMzTab.Lib.Model{
/**
 * User: Qingwei
 * Date: 22/05/13
 */

    public class MetadataRun{
        public static void main(string[] args){
            MZTabDescription tabDescription = new MZTabDescription(MzTabMode.Summary, MzTabType.Identification){
                Id = "PRIDE_1234"
            };
            Metadata mtd = new Metadata(tabDescription);

            mtd.Title = "My first test experiment";
            mtd.Description = "An experiment investigating the effects of Il-6.";

            mtd.AddSampleProcessingParam(1, new CVParam("SEP", "SEP:00173", "SDS PAGE", null));
            mtd.AddSampleProcessingParam(2, new CVParam("SEP", "SEP:00142", "enzyme digestion", null));
            mtd.AddSampleProcessingParam(2, new CVParam("MS", "MS:1001251", "Trypsin", null));

            mtd.AddInstrumentName(1, new CVParam("MS", "MS:100049", "LTQ Orbitrap", null));
            mtd.AddInstrumentName(2,
                                  new CVParam("MS", "MS:1000031", "Instrument model",
                                              "name of the instrument not included in the CV"));
            mtd.AddInstrumentSource(1, new CVParam("MS", "MS:1000073", "ESI", null));
            mtd.AddInstrumentSource(2, new CVParam("MS", "MS:1000598", "ETD", null));
            mtd.AddInstrumentAnalyzer(1, new CVParam("MS", "MS:1000291", "linear ion trap", null));
            mtd.AddInstrumentAnalyzer(2, new CVParam("MS", "MS:1000484", "orbitrap", null));
            mtd.AddInstrumentDetector(1, new CVParam("MS", "MS:1000253", "electron multiplier", null));
            mtd.AddInstrumentDetector(2, new CVParam("MS", "MS:1000348", "focal plane collector", null));

            mtd.AddSoftwareParam(1, new CVParam("MS", "MS:1001207", "Mascot", "2.3"));
            mtd.AddSoftwareParam(2, new CVParam("MS", "MS:1001561", "Scaffold", "1.0"));
            mtd.AddSoftwareSetting(1, "Fragment tolerance = 0.1Da");
            mtd.AddSoftwareSetting(1, "Parent tolerance = 0.5Da");

            mtd.AddFalseDiscoveryRateParam(new CVParam("MS", "MS:1001364", "pep:global FDR", "0.01"));
            mtd.AddFalseDiscoveryRateParam(new CVParam("MS", "MS:1001214", "pep:global FDR", "0.08"));

            mtd.AddPublicationItem(1, PublicationType.PUBMED, "21063943");
            mtd.AddPublicationItem(1, PublicationType.DOI, "10.1007/978-1-60761-987-1_6");
            mtd.AddPublicationItem(2, PublicationType.PUBMED, "20615486");
            mtd.AddPublicationItem(2, PublicationType.DOI, "10.1016/j.jprot.2010.06.008");

            mtd.AddContactName(1, "James D. Watson");
            mtd.AddContactName(2, "Francis Crick");
            mtd.AddContactAffiliation(1, "Cambridge University, UK");
            mtd.AddContactAffiliation(2, "Cambridge University, UK");
            mtd.AddContactEmail(1, "watson@cam.ac.uk");
            mtd.AddContactEmail(2, "crick@cam.ac.uk");

            mtd.AddUri(new Uri("http://www.ebi.ac.uk/pride/url/to/experiment"));
            mtd.AddUri(new Uri("http://proteomecentral.proteomexchange.org/cgi/GetDataset"));

            mtd.AddFixedModParam(1, new CVParam("UNIMOD", "UNIMOD:4", "Carbamidomethyl", null));
            mtd.AddFixedModSite(1, "M");
            mtd.AddFixedModParam(2, new CVParam("UNIMOD", "UNIMOD:35", "Oxidation", null));
            mtd.AddFixedModSite(2, "N-term");
            mtd.AddFixedModParam(3, new CVParam("UNIMOD", "UNIMOD:1", "Acetyl", null));
            mtd.AddFixedModPosition(3, "Protein C-term");

            mtd.AddVariableModParam(1, new CVParam("UNIMOD", "UNIMOD:21", "Phospho", null));
            mtd.AddVariableModSite(1, "M");
            mtd.AddVariableModParam(2, new CVParam("UNIMOD", "UNIMOD:35", "Oxidation", null));
            mtd.AddVariableModSite(2, "N-term");
            mtd.AddVariableModParam(3, new CVParam("UNIMOD", "UNIMOD:1", "Acetyl", null));
            mtd.AddVariableModPosition(3, "Protein C-term");

            mtd.QuantificationMethod = new CVParam("MS", "MS:1001837", "iTRAQ quantitation analysis", null);
            mtd.ProteinQuantificationUnit = new CVParam("PRIDE", "PRIDE:0000395", "Ratio", null);
            mtd.PeptideQuantificationUnit = new CVParam("PRIDE", "PRIDE:0000395", "Ratio", null);
            mtd.SmallMoleculeQuantificationUnit = new CVParam("PRIDE", "PRIDE:0000395", "Ratio", null);

            mtd.AddMsRunFormat(1, new CVParam("MS", "MS:1000584", "mzML file", null));
            mtd.AddMsRunFormat(2, new CVParam("MS", "MS:1001062", "Mascot MGF file", null));
            mtd.AddMsRunLocation(1, new Url("file://C:\\path\\to\\my\\file"));
            mtd.AddMsRunLocation(2, new Url("ftp://ftp.ebi.ac.uk/path/to/file"));
            mtd.AddMsRunIdFormat(1, new CVParam("MS", "MS:1001530", "mzML unique identifier", null));
            mtd.AddMsRunFragmentationMethod(1, new CVParam("MS", "MS:1000133", "CID", null));
            mtd.AddMsRunFragmentationMethod(2, new CVParam("MS", "MS:1000422", "HCD", null));

            mtd.AddCustom(new UserParam("MS operator", "Florian"));

            mtd.AddSampleSpecies(1, new CVParam("NEWT", "9606", "Homo sapiens (Human)", null));
            mtd.AddSampleSpecies(1, new CVParam("NEWT", "573824", "Human rhinovirus 1", null));
            mtd.AddSampleSpecies(2, new CVParam("NEWT", "9606", "Homo sapiens (Human)", null));
            mtd.AddSampleSpecies(2, new CVParam("NEWT", "12130", "Human rhinovirus 2", null));
            mtd.AddSampleTissue(1, new CVParam("BTO", "BTO:0000759", "liver", null));
            mtd.AddSampleCellType(1, new CVParam("CL", "CL:0000182", "hepatocyte", null));
            mtd.AddSampleDisease(1, new CVParam("DOID", "DOID:684", "hepatocellular carcinoma", null));
            mtd.AddSampleDisease(1, new CVParam("DOID", "DOID:9451", "alcoholic fatty liver", null));
            mtd.AddSampleDescription(1, "Hepatocellular carcinoma samples.");
            mtd.AddSampleDescription(2, "Healthy control samples.");
            mtd.AddSampleCustom(1, new UserParam("Extraction date", "2011-12-21"));
            mtd.AddSampleCustom(1, new UserParam("Extraction reason", "liver biopsy"));

            Sample sample1 = mtd.SampleMap[1];
            Sample sample2 = mtd.SampleMap[2];
            mtd.AddAssayQuantificationReagent(1, new CVParam("PRIDE", "PRIDE:0000114", "iTRAQ reagent", "114"));
            mtd.AddAssayQuantificationReagent(2, new CVParam("PRIDE", "PRIDE:0000115", "iTRAQ reagent", "115"));
            mtd.AddAssayQuantificationReagent(1, new CVParam("PRIDE", "MS:1002038", "unlabeled sample", null));
            mtd.AddAssaySample(1, sample1);
            mtd.AddAssaySample(2, sample2);

            mtd.AddAssayQuantificationModParam(2, 1, new CVParam("UNIMOD", "UNIMOD:188", "Label:13C(6)", null));
            mtd.AddAssayQuantificationModParam(2, 2, new CVParam("UNIMOD", "UNIMOD:188", "Label:13C(6)", null));
            mtd.AddAssayQuantificationModSite(2, 1, "R");
            mtd.AddAssayQuantificationModSite(2, 2, "K");
            mtd.AddAssayQuantificationModPosition(2, 1, "Anywhere");
            mtd.AddAssayQuantificationModPosition(2, 2, "Anywhere");

            MsRun msRun1 = mtd.MsRunMap[1];
            mtd.AddAssayMsRun(1, msRun1);

            Assay assay1 = mtd.AssayMap[1];
            Assay assay2 = mtd.AssayMap[2];
            mtd.AddStudyVariableAssay(1, assay1);
            mtd.AddStudyVariableAssay(1, assay2);

            mtd.AddStudyVariableSample(1, sample1);
            mtd.AddStudyVariableDescription(1, "description Group B (spike-in 0.74 fmol/uL)");

            mtd.AddCVLabel(1, "MS");
            mtd.AddCVFullName(1, "MS");
            mtd.AddCVVersion(1, "3.54.0");
            mtd.AddCVURL(1,
                         "http://psidev.cvs.sourceforge.net/viewvc/psidev/psi/psi-ms/mzML/controlledVocabulary/psi-ms.obo");

            mtd.AddProteinColUnit(ProteinColumn.RELIABILITY,
                                  new CVParam("MS", "MS:00001231", "PeptideProphet:Score", null));

            MZTabColumnFactory peptideFactory = MZTabColumnFactory.GetInstance(Section.Peptide);
            PeptideColumn peptideColumn = (PeptideColumn) peptideFactory.FindColumnByHeader("retention_time");
            mtd.AddPeptideColUnit(peptideColumn, new CVParam("UO", "UO:0000031", "minute", null));

            mtd.AddPSMColUnit(PSMColumn.RETENTION_TIME, new CVParam("UO", "UO:0000031", "minute", null));
            mtd.AddSmallMoleculeColUnit(SmallMoleculeColumn.RETENTION_TIME,
                                        new CVParam("UO", "UO:0000031", "minute", null));

            Console.WriteLine(mtd);
        }
    }
}