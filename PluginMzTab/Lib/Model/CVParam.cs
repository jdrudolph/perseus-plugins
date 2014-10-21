namespace PluginMzTab.Lib.Model{
/**
 * User: Qingwei
 * Date: 04/02/13
 */

    public class CVParam : Param{
        public CVParam(string cvLabel, string accession, string name, string value)
            : base(cvLabel, accession, name, value){}

        public new string CvLabel { get { return cvLabel; } }

        public new string Accession { get { return accession; } }

        public new string Name { get { return name; } }

        public new string Value { get { return value; } }
    }
}