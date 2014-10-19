using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using BaseLibS.Util;

namespace PluginMzTab.Lib.Model{
/**
 * {position}{Parameter}-{Type:accession}|{neutral loss}
 *
 * User: Qingwei
 * Date: 30/01/13
 */

    public class Modification : ICloneable{
        public enum ModificationType{
            MOD, //PSI-MOD
            UNIMOD,
            CHEMMOD,
            SUBST, //Substitution identifier
            UNKNOWN
        }

        private readonly SortedDictionary<int, CVParam> _positionMap = new SortedDictionary<int, CVParam>();
        private readonly Section _section;
        private readonly ModificationType _type;
        private readonly string _accession;
        private CVParam _neutralLoss;

        /**
     * {position}{Parameter}-{Type:accession}|{neutral loss}
     */
        public Modification(Section section, ModificationType type, string accession){
            if (! section.isData()){
                throw new ArgumentException("Section should use Protein, Peptide, PSM or SmallMolecule.");
            }
            _section = section;
            _type = type;

            if (accession == null){
                throw new NullReferenceException("Modification accession can not null!");
            }
            _accession = accession;
        }

        public Modification(Section section, ModificationType type, int accession)
            : this(section, type, accession.ToString(CultureInfo.InvariantCulture)){}


        /**
	     *. If the software has determined that there are no modifications to a given protein “0” MUST be used.
	     */
        public static Modification CreateNoModification(Section section){
            return new Modification(section, ModificationType.UNKNOWN, "0");
        }

        public Section Section { get { return _section; } }

        public void AddPosition(int id, CVParam param){
            _positionMap.Add(id, param);
        }

        public SortedDictionary<int, CVParam> PositionMap { get { return _positionMap; } }

        public ModificationType Type { get { return _type; } }

        public string Accession { get { return _accession; } }

        public CVParam NeutralLoss { get { return _neutralLoss; } set { _neutralLoss = value; } }

        public override string ToString(){
            string bar = MZTabConstants.BAR.ToString(CultureInfo.InvariantCulture);

            StringBuilder sb = new StringBuilder();


            // no modification.
            if (_type == ModificationType.UNKNOWN){
                return _accession;
            }

            if (_positionMap.Count > 0){
                sb.Append(StringUtils.Concat(bar, _positionMap.Select(x => x.Key + "" + x.Value)));
                sb.Append(MZTabConstants.MINUS);
            }

            sb.Append(_type).Append(MZTabConstants.COLON).Append(_accession);
            if (_neutralLoss != null){
                sb.Append(bar).Append(_neutralLoss);
            }

            return sb.ToString();
        }

        public object Clone(){
            return new Modification(_section, _type, _accession){NeutralLoss = _neutralLoss};
        }

        public static ModificationType FindType(string name){
            if (name == null){
                return ModificationType.MOD;
            }

            ModificationType type;
            try{
                type = (ModificationType) Enum.Parse(typeof (ModificationType), name.Trim(), true);
            }
            catch (ArgumentException){
                return ModificationType.MOD;
            }

            return type;
        }
    }
}