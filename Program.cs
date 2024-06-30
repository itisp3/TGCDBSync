using System;
using System.IO;
using System.Timers;
using MySql.Data.MySqlClient;

namespace TGCDBSync
{
    class Program
    {
        static string connectionString = "Server=127.0.0.1;Port=3306;Database=eso;User ID=root;Password=admin;";
        static string filePath = "TGC_SavedVariables.lua";
        static System.Timers.Timer timer = new System.Timers.Timer();

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please provide the output file path as a command-line argument.");
                return;
            }

            filePath = args[0];

            // First call immediately
            WriteDataToFile();

            // Setup a timer to call WriteDataToFile every 15 minutes (900,000 milliseconds)
            timer = new System.Timers.Timer(900000);
            timer.Elapsed += TimerElapsed;
            timer.Start();

            // Prevent the application from exiting immediately
            Console.WriteLine("Press [Enter] to exit the application.");
            Console.ReadLine();
        }

        static void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            WriteDataToFile();
        }

        static void WriteDataToFile()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT Name, Banned, NI, AddedDate FROM players";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    using (StreamWriter writer = new StreamWriter(filePath, false))
                    {
                        while (reader.Read())
                        {
                            string name = reader.GetString("Name");
                            bool banned = reader.GetBoolean("Banned");
                            bool ni = reader.GetBoolean("NI");
                            DateTime addedDate = reader.GetDateTime("AddedDate");

                            string message = $"{name}: ";
                            if (banned)
                            {
                                message += "Banned";
                            }
                            if (ni)
                            {
                                message += "Not Interested";
                            }
                            message += $" (Added on {addedDate.ToShortDateString()})";
                            writer.WriteLine(message);
                        }
                    }
                }

                Console.WriteLine("Data written to file successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
    }
}
