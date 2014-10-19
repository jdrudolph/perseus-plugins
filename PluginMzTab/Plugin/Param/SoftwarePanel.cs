using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BaseLib.Wpf;
using PluginMzTab.Lib.Model;
using PluginMzTab.Plugin.Utils;

namespace PluginMzTab.Plugin.Param{
    public class SoftwarePanel : StackPanel{
        private readonly int _count;
        private readonly Dictionary<string, string> _parameters;
        private readonly CVLookUp _cv;

        private readonly IList<ComboBox> _names = new List<ComboBox>();
        private readonly IList<TextBox> _versions = new List<TextBox>();
        private readonly IList<ListSelectorControl> _settings = new List<ListSelectorControl>();

        private readonly string defaultName;
        private readonly string defaultVersion;

        public SoftwarePanel(Software[] value, CVLookUp cv, Dictionary<string, string> parameters){
            MaxWidth = 800;
            _count = value.Length;
            _cv = cv;
            _parameters = parameters;

            defaultName = _cv.GetNameOfTerm("MS:1001583", "MS");
            defaultVersion = parameters.ContainsKey("Version") ? parameters["Version"] : null;

            InitializeComponent();

            Value = value;
        }

        private void InitializeComponent(){
            _names.Clear();
            _versions.Clear();
            _settings.Clear();

            Grid grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition{Height = new GridLength(1, GridUnitType.Auto)});
            grid.RowDefinitions.Add(new RowDefinition{Height = new GridLength(1, GridUnitType.Auto)});
            grid.RowDefinitions.Add(new RowDefinition{Height = new GridLength(1, GridUnitType.Auto)});
            grid.RowDefinitions.Add(new RowDefinition{
                Height = new GridLength(Constants.ListSelectorHeight, GridUnitType.Pixel)
            });

            grid.ColumnDefinitions.Add(new ColumnDefinition{Width = new GridLength(1, GridUnitType.Auto)});
            for (int i = 0; i < _count; i++){
                grid.ColumnDefinitions.Add(new ColumnDefinition{Width = new GridLength(1d/_count, GridUnitType.Star)});
            }

            Label label1 = new Label{Content = @"name"};
            Grid.SetRow(label1, 1);
            Grid.SetColumn(label1, 0);
            grid.Children.Add(label1);

            Label label2 = new Label{Content = @"version"};
            Grid.SetRow(label2, 2);
            Grid.SetColumn(label2, 0);
            grid.Children.Add(label2);

            Label label3 = new Label{Content = MetadataProperty.SOFTWARE_SETTING.Name};
            Grid.SetRow(label3, 3);
            Grid.SetColumn(label3, 0);
            grid.Children.Add(label3);

            for (int n = 0; n < _count; n++){
                Label label = new Label{
                    Content = string.Format("[{0}]", (n + 1)),
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Top
                };
                Grid.SetRow(label, 0);
                Grid.SetColumn(label, n + 1);
                grid.Children.Add(label);

                ComboBox name = new ComboBox();
                foreach (var cv in _cv.GetNamesOfTerm("MS:1001456", "MS")){
                    name.Items.Add(cv);
                }
                name.SelectedIndex = MzTabMatrixUtils.GetSelectedIndex(defaultName, name.Items);
                Grid.SetRow(name, 1);
                Grid.SetColumn(name, n + 1);
                grid.Children.Add(name);
                _names.Add(name);

                TextBox version = new TextBox();
                Grid.SetRow(version, 2);
                Grid.SetColumn(version, n + 1);
                grid.Children.Add(version);
                _versions.Add(version);

                ListSelectorControl setting = new ListSelectorControl{MinHeight = Constants.ListSelectorHeight - 6};
                if (_parameters != null){
                    foreach (var key in _parameters.Select(x => x.Key)){
                        if (key == null || key.StartsWith("Version")){
                            continue;
                        }
                        setting.Items.Add(key);
                    }
                }
                Grid.SetRow(setting, 3);
                Grid.SetColumn(setting, n + 1);
                grid.Children.Add(setting);
                _settings.Add(setting);
            }

            Children.Add(grid);
        }

        public Software[] Value{
            get{
                Software[] result = new Software[_count];

                for (int i = 0; i < _count; i++){
                    Software software = new Software(i + 1){
                        Param =
                            _cv.GetParam(_names[i].SelectedItem != null ? _names[i].SelectedItem.ToString() : "", "MS",
                                         _versions[i].Text)
                    };

                    foreach (var item in _settings[i].SelectedItems){
                        string key = item as string;
                        if (key == null){
                            continue;
                        }
                        string value = _parameters != null && _parameters.ContainsKey(key) ? _parameters[key] : null;
                        if (key == "Version"){
                            continue;
                        }
                        software.AddSetting(string.Format("{0}={1}", key, value));
                    }

                    result[i] = software;
                }

                return result;
            }
            set{
                if (value == null){
                    return;
                }

                Software[] software = value;

                for (int i = 0; i < _count; i++){
                    if (i >= software.Length || software[i] == null){
                        _names[i].SelectedIndex = MzTabMatrixUtils.GetSelectedIndex(defaultName, _names[i].Items);
                        _versions[i].Text = defaultVersion;
                        continue;
                    }
                    _names[i].SelectedIndex =
                        MzTabMatrixUtils.GetSelectedIndex(
                            software[i].Param == null ? defaultName : software[i].Param.Name, _names[i].Items);
                    _versions[i].Text = software[i].Param == null ? defaultVersion : software[i].Param.Value;

                    if (software[i].SettingList != null){
                        IList<int> indices = new List<int>();
                        for (int j = 0; j < _settings[i].Items.Count; j++){
                            string setting = _settings[i].Items[j] as string;
                            if (setting == null){
                                continue;
                            }
                            if (software[i].SettingList.Any(setting.StartsWith)){
                                indices.Add(j);
                            }
                        }
                        _settings[i].SelectedIndices = indices.ToArray();
                    }
                }
            }
        }

        public static int MiniumHeight{
            get{
                return 1*Constants.LabelHeight + 2*Constants.TextBoxHeight + 1*Constants.ListSelectorHeight +
                       Constants.puffer;
            }
        }
    }
}