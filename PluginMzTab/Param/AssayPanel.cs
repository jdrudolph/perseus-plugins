using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BaseLib.Mol;
using BaseLib.Util;
using BaseLib.Wpf;
using BaseLibS.Util;
using MzTabLibrary.model;
using PluginMzTab.extended;
using PluginMzTab.utils;

namespace PluginMzTab.param{
    public abstract class AssayPanel : StackPanel
    {
        public abstract Assay[] Value { get; set; }

        protected readonly List<string> _modifications = new List<string>(Tables.LabelModifications.Keys);

        public static IList<Assay> UniqueGroups(IList<Assay> assays){
            int[] reference;
            return UniqueGroups(assays, out reference);
        }

        public static IList<Assay> UniqueGroups(IList<Assay> assays, out int[] reference){
            reference = new int[assays.Count];

            IList<string> reagentList = new List<string>();
            IList<string> modificationList = new List<string>();

            IList<Assay> groups = new List<Assay>();
            for (int i = 0; i < assays.Count; i++){
                var a = assays[i];

                string reagent = a.QuantificationReagent.Name;
                string modString = "";
                if (a.QuantificationModMap != null){
                    modString = StringUtils.Concat("&&", a.QuantificationModMap.Where(x => x.Value != null && x.Value.Param != null).Select(x => x.Value.Param.Name));
                }
                bool add = false;
                if (!reagentList.Contains(reagent)){
                    add = true;
                    reagentList.Add(reagent);
                    modificationList.Add(modString);
                }
                if (!modificationList.Contains(modString)){
                    add = true;
                    reagentList.Add(reagent);
                    modificationList.Add(modString);
                }

                for (int j = 0; j < reagentList.Count && j < modificationList.Count; j++){
                    if (reagentList[j].Equals(reagent) || modificationList[j].Equals(modString)){
                        reference[i] = j;
                        break;
                    }
                }

                if (!add){
                    continue;
                }

                Assay assay = new Assay(groups.Count + 1){QuantificationReagent = a.QuantificationReagent};
                if (a.QuantificationModMap != null){
                    foreach (var m in a.QuantificationModMap.Values){
                        assay.addQuantificationMod(m);
                    }
                }
                groups.Add(assay);
            }
            return groups;
        }

        protected Assay GetAssay(int id, int _count, string text, CVLookUp _cv){
            Assay assay = new Assay(id);

            if (!string.IsNullOrEmpty(text)){
                assay.QuantificationReagent = _cv.GetParam(text, "PRIDE");
            }

            return assay;
        }

        public static string[] UniqueSamples(Assay[] value){
            if (value != null)
            {
                Sample[] samples = value.Where(x => x.Sample != null).Select(x => x.Sample).ToArray();
                string[] temp = samples.Select(x => x.Description).ToArray();
                return ArrayUtils.UniqueValues(temp);
            }
            return new string[0];
        }

        protected IEnumerable<AssayQuantificationMod> GetModifications(Assay assay, ListSelectorControl listBox, CVLookUp cv)
        {
            List<AssayQuantificationMod> result = new List<AssayQuantificationMod>();

            foreach (var item in listBox.SelectedItems){
                string name = item.ToString();
                Param param = cv.GetParam(name, "MOD");

                if (param == null){
                    continue;
                }

                string position = null;
                string site = null;
                if (Tables.Modifications.ContainsKey(name)){
                    var m = Tables.Modifications[name];
                    if (m != null){
                        switch (m.Position){
                            case ModificationPosition.anywhere:
                                position = "Anywhere";
                                break;
                            case ModificationPosition.anyNterm:
                                position = "Any N-term";
                                break;
                            case ModificationPosition.anyCterm:
                                position = "Any C-term";
                                break;
                            case ModificationPosition.proteinNterm:
                                position = "Protein N-term";
                                break;
                            case ModificationPosition.proteinCterm:
                                position = "Protein C-term";
                                break;
                            default:
                                position = m.Position.ToString();
                                break;
                        }
                        site = m.GetSiteArray() != null ? StringUtils.Concat("|", m.GetSiteArray()) : "-";
                    }
                }

                result.Add(new AssayQuantificationMod(assay, result.Count + 1) { Param = param, Position = position, Site = site});
            }
            return result;
        }

        protected void SetModifications(Assay assay, ListSelectorControl listbox){
            IList<int> indizes = new List<int>();
            if (assay.QuantificationModMap != null && assay.QuantificationModMap.Count > 0)
            {
                foreach (var mod in assay.QuantificationModMap.Values)
                {
                    if (mod == null || mod.Param == null || mod.Param.Name == null)
                    {
                        continue;
                    }
                    int index = MzTabMatrixUtils.GetSelectedIndex(mod.Param.Name, listbox.Items);
                    if (index == -1)
                    {
                        continue;
                    }
                    indizes.Add(index);
                }
            }
            listbox.SelectedIndices = indizes.ToArray();
        }
    }

    public class AssayPanel1 : AssayPanel{
        private readonly int _count;

        private readonly CVLookUp _cv;

        private readonly IList<ComboBox> _reagents = new List<ComboBox>();
        private readonly IList<ListSelectorControl> _mods = new List<ListSelectorControl>();
        
        private int[] msrunReference;
        private int[] sampleReference;
        private int[] groupReference;

        public AssayPanel1(int count, Assay[] value, CVLookUp cv){
            _cv = cv;
            _count = count;
            
            InitializeComponent();

            Value = value;
        }

        public void InitializeComponent(){
            _reagents.Clear();

            Grid grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(Constants.LabelHeight)});
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(Math.Max(Constants.ComboBoxHeight, Constants.LabelHeight)) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(Math.Max(Constants.ListSelectorHeight, Constants.LabelHeight)) });

            grid.ColumnDefinitions.Add(new ColumnDefinition{Width = new GridLength(1, GridUnitType.Auto)});
            for (int i = 0; i < _count; i++){
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto), MinWidth = 200 });
            }

            IList<string> reagents = _cv.GetQuantReagents();

            Label label1 = new Label { Content = MetadataProperty.ASSAY_QUANTIFICATION_REAGENT.Name, HorizontalContentAlignment = HorizontalAlignment.Left, VerticalContentAlignment = VerticalAlignment.Top };
            Grid.SetRow(label1, 1);
            Grid.SetColumn(label1, 0);
            grid.Children.Add(label1);

            Label label2 = new Label { Content = MetadataSubElement.ASSAY_QUANTIFICATION_MOD.Name, HorizontalContentAlignment = HorizontalAlignment.Left, VerticalContentAlignment = VerticalAlignment.Top};
            Grid.SetRow(label2, 2);
            Grid.SetColumn(label2, 0);
            grid.Children.Add(label2);

            string[] bins = new string[_count];
            for (int n = 0; n < _count; n++){
                string name = string.Format("GROUP {0}", (n + 1));
                bins[n] = name;
                Label label = new Label { Content = name, HorizontalContentAlignment = HorizontalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Top, FontWeight = FontWeights.Bold };
                Grid.SetRow(label, 0);
                Grid.SetColumn(label, n + 1);
                grid.Children.Add(label);

                ComboBox reagent = new ComboBox();
                foreach (var r in reagents){
                    reagent.Items.Add(r);
                }
                Grid.SetRow(reagent, 1);
                Grid.SetColumn(reagent, n + 1);
                grid.Children.Add(reagent);
                _reagents.Add(reagent);

                ListSelectorControl mod = new ListSelectorControl();
                foreach (var m in _modifications){
                    mod.Items.Add(m);
                }
                Grid.SetRow(mod, 2);
                Grid.SetColumn(mod, n+1);
                grid.Children.Add(mod);
                _mods.Add(mod);
            }

            Children.Add(grid);
        }

        public override sealed Assay[] Value{
            get{
                if (groupReference == null){
                    return null;
                }

                IList<Assay> result =new List<Assay>();

                for (int i = 0; i < groupReference.Length; i++){
                    int n = groupReference[i];
                    Assay assay = null;
                    if (_count > n){
                        assay = GetAssay(i + 1, _count, _reagents[n].Text, _cv);

                        int m = 1;
                        foreach (var item in _mods[n].SelectedItems){
                            assay.addQuantificationMod(new AssayQuantificationMod(assay, m++) { Param = _cv.GetParam(item.ToString(), "MOD") });
                        }
                    }
                    
                    if (assay == null){
                        continue;
                    }

                    if (msrunReference[i] != -1){
                        assay.MsRun = new MsRunImpl(msrunReference[i]);
                    }

                    if (sampleReference[i] != -1){
                        assay.Sample = new Sample(sampleReference[i]);
                    }

                    result.Add(assay);
                }

                return result.ToArray();
            }
            set{
                IList<Assay> assays = value;
                if (assays == null || assays.Count == 0){
                    return;
                }

                IList<Assay> groups = UniqueGroups(assays);

                if (groups == null){
                    return;
                }

                msrunReference = new int[assays.Count];
                sampleReference = new int[assays.Count];
                groupReference = new int[assays.Count];
                for (int i = 0; i < assays.Count; i++){
                    groupReference[i] = MzTabMatrixUtils.IndexOf(groups, assays[i]);
                    msrunReference[i] = assays[i].MsRun == null ? -1 : assays[i].MsRun.Id;
                    sampleReference[i] = assays[i].Sample == null ? -1 : assays[i].Sample.Id;
                }

                for (int i = 0; i < _count; i++){
                    if(_reagents[i].SelectedIndex == -1){
                        _reagents[i].SelectedIndex = MzTabMatrixUtils.GetSelectedIndex(_cv.GetNameOfTerm("PRIDE:0000434", "PRIDE"), _reagents[i].Items);
                    }

                    if (i >= groups.Count || groups[i] == null){
                        continue;
                    }

                    if (groups[i].QuantificationReagent != null){
                        _reagents[i].SelectedIndex = MzTabMatrixUtils.GetSelectedIndex(groups[i].QuantificationReagent.Name, _reagents[i].Items);
                    }

                    SetModifications(groups[i], _mods[i]);
                }
            }
        }

        public static int MinimumHeight(){
            return 1*Constants.LabelHeight + 1*Math.Max(Constants.ComboBoxHeight, Constants.LabelHeight) + 1*Constants.ListSelectorHeight;
        }
    }

    public class AssayPanel2 : AssayPanel
    {
        private readonly int _count;
        private readonly CVLookUp _cv;
        
        private readonly string[] _uniqueSamples = new string[0];

        private readonly IList<Label> _groups = new List<Label>();
        private readonly IList<Label> _reagents = new List<Label>();
        private readonly IList<ListSelectorControl> _mods = new List<ListSelectorControl>();
        private readonly IList<MultiListSelectorControl> _msrunRefs = new List<MultiListSelectorControl>();
        
        private readonly List<string> _msRunText = new List<string>();
        private readonly List<MsRunImpl> _msRunList = new List<MsRunImpl>();
        
        private readonly List<string> _sampleText = new List<string>();
        private readonly List<Sample> _sampleList = new List<Sample>();

        public AssayPanel2(int count, Assay[] value, CVLookUp cv){
            _cv = cv;
            _count = count;

            foreach (var assay in value){
                if (assay == null){
                    continue;
                }

                if (assay.MsRun != null){
                    _msRunText.Add(MsRunToString(assay.MsRun));
                    _msRunList.Add(assay.MsRun as MsRunImpl);
                }

                if (assay.Sample != null){
                    _sampleText.Add(SampleToString(assay.Sample));
                    _sampleList.Add(assay.Sample);
                }
            }

            _uniqueSamples = ArrayUtils.UniqueValues(_sampleText);

            InitializeComponent();

            Value = value;
        }

        private string MsRunToString(MsRun msrun){
            return string.Format("[{0}] {1}", msrun.Id, msrun is MsRunImpl ? (msrun as MsRunImpl).Description : Path.GetFileNameWithoutExtension(msrun.Location.Value));
        }

        private string SampleToString(Sample sample){
            return string.Format("sample[{0}] {1}", sample.Id.ToString("00"), sample.Description);
        }

        private void InitializeComponent()
        {
            _reagents.Clear();
            _mods.Clear();

            int height = _uniqueSamples.Length*Constants.height;

            Grid grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            grid.RowDefinitions.Add(new RowDefinition{ Height = new GridLength(Constants.ListSelectorHeight, GridUnitType.Pixel) });
            grid.RowDefinitions.Add(new RowDefinition{ Height = new GridLength(height, GridUnitType.Pixel) });

            grid.ColumnDefinitions.Add(new ColumnDefinition{Width = new GridLength(1, GridUnitType.Auto)});
            for (int i = 0; i < _count; i++){
                grid.ColumnDefinitions.Add(new ColumnDefinition{ Width = new GridLength(100f / _count, GridUnitType.Star) });
            }

            Label label1 = new Label { Content = "quantification", HorizontalContentAlignment = HorizontalAlignment.Left, VerticalContentAlignment = VerticalAlignment.Top };
            Grid.SetRow(label1, 1);
            Grid.SetColumn(label1, 0);
            grid.Children.Add(label1);

            Label label2 = new Label { Content = "reagent", HorizontalContentAlignment = HorizontalAlignment.Left, VerticalContentAlignment = VerticalAlignment.Top };
            Grid.SetRow(label2, 2);
            Grid.SetColumn(label2, 0);
            grid.Children.Add(label2);

            Label label3 = new Label { Content = "references", HorizontalContentAlignment = HorizontalAlignment.Left, VerticalContentAlignment = VerticalAlignment.Top };
            Grid.SetRow(label3, 3);
            Grid.SetColumn(label3, 0);
            grid.Children.Add(label3);

            for (int n = 0; n < _count; n++){
                Label group = new Label { HorizontalContentAlignment = HorizontalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Top, FontWeight = FontWeights.Bold };
                Grid.SetRow(group, 0);
                Grid.SetColumn(group, n + 1);
                grid.Children.Add(group);
                _groups.Add(group);

                Label reagent = new Label{HorizontalContentAlignment = HorizontalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Top };
                Grid.SetRow(reagent, 1);
                Grid.SetColumn(reagent, n+1);
                grid.Children.Add(reagent);
                _reagents.Add(reagent);

                ListSelectorControl mod = new ListSelectorControl();
                foreach (var m in _modifications){
                    mod.Items.Add(m);
                }
                Grid.SetRow(mod, 2);
                Grid.SetColumn(mod, n + 1);
                grid.Children.Add(mod);
                _mods.Add(mod);

                MultiListSelectorControl msrunRef = new MultiListSelectorControl { MinHeight = height - 6 };
                msrunRef.Init(_msRunText, _uniqueSamples); 
                Grid.SetRow(msrunRef, 3);
                Grid.SetColumn(msrunRef, n+1);
                grid.Children.Add(msrunRef);
                _msrunRefs.Add(msrunRef);
            }

            Children.Add(grid);
        }

        public override sealed Assay[] Value
        {
            get
            {
                IList<Assay> result = new List<Assay>();
                for (int i = 0; i < _count; i++){
                    Param reagent = _cv.GetParam(_reagents[i].Content.ToString(), "PRIDE");

                    MultiListSelectorControl selector = _msrunRefs[i];
                    for (int j = 0; j < _uniqueSamples.Length; j++){

                        var runs = selector.GetSelectedIndices(j);
                        for (int k = 0; k < runs.Length; k++){
                            Assay assay = new Assay(result.Count + 1){QuantificationReagent = reagent};

                            int index = _msRunText.IndexOf(selector.items[runs[k]]);
                            if (index != -1){
                                assay.MsRun = _msRunList[index];
                            }

                            index = _sampleText.IndexOf(_uniqueSamples[j]);
                            if (index != -1){
                                assay.Sample = _sampleList[index];
                            }

                            foreach (var mod in GetModifications(assay, _mods[i], _cv)){
                                assay.addQuantificationMod(mod);
                            }

                            result.Add(assay);
                        }
                    }
                }

                return result.ToArray();
            }
            set
            {
                IList<Assay> assays = value;
                if (assays == null || assays.Count == 0){
                    return;
                }

                int[] reference;
                var groups = UniqueGroups(assays, out reference);

                if (groups == null){
                    return;
                }
                
                for (int i = 0; i < _count; i++){
                    if (i >= groups.Count || groups[i] == null){
                        _reagents[i].DataContext = "";
                        _mods[i].DataContext = "";
                        continue;
                    }
                    _groups[i].Content = string.Format("GROUP {0}", i + 1);
                    _reagents[i].Content = groups[i].QuantificationReagent == null ? "" : groups[i].QuantificationReagent.Name;

                    SetModifications(groups[i], _mods[i]);

                    int[][] msrunIndices = new int[_uniqueSamples.Length][];
                    for (int j = 0; j < _uniqueSamples.Length; j++){
                        var temp = ArrayUtils.IndicesOf(_sampleText, _uniqueSamples[j]);
                        if (temp.Length == 0){
                            msrunIndices[j] = new int[0];
                            continue;
                        }
                        msrunIndices[j] = temp.Where(t => reference[t] == i).ToArray();
                    }

                    _msrunRefs[i].SelectedIndices = msrunIndices;
                }                
            }
        }

        public static int MinimumHeight(int samples){
            return 1 * Constants.LabelHeight + 1 * Constants.ListSelectorHeight + samples * Constants.height;
        }
    }
}