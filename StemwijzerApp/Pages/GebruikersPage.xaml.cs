using MySqlConnector;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace StemwijzerApp.Pages
{
    public partial class GebruikersPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private const string ConnString = "Server=localhost;Database=stemwijzer;Uid=root;Pwd=;";
        public ObservableCollection<VoorbeeldGebruiker> Gebruikers { get; set; } = new ObservableCollection<VoorbeeldGebruiker>();

        public GebruikersPage()
        {
            InitializeComponent();
            DataContext = this;
            this.Loaded += (s, e) => LoadGebruikers();
        }

        private void LoadGebruikers()
        {
            Gebruikers.Clear();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(ConnString))
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand("SELECT id, name, username, email, role, birthdate FROM users", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DateTime? birth = reader.IsDBNull(reader.GetOrdinal("birthdate")) ? (DateTime?)null : reader.GetDateTime("birthdate");
                            string info = "-";
                            if (birth.HasValue)
                            {
                                int age = DateTime.Today.Year - birth.Value.Year;
                                if (birth.Value.Date > DateTime.Today.AddYears(-age)) age--;
                                info = $"{birth.Value:dd-MM-yyyy} ({age})";
                            }
                            Gebruikers.Add(new VoorbeeldGebruiker
                            {
                                Id = reader.GetInt32("id"),
                                VolledigeNaam = reader.IsDBNull(reader.GetOrdinal("name")) ? "-" : reader.GetString("name"),
                                Username = reader.IsDBNull(reader.GetOrdinal("username")) ? "-" : reader.GetString("username"),
                                Email = reader.IsDBNull(reader.GetOrdinal("email")) ? "-" : reader.GetString("email"),
                                Rol = reader.IsDBNull(reader.GetOrdinal("role")) ? "user" : reader.GetString("role"),
                                GeboorteInfo = info
                            });
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(TxtVoornaam.Text) || string.IsNullOrWhiteSpace(TxtAchternaam.Text) ||
                string.IsNullOrWhiteSpace(TxtEmail.Text) || string.IsNullOrWhiteSpace(TxtGebruikersnaam.Text) ||
                string.IsNullOrWhiteSpace(TxtWoonplaats.Text) || string.IsNullOrEmpty(PbWachtwoord.Password) ||
                !DpGeboortedatum.SelectedDate.HasValue)
            {
                MessageBox.Show("Alle velden zijn verplicht.");
                return false;
            }
            if (!Regex.IsMatch(TxtEmail.Text, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MessageBox.Show("Ongeldig e-mailadres.");
                return false;
            }
            if (TxtGebruikersnaam.Text.Length < 4)
            {
                MessageBox.Show("Gebruikersnaam moet minimaal 4 tekens bevatten.");
                return false;
            }
            if (PbWachtwoord.Password.Length < 6)
            {
                MessageBox.Show("Wachtwoord moet minimaal 6 tekens bevatten.");
                return false;
            }
            if (DpGeboortedatum.SelectedDate.Value > DateTime.Today.AddYears(-18))
            {
                MessageBox.Show("De gebruiker moet minimaal 18 jaar oud zijn.");
                return false;
            }
            return true;
        }

        private void Toevoegen_Click(object sender, RoutedEventArgs e)
        {
            if (!IsValid()) return;
            try
            {
                using (MySqlConnection conn = new MySqlConnection(ConnString))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand("INSERT INTO users (name, username, email, password, role, birthdate, city) VALUES (@n, @u, @e, @p, @r, @b, @c)", conn);
                    cmd.Parameters.AddWithValue("@n", $"{TxtVoornaam.Text.Trim()} {TxtAchternaam.Text.Trim()}");
                    cmd.Parameters.AddWithValue("@u", TxtGebruikersnaam.Text.Trim());
                    cmd.Parameters.AddWithValue("@e", TxtEmail.Text.Trim());
                    cmd.Parameters.AddWithValue("@p", PbWachtwoord.Password);
                    cmd.Parameters.AddWithValue("@r", (CmbRol.SelectedItem as ComboBoxItem)?.Content.ToString());
                    cmd.Parameters.AddWithValue("@b", DpGeboortedatum.SelectedDate.Value);
                    cmd.Parameters.AddWithValue("@c", TxtWoonplaats.Text.Trim());
                    cmd.ExecuteNonQuery();
                }
                LoadGebruikers();
                NieuweGebruikerForm.Visibility = Visibility.Collapsed;
                ClearForm();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void VerwijderGebruiker_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button)?.CommandParameter as VoorbeeldGebruiker;
            if (item == null || MessageBox.Show("Verwijderen?", "Bevestigen", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            try
            {
                using (MySqlConnection conn = new MySqlConnection(ConnString))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand("DELETE FROM users WHERE id = @id", conn);
                    cmd.Parameters.AddWithValue("@id", item.Id);
                    cmd.ExecuteNonQuery();
                }
                LoadGebruikers();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void ClearForm() { TxtVoornaam.Clear(); TxtAchternaam.Clear(); TxtEmail.Clear(); TxtGebruikersnaam.Clear(); TxtWoonplaats.Clear(); PbWachtwoord.Clear(); DpGeboortedatum.SelectedDate = null; }
        private void NieuweGebruiker_Click(object sender, RoutedEventArgs e) => NieuweGebruikerForm.Visibility = Visibility.Visible;
        private void Annuleren_Click(object sender, RoutedEventArgs e) => NieuweGebruikerForm.Visibility = Visibility.Collapsed;
    }

    public class VoorbeeldGebruiker
    {
        public int Id { get; set; }
        public string VolledigeNaam { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Rol { get; set; }
        public string GeboorteInfo { get; set; }
    }
}
