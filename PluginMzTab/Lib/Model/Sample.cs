using System.Collections.Generic;
using System.Text;

namespace PluginMzTab.Lib.Model{

    public class Sample : IndexedElement{
        private readonly List<Param> _speciesList = new List<Param>();
        private readonly List<Param> _tissueList = new List<Param>();
        private readonly List<Param> _cellTypeList = new List<Param>();
        private readonly List<Param> _diseaseList = new List<Param>();
        private string _description;
        private readonly List<Param> _customList = new List<Param>();

        public Sample(int id) : base(MetadataElement.SAMPLE, id){}

        public List<Param> SpeciesList { get { return _speciesList; } }

        public List<Param> TissueList { get { return _tissueList; } }

        public List<Param> CellTypeList { get { return _cellTypeList; } }

        public List<Param> DiseaseList { get { return _diseaseList; } }

        public string Description { get { return _description; } set { _description = value; } }

        public List<Param> CustomList { get { return _customList; } }

        public void AddSpecies(Param param){
            _speciesList.Add(param);
        }

        public void AddTissue(Param param){
            _tissueList.Add(param);
        }

        public void AddCellType(Param param){
            _cellTypeList.Add(param);
        }

        public void AddDisease(Param param){
            _diseaseList.Add(param);
        }

        public void AddCustom(Param custom){
            _customList.Add(custom);
        }

        public override string ToString(){
            StringBuilder sb = new StringBuilder();

            sb = printList(_speciesList, MetadataProperty.SAMPLE_SPECIES, sb);
            sb = printList(_tissueList, MetadataProperty.SAMPLE_TISSUE, sb);
            sb = printList(_cellTypeList, MetadataProperty.SAMPLE_CELL_TYPE, sb);
            sb = printList(_diseaseList, MetadataProperty.SAMPLE_DISEASE, sb);

            if (_description != null){
                sb.Append(printProperty(MetadataProperty.SAMPLE_DESCRIPTION, _description))
                  .Append(MZTabConstants.NEW_LINE);
            }

            foreach (Param custom in _customList){
                sb.Append(printProperty(MetadataProperty.SAMPLE_CUSTOM, custom)).Append(MZTabConstants.NEW_LINE);
            }

            return sb.ToString();
        }
    }
}