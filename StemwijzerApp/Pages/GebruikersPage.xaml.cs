using System.Windows.Controls;

namespace StemwijzerApp.Pages
{
    /// <summary>
    /// Interaction logic for GebruikersPage.xaml
    /// </summary>
    public partial class GebruikersPage : Page
    {
        public GebruikersPage()
        {
            InitializeComponent();
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }

    public class VoorbeeldGebruiker
    {
        public string Naam { get; set; }
        public string Gebruikersnaam { get; set; }
        public string Email { get; set; }
        public string Geboortedatum { get; set; }
        public string Woonplaats { get; set; }
        public string Rol { get; set; }
        public string Aangemaakt { get; set; }
    }
}
