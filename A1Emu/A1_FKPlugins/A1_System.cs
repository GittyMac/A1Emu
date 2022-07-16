using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Security.Cryptography;
using System.Xml;
using MySqlConnector;
using NetCoreServer;

//ArkONE Plugin 0
//System

class A1_System : TcpSession
{
        A1_Parser a1_Parser;

        A1_Sender a1_Sender;

        FKUser a1user;

        int sessionPort = 80;

        string sqServer = "";

        string serverDirectory = "";

        int chunksLeft = 0;

        string saveData = "";

        public A1_System(TcpServer server, int port, string sqServerInput, string directory) : base(server)
        {
            a1_Parser = new A1_Parser(); 
            a1_Sender = new A1_Sender();
            a1user = new FKUser(); 
            sessionPort = port; sqServer = sqServerInput; 
            serverDirectory = directory;
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            string message = Encoding.ASCII.GetString(buffer, (int)offset, (int)size - 1);
            Console.WriteLine("[0 - System - " + this.Id + "] Recieved: " + message);
            
            List<string> responses = new List<string>();

            //Initializes a buddy list if null.
            if(a1user.buddies == null){
                a1user.buddies = new List<FKUser>();
            }

            string[] commands = a1_Parser.ParseReceivedMessage(message);
            foreach (string command in commands)
            {
                string[] commandInfo = a1_Parser.ParseCommand(command);
                switch (commandInfo[0])
                {
                    //Plugin 0 (Core)
                    case "a_lgu":
                        responses.Add(LoginGuestUser(commandInfo[1], commandInfo[2], commandInfo[3]).Remove(0, 1));
                        break;
                    case "a_gpd":
                        responses.Add(GetPluginDetails(commandInfo[1]).Remove(0, 1));
                        break;
                    case "a_gsd":
                        responses.Add(GetServerDetails(commandInfo[1]).Remove(0, 1));
                        break;
                    case "a_lru":
                        responses.Add(LoginRegisteredUser(commandInfo[1], commandInfo[2], commandInfo[3]).Remove(0, 1));
                        break;
                    case "u_reg":
                        responses.Add(RegisterUser(commandInfo[1], commandInfo[2], commandInfo[3], commandInfo[4], commandInfo[5], commandInfo[6], commandInfo[7]).Remove(0, 1));
                        break;

                    //Plugin 1 (User)
                    case "u_gbl":
                        responses.Add(GetBuddyList());
                        break;
                    case "u_ccs":
                        responses.Add(ChangeChatStatus(commandInfo[1]));
                        break;
                    case "u_cph":
                        responses.Add(ChangePhoneStatus(commandInfo[1]));
                        break;
                    case "u_abd":
                        responses.Add(AddBuddy(commandInfo[1]));
                        break;
                    case "u_abr":
                        responses.Add(AddBuddyResponse(commandInfo[1], commandInfo[2]));
                        break;
                    case "u_spm":
                        responses.Add(SendPrivateMessage(commandInfo[1], commandInfo[2], commandInfo[3]));
                        break;
                    case "u_dbd":
                        responses.Add(DeleteBuddy(commandInfo[1]));
                        break;
                    case "u_dbr":
                        responses.Add(DeleteBuddyResponse(commandInfo[1], commandInfo[2], commandInfo[3]));
                        break;

                    //Plugin 7 (Galaxy)
                    case "lpv":
                        responses.Add(LoadProfileVersion());
                        break;
                    case "vsu":
                        responses.Add(VersionStatisticsRequest(commandInfo[1]));
                        break;
                    case "sp":
                        responses.Add(SaveProfile(commandInfo[1]));
                        break;
                    case "spp":
                        responses.Add(SaveProfilePart(commandInfo[1], commandInfo[2]));
                        break;
                    case "lp":
                        responses.Add(LoadProfile());
                        break;
                    case "gls":
                        responses.Add(GetLeaderboardStats(commandInfo[1]));
                        break;


                    default:
                        responses.Add(@"<unknown />");
                        break;
                }
            }

            foreach(string responseString in responses){
                //Formats the reponse to send.
                List<byte[]> d = new List<byte[]>();
                Byte[] reply = Encoding.ASCII.GetBytes(responseString);
                byte[] b2 = new byte[] {0x00};
                d.Add(reply);
                d.Add(b2);
                byte[] b3 = d.SelectMany(a => a).ToArray();

                Console.WriteLine(responseString);
                Send(b3, 0, b3.Length);
            }
        }

        //========
        //Commands
        //========

        //Plugin 0

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

            string serverID = "1";

            string xIPAddress = "localhost";
            string xPort = "80";

            string bIPAddress = "localhost";
            string bPort = "80";
            
            switch(p){
                //User
                case "1":
                    xPort = (sessionPort).ToString();
                    bPort = (sessionPort).ToString();
                    break;

                //Galaxy
                case "7":
                    xPort = (sessionPort).ToString();
                    bPort = (sessionPort).ToString();
                    break;

                //Trunk
                case "10":
                    xPort = (sessionPort).ToString();
                    bPort = (sessionPort).ToString();
                    break;
            }
            
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            using (XmlWriter writer = XmlWriter.Create(responseStream, settings))
            {      
                writer.WriteStartElement("a_gpd");

                //Server ID
                writer.WriteAttributeString("s", serverID);

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

        string GetServerDetails(string s)
        {
            var responseStream = new MemoryStream();

            string serverID = "1";

            string xIPAddress = "localhost";
            string xPort = "80";

            string bIPAddress = "localhost";
            string bPort = "80";

            switch(s){
                //User
                case "1":
                    xPort = (sessionPort).ToString();
                    bPort = (sessionPort).ToString();
                    break;

                //Galaxy
                case "7":
                    xPort = (sessionPort).ToString();
                    bPort = (sessionPort).ToString();
                    break;

                //Trunk
                case "10":
                    xPort = (sessionPort).ToString();
                    bPort = (sessionPort).ToString();
                    break;
            }

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            using (XmlWriter writer = XmlWriter.Create(responseStream, settings))
            {
                writer.WriteStartElement("a_gsd");

                //Server ID
                writer.WriteAttributeString("s", serverID);

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
            //TODO - Find an alternative to the obsolete RNGCryptoServiceProvider.
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
                        string sqlID = "UPDATE user SET connectionID = @cID WHERE uID=@userID";
                        MySqlCommand sqCommandID = new MySqlCommand(sqlID,con);
                        sqCommandID.Parameters.AddWithValue("@userID", a1user.userID);
                        sqCommandID.Parameters.AddWithValue("@cID", this.Id);
                        con.Open();
                        sqCommandID.ExecuteNonQuery();
                        con.Close();
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

        //PLUGIN 1 Commands
        string GetBuddyList(){
            var responseStream = new MemoryStream();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            settings.Encoding = Encoding.ASCII;
            using (XmlWriter writer = XmlWriter.Create(responseStream, settings))
            {
                writer.WriteStartElement("u_gbl");
                writer.WriteAttributeString("r", "0");

                string buddyList = "";

                var con = new MySqlConnection(sqServer);  
                string sql = "SELECT buddyList FROM user WHERE uID=@userID";
                MySqlCommand sqCommand = new MySqlCommand(sql,con);
                sqCommand.Parameters.AddWithValue("@userID", a1user.userID);
                con.Open();
                using (MySqlDataReader sqReader = sqCommand.ExecuteReader())
                {
                    while (sqReader.Read())
                    {    
                        buddyList = sqReader["buddyList"].ToString();
                    }
                    con.Close();
                }   
                Console.WriteLine(buddyList);
                a1user.rawBuddies = buddyList.Split(',');

                if(buddyList != ""){
                    foreach(string buddy in a1user.rawBuddies){
                        var conB = new MySqlConnection(sqServer);  
                        string sqlB = "SELECT * FROM user WHERE uID=@userID";
                        MySqlCommand sqCommandB = new MySqlCommand(sqlB,conB);
                        sqCommandB.Parameters.AddWithValue("@userID", buddy);
                        conB.Open();
                        using (MySqlDataReader sqReader = sqCommandB.ExecuteReader())
                        {
                            FKUser buddyUser = new FKUser();

                            while (sqReader.Read())
                            {    
                                buddyUser.userID = Convert.ToInt32(buddy);
                                buddyUser.username = sqReader["u"].ToString();
                                buddyUser.isOnline = Convert.ToInt32(sqReader["isOnline"]);
                                buddyUser.status = Convert.ToInt32(sqReader["chatStatus"]);
                                if(sqReader["phoneStatus"].ToString() != ""){
                                    buddyUser.phoneStatus = Convert.ToInt32(sqReader["phoneStatus"]);
                                }else{
                                    buddyUser.phoneStatus = 0;
                                }
                                
                                if(buddyUser.isOnline == 1){
                                    buddyUser.connectionID = sqReader["connectionID"].ToString();
                                }
                            }

                            writer.WriteStartElement("buddy");
                            writer.WriteAttributeString("id", buddyUser.userID.ToString());
                            writer.WriteAttributeString("n", buddyUser.username);
                            writer.WriteAttributeString("s", buddyUser.status.ToString());
                            writer.WriteAttributeString("o", buddyUser.isOnline.ToString());
                            writer.WriteAttributeString("ph", buddyUser.phoneStatus.ToString());
                            writer.WriteEndElement();

                            conB.Close();
                        }   
                    }
                }

                a1user.isOnline = 1;
                string sql1 = "UPDATE user SET isOnline = @online WHERE uID=@userID";
                MySqlCommand onlineSet = new MySqlCommand(sql1,con);
                onlineSet.Parameters.AddWithValue("@userID", a1user.userID);
                onlineSet.Parameters.AddWithValue("@online", "1");
                con.Open();
                onlineSet.ExecuteNonQuery();
                con.Close();

                if(buddyList != "")
                    SendStatusUpdate("u_cos", "o", a1user.isOnline.ToString());

                writer.WriteEndElement();
                writer.Flush();
                writer.Close();
            }

            return System.Text.ASCIIEncoding.ASCII.GetString(responseStream.ToArray());
        }

        string ChangeChatStatus(string s){
            //Chat Statuses
            //0 - Ready to Party
            //1 - Do Not Disturb
            //2 - Playing
            //3 - Partying
            a1user.status = int.Parse(s);

            var con = new MySqlConnection(sqServer);  
            string sql1 = "UPDATE user SET chatStatus = @ccs WHERE uID=@userID";
            MySqlCommand sqCommand = new MySqlCommand(sql1,con);
            sqCommand.Parameters.AddWithValue("@userID", a1user.userID);
            sqCommand.Parameters.AddWithValue("@ccs", a1user.status);
            con.Open();
            sqCommand.ExecuteNonQuery();
            con.Close();

            var responseStream = new MemoryStream();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            settings.Encoding = Encoding.ASCII;
            using (XmlWriter writer = XmlWriter.Create(responseStream, settings))
            {
                writer.WriteStartElement("u_ccs");

                writer.WriteAttributeString("s", a1user.status.ToString());

                writer.WriteAttributeString("id", a1user.userID.ToString());

                writer.WriteEndElement();
                writer.Flush();
                writer.Close();
            }

            SendStatusUpdate("u_ccs", "s", a1user.status.ToString());

            return System.Text.ASCIIEncoding.ASCII.GetString(responseStream.ToArray());
        }

        string ChangePhoneStatus(string ph){
            a1user.phoneStatus = int.Parse(ph);

            var con = new MySqlConnection(sqServer);  
            string sql1 = "UPDATE user SET chatStatus = @ccs WHERE uID=@userID";
            MySqlCommand sqCommand = new MySqlCommand(sql1,con);
            sqCommand.Parameters.AddWithValue("@userID", a1user.userID);
            sqCommand.Parameters.AddWithValue("@ccs", a1user.status);
            con.Open();
            sqCommand.ExecuteNonQuery();
            con.Close();

            var responseStream = new MemoryStream();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            settings.Encoding = Encoding.ASCII;
            using (XmlWriter writer = XmlWriter.Create(responseStream, settings))
            {
                writer.WriteStartElement("u_cph");

                writer.WriteAttributeString("ph", a1user.phoneStatus.ToString());

                writer.WriteAttributeString("id", a1user.userID.ToString());

                writer.WriteEndElement();
                writer.Flush();
                writer.Close();
            }

            SendStatusUpdate("u_cph", "ph", a1user.phoneStatus.ToString());

            return System.Text.ASCIIEncoding.ASCII.GetString(responseStream.ToArray());
        }

        string SendPrivateMessage(string m, string t, string f){
            var responseStream = new MemoryStream();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            settings.Encoding = Encoding.ASCII;
            using (XmlWriter writer = XmlWriter.Create(responseStream, settings))
            {
                writer.WriteStartElement("u_spm");

                writer.WriteAttributeString("r", "0");

                writer.WriteAttributeString("m", m);

                writer.WriteAttributeString("t", t);

                writer.WriteAttributeString("f", f);

                writer.WriteEndElement();
                writer.Flush();
                writer.Close();
            }

            string conID = "";

            var conB = new MySqlConnection(sqServer);  
            string sqlB = "SELECT * FROM user WHERE uID=@userID";
            MySqlCommand sqCommandB = new MySqlCommand(sqlB,conB);
            sqCommandB.Parameters.AddWithValue("@userID", t);
            conB.Open();
            using (MySqlDataReader sqReader = sqCommandB.ExecuteReader())
            {
                FKUser buddyUser = new FKUser();

                while (sqReader.Read())
                {    
                    buddyUser.isOnline = Convert.ToInt32(sqReader["isOnline"]);
                    if(buddyUser.isOnline == 1){
                        conID = sqReader["connectionID"].ToString();
                    }
                }

                conB.Close();
            }  

            a1_Sender.SendToUser(this.Server, conID, System.Text.ASCIIEncoding.ASCII.GetString(responseStream.ToArray()));

            return System.Text.ASCIIEncoding.ASCII.GetString(responseStream.ToArray());
        }

        string AddBuddy(string n){
            FKUser buddy = new FKUser();
            var con = new MySqlConnection(sqServer);  

            string sql = "SELECT * FROM user WHERE u=@Uname";
            MySqlCommand sqCommand = new MySqlCommand(sql,con);
            sqCommand.Parameters.AddWithValue("@Uname", n);
            con.Open();
            using (MySqlDataReader sqReader = sqCommand.ExecuteReader())
            {
                while (sqReader.Read())
                {    
                    buddy.username = sqReader["u"].ToString();
                    try{
                    buddy.userID = int.Parse(sqReader["uID"].ToString());
                    buddy.status = int.Parse(sqReader["chatStatus"].ToString());
                    buddy.isOnline = int.Parse(sqReader["isOnline"].ToString());
                    if(buddy.isOnline == 1){
                        buddy.connectionID = sqReader["connectionID"].ToString();
                    }
                    }catch{

                    }
                }
                con.Close();
            }

            bool fail = false;
            bool isAlreadyBuddy = false;
            foreach(string buddyItem in a1user.rawBuddies){ if(buddyItem == buddy.userID.ToString()){ isAlreadyBuddy = true; } }

            var responseStream = new MemoryStream();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            settings.Encoding = Encoding.ASCII;
            using (XmlWriter writer = XmlWriter.Create(responseStream, settings))
            {
                writer.WriteStartElement("u_abd");

                if(buddy.isOnline == 0){
                    writer.WriteAttributeString("r", "5");
                    fail = true;
                }else if(isAlreadyBuddy){
                    writer.WriteAttributeString("r", "3");
                    fail = true;
                }else if(buddy.username == null || buddy.username == "GUESTUSER"){
                    writer.WriteAttributeString("r", "2");
                    fail = true;
                }

                writer.WriteAttributeString("n", n);

                writer.WriteEndElement();
                writer.Flush();
                writer.Close();
            }
            if(fail){
                return System.Text.ASCIIEncoding.ASCII.GetString(responseStream.ToArray());
            }else{
                var responseStream1 = new MemoryStream();
                using (XmlWriter writer1 = XmlWriter.Create(responseStream1, settings))
                {
                    writer1.WriteStartElement("u_abr");

                    writer1.WriteAttributeString("b", a1user.userID.ToString());

                    writer1.WriteAttributeString("n", a1user.username.ToString());

                    writer1.WriteEndElement();
                    writer1.Flush();
                    writer1.Close();
                }
                a1_Sender.SendToUser(this.Server, buddy.connectionID, System.Text.ASCIIEncoding.ASCII.GetString(responseStream1.ToArray()));
                return "<notneeded/>";
            }
        }

        string AddBuddyResponse(string r, string n){
            bool accepted = false;

            FKUser buddy = new FKUser();
            var con = new MySqlConnection(sqServer);  

            string sql = "SELECT * FROM user WHERE u=@Uname";
            MySqlCommand sqCommand = new MySqlCommand(sql,con);
            sqCommand.Parameters.AddWithValue("@Uname", n);
            con.Open();
            using (MySqlDataReader sqReader = sqCommand.ExecuteReader())
            {
                while (sqReader.Read())
                {    
                    buddy.username = sqReader["u"].ToString();
                    buddy.userID = int.Parse(sqReader["uID"].ToString());
                    buddy.status = int.Parse(sqReader["chatStatus"].ToString());
                    buddy.isOnline = int.Parse(sqReader["isOnline"].ToString());
                    if(sqReader["phoneStatus"].ToString() != ""){
                    buddy.phoneStatus = int.Parse(sqReader["phoneStatus"].ToString());
                    }else{
                        buddy.phoneStatus = 0;
                    }
                    if(buddy.isOnline == 1){
                        buddy.connectionID = sqReader["connectionID"].ToString();
                    }
                }
                con.Close();
            }

            var responseStream = new MemoryStream();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            settings.Encoding = Encoding.ASCII;
            using (XmlWriter writer = XmlWriter.Create(responseStream, settings))
            {
                writer.WriteStartElement("u_abd");

                if(r == "1"){
                    accepted = true;
                    writer.WriteAttributeString("r", "0");
                    string sql1 = "UPDATE user SET buddyList = CONCAT_WS(',', buddyList, @buddy) WHERE uID=@userID";
                    MySqlCommand sqCommand1 = new MySqlCommand(sql1,con);
                    sqCommand1.Parameters.AddWithValue("@userID", a1user.userID.ToString());
                    sqCommand1.Parameters.AddWithValue("@buddy", buddy.userID.ToString());
                    con.Open();
                    sqCommand1.ExecuteNonQuery();
                    con.Close();

                    string sql2 = "UPDATE user SET buddyList = CONCAT_WS(',', buddyList, @userID) WHERE uID=@buddy";
                    MySqlCommand sqCommand2 = new MySqlCommand(sql2,con);
                    sqCommand2.Parameters.AddWithValue("@userID", a1user.userID.ToString());
                    sqCommand2.Parameters.AddWithValue("@buddy", buddy.userID.ToString());
                    con.Open();
                    sqCommand2.ExecuteNonQuery();
                    con.Close();
                    writer.WriteAttributeString("b", buddy.userID.ToString());
                    writer.WriteAttributeString("ph", buddy.phoneStatus.ToString());
                    writer.WriteAttributeString("s", buddy.status.ToString());
                    writer.WriteAttributeString("o", buddy.isOnline.ToString());
                }

                var responseStream1 = new MemoryStream();
                using (XmlWriter writer1 = XmlWriter.Create(responseStream1, settings))
                {
                    writer1.WriteStartElement("u_abd");

                    if(r == "1"){
                        writer1.WriteAttributeString("r", "0");
                        writer1.WriteAttributeString("ph", a1user.phoneStatus.ToString());
                        writer1.WriteAttributeString("s", a1user.status.ToString());
                        writer1.WriteAttributeString("o", a1user.isOnline.ToString());
                    }else{
                        writer1.WriteAttributeString("r", "4");
                    }

                    writer1.WriteAttributeString("b", a1user.userID.ToString());

                    writer1.WriteAttributeString("n", a1user.username);

                    writer1.WriteEndElement();
                    writer1.Flush();
                    writer1.Close();
                }
                a1_Sender.SendToUser(this.Server, buddy.connectionID, System.Text.ASCIIEncoding.ASCII.GetString(responseStream1.ToArray()));

                writer.WriteAttributeString("n", n);

                writer.WriteEndElement();
                writer.Flush();
                writer.Close();
            }

            if(accepted){
                return System.Text.ASCIIEncoding.ASCII.GetString(responseStream.ToArray());
            }else{
                return "<notneeded/>";
            }
            
        }

        string DeleteBuddy(string b){
            FKUser buddy = new FKUser();
            var con = new MySqlConnection(sqServer);  

            string sql = "SELECT * FROM user WHERE uID=@userID";
            MySqlCommand sqCommand = new MySqlCommand(sql,con);
            sqCommand.Parameters.AddWithValue("@userID", b);
            con.Open();
            using (MySqlDataReader sqReader = sqCommand.ExecuteReader())
            {
                while (sqReader.Read())
                {    
                    buddy.username = sqReader["u"].ToString();
                    try{
                    buddy.userID = int.Parse(sqReader["uID"].ToString());
                    buddy.status = int.Parse(sqReader["chatStatus"].ToString());
                    buddy.isOnline = int.Parse(sqReader["isOnline"].ToString());
                    buddy.rawBuddies = sqReader["buddyList"].ToString().Split(',');
                    if(buddy.isOnline == 1){
                        buddy.connectionID = sqReader["connectionID"].ToString();
                    }
                    }catch{

                    }
                }
                con.Close();
            }

            bool fail = false;

            var responseStream = new MemoryStream();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            settings.Encoding = Encoding.ASCII;
            using (XmlWriter writer = XmlWriter.Create(responseStream, settings))
            {
                writer.WriteStartElement("u_dbd");

                if(buddy.isOnline == 0){
                    writer.WriteAttributeString("r", "5");
                    fail = true;
                }else if(buddy.username == null || buddy.username == "GUESTUSER"){
                    writer.WriteAttributeString("r", "2");
                    fail = true;
                }

                writer.WriteAttributeString("u", a1user.userID.ToString());

                writer.WriteAttributeString("b", b);

                writer.WriteEndElement();
                writer.Flush();
                writer.Close();
            }
            if(fail){
                return System.Text.ASCIIEncoding.ASCII.GetString(responseStream.ToArray());
            }else{
                var responseStream1 = new MemoryStream();
                using (XmlWriter writer1 = XmlWriter.Create(responseStream1, settings))
                {
                    writer1.WriteStartElement("u_dbr");

                    writer1.WriteAttributeString("b", a1user.userID.ToString());

                    writer1.WriteAttributeString("n", a1user.username.ToString());

                    writer1.WriteEndElement();
                    writer1.Flush();
                    writer1.Close();
                }
                a1_Sender.SendToUser(this.Server, buddy.connectionID, System.Text.ASCIIEncoding.ASCII.GetString(responseStream1.ToArray()));

                var buddyList = new List<string>(a1user.rawBuddies);
                buddyList.Remove(buddy.userID.ToString());
                a1user.rawBuddies = buddyList.ToArray();
                string buddies = String.Join(",", a1user.rawBuddies);
                string sql1 = "UPDATE user SET buddyList = @buddies WHERE uID=@userID";
                MySqlCommand sqCommand1 = new MySqlCommand(sql1,con);
                sqCommand1.Parameters.AddWithValue("@userID", a1user.userID.ToString());
                if(buddies != ""){
                    sqCommand1.Parameters.AddWithValue("@buddies", buddies);
                }else{
                    sqCommand1.Parameters.AddWithValue("@buddies", DBNull.Value);
                }
                con.Open();
                sqCommand1.ExecuteNonQuery();
                con.Close();

                buddyList = new List<string>(buddy.rawBuddies);
                buddyList.Remove(a1user.userID.ToString());
                buddy.rawBuddies = buddyList.ToArray();
                buddies = String.Join(",", buddy.rawBuddies);
                string sql2 = "UPDATE user SET buddyList = @buddies WHERE uID=@buddy";
                MySqlCommand sqCommand2 = new MySqlCommand(sql2,con);
                if(buddies != ""){
                    sqCommand2.Parameters.AddWithValue("@buddies", buddies);
                }else{
                    sqCommand2.Parameters.AddWithValue("@buddies", DBNull.Value);
                }
                sqCommand2.Parameters.AddWithValue("@buddy", buddy.userID.ToString());
                con.Open();
                sqCommand2.ExecuteNonQuery();
                con.Close();

                var responseStream2 = new MemoryStream();
                using (XmlWriter writer1 = XmlWriter.Create(responseStream2, settings))
                {
                    writer1.WriteStartElement("u_dbd");

                    writer1.WriteAttributeString("r", "0");

                    writer1.WriteAttributeString("u", a1user.userID.ToString());

                    writer1.WriteAttributeString("b", b);

                    writer1.WriteEndElement();
                    writer1.Flush();
                    writer1.Close();
                }

                return System.Text.ASCIIEncoding.ASCII.GetString(responseStream2.ToArray());
            }
        }

        string DeleteBuddyResponse(string r, string n, string b){
            var responseStream = new MemoryStream();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            settings.Encoding = Encoding.ASCII;
            using (XmlWriter writer = XmlWriter.Create(responseStream, settings))
            {
                writer.WriteStartElement("u_dbd");

                //There is literally no way to decline it, so it'll always be a 0.
                writer.WriteAttributeString("r", "0");

                writer.WriteAttributeString("u", a1user.userID.ToString());

                writer.WriteAttributeString("b", b);

                writer.WriteEndElement();
                writer.Flush();
                writer.Close();
            }

            return System.Text.ASCIIEncoding.ASCII.GetString(responseStream.ToArray());
        }



        //PLUGIN 7 Commands

        string LoadProfileVersion(){
            var responseStream = new MemoryStream();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            settings.Encoding = Encoding.ASCII;
            using (XmlWriter writer = XmlWriter.Create(responseStream, settings))
            {
                writer.WriteStartElement("h7_0");
                writer.WriteStartElement("lpv");

                //Save ID/Version
                if(File.Exists(serverDirectory + a1user.username + @"/profile")){
                    XmlDocument xmlDocument = new XmlDocument();
                    xmlDocument.LoadXml(File.ReadAllText(serverDirectory + a1user.username + @"/profile"));
                    XmlElement root = xmlDocument.DocumentElement;
                    string saveID = root.GetAttribute("sid");
                    writer.WriteAttributeString("v", saveID.ToString());
                }

                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.Flush();
                writer.Close();
            }

            return System.Text.ASCIIEncoding.ASCII.GetString(responseStream.ToArray());
        }

        string VersionStatisticsRequest(string id){
            var responseStream = new MemoryStream();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            settings.Encoding = Encoding.ASCII;
            using (XmlWriter writer = XmlWriter.Create(responseStream, settings))
            {
                writer.WriteStartElement("h7_0");
                writer.WriteStartElement("vsu");

                writer.WriteAttributeString("id", "0");

                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.Flush();
                writer.Close();
            }

            return System.Text.ASCIIEncoding.ASCII.GetString(responseStream.ToArray());
        } 

        string SaveProfile(string c){
            var responseStream = new MemoryStream();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            settings.Encoding = Encoding.ASCII;

            //Gets the number of chunks to save.
            chunksLeft = int.Parse(c);

            saveData = "";


            using (XmlWriter writer = XmlWriter.Create(responseStream, settings))
            {
                writer.WriteStartElement("h7_0");
                writer.WriteStartElement("rr");

                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.Flush();
                writer.Close();
            }

            return System.Text.ASCIIEncoding.ASCII.GetString(responseStream.ToArray());
        } 

        string SaveProfilePart(string v, string n){
            var responseStream = new MemoryStream();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            settings.Encoding = Encoding.ASCII;

            using (XmlWriter writer = XmlWriter.Create(responseStream, settings))
            {
                writer.WriteStartElement("h7_0");

                saveData = v + saveData;

                if(chunksLeft == 1){
                    writer.WriteStartElement("sp");

                    saveData = WebUtility.HtmlDecode(saveData);

                    XmlDocument save = new XmlDocument();
                    save.LoadXml(saveData);
                    XmlElement rootSave = save.DocumentElement;

                    string profileName = rootSave.GetAttribute("gname");
                    if(rootSave.GetAttribute("sid") != ""){
                        writer.WriteAttributeString("v", (int.Parse(rootSave.GetAttribute("sid")) + 1).ToString());
                    }else{
                        writer.WriteAttributeString("v", "1");
                    }

                    if(!Directory.Exists(serverDirectory + profileName)){
                        Directory.CreateDirectory(serverDirectory + profileName);
                    }
                    File.WriteAllText(serverDirectory + profileName + @"/profile", saveData);
                }else{
                    writer.WriteStartElement("rr");
                    chunksLeft -= 1;
                }

                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.Flush();
                writer.Close();
            }

            return System.Text.ASCIIEncoding.ASCII.GetString(responseStream.ToArray());
        } 

        string LoadProfile(){
            var responseStream = new MemoryStream();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            settings.Encoding = Encoding.ASCII;

            using (XmlWriter writer = XmlWriter.Create(responseStream, settings))
            {
                writer.WriteStartElement("h7_0");

                writer.WriteRaw(File.ReadAllText(serverDirectory + a1user.username + @"/profile"));

                writer.WriteEndElement();
                writer.Flush();
                writer.Close();
            }

            return System.Text.ASCIIEncoding.ASCII.GetString(responseStream.ToArray());
        }

        string GetLeaderboardStats(string id){
            //TODO - When we get multiplayer working, get Most Played (MULTI) added.

            var responseStream = new MemoryStream();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            settings.Encoding = Encoding.ASCII;

            int category = int.Parse(id);


            using (XmlWriter writer = XmlWriter.Create(responseStream, settings))
            {
                writer.WriteStartElement("h7_0");

                writer.WriteStartElement("gls");
                writer.WriteAttributeString("id", category.ToString());

                writer.WriteStartElement("records");
                writer.WriteAttributeString("id", category.ToString());

                XmlDocument profile = new XmlDocument();
                profile.Load(serverDirectory + a1user.username + @"/profile");
                switch(category){
                    case 1:
                        var gameNodes = profile.SelectNodes("/profile/statistics/games/game");
                        foreach (XmlNode node in gameNodes)
                        {
                            writer.WriteStartElement("record");
                            writer.WriteAttributeString("id", node.Attributes["id"].Value);
                            writer.WriteAttributeString("sp", node.Attributes["count"].Value);

                            writer.WriteEndElement();
                        }
                        break;
                    case 2:

                        var itemNodes = profile.SelectNodes("/profile/menu/items/item");
                        foreach (XmlNode node in itemNodes)
                        {
                            writer.WriteStartElement("record");
                            writer.WriteAttributeString("id", node.Attributes["id"].Value);
                            writer.WriteAttributeString("c", node.Attributes["total"].Value);
                            writer.WriteEndElement();
                        }
                        break;
                }

                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.Flush();
                writer.Close();
            }

            return System.Text.ASCIIEncoding.ASCII.GetString(responseStream.ToArray());
        } 

        //Other
        public void SendStatusUpdate(string statusHeader, string shortHeader, string status){
            string buddyList = "";
            var con = new MySqlConnection(sqServer);  
            string sql = "SELECT buddyList FROM user WHERE uID=@userID";
            MySqlCommand sqCommand = new MySqlCommand(sql,con);
            sqCommand.Parameters.AddWithValue("@userID", a1user.userID);
            con.Open();
            using (MySqlDataReader sqReader = sqCommand.ExecuteReader())
            {
                while (sqReader.Read())
                {    
                    buddyList = sqReader["buddyList"].ToString();
                }
                con.Close();
            }   
            Console.WriteLine(buddyList);
            a1user.rawBuddies = buddyList.Split(',');

            foreach(string buddy in a1user.rawBuddies){
                    if(buddy != ""){
                        int isOnline = 0;
                        string conID = "";

                        var conB = new MySqlConnection(sqServer);  
                        string sqlB = "SELECT * FROM user WHERE uID=@userID";
                        MySqlCommand sqCommandB = new MySqlCommand(sqlB,conB);
                        sqCommandB.Parameters.AddWithValue("@userID", buddy);
                        conB.Open();
                        using (MySqlDataReader sqReader = sqCommandB.ExecuteReader())
                        {

                            while (sqReader.Read())
                            {    
                                isOnline = Convert.ToInt32(sqReader["isOnline"]);;
                                if(isOnline == 1){
                                    conID = sqReader["connectionID"].ToString();
                                }
                            }

                            conB.Close();
                        }  

                        if(isOnline == 1){
                            var responseStream1 = new MemoryStream();
                            XmlWriterSettings settings = new XmlWriterSettings();
                            settings.OmitXmlDeclaration = true;
                            settings.ConformanceLevel = ConformanceLevel.Fragment;
                            settings.Encoding = Encoding.ASCII;
                            using (XmlWriter writer1 = XmlWriter.Create(responseStream1, settings))
                            {
                                writer1.WriteStartElement(statusHeader);

                                writer1.WriteAttributeString(shortHeader, status);

                                writer1.WriteAttributeString("id", a1user.userID.ToString());

                                writer1.WriteEndElement();
                                writer1.Flush();
                                writer1.Close();
                            }
                            a1_Sender.SendToUser(this.Server, conID, System.Text.ASCIIEncoding.ASCII.GetString(responseStream1.ToArray()));
                        }
                    }
            }
        }

        protected override void OnConnected()
        {
            Console.WriteLine($"[0 - System] TCP session with Id {Id} connected!");
        }

        protected override void OnDisconnected()
        {
            Console.WriteLine($"[0 - System] TCP session with Id {Id} disconnected!");
            
            if(a1user.username != null){
                a1user.isOnline = 0;
                var con = new MySqlConnection(sqServer);  
                string sql1 = "UPDATE user SET isOnline = @online WHERE uID=@userID";
                MySqlCommand onlineSet = new MySqlCommand(sql1,con);
                onlineSet.Parameters.AddWithValue("@userID", a1user.userID);
                onlineSet.Parameters.AddWithValue("@online", "0");
                con.Open();
                onlineSet.ExecuteNonQuery();
                con.Close();

                SendStatusUpdate("u_cos", "o", "0");
            }
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

    string serverDirectory = "";

    public ServerSystem(IPAddress address, int port, string sqServerInput, string directory) : base(address, port) 
    {
        serverPort = port; 
        sqServer = sqServerInput; 
        serverDirectory = directory; 
        this.OptionReceiveBufferSize = 51200; //FK sends very large packets for saves, bigger than the NCS default of 8192.
        this.OptionKeepAlive = true;
    }

    protected override TcpSession CreateSession() { return new A1_System(this, serverPort, sqServer, serverDirectory); }

    protected override void OnError(SocketError error)
    {
        Console.WriteLine($"[0 - System] TCP server caught an error with code {error}");
    }
}