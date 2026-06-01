using System.Windows.Controls;

namespace StemwijzerApp.Pages
{
    /// <summary>
    /// Interaction logic for StandpuntenPage.xaml
    /// </summary>
    public partial class StandpuntenPage : Page
    {
        public StandpuntenPage()
        {
            InitializeComponent();
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }

    public class VoorbeeldStandpunt
    {
        public string Titel {  get; set; }
        public string Categorie { get; set; }
        public string Beschrijving { get; set; }
    }
}
