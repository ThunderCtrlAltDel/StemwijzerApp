using System.Windows.Controls;

namespace StemwijzerApp.Pages
{
    /// <summary>
    /// Interaction logic for PartijenPage.xaml
    /// </summary>
    public partial class PartijenPage : Page
    {
        public PartijenPage()
        {
            InitializeComponent();
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }

    public class VoorbeeldPartij
    {
        public string Afkorting { get; set; }
        public string Naam { get; set; }
        public string Beschrijving { get; set; }
        public string HexKleur { get; set; }
    }
}
