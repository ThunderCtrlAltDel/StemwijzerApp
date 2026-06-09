using System.Windows;
using System.Windows.Controls;

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
        Pages.StandpuntenarrangementenPage standpuntenarrangementenPage = new Pages.StandpuntenarrangementenPage();
        Pages.BeheerdersPage beheerdersPage = new Pages.BeheerdersPage();
        Pages.ResultatenPage resultatenPage = new Pages.ResultatenPage();
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

        private void BtnStandpuntenarrangementen_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(standpuntenarrangementenPage);
            UpdateMenuStyles(BtnStandpuntenarrangementen);
        }

        private void BtnBeheerders_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(beheerdersPage);
            UpdateMenuStyles(BtnBeheerders);
        }

        private void BtnResultaten_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(resultatenPage);
            UpdateMenuStyles(BtnResultaten);
        }
    }
}
