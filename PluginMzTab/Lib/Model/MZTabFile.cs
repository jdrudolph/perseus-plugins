using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace PluginMzTab.Lib.Model{
/**
 * In the jmzTab core model, the MZTabFile class is the central entry point to manage the internal relationships
 * among the different sections in the file. It contains three key components: i) Metadata, which is a mandatory
 * meta-model that provides the definitions contained in the dataset included in the file; ii) {@link MZTabColumnFactory},
 * a factory class that can be used to generate stable {@link MZTabColumn} elements, and to add dynamically different
 * optional columns (like e.g. protein and peptide abundance related columns). The {@link Metadata} and {@link MZTabColumnFactory}
 * constitute the framework for the MZTabFile class; and iii) Consistency constraints among the different sections
 * of the model. For example, the MZTabFile class supports the iterative modification of the elements ‘{@link MsRun}’,
 * ‘{@link Sample}’, ‘{@link StudyVariable}’, and ‘{@link Assay}’ assigned numbers (1-n) and its location in the file,
 * maintaining the  internal consistency between the Metadata section and the optional elements in the table-based sections.
 * These methods are particularly useful when information coming from different experiments (e.g. ms runs) is
 * condensed in a single mzTab file.
 */

    public class MZTabFile{
        // The metadata section.
        private readonly Metadata metadata;

        // header line section.
        private MZTabColumnFactory _proteinColumnFactory;
        private MZTabColumnFactory _peptideColumnFactory;
        private MZTabColumnFactory _psmColumnFactory;
        private MZTabColumnFactory _smallMoleculeColumnFactory;

        // The line number indexed sorted map.
        private readonly SortedDictionary<int, Comment> comments = new SortedDictionary<int, Comment>();
        private readonly SortedDictionary<int, Protein> proteins = new SortedDictionary<int, Protein>();
        private readonly SortedDictionary<int, Peptide> peptides = new SortedDictionary<int, Peptide>();
        private readonly SortedDictionary<int, PSM> psms = new SortedDictionary<int, PSM>();

        private readonly SortedDictionary<int, SmallMolecule> smallMolecules =
            new SortedDictionary<int, SmallMolecule>();

        /**
         * Create a MZTabFile with defined metadata.
         *
         * @param metadata SHOULD NOT set null.
         */

        public MZTabFile(Metadata metadata){
            if (metadata == null){
                throw new NullReferenceException("Metadata should be created first.");
            }

            this.metadata = metadata;
        }

        /**
         * Get all comment line in mzTab. Comment lines can be placed anywhere in an mzTab file. These lines must
         * start with the three-letter code COM and are ignored by most parsers. Empty lines can also occur anywhere
         * in an mzTab file and are ignored.
         *
         * @return a unmodifiable collection.
         */

        public ReadOnlyCollection<Comment> getComments(){
            return new ReadOnlyCollection<Comment>(comments.Values.ToList());
        }

        /**
         * Get the metadata section can provide additional information about the dataset(s) reported in the mzTab file.
         */

        public Metadata getMetadata(){
            return metadata;
        }

        /**
         * Get the Protein header line column factory.
         */

        public MZTabColumnFactory getProteinColumnFactory(){
            return _proteinColumnFactory;
        }

        /**
         * Get the Peptide header line column factory.
         */

        public MZTabColumnFactory getPeptideColumnFactory(){
            return _peptideColumnFactory;
        }

        /**
         * Get the PSM header line column factory.
         */

        public MZTabColumnFactory getPsmColumnFactory(){
            return _psmColumnFactory;
        }

        /**
         * Get the Small Molecule header line column factory.
         */

        public MZTabColumnFactory getSmallMoleculeColumnFactory(){
            return _smallMoleculeColumnFactory;
        }

        /**
         * Set the Protein header line column factory.
         *
         * @param proteinColumnFactory if null, system will ignore Protein table output.
         */

        public void setProteinColumnFactory(MZTabColumnFactory proteinColumnFactory){
            _proteinColumnFactory = proteinColumnFactory;
        }

        /**
         * Set the Peptide header line column factory.
         *
         * @param peptideColumnFactory if null, system will ignore Peptide table output.
         */

        public void setPeptideColumnFactory(MZTabColumnFactory peptideColumnFactory){
            _peptideColumnFactory = peptideColumnFactory;
        }

        /**
     * Set the PSM header line column factory.
     *
     * @param psmColumnFactory if null, system will ignore PSM table output.
     */

        public void setPSMColumnFactory(MZTabColumnFactory psmColumnFactory){
            _psmColumnFactory = psmColumnFactory;
        }

        /**
     * Set the Small Molecule header line column factory.
     *
     * @param smallMoleculeColumnFactory if null, system will ignore Small Molecule table output.
     */

        public void setSmallMoleculeColumnFactory(MZTabColumnFactory smallMoleculeColumnFactory){
            _smallMoleculeColumnFactory = smallMoleculeColumnFactory;
        }

        /**
     * Add a Protein record.
     *
     * @param protein SHOULD NOT set null.
     */

        public void addProtein(Protein protein){
            if (protein == null){
                throw new NullReferenceException("Protein record is null!");
            }

            int lineNumber = proteins.Count == 0 ? 1 : proteins.Last().Key + 1;
            proteins.Add(lineNumber, protein);
        }

        /**
     * Add a Protein record.
     *
     * @param lineNumber SHOULD be positive integer
     * @param protein SHOULD NOT set null.
     *
     * @throws ArgumentException  if there exists Protein object for assigned lineNumber
     */

        public void addProtein(int lineNumber, Protein protein){
            if (protein == null){
                throw new NullReferenceException("Protein record is null!");
            }
            if (lineNumber <= 0){
                throw new ArgumentException("Line number should be positive integer");
            }
            if (proteins.ContainsKey(lineNumber)){
                throw new ArgumentException("There already exist protein record in line number " + lineNumber);
            }

            proteins.Add(lineNumber, protein);
        }

        /**
     * Add a Peptide record.
     *
     * @param peptide SHOULD NOT set null.
     */

        public void addPeptide(Peptide peptide){
            if (peptide == null){
                throw new NullReferenceException("Peptide record is null!");
            }

            int position = peptides.Count == 0 ? 1 : peptides.Last().Key + 1;
            peptides.Add(position, peptide);
        }

        /**
     * Add a Peptide record.
     *
     * @param lineNumber SHOULD be positive integer
     * @param peptide SHOULD NOT set null.
     *
     * @throws ArgumentException  if there exists Peptide object for assigned lineNumber
     */

        public void addPeptide(int lineNumber, Peptide peptide){
            if (peptide == null){
                throw new NullReferenceException("Peptide record is null!");
            }
            if (lineNumber <= 0){
                throw new ArgumentException("Line number should be positive integer");
            }
            if (peptides.ContainsKey(lineNumber)){
                throw new ArgumentException("There already exist peptide record in line number " + lineNumber);
            }

            peptides.Add(lineNumber, peptide);
        }

        /**
     * Add a PSM record.
     *
     * @param psm SHOULD NOT set null.
     */

        public void addPSM(PSM psm){
            if (psm == null){
                throw new NullReferenceException("PSM record is null!");
            }

            int position = psms.Count == 0 ? 1 : psms.Last().Key + 1;
            psms.Add(position, psm);
        }

        /**
     * Add a PSM record.
     *
     * @param lineNumber SHOULD be positive integer
     * @param psm SHOULD NOT set null.
     *
     * @throws ArgumentException  if there exists PSM object for assigned lineNumber
     */

        public void addPSM(int lineNumber, PSM psm){
            if (psm == null){
                throw new NullReferenceException("PSM record is null!");
            }
            if (lineNumber <= 0){
                throw new ArgumentException("Line number should be positive integer");
            }
            if (psms.ContainsKey(lineNumber)){
                throw new ArgumentException("There already exist PSM record in line number " + lineNumber);
            }

            psms.Add(lineNumber, psm);
        }

        /**
     * Add a Small Molecule record.
     *
     * @param smallMolecule SHOULD NOT set null.
     */

        public void addSmallMolecule(SmallMolecule smallMolecule){
            if (smallMolecule == null){
                throw new NullReferenceException("Small Molecule record is null!");
            }

            int position = smallMolecules.Count == 0 ? 1 : smallMolecules.Last().Key + 1;
            smallMolecules.Add(position, smallMolecule);
        }

        /**
     * Add a SmallMolecule record.
     *
     * @param lineNumber SHOULD be positive integer
     * @param smallMolecule SHOULD NOT set null.
     *
     * @throws ArgumentException  if there exists SmallMolecule object for assigned lineNumber
     */

        public void addSmallMolecule(int lineNumber, SmallMolecule smallMolecule){
            if (smallMolecule == null){
                throw new NullReferenceException("Small Molecule record is null!");
            }
            if (lineNumber <= 0){
                throw new ArgumentException("Line number should be positive integer");
            }
            if (smallMolecules.ContainsKey(lineNumber)){
                throw new ArgumentException("There already exist small molecule record in line number " +
                                            lineNumber);
            }

            smallMolecules.Add(lineNumber, smallMolecule);
        }

        /**
     * Add a Comment record.
     *
     * @param lineNumber SHOULD be positive integer
     * @param comment SHOULD NOT set null.
     *
     * @throws ArgumentException  if there exists Protein object for assigned lineNumber
     */

        public void addComment(int lineNumber, Comment comment){
            if (comment == null){
                throw new NullReferenceException("Comment record is null!");
            }
            if (lineNumber <= 0){
                throw new ArgumentException("Line number should be positive integer");
            }
            if (comments.ContainsKey(lineNumber)){
                throw new ArgumentException("There already exist comment in line number " + lineNumber);
            }

            comments.Add(lineNumber, comment);
        }

        /**
     * Returns all proteins identified by the given accession.
     *
     * @param accession The accession identifying the proteins.
     * @return A unmodifiable collection of proteins identified by the given accession.
     */

        public ReadOnlyCollection<Protein> getProteins(String accession){
            List<Protein> result = new List<Protein>();

            foreach (Protein record in proteins.Values){
                if (record.Accession.Equals(accession)){
                    result.Add(record);
                }
            }

            return new ReadOnlyCollection<Protein>(result);
        }

        /**
     * Returns a Collection holding all proteins identified in this mzTabFile.
     *
     * @return A unmodifiable collection of proteins
     */

        public ReadOnlyCollection<Protein> getProteins(){
            return new ReadOnlyCollection<Protein>(proteins.Values.ToList());
        }

        public ReadOnlyDictionary<int, Protein> getProteinsWithLineNumber(){
            return new ReadOnlyDictionary<int, Protein>(proteins);
        }

        /**
     * Returns a Collection holding all peptides found in the mzTab file.
     *
     * @return A unmodifiable collection of peptides.
     */

        public ReadOnlyCollection<Peptide> getPeptides(){
            return new ReadOnlyCollection<Peptide>(peptides.Values.ToList());
        }

        /**
     * Returns a Collection holding all PSMs found in the mzTab file.
     *
     * @return A unmodifiable collection of PSMs.
     */

        public ReadOnlyCollection<PSM> getPSMs(){
            return new ReadOnlyCollection<PSM>(psms.Values.ToList());
        }

        /**
     * Returns all SmallMoleculeS identified in the mzTab file.
     *
     * @return A unmodifiable collection of SmallMolecules
     */

        public ReadOnlyCollection<SmallMolecule> getSmallMolecules(){
            return new ReadOnlyCollection<SmallMolecule>(smallMolecules.Values.ToList());
        }

        /**
     * Judge there exists records in MZTabFile or not.
     */

        public bool isEmpty(){
            return proteins.Count == 0 && peptides.Count == 0 && psms.Count == 0 && smallMolecules.Count == 0;
        }

        /**
     * Print MZTabFile into a output stream.
     *
     * @param out SHOULD NOT be null
     */

        public void printMZTab(TextWriter output){
            if (output == null){
                throw new NullReferenceException("Output stream should be defined first.");
            }

            if (isEmpty()){
                return;
            }
            output.Write(metadata.ToString());

            output.Write(MZTabConstants.NEW_LINE);

            // print comment
            foreach (Comment comment in comments.Values){
                output.Write(comment.ToString());
                output.Write(MZTabConstants.NEW_LINE);
            }
            if (comments.Count != 0){
                output.Write(MZTabConstants.NEW_LINE);
            }

            // print protein
            if (_proteinColumnFactory != null && proteins.Count != 0){
                output.Write(_proteinColumnFactory.ToString());
                output.Write(MZTabConstants.NEW_LINE);
                foreach (Protein protein in proteins.Values){
                    output.Write(protein.ToString());
                    output.Write(MZTabConstants.NEW_LINE);
                }

                output.Write(MZTabConstants.NEW_LINE);
            }

            // print peptide
            if (_peptideColumnFactory != null && peptides.Count != 0){
                output.Write(_peptideColumnFactory.ToString());
                output.Write(MZTabConstants.NEW_LINE);

                foreach (Peptide peptide in peptides.Values){
                    output.Write(peptide.ToString());
                    output.Write(MZTabConstants.NEW_LINE);
                }
                output.Write(MZTabConstants.NEW_LINE);
            }

            // print PSM
            if (_psmColumnFactory != null && psms.Count != 0){
                output.Write(_psmColumnFactory.ToString());
                output.Write(MZTabConstants.NEW_LINE);

                foreach (PSM psm in psms.Values){
                    output.Write(psm.ToString());
                    output.Write(MZTabConstants.NEW_LINE);
                }
                output.Write(MZTabConstants.NEW_LINE);
            }

            // print small molecule
            if (_smallMoleculeColumnFactory != null && smallMolecules.Count != 0){
                output.Write(_smallMoleculeColumnFactory.ToString());
                output.Write(MZTabConstants.NEW_LINE);

                foreach (SmallMolecule smallMolecule in smallMolecules.Values){
                    output.Write(smallMolecule.ToString());
                    output.Write(MZTabConstants.NEW_LINE);
                }
                output.Write(MZTabConstants.NEW_LINE);
            }
        }

        /**
     * Translate a MZTabFile into a string.
     */

        public String toString(){
            StringBuilder sb = new StringBuilder();

            // print comment
            foreach (Comment comment in comments.Values){
                sb.Append(comment).Append(MZTabConstants.NEW_LINE);
            }
            if (comments.Count != 0){
                sb.Append(MZTabConstants.NEW_LINE);
            }

            sb.Append(metadata).Append(MZTabConstants.NEW_LINE);

            if (_proteinColumnFactory != null){
                sb.Append(_proteinColumnFactory).Append(MZTabConstants.NEW_LINE);
                foreach (Protein protein in proteins.Values){
                    sb.Append(protein).Append(MZTabConstants.NEW_LINE);
                }
                sb.Append(MZTabConstants.NEW_LINE);
            }

            if (_peptideColumnFactory != null){
                sb.Append(_peptideColumnFactory).Append(MZTabConstants.NEW_LINE);
                foreach (Peptide peptide in peptides.Values){
                    sb.Append(peptide).Append(MZTabConstants.NEW_LINE);
                }
                sb.Append(MZTabConstants.NEW_LINE);
            }

            if (_psmColumnFactory != null){
                sb.Append(_psmColumnFactory).Append(MZTabConstants.NEW_LINE);
                foreach (PSM psm in psms.Values){
                    sb.Append(psm).Append(MZTabConstants.NEW_LINE);
                }
                sb.Append(MZTabConstants.NEW_LINE);
            }

            if (_smallMoleculeColumnFactory != null){
                sb.Append(_smallMoleculeColumnFactory).Append(MZTabConstants.NEW_LINE);
                foreach (SmallMolecule smallMolecule in smallMolecules.Values){
                    sb.Append(smallMolecule).Append(MZTabConstants.NEW_LINE);
                }
                sb.Append(MZTabConstants.NEW_LINE);
            }

            return sb.ToString();
        }
    }
}