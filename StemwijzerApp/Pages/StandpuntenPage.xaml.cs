using MySqlConnector;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace StemwijzerApp.Pages
{
    /// <summary>
    /// Interaction logic for StandpuntenPage.xaml
    /// </summary>
    public partial class StandpuntenPage : Page
    {
        public ObservableCollection<VoorbeeldStandpunt> Standpunten { get; set; }
        private VoorbeeldStandpunt _geselecteerdStandpunt;
        private DatabaseHandler _dbHandler;
        private readonly string _connectionString = "Server=localhost;Database=stemwijzer;Uid=root;Pwd=;Convert Zero Datetime=True;";

        public StandpuntenPage()
        {
            InitializeComponent();
            _dbHandler = new DatabaseHandler();
            LoadStandpunten();
            DataContext = this;

            this.Unloaded += StandpuntenPage_Unloaded;
        }

        private void LoadStandpunten()
        {
            Standpunten = new ObservableCollection<VoorbeeldStandpunt>();
            string query = "SELECT id, question, category, description FROM questions";

            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Standpunten.Add(new VoorbeeldStandpunt
                                {
                                    Id = reader.GetInt32("id"),
                                    Titel = reader.IsDBNull(reader.GetOrdinal("question")) ? "" : reader.GetString("question"),
                                    Categorie = reader.IsDBNull(reader.GetOrdinal("category")) ? "Algemeen" : reader.GetString("category"),
                                    Beschrijving = reader.IsDBNull(reader.GetOrdinal("description")) ? "" : reader.GetString("description")
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij ophalen standpunten uit database: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (Standpunten.Count == 0)
            {
                InsertBasisData();
            }
        }

        private void InsertBasisData()
        {
            string query1 = "INSERT INTO questions (question, category, description) VALUES ('Meer windmolens bouwen', 'Klimaat', 'Er moeten meer windmolens gebouwd worden voor duurzame energie')";
            string query2 = "INSERT INTO questions (question, category, description) VALUES ('Belastingverlaging middeninkomens', 'Economie', 'De belastingen moeten omlaag voor middeninkomens')";

            _dbHandler.ExecuteQuery(query1);
            _dbHandler.ExecuteQuery(query2);

            LoadStandpunten();
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
                string query = "INSERT INTO questions (question, category, description) VALUES (@titel, @categorie, @beschrijving)";

                try
                {
                    using (MySqlConnection connection = new MySqlConnection(_connectionString))
                    {
                        connection.Open();
                        using (MySqlCommand command = new MySqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@titel", TxtTitel.Text);
                            command.Parameters.AddWithValue("@categorie", TxtCategorie.Text);
                            command.Parameters.AddWithValue("@beschrijving", TxtBeschrijving.Text);
                            command.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Fout bij opslaan: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                string query = "UPDATE questions SET question = @titel, category = @categorie, description = @beschrijving WHERE id = @id";

                try
                {
                    using (MySqlConnection connection = new MySqlConnection(_connectionString))
                    {
                        connection.Open();
                        using (MySqlCommand command = new MySqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@titel", TxtTitel.Text);
                            command.Parameters.AddWithValue("@categorie", TxtCategorie.Text);
                            command.Parameters.AddWithValue("@beschrijving", TxtBeschrijving.Text);
                            command.Parameters.AddWithValue("@id", _geselecteerdStandpunt.Id);
                            command.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Fout bij updaten: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            ClearForm();
            LoadStandpunten();

            DataContext = null;
            DataContext = this;
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

                    string query = "DELETE FROM questions WHERE id = @id";

                    try
                    {
                        using (MySqlConnection connection = new MySqlConnection(_connectionString))
                        {
                            connection.Open();
                            using (MySqlCommand command = new MySqlCommand(query, connection))
                            {
                                command.Parameters.AddWithValue("@id", standpuntGeklikt.Id);
                                command.ExecuteNonQuery();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Fout bij verwijderen: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    LoadStandpunten();

                    DataContext = null;
                    DataContext = this;
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

        private void ClearForm()
        {
            TxtTitel.Clear();
            TxtBeschrijving.Clear();
            TxtCategorie.Clear();
            _geselecteerdStandpunt = null;
            SetFormEditingState(true, isViewing: false);
            NieuwStandpuntForm.Visibility = Visibility.Collapsed;
        }

        private void StandpuntenPage_Unloaded(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }
    }

    public class VoorbeeldStandpunt : System.ComponentModel.INotifyPropertyChanged
    {
        private int _id;
        private string _titel;
        private string _categorie;
        private string _beschrijving;

        public int Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(nameof(Id)); }
        }
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
