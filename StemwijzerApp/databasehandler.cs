using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace StemwijzerApp
{
    public class DatabaseHandler
    {

        private string connectionString = "Server=localhost;Database=stemwijzer;Uid=root;Pwd=;Convert Zero Datetime=True;";
        private MySqlConnection connection;

        public DatabaseHandler()
        {
            connection = new MySqlConnection(connectionString);
        }

        public void OpenConnection()
        {
            try
            {
                if (connection.State == System.Data.ConnectionState.Closed)
                {
                    connection.Open();
                    Console.WriteLine("Verbinding geopend!");
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Fout bij het openen van de verbinding: {ex.Message}");
            }
        }

        public void CloseConnection()
        {
            try
            {
                if (connection.State == System.Data.ConnectionState.Open)
                {
                    connection.Close();
                    Console.WriteLine("Verbinding gesloten!");
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Fout bij het sluiten van de verbinding: {ex.Message}");
            }
        }

        public void ExecuteQuery(string query)
        {
            try
            {
                OpenConnection();
                MySqlCommand command = new MySqlCommand(query, connection);
                int rowsAffected = command.ExecuteNonQuery();
                Console.WriteLine($"{rowsAffected} rij(en) aangepast.");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Fout bij het uitvoeren van de query: {ex.Message}");
            }
            finally
            {
                CloseConnection();
            }
        }

        public List<Verkiezing> ReadElections()
        {
            List<Verkiezing> verkiezingen = new List<Verkiezing>();
            string query = "SELECT id, name, date, description FROM elections";

            try
            {
                OpenConnection();
                MySqlCommand command = new MySqlCommand(query, connection);
                MySqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Verkiezing v = new Verkiezing
                    {
                        Id = reader.GetInt32("id"),
                        Name = reader.GetString("name"),
                        Date = reader.GetDateTime("date"),
                        Description = reader.IsDBNull(reader.GetOrdinal("description")) ? "" : reader.GetString("description")
                    };
                    verkiezingen.Add(v);
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Fout bij het lezen van verkiezingen: {ex.Message}");
            }
            finally
            {
                CloseConnection();
            }
            return verkiezingen;
        }

        public List<Stelling> GetQuestionsByQuestionnaire(int questionnaireId)
        {
            List<Stelling> stellingen = new List<Stelling>();
            string query = "SELECT id, question, weight FROM questions WHERE questionnaire_id = @id";

            try
            {
                OpenConnection();
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", questionnaireId);
                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    stellingen.Add(new Stelling
                    {
                        Id = reader.GetInt32("id"),
                        Question = reader.GetString("question"),
                        Weight = reader.GetInt32("weight")
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fout bij ophalen stellingen: {ex.Message}");
            }
            finally
            {
                CloseConnection();
            }
            return stellingen;
        }


        public bool SaveUserAnswer(int userId, int questionId, int answer)
        {

            string query = @"INSERT INTO user_answers (user_id, question_id, answer) 
                             VALUES (@userId, @questionId, @answer)
                             ON DUPLICATE KEY UPDATE answer = @answer";

            try
            {
                OpenConnection();
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@userId", userId);
                command.Parameters.AddWithValue("@questionId", questionId);
                command.Parameters.AddWithValue("@answer", answer);

                return command.ExecuteNonQuery() > 0;
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Fout bij opslaan antwoord: {ex.Message}");
                return false;
            }
            finally
            {
                CloseConnection();
            }
        }


        public List<PartijAntwoord> GetPartyAnswersByElection(int electionId)
        {
            List<PartijAntwoord> partijAntwoorden = new List<PartijAntwoord>();
            string query = @"SELECT pa.party_id, p.name AS party_name, pa.question_id, pa.answer 
                             FROM party_answers pa
                             JOIN parties p ON pa.party_id = p.id
                             JOIN election_parties ep ON p.id = ep.party_id
                             WHERE ep.election_id = @electionId";

            try
            {
                OpenConnection();
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@electionId", electionId);
                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    partijAntwoorden.Add(new PartijAntwoord
                    {
                        PartyId = reader.GetInt32("party_id"),
                        PartyName = reader.GetString("party_name"),
                        QuestionId = reader.GetInt32("question_id"),
                        Answer = reader.GetInt32("answer")
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fout bij ophalen partijantwoorden: {ex.Message}");
            }
            finally
            {
                CloseConnection();
            }
            return partijAntwoorden;
        }

        public Gebruiker Login(string email, string password)
        {
            string query = "SELECT id, name, role FROM users WHERE email = @email AND password = @password";

            try
            {
                OpenConnection();
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@email", email);
                cmd.Parameters.AddWithValue("@password", password); // Let op: in je test-SQL staan momenteel Bcrypt hashes, dit is voor platte tekst.
                MySqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return new Gebruiker
                    {
                        Id = reader.GetInt32("id"),
                        Name = reader.GetString("name"),
                        Role = reader.GetString("role")
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fout bij inloggen: {ex.Message}");
            }
            finally
            {
                CloseConnection();
            }
            return null; 
        }
    }

 
    public class Verkiezing { public int Id { get; set; } public string Name { get; set; } public DateTime Date { get; set; } public string Description { get; set; } }
    public class Stelling { public int Id { get; set; } public string Question { get; set; } public int Weight { get; set; } }
    public class PartijAntwoord { public int PartyId { get; set; } public string PartyName { get; set; } public int QuestionId { get; set; } public int Answer { get; set; } }
    public class Gebruiker { public int Id { get; set; } public string Name { get; set; } public string Role { get; set; } }
}
