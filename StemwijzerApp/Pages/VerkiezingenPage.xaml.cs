using System.Windows.Controls;

namespace StemwijzerApp.Pages
{
    /// <summary>
    /// Interaction logic for VerkiezingenPage.xaml
    /// </summary>
    public partial class VerkiezingenPage : Page
    {
        public VerkiezingenPage()
        {
            InitializeComponent();
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }

    public class VoorbeeldVerkiezing
    {
        public string Naam { get; set; }
        public string Datum { get; set; }
        public string Type { get; set; }
        public string Beschrijving { get; set; }
        public string Acties { get; set; }
    }
}
