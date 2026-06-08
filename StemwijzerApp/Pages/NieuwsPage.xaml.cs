using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace StemwijzerApp.Pages
{
    /// <summary>
    /// Interaction logic for NieuwsPage.xaml
    /// </summary>
    public partial class NieuwsPage : Page
    {
        public NieuwsPage()
        {
            InitializeComponent();
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }

    public class VoorbeeldNieuws
    {
        public string Titel { get; set; }
        public string Auteur { get; set; }
        public string Categorie { get; set; }

        public string Datum { get; set; }
    }
}
