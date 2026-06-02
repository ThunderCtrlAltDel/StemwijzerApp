using System.Windows.Controls;

namespace StemwijzerApp.Pages
{
    /// <summary>
    /// Interaction logic for ReactiesPage.xaml
    /// </summary>
    public partial class ReactiesPage : Page
    {
        public ReactiesPage()
        {
            InitializeComponent();
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }

    public class VoorbeeldReactie
    {
        public string Auteur { get; set; }
        public string Artikel { get; set; }
        public string Inhoud { get; set; }

        public string Status { get; set; }
        public string Datum { get; set; }
        
    }
}
