using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace StemwijzerApp.Pages
{
    public partial class VerkiezingenPage : Page
    {
        public ObservableCollection<VoorbeeldVerkiezing> Verkiezingen { get; set; }
        private VoorbeeldVerkiezing _geselecteerdeVerkiezing;

        private readonly string _bestandsPad = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "StemwijzerApp",
            "verkiezingen.json"
        );

        public VerkiezingenPage()
        {
            InitializeComponent();
            LoadVerkiezingen();
            DataContext = this;

            this.Unloaded += VerkiezingenPage_Unloaded;
        }

        private void LoadVerkiezingen()
        {
            try
            {
                if (File.Exists(_bestandsPad))
                {
                    string jsonString = File.ReadAllText(_bestandsPad);
                    Verkiezingen = JsonSerializer.Deserialize<ObservableCollection<VoorbeeldVerkiezing>>(jsonString);
                }
            }
            catch
            {
            }

            if (Verkiezingen == null)
            {
                Verkiezingen = new ObservableCollection<VoorbeeldVerkiezing>
                {
                    new VoorbeeldVerkiezing
                    {
                        Naam = "Tweede Kamerverkiezingen 2025",
                        Datum = "22-11-2025",
                        Type = "Landelijk",
                        Beschrijving = "Verkiezingen voor de Tweede Kamer"
                    }
                };
                SaveVerkiezingen();
            }
        }

        private void SaveVerkiezingen()
        {
            try
            {
                string mapPad = Path.GetDirectoryName(_bestandsPad);
                if (!Directory.Exists(mapPad))
                {
                    Directory.CreateDirectory(mapPad);
                }

                string jsonString = JsonSerializer.Serialize(Verkiezingen, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_bestandsPad, jsonString);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij het opslaan: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void NieuweVerkiezing_Click(object sender, RoutedEventArgs e)
        {
            _geselecteerdeVerkiezing = null;
            TxtNaam.Clear();
            DpDatum.SelectedDate = null;
            CmbType.SelectedIndex = 0;
            TxtBeschrijving.Clear();

            LblFormTitel.Text = "Nieuwe Verkiezing Toevoegen";
            BtnToevoegen.Content = "Toevoegen";
            NieuweVerkiezingForm.Visibility = Visibility.Visible;
        }

        private void Annuleren_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void Toevoegen_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtNaam.Text))
            {
                MessageBox.Show("Vul tenminste een naam in.", "Melding", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string datumText = DpDatum.SelectedDate.HasValue ? DpDatum.SelectedDate.Value.ToString("d-M-yyyy") : string.Empty;
            string typeText = CmbType.SelectedIndex > 0 ? (CmbType.SelectedItem as ComboBoxItem)?.Content.ToString() : string.Empty;

            if (_geselecteerdeVerkiezing == null)
            {
                VoorbeeldVerkiezing nieuw = new VoorbeeldVerkiezing
                {
                    Naam = TxtNaam.Text,
                    Datum = datumText,
                    Type = typeText,
                    Beschrijving = TxtBeschrijving.Text
                };
                Verkiezingen.Add(nieuw);
            }
            else
            {
                _geselecteerdeVerkiezing.Naam = TxtNaam.Text;
                _geselecteerdeVerkiezing.Datum = datumText;
                _geselecteerdeVerkiezing.Type = typeText;
                _geselecteerdeVerkiezing.Beschrijving = TxtBeschrijving.Text;
            }

            SaveVerkiezingen();
            ClearForm();
        }

        private void BewerkVerkiezing_Click(object sender, RoutedEventArgs e)
        {
            Button knop = sender as Button;
            if (knop != null)
            {
                _geselecteerdeVerkiezing = knop.CommandParameter as VoorbeeldVerkiezing;
                if (_geselecteerdeVerkiezing != null)
                {
                    TxtNaam.Text = _geselecteerdeVerkiezing.Naam;
                    TxtBeschrijving.Text = _geselecteerdeVerkiezing.Beschrijving;

                    if (DateTime.TryParse(_geselecteerdeVerkiezing.Datum, out DateTime parsedDate))
                    {
                        DpDatum.SelectedDate = parsedDate;
                    }
                    else
                    {
                        DpDatum.SelectedDate = null;
                    }

                    CmbType.SelectedIndex = 0;
                    for (int i = 0; i < CmbType.Items.Count; i++)
                    {
                        if ((CmbType.Items[i] as ComboBoxItem)?.Content.ToString() == _geselecteerdeVerkiezing.Type)
                        {
                            CmbType.SelectedIndex = i;
                            break;
                        }
                    }

                    LblFormTitel.Text = "Verkiezing Bewerken";
                    BtnToevoegen.Content = "Opslaan";
                    NieuweVerkiezingForm.Visibility = Visibility.Visible;
                }
            }
        }

        private void VerwijderVerkiezing_Click(object sender, RoutedEventArgs e)
        {
            Button knop = sender as Button;
            if (knop != null)
            {
                VoorbeeldVerkiezing item = knop.CommandParameter as VoorbeeldVerkiezing;
                if (item != null)
                {
                    if (_geselecteerdeVerkiezing == item) ClearForm();
                    Verkiezingen.Remove(item);
                    SaveVerkiezingen();
                }
            }
        }

        private void ClearForm()
        {
            TxtNaam.Clear();
            DpDatum.SelectedDate = null;
            CmbType.SelectedIndex = 0;
            TxtBeschrijving.Clear();
            _geselecteerdeVerkiezing = null;
            NieuweVerkiezingForm.Visibility = Visibility.Collapsed;
        }

        private void VerkiezingenPage_Unloaded(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void DataGrid_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = true;
                var eventArg = new System.Windows.Input.MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
                {
                    RoutedEvent = UIElement.MouseWheelEvent,
                    Source = sender
                };
                var parent = ((Control)sender).Parent as UIElement;
                if (parent != null)
                {
                    parent.RaiseEvent(eventArg);
                }
            }
        }
    }

    public class VoorbeeldVerkiezing : System.ComponentModel.INotifyPropertyChanged
    {
        private string _naam;
        private string _datum;
        private string _type;
        private string _beschrijving;

        public string Naam { get => _naam; set { _naam = value; OnPropertyChanged(nameof(Naam)); } }
        public string Datum { get => _datum; set { _datum = value; OnPropertyChanged(nameof(Datum)); } }
        public string Type { get => _type; set { _type = value; OnPropertyChanged(nameof(Type)); } }
        public string Beschrijving { get => _beschrijving; set { _beschrijving = value; OnPropertyChanged(nameof(Beschrijving)); } }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
    }
}