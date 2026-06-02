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
    public partial class ResultatenPage : Page
    {
        public ResultatenPage()
        {
            InitializeComponent();
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           
        }
    }

    public class VoorbeeldResultaat
    {
        public string Gebruiker { get; set; }
        public string Verkiezing { get; set; }
        public string Voortgang { get; set; }
        public string BesteMatch { get; set; }
        public int OvereenkomstWaarde { get; set; }
        public string Datum { get; set; }
    }
}