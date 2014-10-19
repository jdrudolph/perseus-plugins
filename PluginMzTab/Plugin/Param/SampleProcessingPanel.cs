using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BaseLib.Wpf;
using PluginMzTab.Lib.Model;
using PluginMzTab.Plugin.Utils;

namespace PluginMzTab.Plugin.Param{
    public class SampleProcessingPanel : StackPanel{
        private readonly CVLookUp _cv;
        private readonly int _count;

        private readonly IList<ListSelectorControl> _steps = new List<ListSelectorControl>();

        public SampleProcessingPanel(SplitList<Lib.Model.Param>[] steps, CVLookUp cv){
            _cv = cv;
            _count = steps.Length;

            InitializeComponent();

            Value = steps;
        }

        private void InitializeComponent(){
            _steps.Clear();

            Grid grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition{Height = new GridLength(1, GridUnitType.Auto)});
            grid.RowDefinitions.Add(new RowDefinition{Height = new GridLength(100, GridUnitType.Star)});

            grid.ColumnDefinitions.Add(new ColumnDefinition{Width = new GridLength(1, GridUnitType.Auto)});
            for (int i = 0; i < _count; i++){
                grid.ColumnDefinitions.Add(new ColumnDefinition{Width = new GridLength(1, GridUnitType.Auto)});
            }

            for (int n = 0; n < _count; n++){
                Label label = new Label{
                    Content = string.Format("STEP {0}", (n + 1)),
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Top
                };
                Grid.SetRow(label, 0);
                Grid.SetColumn(label, n + 1);
                grid.Children.Add(label);

                ListSelectorControl items = new ListSelectorControl();
                List<string> values = _cv.GetSampleProcessing();
                values.Sort();
                foreach (var v in values){
                    items.Items.Add(v);
                }
                Grid.SetRow(items, 1);
                Grid.SetColumn(items, n + 1);
                grid.Children.Add(items);
                _steps.Add(items);
            }

            Children.Add(grid);
        }

        public SplitList<Lib.Model.Param>[] Value{
            get{
                IList<SplitList<Lib.Model.Param>> result = new List<SplitList<Lib.Model.Param>>();
                for (int i = 0; i < _count; i++){
                    if (_steps[i].SelectedItems.Count > 0){
                        SplitList<Lib.Model.Param> step = new SplitList<Lib.Model.Param>();

                        foreach (object item in _steps[i].SelectedItems){
                            var p = _cv.GetParam(item.ToString(), "MS");
                            if (p is UserParam){
                                p = _cv.GetParam(item.ToString(), "SEP");
                            }
                            step.Add(p);
                        }

                        result.Add(step);
                    }
                }
                return result.ToArray();
            }
            set{
                if (value == null){
                    return;
                }

                SplitList<Lib.Model.Param>[] values = value;
                for (int i = 0; i < _count; i++){
                    if (i >= values.Length || values[i] == null){
                        _steps[i].SelectedIndices = new int[0];
                        continue;
                    }

                    if (values[i] != null && _steps[i].Items != null && _steps[i].Items.Count > 0){
                        var selection = new List<string>(values[i].Select(x => x.Name));
                        _steps[i].SelectedIndices = MzTabMatrixUtils.GetSelectedIndices(selection, _steps[i].Items);
                    }
                }
            }
        }

        public static int MiniumHeight { get { return 1*Constants.LabelHeight + 1*Constants.ListSelectorHeight + Constants.puffer; } }
    }
}