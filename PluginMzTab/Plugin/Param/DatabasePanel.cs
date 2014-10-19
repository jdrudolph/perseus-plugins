using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using BaseLibS.Mol;
using BaseLibS.Util;
using PluginMzTab.Plugin.Utils;
using Button = System.Windows.Controls.Button;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using Label = System.Windows.Controls.Label;
using Panel = System.Windows.Controls.Panel;
using TextBox = System.Windows.Controls.TextBox;

namespace PluginMzTab.Plugin.Param {
    public class DatabasePanel : StackPanel {
        private readonly int _count;

        private readonly IList<TextBox>  _files = new List<TextBox>();
        private readonly IList<Button> _buttons = new List<Button>();

        private readonly IList<TextBox> _versions = new List<TextBox>();
        private readonly IList<TextBox> _sources = new List<TextBox>();
        private readonly IList<TextBox> _species = new List<TextBox>();
        private readonly IList<TextBox> _taxid = new List<TextBox>();        
        private readonly IList<TextBox> _expressions = new List<TextBox>();
        private readonly IList<TextBox> _prefixes = new List<TextBox>(); 
        
        public DatabasePanel(Database[] value) {
            _count = value.Length;

            InitializeComponent();

            Value = value;
        }

        private void InitializeComponent(){
            _files.Clear();
            _versions.Clear();
            _sources.Clear();
            _species.Clear();
            _taxid.Clear();
            _expressions.Clear();     
            _prefixes.Clear();

            Grid grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            grid.RowDefinitions.Add(new RowDefinition{Height = new GridLength(1, GridUnitType.Auto)});
            grid.RowDefinitions.Add(new RowDefinition{Height = new GridLength(1, GridUnitType.Auto)});
            grid.RowDefinitions.Add(new RowDefinition{Height = new GridLength(1, GridUnitType.Auto)});
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });

            grid.ColumnDefinitions.Add(new ColumnDefinition{Width = new GridLength(1, GridUnitType.Auto)});
            for (int i = 0; i < _count; i++){
                grid.ColumnDefinitions.Add(new ColumnDefinition{Width = new GridLength(80f/_count, GridUnitType.Star)});
            }

            AddLabel(grid, @"file", 1, 0);
            AddLabel(grid, @"version", 2, 0);
            AddLabel(grid, @"source", 3, 0);
            AddLabel(grid, @"specie", 4, 0);
            AddLabel(grid, @"taxid", 5, 0);
            AddLabel(grid, @"expression", 6, 0);
            AddLabel(grid, @"prefix", 7, 0);

            for (int n = 0; n < _count; n++){
                Label label = new Label{
                    Content = string.Format("[{0}]", (n + 1)),
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Top
                };
                Grid.SetRow(label, 0);
                Grid.SetColumn(label, n + 1);
                grid.Children.Add(label);

                _files.Add(CreateLoadFile(grid, 1, n + 1));
                _versions.Add(CreateTextBox(grid, 2, n + 1));
                _sources.Add(CreateTextBox(grid, 3, n + 1));
                _species.Add(CreateTextBox(grid, 4, n + 1));
                _taxid.Add(CreateTextBox(grid, 5, n + 1));
                _expressions.Add(CreateTextBox(grid, 6, n + 1));
                _prefixes.Add(CreateTextBox(grid, 7, n + 1));
            }

            Children.Add(grid);
        }

        private TextBox CreateLoadFile(Grid grid, int row, int col){
            Grid internalGrid = new Grid();
            internalGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            internalGrid.ColumnDefinitions.Add(new ColumnDefinition{Width = new GridLength(80, GridUnitType.Star)});
            internalGrid.ColumnDefinitions.Add(new ColumnDefinition{ Width = new GridLength(1, GridUnitType.Auto) });

            TextBox textbox = new TextBox();
            Grid.SetRow(textbox, 0);
            Grid.SetColumn(textbox, 0);
            internalGrid.Children.Add(textbox);

            Button button = new Button{Content= "Select"};
            button.Click += OnButtonClick;
            Grid.SetRow(button, 0);
            Grid.SetColumn(button, 1);
            internalGrid.Children.Add(button);
            _buttons.Add(button);

            Panel panel = new StackPanel();
            Grid.SetRow(panel, row);
            Grid.SetColumn(panel, col);
            grid.Children.Add(panel);

            panel.Children.Add(internalGrid);

            return textbox;
        }

        private static TextBox CreateTextBox(Grid grid, int row, int col){
            TextBox textbox = new TextBox();
            Grid.SetRow(textbox, row);
            Grid.SetColumn(textbox, col);
            grid.Children.Add(textbox);
            return textbox;

        }

        private static void AddLabel(Grid grid, string text, int row, int col){
            Label label = new Label{
                Content = text,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };
            Grid.SetRow(label, row);
            Grid.SetColumn(label, col);
            grid.Children.Add(label);
        }

        private void OnButtonClick(object sender, RoutedEventArgs e) {
            OpenFileDialog dialog = new OpenFileDialog {
                Multiselect = false,
                Filter = FileUtils.fastaFilter
            };

            int index = -1;
            if (sender is Button) {
                index = _buttons.IndexOf(sender as Button);
            }


            if (dialog.ShowDialog() == DialogResult.OK) {
                if (index != -1){
                    _files[index].Text = dialog.FileName;
                    try{
                        string filename = Path.GetFileName(dialog.FileName);
                        if (Tables.Databases.ContainsKey(filename)){
                            SequenceDatabase db = Tables.Databases[filename];
                            _sources[index].Text = db.Source;
                            _species[index].Text = db.Species;
                            _taxid[index].Text = db.Taxid;
                            _expressions[index].Text = db.SearchExpression;

                        }
                    }
                    catch (Exception){
                        Console.Error.WriteLine( "The selected database was not specified in ./conf/database.xml");
                    }
                }
            }
        }

        public Database[] Value {
            get{
                Database[] result = new Database[_count];
                for (int i = 0; i < _count; i++){
                    result[i] = new Database(_files[i].Text, _versions[i].Text, _prefixes[i].Text, _sources[i].Text, _species[i].Text, _taxid[i].Text, _expressions[i].Text);
                }

                return result;
            }

            set{
                if (value == null){
                    return;
                }
                Database[] dbs = value;

                for (int i = 0; i < _count; i++){
                    if (i >= dbs.Length || dbs[i] == null){
                        _files[i].Text = "";
                        _versions[i].Text = "";
                        _sources[i].Text = "";
                        _species[i].Text = "";
                        _taxid[i].Text = "";
                        _expressions[i].Text = "";
                        _prefixes[i].Text = "";
                        continue;
                    }

                    _files[i].Text = dbs[i].File;
                    _versions[i].Text = dbs[i].Version;
                    _sources[i].Text = dbs[i].Source;
                    _species[i].Text = dbs[i].Species;
                    _taxid[i].Text = dbs[i].Taxid;
                    _expressions[i].Text = dbs[i].SearchExpression;
                    _prefixes[i].Text = dbs[i].Prefix;
                }
            }
        }

        public static int MiniumHeight{
            get{
                return 1*Constants.LabelHeight + 7*Math.Max(Constants.TextBoxHeight, Constants.LabelHeight) +
                       Constants.puffer;
            }
        }
    }
}
