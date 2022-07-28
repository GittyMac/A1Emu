using System;
using System.IO;
using System.Reflection;
using MySqlConnector;

class Program
{
    static void Main(string[] args)
    {
        //Configuration
        int port = 2000;
        string directory = "";
        string sqServer = "";
        string ip = "localhost";

        if (!File.Exists(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/config"))
        {
            Console.WriteLine("[ERROR] A1Emu did not find a config file in the application's directory.");
        }

        try
        {
            foreach (string line in File.ReadLines(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/config"))
            {
                if (line.Contains("port="))
                {
                    port = Int32.Parse(line.Replace("port=", ""));
                }
                else if (line.Contains("directory="))
                {
                    directory = line.Replace("directory=", "");
                }
                else if (line.Contains("sq="))
                {
                    sqServer = line.Replace("sq=", "");
                }
                else if (line.Contains("ip="))
                {
                    ip = line.Replace("ip=", "");
                }
            }
        }
        catch
        {
            Console.WriteLine("[A1Emu] ERROR! Failed to load the config file.");
        }

        //TODO - Generate a SQL DB. (Will probably be an external script or a manual README instruction)

        //TODO - Truncate all the multiplayer tables whenever they get added.

        Console.WriteLine("[A1Emu] Performing SQL cleanup...");

        var con = new MySqlConnection(sqServer);

        string sqlResetOnineStatus = "UPDATE user SET isOnline = 0;";
        string sqlTruncateMP5 = "TRUNCATE TABLE mp_5;";

        MySqlCommand resetOnlineStatus = new MySqlCommand(sqlResetOnineStatus, con);
        MySqlCommand truncateMP5 = new MySqlCommand(sqlTruncateMP5, con);
        
        con.Open();
        resetOnlineStatus.ExecuteNonQuery();
        truncateMP5.ExecuteNonQuery();
        con.Close();

        var A1_Plugin0 = new ServerSystem(System.Net.IPAddress.Parse(ip), port, sqServer, directory);
        A1_Plugin0.Start();

        Console.WriteLine("[A1Emu] Started!");

        for (;;)
        {
            string line = Console.ReadLine();
            if (string.IsNullOrEmpty(line))
                break;
        }

        A1_Plugin0.Stop();
    }
}
