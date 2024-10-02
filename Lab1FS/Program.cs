using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace Lab1FS
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string connectionString = "Data Source=catFacts.db";

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                string tableCommand = "CREATE TABLE IF NOT EXISTS CatFacts (Id INTEGER PRIMARY KEY AUTOINCREMENT, Fact TEXT)";
                using (var createTable = new SqliteCommand(tableCommand, connection))
                {
                    createTable.ExecuteNonQuery();
                }
            }

            while (true)
            {
                Console.WriteLine("Enter command (fact/data/exit):");
                string command = Console.ReadLine();

                if (command == "fact")
                {
                    await GetAndSaveCatFact(connectionString);
                }
                else if (command == "data")
                {
                    DisplayAllCatFacts(connectionString);
                }
                else if (command == "exit")
                {
                    break;
                }
                else
                {
                    Console.WriteLine("Unknown command. Please enter 'fact', 'data', or 'exit'.");
                }
            }
        }

        static async Task GetAndSaveCatFact(string connectionString)
        {
            string apiUrl = "https://catfact.ninja/fact";

            using HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();
            string responseData = await response.Content.ReadAsStringAsync();

            string fact = JsonDocument.Parse(responseData).RootElement.GetProperty("fact").GetString();

            Console.WriteLine("API Response:");
            Console.WriteLine(fact);

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                string insertCommand = "INSERT INTO CatFacts (Fact) VALUES (@Fact)";
                using (var insert = new SqliteCommand(insertCommand, connection))
                {
                    insert.Parameters.AddWithValue("@Fact", fact);
                    insert.ExecuteNonQuery();
                }
            }

            Console.WriteLine("Response saved to database.");
        }

        static void DisplayAllCatFacts(string connectionString)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                string selectCommand = "SELECT * FROM CatFacts";
                using (var select = new SqliteCommand(selectCommand, connection))
                {
                    using (SqliteDataReader reader = select.ExecuteReader())
                    {
                        Console.WriteLine("Stored Cat Facts:");
                        while (reader.Read())
                        {
                            Console.WriteLine(reader["Fact"]);
                        }
                    }
                }
            }
        }
    }
}
