using MySqlConnector;
using PlotTwist;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace StemwijzerApp.Pages
{
    public partial class ResultatenPage : Page
    {
        public ObservableCollection<VoorbeeldResultaat> Resultaten { get; set; }
        public ObservableCollection<ResultaatGebruikerMock> BeschikbareGebruikers { get; set; }
        public ObservableCollection<VerkiezingMock> BeschikbareVerkiezingen { get; set; }
        public ObservableCollection<StellingInvulMock> HuidigeStellingen { get; set; }
        private VoorbeeldResultaat _geselecteerdtResultaat;
        private DatabaseHandler _dbHandler = new DatabaseHandler();

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
            LoadVerkiezingenData();
            LoadResultaten();

            DataContext = null;
            DataContext = this;
        }

        private void LoadGebruikersData()
        {
            System.Diagnostics.Debug.WriteLine("Plaatje reference check: afbeelding.png");
            BeschikbareGebruikers = new ObservableCollection<ResultaatGebruikerMock>();
            string query = "SELECT id, name, email FROM users WHERE role = 'user'";

            try
            {
                using (MySqlConnection conn = new MySqlConnection("Server=localhost;Database=stemwijzer;Uid=root;Pwd=;"))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    MySqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        BeschikbareGebruikers.Add(new ResultaatGebruikerMock
                        {
                            Id = reader.GetInt32("id"),
                            VolledigeNaam = reader.GetString("name"),
                            Email = reader.GetString("email")
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij laden gebruikers: {ex.Message}");
            }
        }

        private void LoadVerkiezingenData()
        {
            BeschikbareVerkiezingen = new ObservableCollection<VerkiezingMock>();
            string query = "SELECT id, name FROM elections";

            try
            {
                using (MySqlConnection conn = new MySqlConnection("Server=localhost;Database=stemwijzer;Uid=root;Pwd=;"))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    MySqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        BeschikbareVerkiezingen.Add(new VerkiezingMock
                        {
                            Id = reader.GetInt32("id"),
                            Name = reader.GetString("name")
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij laden verkiezingen: {ex.Message}");
            }
        }

        private void LoadResultaten()
        {
            Resultaten = new ObservableCollection<VoorbeeldResultaat>();
            string query = @"SELECT DISTINCT u.id AS user_id, u.name AS user_name, u.email AS user_email, e.id AS election_id, e.name AS election_name 
                             FROM user_answers ua
                             JOIN users u ON ua.user_id = u.id
                             JOIN questions q ON ua.question_id = q.id
                             JOIN questionnaires qn ON q.questionnaire_id = qn.id
                             JOIN elections e ON qn.election_id = e.id";

            try
            {
                using (MySqlConnection conn = new MySqlConnection("Server=localhost;Database=stemwijzer;Uid=root;Pwd=;"))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    MySqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        Resultaten.Add(new VoorbeeldResultaat
                        {
                            UserId = reader.GetInt32("user_id"),
                            ElectionId = reader.GetInt32("election_id"),
                            VolledigeNaam = reader.GetString("user_name"),
                            Email = reader.GetString("user_email"),
                            Verkiezing = reader.GetString("election_name"),
                            Datum = DateTime.Now.ToString("dd-MM-yyyy"),
                            Status = "Ingevuld"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij laden resultaten: {ex.Message}");
            }
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

        private void NieuwResultaat_Click(object sender, RoutedEventArgs e)
        {
            _geselecteerdtResultaat = null;
            if (CmbGebruiker.Items.Count > 0) CmbGebruiker.SelectedIndex = 0;
            if (CmbVerkiezing.Items.Count > 0) CmbVerkiezing.SelectedIndex = 0;

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
            var gekozenVerkiezing = CmbVerkiezing.SelectedItem as VerkiezingMock;

            if (gekozenGebruiker == null || gekozenVerkiezing == null)
            {
                MessageBox.Show("Selecteer een gebruiker en een verkiezing.");
                return;
            }

            if (_geselecteerdtResultaat == null)
            {
                _geselecteerdtResultaat = new VoorbeeldResultaat
                {
                    UserId = gekozenGebruiker.Id,
                    ElectionId = gekozenVerkiezing.Id,
                    VolledigeNaam = gekozenGebruiker.VolledigeNaam,
                    Email = gekozenGebruiker.Email,
                    Verkiezing = gekozenVerkiezing.Name,
                    Datum = DateTime.Now.ToString("dd-MM-yyyy"),
                    Status = "In afwachting"
                };
            }

            LblResultaatGebruiker.Text = gekozenGebruiker.VolledigeNaam;
            LblResultaatEmail.Text = gekozenGebruiker.Email;
            LblResultaatVerkiezing.Text = gekozenVerkiezing.Name;

            LoadStellingenVoorVerkiezing(gekozenVerkiezing.Id, gekozenGebruiker.Id);

            NieuwResultaatForm.Visibility = Visibility.Collapsed;
            MainDataGrid.Visibility = Visibility.Collapsed;
            AntwoordenInvullenForm.Visibility = Visibility.Visible;
        }

        private void LoadStellingenVoorVerkiezing(int electionId, int userId)
        {
            HuidigeStellingen = new ObservableCollection<StellingInvulMock>();
            string query = @"SELECT q.id, q.question, q.weight, ua.answer 
                             FROM questions q
                             JOIN questionnaires qn ON q.questionnaire_id = qn.id
                             LEFT JOIN user_answers ua ON q.id = ua.question_id AND ua.user_id = @userId
                             WHERE qn.election_id = @electionId";

            try
            {
                using (MySqlConnection conn = new MySqlConnection("Server=localhost;Database=stemwijzer;Uid=root;Pwd=;"))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@electionId", electionId);
                    cmd.Parameters.AddWithValue("@userId", userId);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        var stelling = new StellingInvulMock
                        {
                            Id = reader.GetInt32("id"),
                            Question = reader.GetString("question"),
                            Weight = reader.GetInt32("weight"),
                            IdString = "Q" + reader.GetInt32("id")
                        };

                        if (!reader.IsDBNull(reader.GetOrdinal("answer")))
                        {
                            int ans = reader.GetInt32("answer");
                            if (ans == 2) stelling.IsEens = true;
                            else if (ans == 1) stelling.IsNeutraal = true;
                            else if (ans == 0) stelling.IsOneens = true;
                        }
                        else
                        {
                            stelling.IsNeutraal = true;
                        }

                        HuidigeStellingen.Add(stelling);
                    }
                }
                StellingenItemsControl.ItemsSource = HuidigeStellingen;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij laden stellingen: {ex.Message}");
            }
        }

        private void OpslaanAntwoorden_Click(object sender, RoutedEventArgs e)
        {
            if (_geselecteerdtResultaat == null || HuidigeStellingen == null) return;

            string query = @"INSERT INTO user_answers (user_id, question_id, answer) 
                             VALUES (@userId, @questionId, @answer)
                             ON DUPLICATE KEY UPDATE answer = @answer";

            try
            {
                using (MySqlConnection conn = new MySqlConnection("Server=localhost;Database=stemwijzer;Uid=root;Pwd=;"))
                {
                    conn.Open();
                    foreach (var stelling in HuidigeStellingen)
                    {
                        int antwoordWaarde = 1;
                        if (stelling.IsEens) antwoordWaarde = 2;
                        else if (stelling.IsOneens) antwoordWaarde = 0;

                        MySqlCommand cmd = new MySqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@userId", _geselecteerdtResultaat.UserId);
                        cmd.Parameters.AddWithValue("@questionId", stelling.Id);
                        cmd.Parameters.AddWithValue("@answer", antwoordWaarde);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij opslaan antwoorden: {ex.Message}");
            }

            ClearForm();
            LoadResultaten();
            DataContext = null;
            DataContext = this;
        }

        private void BewerkResultaat_Click(object sender, RoutedEventArgs e)
        {
            Button knop = sender as Button;
            if (knop != null)
            {
                _geselecteerdtResultaat = knop.CommandParameter as VoorbeeldResultaat;
                if (_geselecteerdtResultaat != null)
                {
                    LblResultaatGebruiker.Text = _geselecteerdtResultaat.VolledigeNaam;
                    LblResultaatEmail.Text = _geselecteerdtResultaat.Email;
                    LblResultaatVerkiezing.Text = _geselecteerdtResultaat.Verkiezing;

                    LoadStellingenVoorVerkiezing(_geselecteerdtResultaat.ElectionId, _geselecteerdtResultaat.UserId);

                    NieuwResultaatForm.Visibility = Visibility.Collapsed;
                    MainDataGrid.Visibility = Visibility.Collapsed;
                    AntwoordenInvullenForm.Visibility = Visibility.Visible;
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
                    MessageBoxResult res = MessageBox.Show("Wil je alle antwoorden van deze gebruiker voor deze verkiezing verwijderen?", "Bevestigen", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (res == MessageBoxResult.Yes)
                    {
                        string query = @"DELETE ua FROM user_answers ua
                                         JOIN questions q ON ua.question_id = q.id
                                         JOIN questionnaires qn ON q.questionnaire_id = qn.id
                                         WHERE ua.user_id = @userId AND qn.election_id = @electionId";

                        try
                        {
                            using (MySqlConnection conn = new MySqlConnection("Server=localhost;Database=stemwijzer;Uid=root;Pwd=;"))
                            {
                                conn.Open();
                                MySqlCommand cmd = new MySqlCommand(query, conn);
                                cmd.Parameters.AddWithValue("@userId", item.UserId);
                                cmd.Parameters.AddWithValue("@electionId", item.ElectionId);
                                cmd.ExecuteNonQuery();
                            }
                            Resultaten.Remove(item);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Fout bij verwijderen: {ex.Message}");
                        }
                    }
                }
            }
        }

        private void ClearForm()
        {
            if (CmbGebruiker.Items.Count > 0) CmbGebruiker.SelectedIndex = 0;
            if (CmbVerkiezing.Items.Count > 0) CmbVerkiezing.SelectedIndex = 0;
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
                parent?.RaiseEvent(eventArg);
            }
        }
    }

    public class VoorbeeldResultaat : System.ComponentModel.INotifyPropertyChanged
    {
        private string _volledigeNaam, _email, _verkiezing, _datum, _status;
        public int UserId { get; set; }
        public int ElectionId { get; set; }
        public string VolledigeNaam { get => _volledigeNaam; set { _volledigeNaam = value; OnPropertyChanged(nameof(VolledigeNaam)); } }
        public string Email { get => _email; set { _email = value; OnPropertyChanged(nameof(Email)); } }
        public string Verkiezing { get => _verkiezing; set { _verkiezing = value; OnPropertyChanged(nameof(Verkiezing)); } }
        public string Datum { get => _datum; set { _datum = value; OnPropertyChanged(nameof(Datum)); } }
        public string Status { get => _status; set { _status = value; OnPropertyChanged(nameof(Status)); } }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
    }

    public class ResultaatGebruikerMock
    {
        public int Id { get; set; }
        public string VolledigeNaam { get; set; }
        public string Email { get; set; }
    }

    public class VerkiezingMock
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class StellingInvulMock : System.ComponentModel.INotifyPropertyChanged
    {
        private bool _isEens, _isNeutraal, _isOneens;
        public int Id { get; set; }
        public string Question { get; set; }
        public int Weight { get; set; }
        public string IdString { get; set; }
        public string WeightText => $"Gewicht: {Weight}";

        public bool IsEens { get => _isEens; set { _isEens = value; OnPropertyChanged(nameof(IsEens)); } }
        public bool IsNeutraal { get => _isNeutraal; set { _isNeutraal = value; OnPropertyChanged(nameof(IsNeutraal)); } }
        public bool IsOneens { get => _isOneens; set { _isOneens = value; OnPropertyChanged(nameof(IsOneens)); } }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
    }
}