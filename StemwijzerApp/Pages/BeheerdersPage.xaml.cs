using MySqlConnector;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StemwijzerApp.Pages
{
    public partial class BeheerdersPage : Page
    {
        public ObservableCollection<VoorbeeldBeheerder> Beheerders { get; set; }
        private VoorbeeldBeheerder _geselecteerdeBeheerder;
        private DatabaseHandler _dbHandler = new DatabaseHandler();

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
            Beheerders = new ObservableCollection<VoorbeeldBeheerder>();
            string query = "SELECT id, name, email, role, created_at FROM users WHERE role = 'admin'";

            try
            {
                _dbHandler.OpenConnection();
                MySqlCommand command = new MySqlCommand(query, new MySqlConnection("Server=localhost;Database=stemwijzer;Uid=root;Pwd=;Convert Zero Datetime=True;"));
                command.Connection.Open();
                MySqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Beheerders.Add(new VoorbeeldBeheerder
                    {
                        Id = reader.GetInt32("id"),
                        Naam = reader.GetString("name"),
                        Email = reader.GetString("email"),
                        Rol = reader.GetString("role"),
                        Aangemaakt = reader.GetDateTime("created_at").ToString("d-M-yyyy")
                    });
                }
                command.Connection.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij het laden van beheerders: {ex.Message}");
            }
        }

        private void Toevoegen_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtNaam.Text) || string.IsNullOrWhiteSpace(TxtEmail.Text))
            {
                MessageBox.Show("Vul tenminste een naam en e-mailadres in.");
                return;
            }

            string rolText = "admin";
            string wachtwoord = TxtWachtwoordZichtbaar.Visibility == Visibility.Visible ? TxtWachtwoordZichtbaar.Text : TxtWachtwoord.Password;

            if (_geselecteerdeBeheerder == null)
            {
                if (string.IsNullOrWhiteSpace(wachtwoord))
                {
                    MessageBox.Show("Wachtwoord is verplicht bij een nieuwe beheerder.");
                    return;
                }

                string query = "INSERT INTO users (name, email, password, role) VALUES (@name, @email, @password, @role)";

                try
                {
                    using (MySqlConnection conn = new MySqlConnection("Server=localhost;Database=stemwijzer;Uid=root;Pwd=;"))
                    {
                        conn.Open();
                        MySqlCommand cmd = new MySqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@name", TxtNaam.Text);
                        cmd.Parameters.AddWithValue("@email", TxtEmail.Text);
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
            else
            {
                string query;
                bool wachtwoordAanpassen = !string.IsNullOrWhiteSpace(wachtwoord);

                if (wachtwoordAanpassen)
                {
                    query = "UPDATE users SET name = @name, email = @email, role = @role, password = @password WHERE id = @id";
                }
                else
                {
                    query = "UPDATE users SET name = @name, email = @email, role = @role WHERE id = @id";
                }

                try
                {
                    using (MySqlConnection conn = new MySqlConnection("Server=localhost;Database=stemwijzer;Uid=root;Pwd=;"))
                    {
                        conn.Open();
                        MySqlCommand cmd = new MySqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@id", _geselecteerdeBeheerder.Id);
                        cmd.Parameters.AddWithValue("@name", TxtNaam.Text);
                        cmd.Parameters.AddWithValue("@email", TxtEmail.Text);
                        cmd.Parameters.AddWithValue("@role", rolText);
                        if (wachtwoordAanpassen) cmd.Parameters.AddWithValue("@password", wachtwoord);

                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Fout bij bijwerken: {ex.Message}");
                }
            }

            ClearForm();
            LoadBeheerders();
            DataContext = null;
            DataContext = this;
        }

        private void VerwijderBeheerder_Click(object sender, RoutedEventArgs e)
        {
            Button knop = sender as Button;
            VoorbeeldBeheerder item = knop?.CommandParameter as VoorbeeldBeheerder;

            if (item != null)
            {
                MessageBoxResult result = MessageBox.Show($"Weet je zeker dat je {item.Naam} wilt verwijderen?", "Bevestigen", MessageBoxButton.YesNo, MessageBoxImage.Warning);

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

                        if (_geselecteerdeBeheerder == item) ClearForm();
                        Beheerders.Remove(item);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Fout bij verwijderen: {ex.Message}");
                    }
                }
            }
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
                TxtWachtwoordZichtbaar.Clear();
                TxtWachtwoord.Visibility = Visibility.Visible;
                TxtWachtwoordZichtbaar.Visibility = Visibility.Collapsed;
                BtnWachtwoordZichtbaar.Content = "👁️";

                CmbRol.SelectedIndex = 0;
                for (int i = 0; i < CmbRol.Items.Count; i++)
                {
                    if ((CmbRol.Items[i] as ComboBoxItem)?.Content.ToString().Equals(_geselecteerdeBeheerder.Rol, StringComparison.OrdinalIgnoreCase) == true)
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

        private void NieuweBeheerder_Click(object sender, RoutedEventArgs e)
        {
            _geselecteerdeBeheerder = null;
            TxtNaam.Clear();
            TxtEmail.Clear();

            TxtWachtwoord.Clear();
            TxtWachtwoordZichtbaar.Clear();
            TxtWachtwoord.Visibility = Visibility.Visible;
            TxtWachtwoordZichtbaar.Visibility = Visibility.Collapsed;
            BtnWachtwoordZichtbaar.Content = "👁️";

            CmbRol.SelectedIndex = 0;

            LblWachtwoordTitel.Text = "Wachtwoord";
            LblFormTitel.Text = "Nieuwe Beheerder Toevoegen";
            BtnToevoegen.Content = "Toevoegen";
            NieuweBeheerderForm.Visibility = Visibility.Visible;
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

        private void Annuleren_Click(object sender, RoutedEventArgs e) => ClearForm();

        private void ClearForm()
        {
            TxtNaam.Clear();
            TxtEmail.Clear();

            TxtWachtwoord.Clear();
            TxtWachtwoordZichtbaar.Clear();
            TxtWachtwoord.Visibility = Visibility.Visible;
            TxtWachtwoordZichtbaar.Visibility = Visibility.Collapsed;
            BtnWachtwoordZichtbaar.Content = "👁️";

            CmbRol.SelectedIndex = 0;
            _geselecteerdeBeheerder = null;
            NieuweBeheerderForm.Visibility = Visibility.Collapsed;
        }

        private void BeheerdersPage_Unloaded(object sender, RoutedEventArgs e) => ClearForm();

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

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
                parent?.RaiseEvent(eventArg);
            }
        }
    }

    public class VoorbeeldBeheerder : System.ComponentModel.INotifyPropertyChanged
    {
        private int _id;
        private string _naam, _email, _rol, _aangemaakt;

        public int Id { get => _id; set { _id = value; OnPropertyChanged(nameof(Id)); } }
        public string Naam { get => _naam; set { _naam = value; OnPropertyChanged(nameof(Naam)); } }
        public string Email { get => _email; set { _email = value; OnPropertyChanged(nameof(Email)); } }
        public string Rol { get => _rol; set { _rol = value; OnPropertyChanged(nameof(Rol)); } }
        public string Aangemaakt { get => _aangemaakt; set { _aangemaakt = value; OnPropertyChanged(nameof(Aangemaakt)); } }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
    }
}