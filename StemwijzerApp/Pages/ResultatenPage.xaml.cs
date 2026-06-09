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
    public partial class ResultatenPage : Page
    {
        public ObservableCollection<VoorbeeldResultaat> Resultaten { get; set; }
        public ObservableCollection<ResultaatGebruikerMock> BeschikbareGebruikers { get; set; }
        private VoorbeeldResultaat _geselecteerdtResultaat;

        private readonly string _resultatenPad = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "StemwijzerApp",
            "resultaten.json"
        );

        private readonly string _gebruikersPad = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "StemwijzerApp",
            "gebruikers.json"
        );

        public ResultatenPage()
        {
            InitializeComponent();
            DataContext = this;

            this.Loaded += ResultatenPage_Loaded;
            this.Unloaded += ResultatenPage_Unloaded;
        }

        private void ResultatenPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadGebruikersData();
            LoadResultaten();

            DataContext = null;
            DataContext = this;
        }

        private void LoadGebruikersData()
        {
            BeschikbareGebruikers = new ObservableCollection<ResultaatGebruikerMock>();
            try
            {
                if (File.Exists(_gebruikersPad))
                {
                    string jsonString = File.ReadAllText(_gebruikersPad);
                    var geladenGebruikers = JsonSerializer.Deserialize<List<ResultaatGebruikerMock>>(jsonString);
                    if (geladenGebruikers != null)
                    {
                        foreach (var g in geladenGebruikers)
                        {
                            BeschikbareGebruikers.Add(g);
                        }
                    }
                }
            }
            catch { }

            if (BeschikbareGebruikers.Count == 0)
            {
                BeschikbareGebruikers.Add(new ResultaatGebruikerMock { Voornaam = "Jan", Achternaam = "Jansen", Gebruikersnaam = "gebruiker1", Email = "gebruiker@voorbeeld.nl" });
            }
        }

        private void LoadResultaten()
        {
            try
            {
                if (File.Exists(_resultatenPad))
                {
                    string jsonString = File.ReadAllText(_resultatenPad);
                    Resultaten = JsonSerializer.Deserialize<ObservableCollection<VoorbeeldResultaat>>(jsonString);
                }
            }
            catch { }

            if (Resultaten == null || Resultaten.Count == 0)
            {
                Resultaten = new ObservableCollection<VoorbeeldResultaat>
                {
                    new VoorbeeldResultaat
                    {
                        VolledigeNaam = "Jan Jansen (gebruiker1)",
                        Verkiezing = "Tweede Kamerverkiezingen 2025",
                        Datum = "09-06-2026",
                        Status = "Ingevuld"
                    }
                };
                SaveResultaten();
            }
        }

        private void SaveResultaten()
        {
            try
            {
                string mapPad = Path.GetDirectoryName(_resultatenPad);
                if (!Directory.Exists(mapPad)) Directory.CreateDirectory(mapPad);
                string jsonString = JsonSerializer.Serialize(Resultaten, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_resultatenPad, jsonString);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij het opslaan: {ex.Message}");
            }
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

        private void NieuwResultaat_Click(object sender, RoutedEventArgs e)
        {
            LoadGebruikersData();
            _geselecteerdtResultaat = null;
            CmbGebruiker.SelectedIndex = 0;
            TxtVerkiezing.Clear();

            LblFormTitel.Text = "Nieuw Resultaat Toevoegen";
            BtnToevoegen.Content = "Toevoegen en Antwoorden Invullen";

            AntwoordenInvullenForm.Visibility = Visibility.Collapsed;
            NieuwResultaatForm.Visibility = Visibility.Visible;
            MainDataGrid.Visibility = Visibility.Visible;
        }

        private void Annuleren_Click(object sender, RoutedEventArgs e) => ClearForm();

        private void Toevoegen_Click(object sender, RoutedEventArgs e)
        {
            var gekozenGebruiker = CmbGebruiker.SelectedItem as ResultaatGebruikerMock;
            if (gekozenGebruiker == null)
            {
                MessageBox.Show("Selecteer een gebruiker.");
                return;
            }

            string verkiezingText = TxtVerkiezing.Text;
            if (string.IsNullOrWhiteSpace(verkiezingText))
            {
                MessageBox.Show("Vul een verkiezing in.");
                return;
            }

            string naamText = gekozenGebruiker.Voornaam + " " + gekozenGebruiker.Achternaam + " (" + gekozenGebruiker.Gebruikersnaam + ")";

            if (_geselecteerdtResultaat == null)
            {
                _geselecteerdtResultaat = new VoorbeeldResultaat
                {
                    VolledigeNaam = naamText,
                    Verkiezing = verkiezingText,
                    Datum = DateTime.Now.ToString("dd-MM-yyyy"),
                    Status = "In afwachting"
                };
                Resultaten.Add(_geselecteerdtResultaat);
            }
            else
            {
                _geselecteerdtResultaat.VolledigeNaam = naamText;
                _geselecteerdtResultaat.Verkiezing = verkiezingText;
            }

            SaveResultaten();

            LblResultaatGebruiker.Text = gekozenGebruiker.Voornaam + " " + gekozenGebruiker.Achternaam + " (" + gekozenGebruiker.Gebruikersnaam + ")";
            LblResultaatEmail.Text = string.IsNullOrEmpty(gekozenGebruiker.Email) ? "gebruiker@voorbeeld.nl" : gekozenGebruiker.Email;
            LblResultaatVerkiezing.Text = verkiezingText;

            NieuwResultaatForm.Visibility = Visibility.Collapsed;
            MainDataGrid.Visibility = Visibility.Collapsed;
            AntwoordenInvullenForm.Visibility = Visibility.Visible;
        }

        private void OpslaanAntwoorden_Click(object sender, RoutedEventArgs e)
        {
            if (_geselecteerdtResultaat != null)
            {
                _geselecteerdtResultaat.Status = "Ingevuld";
                SaveResultaten();
            }
            ClearForm();
            ResultatenPage_Loaded(null, null);
        }

        private void BewerkResultaat_Click(object sender, RoutedEventArgs e)
        {
            LoadGebruikersData();
            Button knop = sender as Button;
            if (knop != null)
            {
                _geselecteerdtResultaat = knop.CommandParameter as VoorbeeldResultaat;
                if (_geselecteerdtResultaat != null)
                {
                    TxtVerkiezing.Text = _geselecteerdtResultaat.Verkiezing;
                    CmbGebruiker.SelectedIndex = 0;
                    for (int i = 0; i < CmbGebruiker.Items.Count; i++)
                    {
                        var g = CmbGebruiker.Items[i] as ResultaatGebruikerMock;
                        if (g != null)
                        {
                            string matchNaam = g.Voornaam + " " + g.Achternaam + " (" + g.Gebruikersnaam + ")";
                            if (matchNaam == _geselecteerdtResultaat.VolledigeNaam)
                            {
                                CmbGebruiker.SelectedIndex = i;
                                break;
                            }
                        }
                    }

                    LblFormTitel.Text = "Resultaat Bewerken";
                    BtnToevoegen.Content = "Opslaan";
                    AntwoordenInvullenForm.Visibility = Visibility.Collapsed;
                    NieuwResultaatForm.Visibility = Visibility.Visible;
                }
            }
        }

        private void VerwijderResultaat_Click(object sender, RoutedEventArgs e)
        {
            Button knop = sender as Button;
            if (knop != null)
            {
                VoorbeeldResultaat item = knop.CommandParameter as VoorbeeldResultaat;
                if (item != null)
                {
                    if (_geselecteerdtResultaat == item) ClearForm();
                    Resultaten.Remove(item);
                    SaveResultaten();
                }
            }
        }

        private void ClearForm()
        {
            CmbGebruiker.SelectedIndex = 0;
            TxtVerkiezing.Clear();
            _geselecteerdtResultaat = null;
            NieuwResultaatForm.Visibility = Visibility.Collapsed;
            AntwoordenInvullenForm.Visibility = Visibility.Collapsed;
            MainDataGrid.Visibility = Visibility.Visible;
        }

        private void ResultatenPage_Unloaded(object sender, RoutedEventArgs e) => ClearForm();

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

    public class VoorbeeldResultaat : System.ComponentModel.INotifyPropertyChanged
    {
        private string _volledigeNaam, _verkiezing, _datum, _status;

        public string VolledigeNaam { get => _volledigeNaam; set { _volledigeNaam = value; OnPropertyChanged(nameof(VolledigeNaam)); } }
        public string Verkiezing { get => _verkiezing; set { _verkiezing = value; OnPropertyChanged(nameof(Verkiezing)); } }
        public string Datum { get => _datum; set { _datum = value; OnPropertyChanged(nameof(Datum)); } }
        public string Status { get => _status; set { _status = value; OnPropertyChanged(nameof(Status)); } }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
    }

    public class ResultaatGebruikerMock
    {
        public string Voornaam { get; set; }
        public string Achternaam { get; set; }
        public string Gebruikersnaam { get; set; }
        public string Email { get; set; }
    }
}