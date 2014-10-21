using System.Text;

/**
 * The name of the instrument used in the experiment. Multiple instruments are numbered 1..n
 * The instrument's source used in the experiment. Multiple instruments are numbered 1..n
 * The instrument’s analyzer PublicationType used in the experiment. Multiple instruments are enumerated 1..n
 * The instrument's detector PublicationType used in the experiment. Multiple instruments are numbered 1..n
 *
 * User: Qingwei
 * Date: 23/05/13
 */

namespace PluginMzTab.Lib.Model{
    public class Instrument : IndexedElement{
        private Param name;
        private Param source;
        private Param analyzer;
        private Param detector;

        //Never used private int id;

        public Instrument(int id) : base(MetadataElement.INSTRUMENT, id){}

        public Param Name { get { return name; } set { name = value; } }

        public Param Source { get { return source; } set { source = value; } }

        public Param Analyzer { get { return analyzer; } set { analyzer = value; } }

        public Param Detector { get { return detector; } set { detector = value; } }

        public override string ToString(){
            StringBuilder sb = new StringBuilder();

            if (name != null){
                sb.Append(printProperty(MetadataProperty.INSTRUMENT_NAME, name)).Append(MZTabConstants.NEW_LINE);
            }
            if (source != null){
                sb.Append(printProperty(MetadataProperty.INSTRUMENT_SOURCE, source)).Append(MZTabConstants.NEW_LINE);
            }
            if (analyzer != null){
                sb.Append(printProperty(MetadataProperty.INSTRUMENT_ANALYZER, analyzer)).Append(MZTabConstants.NEW_LINE);
            }
            if (detector != null){
                sb.Append(printProperty(MetadataProperty.INSTRUMENT_DETECTOR, detector)).Append(MZTabConstants.NEW_LINE);
            }

            return sb.ToString();
        }
    }
}