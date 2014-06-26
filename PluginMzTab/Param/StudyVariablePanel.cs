using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BaseLib.Wpf;
using MzTabLibrary.model;
using PluginMzTab.extended;
using PluginMzTab.utils;

namespace PluginMzTab.param{
    public abstract class StudyVariablePanel : WrapPanel
    {
        public abstract StudyVariable[] Value { get; set; }
    }

    public class StudyVariablePanel1 : StudyVariablePanel{
        private readonly int _count;
        private const int numElement = 5;

        private int[][] sampleIndizes;
        private int[][] assayIndizes;

        private readonly IList<TextBox> _description = new List<TextBox>();

        public StudyVariablePanel1(StudyVariable[] value){
            _count = value.Length;

            MaxWidth = 750;

            InitializeComponent();
           
            Value = value;
        }

        private void InitializeComponent(){
            _description.Clear();

            for (int n = 0; n < _count; n++){
                Grid grid = new Grid();
                grid.RowDefinitions.Add(new RowDefinition());
                grid.RowDefinitions.Add(new RowDefinition());

                int c = 0;
                
                if (n%numElement == 0){
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });

                    Label label1 = new Label{Content = MetadataProperty.STUDY_VARIABLE_DESCRIPTION.Name, Width = 80};
                    Grid.SetRow(label1, 1);
                    Grid.SetColumn(label1, 0);
                    grid.Children.Add(label1);
                    c++;
                }

                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength((MaxWidth - 80) * 0.92 / numElement, GridUnitType.Pixel) });

                Label label = new Label { Content = string.Format("study variable[{0}]", (n + 1)), HorizontalContentAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Top, FontWeight = FontWeights.Bold };
                Grid.SetRow(label, 0);
                Grid.SetColumn(label, c);
                grid.Children.Add(label);
                
                TextBox description = new TextBox();
                Grid.SetRow(description, 1);
                Grid.SetColumn(description, c);
                grid.Children.Add(description);
                _description.Add(description);
                
                Children.Add(grid);
            }
        }

        public sealed override StudyVariable[] Value{
            get{
                IList<StudyVariable> result = new List<StudyVariable>();

                for (int n = 0; n < _count; n++){
                    StudyVariable variable = new StudyVariable(result.Count + 1) { Description = _description[n].Text };
                    result.Add(variable);

                    if (n < sampleIndizes.Length && sampleIndizes[n] != null){
                        for (int i = 0; i < sampleIndizes[n].Length; i++){
                            variable.AddSample(new Sample(sampleIndizes[n][i]));
                        }
                    }
                    if (n < assayIndizes.Length && assayIndizes[n] != null){
                        for (int i = 0; i < assayIndizes[n].Length; i++){
                            variable.AddAssay(new Assay(assayIndizes[n][i]));
                        }
                    }
                }

                return result.ToArray();
            }
            set{
                IList<StudyVariable> variables = value;
                if (variables == null || variables.Count == 0){
                    return;
                }

                sampleIndizes = new int[variables.Count][];
                assayIndizes = new int[variables.Count][];
                for (int i = 0; i < variables.Count; i++){
                    if (variables[i] == null){
                        continue;
                    }
                    sampleIndizes[i] = variables[i].SampleMap.Keys.Select(id => id).ToArray();
                    assayIndizes[i] = variables[i].AssayMap.Keys.Select(id => id).ToArray();                    
                }

                for (int i = 0; i < _count; i++){
                    if (i >= variables.Count || variables[i] == null){
                        _description[i].Text = "";
                        sampleIndizes[i] = new int[0];
                        assayIndizes[i] = new int[0];
                        continue;
                    }
                    _description[i].Text = variables[i].Description;
                }
            }
        }

        public static int MinimumHeight(int count){
            return (count / numElement + (count % numElement == 0 ? 0 : 1)) * (Constants.LabelHeight + Constants.TextBoxHeight + 6); 
        }
    }

    public class StudyVariablePanel2 : StudyVariablePanel{
        
        private readonly int _count;

        private readonly List<string> _assayText =new List<string>();
        private readonly List<Assay> _assayList = new List<Assay>();

        private readonly List<string> _sampleText = new List<string>();
        private readonly List<Sample> _sampleList = new List<Sample>();

        private readonly IList<Label> _description = new List<Label>();
        private readonly MultiListSelectorControl _sampleRefs;
        private readonly MultiListSelectorControl _assayRefs;

        public StudyVariablePanel2(StudyVariable[] value){
            HorizontalAlignment = HorizontalAlignment.Left;

            _count = value.Length;
            
            foreach (StudyVariable variable in value){
                if (variable.AssayMap != null){
                    foreach (var assay in variable.AssayMap.Values){
                        if (assay == null){
                            continue;
                        }
                        _assayText.Add(AssayToString(assay));
                        _assayList.Add(assay);                        
                    }
                }

                if (variable.SampleMap != null){
                    foreach (var sample in variable.SampleMap.Values)
                    {
                        if (sample == null)
                        {
                            continue;
                        }
                        _sampleText.Add(SampleToString(sample));
                        _sampleList.Add(sample);
                    }
                }
            }

            MaxWidth = 750;
            Width = MaxWidth;

            _sampleRefs = new MultiListSelectorControl { MinHeight = _count * Constants.height - 6, HorizontalAlignment = HorizontalAlignment.Stretch};
            _assayRefs = new MultiListSelectorControl  { MinHeight = _count * Constants.height - 6, HorizontalAlignment = HorizontalAlignment.Stretch};

            InitializeComponent();

            Value = value;
        }

        private void InitializeComponent(){
            Grid grid = new Grid{HorizontalAlignment = HorizontalAlignment.Stretch};

            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100, GridUnitType.Star) });
            
            for (int i = 0; i < _count; i++){
                _description.Add(new Label());
            }

            Label label1 = new Label{Content = MetadataProperty.STUDY_VARIABLE_SAMPLE_REFS.Name};
            Grid.SetRow(label1, 0);
            Grid.SetColumn(label1, 0);
            grid.Children.Add(label1);

            Grid.SetRow(_sampleRefs, 0);
            Grid.SetColumn(_sampleRefs, 1);
            grid.Children.Add(_sampleRefs);

            Label label2 = new Label { Content = MetadataProperty.STUDY_VARIABLE_ASSAY_REFS.Name };
            Grid.SetRow(label2, 1);
            Grid.SetColumn(label2, 0);
            grid.Children.Add(label2);
            
            Grid.SetRow(_assayRefs, 1);
            Grid.SetColumn(_assayRefs, 1);
            grid.Children.Add(_assayRefs);

            Children.Add(grid);
        }

        public sealed override StudyVariable[] Value{
            get{
                IList<StudyVariable> result = new List<StudyVariable>();

                for (int n = 0; n < _count; n++){
                    StudyVariable variable = new StudyVariable(result.Count + 1){Description = _description[n].Content.ToString()};

                    for (int i = 0; i < _sampleRefs.SelectedIndices[n].Length; i++){
                        string text = _sampleRefs.items[_sampleRefs.SelectedIndices[n][i]];
                        int index = _sampleText.IndexOf(text);
                        if (index != -1){
                            variable.AddSample(_sampleList[index]);
                        }
                    }

                    for (int i = 0; i < _assayRefs.SelectedIndices[n].Length; i++){
                        string text = _assayRefs.items[_assayRefs.SelectedIndices[n][i]];
                        int index = _assayText.IndexOf(text);
                        if (index != -1){
                            variable.AddAssay(_assayList[index]);
                        }
                    }


                    result.Add(variable);
                }

                return result.ToArray();
            }
            set{
                IList<StudyVariable> variables = value;
                if (variables == null || variables.Count == 0){
                    return;
                }

                int[][] sampleIndizes = new int[_count][];
                int[][] assayIndizes = new int[_count][];

                string[] bins = new string[_count];
                for (int i = 0; i < _count; i++){
                    if (i >= variables.Count || variables[i] == null){
                        bins[i] = "";
                        _description[i].Content = "";
                        sampleIndizes[i] = new int[0];
                        assayIndizes[i] = new int[0];
                        continue;
                    }
                    _description[i].Content = variables[i].Description;
                    bins[i] = _description[i].Content.ToString();

                    
                    sampleIndizes[i] = variables[i].SampleMap.Values.Select(x => _sampleText.IndexOf(SampleToString(x))).ToArray();
                    assayIndizes[i] = variables[i].AssayMap.Values.Select(x => _assayText.IndexOf(AssayToString(x))).ToArray();
                }

                _sampleRefs.Init(_sampleText, bins);
                _assayRefs.Init(_assayText, bins);

                _sampleRefs.SelectedIndices = sampleIndizes;
                _assayRefs.SelectedIndices = assayIndizes;
            }
        }

        private string SampleToString(Sample sample){
            return string.Format("[{0}] {1}", sample.Id, sample.Description);
        }

        private string AssayToString(Assay assay){
            return string.Format("[{0}] {1} - {2}", assay.Id, assay.QuantificationReagent.Name, assay.MsRun is MsRunImpl ? (assay.MsRun as MsRunImpl).Description : assay.MsRun.Location.Value);
        }

        public static int MinimumHeight(int count){
            return 1*Constants.LabelHeight + 1*Constants.LabelHeight + 2*(count)*Constants.height;
        }
    }
}