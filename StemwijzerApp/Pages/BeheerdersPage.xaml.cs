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
    /// Interaction logic for BeheerdersPage.xaml
    /// </summary>
    public partial class BeheerdersPage : Page
    {
        public BeheerdersPage()
        {
            InitializeComponent();
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }

    public class VoorbeeldBeheerder
    {
        public string Naam { get; set; }
        public string Email { get; set; }
        public string Rol { get; set; }

        public string Aangemaakt { get; set; }
    }
}
