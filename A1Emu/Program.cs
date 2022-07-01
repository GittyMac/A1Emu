using System;
using System.IO;
using System.Reflection;

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

        //TODO - Generate a SQL DB.

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
