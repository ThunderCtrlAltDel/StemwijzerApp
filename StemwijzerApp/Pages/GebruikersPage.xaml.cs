using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace StemwijzerApp.Pages
{
    public partial class GebruikersPage : Page
    {
        public ObservableCollection<VoorbeeldGebruiker> Gebruikers { get; set; }
        private VoorbeeldGebruiker _geselecteerdeGebruiker;

        private readonly string _bestandsPad = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "StemwijzerApp",
            "gebruikers.json"
        );

        public GebruikersPage()
        {
            InitializeComponent();
            LoadGebruikers();
            DataContext = this;

            this.Unloaded += GebruikersPage_Unloaded;
        }

        private void LoadGebruikers()
        {
            try
            {
                if (File.Exists(_bestandsPad))
                {
                    string jsonString = File.ReadAllText(_bestandsPad);
                    Gebruikers = JsonSerializer.Deserialize<ObservableCollection<VoorbeeldGebruiker>>(jsonString);
                }
            }
            catch
            {
            }

            if (Gebruikers == null)
            {
                Gebruikers = new ObservableCollection<VoorbeeldGebruiker>
                {
                    new VoorbeeldGebruiker
                    {
                        Voornaam = "Jan",
                        Achternaam = "Jansen",
                        Gebruikersnaam = "gebruiker1",
                        Email = "gebruiker@voorbeeld.nl",
                        Geboortedatum = "15-5-1990",
                        Woonplaats = "Amsterdam",
                        Rol = "Gebruiker",
                        Aangemaakt = "15-1-2024"
                    }
                };
                SaveGebruikers();
            }
        }

        private void SaveGebruikers()
        {
            try
            {
                string mapPad = Path.GetDirectoryName(_bestandsPad);
                if (!Directory.Exists(mapPad))
                {
                    Directory.CreateDirectory(mapPad);
                }

                string jsonString = JsonSerializer.Serialize(Gebruikers, new JsonSerializerOptions { WriteIndented = true });
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

        private void NieuweGebruiker_Click(object sender, RoutedEventArgs e)
        {
            _geselecteerdeGebruiker = null;
            TxtVoornaam.Clear();
            TxtAchternaam.Clear();
            TxtEmail.Clear();
            TxtGebruikersnaam.Clear();
            DpGeboortedatum.SelectedDate = null;
            TxtWoonplaats.Clear();
            TxtWachtwoord.Clear();
            CmbRol.SelectedIndex = 0;

            LblFormTitel.Text = "Nieuwe Gebruiker Toevoegen";
            BtnToevoegen.Content = "Toevoegen";
            NieuweGebruikerForm.Visibility = Visibility.Visible;
        }

        private void Annuleren_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void Toevoegen_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtVoornaam.Text) || string.IsNullOrWhiteSpace(TxtGebruikersnaam.Text))
            {
                MessageBox.Show("Vul tenminste een voornaam en gebruikersnaam in.", "Melding", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string datumText = DpGeboortedatum.SelectedDate.HasValue ? DpGeboortedatum.SelectedDate.Value.ToString("d-M-yyyy") : string.Empty;
            string rolText = (CmbRol.SelectedItem as ComboBoxItem)?.Content.ToString();

            if (_geselecteerdeGebruiker == null)
            {
                VoorbeeldGebruiker nieuw = new VoorbeeldGebruiker
                {
                    Voornaam = TxtVoornaam.Text,
                    Achternaam = TxtAchternaam.Text,
                    Email = TxtEmail.Text,
                    Gebruikersnaam = TxtGebruikersnaam.Text,
                    Geboortedatum = datumText,
                    Woonplaats = TxtWoonplaats.Text,
                    Rol = rolText,
                    Aangemaakt = DateTime.Now.ToString("d-M-yyyy")
                };
                Gebruikers.Add(nieuw);
            }
            else
            {
                _geselecteerdeGebruiker.Voornaam = TxtVoornaam.Text;
                _geselecteerdeGebruiker.Achternaam = TxtAchternaam.Text;
                _geselecteerdeGebruiker.Email = TxtEmail.Text;
                _geselecteerdeGebruiker.Gebruikersnaam = TxtGebruikersnaam.Text;
                _geselecteerdeGebruiker.Geboortedatum = datumText;
                _geselecteerdeGebruiker.Woonplaats = TxtWoonplaats.Text;
                _geselecteerdeGebruiker.Rol = rolText;
            }

            SaveGebruikers();
            ClearForm();
        }

        private void BewerkGebruiker_Click(object sender, RoutedEventArgs e)
        {
            Button knop = sender as Button;
            if (knop != null)
            {
                _geselecteerdeGebruiker = knop.CommandParameter as VoorbeeldGebruiker;
                if (_geselecteerdeGebruiker != null)
                {
                    TxtVoornaam.Text = _geselecteerdeGebruiker.Voornaam;
                    TxtAchternaam.Text = _geselecteerdeGebruiker.Achternaam;
                    TxtEmail.Text = _geselecteerdeGebruiker.Email;
                    TxtGebruikersnaam.Text = _geselecteerdeGebruiker.Gebruikersnaam;
                    TxtWoonplaats.Text = _geselecteerdeGebruiker.Woonplaats;
                    TxtWachtwoord.Clear();

                    if (DateTime.TryParse(_geselecteerdeGebruiker.Geboortedatum, out DateTime parsedDate))
                    {
                        DpGeboortedatum.SelectedDate = parsedDate;
                    }
                    else
                    {
                        DpGeboortedatum.SelectedDate = null;
                    }

                    for (int i = 0; i < CmbRol.Items.Count; i++)
                    {
                        if ((CmbRol.Items[i] as ComboBoxItem)?.Content.ToString() == _geselecteerdeGebruiker.Rol)
                        {
                            CmbRol.SelectedIndex = i;
                            break;
                        }
                    }

                    LblFormTitel.Text = "Gebruiker Bewerken";
                    BtnToevoegen.Content = "Opslaan";
                    NieuweGebruikerForm.Visibility = Visibility.Visible;
                }
            }
        }

        private void VerwijderGebruiker_Click(object sender, RoutedEventArgs e)
        {
            Button knop = sender as Button;
            if (knop != null)
            {
                VoorbeeldGebruiker item = knop.CommandParameter as VoorbeeldGebruiker;
                if (item != null)
                {
                    if (_geselecteerdeGebruiker == item) ClearForm();
                    Gebruikers.Remove(item);
                    SaveGebruikers();
                }
            }
        }

        private void ClearForm()
        {
            TxtVoornaam.Clear();
            TxtAchternaam.Clear();
            TxtEmail.Clear();
            TxtGebruikersnaam.Clear();
            DpGeboortedatum.SelectedDate = null;
            TxtWoonplaats.Clear();
            TxtWachtwoord.Clear();
            CmbRol.SelectedIndex = 0;
            _geselecteerdeGebruiker = null;
            NieuweGebruikerForm.Visibility = Visibility.Collapsed;
        }

        private void GebruikersPage_Unloaded(object sender, RoutedEventArgs e)
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

    public class VoorbeeldGebruiker : System.ComponentModel.INotifyPropertyChanged
    {
        private string _voornaam;
        private string _achternaam;
        private string _gebruikersnaam;
        private string _email;
        private string _geboortedatum;
        private string _woonplaats;
        private string _rol;
        private string _aangemaakt;

        public string Voornaam { get => _voornaam; set { _voornaam = value; OnPropertyChanged(nameof(Voornaam)); } }
        public string Achternaam { get => _achternaam; set { _achternaam = value; OnPropertyChanged(nameof(Achternaam)); } }
        public string Gebruikersnaam { get => _gebruikersnaam; set { _gebruikersnaam = value; OnPropertyChanged(nameof(Gebruikersnaam)); } }
        public string Email { get => _email; set { _email = value; OnPropertyChanged(nameof(Email)); } }
        public string Geboortedatum { get => _geboortedatum; set { _geboortedatum = value; OnPropertyChanged(nameof(Geboortedatum)); } }
        public string Woonplaats { get => _woonplaats; set { _woonplaats = value; OnPropertyChanged(nameof(Woonplaats)); } }
        public string Rol { get => _rol; set { _rol = value; OnPropertyChanged(nameof(Rol)); } }
        public string Aangemaakt { get => _aangemaakt; set { _aangemaakt = value; OnPropertyChanged(nameof(Aangemaakt)); } }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
    }
}