using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StemwijzerApp.Pages
{
    public partial class DashboardPage : Page, System.ComponentModel.INotifyPropertyChanged
    {
        private string _totaalPartijen = "0";
        private string _actieveVerkiezingen = "0";
        private string _totaalGebruikers = "0";
        private string _ingevuldeStemwijzers = "0";

        private string _yAsMax = "4";
        private string _yAsDrieKwart = "3";
        private string _yAsHelft = "2";
        private string _yAsKwart = "1";

        public string TotaalPartijen { get => _totaalPartijen; set { _totaalPartijen = value; OnPropertyChanged(nameof(TotaalPartijen)); } }
        public string ActieveVerkiezingen { get => _actieveVerkiezingen; set { _actieveVerkiezingen = value; OnPropertyChanged(nameof(ActieveVerkiezingen)); } }
        public string TotaalGebruikers { get => _totaalGebruikers; set { _totaalGebruikers = value; OnPropertyChanged(nameof(TotaalGebruikers)); } }
        public string IngevuldeStemwijzers { get => _ingevuldeStemwijzers; set { _ingevuldeStemwijzers = value; OnPropertyChanged(nameof(IngevuldeStemwijzers)); } }

        public string YAsMax { get => _yAsMax; set { _yAsMax = value; OnPropertyChanged(nameof(YAsMax)); } }
        public string YAsDrieKwart { get => _yAsDrieKwart; set { _yAsDrieKwart = value; OnPropertyChanged(nameof(YAsDrieKwart)); } }
        public string YAsHelft { get => _yAsHelft; set { _yAsHelft = value; OnPropertyChanged(nameof(YAsHelft)); } }
        public string YAsKwart { get => _yAsKwart; set { _yAsKwart = value; OnPropertyChanged(nameof(YAsKwart)); } }

        public ObservableCollection<GrafiekBalk> GrafekBalken { get; set; } = new ObservableCollection<GrafiekBalk>();

        public DashboardPage()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += DashboardPage_Loaded;
        }

        private void DashboardPage_Loaded(object sender, RoutedEventArgs e)
        {
            BerekenLiveStatistieken();
            LaadGrafiekData();
        }

        private void BerekenLiveStatistieken()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection("Server=localhost;Database=stemwijzer;Uid=root;Pwd=;"))
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM parties", conn)) TotaalPartijen = string.Format("{0:N0}", Convert.ToInt32(cmd.ExecuteScalar()));
                    using (MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM elections", conn)) ActieveVerkiezingen = string.Format("{0:N0}", Convert.ToInt32(cmd.ExecuteScalar()));
                    using (MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM users WHERE role = 'user'", conn)) TotaalGebruikers = string.Format("{0:N0}", Convert.ToInt32(cmd.ExecuteScalar()));

                    string query = @"SELECT COUNT(*) FROM (SELECT ua.user_id, qq.questionnaire_id FROM user_answers ua JOIN questionnaire_questions qq ON ua.question_id = qq.question_id GROUP BY ua.user_id, qq.questionnaire_id) AS temp";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn)) IngevuldeStemwijzers = string.Format("{0:N0}", Convert.ToInt32(cmd.ExecuteScalar()));
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void LaadGrafiekData()
        {
            GrafekBalken.Clear();
            Dictionary<string, int> partijStemmen = new Dictionary<string, int>();
            Dictionary<string, string> kleuren = new Dictionary<string, string>();

            try
            {
                using (MySqlConnection conn = new MySqlConnection("Server=localhost;Database=stemwijzer;Uid=root;Pwd=;"))
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("SELECT abbreviation, color_hex FROM parties", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string abbr = reader.GetString(0);
                            partijStemmen[abbr] = 0;
                            kleuren[abbr] = reader.IsDBNull(1) ? "#3B82F6" : reader.GetString(1);
                        }
                    }

                    string query = @"SELECT p.abbreviation, ua.user_id, qq.questionnaire_id, SUM(CASE WHEN ua.answer = pa.answer THEN q.weight ELSE 0 END) AS score, SUM(q.weight) AS max FROM questions q JOIN questionnaire_questions qq ON q.id = qq.question_id JOIN questionnaires qn ON qq.questionnaire_id = qn.id JOIN election_parties ep ON qn.election_id = ep.election_id JOIN parties p ON ep.party_id = p.id LEFT JOIN user_answers ua ON q.id = ua.question_id LEFT JOIN party_answers pa ON q.id = pa.question_id AND pa.party_id = p.id WHERE ua.user_id IS NOT NULL GROUP BY p.abbreviation, ua.user_id, qq.questionnaire_id";

                    Dictionary<string, double> hoogste = new Dictionary<string, double>();
                    Dictionary<string, string> winnaar = new Dictionary<string, string>();
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string p = reader.GetString(0);
                            string key = reader.GetInt32(1) + "_" + reader.GetInt32(2);
                            double pct = (double)reader.GetInt32(3) / reader.GetInt32(4) * 100;
                            if (!hoogste.ContainsKey(key) || pct > hoogste[key]) { hoogste[key] = pct; winnaar[key] = p; }
                        }
                    }
                    foreach (var w in winnaar.Values) if (partijStemmen.ContainsKey(w)) partijStemmen[w]++;
                }

                var top = partijStemmen.OrderByDescending(x => x.Value).Take(5).ToList();
                int max = top.Count > 0 ? top.Max(x => x.Value) : 4;
                if (max == 0) max = 4;
                YAsMax = max.ToString(); YAsDrieKwart = (max * 0.75).ToString("F0"); YAsHelft = (max * 0.5).ToString("F0"); YAsKwart = (max * 0.25).ToString("F0");

                foreach (var item in top)
                {
                    GrafekBalken.Add(new GrafiekBalk { PartijNaam = item.Key, StemmenAantal = item.Value, BalkHoogte = ((double)item.Value / max) * 200, BalkKleur = kleuren[item.Key] });
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
    }

    public class GrafiekBalk
    {
        public string PartijNaam { get; set; }
        public int StemmenAantal { get; set; }
        public double BalkHoogte { get; set; }
        public string BalkKleur { get; set; }
    }
}