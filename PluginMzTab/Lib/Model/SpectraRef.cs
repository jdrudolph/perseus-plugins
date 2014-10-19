using System;
using System.Text;

namespace PluginMzTab.Lib.Model{
/**
 * Peptides and small molecules MAY be linked to the source spectrum (in an external file)
 * from which the identifications are made by way of a reference in the spectra_ref attribute
 * and via the ms_file element which stores the URL of the file in the location attribute.
 * It is advantageous if there is a consistent system for identifying spectra in different file formats.
 * The following table is implemented in the PSI-MS CV for providing consistent identifiers for
 * different spectrum file formats. This is the exact same approach followed in mzIdentML and mzQuantML.
 */

    public class SpectraRef{
        /**
     * The msRun identifier
     */
        private readonly MsRun msRun;
        /**
     * Reference to the spectrum in the
     * msRun.
     */
        private readonly string reference;

        /**
     * Creates a new SpectraRef object.
     *
     * @param reference The reference to the spectrum in the MS fiile.
     */

        public SpectraRef(MsRun msRun, string reference){
            if (msRun == null){
                throw new NullReferenceException("msRun can not null!");
            }
            if (reference == null){
                throw new NullReferenceException("msRun reference can not empty!");
            }

            this.msRun = msRun;
            this.reference = reference;
        }

        public MsRun MsRun { get { return msRun; } }

        public string Reference { get { return reference; } }

        public override string ToString(){
            StringBuilder sb = new StringBuilder();

            sb.Append(msRun.Reference).Append(MZTabConstants.COLON).Append(reference);

            return sb.ToString();
        }
    }
}