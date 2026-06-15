using MySqlConnector;
using PlotTwist;
using System;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace StemwijzerApp.Pages
{
    public partial class PartijenPage : Page
    {
        public ObservableCollection<VoorbeeldPartij> Partijen { get; set; }
        private VoorbeeldPartij _geselecteerdePartij;
        private DatabaseHandler _dbHandler = new DatabaseHandler();

        [DllImport("comdlg32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool ChooseColor(ref CHOOSECOLOR cc);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct CHOOSECOLOR
        {
            public int lStructSize;
            public IntPtr hwndOwner;
            public IntPtr hInstance;
            public int rgbResult;
            public IntPtr lpCustColors;
            public int Flags;
            public IntPtr lCustData;
            public IntPtr lpfnHook;
            public IntPtr lpTemplateName;
        }

        private static int[] customColors = new int[16];

        public PartijenPage()
        {
            InitializeComponent();
            DataContext = this;

            this.Loaded += PartijenPage_Loaded;
            this.Unloaded += PartijenPage_Unloaded;
        }

        private void PartijenPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadPartijen();
            DataContext = null;
            DataContext = this;
        }

        private void LoadPartijen()
        {
            Partijen = new ObservableCollection<VoorbeeldPartij>();
            string query = "SELECT id, name, abbreviation, description, color_hex FROM parties";

            try
            {
                _dbHandler.OpenConnection();
                MySqlCommand command = new MySqlCommand(query, new MySqlConnection("Server=localhost;Database=stemwijzer;Uid=root;Pwd=;Convert Zero Datetime=True;"));
                command.Connection.Open();
                MySqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Partijen.Add(new VoorbeeldPartij
                    {
                        Id = reader.GetInt32("id"),
                        Afkorting = reader.GetString("abbreviation"),
                        Naam = reader.GetString("name"),
                        Beschrijving = reader.IsDBNull(reader.GetOrdinal("description")) ? "" : reader.GetString("description"),
                        Kleur = reader.GetString("color_hex")
                    });
                }
                command.Connection.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij het laden van partijen: {ex.Message}");
            }
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

        private void NieuwePartij_Click(object sender, RoutedEventArgs e)
        {
            _geselecteerdePartij = null;
            TxtNaam.Clear();
            TxtAfkorting.Clear();
            TxtBeschrijving.Clear();
            TxtKleurHex.Text = "#FF6B00";

            SetFieldsEnabled(true);

            LblFormTitel.Text = "Nieuwe Partij Toevoegen";
            BtnToevoegen.Content = "Toevoegen";
            BtnToevoegen.Visibility = Visibility.Visible;
            NieuwePartijForm.Visibility = Visibility.Visible;
        }

        private void Annuleren_Click(object sender, RoutedEventArgs e) => ClearForm();

        private void Toevoegen_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtNaam.Text) || string.IsNullOrWhiteSpace(TxtAfkorting.Text))
            {
                MessageBox.Show("Vul tenminste een naam en afkorting in.");
                return;
            }

            string kleurText = TxtKleurHex.Text;
            if (string.IsNullOrWhiteSpace(kleurText)) kleurText = "#FF6B00";

            string query = string.Empty;

            if (_geselecteerdePartij == null)
            {
                query = "INSERT INTO parties (name, abbreviation, description, color_hex) VALUES (@name, @abbreviation, @description, @color_hex)";
            }
            else
            {
                query = "UPDATE parties SET name = @name, abbreviation = @abbreviation, description = @description, color_hex = @color_hex WHERE id = @id";
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection("Server=localhost;Database=stemwijzer;Uid=root;Pwd=;"))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@name", TxtNaam.Text);
                    cmd.Parameters.AddWithValue("@abbreviation", TxtAfkorting.Text);
                    cmd.Parameters.AddWithValue("@description", TxtBeschrijving.Text);
                    cmd.Parameters.AddWithValue("@color_hex", kleurText);

                    if (_geselecteerdePartij != null)
                    {
                        cmd.Parameters.AddWithValue("@id", _geselecteerdePartij.Id);
                    }

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij opslaan: {ex.Message}");
            }

            ClearForm();
            LoadPartijen();
            DataContext = null;
            DataContext = this;
        }

        private void BekijkPartij_Click(object sender, RoutedEventArgs e)
        {
            Button knop = sender as Button;
            _geselecteerdePartij = knop?.CommandParameter as VoorbeeldPartij;

            if (_geselecteerdePartij != null)
            {
                TxtNaam.Text = _geselecteerdePartij.Naam;
                TxtAfkorting.Text = _geselecteerdePartij.Afkorting;
                TxtBeschrijving.Text = _geselecteerdePartij.Beschrijving;
                TxtKleurHex.Text = _geselecteerdePartij.Kleur;

                SetFieldsEnabled(false);

                LblFormTitel.Text = "Partij Details";
                BtnToevoegen.Visibility = Visibility.Collapsed;
                NieuwePartijForm.Visibility = Visibility.Visible;
            }
        }

        private void BewerkPartij_Click(object sender, RoutedEventArgs e)
        {
            Button knop = sender as Button;
            _geselecteerdePartij = knop?.CommandParameter as VoorbeeldPartij;

            if (_geselecteerdePartij != null)
            {
                TxtNaam.Text = _geselecteerdePartij.Naam;
                TxtAfkorting.Text = _geselecteerdePartij.Afkorting;
                TxtBeschrijving.Text = _geselecteerdePartij.Beschrijving;
                TxtKleurHex.Text = _geselecteerdePartij.Kleur;

                SetFieldsEnabled(true);

                LblFormTitel.Text = "Partij Bewerken";
                BtnToevoegen.Content = "Opslaan";
                BtnToevoegen.Visibility = Visibility.Visible;
                NieuwePartijForm.Visibility = Visibility.Visible;
            }
        }

        private void VerwijderPartij_Click(object sender, RoutedEventArgs e)
        {
            Button knop = sender as Button;
            VoorbeeldPartij item = knop?.CommandParameter as VoorbeeldPartij;

            if (item != null)
            {
                MessageBoxResult result = MessageBox.Show($"Weet je zeker dat je {item.Naam} wilt verwijderen?", "Bevestigen", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    string query = "DELETE FROM parties WHERE id = @id";

                    try
                    {
                        using (MySqlConnection conn = new MySqlConnection("Server=localhost;Database=stemwijzer;Uid=root;Pwd=;"))
                        {
                            conn.Open();
                            MySqlCommand cmd = new MySqlCommand(query, conn);
                            cmd.Parameters.AddWithValue("@id", item.Id);
                            cmd.ExecuteNonQuery();
                        }

                        if (_geselecteerdePartij == item) ClearForm();
                        Partijen.Remove(item);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Fout bij verwijderen: {ex.Message}");
                    }
                }
            }
        }

        private void BdrKleurVoorbeeld_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (TxtKleurHex.IsEnabled == false) return;

            CHOOSECOLOR cc = new CHOOSECOLOR();
            GCHandle handle = GCHandle.Alloc(customColors, GCHandleType.Pinned);

            try
            {
                cc.lStructSize = Marshal.SizeOf(typeof(CHOOSECOLOR));
                cc.hwndOwner = new System.Windows.Interop.WindowInteropHelper(Window.GetWindow(this)).Handle;
                cc.lpCustColors = handle.AddrOfPinnedObject();
                cc.Flags = 0x00000001 | 0x00000002;

                try
                {
                    Color huidig = (Color)ColorConverter.ConvertFromString(TxtKleurHex.Text);
                    cc.rgbResult = (huidig.B << 16) | (huidig.G << 8) | huidig.R;
                }
                catch { }

                if (ChooseColor(ref cc))
                {
                    int r = cc.rgbResult & 0xFF;
                    int g = (cc.rgbResult >> 8) & 0xFF;
                    int b = (cc.rgbResult >> 16) & 0xFF;

                    TxtKleurHex.Text = string.Format("#{0:X2}{1:X2}{2:X2}", r, g, b);
                }
            }
            finally
            {
                handle.Free();
            }
        }

        private void TxtKleurHex_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (BdrKleurVoorbeeld != null && !string.IsNullOrWhiteSpace(TxtKleurHex.Text))
                {
                    var color = (Color)ColorConverter.ConvertFromString(TxtKleurHex.Text);
                    BdrKleurVoorbeeld.Background = new SolidColorBrush(color);
                }
            }
            catch { }
        }

        private void SetFieldsEnabled(bool enabled)
        {
            TxtNaam.IsEnabled = enabled;
            TxtAfkorting.IsEnabled = enabled;
            TxtBeschrijving.IsEnabled = enabled;
            TxtKleurHex.IsEnabled = enabled;
        }

        private void ClearForm()
        {
            TxtNaam.Clear();
            TxtAfkorting.Clear();
            TxtBeschrijving.Clear();
            TxtKleurHex.Text = "#FF6B00";
            _geselecteerdePartij = null;

            SetFieldsEnabled(true);
            NieuwePartijForm.Visibility = Visibility.Collapsed;
        }

        private void PartijenPage_Unloaded(object sender, RoutedEventArgs e) => ClearForm();

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

    public class VoorbeeldPartij : System.ComponentModel.INotifyPropertyChanged
    {
        private int _id;
        private string _naam, _afkorting, _beschrijving, _kleur;

        public int Id { get => _id; set { _id = value; OnPropertyChanged(nameof(Id)); } }
        public string Naam { get => _naam; set { _naam = value; OnPropertyChanged(nameof(Naam)); } }
        public string Afkorting { get => _afkorting; set { _afkorting = value; OnPropertyChanged(nameof(Afkorting)); } }
        public string Beschrijving { get => _beschrijving; set { _beschrijving = value; OnPropertyChanged(nameof(Beschrijving)); } }
        public string Kleur { get => _kleur; set { _kleur = value; OnPropertyChanged(nameof(Kleur)); } }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
    }
}