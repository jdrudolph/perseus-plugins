using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using BaseLib.Wpf;
using MzTabLibrary.model;
using PluginMzTab.extended;
using PluginMzTab.utils;
using ComboBox = System.Windows.Controls.ComboBox;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using Label = System.Windows.Controls.Label;
using ListBox = System.Windows.Controls.ListBox;
using TextBox = System.Windows.Controls.TextBox;

namespace PluginMzTab.param{
    public abstract class MsRunPanel : StackPanel{
        public abstract MsRunImpl[] Value { get; set; }

        public static IList<MsRunImpl> UniqueGroups(IList<MsRunImpl> msruns){
            return MzTabMatrixUtils.Unique(msruns);
        }
    }

    public class MsRunPanel1 : MsRunPanel{
        private readonly int _count;
        private readonly CVLookUp _cv;
        private readonly IList<TextBox> _path = new List<TextBox>();
        private readonly IList<ComboBox> _formats = new List<ComboBox>();
        private readonly IList<ComboBox> _idFormats = new List<ComboBox>();
        private readonly IList<ComboBox> _fragmentations = new List<ComboBox>();
        private readonly MultiListSelectorControl _locations = new MultiListSelectorControl();

        private readonly string _defaultFormat;
        private readonly string _defaultIdFormat;
        private readonly string _defaultFragmentation;
        private readonly IList<string> _filenames;

        public MsRunPanel1(int count, MsRunImpl[] msRunsImpl, CVLookUp cv){
            _count = count;
            _cv = cv;
            
            _defaultFormat = _cv.GetNameOfTerm("MS:1000563", "MS");
            _defaultIdFormat = _cv.GetNameOfTerm("MS:1000768", "MS");
            _defaultFragmentation = _cv.GetNameOfTerm("MS:1000133", "MS");

            _filenames = msRunsImpl.Where(x => x != null).Select(x => x.Description).ToList();
            _locations.MinHeight = _count * Constants.height - 6 ;

            InitializeComponent();

            Value = msRunsImpl;            
        }

        private void InitializeComponent()
        {
            _formats.Clear();
            _idFormats.Clear();

            Grid grid = new Grid{VerticalAlignment = VerticalAlignment.Top, HorizontalAlignment = HorizontalAlignment.Left};
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition{Height = new GridLength(100f, GridUnitType.Star)});

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });

            Label label1 = new Label { Content = MetadataProperty.MS_RUN_FORMAT.Name, HorizontalContentAlignment = HorizontalAlignment.Left, VerticalContentAlignment = VerticalAlignment.Top };
            Grid.SetRow(label1, 1);
            Grid.SetColumn(label1, 0);
            grid.Children.Add(label1);

            Label label2 = new Label { Content = MetadataProperty.MS_RUN_ID_FORMAT.Name, HorizontalContentAlignment = HorizontalAlignment.Left, VerticalContentAlignment = VerticalAlignment.Top };
            Grid.SetRow(label2, 2);
            Grid.SetColumn(label2, 0);
            grid.Children.Add(label2);

            Label label3 = new Label { Content = MetadataProperty.MS_RUN_FRAGMENTATION_METHOD.Name, HorizontalContentAlignment = HorizontalAlignment.Left, VerticalContentAlignment = VerticalAlignment.Top };
            Grid.SetRow(label3, 3);
            Grid.SetColumn(label3, 0);
            grid.Children.Add(label3);

            Label label4 = new Label { Content = "path", HorizontalContentAlignment = HorizontalAlignment.Left, VerticalContentAlignment = VerticalAlignment.Top };
            Grid.SetRow(label4, 4);
            Grid.SetColumn(label4, 0);
            grid.Children.Add(label4);

            Label label5 = new Label { Content = "filenames", HorizontalContentAlignment = HorizontalAlignment.Stretch, VerticalContentAlignment = VerticalAlignment.Top };
            Grid.SetRow(label5, 5);
            Grid.SetColumn(label5, 0);
            grid.Children.Add(label5);

            string[]bins =new string[_count];
            for (int n = 0; n < _count; n++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80f / _count, GridUnitType.Star) });

                string name = string.Format("GROUP {0}", (n + 1));
                bins[n] = name;
                Label label = new Label { Content = name, HorizontalAlignment = HorizontalAlignment.Center, FontWeight = FontWeights.Bold};
                Grid.SetRow(label, 0);
                Grid.SetColumn(label, n + 1);
                grid.Children.Add(label);

                ComboBox format = new ComboBox();
                foreach (var cv in _cv.GetNamesOfTerm("MS:1000560", "MS"))
                {
                    format.Items.Add(cv);
                }
                format.SelectedIndex = MzTabMatrixUtils.GetSelectedIndex(_defaultFormat, format.Items);
                Grid.SetRow(format, 1);
                Grid.SetColumn(format, n + 1);
                grid.Children.Add(format);
                _formats.Add(format);

                ComboBox idFormat = new ComboBox();
                foreach (var cv in _cv.GetNamesOfTerm("MS:1000767", "MS"))
                {
                    idFormat.Items.Add(cv);
                }
                idFormat.SelectedIndex = MzTabMatrixUtils.GetSelectedIndex(_defaultIdFormat, idFormat.Items);
                Grid.SetRow(idFormat, 2);
                Grid.SetColumn(idFormat, n + 1);
                grid.Children.Add(idFormat);
                _idFormats.Add(idFormat);

                ComboBox fragmentation = new ComboBox();
                foreach (var cv in _cv.GetNamesOfTerm("MS:1000044", "MS"))
                {
                    fragmentation.Items.Add(cv);
                }
                fragmentation.SelectedIndex = MzTabMatrixUtils.GetSelectedIndex(_defaultFragmentation, fragmentation.Items);
                Grid.SetRow(fragmentation, 3);
                Grid.SetColumn(fragmentation, n + 1);
                grid.Children.Add(fragmentation);
                _fragmentations.Add(fragmentation);

                TextBox path = new TextBox{Text = "C:\\"};
                path.MouseDoubleClick += OpenFolderDiaglog;
                Grid.SetRow(path, 4);
                Grid.SetColumn(path, n + 1);
                grid.Children.Add(path);
                _path.Add(path);
            }

            _locations.Init(_filenames, bins);
            Grid.SetColumnSpan(_locations, _count);
            Grid.SetRow(_locations, 5);
            Grid.SetColumn(_locations, 1);
            grid.Children.Add(_locations);
            
            Children.Add(grid);
        }

        public override sealed MsRunImpl[] Value
        {
            get
            {
                IList<MsRunImpl> result = new List<MsRunImpl>();

                for (int n = 0; n < _count; n++){

                    Param format = _cv.GetParam(_formats[n].SelectedItem == null ? null : _formats[n].SelectedItem.ToString(), "MS");
                    Param idformat = _cv.GetParam(_idFormats[n].SelectedItem == null ? null : _idFormats[n].SelectedItem.ToString(), "MS");
                    Param fragmentation = _cv.GetParam(_fragmentations[n].SelectedItem == null ? null : _fragmentations[n].SelectedItem.ToString(), "MS");

                    foreach (var item in _locations.GetSelectedIndices(n)){
                        string file = _locations.items[item];
                        if (string.IsNullOrEmpty(file)){
                            continue;
                        }

                        MsRunImpl runImpl = new MsRunImpl(result.Count + 1){
                            Format = format,
                            IdFormat = idformat,
                            FragmentationMethod = fragmentation
                        };
                        // Add rawfile location to this instance
                        runImpl.Location = new Url(_path[n].Text == null ? file : Path.Combine(_path[n].Text, file + (format != null && format.Accession == "MS:1000563" ? ".raw" : "")));
                        
                        result.Add(runImpl);
                    }
                }

                return result.ToArray();
            }
            set
            {
                IList<MsRunImpl> runs = value;
                if (runs == null || runs.Count == 0)
                {
                    return;
                }

                IList<MsRunImpl> groups = MzTabMatrixUtils.Unique(runs);
                if (groups == null)
                {
                    return;
                }

                int[][] ind = new int[_count][];
                for (int n = 0; n < _count; n++)
                {
                    ind[n] = new int[0];
                    if (n >= groups.Count || groups[n] == null)
                    {
                        _formats[n].SelectedIndex = MzTabMatrixUtils.GetSelectedIndex(_defaultFormat, _formats[n].Items);
                        _idFormats[n].SelectedIndex = MzTabMatrixUtils.GetSelectedIndex(_defaultIdFormat, _idFormats[n].Items);
                        _fragmentations[n].SelectedIndex = MzTabMatrixUtils.GetSelectedIndex(_defaultFragmentation, _fragmentations[n].Items);
                        continue;
                    }

                    _formats[n].SelectedIndex = MzTabMatrixUtils.GetSelectedIndex(groups[n].Format == null ? _defaultFormat : groups[n].Format.Name, _formats[n].Items);
                    _idFormats[n].SelectedIndex = MzTabMatrixUtils.GetSelectedIndex(groups[n].IdFormat == null ? _defaultIdFormat : groups[n].IdFormat.Name, _idFormats[n].Items);
                    _fragmentations[n].SelectedIndex = MzTabMatrixUtils.GetSelectedIndex(groups[n].FragmentationMethod == null ? _defaultFragmentation : groups[n].FragmentationMethod.Name, _fragmentations[n].Items);

                    _path[n].Text = "";

                    IList<int> indizes =new List<int>();
                    for (int i = 0; i < runs.Count; i++){
                        if (MzTabMatrixUtils.Equals(runs[i], groups[n])){
                            _path[n].Text = runs[i].FilePath;
                            indizes.Add(i);
                        }
                    }
                    ind[n] = indizes.ToArray();
                }

                _locations.SelectedIndices = ind;
            }
        }

        public static int MiniumHeight(int count){
            return Constants.LabelHeight + Constants.TextBoxHeight + 3*Constants.ComboBoxHeight + count * Constants.height + 6;
        }

        private void OpenFolderDiaglog(object sender, MouseButtonEventArgs e){
            FolderBrowserDialog dialog = new FolderBrowserDialog{SelectedPath = @"C:\"};
            if (dialog.ShowDialog() == DialogResult.OK){
                if (sender is TextBox){
                    (sender as TextBox).Text = dialog.SelectedPath;
                }
            }
        }
    }

    public class MsRunPanel2 : MsRunPanel{
        private readonly CVLookUp _cv;
        private readonly int _count;

        private readonly IList<TextBox> _formats = new List<TextBox>();
        private readonly IList<TextBox> _idFormats = new List<TextBox>();
        private readonly IList<TextBox> _fragmentations = new List<TextBox>();
        private readonly IList<ListBox> _locations = new List<ListBox>();

        private readonly string _defaultFormat;
        private readonly string _defaultIdFormat;
        private readonly string _defaultFragmentation;

        public MsRunPanel2(int count, MsRunImpl[] msRunsImpl, CVLookUp cv)
        {
            HorizontalAlignment = HorizontalAlignment.Left;
            _cv = cv;
            _count = count;

            _defaultFormat = _cv.GetNameOfTerm("MS:1000563", "MS");
            _defaultIdFormat = _cv.GetNameOfTerm("MS:1000768", "MS");
            _defaultFragmentation = _cv.GetNameOfTerm("MS:1000133", "MS");

            InitializeComponent();

            Value = msRunsImpl;
        }

        private void InitializeComponent(){
            _formats.Clear();
            _idFormats.Clear();

            Grid grid = new Grid { HorizontalAlignment = HorizontalAlignment.Stretch }; 
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition{ Height = new GridLength(Constants.ListSelectorHeight - 6) });

            grid.ColumnDefinitions.Add(new ColumnDefinition{ Width = new GridLength(1, GridUnitType.Auto) });

            Label label1 = new Label { Content = MetadataProperty.MS_RUN_FORMAT.Name, HorizontalContentAlignment = HorizontalAlignment.Left, VerticalContentAlignment = VerticalAlignment.Top};
            Grid.SetRow(label1, 1);
            Grid.SetColumn(label1, 0);
            grid.Children.Add(label1);

            Label label2 = new Label { Content = MetadataProperty.MS_RUN_ID_FORMAT.Name, HorizontalContentAlignment = HorizontalAlignment.Left, VerticalContentAlignment = VerticalAlignment.Top };
            Grid.SetRow(label2, 2);
            Grid.SetColumn(label2, 0);
            grid.Children.Add(label2);

            Label label3 = new Label { Content = MetadataProperty.MS_RUN_FRAGMENTATION_METHOD.Name, HorizontalContentAlignment = HorizontalAlignment.Left, VerticalContentAlignment = VerticalAlignment.Top };
            Grid.SetRow(label3, 3);
            Grid.SetColumn(label3, 0);
            grid.Children.Add(label3);

            Label label4 = new Label { Content = MetadataProperty.MS_RUN_LOCATION.Name, HorizontalContentAlignment = HorizontalAlignment.Left, VerticalContentAlignment = VerticalAlignment.Top };
            Grid.SetRow(label4, 4);
            Grid.SetColumn(label4, 0);
            grid.Children.Add(label4);


            for (int n = 0; n < _count; n++){
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
                
                string name = string.Format("GROUP {0}", (n + 1));
                Label label = new Label{Content = name, HorizontalAlignment = HorizontalAlignment.Center, FontWeight = FontWeights.Bold};
                Grid.SetRow(label, 0);
                Grid.SetColumn(label, n + 1);
                grid.Children.Add(label);

                TextBox format = new TextBox{IsReadOnly = true};
                Grid.SetRow(format, 1);
                Grid.SetColumn(format, n + 1);
                grid.Children.Add(format);
                _formats.Add(format);

                TextBox idFormat = new TextBox { IsReadOnly = true};
                Grid.SetRow(idFormat, 2);
                Grid.SetColumn(idFormat, n + 1);
                grid.Children.Add(idFormat);
                _idFormats.Add(idFormat);

                TextBox fragmentation = new TextBox { IsReadOnly = true };
                Grid.SetRow(fragmentation, 3);
                Grid.SetColumn(fragmentation, n + 1);
                grid.Children.Add(fragmentation);
                _fragmentations.Add(fragmentation);

                ListBox location = new ListBox();
                Grid.SetRow(location, 4);
                Grid.SetColumn(location, n + 1);
                grid.Children.Add(location);
                _locations.Add(location);                
            }

            Children.Add(grid);
        }

        public override sealed MsRunImpl[] Value
        {
            get{
                IList<MsRunImpl> result = new List<MsRunImpl>();

                for (int n = 0; n < _count; n++){
                    if (_locations[n].Items.Count == 0){
                        break;
                    }

                    Param format = _cv.GetParam(_formats[n].Text, "MS");
                    Param idformat = _cv.GetParam(_idFormats[n].Text, "MS");
                    Param fragmentation = _cv.GetParam(_fragmentations[n].Text, "MS");


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
                        _formats[i].Text = _defaultFormat;
                        _idFormats[i].Text = _defaultIdFormat;
                        _fragmentations[i].Text = _defaultFragmentation;

                        continue;
                    }

                    _formats[i].Text = groups[i].Format == null ? _defaultFormat : groups[i].Format.Name;
                    _idFormats[i].Text = groups[i].IdFormat == null ? _defaultIdFormat : groups[i].IdFormat.Name;
                    _fragmentations[i].Text = groups[i].FragmentationMethod == null ? _defaultFragmentation : groups[i].FragmentationMethod.Name;

                    foreach (MsRunImpl run in runs){
                        if (MzTabMatrixUtils.Equals(run, groups[i])){
                            _locations[i].Items.Add(run.Location.Value);
                        }
                    }
                }               
            }
        }

        public static int MiniumHeight(){
            return 1*Constants.LabelHeight + 3*Math.Max(Constants.ComboBoxHeight, Constants.LabelHeight) + 1*Constants.ListSelectorHeight;
        }
    }
}