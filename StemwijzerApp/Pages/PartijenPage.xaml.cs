using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StemwijzerApp.Pages
{
    public partial class PartijenPage : Page
    {
        public ObservableCollection<Partij> Partijen { get; set; }

        public PartijenPage()
        {
            InitializeComponent();
            LoadPartijen();
            DataContext = this;
        }

        private void LoadPartijen()
        {
            Partijen = new ObservableCollection<Partij>
            {
                new Partij("VVD", "Volkspartij voor Vrijheid en Democratie", "Liberale partij", (Brush)(new BrushConverter().ConvertFrom("#FF9800"))),
                new Partij("PvdA", "Partij van de Arbeid", "Sociaaldemocratische partij", (Brush)(new BrushConverter().ConvertFrom("#C2185B"))),
                new Partij("PVV", "Partij voor de Vrijheid", "Rechtse populistische partij", (Brush)(new BrushConverter().ConvertFrom("#2196F3"))),
                new Partij("GL", "GroenLinks", "Groene en linkse partij", (Brush)(new BrushConverter().ConvertFrom("#8BC34A"))),
                new Partij("D66", "Democraten 66", "Sociaal-liberale partij", (Brush)(new BrushConverter().ConvertFrom("#2E7D32"))),
                new Partij("CDA", "Christen-Democratisch Appèl", "Christendemocratische partij", (Brush)(new BrushConverter().ConvertFrom("#00695C"))),
                new Partij("SP", "Socialistische Partij", "Socialistische partij", (Brush)(new BrushConverter().ConvertFrom("#F44336"))),
                new Partij("CU", "ChristenUnie", "Christelijk-sociale partij", (Brush)(new BrushConverter().ConvertFrom("#03A9F4"))),
                new Partij("PvdD", "Partij voor de Dieren", "Dierenrechtenpartij", (Brush)(new BrushConverter().ConvertFrom("#006400"))),
                new Partij("FvD", "Forum voor Democratie", "Rechts-conservatieve partij", (Brush)(new BrushConverter().ConvertFrom("#7B1F1F"))),
                new Partij("SGP", "Staatkundig Gereformeerde Partij", "Orthodox-protestantse partij", (Brush)(new BrushConverter().ConvertFrom("#FF9800"))),
                new Partij("DENK", "DENK", "Sociaaldemocratische migrantenpartij", (Brush)(new BrushConverter().ConvertFrom("#00BCD4")))
            };
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }
    }

    public class Partij
    {
        public string Afkorting { get; set; }
        public string Naam { get; set; }
        public string Beschrijving { get; set; }
        public Brush Kleur { get; set; }

        public Partij(string afk, string naam, string beschrijving, Brush kleur)
        {
            Afkorting = afk;
            Naam = naam;
            Beschrijving = beschrijving;
            Kleur = kleur;
        }
    }
}