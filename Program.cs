using System;
using System.IO;
using System.Timers;
using System.Net.Http;
using System.Threading.Tasks;
using System.Reflection.PortableExecutable;
using System.Media;
using static BanListJSONResponse;
using System.Text.Json;

string connectionString = "Server=127.0.0.1;Port=3306;Database=eso;User ID=root;Password=admin;";
string filePath = "TGC_SavedVariables.lua";
System.Timers.Timer timer = new System.Timers.Timer();
bool terminateRequested = false;

// Check if TGC_FILE_PATH environment variable is set
string envFilePath = Environment.GetEnvironmentVariable("TGC_FILE_PATH");
if (!string.IsNullOrEmpty(envFilePath))
{
    filePath = envFilePath + "\\TGC_SavedVariables.lua";
}

// First call immediately
WriteDataToFile();

// Setup a timer to call WriteDataToFile every 15 minutes (900,000 milliseconds)
timer = new System.Timers.Timer(10000);
timer.Elapsed += TimerElapsed;
Console.WriteLine(terminateRequested);
timer.Start();

// Setup a separate thread to handle user input asynchronously
Console.WriteLine("Press [Enter] to exit the application.");
SetupInputListener();

// Prevent the application from exiting immediately
while (!terminateRequested)
{
    // Do nothing, let the timer and input listener continue running
    System.Threading.Thread.Sleep(1000); // Sleep briefly to avoid CPU usage
}

// Clean up resources if needed
timer.Stop();
timer.Dispose();

Console.WriteLine("Application terminated. Press any key to exit.");
Console.ReadKey(); // Wait for user to acknowledge

void TimerElapsed(object sender, ElapsedEventArgs e)
{
    WriteDataToFile();
}

async Task WriteDataToFile()
{
    try
    {
        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("x-query-key", "fetchBanList");
            HttpResponseMessage response = await client.GetAsync("https://tgc.moktor.com/banlist");
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            Player[] players = ConvertResponseBodyToPlayerList(responseBody);

            using (StreamWriter writer = new StreamWriter(filePath, false))
            {
                writer.WriteLine("TGC_SavedVariables =");
                writer.WriteLine("{");
                writer.WriteLine("    [\"players\"] =");
                writer.WriteLine("    {");

                int index = 1;
                foreach(Player player in players)
                {
                    DateTime addedDate = DateTime.Now;

                    writer.WriteLine($"        [{index}] = ");
                    writer.WriteLine("        {");
                    writer.WriteLine($"            [\"Name\"] = \"{player.playerName}\",");
                    writer.WriteLine($"            [\"Banned\"] = {player.banned.ToString().ToLower()},");
                    writer.WriteLine($"            [\"NI\"] = {player.notInterested.ToString().ToLower()},");
                    writer.WriteLine($"            [\"AddedDate\"] = \"{addedDate.ToString("yyyy-MM-dd")}\",");
                    writer.WriteLine("        },");
                    index++;
                }

                writer.WriteLine("    },");
                writer.WriteLine("}");
            }
        }

        Console.WriteLine("Data written to file successfully." + terminateRequested);
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error: " + ex.Message);
    }
}

Player[] ConvertResponseBodyToPlayerList(string responseBody)
{
    var response = JsonSerializer.Deserialize<BanListResponse>(responseBody);
    var playerDict = new Dictionary<string, Player>();

    foreach (var player in response.BanList)
    {
        if (!playerDict.ContainsKey(player.PlayerName))
        {
            playerDict[player.PlayerName] = new Player
            {
                playerName = player.PlayerName,
                banned = true,
                notInterested = false
            };
        }
        else
        {
            playerDict[player.PlayerName].banned = true;
        }
    }

    foreach (var player in response.NotInterested)
    {
        if (!playerDict.ContainsKey(player.PlayerName))
        {
            playerDict[player.PlayerName] = new Player
            {
                playerName = player.PlayerName,
                banned = false,
                notInterested = true
            };
        }
        else
        {
            playerDict[player.PlayerName].notInterested = true;
        }
    }

    return playerDict.Values.ToArray();
}
async void SetupInputListener()
{
    await System.Threading.Tasks.Task.Run(() =>
    {
        while (!terminateRequested)
        {
            if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Enter)
            {
                terminateRequested = true;
                break;
            }
        }
    });
}