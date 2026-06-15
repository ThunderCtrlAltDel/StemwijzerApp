using MySqlConnector;
using PlotTwist;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace StemwijzerApp.Pages
{
    public partial class StandpuntenarrangementenPage : Page
    {
        public ObservableCollection<VoorbeeldArrangement> Arrangementen { get; set; }
        public ObservableCollection<SelecteerbaarStandpunt> BeschikbareStandpunten { get; set; }
        public ObservableCollection<VoorbeeldVerkiezingMock> BeschikbareVerkiezingen { get; set; }
        private VoorbeeldArrangement _geselecteerdArrangement;
        private bool _isLaden = false;
        private DatabaseHandler _dbHandler = new DatabaseHandler();

        public StandpuntenarrangementenPage()
        {
            InitializeComponent();
            DataContext = this;

            this.Loaded += StandpuntenarrangementenPage_Loaded;
            this.Unloaded += StandpuntenarrangementenPage_Unloaded;
        }

        private void StandpuntenarrangementenPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadVerkiezingenData();
            LoadStandpuntenData();
            LoadArrangementen();

            DataContext = null;
            DataContext = this;
        }

        private void LoadStandpuntenData()
        {
            BeschikbareStandpunten = new ObservableCollection<SelecteerbaarStandpunt>();
            string query = "SELECT id, question, weight FROM questions";

            try
            {
                using (MySqlConnection conn = new MySqlConnection("Server=localhost;Database=stemwijzer;Uid=root;Pwd=;"))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    MySqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        var nieuwItem = new SelecteerbaarStandpunt
                        {
                            Id = reader.GetInt32("id"),
                            Titel = reader.GetString("question"),
                            Gewicht = reader.GetInt32("weight")
                        };
                        nieuwItem.SelectionChanged += (sender, e) => UpdateAantalGeselecteerdText();
                        BeschikbareStandpunten.Add(nieuwItem);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij laden standpunten: {ex.Message}");
            }
            LstStandpunten.ItemsSource = BeschikbareStandpunten;
        }

        private void LoadVerkiezingenData()
        {
            BeschikbareVerkiezingen = new ObservableCollection<VoorbeeldVerkiezingMock>();
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
                        string naam = reader.GetString("name");
                        if (!string.IsNullOrWhiteSpace(naam))
                        {
                            BeschikbareVerkiezingen.Add(new VoorbeeldVerkiezingMock
                            {
                                Id = reader.GetInt32("id"),
                                Naam = naam
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij laden verkiezingen: {ex.Message}");
            }
        }

        private void LoadArrangementen()
        {
            Arrangementen = new ObservableCollection<VoorbeeldArrangement>();
            string query = @"SELECT qn.id, qn.title, qn.created_at, e.name AS election_name, e.id AS election_id
                             FROM questionnaires qn
                             LEFT JOIN elections e ON qn.election_id = e.id";

            try
            {
                _dbHandler.OpenConnection();
                using (MySqlConnection conn = new MySqlConnection("Server=localhost;Database=stemwijzer;Uid=root;Pwd=;Convert Zero Datetime=True;"))
                {
                    conn.Open();
                    MySqlCommand command = new MySqlCommand(query, conn);
                    MySqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        var arrangement = new VoorbeeldArrangement
                        {
                            Id = reader.GetInt32("id"),
                            Naam = reader.GetString("title"),
                            VerkiezingId = reader.IsDBNull(reader.GetOrdinal("election_id")) ? 0 : reader.GetInt32("election_id"),
                            Verkiezing = reader.IsDBNull(reader.GetOrdinal("election_name")) ? "Geen koppeling" : reader.GetString("election_name"),
                            Aangemaakt = reader.GetDateTime("created_at").ToString("d-M-yyyy"),
                            GeselecteerdeIds = new List<int>()
                        };
                        Arrangementen.Add(arrangement);
                    }
                }

                foreach (var arr in Arrangementen)
                {
                    string subQuery = "SELECT id FROM questions WHERE questionnaire_id = @qnId";
                    using (MySqlConnection conn2 = new MySqlConnection("Server=localhost;Database=stemwijzer;Uid=root;Pwd=;"))
                    {
                        conn2.Open();
                        MySqlCommand cmd2 = new MySqlCommand(subQuery, conn2);
                        cmd2.Parameters.AddWithValue("@qnId", arr.Id);
                        MySqlDataReader subReader = cmd2.ExecuteReader();
                        while (subReader.Read())
                        {
                            arr.GeselecteerdeIds.Add(subReader.GetInt32("id"));
                        }
                    }
                    arr.TriggerCountUpdate();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij laden arrangementen: {ex.Message}");
            }
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

        private void NieuwArrangement_Click(object sender, RoutedEventArgs e)
        {
            _isLaden = true;
            _geselecteerdArrangement = null;
            TxtNaam.Clear();
            if (CmbVerkiezing.Items.Count > 0) CmbVerkiezing.SelectedIndex = 0;
            foreach (var s in BeschikbareStandpunten) s.IsGeselecteerd = false;
            _isLaden = false;
            UpdateAantalGeselecteerdText();
            LblFormTitel.Text = "Nieuw Arrangement Toevoegen";
            BtnToevoegen.Content = "Toevoegen";
            NieuwArrangementForm.Visibility = Visibility.Visible;
        }

        private void Annuleren_Click(object sender, RoutedEventArgs e) => ClearForm();

        private void Toevoegen_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtNaam.Text))
            {
                MessageBox.Show("Vul tenminste een naam in.");
                return;
            }

            var gekozenVerkiezing = CmbVerkiezing.SelectedItem as VoorbeeldVerkiezingMock;
            if (gekozenVerkiezing == null) return;

            string query = string.Empty;
            long arrangementId = 0;

            if (_geselecteerdArrangement == null)
            {
                query = "INSERT INTO questionnaires (title, election_id) VALUES (@title, @electionId)";
            }
            else
            {
                query = "UPDATE questionnaires SET title = @title, election_id = @electionId WHERE id = @id";
                arrangementId = _geselecteerdArrangement.Id;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection("Server=localhost;Database=stemwijzer;Uid=root;Pwd=;"))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@title", TxtNaam.Text);
                    cmd.Parameters.AddWithValue("@electionId", gekozenVerkiezing.Id);
                    if (_geselecteerdArrangement != null) cmd.Parameters.AddWithValue("@id", arrangementId);

                    cmd.ExecuteNonQuery();

                    if (_geselecteerdArrangement == null)
                    {
                        arrangementId = cmd.LastInsertedId;
                    }
                }

                using (MySqlConnection conn = new MySqlConnection("Server=localhost;Database=stemwijzer;Uid=root;Pwd=;"))
                {
                    conn.Open();
                    string resetQuery = "UPDATE questions SET questionnaire_id = 0 WHERE questionnaire_id = @qnId";
                    MySqlCommand resetCmd = new MySqlCommand(resetQuery, conn);
                    resetCmd.Parameters.AddWithValue("@qnId", arrangementId);
                    resetCmd.ExecuteNonQuery();

                    string updateStellingQuery = "UPDATE questions SET questionnaire_id = @qnId WHERE id = @stellingId";
                    foreach (var s in BeschikbareStandpunten.Where(x => x.IsGeselecteerd))
                    {
                        MySqlCommand updateCmd = new MySqlCommand(updateStellingQuery, conn);
                        updateCmd.Parameters.AddWithValue("@qnId", arrangementId);
                        updateCmd.Parameters.AddWithValue("@stellingId", s.Id);
                        updateCmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij opslaan: {ex.Message}");
            }

            ClearForm();
            LoadArrangementen();
            DataContext = null;
            DataContext = this;
        }

        private void BewerkArrangement_Click(object sender, RoutedEventArgs e)
        {
            _isLaden = true;
            Button knop = sender as Button;
            _geselecteerdArrangement = knop?.CommandParameter as VoorbeeldArrangement;

            if (_geselecteerdArrangement != null)
            {
                TxtNaam.Text = _geselecteerdArrangement.Naam;

                for (int i = 0; i < CmbVerkiezing.Items.Count; i++)
                {
                    if ((CmbVerkiezing.Items[i] as VoorbeeldVerkiezingMock)?.Id == _geselecteerdArrangement.VerkiezingId)
                    {
                        CmbVerkiezing.SelectedIndex = i;
                        break;
                    }
                }

                foreach (var s in BeschikbareStandpunten)
                {
                    s.IsGeselecteerd = _geselecteerdArrangement.GeselecteerdeIds != null && _geselecteerdArrangement.GeselecteerdeIds.Contains(s.Id);
                }

                _isLaden = false;
                UpdateAantalGeselecteerdText();
                LblFormTitel.Text = "Arrangement Bewerken";
                BtnToevoegen.Content = "Opslaan";
                NieuwArrangementForm.Visibility = Visibility.Visible;
            }
        }

        private void VerwijderArrangement_Click(object sender, RoutedEventArgs e)
        {
            Button knop = sender as Button;
            VoorbeeldArrangement item = knop?.CommandParameter as VoorbeeldArrangement;
            if (item != null)
            {
                MessageBoxResult result = MessageBox.Show("Weet je zeker dat je dit arrangement wilt verwijderen?", "Bevestigen", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    string query = "DELETE FROM questionnaires WHERE id = @id";
                    try
                    {
                        using (MySqlConnection conn = new MySqlConnection("Server=localhost;Database=stemwijzer;Uid=root;Pwd=;"))
                        {
                            conn.Open();
                            MySqlCommand cmd = new MySqlCommand(query, conn);
                            cmd.Parameters.AddWithValue("@id", item.Id);
                            cmd.ExecuteNonQuery();
                        }
                        if (_geselecteerdArrangement == item) ClearForm();
                        Arrangementen.Remove(item);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Fout bij verwijderen: {ex.Message}");
                    }
                }
            }
        }

        private void UpdateAantalGeselecteerdText()
        {
            if (_isLaden || BeschikbareStandpunten == null) return;
            int aantal = BeschikbareStandpunten.Count(s => s.IsGeselecteerd);
            LblAantalGeselecteerd.Text = $"{aantal} standpunt(en) geselecteerd";
        }

        private void ClearForm()
        {
            _isLaden = true;
            TxtNaam.Clear();
            if (CmbVerkiezing.Items.Count > 0) CmbVerkiezing.SelectedIndex = 0;
            foreach (var s in BeschikbareStandpunten) s.IsGeselecteerd = false;
            _geselecteerdArrangement = null;
            _isLaden = false;
            UpdateAantalGeselecteerdText();
            NieuwArrangementForm.Visibility = Visibility.Collapsed;
        }

        private void StandpuntenarrangementenPage_Unloaded(object sender, RoutedEventArgs e) => ClearForm();

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

    public class VoorbeeldArrangement : System.ComponentModel.INotifyPropertyChanged
    {
        private string _naam, _verkiezing, _aangemaakt;
        private List<int> _geselecteerdeIds = new List<int>();

        public int Id { get; set; }
        public int VerkiezingId { get; set; }
        public string Naam { get => _naam; set { _naam = value; OnPropertyChanged(nameof(Naam)); } }
        public string Verkiezing { get => _verkiezing; set { _verkiezing = value; OnPropertyChanged(nameof(Verkiezing)); } }
        public string Aangemaakt { get => _aangemaakt; set { _aangemaakt = value; OnPropertyChanged(nameof(Aangemaakt)); } }
        public List<int> GeselecteerdeIds { get => _geselecteerdeIds; set { _geselecteerdeIds = value; OnPropertyChanged(nameof(GeselecteerdeIds)); OnPropertyChanged(nameof(AantalText)); } }
        public string AantalText => $"{GeselecteerdeIds?.Count ?? 0} standpunten";

        public void TriggerCountUpdate()
        {
            OnPropertyChanged(nameof(AantalText));
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
    }

    public class SelecteerbaarStandpunt : System.ComponentModel.INotifyPropertyChanged
    {
        private bool _isGeselecteerd;
        public int Id { get; set; }
        public string Titel { get; set; }
        public int Gewicht { get; set; }
        public string GewichtText => $"Gewicht: {Gewicht}";
        public bool IsGeselecteerd { get => _isGeselecteerd; set { _isGeselecteerd = value; OnPropertyChanged(nameof(IsGeselecteerd)); SelectionChanged?.Invoke(this, EventArgs.Empty); } }
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        public event EventHandler SelectionChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
    }

    public class VoorbeeldVerkiezingMock
    {
        public int Id { get; set; }
        public string Naam { get; set; }
    }
}