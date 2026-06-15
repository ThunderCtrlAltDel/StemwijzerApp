using MySqlConnector;
using PlotTwist;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace StemwijzerApp.Pages
{
    public partial class GebruikersPage : Page
    {
        public ObservableCollection<VoorbeeldGebruiker> Gebruikers { get; set; }
        private VoorbeeldGebruiker _geselecteerdeGebruiker;
        private DatabaseHandler _dbHandler = new DatabaseHandler();

        public GebruikersPage()
        {
            InitializeComponent();
            DataContext = this;

            this.Loaded += GebruikersPage_Loaded;
            this.Unloaded += GebruikersPage_Unloaded;
        }

        private void GebruikersPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadGebruikers();
            DataContext = null;
            DataContext = this;
        }

        private void LoadGebruikers()
        {
            Gebruikers = new ObservableCollection<VoorbeeldGebruiker>();
            string query = "SELECT id, name, email, birthdate, city, role, created_at FROM users";

            try
            {
                _dbHandler.OpenConnection();
                MySqlCommand command = new MySqlCommand(query, new MySqlConnection("Server=localhost;Database=stemwijzer;Uid=root;Pwd=;Convert Zero Datetime=True;"));
                command.Connection.Open();
                MySqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    DateTime? geboortedatum = reader.IsDBNull(reader.GetOrdinal("birthdate")) ? (DateTime?)null : reader.GetDateTime("birthdate");
                    string leeftijdHaakjes = "()";

                    if (geboortedatum.HasValue)
                    {
                        int leeftijd = DateTime.Now.Year - geboortedatum.Value.Year;
                        if (DateTime.Now.DayOfYear < geboortedatum.Value.DayOfYear)
                        {
                            leeftijd--;
                        }
                        leeftijdHaakjes = $"({leeftijd})";
                    }

                    Gebruikers.Add(new VoorbeeldGebruiker
                    {
                        Id = reader.GetInt32("id"),
                        VolledigeNaam = reader.GetString("name"),
                        Email = reader.GetString("email"),
                        Geboortedatum = geboortedatum,
                        GeboortedatumFormaat = geboortedatum.HasValue ? geboortedatum.Value.ToString("d-M-yyyy") : "",
                        LeeftijdHaakjes = leeftijdHaakjes,
                        Woonplaats = reader.IsDBNull(reader.GetOrdinal("city")) ? "" : reader.GetString("city"),
                        Rol = reader.GetString("role"),
                        Aangemaakt = reader.GetDateTime("created_at").ToString("d-M-yyyy")
                    });
                }
                command.Connection.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij het laden van gebruikers: {ex.Message}");
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
            DpGeboortedatum.SelectedDate = null;
            TxtWoonplaats.Clear();

            TxtWachtwoord.Clear();
            TxtWachtwoordZichtbaar.Clear();
            TxtWachtwoord.Visibility = Visibility.Visible;
            TxtWachtwoordZichtbaar.Visibility = Visibility.Collapsed;
            BtnWachtwoordZichtbaar.Content = "👁️";

            CmbRol.SelectedIndex = 0;
            SetFieldsEnabled(true);

            LblWachtwoordTitel.Text = "Wachtwoord";
            LblFormTitel.Text = "Nieuwe Gebruiker Toevoegen";
            BtnToevoegen.Content = "Toevoegen";
            BtnToevoegen.Visibility = Visibility.Visible;
            NieuweGebruikerForm.Visibility = Visibility.Visible;
        }

        private void Annuleren_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void Toevoegen_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtVoornaam.Text) || string.IsNullOrWhiteSpace(TxtEmail.Text))
            {
                MessageBox.Show("Vul tenminste een voornaam en e-mailadres in.", "Melding", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string volledigeNaam = $"{TxtVoornaam.Text} {TxtAchternaam.Text}".Trim();
            string rolText = (CmbRol.SelectedItem as ComboBoxItem)?.Content.ToString().ToLower();
            string wachtwoord = TxtWachtwoordZichtbaar.Visibility == Visibility.Visible ? TxtWachtwoordZichtbaar.Text : TxtWachtwoord.Password;

            if (_geselecteerdeGebruiker == null)
            {
                if (string.IsNullOrWhiteSpace(wachtwoord))
                {
                    MessageBox.Show("Wachtwoord is verplicht bij een nieuwe gebruiker.");
                    return;
                }

                string query = "INSERT INTO users (name, email, birthdate, city, password, role) VALUES (@name, @email, @birthdate, @city, @password, @role)";

                try
                {
                    using (MySqlConnection conn = new MySqlConnection("Server=localhost;Database=stemwijzer;Uid=root;Pwd=;"))
                    {
                        conn.Open();
                        MySqlCommand cmd = new MySqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@name", volledigeNaam);
                        cmd.Parameters.AddWithValue("@email", TxtEmail.Text);
                        cmd.Parameters.AddWithValue("@birthdate", DpGeboortedatum.SelectedDate.HasValue ? (object)DpGeboortedatum.SelectedDate.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@city", TxtWoonplaats.Text);
                        cmd.Parameters.AddWithValue("@password", wachtwoord);
                        cmd.Parameters.AddWithValue("@role", rolText);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Fout bij toevoegen: {ex.Message}");
                }
            }

            ClearForm();
            LoadGebruikers();
            DataContext = null;
            DataContext = this;
        }

        private void BekijkGebruiker_Click(object sender, RoutedEventArgs e)
        {
            Button knop = sender as Button;
            _geselecteerdeGebruiker = knop?.CommandParameter as VoorbeeldGebruiker;

            if (_geselecteerdeGebruiker != null)
            {
                string[] namen = _geselecteerdeGebruiker.VolledigeNaam.Split(new[] { ' ' }, 2);
                TxtVoornaam.Text = namen.Length > 0 ? namen[0] : string.Empty;
                TxtAchternaam.Text = namen.Length > 1 ? namen[1] : string.Empty;
                TxtEmail.Text = _geselecteerdeGebruiker.Email;
                DpGeboortedatum.SelectedDate = _geselecteerdeGebruiker.Geboortedatum;
                TxtWoonplaats.Text = _geselecteerdeGebruiker.Woonplaats;

                TxtWachtwoord.Clear();
                TxtWachtwoordZichtbaar.Clear();
                TxtWachtwoord.Visibility = Visibility.Visible;
                TxtWachtwoordZichtbaar.Visibility = Visibility.Collapsed;
                BtnWachtwoordZichtbaar.Content = "👁️";

                for (int i = 0; i < CmbRol.Items.Count; i++)
                {
                    if ((CmbRol.Items[i] as ComboBoxItem)?.Content.ToString().Equals(_geselecteerdeGebruiker.Rol, StringComparison.OrdinalIgnoreCase) == true)
                    {
                        CmbRol.SelectedIndex = i;
                        break;
                    }
                }

                SetFieldsEnabled(false);

                LblFormTitel.Text = "Gebruiker Details";
                BtnToevoegen.Visibility = Visibility.Collapsed;
                NieuweGebruikerForm.Visibility = Visibility.Visible;
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
                    MessageBoxResult result = MessageBox.Show($"Weet je zeker dat je {item.VolledigeNaam} wilt verwijderen?", "Bevestigen", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        string query = "DELETE FROM users WHERE id = @id";

                        try
                        {
                            using (MySqlConnection conn = new MySqlConnection("Server=localhost;Database=stemwijzer;Uid=root;Pwd=;"))
                            {
                                conn.Open();
                                MySqlCommand cmd = new MySqlCommand(query, conn);
                                cmd.Parameters.AddWithValue("@id", item.Id);
                                cmd.ExecuteNonQuery();
                            }

                            if (_geselecteerdeGebruiker == item) ClearForm();
                            Gebruikers.Remove(item);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Fout bij verwijderen: {ex.Message}");
                        }
                    }
                }
            }
        }

        private void BtnWachtwoordZichtbaar_Click(object sender, RoutedEventArgs e)
        {
            if (TxtWachtwoord.Visibility == Visibility.Visible)
            {
                TxtWachtwoordZichtbaar.Text = TxtWachtwoord.Password;
                TxtWachtwoord.Visibility = Visibility.Collapsed;
                TxtWachtwoordZichtbaar.Visibility = Visibility.Visible;
                BtnWachtwoordZichtbaar.Content = "🔒";
            }
            else
            {
                TxtWachtwoord.Password = TxtWachtwoordZichtbaar.Text;
                TxtWachtwoord.Visibility = Visibility.Visible;
                TxtWachtwoordZichtbaar.Visibility = Visibility.Collapsed;
                BtnWachtwoordZichtbaar.Content = "👁️";
            }
        }

        private void SetFieldsEnabled(bool enabled)
        {
            TxtVoornaam.IsEnabled = enabled;
            TxtAchternaam.IsEnabled = enabled;
            TxtEmail.IsEnabled = enabled;
            DpGeboortedatum.IsEnabled = enabled;
            TxtWoonplaats.IsEnabled = enabled;
            CmbRol.IsEnabled = enabled;
            TxtWachtwoord.IsEnabled = enabled;
            TxtWachtwoordZichtbaar.IsEnabled = enabled;
            BtnWachtwoordZichtbaar.IsEnabled = enabled;
        }

        private void ClearForm()
        {
            TxtVoornaam.Clear();
            TxtAchternaam.Clear();
            TxtEmail.Clear();
            DpGeboortedatum.SelectedDate = null;
            TxtWoonplaats.Clear();

            TxtWachtwoord.Clear();
            TxtWachtwoordZichtbaar.Clear();
            TxtWachtwoord.Visibility = Visibility.Visible;
            TxtWachtwoordZichtbaar.Visibility = Visibility.Collapsed;
            BtnWachtwoordZichtbaar.Content = "👁️";

            CmbRol.SelectedIndex = 0;
            _geselecteerdeGebruiker = null;
            SetFieldsEnabled(true);
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
                parent?.RaiseEvent(eventArg);
            }
        }
    }

    public class VoorbeeldGebruiker : System.ComponentModel.INotifyPropertyChanged
    {
        private int _id;
        private string _volledigeNaam;
        private string _email;
        private DateTime? _geboortedatum;
        private string _geboortedatumFormaat;
        private string _leeftijdHaakjes;
        private string _woonplaats;
        private string _rol;
        private string _aangemaakt;

        public int Id { get => _id; set { _id = value; OnPropertyChanged(nameof(Id)); } }
        public string VolledigeNaam { get => _volledigeNaam; set { _volledigeNaam = value; OnPropertyChanged(nameof(VolledigeNaam)); } }
        public string Email { get => _email; set { _email = value; OnPropertyChanged(nameof(Email)); } }
        public DateTime? Geboortedatum { get => _geboortedatum; set { _geboortedatum = value; OnPropertyChanged(nameof(Geboortedatum)); } }
        public string GeboortedatumFormaat { get => _geboortedatumFormaat; set { _geboortedatumFormaat = value; OnPropertyChanged(nameof(GeboortedatumFormaat)); } }
        public string LeeftijdHaakjes { get => _leeftijdHaakjes; set { _leeftijdHaakjes = value; OnPropertyChanged(nameof(LeeftijdHaakjes)); } }
        public string Woonplaats { get => _woonplaats; set { _woonplaats = value; OnPropertyChanged(nameof(Woonplaats)); } }
        public string Rol { get => _rol; set { _rol = value; OnPropertyChanged(nameof(Rol)); } }
        public string Aangemaakt { get => _aangemaakt; set { _aangemaakt = value; OnPropertyChanged(nameof(Aangemaakt)); } }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
    }
}