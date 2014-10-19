using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using BaseLib.Util;

namespace PluginMzTab.Lib.Model{
    public class StudyVariable : IndexedElement{
        private string description;
        private SortedDictionary<int, Assay> assayMap = new SortedDictionary<int, Assay>();
        private SortedDictionary<int, Sample> sampleMap = new SortedDictionary<int, Sample>();

        public StudyVariable(int id)
            : base(MetadataElement.STUDY_VARIABLE, id){}

        public string Description { get { return description; } set { description = value; } }

        public SortedDictionary<int, Assay> AssayMap { get { return assayMap; } }

        public SortedDictionary<int, Sample> SampleMap { get { return sampleMap; } }

        public void AddAssay(int pid, Assay assay){
            assayMap.Add(pid, assay);
        }

        public void AddAssay(Assay assay){
            assayMap.Add(assay.Id, assay);
        }

        public void AddAllAssays(IList<Assay> assays){
            foreach (Assay assay in assays){
                AddAssay(assay);
            }
        }

        public void AddSample(int pid, Sample sample){
            sampleMap.Add(pid, sample);
        }

        public void AddSample(Sample sample){
            if (sampleMap.ContainsKey(sample.Id)){
                return;
            }
            sampleMap.Add(sample.Id, sample);
        }

        public void AddAllSamples(IList<Sample> samples){
            foreach (Sample sample in samples){
                AddSample(sample);
            }
        }

        private string COMMA { get { return MZTabConstants.COMMA.ToString(CultureInfo.InvariantCulture) + " "; } }

        public string SampleRef { get { return SampleMap.Count == 0 ? "" : StringUtils.Concat(COMMA, SampleMap.Values.Select(x => x.Reference)); } }

        public string AssayRef { get { return AssayMap.Count == 0 ? "" : StringUtils.Concat(COMMA, AssayMap.Values.Select(x => x.Reference)); } }

        public override string ToString(){
            StringBuilder sb = new StringBuilder();

            if (description != null){
                sb.Append(printProperty(MetadataProperty.STUDY_VARIABLE_DESCRIPTION, description))
                  .Append(MZTabConstants.NEW_LINE);
            }

            if (!string.IsNullOrEmpty(AssayRef)){
                printPrefix(sb)
                    .Append(Reference)
                    .Append(MZTabConstants.MINUS)
                    .Append(MetadataProperty.STUDY_VARIABLE_ASSAY_REFS);
                sb.Append(MZTabConstants.TAB).Append(AssayRef).Append(MZTabConstants.NEW_LINE);
            }


            if (!string.IsNullOrEmpty(SampleRef)){
                printPrefix(sb)
                    .Append(Reference)
                    .Append(MZTabConstants.MINUS)
                    .Append(MetadataProperty.STUDY_VARIABLE_SAMPLE_REFS);
                sb.Append(MZTabConstants.TAB).Append(SampleRef).Append(MZTabConstants.NEW_LINE);
            }

            return sb.ToString();
        }
    }
}