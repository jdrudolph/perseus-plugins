using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using PluginMzTab.Lib.Model;
using PluginMzTab.Plugin.Utils;

namespace PluginMzTab.Plugin.Param{
    public class ContactPanel : StackPanel{
        private readonly int _count;

        private readonly IList<TextBox> _names = new List<TextBox>();
        private readonly IList<TextBox> _affiliations = new List<TextBox>();
        private readonly IList<TextBox> _emails = new List<TextBox>();

        public ContactPanel(Contact[] value){
            _count = value.Length;

            InitializeComponent();

            Value = value;
        }

        private void InitializeComponent(){
            _names.Clear();
            _affiliations.Clear();
            _emails.Clear();

            Grid grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition{Height = new GridLength(1, GridUnitType.Auto)});
            grid.RowDefinitions.Add(new RowDefinition{Height = new GridLength(1, GridUnitType.Auto)});
            grid.RowDefinitions.Add(new RowDefinition{Height = new GridLength(1, GridUnitType.Auto)});
            grid.RowDefinitions.Add(new RowDefinition{Height = new GridLength(1, GridUnitType.Auto)});

            grid.ColumnDefinitions.Add(new ColumnDefinition{Width = new GridLength(1, GridUnitType.Auto)});
            for (int i = 0; i < _count; i++){
                grid.ColumnDefinitions.Add(new ColumnDefinition{Width = new GridLength(80f/_count, GridUnitType.Star)});
            }

            Label label1 = new Label{
                Content = MetadataProperty.CONTACT_NAME.Name,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };
            Grid.SetRow(label1, 1);
            Grid.SetColumn(label1, 0);
            grid.Children.Add(label1);

            Label label2 = new Label{
                Content = MetadataProperty.CONTACT_AFFILIATION.Name,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };
            Grid.SetRow(label2, 2);
            Grid.SetColumn(label2, 0);
            grid.Children.Add(label2);

            Label label3 = new Label{
                Content = MetadataProperty.CONTACT_EMAIL.Name,
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
                    VerticalAlignment = VerticalAlignment.Top
                };
                Grid.SetRow(label, 0);
                Grid.SetColumn(label, n + 1);
                grid.Children.Add(label);

                TextBox name = new TextBox();
                Grid.SetRow(name, 1);
                Grid.SetColumn(name, n + 1);
                grid.Children.Add(name);
                _names.Add(name);

                TextBox affiliation = new TextBox();
                Grid.SetRow(affiliation, 2);
                Grid.SetColumn(affiliation, n + 1);
                grid.Children.Add(affiliation);
                _affiliations.Add(affiliation);

                TextBox email = new TextBox();
                Grid.SetRow(email, 3);
                Grid.SetColumn(email, n + 1);
                grid.Children.Add(email);
                _emails.Add(email);
            }

            Children.Add(grid);
        }

        public Contact[] Value{
            get{
                Contact[] result = new Contact[_count];

                for (int i = 0; i < _count; i++){
                    Contact contact = new Contact(i + 1);

                    if (!string.IsNullOrEmpty(_names[i].Text)){
                        contact.Name = _names[i].Text;
                    }
                    if (!string.IsNullOrEmpty(_affiliations[i].Text)){
                        contact.Affiliation = _affiliations[i].Text;
                    }
                    if (!string.IsNullOrEmpty(_emails[i].Text)){
                        contact.Email = _emails[i].Text;
                    }

                    result[i] = contact;
                }

                return result;
            }
            set{
                if (value == null){
                    return;
                }

                Contact[] contacts = value;

                for (int i = 0; i < _count; i++){
                    if (i >= contacts.Length || contacts[i] == null){
                        _names[i].Text = "";
                        _affiliations[i].Text = @"Max Planck Institute of Biochemistry";
                        _emails[i].Text = "";
                        continue;
                    }

                    if (!string.IsNullOrEmpty(contacts[i].Name)){
                        _names[i].Text = contacts[i].Name;
                    }
                    if (!string.IsNullOrEmpty(contacts[i].Affiliation)){
                        _affiliations[i].Text = contacts[i].Affiliation;
                    }
                    if (!string.IsNullOrEmpty(contacts[i].Email)){
                        _emails[i].Text = contacts[i].Email;
                    }
                }
            }
        }

        public static int MinimumHeight{
            get{
                return 1*Constants.LabelHeight + 3*Math.Max(Constants.TextBoxHeight, Constants.LabelHeight) +
                       Constants.puffer;
            }
        }
    }
}