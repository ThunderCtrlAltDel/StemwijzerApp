using MySqlConnector;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace StemwijzerApp.Pages
{
    /// <summary>
    /// Interaction logic for VerkiezingenPage.xaml
    /// </summary>
    public partial class VerkiezingenPage : Page
    {
        public ObservableCollection<VoorbeeldVerkiezing> Verkiezingen { get; set; }
        private VoorbeeldVerkiezing _geselecteerdeVerkiezing;
        private DatabaseHandler _dbHandler = new DatabaseHandler();

        public VerkiezingenPage()
        {
            InitializeComponent();
            DataContext = this;

            this.Loaded += VerkiezingenPage_Loaded;
            this.Unloaded += VerkiezingenPage_Unloaded;
        }

        private void VerkiezingenPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadVerkiezingen();
            DataContext = null;
            DataContext = this;
        }

        private void LoadVerkiezingen()
        {
            Verkiezingen = new ObservableCollection<VoorbeeldVerkiezing>();
            string query = "SELECT id, name, date, description FROM elections";

            try
            {
                _dbHandler.OpenConnection();
                MySqlCommand command = new MySqlCommand(query, new MySqlConnection("Server=localhost;Database=stemwijzer;Uid=root;Pwd=;Convert Zero Datetime=True;"));
                command.Connection.Open();
                MySqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    string rawDescription = reader.IsDBNull(reader.GetOrdinal("description")) ? "" : reader.GetString("description");
                    string type = "Landelijk";
                    string beschrijving = rawDescription;

                    if (rawDescription.StartsWith("[") && rawDescription.Contains("]"))
                    {
                        int sluitIndex = rawDescription.IndexOf("]");
                        type = rawDescription.Substring(1, sluitIndex - 1);
                        beschrijving = rawDescription.Substring(sluitIndex + 1).Trim();
                    }

                    Verkiezingen.Add(new VoorbeeldVerkiezing
                    {
                        Id = reader.GetInt32("id"),
                        Naam = reader.GetString("name"),
                        Datum = reader.GetDateTime("date").ToString("dd-MM-yyyy"),
                        Type = type,
                        Beschrijving = beschrijving
                    });
                }
                command.Connection.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij het laden van verkiezingen: {ex.Message}");
            }
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void NieuweVerkiezing_Click(object sender, RoutedEventArgs e)
        {
            _geselecteerdeVerkiezing = null;
            TxtNaam.Clear();
            DpDatum.SelectedDate = null;
            CmbType.SelectedIndex = 0;
            TxtBeschrijving.Clear();

            LblFormTitel.Text = "Nieuwe Verkiezing Toevoegen";
            BtnToevoegen.Content = "Toevoegen";
            NieuweVerkiezingForm.Visibility = Visibility.Visible;
        }

        private void Annuleren_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void Toevoegen_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtNaam.Text) || !DpDatum.SelectedDate.HasValue)
            {
                MessageBox.Show("Vul tenminste een naam en datum in.", "Melding", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string typeTag = (CmbType.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Landelijk";
            string gecombineerdeBeschrijving = $"[{typeTag}] {TxtBeschrijving.Text.Trim()}";
            string query = string.Empty;

            if (_geselecteerdeVerkiezing == null)
            {
                query = "INSERT INTO elections (name, date, description) VALUES (@name, @date, @description)";
            }
            else
            {
                query = "UPDATE elections SET name = @name, date = @date, description = @description WHERE id = @id";
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection("Server=localhost;Database=stemwijzer;Uid=root;Pwd=;"))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@name", TxtNaam.Text.Trim());
                    cmd.Parameters.AddWithValue("@date", DpDatum.SelectedDate.Value);
                    cmd.Parameters.AddWithValue("@description", gecombineerdeBeschrijving);

                    if (_geselecteerdeVerkiezing != null)
                    {
                        cmd.Parameters.AddWithValue("@id", _geselecteerdeVerkiezing.Id);
                    }

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij opslaan verkiezing: {ex.Message}");
            }

            ClearForm();
            LoadVerkiezingen();
            DataContext = null;
            DataContext = this;
        }

        private void BewerkVerkiezing_Click(object sender, RoutedEventArgs e)
        {
            Button knop = sender as Button;
            if (knop != null)
            {
                _geselecteerdeVerkiezing = knop.CommandParameter as VoorbeeldVerkiezing;
                if (_geselecteerdeVerkiezing != null)
                {
                    TxtNaam.Text = _geselecteerdeVerkiezing.Naam;
                    TxtBeschrijving.Text = _geselecteerdeVerkiezing.Beschrijving;

                    if (DateTime.TryParse(_geselecteerdeVerkiezing.Datum, out DateTime parsedDate))
                    {
                        DpDatum.SelectedDate = parsedDate;
                    }
                    else
                    {
                        DpDatum.SelectedDate = null;
                    }

                    CmbType.SelectedIndex = 0;
                    for (int i = 0; i < CmbType.Items.Count; i++)
                    {
                        if ((CmbType.Items[i] as ComboBoxItem)?.Content.ToString().Equals(_geselecteerdeVerkiezing.Type, StringComparison.OrdinalIgnoreCase) == true)
                        {
                            CmbType.SelectedIndex = i;
                            break;
                        }
                    }

                    LblFormTitel.Text = "Verkiezing Bewerken";
                    BtnToevoegen.Content = "Opslaan";
                    NieuweVerkiezingForm.Visibility = Visibility.Visible;
                }
            }
        }

        private void VerwijderVerkiezing_Click(object sender, RoutedEventArgs e)
        {
            Button knop = sender as Button;
            if (knop != null)
            {
                VoorbeeldVerkiezing item = knop.CommandParameter as VoorbeeldVerkiezing;
                if (item != null)
                {
                    MessageBoxResult result = MessageBox.Show($"Weet je zeker dat je {item.Naam} wilt verwijderen?", "Bevestigen", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.Yes)
                    {
                        string query = "DELETE FROM elections WHERE id = @id";
                        try
                        {
                            using (MySqlConnection conn = new MySqlConnection("Server=localhost;Database=stemwijzer;Uid=root;Pwd=;"))
                            {
                                conn.Open();
                                MySqlCommand cmd = new MySqlCommand(query, conn);
                                cmd.Parameters.AddWithValue("@id", item.Id);
                                cmd.ExecuteNonQuery();
                            }
                            if (_geselecteerdeVerkiezing == item) ClearForm();
                            Verkiezingen.Remove(item);
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
            TxtNaam.Clear();
            DpDatum.SelectedDate = null;
            CmbType.SelectedIndex = 0;
            TxtBeschrijving.Clear();
            _geselecteerdeVerkiezing = null;
            NieuweVerkiezingForm.Visibility = Visibility.Collapsed;
        }

        private void VerkiezingenPage_Unloaded(object sender, RoutedEventArgs e)
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

    public class VoorbeeldVerkiezing : System.ComponentModel.INotifyPropertyChanged
    {
        private int _id;
        private string _naam;
        private string _datum;
        private string _type;
        private string _beschrijving;

        public int Id { get => _id; set { _id = value; OnPropertyChanged(nameof(Id)); } }
        public string Naam { get => _naam; set { _naam = value; OnPropertyChanged(nameof(Naam)); } }
        public string Datum { get => _datum; set { _datum = value; OnPropertyChanged(nameof(Datum)); } }
        public string Type { get => _type; set { _type = value; OnPropertyChanged(nameof(Type)); } }
        public string Beschrijving { get => _beschrijving; set { _beschrijving = value; OnPropertyChanged(nameof(Beschrijving)); } }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
    }
}
