using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
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

        private readonly string _bestandsPad = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "StemwijzerApp",
            "partijen.json"
        );

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
            try
            {
                if (File.Exists(_bestandsPad))
                {
                    string jsonString = File.ReadAllText(_bestandsPad);
                    if (!string.IsNullOrWhiteSpace(jsonString) && jsonString.Trim() != "[]")
                    {
                        Partijen = JsonSerializer.Deserialize<ObservableCollection<VoorbeeldPartij>>(jsonString);
                    }
                }
            }
            catch
            {
                Partijen = null;
            }

            if (Partijen == null || Partijen.Count < 2)
            {
                Partijen = new ObservableCollection<VoorbeeldPartij>
        {
            new VoorbeeldPartij { Afkorting = "VVD", Naam = "Volkspartij voor Vrijheid en Democratie", Beschrijving = "Liberale partij", Kleur = "#FF6B00" },
            new VoorbeeldPartij { Afkorting = "PvdA", Naam = "Partij van de Arbeid", Beschrijving = "Sociaaldemocratische partij", Kleur = "#E31B23" },
            new VoorbeeldPartij { Afkorting = "PVV", Naam = "Partij voor de Vrijheid", Beschrijving = "Rechtse populistische partij", Kleur = "#007BC7" },
            new VoorbeeldPartij { Afkorting = "GL", Naam = "GroenLinks", Beschrijving = "Groene en linkse partij", Kleur = "#74BD43" },
            new VoorbeeldPartij { Afkorting = "D66", Naam = "Democraten 66", Beschrijving = "Sociaal-liberale partij", Kleur = "#00AE41" },
            new VoorbeeldPartij { Afkorting = "CDA", Naam = "Christen-Democratisch Appèl", Beschrijving = "Christendemocratische partij", Kleur = "#007C5C" },
            new VoorbeeldPartij { Afkorting = "SP", Naam = "Socialistische Partij", Beschrijving = "Socialistische partij", Kleur = "#FF0000" },
            new VoorbeeldPartij { Afkorting = "CU", Naam = "ChristenUnie", Beschrijving = "Christelijk-sociale partij", Kleur = "#00A7EB" },
            new VoorbeeldPartij { Afkorting = "PvdD", Naam = "Partij voor de Dieren", Beschrijving = "Dierenrechtenpartij", Kleur = "#006B28" },
            new VoorbeeldPartij { Afkorting = "FvD", Naam = "Forum voor Democratie", Beschrijving = "Rechts-conservatieve partij", Kleur = "#800000" },
            new VoorbeeldPartij { Afkorting = "SGP", Naam = "Staatkundig Gereformeerde Partij", Beschrijving = "Orthodox-protestantse partij", Kleur = "#FF7300" }
        };
                SavePartijen();
            }
        }

        private void SavePartijen()
        {
            try
            {
                string mapPad = Path.GetDirectoryName(_bestandsPad);
                if (!Directory.Exists(mapPad)) Directory.CreateDirectory(mapPad);
                string jsonString = JsonSerializer.Serialize(Partijen, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_bestandsPad, jsonString);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij het opslaan: {ex.Message}");
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

            if (_geselecteerdePartij == null)
            {
                Partijen.Add(new VoorbeeldPartij
                {
                    Naam = TxtNaam.Text,
                    Afkorting = TxtAfkorting.Text,
                    Beschrijving = TxtBeschrijving.Text,
                    Kleur = kleurText
                });
            }
            else
            {
                _geselecteerdePartij.Naam = TxtNaam.Text;
                _geselecteerdePartij.Afkorting = TxtAfkorting.Text;
                _geselecteerdePartij.Beschrijving = TxtBeschrijving.Text;
                _geselecteerdePartij.Kleur = kleurText;
            }

            SavePartijen();
            ClearForm();
            PartijenPage_Loaded(null, null);
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
                if (_geselecteerdePartij == item) ClearForm();
                Partijen.Remove(item);
                SavePartijen();
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
        private string _naam, _afkorting, _beschrijving, _kleur;

        public string Naam { get => _naam; set { _naam = value; OnPropertyChanged(nameof(Naam)); } }
        public string Afkorting { get => _afkorting; set { _afkorting = value; OnPropertyChanged(nameof(Afkorting)); } }
        public string Beschrijving { get => _beschrijving; set { _beschrijving = value; OnPropertyChanged(nameof(Beschrijving)); } }
        public string Kleur { get => _kleur; set { _kleur = value; OnPropertyChanged(nameof(Kleur)); } }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
    }
}