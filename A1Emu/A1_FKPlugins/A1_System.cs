using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Security.Cryptography;
using MySqlConnector;
using System.Xml;
using NetCoreServer;

//ArkONE Plugin 0
//System

class A1_System : TcpSession
{
        A1_Parser a1parser;

        FKUser a1user;

        int sessionPort = 80;

        string sqServer = "";

        public A1_System(TcpServer server, int port, string sqServerInput) : base(server){a1parser = new A1_Parser(); a1user = new FKUser(); sessionPort = port; sqServer = sqServerInput;}

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            string message = Encoding.ASCII.GetString(buffer, (int)offset, (int)size - 1);
            Console.WriteLine("[0 - System] Recieved: " + message);

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
                    case "a_lru":
                        response += LoginRegisteredUser(commandInfo[1], commandInfo[2], commandInfo[3]).Remove(0, 1);
                        break;
                    case "u_reg":
                        response += RegisterUser(commandInfo[1], commandInfo[2], commandInfo[3], commandInfo[4], commandInfo[5], commandInfo[6], commandInfo[7]).Remove(0, 1);
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

            string serviceID = p;

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

            switch(s){
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

        string RegisterUser(string sa, string sq, string d, string a, string c, string p, string l)
        {
            var responseStream = new MemoryStream();
            
            int resultCode = 0;

            //Making the password more secure.
            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);
            var encryptedP = new Rfc2898DeriveBytes(p,salt,10000);
            
            byte[] hash = encryptedP.GetBytes(20);
            byte[] hashBytes = new byte[36];

            Array.Copy(salt,0,hashBytes,0,16);
            Array.Copy(hash,0,hashBytes,16,20);
            string password = Convert.ToBase64String(hashBytes);

            //Setting up SQL connection.
            FKUser user = new FKUser();
            var con1 = new MySqlConnection(sqServer);  

            string sql1 = "SELECT * FROM user WHERE u=@Uname";
            MySqlCommand sqCommand = new MySqlCommand(sql1,con1);
            sqCommand.Parameters.AddWithValue("@Uname", l);
            con1.Open();
            using (MySqlDataReader sqReader = sqCommand.ExecuteReader())
            {
                while (sqReader.Read())
                {    
                    user.username = sqReader["u"].ToString();
                }
                con1.Close();
            }        
            if(user.username != null){
                //If username is claimed.
                resultCode = 1;
            }else{
                //Adds user to the DB.
                int uID = 0; 
                using (var con = new MySqlConnection(sqServer)){        
                    con.Open();

                    using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM user", con))
                    {
                        uID = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    var sql = "INSERT INTO user(u, p, sq, sa, uID) VALUES(@user, @pass, @secQ, @secA, @userID)";
                    using (var cmd = new MySqlCommand(sql, con)){
                        cmd.Parameters.AddWithValue("@user", l);
                        cmd.Parameters.AddWithValue("@pass", password);
                        cmd.Parameters.AddWithValue("@secQ", sq);
                        cmd.Parameters.AddWithValue("@secA", sa);
                        cmd.Parameters.AddWithValue("@userID", uID);
                        cmd.Prepare();

                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                    }
                    con.Close();
                }
            }

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            using (XmlWriter writer = XmlWriter.Create(responseStream, settings))
            {
                writer.WriteStartElement("u_reg");

                //Result
                writer.WriteAttributeString("r", resultCode.ToString());

                writer.WriteEndElement();
                writer.Flush();
                writer.Close();
            }

            return System.Text.ASCIIEncoding.UTF8.GetString(responseStream.ToArray());
        }

        string LoginRegisteredUser(string l, string p, string n)
        {
            var responseStream = new MemoryStream();

            int resultCode = 0;

            //Setting up SQL connection.
            var con = new MySqlConnection(sqServer);  

            string sql = "SELECT * FROM user WHERE u=@Uname";
            MySqlCommand sqCommand = new MySqlCommand(sql,con);
            sqCommand.Parameters.AddWithValue("@Uname", n);
            con.Open();
            using (MySqlDataReader sqReader = sqCommand.ExecuteReader())
            {
                while (sqReader.Read())
                {
                    a1user.username = sqReader["u"].ToString();
                    a1user.password = sqReader["p"].ToString();
                    a1user.userID = Int32.Parse(sqReader["uID"].ToString());
                }
                con.Close();
            }        
            if(a1user.username != null){
                //If user exists.
                try{
                    //Getting password ready for check.
                    byte[] hashBytes = Convert.FromBase64String(a1user.password);
                    byte[] salt = new byte[16];
                    Array.Copy(hashBytes,0,salt,0,16);
                    var encryptedP = new Rfc2898DeriveBytes(p,salt,10000);
                    byte[] hash = encryptedP.GetBytes(20);

                    bool correct = true;
                    for (int iC = 0; iC < 20; iC++){
                        if(hashBytes[iC + 16] != hash[iC]){
                            correct = false;
                        }
                    }
                    if(correct){
                        //Password correct.
                        resultCode = 0;
                    }else{
                        //Password incorrect.
                        resultCode = 4;
                    }
                }catch{
                    //If the check fails.
                    resultCode = 6;
                }
            }else{
                //If no user is found.
                resultCode = 5;
            }

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            using (XmlWriter writer = XmlWriter.Create(responseStream, settings))
            {
                writer.WriteStartElement("a_lru");

                //Result
                writer.WriteAttributeString("r", resultCode.ToString());

                if(resultCode == 0){
                    //User ID
                    writer.WriteAttributeString("u", a1user.userID.ToString());
                }

                //service?
                writer.WriteAttributeString("s", "1");

                writer.WriteEndElement();
                writer.Flush();
                writer.Close();
            }

            return System.Text.ASCIIEncoding.UTF8.GetString(responseStream.ToArray());
        }

        protected override void OnConnected()
        {
            Console.WriteLine($"[0 - System] TCP session with Id {Id} connected!");
        }

        protected override void OnDisconnected()
        {
            Console.WriteLine($"[0 - System] TCP session with Id {Id} disconnected!");
        }

        protected override void OnError(SocketError error)
        {
            Console.WriteLine($"[0 - System] Error - {error}");
        }

}

class ServerSystem : TcpServer
{
    int serverPort = 80;

    string sqServer = "";

    public ServerSystem(IPAddress address, int port, string sqServerInput) : base(address, port) {serverPort = port; sqServer = sqServerInput;}

    protected override TcpSession CreateSession() { return new A1_System(this, serverPort, sqServer); }

    protected override void OnError(SocketError error)
    {
        Console.WriteLine($"[0 - System] TCP server caught an error with code {error}");
    }
}