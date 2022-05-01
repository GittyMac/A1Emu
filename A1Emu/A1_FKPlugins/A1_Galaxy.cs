using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using NetCoreServer;

//ArkONE Plugin 7
//Galaxy

class A1_Galaxy : TcpSession
{
        A1_Parser a1parser;

        FKUser a1user;

        int sessionPort = 80;

        public A1_Galaxy(TcpServer server, int port) : base(server){a1parser = new A1_Parser(); a1user = new FKUser(); sessionPort = port;}

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            string message = Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
            Console.WriteLine("[7 - Galaxy] Recieved: " + message);

            bool isMultiOutput = false;
            string response = "";

            string[] commands = a1parser.ParseReceivedMessage(message);
            foreach(string command in commands)
            {
                string[] commandInfo = a1parser.ParseCommand(command);
                switch(commandInfo[0]){
                    case "a_lgu":
                        response += LoginGuestUser(commandInfo[1],commandInfo[2],commandInfo[3]);
                        break;
                    case "a_gpd":
                        //TODO - start other A1 plugins
                        response += GetPluginDetails(commandInfo[1]);
                        break;
                }
            }

            //Message Output
            List<byte[]> d = new List<byte[]>();
            Byte[] reply = Encoding.ASCII.GetBytes(response);
            byte[] b2 = new byte[] {0x00};
            d.Add(reply);
            d.Add(b2);
            byte[] b3 = d.SelectMany(a => a).ToArray();
            if(!isMultiOutput)
                SendAsync(b3,0,b3.Length);
            else Server.Multicast(b3,0,b3.Length); SendAsync(b3,0,b3.Length);

            // Multicast message to all connected sessions
            //Server.Multicast(message);
        }

        string LoginGuestUser(string d, string a, string c){
            var responseStream = new MemoryStream();    
            using (XmlWriter writer = XmlWriter.Create(responseStream))
            {      
                writer.WriteStartElement("a_lgu");

                //Result
                writer.WriteElementString("r", "0");

                //User ID
                writer.WriteElementString("u", "0");

                //Username
                writer.WriteElementString("n", "GUESTUSER");

                //Password
                writer.WriteElementString("p", "");

                //Service ID
                writer.WriteElementString("s", "1");
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }

            return System.Text.ASCIIEncoding.UTF8.GetString(responseStream.ToArray());
        }

        string GetPluginDetails(string p){
            var responseStream = new MemoryStream();  

            string serviceID = p;

            string xIPAddress = "localhost";
            string xPort = "80";

            string bIPAddress = "localhost";
            string bPort = "80";
            
            switch(p){
                //Galaxy
                case "7":
                    xPort = (sessionPort + 7).ToString();
                    bPort = (sessionPort + 7).ToString();
                    break;

                //Trunk
                case "10":
                    xPort = (sessionPort + 10).ToString();
                    bPort = (sessionPort + 10).ToString();
                    break;
            }
              
            using (XmlWriter writer = XmlWriter.Create(responseStream))
            {      
                writer.WriteStartElement("a_gpd");

                //Service ID
                writer.WriteElementString("s", serviceID);

                //xIPAddress
                writer.WriteElementString("xi", xIPAddress);

                //xPort
                writer.WriteElementString("xp", xPort);

                //bIPAddress
                writer.WriteElementString("bi", bIPAddress);

                //Plugin ID
                writer.WriteElementString("p", p);
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }

            return System.Text.ASCIIEncoding.UTF8.GetString(responseStream.ToArray());
        }

        protected override void OnConnected()
        {
            Console.WriteLine($"[7 - Galaxy] TCP session with Id {Id} connected!");
        }

        protected override void OnDisconnected()
        {
            Console.WriteLine($"[7 - Galaxy] TCP session with Id {Id} disconnected!");
        }

        protected override void OnError(SocketError error)
        {
            Console.WriteLine($"[7 - Galaxy] Error - {error}");
        }
}

class ServerGalaxy : TcpServer
{
    int serverPort = 80;

    public ServerGalaxy(IPAddress address, int port) : base(address, port) {serverPort = port;}

    protected override TcpSession CreateSession() { return new A1_Galaxy(this, serverPort); }

    protected override void OnError(SocketError error)
    {
        Console.WriteLine($"[7 - Galaxy] TCP server caught an error with code {error}");
    }
}