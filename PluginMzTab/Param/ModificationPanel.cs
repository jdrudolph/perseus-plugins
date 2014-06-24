using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BaseLib.Mol;
using MzTabLibrary.model;
using PluginMzTab.utils;

namespace PluginMzTab.param{
    public class ModificationPanel : StackPanel
    {
        private readonly int _count;
        private readonly MetadataElement _type = MetadataElement.FIXED_MOD;

        private readonly IList<ComboBox> _names = new List<ComboBox>();
        private readonly IList<TextBox> _site = new List<TextBox>();
        private readonly IList<TextBox> _position = new List<TextBox>();
        private readonly CVLookUp _cv;

        public ModificationPanel(Mod[] value, CVLookUp cv)
        {            
            _count = value.Length;
            _type = value.Length > 0 && value.First() != null ? value.First().Element : MetadataElement.FIXED_MOD;
            _cv = cv;

            InitializeComponent();

            Value = value;
        }

        private void InitializeComponent(){
            _names.Clear();
            _site.Clear();
            _position.Clear();

            Grid grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition{Height = new GridLength(1, GridUnitType.Auto)});
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
            for (int i = 0; i < _count; i++){
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80f/_count, GridUnitType.Star), MinWidth = 100});
            }

            Label label1 = new Label
            {
                Content = "name",
                HorizontalContentAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };
            Grid.SetRow(label1, 1);
            Grid.SetColumn(label1, 0);
            grid.Children.Add(label1);

            Label label2 = new Label
            {
                Content = MetadataProperty.FindProperty(_type, "position").Name,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };
            Grid.SetRow(label2, 2);
            Grid.SetColumn(label2, 0);
            grid.Children.Add(label2);

            Label label3 = new Label
            {
                Content = MetadataProperty.FindProperty(_type, "site").Name,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };
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
                foreach (var cv in Tables.ModificationList.Select(x => x.Name)){
                    name.Items.Add(cv);
                }
                Grid.SetRow(name, 1);
                Grid.SetColumn(name, n + 1);
                grid.Children.Add(name);
                _names.Add(name);

                TextBox position = new TextBox();
                Grid.SetRow(position, 2);
                Grid.SetColumn(position, n + 1);
                grid.Children.Add(position);
                _position.Add(position);

                TextBox site = new TextBox();
                Grid.SetRow(site, 3);
                Grid.SetColumn(site, n + 1);
                grid.Children.Add(site);
                _site.Add(site);
            }
            
            Children.Add(grid);
        }

        public Mod[] Value{
            get{
                Mod[] result = new Mod[_count];

                for (int i = 0; i < _count; i++){
                    int id = i + 1;
                    Mod mod = _type.Equals(MetadataElement.FIXED_MOD) ? (Mod) new FixedMod(id) : new VariableMod(id);

                    if (_names[i].SelectedItem != null){
                        string temp = _names[i].SelectedItem.ToString();
                        if (Tables.Modifications.ContainsKey(temp)){
                            mod.Param = _cv.GetModificationParam(Tables.Modifications[temp]);
                        }
                    }
                    if (!string.IsNullOrEmpty(_position[i].Text)){
                        mod.Position = _position[i].Text;
                    }
                    if (!string.IsNullOrEmpty(_site[i].Text))
                    {
                        mod.Site = _site[i].Text;
                    }

                    result[i] = mod;
                }

                return result;
            }
            set{
                if (value == null){
                    return;
                }

                Mod[] mods = value;

                for (int i = 0; i < _count; i++){
                    if (i >= mods.Length || mods[i] == null){
                        _names[i].Text = "";
                        _site[i].Text = "";
                        _position[i].Text = "";
                        continue;
                    }

                    if (mods[i].Param != null){
                        _names[i].SelectedIndex = MzTabMatrixUtils.GetSelectedIndex(mods[i].Param.Name, _names[i].Items);
                    }
                    if (!string.IsNullOrEmpty(mods[i].Position)){
                        _position[i].Text = mods[i].Position;
                    }
                    if (!string.IsNullOrEmpty(mods[i].Site)){
                        _site[i].Text = mods[i].Site;
                    }
                }
            }
        }

        public static int MinimumHeight { get { return 1*Constants.LabelHeight + Math.Max(Constants.ComboBoxHeight, Constants.LabelHeight) +  2*Math.Max(Constants.TextBoxHeight,Constants.LabelHeight) + Constants.puffer; } }
    }
}