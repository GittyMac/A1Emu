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

//ArkONE Plugin 1
//User

class A1_User : TcpSession
{
        A1_Parser a1parser;

        FKUser a1user;

        int sessionPort = 80;

        public A1_User(TcpServer server, int port) : base(server){a1parser = new A1_Parser(); a1user = new FKUser(); sessionPort = port;}

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            string message = Encoding.ASCII.GetString(buffer, (int)offset, (int)size - 1);
            Console.WriteLine("[1 - User] Recieved: " + message);

            bool isMultiOutput = false;
            string response = "";

            string[] commands = a1parser.ParseReceivedMessage(message);
            foreach (string command in commands)
            {
                string[] commandInfo = a1parser.ParseCommand(command);
                switch (commandInfo[0])
                {
                    case "a_lgu":
                        response += LoginGuestUser(commandInfo[1], commandInfo[2], commandInfo[3]).Remove(0, 1);
                        break;
                    case "a_gpd":
                        response += GetPluginDetails(commandInfo[1]).Remove(0, 1);
                        break;
                    case "a_gsd":
                        response += GetServiceDetails(commandInfo[1]).Remove(0, 1);
                        break;
                }
            }


            Console.WriteLine(response);

            //Message Output
            List<byte[]> d = new List<byte[]>();
            Byte[] reply = Encoding.ASCII.GetBytes(response);
            byte[] b2 = new byte[] {0x00};
            d.Add(reply);
            d.Add(b2);
            byte[] b3 = d.SelectMany(a => a).ToArray();
            if(!isMultiOutput)
                SendAsync(b3, 0, b3.Length);
            else Server.Multicast(b3,0,b3.Length); SendAsync(b3,0,b3.Length);

            // Multicast message to all connected sessions
            //Server.Multicast(message);
        }

        string LoginGuestUser(string d, string a, string c){
            var responseStream = new MemoryStream();    

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            using (XmlWriter writer = XmlWriter.Create(responseStream, settings))
            {      
                writer.WriteStartElement("a_lgu");

                //Result
                writer.WriteAttributeString("r", "0");

                //User ID
                writer.WriteAttributeString("u", "0");

                //Username
                writer.WriteAttributeString("n", "GUESTUSER");

                //Password
                writer.WriteAttributeString("p", "");

                //Service ID
                writer.WriteAttributeString("s", "1");
                writer.WriteEndElement();
                writer.Flush();
                writer.Close();
            }

            return System.Text.ASCIIEncoding.UTF8.GetString(responseStream.ToArray());
        }

        string GetPluginDetails(string p){
            var responseStream = new MemoryStream();  

            string serviceID = "1";

            string xIPAddress = "localhost";
            string xPort = "80";

            string bIPAddress = "localhost";
            string bPort = "80";
            
            switch(p){
                //User
                case "1":
                    xPort = (sessionPort + 1).ToString();
                    bPort = (sessionPort + 1).ToString();
                    break;

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
            
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            using (XmlWriter writer = XmlWriter.Create(responseStream, settings))
            {      
                writer.WriteStartElement("a_gpd");

                //Service ID
                writer.WriteAttributeString("s", serviceID);

                //xIPAddress
                writer.WriteAttributeString("xi", xIPAddress);

                //xPort
                writer.WriteAttributeString("xp", xPort);

                //bIPAddress
                writer.WriteAttributeString("bi", bIPAddress);

                //bPort
                writer.WriteAttributeString("bp", bPort);

                //Plugin ID
                writer.WriteAttributeString("p", p);
                writer.WriteEndElement();
                writer.Flush();
                writer.Close();
            }

            return System.Text.ASCIIEncoding.UTF8.GetString(responseStream.ToArray());
        }

        string GetServiceDetails(string s)
        {
            var responseStream = new MemoryStream();

            string serviceID = s;

            string xIPAddress = "localhost";
            string xPort = "80";

            string bIPAddress = "localhost";
            string bPort = "80";

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            using (XmlWriter writer = XmlWriter.Create(responseStream, settings))
            {
                writer.WriteStartElement("a_gsd");

                //Service ID
                writer.WriteAttributeString("s", serviceID);

                //xIPAddress
                writer.WriteAttributeString("xi", xIPAddress);

                //xPort
                writer.WriteAttributeString("xp", xPort);

                //bIPAddress
                writer.WriteAttributeString("bi", bIPAddress);

                //bPort
                writer.WriteAttributeString("bp", bPort);

                writer.WriteEndElement();
                writer.Flush();
                writer.Close();
            }

            return System.Text.ASCIIEncoding.UTF8.GetString(responseStream.ToArray());
        }

        protected override void OnConnected()
        {
            Console.WriteLine($"[1 - User] TCP session with Id {Id} connected!");
        }

        protected override void OnDisconnected()
        {
            Console.WriteLine($"[1 - User] TCP session with Id {Id} disconnected!");
        }

        protected override void OnError(SocketError error)
        {
            Console.WriteLine($"[1 - User] Error - {error}");
        }

}

class ServerUser : TcpServer
{
    int serverPort = 80;

    public ServerUser(IPAddress address, int port) : base(address, port) {serverPort = port;}

    protected override TcpSession CreateSession() { return new A1_User(this, serverPort); }

    protected override void OnError(SocketError error)
    {
        Console.WriteLine($"[1 - User] TCP server caught an error with code {error}");
    }
}