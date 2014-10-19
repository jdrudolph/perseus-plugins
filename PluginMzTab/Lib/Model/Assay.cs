using System;
using System.Collections.Generic;
using System.Text;

namespace PluginMzTab.Lib.Model{

    public class Assay : IndexedElement{
        private Param quantificationReagent;
        private Sample sample;
        private MsRun msRun;

        private SortedDictionary<int, AssayQuantificationMod> quantificationModMap =
            new SortedDictionary<int, AssayQuantificationMod>();

        public Assay(int id) : base(MetadataElement.ASSAY, id){}

        public Sample Sample { get { return sample; } set { sample = value; } }

        public MsRun MsRun { get { return msRun; } set { msRun = value; } }

        public SortedDictionary<int, AssayQuantificationMod> QuantificationModMap { get { return quantificationModMap; } }

        public Param QuantificationReagent { get { return quantificationReagent; } set { quantificationReagent = value; } }

        public void setQuantificationModMap(SortedDictionary<int, AssayQuantificationMod> quantificationModMap){
            if (quantificationModMap == null){
                quantificationModMap = new SortedDictionary<int, AssayQuantificationMod>();
            }

            this.quantificationModMap = quantificationModMap;
        }

        public void addQuantificationMod(AssayQuantificationMod mod){
            if (mod == null){
                throw new Exception("AssayQuantificationMod should not be null");
            }

            quantificationModMap.Add(mod.Id, mod);
        }

        public void addQuantificationModParam(int id, Param param){
            AssayQuantificationMod mod = null;
            if (quantificationModMap.ContainsKey(id)){
                mod = quantificationModMap[id];
            }
            if (mod == null){
                mod = new AssayQuantificationMod(this, id){Param = param};
                quantificationModMap.Add(id, mod);
            }
            else{
                mod.Param = param;
            }
        }

        public void addQuantificationModSite(int id, string site){
            AssayQuantificationMod mod = quantificationModMap[id];
            if (mod == null){
                mod = new AssayQuantificationMod(this, id);
                mod.Site = site;
                quantificationModMap.Add(id, mod);
            }
            else{
                mod.Site = site;
            }
        }

        public void addQuantificationModPosition(int id, string position){
            AssayQuantificationMod mod = quantificationModMap[id];
            if (mod == null){
                mod = new AssayQuantificationMod(this, id);
                mod.Position = position;
                quantificationModMap.Add(id, mod);
            }
            else{
                mod.Position = (position);
            }
        }

        public override string ToString(){
            StringBuilder sb = new StringBuilder();

            if (quantificationReagent != null){
                printPrefix(sb)
                    .Append(Reference)
                    .Append(MZTabConstants.MINUS)
                    .Append(MetadataProperty.ASSAY_QUANTIFICATION_REAGENT);
                sb.Append(MZTabConstants.TAB).Append(quantificationReagent).Append(MZTabConstants.NEW_LINE);
            }

            if (sample != null){
                printPrefix(sb).Append(Reference).Append(MZTabConstants.MINUS).Append(MetadataProperty.ASSAY_SAMPLE_REF);
                sb.Append(MZTabConstants.TAB).Append(sample.Reference).Append(MZTabConstants.NEW_LINE);
            }

            if (msRun != null){
                printPrefix(sb)
                    .Append(Reference)
                    .Append(MZTabConstants.MINUS)
                    .Append(MetadataProperty.ASSAY_MS_RUN_REF);
                sb.Append(MZTabConstants.TAB).Append(msRun.Reference).Append(MZTabConstants.NEW_LINE);
            }

            foreach (AssayQuantificationMod mod in quantificationModMap.Values){
                sb.Append(mod);
            }

            return sb.ToString();
        }
    }
}