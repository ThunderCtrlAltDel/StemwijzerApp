using System.Windows;
using System.Windows.Controls;
using StemwijzerApp.Pages;

namespace StemwijzerApp
{
    public partial class MainWindow : Window
    {
        DashboardPage dashboardPage = new DashboardPage();
        PartijenPage partijenPage = new PartijenPage();
        StandpuntenPage standpuntenPage = new StandpuntenPage();
        VerkiezingenPage verkiezingenPage = new VerkiezingenPage();
        GebruikersPage gebruikersPage = new GebruikersPage();
        StandpuntenarrangementenPage standpuntenarrangementenPage = new StandpuntenarrangementenPage();
        BeheerdersPage beheerdersPage = new BeheerdersPage();
        ResultatenPage resultatenPage = new ResultatenPage();

        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigate(new LoginPage());
        }

        public void ShowDashboard()
        {
            Sidebar.Visibility = Visibility.Visible;
            MainFrame.Navigate(dashboardPage);
            UpdateMenuStyles(BtnDashboard);
        }

        private void BtnUitloggen_Click(object sender, RoutedEventArgs e)
        {
            Sidebar.Visibility = Visibility.Collapsed;
            MainFrame.Navigate(new LoginPage());
            this.Title = "Stemwijzer Beheer - Inloggen";
        }

        private void BtnDashboard_Click(object sender, RoutedEventArgs e) { MainFrame.Navigate(dashboardPage); UpdateMenuStyles(BtnDashboard); }
        private void BtnPartijen_Click(object sender, RoutedEventArgs e) { MainFrame.Navigate(partijenPage); UpdateMenuStyles(BtnPartijen); }
        private void BtnStandpunten_Click(object sender, RoutedEventArgs e) { MainFrame.Navigate(standpuntenPage); UpdateMenuStyles(BtnStandpunten); }
        private void BtnVerkiezingen_Click(object sender, RoutedEventArgs e) { MainFrame.Navigate(verkiezingenPage); UpdateMenuStyles(BtnVerkiezingen); }
        private void BtnGebruikers_Click(object sender, RoutedEventArgs e) { MainFrame.Navigate(gebruikersPage); UpdateMenuStyles(BtnGebruikers); }
        private void BtnStandpuntenarrangementen_Click(object sender, RoutedEventArgs e) { MainFrame.Navigate(standpuntenarrangementenPage); UpdateMenuStyles(BtnStandpuntenarrangementen); }
        private void BtnBeheerders_Click(object sender, RoutedEventArgs e) { MainFrame.Navigate(beheerdersPage); UpdateMenuStyles(BtnBeheerders); }
        private void BtnResultaten_Click(object sender, RoutedEventArgs e) { MainFrame.Navigate(resultatenPage); UpdateMenuStyles(BtnResultaten); }

        private void UpdateMenuStyles(Button geselecteerdeKnop)
        {
            foreach (var child in MenuStackPanel.Children)
            {
                if (child is Button btn) btn.Style = (Style)FindResource("MenuButtonStyle");
            }
            geselecteerdeKnop.Style = (Style)FindResource("ActiveMenuButtonStyle");
            this.Title = "Stemwijzer Beheer - " + geselecteerdeKnop.Content.ToString();
        }
    }
}