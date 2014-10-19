/**
 * Every line in an mzTab file MUST start with a three letter code identifying the PublicationType of line delimited by
 * a Tab character. The three letter codes are as follows :
 * - MTD for metadata
 * - PRH for the protein table header line (the column labels)
 * - PRT for rows of the protein table
 * - PEH for the peptide table header line (the column labels)
 * - PEP for rows of the peptide table
 * - PSH for the PSM table header (the column labels)
 * - PSM for rows of the PSM table
 * - SMH for small molecule table header line (the column labels)
 * - SML for rows of the small molecule table
 * - COM for comment lines
 */

namespace PluginMzTab.Lib.Model{
    public class Section{
        public static readonly Section Comment = new Section("COM", "comment", 0);
        public static readonly Section Metadata = new Section("MTD", "metadata", 1);
        public static readonly Section Protein_Header = new Section("PRH", "protein_header", 2);
        public static readonly Section Protein = new Section("PRT", "protein", 3);
        public static readonly Section Peptide_Header = new Section("PEH", "peptide_header", 4);
        public static readonly Section Peptide = new Section("PEP", "peptide", 5);
        public static readonly Section PSM_Header = new Section("PSH", "psm_header", 6);
        public static readonly Section PSM = new Section("PSM", "psm", 7);
        public static readonly Section Small_Molecule_Header = new Section("SMH", "small_molecule_header", 8);
        public static readonly Section Small_Molecule = new Section("SML", "small_molecule", 9);

        private readonly string prefix;
        private readonly string name;
        private readonly int level;

        private Section(string prefix, string name, int level){
            this.prefix = prefix;
            this.name = name;
            this.level = level;
        }

        public string Prefix { get { return prefix; } }

        public string Name { get { return name; } }

        public int Level { get { return level; } }

        public static Section FindSection(int level){
            Section section;
            switch (level){
                case 0:
                    section = Comment;
                    break;
                case 1:
                    section = Metadata;
                    break;
                case 2:
                    section = Protein_Header;
                    break;
                case 3:
                    section = Protein;
                    break;
                case 4:
                    section = Peptide_Header;
                    break;
                case 5:
                    section = Peptide;
                    break;
                case 6:
                    section = PSM_Header;
                    break;
                case 7:
                    section = PSM;
                    break;
                case 8:
                    section = Small_Molecule_Header;
                    break;
                case 9:
                    section = Small_Molecule;
                    break;
                default:
                    section = null;
                    break;
            }
            return section;
        }

        public bool isComment(){
            return this == Comment;
        }

        public bool isMetadata(){
            return this == Metadata;
        }

        public bool isHeader(){
            return this == Protein_Header || this == Peptide_Header || this == PSM_Header ||
                   this == Small_Molecule_Header;
        }

        public bool isData(){
            return this == Protein || this == Peptide || this == PSM || this == Small_Molecule;
        }

        public static Section toHeaderSection(Section section){
            if (section.Equals(Peptide)){
                return Peptide_Header;
            }
            if (section.Equals(Peptide_Header)){
                return Peptide_Header;
            }
            if (section.Equals(Protein)){
                return Protein_Header;
            }
            if (section.Equals(Protein_Header)){
                return Protein_Header;
            }
            if (section.Equals(PSM)){
                return PSM_Header;
            }
            if (section.Equals(PSM_Header)){
                return PSM_Header;
            }
            if (section.Equals(Small_Molecule)){
                return Small_Molecule_Header;
            }
            if (section.Equals(Small_Molecule_Header)){
                return Small_Molecule_Header;
            }
            return null;
        }

        public static Section toDataSection(Section section){
            if (section.Equals(Peptide)){
                return Peptide;
            }
            if (section.Equals(Peptide_Header)){
                return Peptide;
            }
            if (section.Equals(Protein)){
                return Protein;
            }
            if (section.Equals(Protein_Header)){
                return Protein;
            }
            if (section.Equals(PSM)){
                return PSM;
            }
            if (section.Equals(PSM_Header)){
                return PSM;
            }
            if (section.Equals(Small_Molecule)){
                return Small_Molecule;
            }
            if (section.Equals(Small_Molecule_Header)){
                return Small_Molecule;
            }


            return null;
        }

        public static Section findSection(string name){
            if (name == null){
                return null;
            }

            if (name.Equals(Comment.Name) || name.Equals(Comment.Prefix)){
                return Comment;
            }
            if (name.Equals(Metadata.Name) || name.Equals(Metadata.Prefix)){
                return Metadata;
            }
            if (name.Equals(Peptide_Header.Name) || name.Equals(Peptide_Header.Prefix)){
                return Peptide_Header;
            }
            if (name.Equals(Peptide.Name) || name.Equals(Peptide.Prefix)){
                return Peptide;
            }
            if (name.Equals(Protein_Header.Name) || name.Equals(Protein_Header.Prefix)){
                return Protein_Header;
            }
            if (name.Equals(Protein.Name) || name.Equals(Protein.Prefix)){
                return Protein;
            }
            if (name.Equals(PSM_Header.Name) || name.Equals(PSM_Header.Prefix)){
                return PSM_Header;
            }
            if (name.Equals(PSM.Name) || name.Equals(PSM.Prefix)){
                return PSM;
            }
            if (name.Equals(Small_Molecule_Header.Name) || name.Equals(Small_Molecule_Header.Prefix)){
                return Small_Molecule_Header;
            }
            if (name.Equals(Small_Molecule.Name) || name.Equals(Small_Molecule.Prefix)){
                return Small_Molecule;
            }
            return null;
        }
    }
}