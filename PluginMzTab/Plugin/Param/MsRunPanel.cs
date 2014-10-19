using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using BaseLib.Util;
using BaseLib.Wpf;
using PluginMzTab.Lib.Model;
using PluginMzTab.Plugin.Extended;
using PluginMzTab.Plugin.Utils;
using Button = System.Windows.Controls.Button;
using ComboBox = System.Windows.Controls.ComboBox;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using Label = System.Windows.Controls.Label;
using ListBox = System.Windows.Controls.ListBox;
using TextBox = System.Windows.Controls.TextBox;

namespace PluginMzTab.Plugin.Param{
    public abstract class MsRunPanel : StackPanel{
        public abstract MsRunImpl[] Value { get; set; }

        public static IList<MsRunImpl> UniqueGroups(IList<MsRunImpl> msruns){
            return MzTabMatrixUtils.Unique(msruns);
        }
    }

    public class MsRunPanel1 : MsRunPanel{
        private readonly int _count;
        private readonly CVLookUp _cv;
        private readonly IList<ComboBox> _formats = new List<ComboBox>();
        private readonly IList<ComboBox> _idFormats = new List<ComboBox>();
        private readonly IList<ComboBox> _fragmentations = new List<ComboBox>();
        private readonly IList<Button> _buttons = new List<Button>();

        private readonly IList<ListBox> _locations = new List<ListBox>();

        //private readonly Dictionary<int, MsRunImpl> assignment = new Dictionary<int, MsRunImpl>();
        private readonly Dictionary<string, MsRunImpl> assignment = new Dictionary<string, MsRunImpl>();

        public MsRunPanel1(int count, MsRunImpl[] msRunsImpl, CVLookUp cv){
            _count = count;
            _cv = cv;

            InitializeComponent();

            Value = msRunsImpl;
        }

        private void InitializeComponent(){
            _formats.Clear();
            _idFormats.Clear();
            _fragmentations.Clear();
            _buttons.Clear();

            Grid grid = new Grid{
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                ClipToBounds = true
            };
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition{Height = new GridLength(1f, GridUnitType.Star)});

            grid.ColumnDefinitions.Add(new ColumnDefinition{Width = new GridLength(1, GridUnitType.Auto)});

            Label label1 = new Label{
                Content = MetadataProperty.MS_RUN_FORMAT.Name,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                VerticalContentAlignment = VerticalAlignment.Top
            };
            Grid.SetRow(label1, 1);
            Grid.SetColumn(label1, 0);
            grid.Children.Add(label1);

            Label label2 = new Label{
                Content = MetadataProperty.MS_RUN_ID_FORMAT.Name,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                VerticalContentAlignment = VerticalAlignment.Top
            };
            Grid.SetRow(label2, 2);
            Grid.SetColumn(label2, 0);
            grid.Children.Add(label2);

            Label label3 = new Label{
                Content = MetadataProperty.MS_RUN_FRAGMENTATION_METHOD.Name,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                VerticalContentAlignment = VerticalAlignment.Top
            };
            Grid.SetRow(label3, 3);
            Grid.SetColumn(label3, 0);
            grid.Children.Add(label3);

            Label label4 = new Label{
                Content = "load",
                HorizontalContentAlignment = HorizontalAlignment.Left,
                VerticalContentAlignment = VerticalAlignment.Top
            };
            Grid.SetRow(label4, 4);
            Grid.SetColumn(label4, 0);
            grid.Children.Add(label4);

            Label label5 = new Label{
                Content = "filenames",
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Top
            };
            Grid.SetRow(label5, 5);
            Grid.SetColumn(label5, 0);
            grid.Children.Add(label5);

            for (int n = 0; n < _count; n++){
                grid.ColumnDefinitions.Add(new ColumnDefinition{Width = new GridLength(80f/_count, GridUnitType.Star)});

                string name = string.Format("GROUP {0}", (n + 1));
                
                Label label = new Label{
                    Content = name,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    FontWeight = FontWeights.Bold
                };
                Grid.SetRow(label, 0);
                Grid.SetColumn(label, n + 1);
                grid.Children.Add(label);


                ComboBox format = new ComboBox();
                foreach (var cv in _cv.GetNamesOfTerm("MS:1000560", "MS")){
                    format.Items.Add(cv);
                }
                Grid.SetRow(format, 1);
                Grid.SetColumn(format, n + 1);
                grid.Children.Add(format);
                _formats.Add(format);

                ComboBox idFormat = new ComboBox();
                foreach (var cv in _cv.GetNamesOfTerm("MS:1000767", "MS")){
                    idFormat.Items.Add(cv);
                }
                Grid.SetRow(idFormat, 2);
                Grid.SetColumn(idFormat, n + 1);
                grid.Children.Add(idFormat);
                _idFormats.Add(idFormat);

                ComboBox fragmentation = new ComboBox();
                fragmentation.Items.Add("");
                foreach (var cv in _cv.GetNamesOfTerm("MS:1000044", "MS")){
                    fragmentation.Items.Add(cv);
                }
                Grid.SetRow(fragmentation, 3);
                Grid.SetColumn(fragmentation, n + 1);
                grid.Children.Add(fragmentation);
                _fragmentations.Add(fragmentation);

                Button button = new Button{Content = "Load files"};
                button.Click += OnButtonClick;
                Grid.SetRow(button, 4);
                Grid.SetColumn(button, n + 1);
                grid.Children.Add(button);
                _buttons.Add(button);

                int height = _count * Constants.height - 6;
                ListBox location = new ListBox { MinHeight = height, MaxHeight = height };
                Grid.SetRow(location, 5);
                Grid.SetColumn(location, n + 1);
                grid.Children.Add(location);
                _locations.Add(location);
            }
            
            Children.Add(grid);
        }

        private void OnButtonClick(object sender, RoutedEventArgs e){
            OpenFileDialog dialog = new OpenFileDialog{
                Multiselect = true,
                Filter = @"Thermo Raw file (*.raw)|*.raw|Andromeda PeakList file (*.apl)|*.apl"
            };

            int index = -1;
            if (sender is Button){
                index = _buttons.IndexOf(sender as Button);
                if (_formats[index] != null && _formats[index].SelectedValue != null){
                    if (_formats[index].SelectedValue.Equals(_cv.GetParam("MS:1000563", "MS").Name)){
                        dialog.Filter = @"Thermo Raw file (*.raw)|*.raw";
                    } else if (_formats[index].SelectedValue.Equals(_cv.GetParam("Andromeda Peak list file", "MS").Name)) {
                        dialog.Filter = @"Andromeda PeakList file (*.apl)|*.apl";
                    }
                    else{
                        dialog.Filter = @"Andromeda PeakList file (*.apl)|*.apl";
                    }
                }
            }


            if (dialog.ShowDialog() == DialogResult.OK){
                if (index != -1){                    
                    foreach (var file in dialog.FileNames){
                        var filenames = _locations[index].Items.Cast<string>();                       
                        if (filenames.Contains(file)) {
                            continue;
                        }
                        
                        if (filenames.Any(x => x != null && Path.GetFileName(x).Equals(file))){
                            continue;
                        }

                        _locations[index].Items.Add(file);
                    }
                }
            }
        }

        public override sealed MsRunImpl[] Value{
            get{
                IList<MsRunImpl> result = new List<MsRunImpl>();

                for (int n = 0; n < _count; n++){
                    Lib.Model.Param format =
                        _cv.GetParam(_formats[n].SelectedItem == null ? null : _formats[n].SelectedItem.ToString(), "MS");
                    Lib.Model.Param idformat =
                        _cv.GetParam(_idFormats[n].SelectedItem == null ? null : _idFormats[n].SelectedItem.ToString(),
                                     "MS");
                    Lib.Model.Param fragmentation =
                        _cv.GetParam(
                            _fragmentations[n].SelectedItem == null ? null : _fragmentations[n].SelectedItem.ToString(),
                            "MS");

                    foreach (string file in _locations[n].Items){

                        if (string.IsNullOrEmpty(file)){
                            continue;
                        }

                        string filename = Path.GetFileNameWithoutExtension(file);
                        if (filename == null){
                            continue;
                        }

                        int id = Math.Max(ArrayUtils.Max(assignment.Values.Select(x => x.Id).ToArray()) + 1, result.Count + 1);

                        bool found = assignment.ContainsKey(filename);

                        var run = assignment.ContainsKey(filename) ? assignment[filename] : new MsRunImpl(id) {
                            Format = format,
                            IdFormat = idformat,
                            FragmentationMethod = fragmentation,
                        };
                        run.Location = new Url(file);

                        result.Add(run);
                    }
                }

                return result.ToArray();
            }
            set{
                assignment.Clear();

                IList<MsRunImpl> runs = value;
                if (runs == null || runs.Count == 0){
                    return;
                }

                foreach (var run in runs){
                    string key = run.Description;
                    if (key == null){
                        continue;
                    }
                    if (!assignment.ContainsKey(key)){
                        assignment.Add(key, run);
                    }
                }

                IList<MsRunImpl> groups = MzTabMatrixUtils.Unique(runs);
                if (groups == null){
                    return;
                }

                
                for (int n = 0; n < _count; n++){
                    if (n >= groups.Count || groups[n] == null){
                        _formats[n].SelectedIndex = -1;
                        _idFormats[n].SelectedIndex = -1;
                        _fragmentations[n].SelectedIndex = -1;
                        continue;
                    }

                    _formats[n].SelectedIndex =
                        MzTabMatrixUtils.GetSelectedIndex(groups[n].Format == null ? null : groups[n].Format.Name,
                                                          _formats[n].Items);
                    _idFormats[n].SelectedIndex =
                        MzTabMatrixUtils.GetSelectedIndex(groups[n].IdFormat == null ? null : groups[n].IdFormat.Name,
                                                          _idFormats[n].Items);
                    _fragmentations[n].SelectedIndex =
                        MzTabMatrixUtils.GetSelectedIndex(
                            groups[n].FragmentationMethod == null ? null : groups[n].FragmentationMethod.Name,
                            _fragmentations[n].Items);


                    
                    for (int i = 0; i < runs.Count; i++){
                        if (MzTabMatrixUtils.Equals(runs[i], groups[n])){
                            if (runs[i].Location != null){
                                string file = runs[i].Location.Value;
                                if (File.Exists(file)){
                                    _locations[n].Items.Add(file);
                                }
                            }
                        }
                    }
                    
                }                
            }
        }

        private MsRunImpl GetMsRunImpl(string file){
            MsRunImpl run =
                assignment.Values.Where(x => x.Location != null).FirstOrDefault(x => x.Location.Value.Equals(file));
            if (run == null){
                string filename = Path.GetFileNameWithoutExtension(file);
                List<MsRunImpl> tmp = assignment.Values.Where(x => x.Description != null).ToList();
                List<string> pool = tmp.Select(x => Path.GetFileNameWithoutExtension(x.Description)).ToList();
                int index = pool.IndexOf(filename);
                if (index != -1){
                    run = tmp[index];
                    run.Location = new Url(file);
                }
            }
            return run;
        }

        public static int MiniumHeight(int count){
            return Constants.LabelHeight + Constants.TextBoxHeight + 3*Constants.ComboBoxHeight + count*Constants.height +
                   6;
        }
    }

    public class MsRunPanel2 : MsRunPanel{
        private readonly CVLookUp _cv;
        private readonly int _count;

        private readonly IList<TextBox> _formats = new List<TextBox>();
        private readonly IList<TextBox> _idFormats = new List<TextBox>();
        private readonly IList<TextBox> _fragmentations = new List<TextBox>();
        private readonly IList<ListBox> _locations = new List<ListBox>();

        private readonly string[] _defaultFormat;
        private readonly string[] _defaultIdFormat;

        public MsRunPanel2(int count, MsRunImpl[] msRunsImpl, CVLookUp cv){
            HorizontalAlignment = HorizontalAlignment.Stretch;
            _cv = cv;
            _count = count;

            _defaultFormat = new string[2];
            _defaultFormat[0] = _cv.GetNameOfTerm("MS:1000563", "MS");
            _defaultFormat[1] = _cv.GetNameOfTerm("Andromeda Peak list file", "MS");

            _defaultIdFormat = new string[2];
            _defaultIdFormat[0] = _cv.GetNameOfTerm("MS:1000768", "MS");
            _defaultIdFormat[1] = _cv.GetNameOfTerm("MS:1000774", "MS");

            InitializeComponent();

            Value = msRunsImpl;
        }

        private void InitializeComponent(){
            _formats.Clear();
            _idFormats.Clear();

            Grid grid = new Grid{HorizontalAlignment = HorizontalAlignment.Stretch, ClipToBounds = true};
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition{Height = new GridLength(Constants.ListSelectorHeight - 6)});

            grid.ColumnDefinitions.Add(new ColumnDefinition{Width = new GridLength(1, GridUnitType.Auto)});

            Label label1 = new Label{
                Content = MetadataProperty.MS_RUN_FORMAT.Name,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                VerticalContentAlignment = VerticalAlignment.Top
            };
            Grid.SetRow(label1, 1);
            Grid.SetColumn(label1, 0);
            grid.Children.Add(label1);

            Label label2 = new Label{
                Content = MetadataProperty.MS_RUN_ID_FORMAT.Name,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                VerticalContentAlignment = VerticalAlignment.Top
            };
            Grid.SetRow(label2, 2);
            Grid.SetColumn(label2, 0);
            grid.Children.Add(label2);

            Label label3 = new Label{
                Content = MetadataProperty.MS_RUN_FRAGMENTATION_METHOD.Name,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                VerticalContentAlignment = VerticalAlignment.Top
            };
            Grid.SetRow(label3, 3);
            Grid.SetColumn(label3, 0);
            grid.Children.Add(label3);

            Label label4 = new Label{
                Content = MetadataProperty.MS_RUN_LOCATION.Name,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                VerticalContentAlignment = VerticalAlignment.Top
            };
            Grid.SetRow(label4, 4);
            Grid.SetColumn(label4, 0);
            grid.Children.Add(label4);


            for (int n = 0; n < _count; n++){
                grid.ColumnDefinitions.Add(new ColumnDefinition{Width = new GridLength(1d/_count, GridUnitType.Star)});

                string name = string.Format("GROUP {0}", (n + 1));
                Label label = new Label{
                    Content = name,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    FontWeight = FontWeights.Bold,
                    ClipToBounds = true
                };
                Grid.SetRow(label, 0);
                Grid.SetColumn(label, n + 1);
                grid.Children.Add(label);

                TextBox format = new TextBox{IsReadOnly = true, ClipToBounds = true};
                Grid.SetRow(format, 1);
                Grid.SetColumn(format, n + 1);
                grid.Children.Add(format);
                _formats.Add(format);

                TextBox idFormat = new TextBox{IsReadOnly = true, ClipToBounds = true};
                Grid.SetRow(idFormat, 2);
                Grid.SetColumn(idFormat, n + 1);
                grid.Children.Add(idFormat);
                _idFormats.Add(idFormat);

                TextBox fragmentation = new TextBox{IsReadOnly = true, ClipToBounds = true};
                Grid.SetRow(fragmentation, 3);
                Grid.SetColumn(fragmentation, n + 1);
                grid.Children.Add(fragmentation);
                _fragmentations.Add(fragmentation);

                ListBox location = new ListBox{ClipToBounds = true};
                Grid.SetRow(location, 4);
                Grid.SetColumn(location, n + 1);
                grid.Children.Add(location);
                _locations.Add(location);
            }

            Children.Add(grid);
        }

        public override sealed MsRunImpl[] Value{
            get{
                IList<MsRunImpl> result = new List<MsRunImpl>();

                for (int n = 0; n < _count; n++){
                    if (_locations[n].Items.Count == 0){
                        break;
                    }

                    Lib.Model.Param fragmentation = null;
                    Lib.Model.Param format = null;
                    Lib.Model.Param idformat = null;
                    if (!string.IsNullOrEmpty(_formats[n].Text)){
                        format = _cv.GetParam(_formats[n].Text, "MS");
                    }
                    if (!string.IsNullOrEmpty(_idFormats[n].Text)){
                        idformat = _cv.GetParam(_idFormats[n].Text, "MS");
                    }
                    if (!string.IsNullOrEmpty(_fragmentations[n].Text)){
                        fragmentation = _cv.GetParam(_fragmentations[n].Text, "MS");
                    }


                    foreach (var item in _locations[n].Items){
                        MsRunImpl runImpl = new MsRunImpl(result.Count + 1){
                            Location = new Url(item.ToString()),
                            Format = format,
                            IdFormat = idformat,
                            FragmentationMethod = fragmentation
                        };
                        result.Add(runImpl);
                    }
                }

                return result.ToArray();
            }
            set{
                IList<MsRunImpl> runs = value;
                if (runs == null || runs.Count == 0){
                    return;
                }

                IList<MsRunImpl> groups = MzTabMatrixUtils.Unique(runs);
                if (groups == null){
                    return;
                }

                for (int i = 0; i < _count; i++){
                    if (i >= groups.Count || groups[i] == null){
                        _formats[i].Text = _defaultFormat.Length > i ? _defaultFormat[i] : null;
                        _idFormats[i].Text = _defaultIdFormat.Length > i ? _defaultIdFormat[i] : null;
                        _fragmentations[i].Text = "";

                        continue;
                    }

                    _formats[i].Text = groups[i].Format != null ? groups[i].Format.Name : (_defaultFormat.Length > i ? _defaultFormat[i] : null);
                    _idFormats[i].Text = groups[i].IdFormat != null ? groups[i].IdFormat.Name : ( _defaultIdFormat.Length > i ? _defaultIdFormat[i] : null);
                    _fragmentations[i].Text = groups[i].FragmentationMethod == null
                                                  ? ""
                                                  : groups[i].FragmentationMethod.Name;

                    foreach (MsRunImpl run in runs){
                        if (MzTabMatrixUtils.Equals(run, groups[i]) && run.Location != null){
                            _locations[i].Items.Add(run.Location.Value);
                        }
                    }
                }
            }
        }

        public static int MiniumHeight(){
            return 1*Constants.LabelHeight + 3*Math.Max(Constants.ComboBoxHeight, Constants.LabelHeight) +
                   1*Constants.ListSelectorHeight;
        }
    }
}