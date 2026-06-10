using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StemwijzerApp.Pages
{
    public partial class BeheerdersPage : Page
    {
        public ObservableCollection<VoorbeeldBeheerder> Beheerders { get; set; }
        private VoorbeeldBeheerder _geselecteerdeBeheerder;

        private readonly string _bestandsPad = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "StemwijzerApp",
            "beheerders.json"
        );

        public BeheerdersPage()
        {
            InitializeComponent();
            DataContext = this;

            this.Loaded += BeheerdersPage_Loaded;
            this.Unloaded += BeheerdersPage_Unloaded;
        }

        private void BeheerdersPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadBeheerders();
            DataContext = null;
            DataContext = this;
        }

        private void LoadBeheerders()
        {
            try
            {
                if (File.Exists(_bestandsPad))
                {
                    string jsonString = File.ReadAllText(_bestandsPad);
                    if (!string.IsNullOrWhiteSpace(jsonString) && jsonString.Trim() != "[]")
                    {
                        Beheerders = JsonSerializer.Deserialize<ObservableCollection<VoorbeeldBeheerder>>(jsonString);
                    }
                }
            }
            catch
            {
                Beheerders = null;
            }

            if (Beheerders == null || Beheerders.Count == 0)
            {
                Beheerders = new ObservableCollection<VoorbeeldBeheerder>
                {
                    new VoorbeeldBeheerder
                    {
                        Naam = "Hoofd Beheerder",
                        Email = "admin@stemwijzer.nl",
                        Rol = "Hoofdbeheerder",
                        Aangemaakt = "1-1-2024"
                    }
                };
                SaveBeheerders();
            }
        }

        private void SaveBeheerders()
        {
            try
            {
                string mapPad = Path.GetDirectoryName(_bestandsPad);
                if (!Directory.Exists(mapPad)) Directory.CreateDirectory(mapPad);
                string jsonString = JsonSerializer.Serialize(Beheerders, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_bestandsPad, jsonString);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij het opslaan: {ex.Message}");
            }
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

        private void NieuweBeheerder_Click(object sender, RoutedEventArgs e)
        {
            _geselecteerdeBeheerder = null;
            TxtNaam.Clear();
            TxtEmail.Clear();
            TxtWachtwoord.Clear();
            CmbRol.SelectedIndex = 0;

            LblWachtwoordTitel.Text = "Wachtwoord";
            LblFormTitel.Text = "Nieuwe Beheerder Toevoegen";
            BtnToevoegen.Content = "Toevoegen";
            NieuweBeheerderForm.Visibility = Visibility.Visible;
        }

        private void Annuleren_Click(object sender, RoutedEventArgs e) => ClearForm();

        private void Toevoegen_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtNaam.Text) || string.IsNullOrWhiteSpace(TxtEmail.Text))
            {
                MessageBox.Show("Vul tenminste een naam en e-mailadres in.");
                return;
            }

            string rolText = (CmbRol.SelectedItem as ComboBoxItem)?.Content.ToString();

            if (_geselecteerdeBeheerder == null)
            {
                Beheerders.Add(new VoorbeeldBeheerder
                {
                    Naam = TxtNaam.Text,
                    Email = TxtEmail.Text,
                    Rol = rolText,
                    Aangemaakt = DateTime.Now.ToString("d-M-yyyy")
                });
            }
            else
            {
                _geselecteerdeBeheerder.Naam = TxtNaam.Text;
                _geselecteerdeBeheerder.Email = TxtEmail.Text;
                _geselecteerdeBeheerder.Rol = rolText;
            }

            SaveBeheerders();
            ClearForm();
            BeheerdersPage_Loaded(null, null);
        }

        private void BewerkBeheerder_Click(object sender, RoutedEventArgs e)
        {
            Button knop = sender as Button;
            _geselecteerdeBeheerder = knop?.CommandParameter as VoorbeeldBeheerder;

            if (_geselecteerdeBeheerder != null)
            {
                TxtNaam.Text = _geselecteerdeBeheerder.Naam;
                TxtEmail.Text = _geselecteerdeBeheerder.Email;
                TxtWachtwoord.Clear();

                CmbRol.SelectedIndex = 0;
                for (int i = 0; i < CmbRol.Items.Count; i++)
                {
                    if ((CmbRol.Items[i] as ComboBoxItem)?.Content.ToString() == _geselecteerdeBeheerder.Rol)
                    {
                        CmbRol.SelectedIndex = i;
                        break;
                    }
                }

                LblWachtwoordTitel.Text = "Wachtwoord (laat leeg om niet te wijzigen)";
                LblFormTitel.Text = "Beheerder Bewerken";
                BtnToevoegen.Content = "Opslaan";
                NieuweBeheerderForm.Visibility = Visibility.Visible;
            }
        }

        private void VerwijderBeheerder_Click(object sender, RoutedEventArgs e)
        {
            Button knop = sender as Button;
            VoorbeeldBeheerder item = knop?.CommandParameter as VoorbeeldBeheerder;
            if (item != null)
            {
                if (_geselecteerdeBeheerder == item) ClearForm();
                Beheerders.Remove(item);
                SaveBeheerders();
            }
        }

        private void ClearForm()
        {
            TxtNaam.Clear();
            TxtEmail.Clear();
            TxtWachtwoord.Clear();
            CmbRol.SelectedIndex = 0;
            _geselecteerdeBeheerder = null;
            NieuweBeheerderForm.Visibility = Visibility.Collapsed;
        }

        private void BeheerdersPage_Unloaded(object sender, RoutedEventArgs e) => ClearForm();

        private void DataGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = true;
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
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

    public class VoorbeeldBeheerder : System.ComponentModel.INotifyPropertyChanged
    {
        private string _naam, _email, _rol, _aangemaakt;

        public string Naam { get => _naam; set { _naam = value; OnPropertyChanged(nameof(Naam)); } }
        public string Email { get => _email; set { _email = value; OnPropertyChanged(nameof(Email)); } }
        public string Rol { get => _rol; set { _rol = value; OnPropertyChanged(nameof(Rol)); } }
        public string Aangemaakt { get => _aangemaakt; set { _aangemaakt = value; OnPropertyChanged(nameof(Aangemaakt)); } }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
    }
}