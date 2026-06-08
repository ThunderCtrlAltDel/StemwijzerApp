using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StemwijzerApp.Pages
{
    public partial class PartijDetailPage : Page, INotifyPropertyChanged
    {
        // ── INotifyPropertyChanged ────────────────────────────────────────────
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        // ── Gebonden properties ───────────────────────────────────────────────
        private string _afkorting = "";
        public string Afkorting
        {
            get => _afkorting;
            set { _afkorting = value; OnPropertyChanged(); }
        }

        private string _naam = "";
        public string Naam
        {
            get => _naam;
            set { _naam = value; OnPropertyChanged(); OnPropertyChanged(nameof(StandpuntenTitel)); }
        }

        private string _beschrijving = "";
        public string Beschrijving
        {
            get => _beschrijving;
            set { _beschrijving = value; OnPropertyChanged(); }
        }

        private Brush _partijKleur = Brushes.Gray;
        public Brush PartijKleur
        {
            get => _partijKleur;
            set { _partijKleur = value; OnPropertyChanged(); }
        }

        public string StandpuntenTitel => $"Standpunten van {Afkorting}";

        public ObservableCollection<Standpunt> Standpunten { get; set; } = new();

        // ── Constructor ───────────────────────────────────────────────────────
        public PartijDetailPage(Partij partij)
        {
            InitializeComponent();
            DataContext = this;

            Afkorting = partij.Afkorting;
            Naam = partij.Naam;
            Beschrijving = partij.Beschrijving;
            PartijKleur = partij.Kleur;

            LoadStandpunten(partij.Afkorting);
        }

        // ── Demo-data per partij ──────────────────────────────────────────────
        private void LoadStandpunten(string afkorting)
        {
            // Standaard voorbeeldstandpunten voor alle partijen
            // In een echte app haal je dit op uit een database/API
            var standpunten = new ObservableCollection<Standpunt>
            {
                new Standpunt
                {
                    Titel        = "Meer windmolens bouwen",
                    StellingTekst = "Er moeten meer windmolens gebouwd worden voor duurzame energie",
                    Categorie    = "Klimaat",
                    Positie      = GetDefaultPositie(afkorting, "windmolens"),
                    Toelichting  = GetDefaultToelichting(afkorting, "windmolens"),
                },
                new Standpunt
                {
                    Titel        = "Belastingverlaging middeninkomens",
                    StellingTekst = "De belastingen moeten omlaag voor middeninkomens",
                    Categorie    = "Economie",
                    Positie      = GetDefaultPositie(afkorting, "belasting"),
                    Toelichting  = GetDefaultToelichting(afkorting, "belasting"),
                },
                new Standpunt
                {
                    Titel        = "Hogere AOW-leeftijd",
                    StellingTekst = "De AOW-leeftijd moet verder omhoog naar 68 jaar",
                    Categorie    = "Sociale zekerheid",
                    Positie      = GetDefaultPositie(afkorting, "aow"),
                    Toelichting  = GetDefaultToelichting(afkorting, "aow"),
                },
                new Standpunt
                {
                    Titel        = "Strengere asielwetgeving",
                    StellingTekst = "De regels voor asielaanvragen moeten strenger worden",
                    Categorie    = "Migratie",
                    Positie      = GetDefaultPositie(afkorting, "asiel"),
                    Toelichting  = GetDefaultToelichting(afkorting, "asiel"),
                },
                new Standpunt
                {
                    Titel        = "Meer geld naar onderwijs",
                    StellingTekst = "Het budget voor het basisonderwijs moet flink omhoog",
                    Categorie    = "Onderwijs",
                    Positie      = GetDefaultPositie(afkorting, "onderwijs"),
                    Toelichting  = GetDefaultToelichting(afkorting, "onderwijs"),
                },
            };

            Standpunten = standpunten;
            OnPropertyChanged(nameof(Standpunten));
        }

        // Demo-logica: geeft een standaard positie op basis van partij en thema
        private static string GetDefaultPositie(string afkorting, string thema)
        {
            return (afkorting, thema) switch
            {
                ("VVD", "windmolens") => "Neutraal",
                ("VVD", "belasting") => "Eens",
                ("VVD", "aow") => "Eens",
                ("VVD", "asiel") => "Eens",
                ("VVD", "onderwijs") => "Neutraal",

                ("PvdA", "windmolens") => "Eens",
                ("PvdA", "belasting") => "Eens",
                ("PvdA", "aow") => "Oneens",
                ("PvdA", "asiel") => "Oneens",
                ("PvdA", "onderwijs") => "Eens",

                ("PVV", "windmolens") => "Oneens",
                ("PVV", "belasting") => "Eens",
                ("PVV", "aow") => "Oneens",
                ("PVV", "asiel") => "Eens",
                ("PVV", "onderwijs") => "Neutraal",

                ("GL", "windmolens") => "Eens",
                ("GL", "belasting") => "Neutraal",
                ("GL", "aow") => "Oneens",
                ("GL", "asiel") => "Oneens",
                ("GL", "onderwijs") => "Eens",

                ("D66", "windmolens") => "Eens",
                ("D66", "belasting") => "Eens",
                ("D66", "aow") => "Neutraal",
                ("D66", "asiel") => "Neutraal",
                ("D66", "onderwijs") => "Eens",

                _ => "Neutraal"
            };
        }

        private static string GetDefaultToelichting(string afkorting, string thema)
        {
            return (afkorting, thema) switch
            {
                ("VVD", "windmolens") => "VVD steunt duurzame energie maar wil ook ruimte voor bedrijfsleven",
                ("VVD", "belasting") => "VVD wil lagere lasten voor werkenden om de economie te stimuleren",
                ("VVD", "aow") => "VVD vindt dat de pensioenleeftijd mee moet groeien met de levensverwachting",
                ("VVD", "asiel") => "VVD wil streng maar rechtvaardig asielbeleid",
                ("VVD", "onderwijs") => "VVD investeert in de kwaliteit van leraren en technisch onderwijs",

                ("PvdA", "windmolens") => "PvdA ziet windenergie als essentieel onderdeel van de energietransitie",
                ("PvdA", "belasting") => "PvdA wil eerlijkere belasting zodat middeninkomens meer overhouden",
                ("PvdA", "aow") => "PvdA vindt dat mensen niet langer moeten doorwerken dan zij aankunnen",
                ("PvdA", "asiel") => "PvdA wil humaan en rechtvaardig asielbeleid dat vluchtelingen beschermt",
                ("PvdA", "onderwijs") => "PvdA wil fors investeren zodat elk kind gelijke kansen krijgt",

                _ => $"{afkorting} heeft nog geen toelichting ingevoerd voor dit standpunt."
            };
        }

        // ── Positie-knoppen ───────────────────────────────────────────────────
        private void PositieEens_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button { Tag: Standpunt s }) s.Positie = "Eens";
        }

        private void PositieNeutraal_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button { Tag: Standpunt s }) s.Positie = "Neutraal";
        }

        private void PositieOneens_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button { Tag: Standpunt s }) s.Positie = "Oneens";
        }

        // ── Opslaan ───────────────────────────────────────────────────────────
        private void OpslaanButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: sla standpunten op naar database/JSON
            MessageBox.Show($"Standpunten van {Naam} zijn opgeslagen!",
                            "Opgeslagen", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // ── Terug-knop ────────────────────────────────────────────────────────
        private void TerugButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }

    }

    // ── Standpunt model ───────────────────────────────────────────────────────
    public class Standpunt : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public string Titel { get; set; } = "";
        public string StellingTekst { get; set; } = "";
        public string Categorie { get; set; } = "";

        private string _positie = "Neutraal";
        public string Positie
        {
            get => _positie;
            set { _positie = value; OnPropertyChanged(); }
        }

        private string _toelichting = "";
        public string Toelichting
        {
            get => _toelichting;
            set { _toelichting = value; OnPropertyChanged(); }
        }
    }
}
