using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PluginMzTab.Lib.Model;
using PluginMzTab.Plugin.Utils;

namespace PluginMzTab.Plugin.Param{
    public class PublicationPanel : StackPanel{
        private readonly int _count;

        private readonly IList<TextBox> _pubmeds = new List<TextBox>();
        private readonly IList<TextBox> _dois = new List<TextBox>();

        public PublicationPanel(Publication[] value){
            _count = value.Length;

            InitializeComponent();

            Value = value;
        }

        private void InitializeComponent(){
            _pubmeds.Clear();
            _dois.Clear();

            Grid grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition{Height = new GridLength(1, GridUnitType.Auto)});
            grid.RowDefinitions.Add(new RowDefinition{Height = new GridLength(1, GridUnitType.Auto)});
            grid.RowDefinitions.Add(new RowDefinition{Height = new GridLength(1, GridUnitType.Auto)});

            grid.ColumnDefinitions.Add(new ColumnDefinition{Width = new GridLength(1, GridUnitType.Auto)});
            for (int i = 0; i < _count; i++){
                grid.ColumnDefinitions.Add(new ColumnDefinition{Width = new GridLength(80f/_count, GridUnitType.Star)});
            }

            Label label1 = new Label{
                Content = @"pubmed",
                HorizontalContentAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };
            Grid.SetRow(label1, 1);
            Grid.SetColumn(label1, 0);
            grid.Children.Add(label1);

            Label label2 = new Label{
                Content = @"DOI",
                HorizontalContentAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };
            Grid.SetRow(label2, 2);
            Grid.SetColumn(label2, 0);
            grid.Children.Add(label2);

            for (int n = 0; n < _count; n++){
                Label label = new Label{
                    Content = string.Format("[{0}]", (n + 1)),
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Top
                };
                Grid.SetRow(label, 0);
                Grid.SetColumn(label, n + 1);
                grid.Children.Add(label);

                TextBox pubmed = new TextBox();
                Grid.SetRow(pubmed, 1);
                Grid.SetColumn(pubmed, n + 1);
                grid.Children.Add(pubmed);
                _pubmeds.Add(pubmed);

                TextBox doi = new TextBox();
                Grid.SetRow(doi, 2);
                Grid.SetColumn(doi, n + 1);
                grid.Children.Add(doi);
                _dois.Add(doi);
            }

            Children.Add(grid);
        }

        public Publication[] Value{
            get{
                Publication[] result = new Publication[_count];

                for (int i = 0; i < _count; i++){
                    Publication publication = new Publication(i + 1);
                    if (!string.IsNullOrEmpty(_pubmeds[i].Text)){
                        publication.AddPublicationItem(new PublicationItem(PublicationType.PUBMED, _pubmeds[i].Text));
                    }

                    if (!string.IsNullOrEmpty(_dois[i].Text)){
                        publication.AddPublicationItem(new PublicationItem(PublicationType.DOI, _dois[i].Text));
                    }

                    if (publication.Items.Count > 0){
                        result[i] = publication;
                    }
                }

                return result;
            }

            set{
                if (value == null){
                    return;
                }
                Publication[] publications = value;

                for (int i = 0; i < _count; i++){
                    if (i >= publications.Length || publications[i] == null){
                        _pubmeds[i].Text = "";
                        _dois[i].Text = "";
                        continue;
                    }

                    var source = publications[i].Items.FirstOrDefault(x => x.PublicationType == PublicationType.PUBMED);
                    if (source != null){
                        _pubmeds[i].Text = source.Accession;
                    }

                    source = publications[i].Items.FirstOrDefault(x => x.PublicationType == PublicationType.DOI);
                    if (source != null){
                        _dois[i].Text = source.Accession;
                    }
                }
            }
        }

        public static int MiniumHeight{
            get{
                return 1*Constants.LabelHeight + 2*Math.Max(Constants.TextBoxHeight, Constants.LabelHeight) +
                       Constants.puffer;
            }
        }
    }
}