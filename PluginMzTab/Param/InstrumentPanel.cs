using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MzTabLibrary.model;
using PluginMzTab.utils;

namespace PluginMzTab.param{
    public class InstrumentPanel : StackPanel
    {
        private readonly int _count;

        private readonly CVLookUp _cv = new CVLookUp();

        private readonly IList<ComboBox> _names = new List<ComboBox>();
        private readonly IList<ComboBox> _sources = new List<ComboBox>();
        private readonly IList<ComboBox> _analyzers = new List<ComboBox>();
        private readonly IList<ComboBox> _detectors = new List<ComboBox>();

        private readonly string _defaultName;
        private readonly string _defaultSource;
        private readonly string _defaultAnalyzer;
        private readonly string _defaultDetector;

        public InstrumentPanel(Instrument[] instruments, CVLookUp cv){
            _cv = cv;
            _count = instruments.Length;

            _defaultName = _cv.GetNameOfTerm("MS:1000449", "MS");
            _defaultSource = _cv.GetNameOfTerm("MS:1000073", "MS");
            _defaultAnalyzer = _cv.GetNameOfTerm("MS:1000484", "MS");
            _defaultDetector = null;

            InitializeComponent();

            Value = instruments;
        }

        private void InitializeComponent(){
            _names.Clear();
            _sources.Clear();
            _analyzers.Clear();
            _detectors.Clear();

            Grid grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition{Height = new GridLength(1, GridUnitType.Auto)});
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });

            grid.ColumnDefinitions.Add(new ColumnDefinition{Width = new GridLength(1, GridUnitType.Auto)});
            for (int i = 0; i < _count; i++){
                grid.ColumnDefinitions.Add(new ColumnDefinition{Width = new GridLength(1, GridUnitType.Auto)});
            }

            Label label1 = new Label
            {
                Content = MetadataProperty.INSTRUMENT_NAME.Name,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };
            Grid.SetRow(label1, 1);
            Grid.SetColumn(label1, 0);
            grid.Children.Add(label1);

            Label label2 = new Label
            {
                Content = MetadataProperty.INSTRUMENT_SOURCE.Name,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };
            Grid.SetRow(label2, 2);
            Grid.SetColumn(label2, 0);
            grid.Children.Add(label2);

            Label label3= new Label
            {
                Content = MetadataProperty.INSTRUMENT_ANALYZER.Name,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };
            Grid.SetRow(label3, 3);
            Grid.SetColumn(label3, 0);
            grid.Children.Add(label3);

            Label label4 = new Label
            {
                Content = MetadataProperty.INSTRUMENT_DETECTOR.Name,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };
            Grid.SetRow(label4, 4);
            Grid.SetColumn(label4, 0);
            grid.Children.Add(label4);

            for (int n = 0; n < _count; n++){
                Label label = new Label{Content = string.Format("[{0}]", (n + 1)), HorizontalContentAlignment = HorizontalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Top};
                Grid.SetRow(label, 0);
                Grid.SetColumn(label, n+1);
                grid.Children.Add(label);

                ComboBox name = new ComboBox();
                foreach (var cv in _cv.GetNamesOfTerm("MS:1000494", "MS")){
                    name.Items.Add(cv);
                }
                name.SelectedIndex = MzTabMatrixUtils.GetSelectedIndex(_defaultName, name.Items);
                Grid.SetRow(name, 1);
                Grid.SetColumn(name, n + 1);
                grid.Children.Add(name);
                _names.Add(name);

                ComboBox source = new ComboBox();
                foreach (var cv in _cv.GetNamesOfTerm("MS:1000008", "MS")){
                    source.Items.Add(cv);
                }
                source.SelectedIndex = MzTabMatrixUtils.GetSelectedIndex(_defaultSource, source.Items);
                Grid.SetRow(source,2);
                Grid.SetColumn(source, n + 1);
                grid.Children.Add(source);
                _sources.Add(source);

                ComboBox analyzer = new ComboBox();
                foreach (var cv in _cv.GetNamesOfTerm("MS:1000443", "MS")){
                    analyzer.Items.Add(cv);
                }
                analyzer.SelectedIndex = MzTabMatrixUtils.GetSelectedIndex(_defaultAnalyzer, analyzer.Items);
                Grid.SetRow(analyzer, 3);
                Grid.SetColumn(analyzer, n + 1);
                grid.Children.Add(analyzer);
                _analyzers.Add(analyzer);

                ComboBox detector = new ComboBox();
                foreach (var cv in _cv.GetNamesOfTerm("MS:1000026", "MS")){
                    detector.Items.Add(cv);
                }
                detector.SelectedIndex = MzTabMatrixUtils.GetSelectedIndex(null, detector.Items);
                Grid.SetRow(detector, 4);
                Grid.SetColumn(detector, n + 1);
                grid.Children.Add(detector);
                _detectors.Add(detector);
            }

            Children.Add(grid);
        }

        public Instrument[] Value{
            get{
                IList<Instrument> result = new List<Instrument>();

                for (int n = 0; n < _count; n++){
                    Param name = _cv.GetParam(_names[n].SelectedItem == null ? null : _names[n].SelectedItem.ToString(), "MS");
                    Param source = _cv.GetParam(_sources[n].SelectedItem == null ? null : _sources[n].SelectedItem.ToString(), "MS");
                    Param analyzer = _cv.GetParam(_analyzers[n].SelectedItem == null ? null : _analyzers[n].SelectedItem.ToString(), "MS");
                    Param detector = _cv.GetParam(_detectors[n].SelectedItem == null ? null : _detectors[n].SelectedItem.ToString(), "MS");

                    Instrument instrument = new Instrument(n + 1){
                        Analyzer = analyzer,
                        Detector = detector,
                        Name = name,
                        Source = source
                    };

                    result.Add(instrument);
                }
                result = MzTabMatrixUtils.Unique(result);
                return result.ToArray();
            }
            set{
                IList<Instrument> instruments = value;
                if (instruments == null || instruments.Count == 0){
                    return;
                }

                IList<Instrument> groups = MzTabMatrixUtils.Unique(instruments);
                if (groups == null){
                    return;
                }

                for (int i = 0; i < _count; i++){
                    if (i >= groups.Count || groups[i] == null){
                        _names[i].SelectedIndex = MzTabMatrixUtils.GetSelectedIndex(_defaultName, _names[i].Items);
                        _sources[i].SelectedIndex = MzTabMatrixUtils.GetSelectedIndex(_defaultSource, _sources[i].Items);
                        _analyzers[i].SelectedIndex = MzTabMatrixUtils.GetSelectedIndex(_defaultAnalyzer, _analyzers[i].Items);
                        _detectors[i].SelectedIndex = MzTabMatrixUtils.GetSelectedIndex(_defaultDetector, _detectors[i].Items);
                        continue;
                    }

                    _names[i].SelectedIndex =
                        MzTabMatrixUtils.GetSelectedIndex(groups[i].Name == null ? _defaultName : groups[i].Name.Name,
                                                  _names[i].Items);
                    _sources[i].SelectedIndex =
                        MzTabMatrixUtils.GetSelectedIndex(groups[i].Source == null ? _defaultSource : groups[i].Source.Name,
                                                  _sources[i].Items);
                    _analyzers[i].SelectedIndex =
                        MzTabMatrixUtils.GetSelectedIndex(
                            groups[i].Analyzer == null ? _defaultAnalyzer : groups[i].Analyzer.Name, _analyzers[i].Items);
                    _detectors[i].SelectedIndex =
                        MzTabMatrixUtils.GetSelectedIndex(
                            groups[i].Detector == null ? _defaultDetector : groups[i].Detector.Name, _detectors[i].Items);
                }
            }
        }

        public static int MiniumHeight { get { return 1 * Constants.LabelHeight + 4 * Math.Max(Constants.ComboBoxHeight, Constants.LabelHeight) + Constants.puffer; } }
    }
}