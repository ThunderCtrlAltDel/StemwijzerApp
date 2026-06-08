using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace StemwijzerApp.Pages
{
    public partial class StandpuntenPage : Page
    {
        public ObservableCollection<VoorbeeldStandpunt> Standpunten { get; set; }
        private VoorbeeldStandpunt _geselecteerdStandpunt;

        private readonly string _bestandsPad = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "StemwijzerApp",
            "standpunten.json"
        );

        public StandpuntenPage()
        {
            InitializeComponent();
            LoadStandpunten();
            DataContext = this;
        }

        private void LoadStandpunten()
        {
            try
            {
                if (File.Exists(_bestandsPad))
                {
                    string jsonString = File.ReadAllText(_bestandsPad);
                    Standpunten = JsonSerializer.Deserialize<ObservableCollection<VoorbeeldStandpunt>>(jsonString);
                }
            }
            catch
            {
            }

            if (Standpunten == null)
            {
                Standpunten = new ObservableCollection<VoorbeeldStandpunt>
                {
                    new VoorbeeldStandpunt { Titel = "Meer windmolens bouwen", Categorie = "Klimaat", Beschrijving = "Er moeten meer windmolens gebouwd worden voor duurzame energie" },
                    new VoorbeeldStandpunt { Titel = "Belastingverlaging middeninkomens", Categorie = "Economie", Beschrijving = "De belastingen moeten omlaag voor middeninkomens" }
                };
                SaveStandpunten();
            }
        }

        private void SaveStandpunten()
        {
            try
            {
                string mapPad = Path.GetDirectoryName(_bestandsPad);
                if (!Directory.Exists(mapPad))
                {
                    Directory.CreateDirectory(mapPad);
                }

                string jsonString = JsonSerializer.Serialize(Standpunten, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_bestandsPad, jsonString);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij het opslaan van gegevens: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void NieuwStandpunt_Click(object sender, RoutedEventArgs e)
        {
            _geselecteerdStandpunt = null;
            TxtTitel.Text = string.Empty;
            TxtBeschrijving.Text = string.Empty;
            TxtCategorie.Text = string.Empty;

            LblFormTitel.Text = "Nieuw Standpunt Toevoegen";
            BtnToevoegen.Content = "Toevoegen";

            SetFormEditingState(true, isViewing: false);
            NieuwStandpuntForm.Visibility = Visibility.Visible;
        }

        private void Annuleren_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void Toevoegen_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtTitel.Text))
            {
                MessageBox.Show("Vul tenminste een titel in.", "Melding", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_geselecteerdStandpunt == null)
            {
                VoorbeeldStandpunt nieuwStandpunt = new VoorbeeldStandpunt
                {
                    Titel = TxtTitel.Text,
                    Beschrijving = TxtBeschrijving.Text,
                    Categorie = TxtCategorie.Text
                };
                Standpunten.Add(nieuwStandpunt);
            }
            else
            {
                _geselecteerdStandpunt.Titel = TxtTitel.Text;
                _geselecteerdStandpunt.Beschrijving = TxtBeschrijving.Text;
                _geselecteerdStandpunt.Categorie = TxtCategorie.Text;
            }

            SaveStandpunten();
            ClearForm();
        }

        private void BekijkStandpunt_Click(object sender, RoutedEventArgs e)
        {
            Button knop = sender as Button;
            if (knop != null)
            {
                _geselecteerdStandpunt = knop.CommandParameter as VoorbeeldStandpunt;
                if (_geselecteerdStandpunt != null)
                {
                    TxtTitel.Text = _geselecteerdStandpunt.Titel;
                    TxtBeschrijving.Text = _geselecteerdStandpunt.Beschrijving;
                    TxtCategorie.Text = _geselecteerdStandpunt.Categorie;

                    LblFormTitel.Text = "Standpunt Details";

                    SetFormEditingState(false, isViewing: true);
                    NieuwStandpuntForm.Visibility = Visibility.Visible;
                }
            }
        }

        private void BewerkStandpunt_Click(object sender, RoutedEventArgs e)
        {
            Button knop = sender as Button;
            if (knop != null)
            {
                _geselecteerdStandpunt = knop.CommandParameter as VoorbeeldStandpunt;
                if (_geselecteerdStandpunt != null)
                {
                    TxtTitel.Text = _geselecteerdStandpunt.Titel;
                    TxtBeschrijving.Text = _geselecteerdStandpunt.Beschrijving;
                    TxtCategorie.Text = _geselecteerdStandpunt.Categorie;

                    LblFormTitel.Text = "Standpunt Bewerken";
                    BtnToevoegen.Content = "Opslaan";

                    SetFormEditingState(true, isViewing: false);
                    NieuwStandpuntForm.Visibility = Visibility.Visible;
                }
            }
        }

        private void VerwijderStandpunt_Click(object sender, RoutedEventArgs e)
        {
            Button knop = sender as Button;
            if (knop != null)
            {
                VoorbeeldStandpunt standpuntGeklikt = knop.CommandParameter as VoorbeeldStandpunt;
                if (standpuntGeklikt != null)
                {
                    if (_geselecteerdStandpunt == standpuntGeklikt)
                    {
                        ClearForm();
                    }
                    Standpunten.Remove(standpuntGeklikt);
                    SaveStandpunten();
                }
            }
        }

        private void SetFormEditingState(bool isEnabled, bool isViewing)
        {
            TxtTitel.IsEnabled = isEnabled;
            TxtBeschrijving.IsEnabled = isEnabled;
            TxtCategorie.IsEnabled = isEnabled;

            BtnToevoegen.Visibility = isViewing ? Visibility.Collapsed : Visibility.Visible;
            BtnAnnuleren.Visibility = isViewing ? Visibility.Collapsed : Visibility.Visible;
            BtnSluiten.Visibility = isViewing ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ClearForm()
        {
            TxtTitel.Clear();
            TxtBeschrijving.Clear();
            TxtCategorie.Clear();
            _geselecteerdStandpunt = null;
            SetFormEditingState(true, isViewing: false);
            NieuwStandpuntForm.Visibility = Visibility.Collapsed;
        }
    }

    public class VoorbeeldStandpunt : System.ComponentModel.INotifyPropertyChanged
    {
        private string _titel;
        private string _categorie;
        private string _beschrijving;

        public string Titel
        {
            get => _titel;
            set { _titel = value; OnPropertyChanged(nameof(Titel)); }
        }
        public string Categorie
        {
            get => _categorie;
            set { _categorie = value; OnPropertyChanged(nameof(Categorie)); }
        }
        public string Beschrijving
        {
            get => _beschrijving;
            set { _beschrijving = value; OnPropertyChanged(nameof(Beschrijving)); }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}