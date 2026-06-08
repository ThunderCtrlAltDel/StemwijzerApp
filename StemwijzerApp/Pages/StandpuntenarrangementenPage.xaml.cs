using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace StemwijzerApp.Pages
{
    public partial class StandpuntenarrangementenPage : Page
    {
        public ObservableCollection<VoorbeeldArrangement> Arrangementen { get; set; }
        public ObservableCollection<SelecteerbaarStandpunt> BeschikbareStandpunten { get; set; }
        private VoorbeeldArrangement _geselecteerdArrangement;
        private bool _isLaden = false;

        private readonly string _arrangementenPad = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "StemwijzerApp",
            "arrangementen.json"
        );

        private readonly string _standpuntenPad = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "StemwijzerApp",
            "standpunten.json"
        );

        public StandpuntenarrangementenPage()
        {
            InitializeComponent();
            LoadStandpuntenData();
            LoadArrangementen();
            DataContext = this;

            this.Loaded += StandpuntenarrangementenPage_Loaded;
            this.Unloaded += StandpuntenarrangementenPage_Unloaded;
        }

        private void StandpuntenarrangementenPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadStandpuntenData();
            LoadArrangementen();

            var tijdelijk = Arrangementen;
            Arrangementen = null;
            Arrangementen = tijdelijk;
        }

        private void LoadStandpuntenData()
        {
            BeschikbareStandpunten = new ObservableCollection<SelecteerbaarStandpunt>();
            try
            {
                if (File.Exists(_standpuntenPad))
                {
                    string jsonString = File.ReadAllText(_standpuntenPad);
                    var geladenStandpunten = JsonSerializer.Deserialize<List<VoorbeeldStandpuntMock>>(jsonString);
                    if (geladenStandpunten != null)
                    {
                        foreach (var s in geladenStandpunten)
                        {
                            var nieuwItem = new SelecteerbaarStandpunt { Titel = s.Titel, Categorie = s.Categorie };
                            nieuwItem.SelectionChanged += (sender, e) => UpdateAantalGeselecteerdText();
                            BeschikbareStandpunten.Add(nieuwItem);
                        }
                    }
                }
            }
            catch
            {
            }

            LstStandpunten.ItemsSource = BeschikbareStandpunten;
        }

        private void LoadArrangementen()
        {
            try
            {
                if (File.Exists(_arrangementenPad))
                {
                    string jsonString = File.ReadAllText(_arrangementenPad);
                    Arrangementen = JsonSerializer.Deserialize<ObservableCollection<VoorbeeldArrangement>>(jsonString);
                }
            }
            catch
            {
            }

            if (Arrangementen != null && BeschikbareStandpunten != null)
            {
                bool dataGewijzigd = false;
                var bestaandeTitels = BeschikbareStandpunten.Select(s => s.Titel).ToHashSet();

                foreach (var arrangement in Arrangementen)
                {
                    if (arrangement.GeselecteerdeTitels != null)
                    {
                        int voorFiltering = arrangement.GeselecteerdeTitels.Count;
                        arrangement.GeselecteerdeTitels = arrangement.GeselecteerdeTitels
                            .Where(titel => bestaandeTitels.Contains(titel))
                            .ToList();

                        if (arrangement.GeselecteerdeTitels.Count != voorFiltering)
                        {
                            dataGewijzigd = true;
                        }
                    }
                }

                if (dataGewijzigd)
                {
                    SaveArrangementen();
                }
            }

            if (Arrangementen == null)
            {
                Arrangementen = new ObservableCollection<VoorbeeldArrangement>
                {
                    new VoorbeeldArrangement
                    {
                        Naam = "Standaard Verkiezingsvragen 2025",
                        Beschrijving = "Complete vragenset voor de Tweede Kamerverkiezingen",
                        Verkiezing = "Tweede Kamerverkiezingen 2025",
                        GeselecteerdeTitels = new List<string> { "Meer windmolens bouwen", "Belastingverlaging middeninkomens" },
                        Aangemaakt = "1-10-2024"
                    }
                };
                SaveArrangementen();
            }
        }

        private void SaveArrangementen()
        {
            try
            {
                string mapPad = Path.GetDirectoryName(_arrangementenPad);
                if (!Directory.Exists(mapPad))
                {
                    Directory.CreateDirectory(mapPad);
                }

                string jsonString = JsonSerializer.Serialize(Arrangementen, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_arrangementenPad, jsonString);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij het opslaan: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void NieuwArrangement_Click(object sender, RoutedEventArgs e)
        {
            _isLaden = true;
            LoadStandpuntenData();

            _geselecteerdArrangement = null;
            TxtNaam.Clear();
            TxtBeschrijving.Clear();
            CmbVerkiezing.SelectedIndex = 0;

            foreach (var s in BeschikbareStandpunten) s.IsGeselecteerd = false;

            _isLaden = false;
            UpdateAantalGeselecteerdText();

            LblFormTitel.Text = "Nieuw Arrangement Toevoegen";
            BtnToevoegen.Content = "Toevoegen";
            NieuwArrangementForm.Visibility = Visibility.Visible;
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

            var gekozenTitels = BeschikbareStandpunten.Where(s => s.IsGeselecteerd).Select(s => s.Titel).ToList();
            string verkiezingText = (CmbVerkiezing.SelectedItem as ComboBoxItem)?.Content.ToString();

            if (_geselecteerdArrangement == null)
            {
                VoorbeeldArrangement nieuw = new VoorbeeldArrangement
                {
                    Naam = TxtNaam.Text,
                    Beschrijving = TxtBeschrijving.Text,
                    Verkiezing = verkiezingText,
                    GeselecteerdeTitels = gekozenTitels,
                    Aangemaakt = DateTime.Now.ToString("d-M-yyyy")
                };
                Arrangementen.Add(nieuw);
            }
            else
            {
                _geselecteerdArrangement.Naam = TxtNaam.Text;
                _geselecteerdArrangement.Beschrijving = TxtBeschrijving.Text;
                _geselecteerdArrangement.Verkiezing = verkiezingText;
                _geselecteerdArrangement.GeselecteerdeTitels = gekozenTitels;
            }

            SaveArrangementen();
            ClearForm();

            StandpuntenarrangementenPage_Loaded(null, null);
        }

        private void BewerkArrangement_Click(object sender, RoutedEventArgs e)
        {
            _isLaden = true;
            LoadStandpuntenData();

            Button knop = sender as Button;
            if (knop != null)
            {
                _geselecteerdArrangement = knop.CommandParameter as VoorbeeldArrangement;
                if (_geselecteerdArrangement != null)
                {
                    TxtNaam.Text = _geselecteerdArrangement.Naam;
                    TxtBeschrijving.Text = _geselecteerdArrangement.Beschrijving;

                    for (int i = 0; i < CmbVerkiezing.Items.Count; i++)
                    {
                        if ((CmbVerkiezing.Items[i] as ComboBoxItem)?.Content.ToString() == _geselecteerdArrangement.Verkiezing)
                        {
                            CmbVerkiezing.SelectedIndex = i;
                            break;
                        }
                    }

                    foreach (var s in BeschikbareStandpunten)
                    {
                        s.IsGeselecteerd = _geselecteerdArrangement.GeselecteerdeTitels != null &&
                                             _geselecteerdArrangement.GeselecteerdeTitels.Contains(s.Titel);
                    }

                    _isLaden = false;
                    UpdateAantalGeselecteerdText();

                    LblFormTitel.Text = "Arrangement Bewerken";
                    BtnToevoegen.Content = "Opslaan";
                    NieuwArrangementForm.Visibility = Visibility.Visible;
                }
            }
            _isLaden = false;
        }

        private void VerwijderArrangement_Click(object sender, RoutedEventArgs e)
        {
            Button knop = sender as Button;
            if (knop != null)
            {
                VoorbeeldArrangement item = knop.CommandParameter as VoorbeeldArrangement;
                if (item != null)
                {
                    if (_geselecteerdArrangement == item) ClearForm();
                    Arrangementen.Remove(item);
                    SaveArrangementen();
                }
            }
        }

        private void UpdateAantalGeselecteerdText()
        {
            if (_isLaden) return;

            int aantal = BeschikbareStandpunten.Count(s => s.IsGeselecteerd);
            LblAantalGeselecteerd.Text = $"{aantal} standpunt(en) geselecteerd";
        }

        private void ClearForm()
        {
            _isLaden = true;
            TxtNaam.Clear();
            TxtBeschrijving.Clear();
            CmbVerkiezing.SelectedIndex = 0;
            foreach (var s in BeschikbareStandpunten) s.IsGeselecteerd = false;
            _geselecteerdArrangement = null;
            _isLaden = false;
            UpdateAantalGeselecteerdText();
            NieuwArrangementForm.Visibility = Visibility.Collapsed;
        }

        private void StandpuntenarrangementenPage_Unloaded(object sender, RoutedEventArgs e)
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

    public class VoorbeeldArrangement : System.ComponentModel.INotifyPropertyChanged
    {
        private string _naam;
        private string _beschrijving;
        private string _verkiezing;
        private List<string> _geselecteerdeTitels = new List<string>();
        private string _aangemaakt;

        public string Naam { get => _naam; set { _naam = value; OnPropertyChanged(nameof(Naam)); } }
        public string Beschrijving { get => _beschrijving; set { _beschrijving = value; OnPropertyChanged(nameof(Beschrijving)); } }
        public string Verkiezing { get => _verkiezing; set { _verkiezing = value; OnPropertyChanged(nameof(Verkiezing)); } }
        public string Aangemaakt { get => _aangemaakt; set { _aangemaakt = value; OnPropertyChanged(nameof(Aangemaakt)); } }

        public List<string> GeselecteerdeTitels
        {
            get => _geselecteerdeTitels;
            set
            {
                _geselecteerdeTitels = value;
                OnPropertyChanged(nameof(GeselecteerdeTitels));
                OnPropertyChanged(nameof(AantalText));
            }
        }

        public string AantalText => $"{GeselecteerdeTitels?.Count ?? 0} standpunten";

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
    }

    public class SelecteerbaarStandpunt : System.ComponentModel.INotifyPropertyChanged
    {
        private bool _isGeselecteerd;
        public string Titel { get; set; }
        public string Categorie { get; set; }

        public bool IsGeselecteerd
        {
            get => _isGeselecteerd;
            set
            {
                _isGeselecteerd = value;
                OnPropertyChanged(nameof(IsGeselecteerd));
                SelectionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        public event EventHandler SelectionChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
    }

    public class VoorbeeldStandpuntMock
    {
        public string Titel { get; set; }
        public string Categorie { get; set; }
    }
}