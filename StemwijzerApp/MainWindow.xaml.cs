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

namespace StemwijzerApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Pages.DashboardPage dashboardPage = new Pages.DashboardPage();
        Pages.PartijenPage partijenPage = new Pages.PartijenPage();
        Pages.StandpuntenPage standpuntenPage = new Pages.StandpuntenPage();
        Pages.VerkiezingenPage verkiezingenPage = new Pages.VerkiezingenPage();
        Pages.GebruikersPage gebruikersPage = new Pages.GebruikersPage();
        Pages.ReactiesPage reactiesPage = new Pages.ReactiesPage();
        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigate(new Pages.DashboardPage());
            BtnDashboard.Style = (Style)FindResource("ActiveMenuButtonStyle");
            this.Title = "Stemwijzer Beheer - Dashboard";
        }
    
        private void BtnDashboard_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(dashboardPage);
            UpdateMenuStyles(BtnDashboard);
            
        }
        private void BtnPartijen_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(partijenPage);
            UpdateMenuStyles(BtnPartijen);
            
        }

        private void BtnStandpunten_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(standpuntenPage);
            UpdateMenuStyles(BtnStandpunten);
            
        }

        private void BtnVerkiezingen_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(verkiezingenPage);
            UpdateMenuStyles(BtnVerkiezingen);
           
        }

        private void BtnGebruikers_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(gebruikersPage);
            UpdateMenuStyles(BtnGebruikers);
            
        }

        private void BtnReacties_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(reactiesPage);
            UpdateMenuStyles(BtnReacties);
            
        }

        private void UpdateMenuStyles(Button geselecteerdeKnop)
        {
            foreach (var child in MenuStackPanel.Children)
            {
                if (child is Button btn)
                {
                    btn.Style = (Style)FindResource("MenuButtonStyle");
                }
            }

            geselecteerdeKnop.Style = (Style)FindResource("ActiveMenuButtonStyle");
            this.Title = "Stemwijzer Beheer - " + geselecteerdeKnop.Content.ToString();
        }



    }
}
