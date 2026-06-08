using System.Windows.Controls;

namespace StemwijzerApp.Pages
{
    /// <summary>
    /// Interaction logic for StandpuntenarrangementenPage.xaml
    /// </summary>
    public partial class StandpuntenarrangementenPage : Page
    {
        public StandpuntenarrangementenPage()
        {
            InitializeComponent();
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }

    public class VoorbeeldArrangement
    {
        public string Naam { get; set; }
        public string Beschrijving { get; set; }
        public string Verkiezing { get; set; }
        public string Aantal { get; set; }
        public string Aangemaakt { get; set; }
    }
}
