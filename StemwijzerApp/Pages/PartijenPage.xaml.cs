using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StemwijzerApp.Pages
{
    public partial class PartijenPage : Page, System.ComponentModel.INotifyPropertyChanged
    {
        public ObservableCollection<VoorbeeldPartij> Partijen { get; set; }
        public ObservableCollection<PartijStellingMock> HuidigeStandpunten { get; set; }
        private VoorbeeldPartij _geselecteerdePartij;
        private DatabaseHandler _dbHandler = new DatabaseHandler();

        [DllImport("comdlg32.dll", EntryPoint = "ChooseColorW", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool ChooseColor(ref CHOOSECOLOR lpcc);

        [StructLayout(LayoutKind.Sequential)]
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
            public string lpszTemplateName;
        }

        private static int[] customColors = new int[16];

        private bool _areStandpuntenEditable = true;
        public bool AreStandpuntenEditable
        {
            get => _areStandpuntenEditable;
            set
            {
                _areStandpuntenEditable = value;
                OnPropertyChanged(nameof(AreStandpuntenEditable));
            }
        }

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
            string query = "SELECT id, name, abbreviation, color_hex, description FROM parties";

            try
            {
                _dbHandler.OpenConnection();
                using (MySqlConnection conn = new MySqlConnection("Server=localhost;Database=stemwijzer;Uid=root;Pwd=;"))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        string hex = reader.IsDBNull(reader.GetOrdinal("color_hex")) ? "#FF6B00" : reader.GetString("color_hex");
                        Brush brush = Brushes.OrangeRed;
                        try { brush = (Brush)new BrushConverter().ConvertFromString(hex); } catch { }

                        Partijen.Add(new VoorbeeldPartij
                        {
                            Id = reader.GetInt32("id"),
                            Naam = reader.GetString("name"),
                            Afkorting = reader.GetString("abbreviation"),
                            KleurHex = hex,
                            KleurBrush = brush,
                            Beschrijving = reader.IsDBNull(reader.GetOrdinal("description")) ? "" : reader.GetString("description")
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij het laden van partijen: {ex.Message}");
            }
        }

        private void LoadStandpuntenVoorPartij(int partyId)
        {
            HuidigeStandpunten = new ObservableCollection<PartijStellingMock>();
            string query = @"SELECT q.id, q.question, pa.answer, pa.explanation 
                             FROM questions q
                             LEFT JOIN party_answers pa ON q.id = pa.question_id AND pa.party_id = @partyId";

            try
            {
                using (MySqlConnection conn = new MySqlConnection("Server=localhost;Database=stemwijzer;Uid=root;Pwd=;"))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@partyId", partyId);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        string rawQuestion = reader.GetString("question");
                        string categorie = "Algemeen";
                        string titel = rawQuestion;

                        if (rawQuestion.StartsWith("[") && rawQuestion.Contains("]"))
                        {
                            int sluitIndex = rawQuestion.IndexOf("]");
                            categorie = rawQuestion.Substring(1, sluitIndex - 1);
                            titel = rawQuestion.Substring(sluitIndex + 1).Trim();
                        }

                        var stelling = new PartijStellingMock
                        {
                            Id = reader.GetInt32("id"),
                            Titel = titel,
                            Categorie = categorie,
                            Beschrijving = "In hoeverre is de partij het eens met dit standpunt?",
                            IdString = "P" + reader.GetInt32("id"),
                            Toelichting = reader.IsDBNull(reader.GetOrdinal("explanation")) ? "" : reader.GetString("explanation")
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

                        HuidigeStandpunten.Add(stelling);
                    }
                }
                StandpuntenItemsControl.ItemsSource = HuidigeStandpunten;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij laden standpunten: {ex.Message}");
            }
        }

        private void NieuwePartij_Click(object sender, RoutedEventArgs e)
        {
            _geselecteerdePartij = null;
            TxtNaam.Clear();
            TxtAfkorting.Clear();
            TxtKleur.Text = "#FF6B00";
            TxtBeschrijving.Clear();

            LblFormTitel.Text = "Nieuwe Partij Toevoegen";
            BtnToevoegen.Content = "Toevoegen";

            PartijStandpuntenForm.Visibility = Visibility.Collapsed;
            NieuwePartijForm.Visibility = Visibility.Visible;
            MainDataGrid.Visibility = Visibility.Visible;
        }

        private void Toevoegen_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtNaam.Text) || string.IsNullOrWhiteSpace(TxtAfkorting.Text))
            {
                MessageBox.Show("Vul tenminste een naam en afkorting in.");
                return;
            }

            string query = string.Empty;
            if (_geselecteerdePartij == null)
                query = "INSERT INTO parties (name, abbreviation, color_hex, description) VALUES (@name, @abbrev, @color, @desc)";
            else
                query = "UPDATE parties SET name = @name, abbreviation = @abbrev, color_hex = @color, description = @desc WHERE id = @id";

            try
            {
                using (MySqlConnection conn = new MySqlConnection("Server=localhost;Database=stemwijzer;Uid=root;Pwd=;"))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@name", TxtNaam.Text.Trim());
                    cmd.Parameters.AddWithValue("@abbrev", TxtAfkorting.Text.Trim().ToUpper());
                    cmd.Parameters.AddWithValue("@color", TxtKleur.Text.Trim());
                    cmd.Parameters.AddWithValue("@desc", TxtBeschrijving.Text.Trim());
                    if (_geselecteerdePartij != null) cmd.Parameters.AddWithValue("@id", _geselecteerdePartij.Id);

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij opslaan partijgegevens: {ex.Message}");
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
                VulPartijKopGegevens(_geselecteerdePartij);
                LoadStandpuntenVoorPartij(_geselecteerdePartij.Id);

                TxtStandpuntenTitel.Text = $"Standpunten van {_geselecteerdePartij.Afkorting} (Alleen Lezen)";
                BtnOpslaanStandpunten.Visibility = Visibility.Collapsed;
                BtnSluitenStandpunten.Visibility = Visibility.Visible;
                AreStandpuntenEditable = false;

                NieuwePartijForm.Visibility = Visibility.Collapsed;
                MainDataGrid.Visibility = Visibility.Collapsed;
                PartijStandpuntenForm.Visibility = Visibility.Visible;
            }
        }

        private void BewerkPartij_Click(object sender, RoutedEventArgs e)
        {
            Button knop = sender as Button;
            _geselecteerdePartij = knop?.CommandParameter as VoorbeeldPartij;

            if (_geselecteerdePartij != null)
            {
                VulPartijKopGegevens(_geselecteerdePartij);
                LoadStandpuntenVoorPartij(_geselecteerdePartij.Id);

                TxtStandpuntenTitel.Text = $"Standpunten van {_geselecteerdePartij.Afkorting}";
                BtnOpslaanStandpunten.Visibility = Visibility.Visible;
                BtnSluitenStandpunten.Visibility = Visibility.Collapsed;
                AreStandpuntenEditable = true;

                TxtNaam.Text = _geselecteerdePartij.Naam;
                TxtAfkorting.Text = _geselecteerdePartij.Afkorting;
                TxtKleur.Text = _geselecteerdePartij.KleurHex;
                TxtBeschrijving.Text = _geselecteerdePartij.Beschrijving;
                LblFormTitel.Text = "Partij Basisgegevens Wijzigen";
                BtnToevoegen.Content = "Opslaan";

                NieuwePartijForm.Visibility = Visibility.Visible;
                MainDataGrid.Visibility = Visibility.Collapsed;
                PartijStandpuntenForm.Visibility = Visibility.Visible;
            }
        }

        private void OpslaanStandpunten_Click(object sender, RoutedEventArgs e)
        {
            if (_geselecteerdePartij == null || HuidigeStandpunten == null) return;

            string query = @"INSERT INTO party_answers (party_id, question_id, answer, explanation) 
                             VALUES (@partyId, @questionId, @answer, @explanation)
                             ON DUPLICATE KEY UPDATE answer = @answer, explanation = @explanation";

            try
            {
                using (MySqlConnection conn = new MySqlConnection("Server=localhost;Database=stemwijzer;Uid=root;Pwd=;"))
                {
                    conn.Open();
                    foreach (var stelling in HuidigeStandpunten)
                    {
                        int antwoordWaarde = 1;
                        if (stelling.IsEens) antwoordWaarde = 2;
                        else if (stelling.IsOneens) antwoordWaarde = 0;

                        MySqlCommand cmd = new MySqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@partyId", _geselecteerdePartij.Id);
                        cmd.Parameters.AddWithValue("@questionId", stelling.Id);
                        cmd.Parameters.AddWithValue("@answer", antwoordWaarde);
                        cmd.Parameters.AddWithValue("@explanation", stelling.Toelichting.Trim());
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij opslaan standpunten: {ex.Message}");
            }

            ClearForm();
            LoadPartijen();
            DataContext = null;
            DataContext = this;
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

        private void KleurKiezer_Click(object sender, RoutedEventArgs e)
        {
            CHOOSECOLOR cc = new CHOOSECOLOR();
            IntPtr allocatedColors = Marshal.AllocHGlobal(customColors.Length * sizeof(int));
            Marshal.Copy(customColors, 0, allocatedColors, customColors.Length);

            cc.lStructSize = Marshal.SizeOf(typeof(CHOOSECOLOR));
            cc.hwndOwner = new System.Windows.Interop.WindowInteropHelper(Window.GetWindow(this)).Handle;
            cc.lpCustColors = allocatedColors;
            cc.Flags = 0x00000001 | 0x00000002;

            try
            {
                Color huidigeKleur = (Color)ColorConverter.ConvertFromString(TxtKleur.Text.Trim());
                cc.rgbResult = (huidigeKleur.B << 16) | (huidigeKleur.G << 8) | huidigeKleur.R;
            }
            catch
            {
                cc.rgbResult = 0x00006BFF;
            }

            if (ChooseColor(ref cc))
            {
                int r = cc.rgbResult & 0xFF;
                int g = (cc.rgbResult >> 8) & 0xFF;
                int b = (cc.rgbResult >> 16) & 0xFF;
                TxtKleur.Text = $"#{r:X2}{g:X2}{b:X2}";
            }

            Marshal.Copy(allocatedColors, customColors, 0, customColors.Length);
            Marshal.FreeHGlobal(allocatedColors);
        }

        private void TxtKleur_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (BordKleurVoorbeeld == null) return;
            try
            {
                var brush = (Brush)new BrushConverter().ConvertFromString(TxtKleur.Text.Trim());
                BordKleurVoorbeeld.Background = brush;
            }
            catch
            {
                BordKleurVoorbeeld.Background = Brushes.Transparent;
            }
        }

        private void VulPartijKopGegevens(VoorbeeldPartij partij)
        {
            TxtBadgeAfkorting.Text = partij.Afkorting;
            TxtKopPartijNaam.Text = partij.Naam;
            TxtKopPartijBeschrijving.Text = string.IsNullOrWhiteSpace(partij.Beschrijving) ? "Geen omschrijving beschikbaar." : partij.Beschrijving;
            BordPartijKleurBadge.Background = partij.KleurBrush;
        }

        private void Annuleren_Click(object sender, RoutedEventArgs e) => ClearForm();

        private void ClearForm()
        {
            TxtNaam.Clear();
            TxtAfkorting.Clear();
            TxtKleur.Text = "#FF6B00";
            TxtBeschrijving.Clear();
            _geselecteerdePartij = null;
            AreStandpuntenEditable = true;

            NieuwePartijForm.Visibility = Visibility.Collapsed;
            PartijStandpuntenForm.Visibility = Visibility.Collapsed;
            MainDataGrid.Visibility = Visibility.Visible;
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) { }
        private void PartijenPage_Unloaded(object sender, RoutedEventArgs e) => ClearForm();

        private void MainDataGrid_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
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

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
    }

    public class VoorbeeldPartij : System.ComponentModel.INotifyPropertyChanged
    {
        private string _naam, _afkorting, _kleurHex, _beschrijving;
        private Brush _kleurBrush;

        public int Id { get; set; }
        public string Naam { get => _naam; set { _naam = value; OnPropertyChanged(nameof(Naam)); } }
        public string Afkorting { get => _afkorting; set { _afkorting = value; OnPropertyChanged(nameof(Afkorting)); } }
        public string KleurHex { get => _kleurHex; set { _kleurHex = value; OnPropertyChanged(nameof(KleurHex)); } }
        public Brush KleurBrush { get => _kleurBrush; set { _kleurBrush = value; OnPropertyChanged(nameof(KleurBrush)); } }
        public string Beschrijving { get => _beschrijving; set { _beschrijving = value; OnPropertyChanged(nameof(Beschrijving)); } }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
    }

    public class PartijStellingMock : System.ComponentModel.INotifyPropertyChanged
    {
        private bool _isEens, _isNeutraal, _isOneens;
        private string _toelichting;

        public int Id { get; set; }
        public string Titel { get; set; }
        public string Categorie { get; set; }
        public string Beschrijving { get; set; }
        public string IdString { get; set; }

        public bool IsEens { get => _isEens; set { _isEens = value; OnPropertyChanged(nameof(IsEens)); } }
        public bool IsNeutraal { get => _isNeutraal; set { _isNeutraal = value; OnPropertyChanged(nameof(IsNeutraal)); } }
        public bool IsOneens { get => _isOneens; set { _isOneens = value; OnPropertyChanged(nameof(IsOneens)); } }
        public string Toelichting { get => _toelichting; set { _toelichting = value; OnPropertyChanged(nameof(Toelichting)); } }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
    }
}