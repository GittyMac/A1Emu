using System;
using System.Threading;
using System.IO;
using System.Reflection;
using System.Data;

class Program
{
    static void Main(string[] args)
    {
        //Configuration
        int port = 2000;
        string directory = "";
        string sqServer = "";
        string ip = "localhost";
        if(!File.Exists(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/config")){
            Console.WriteLine("[ERROR] A1Emu did not find a config file in the application's directory.");
        }
        try{
            foreach (string line in File.ReadLines(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/config"))
            {  
                if(line.Contains("port=")){
                    port = Int32.Parse(line.Replace("port=",""));
                }else if(line.Contains("directory=")){
                    directory = line.Replace("directory=","");
                }else if(line.Contains("sq=")){
                    sqServer = line.Replace("sq=","");
                }else if(line.Contains("ip=")){
                    ip = line.Replace("ip=","");
                }
            }  
        }catch{
            Console.WriteLine("[A1Emu] ERROR! Failed to load the config file.");
        }
        
        //TODO - Generate a SQL DB.

        //Plugin 0/1/7
        //Thread t = new Thread(delegate ()
        //{
            //replace the IP with your system IP Address...
            //ServerRewrite myserver = new ServerRewrite(ip, port, directory, sqServer);
        //});
        //t.Start();

        var A1_Plugin0 = new ServerSystem(System.Net.IPAddress.Parse(ip), port, sqServer);
        A1_Plugin0.Start();

        var A1_Plugin1 = new ServerUser(System.Net.IPAddress.Parse(ip), port + 1);
        A1_Plugin1.Start();

        var A1_Plugin7 = new ServerGalaxy(System.Net.IPAddress.Parse(ip), port + 7);
        A1_Plugin7.Start();

        var A1_Plugin10 = new ServerTrunk(System.Net.IPAddress.Parse(ip), port + 10);
        A1_Plugin10.Start();

        Console.WriteLine("[A1Emu] Started!");

        for(;;){
            string line = Console.ReadLine();
            if (string.IsNullOrEmpty(line))
                break;
        }

        A1_Plugin0.Stop();
        A1_Plugin1.Stop();
        A1_Plugin7.Stop();
        A1_Plugin10.Stop();
    }
}
