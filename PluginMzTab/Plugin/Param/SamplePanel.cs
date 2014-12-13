using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PluginMzTab.Lib.Model;
using PluginMzTab.Plugin.Utils;

namespace PluginMzTab.Plugin.Param{
    public class SamplePanel : StackPanel{
        private readonly int _count;
        private readonly bool _variable;
        private readonly CVLookUp _cv;
        private readonly IList<Control> _descriptions = new List<Control>();
        private readonly IList<ComboBox> _species = new List<ComboBox>();
        private readonly IList<ComboBox> _tissues = new List<ComboBox>();
        private readonly IList<ComboBox> _cellTypes = new List<ComboBox>();
        private readonly IList<ComboBox> _diseases = new List<ComboBox>();

        public SamplePanel(Sample[] value, bool variable, CVLookUp cv){
            _variable = variable;
            _cv = cv;
            _count = value == null ? 0 : value.Length;

            InitializeComponent();

            Value = value;
        }

        private const int numElement = 4;

        private void InitializeComponent(){
            _descriptions.Clear();
            _species.Clear();
            _tissues.Clear();
            _cellTypes.Clear();
            _diseases.Clear();

            var speciesList = MzTabMatrixUtils.Sort(_cv.GetSpecies("NEWT"));
            var tissueList = MzTabMatrixUtils.Sort(_cv.GetTissues("BTO"));
            var cellTypeList = MzTabMatrixUtils.Sort(_cv.GetCellTypes("CL"));
            var diseaseList = MzTabMatrixUtils.Sort(_cv.GetDiseases("DOID"));

            WrapPanel flowPanel = new WrapPanel{MaxWidth = 750, HorizontalAlignment = HorizontalAlignment.Left};

            for (int n = 0; n < _count; n++){
                Grid grid = new Grid();

                grid.RowDefinitions.Add(_variable
                                            ? new RowDefinition{
                                                Height = new GridLength(Constants.LabelHeight, GridUnitType.Pixel)
                                            }
                                            : new RowDefinition());
                grid.RowDefinitions.Add(new RowDefinition{
                    Height =
                        new GridLength(Math.Max(Constants.TextBoxHeight, Constants.LabelHeight), GridUnitType.Pixel)
                });
                grid.RowDefinitions.Add(new RowDefinition{
                    Height =
                        new GridLength(Math.Max(Constants.ComboBoxHeight, Constants.LabelHeight), GridUnitType.Pixel)
                });
                grid.RowDefinitions.Add(new RowDefinition{
                    Height =
                        new GridLength(Math.Max(Constants.ComboBoxHeight, Constants.LabelHeight), GridUnitType.Pixel)
                });
                grid.RowDefinitions.Add(new RowDefinition{
                    Height =
                        new GridLength(Math.Max(Constants.ComboBoxHeight, Constants.LabelHeight), GridUnitType.Pixel)
                });
                grid.RowDefinitions.Add(new RowDefinition{
                    Height =
                        new GridLength(Math.Max(Constants.ComboBoxHeight, Constants.LabelHeight), GridUnitType.Pixel)
                });

                int c = 0;

                if (n%numElement == 0){
                    grid.ColumnDefinitions.Add(new ColumnDefinition{Width = new GridLength(80, GridUnitType.Pixel)});

                    Label label1 = new Label{Content = MetadataProperty.SAMPLE_DESCRIPTION.Name};
                    Grid.SetRow(label1, 1);
                    Grid.SetColumn(label1, c);
                    grid.Children.Add(label1);

                    Label label2 = new Label{Content = MetadataProperty.SAMPLE_SPECIES.Name};
                    Grid.SetRow(label2, 2);
                    Grid.SetColumn(label2, c);
                    grid.Children.Add(label2);

                    Label label3 = new Label{Content = MetadataProperty.SAMPLE_TISSUE.Name};
                    Grid.SetRow(label3, 3);
                    Grid.SetColumn(label3, c);
                    grid.Children.Add(label3);

                    Label label4 = new Label{Content = MetadataProperty.SAMPLE_CELL_TYPE.Name};
                    Grid.SetRow(label4, 4);
                    Grid.SetColumn(label4, c);
                    grid.Children.Add(label4);

                    Label label5 = new Label{Content = MetadataProperty.SAMPLE_DISEASE.Name};
                    Grid.SetRow(label5, 5);
                    Grid.SetColumn(label5, c);
                    grid.Children.Add(label5);

                    c++;
                }


                grid.ColumnDefinitions.Add(new ColumnDefinition{
                    Width = new GridLength((flowPanel.MaxWidth - 80)*0.9/numElement, GridUnitType.Pixel)
                });

                string name = string.Format("sample[{0}]", (n + 1));
                Label label = new Label{
                    Content = name,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Top,
                    FontWeight = FontWeights.Bold
                };

                if (_variable){
                    Grid.SetRow(label, 0);
                    Grid.SetColumn(label, c);
                    grid.Children.Add(label);
                }

                Control description;
                if (_variable){
                    description = new TextBox();
                }
                else{
                    description = new Label{
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Top,
                        FontWeight = FontWeights.Bold
                    };
                }
                _descriptions.Add(description);
                Grid.SetRow(description, 1);
                Grid.SetColumn(description, c);
                grid.Children.Add(description);

                ComboBox species = new ComboBox();
                species.MouseDoubleClick += ApplyToAll;
                foreach (var cv in speciesList){
                    species.Items.Add(cv);
                }
                _species.Add(species);
                Grid.SetRow(species, 2);
                Grid.SetColumn(species, c);
                grid.Children.Add(species);

                ComboBox tissue = new ComboBox();
                tissue.MouseDoubleClick += ApplyToAll;
                foreach (var cv in tissueList){
                    tissue.Items.Add(cv);
                }
                _tissues.Add(tissue);
                Grid.SetRow(tissue, 3);
                Grid.SetColumn(tissue, c);
                grid.Children.Add(tissue);

                ComboBox cellType = new ComboBox();
                cellType.MouseDoubleClick += ApplyToAll;
                foreach (var cv in cellTypeList){
                    cellType.Items.Add(cv);
                }
                _cellTypes.Add(cellType);
                Grid.SetRow(cellType, 4);
                Grid.SetColumn(cellType, c);
                grid.Children.Add(cellType);

                ComboBox disease = new ComboBox();
                disease.MouseDoubleClick += ApplyToAll;
                foreach (var cv in diseaseList){
                    disease.Items.Add(cv);
                }
                _diseases.Add(disease);
                Grid.SetRow(disease, 5);
                Grid.SetColumn(disease, c);
                grid.Children.Add(disease);

                flowPanel.Children.Add(grid);
            }

            Children.Add(flowPanel);
        }

        private void ApplyToAll(object sender, MouseButtonEventArgs e){
            ComboBox b = sender as ComboBox;
            if (b == null){
                return;
            }
            IList<ComboBox> controls = null;
            if (_species != null && _species.Contains(sender)){
                controls = _species;
            }
            else if (_tissues != null && _tissues.Contains(sender)){
                controls = _tissues;
            }
            else if (_cellTypes != null && _cellTypes.Contains(sender)){
                controls = _cellTypes;
            }
            else if (_diseases != null && _diseases.Contains(sender)){
                controls = _diseases;
            }

            if (controls == null){
                return;
            }

            object selection = b.SelectedValue;
            foreach (ComboBox box in controls){
                if (box.Equals(sender)){
                    continue;
                }
                box.SelectedValue = selection;
            }
        }

        public Sample[] Value{
            get{
                IList<Sample> result = new List<Sample>();

                for (int i = 0; i < _count; i++){
                    string text = null;
                    if (_descriptions[i] is Label){
                        text = (_descriptions[i] as Label).Content.ToString();
                    }
                    if (_descriptions[i] is TextBox){
                        text = (_descriptions[i] as TextBox).Text;
                    }
                    if (string.IsNullOrEmpty(text)){
                        continue;
                    }

                    Sample sample = new Sample(result.Count + 1){Description = text};

                    if (_species[i].SelectedItem != null){
                        sample.SpeciesList.Add(_cv.GetParam(_species[i].SelectedItem.ToString(), "NEWT"));
                    }
                    if (_tissues[i].SelectedItem != null){
                        sample.TissueList.Add(_cv.GetParam(_tissues[i].SelectedItem.ToString(), "BTO"));
                    }
                    if (_cellTypes[i].SelectedItem != null){
                        sample.CellTypeList.Add(_cv.GetParam(_cellTypes[i].SelectedItem.ToString(), "CL"));
                    }
                    if (_diseases[i].SelectedItem != null){
                        sample.DiseaseList.Add(_cv.GetParam(_diseases[i].SelectedItem.ToString(), "DOID"));
                    }

                    result.Add(sample);
                }

                return result.ToArray();
            }
            set{
                if (value == null){
                    return;
                }

                Sample[] samples = value;

                for (int i = 0; i < _count; i++){
                    if (i >= samples.Length || samples[i] == null){
                        continue;
                    }

                    if (!string.IsNullOrEmpty(samples[i].Description)){
                        if (_descriptions[i] is Label){
                            (_descriptions[i] as Label).Content = samples[i].Description;
                        }
                        else if (_descriptions[i] is TextBox){
                            (_descriptions[i] as TextBox).Text = samples[i].Description;
                        }
                    }

                    if (samples[i].SpeciesList != null && samples[i].SpeciesList.Count > 0){
                        _species[i].SelectedIndex =
                            MzTabMatrixUtils.GetSelectedIndex(samples[i].SpeciesList.First().Name, _species[i].Items);
                    }

                    if (samples[i].TissueList != null && samples[i].TissueList.Count > 0){
                        _tissues[i].SelectedIndex = MzTabMatrixUtils.GetSelectedIndex(
                            samples[i].TissueList.First().Name, _tissues[i].Items);
                    }

                    if (samples[i].CellTypeList != null && samples[i].CellTypeList.Count > 0){
                        _cellTypes[i].SelectedIndex =
                            MzTabMatrixUtils.GetSelectedIndex(samples[i].CellTypeList.First().Name, _cellTypes[i].Items);
                    }

                    if (samples[i].DiseaseList != null && samples[i].DiseaseList.Count > 0){
                        _diseases[i].SelectedIndex =
                            MzTabMatrixUtils.GetSelectedIndex(samples[i].DiseaseList.First().Name, _diseases[i].Items);
                    }
                }
            }
        }

        public static int MinimumHeight(int count){
            return (count/numElement + (count%numElement == 0 ? 0 : 1))*
                   (Constants.LabelHeight + Math.Max(Constants.TextBoxHeight, Constants.LabelHeight) +
                    4*Math.Max(Constants.ComboBoxHeight, Constants.LabelHeight) + 6);
        }
    }
}