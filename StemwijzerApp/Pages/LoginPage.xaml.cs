using System.Windows;
using System.Windows.Controls;

namespace StemwijzerApp.Pages
{
    public partial class LoginPage : Page
    {
        public LoginPage() => InitializeComponent();

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (TxtEmail.Text == "admin@stemwijzer.nl" && PbWachtwoord.Password == "admin123")
            {
                ((MainWindow)Application.Current.MainWindow).ShowDashboard();
            }
            else
            {
                MessageBox.Show("Ongeldige inloggegevens.");
            }
        }
    }
}