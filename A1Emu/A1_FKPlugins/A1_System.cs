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

    FKUser a1_User;

    int sessionPort = 80;

    string sqServer = "";

    string serverDirectory = "";

    int chunksLeft = 0;

    string saveData = "";

    string opponentConID = "";
    int opponentUID = 0;
    int teamSide = 5;
    int roundCount = 0;

    public A1_System(TcpServer server, int port, string sqServerInput, string directory) : base(server)
    {
        a1_Parser = new A1_Parser();
        a1_Sender = new A1_Sender();
        a1_User = new FKUser();
        sessionPort = port; sqServer = sqServerInput;
        serverDirectory = directory;
    }

    protected override void OnReceived(byte[] buffer, long offset, long size)
    {
        string message = Encoding.ASCII.GetString(buffer, (int)offset, (int)size - 1);
        Console.WriteLine("[0 - System - " + this.Id + "] Recieved: " + message);

        List<string> responses = new List<string>();

        //Initializes a buddy list if null.
        if (a1_User.buddies == null)
        {
            a1_User.buddies = new List<FKUser>();
        }

        string[] commands = a1_Parser.ParseReceivedMessage(message);
        string[] routingString = a1_Parser.ParseRoutingStrings(message);
        foreach (string command in commands)
        {
            string[] commandInfo = a1_Parser.ParseCommand(command);
            switch (commandInfo[0])
            {
                // ----------------------------- Plugin 0 (Core) ---------------------------- \\
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

                // ----------------------------- Plugin 1 (User) ---------------------------- \\
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
                case "u_inv":
                    responses.Add(InvitePlayer(commandInfo[1], commandInfo[2], commandInfo[3], commandInfo[4], commandInfo[5], commandInfo[6]));
                    break;

                // ---------------------------- Plugin 5 (Soccer) --------------------------- \\
                case "cm":
                    responses.Add(CharacterMove(commandInfo[1], commandInfo[2], commandInfo[3]));
                    break;
                case "bs":
                    responses.Add(BlockShot(commandInfo[1], commandInfo[2], commandInfo[3], commandInfo[4], commandInfo[5]));
                    break;

                // ---------------------------- Plugin 7 (Galaxy) --------------------------- \\
                case "lpv":
                    responses.Add(LoadProfileVersion());
                    break;
                case "vsu":
                    responses.Add(VersionStatisticsRequest(commandInfo[1]));
                    break;
                case "sp":
                    if(routingString[1] == "7")
                        responses.Add(SaveProfile(commandInfo[1]));
                    else if(routingString[1] == "5")
                        responses.Add(ShotParameters(commandInfo[1], commandInfo[2], commandInfo[3], commandInfo[4], commandInfo[5]));
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

                // ----------------------- Multiplayer (Shared by all) ---------------------- \\
                case "jn":
                    //Determines which plugin/game to join.
                    switch(routingString[1])
                    {
                        //Chat
                        case "2":
                            responses.Add(JoinChat(commandInfo[1], commandInfo[2], commandInfo[3], commandInfo[4], commandInfo[5]));
                            break;
                        //General Multiplayer
                        default:
                            responses.Add(JoinGame(commandInfo[1], commandInfo[2], routingString[1]));
                            break;
                    }
                    break;

                case "lv":
                    responses.Add(LeaveGame(commandInfo[1], routingString[1]));
                    break;
                    
                case "rp":
                    responses.Add(ReadyPlay(commandInfo[1], routingString[1]));
                    break;
                
                case "ms":
                    responses.Add(MessageOpponent(commandInfo[1], commandInfo[2], routingString[1]));
                    break;

                default:
                    responses.Add(@"<unknown />");
                    break;
            }
        }

        foreach (string responseString in responses)
        {
            //Formats the reponse to send.
            List<byte[]> d = new List<byte[]>();
            Byte[] reply = Encoding.ASCII.GetBytes(responseString);
            byte[] b2 = new byte[] { 0x00 };
            d.Add(reply);
            d.Add(b2);
            byte[] b3 = d.SelectMany(a => a).ToArray();

            Console.WriteLine(responseString);
            Send(b3, 0, b3.Length);
        }
    }

    // -------------------------------------------------------------------------- \\
    //                               Plugin 0 - Core                              \\
    // -------------------------------------------------------------------------- \\

    string LoginGuestUser(string d, string a, string c)
    {
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

    string GetPluginDetails(string p)
    {
        var responseStream = new MemoryStream();

        string serverID = "1";

        string xIPAddress = "localhost";
        string xPort = "80";

        string bIPAddress = "localhost";
        string bPort = "80";

        switch (p)
        {
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

        switch (s)
        {
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
        var encryptedP = new Rfc2898DeriveBytes(p, salt, 10000);

        byte[] hash = encryptedP.GetBytes(20);
        byte[] hashBytes = new byte[36];

        Array.Copy(salt, 0, hashBytes, 0, 16);
        Array.Copy(hash, 0, hashBytes, 16, 20);
        string password = Convert.ToBase64String(hashBytes);

        //Setting up SQL connection.
        FKUser user = new FKUser();
        var con1 = new MySqlConnection(sqServer);

        string sql1 = "SELECT * FROM user WHERE u=@Uname";
        MySqlCommand sqCommand = new MySqlCommand(sql1, con1);
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
        if (user.username != null)
        {
            //If username is claimed.
            resultCode = 1;
        }
        else
        {
            //Adds user to the DB.
            int uID = 0;
            using (var con = new MySqlConnection(sqServer))
            {
                con.Open();

                using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM user", con))
                {
                    uID = Convert.ToInt32(cmd.ExecuteScalar());
                }

                var sql = "INSERT INTO user(u, p, sq, sa, uID, phoneStatus, chatStatus) VALUES(@user, @pass, @secQ, @secA, @userID, @ps, @cs)";
                using (var cmd = new MySqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@user", l);
                    cmd.Parameters.AddWithValue("@pass", password);
                    cmd.Parameters.AddWithValue("@secQ", sq);
                    cmd.Parameters.AddWithValue("@secA", sa);
                    cmd.Parameters.AddWithValue("@userID", uID);
                    cmd.Parameters.AddWithValue("@ps", 0);
                    cmd.Parameters.AddWithValue("@cs", 0);
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
        MySqlCommand sqCommand = new MySqlCommand(sql, con);
        sqCommand.Parameters.AddWithValue("@Uname", n);
        con.Open();
        using (MySqlDataReader sqReader = sqCommand.ExecuteReader())
        {
            while (sqReader.Read())
            {
                a1_User.username = sqReader["u"].ToString();
                a1_User.password = sqReader["p"].ToString();
                a1_User.userID = Int32.Parse(sqReader["uID"].ToString());
            }
            con.Close();
        }
        if (a1_User.username != null)
        {
            //If user exists.
            try
            {
                //Getting password ready for check.
                byte[] hashBytes = Convert.FromBase64String(a1_User.password);
                byte[] salt = new byte[16];
                Array.Copy(hashBytes, 0, salt, 0, 16);
                var encryptedP = new Rfc2898DeriveBytes(p, salt, 10000);
                byte[] hash = encryptedP.GetBytes(20);

                bool correct = true;
                for (int iC = 0; iC < 20; iC++)
                {
                    if (hashBytes[iC + 16] != hash[iC])
                    {
                        correct = false;
                    }
                }
                if (correct)
                {
                    //Password correct.
                    resultCode = 0;
                    string sqlID = "UPDATE user SET connectionID = @cID WHERE uID=@userID";
                    MySqlCommand sqCommandID = new MySqlCommand(sqlID, con);
                    sqCommandID.Parameters.AddWithValue("@userID", a1_User.userID);
                    sqCommandID.Parameters.AddWithValue("@cID", this.Id);
                    con.Open();
                    sqCommandID.ExecuteNonQuery();
                    con.Close();
                }
                else
                {
                    //Password incorrect.
                    resultCode = 4;
                }
            }
            catch
            {
                //If the check fails.
                resultCode = 6;
            }
        }
        else
        {
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

            if (resultCode == 0)
            {
                //User ID
                writer.WriteAttributeString("u", a1_User.userID.ToString());
            }

            //service?
            writer.WriteAttributeString("s", "1");

            writer.WriteEndElement();
            writer.Flush();
            writer.Close();
        }

        return System.Text.ASCIIEncoding.UTF8.GetString(responseStream.ToArray());
    }

    // -------------------------------------------------------------------------- \\
    //                              Plugin 1 - Users                              \\
    // -------------------------------------------------------------------------- \\

    string GetBuddyList()
    {
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
            MySqlCommand sqCommand = new MySqlCommand(sql, con);
            sqCommand.Parameters.AddWithValue("@userID", a1_User.userID);
            con.Open();
            using (MySqlDataReader sqReader = sqCommand.ExecuteReader())
            {
                while (sqReader.Read())
                {
                    buddyList = sqReader["buddyList"].ToString();
                }
                con.Close();
            }
            a1_User.rawBuddies = buddyList.Split(',');

            //Skips NULL or blank buddy lists.
            if (buddyList != "")
            {
                foreach (string buddy in a1_User.rawBuddies)
                {
                    var conB = new MySqlConnection(sqServer);
                    string sqlB = "SELECT * FROM user WHERE uID=@userID";
                    MySqlCommand sqCommandB = new MySqlCommand(sqlB, conB);
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
                            if (sqReader["phoneStatus"].ToString() != "")
                            {
                                buddyUser.phoneStatus = Convert.ToInt32(sqReader["phoneStatus"]);
                            }
                            else
                            {
                                buddyUser.phoneStatus = 0;
                            }

                            if (buddyUser.isOnline == 1)
                            {
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

            //The client doesn't set online status. So the server sends it when it gets all of the user's buddies.
            a1_User.isOnline = 1;
            string sql1 = "UPDATE user SET isOnline = @online WHERE uID=@userID";
            MySqlCommand onlineSet = new MySqlCommand(sql1, con);
            onlineSet.Parameters.AddWithValue("@userID", a1_User.userID);
            onlineSet.Parameters.AddWithValue("@online", "1");
            con.Open();
            onlineSet.ExecuteNonQuery();
            con.Close();

            if (buddyList != "")
                SendStatusUpdate("u_cos", "o", a1_User.isOnline.ToString());

            writer.WriteEndElement();
            writer.Flush();
            writer.Close();
        }

        return System.Text.ASCIIEncoding.ASCII.GetString(responseStream.ToArray());
    }

    string ChangeChatStatus(string s)
    {
        /* Chat Statuses
        0 - Ready to Party
        1 - Do Not Disturb
        2 - Playing
        3 - Partying */

        a1_User.status = int.Parse(s);

        var con = new MySqlConnection(sqServer);
        string sql1 = "UPDATE user SET chatStatus = @ccs WHERE uID=@userID";
        MySqlCommand sqCommand = new MySqlCommand(sql1, con);
        sqCommand.Parameters.AddWithValue("@userID", a1_User.userID);
        sqCommand.Parameters.AddWithValue("@ccs", a1_User.status);
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

            writer.WriteAttributeString("s", a1_User.status.ToString());

            writer.WriteAttributeString("id", a1_User.userID.ToString());

            writer.WriteEndElement();
            writer.Flush();
            writer.Close();
        }

        SendStatusUpdate("u_ccs", "s", a1_User.status.ToString());

        return System.Text.ASCIIEncoding.ASCII.GetString(responseStream.ToArray());
    }

    string ChangePhoneStatus(string ph)
    {
        /* Phone Statuses
        0 - No Cell Phone
        1 - Has Cell Phone */

        a1_User.phoneStatus = int.Parse(ph);

        var con = new MySqlConnection(sqServer);
        string sql1 = "UPDATE user SET chatStatus = @ccs WHERE uID=@userID";
        MySqlCommand sqCommand = new MySqlCommand(sql1, con);
        sqCommand.Parameters.AddWithValue("@userID", a1_User.userID);
        sqCommand.Parameters.AddWithValue("@ccs", a1_User.status);
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

            writer.WriteAttributeString("ph", a1_User.phoneStatus.ToString());

            writer.WriteAttributeString("id", a1_User.userID.ToString());

            writer.WriteEndElement();
            writer.Flush();
            writer.Close();
        }

        SendStatusUpdate("u_cph", "ph", a1_User.phoneStatus.ToString());

        return System.Text.ASCIIEncoding.ASCII.GetString(responseStream.ToArray());
    }

    string SendPrivateMessage(string m, string t, string f)
    {
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
        MySqlCommand sqCommandB = new MySqlCommand(sqlB, conB);
        sqCommandB.Parameters.AddWithValue("@userID", t);
        conB.Open();
        using (MySqlDataReader sqReader = sqCommandB.ExecuteReader())
        {
            FKUser buddyUser = new FKUser();

            while (sqReader.Read())
            {
                buddyUser.isOnline = Convert.ToInt32(sqReader["isOnline"]);
                if (buddyUser.isOnline == 1)
                {
                    conID = sqReader["connectionID"].ToString();
                }
            }

            conB.Close();
        }

        a1_Sender.SendToUser(this.Server, conID, System.Text.ASCIIEncoding.ASCII.GetString(responseStream.ToArray()));

        return System.Text.ASCIIEncoding.ASCII.GetString(responseStream.ToArray());
    }

    string AddBuddy(string n)
    {
        FKUser buddy = new FKUser();
        var con = new MySqlConnection(sqServer);

        string sql = "SELECT * FROM user WHERE u=@Uname";
        MySqlCommand sqCommand = new MySqlCommand(sql, con);
        sqCommand.Parameters.AddWithValue("@Uname", n);
        con.Open();
        using (MySqlDataReader sqReader = sqCommand.ExecuteReader())
        {
            while (sqReader.Read())
            {
                buddy.username = sqReader["u"].ToString();
                try
                {
                    buddy.userID = int.Parse(sqReader["uID"].ToString());
                    buddy.status = int.Parse(sqReader["chatStatus"].ToString());
                    buddy.isOnline = int.Parse(sqReader["isOnline"].ToString());
                    if (buddy.isOnline == 1)
                    {
                        buddy.connectionID = sqReader["connectionID"].ToString();
                    }
                }
                catch
                {

                }
            }
            con.Close();
        }

        bool fail = false;
        bool isAlreadyBuddy = false;
        foreach (string buddyItem in a1_User.rawBuddies) { if (buddyItem == buddy.userID.ToString()) { isAlreadyBuddy = true; } }

        var responseStream = new MemoryStream();
        XmlWriterSettings settings = new XmlWriterSettings();
        settings.OmitXmlDeclaration = true;
        settings.ConformanceLevel = ConformanceLevel.Fragment;
        settings.Encoding = Encoding.ASCII;
        using (XmlWriter writer = XmlWriter.Create(responseStream, settings))
        {
            writer.WriteStartElement("u_abd");

            if (buddy.isOnline == 0)
            {
                writer.WriteAttributeString("r", "5");
                fail = true;
            }
            else if (isAlreadyBuddy)
            {
                writer.WriteAttributeString("r", "3");
                fail = true;
            }
            else if (buddy.username == null || buddy.username == "GUESTUSER")
            {
                writer.WriteAttributeString("r", "2");
                fail = true;
            }

            writer.WriteAttributeString("n", n);

            writer.WriteEndElement();
            writer.Flush();
            writer.Close();
        }
        if (fail)
        {
            return System.Text.ASCIIEncoding.ASCII.GetString(responseStream.ToArray());
        }
        else
        {
            var responseStream1 = new MemoryStream();
            using (XmlWriter writer1 = XmlWriter.Create(responseStream1, settings))
            {
                writer1.WriteStartElement("u_abr");

                writer1.WriteAttributeString("b", a1_User.userID.ToString());

                writer1.WriteAttributeString("n", a1_User.username.ToString());

                writer1.WriteEndElement();
                writer1.Flush();
                writer1.Close();
            }
            a1_Sender.SendToUser(this.Server, buddy.connectionID, System.Text.ASCIIEncoding.ASCII.GetString(responseStream1.ToArray()));
            return "<notneeded/>";
        }
    }

    string AddBuddyResponse(string r, string n)
    {
        bool accepted = false;

        FKUser buddy = new FKUser();
        var con = new MySqlConnection(sqServer);

        string sql = "SELECT * FROM user WHERE u=@Uname";
        MySqlCommand sqCommand = new MySqlCommand(sql, con);
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
                if (sqReader["phoneStatus"].ToString() != "")
                {
                    buddy.phoneStatus = int.Parse(sqReader["phoneStatus"].ToString());
                }
                else
                {
                    buddy.phoneStatus = 0;
                }
                if (buddy.isOnline == 1)
                {
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

            if (r == "1")
            {
                accepted = true;
                writer.WriteAttributeString("r", "0");
                string sql1 = "UPDATE user SET buddyList = CONCAT_WS(',', buddyList, @buddy) WHERE uID=@userID";
                MySqlCommand sqCommand1 = new MySqlCommand(sql1, con);
                sqCommand1.Parameters.AddWithValue("@userID", a1_User.userID.ToString());
                sqCommand1.Parameters.AddWithValue("@buddy", buddy.userID.ToString());
                con.Open();
                sqCommand1.ExecuteNonQuery();
                con.Close();

                string sql2 = "UPDATE user SET buddyList = CONCAT_WS(',', buddyList, @userID) WHERE uID=@buddy";
                MySqlCommand sqCommand2 = new MySqlCommand(sql2, con);
                sqCommand2.Parameters.AddWithValue("@userID", a1_User.userID.ToString());
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

                if (r == "1")
                {
                    writer1.WriteAttributeString("r", "0");
                    writer1.WriteAttributeString("ph", a1_User.phoneStatus.ToString());
                    writer1.WriteAttributeString("s", a1_User.status.ToString());
                    writer1.WriteAttributeString("o", a1_User.isOnline.ToString());
                }
                else
                {
                    writer1.WriteAttributeString("r", "4");
                }

                writer1.WriteAttributeString("b", a1_User.userID.ToString());

                writer1.WriteAttributeString("n", a1_User.username);

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

        if (accepted)
        {
            return System.Text.ASCIIEncoding.ASCII.GetString(responseStream.ToArray());
        }
        else
        {
            return "<notneeded/>";
        }

    }

    string DeleteBuddy(string b)
    {
        FKUser buddy = new FKUser();
        var con = new MySqlConnection(sqServer);

        string sql = "SELECT * FROM user WHERE uID=@userID";
        MySqlCommand sqCommand = new MySqlCommand(sql, con);
        sqCommand.Parameters.AddWithValue("@userID", b);
        con.Open();
        using (MySqlDataReader sqReader = sqCommand.ExecuteReader())
        {
            while (sqReader.Read())
            {
                buddy.username = sqReader["u"].ToString();
                try
                {
                    buddy.userID = int.Parse(sqReader["uID"].ToString());
                    buddy.status = int.Parse(sqReader["chatStatus"].ToString());
                    buddy.isOnline = int.Parse(sqReader["isOnline"].ToString());
                    buddy.rawBuddies = sqReader["buddyList"].ToString().Split(',');
                    if (buddy.isOnline == 1)
                    {
                        buddy.connectionID = sqReader["connectionID"].ToString();
                    }
                }
                catch
                {

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

            if (buddy.username == null || buddy.username == "GUESTUSER")
            {
                writer.WriteAttributeString("r", "2");
                fail = true;
            }

            writer.WriteAttributeString("u", a1_User.userID.ToString());

            writer.WriteAttributeString("b", b);

            writer.WriteEndElement();
            writer.Flush();
            writer.Close();
        }
        if (fail)
        {
            return System.Text.ASCIIEncoding.ASCII.GetString(responseStream.ToArray());
        }
        else
        {
            var responseStream1 = new MemoryStream();
            using (XmlWriter writer1 = XmlWriter.Create(responseStream1, settings))
            {
                writer1.WriteStartElement("u_dbr");

                writer1.WriteAttributeString("b", a1_User.userID.ToString());

                writer1.WriteAttributeString("n", a1_User.username.ToString());

                writer1.WriteEndElement();
                writer1.Flush();
                writer1.Close();
            }

            if (buddy.isOnline == 1)
            {
                a1_Sender.SendToUser(this.Server, buddy.connectionID, System.Text.ASCIIEncoding.ASCII.GetString(responseStream1.ToArray()));
            }


            var buddyList = new List<string>(a1_User.rawBuddies);
            buddyList.Remove(buddy.userID.ToString());
            a1_User.rawBuddies = buddyList.ToArray();
            string buddies = String.Join(",", a1_User.rawBuddies);
            string sql1 = "UPDATE user SET buddyList = @buddies WHERE uID=@userID";
            MySqlCommand sqCommand1 = new MySqlCommand(sql1, con);
            sqCommand1.Parameters.AddWithValue("@userID", a1_User.userID.ToString());
            if (buddies != "")
            {
                sqCommand1.Parameters.AddWithValue("@buddies", buddies);
            }
            else
            {
                sqCommand1.Parameters.AddWithValue("@buddies", DBNull.Value);
            }
            con.Open();
            sqCommand1.ExecuteNonQuery();
            con.Close();

            buddyList = new List<string>(buddy.rawBuddies);
            buddyList.Remove(a1_User.userID.ToString());
            buddy.rawBuddies = buddyList.ToArray();
            buddies = String.Join(",", buddy.rawBuddies);
            string sql2 = "UPDATE user SET buddyList = @buddies WHERE uID=@buddy";
            MySqlCommand sqCommand2 = new MySqlCommand(sql2, con);
            if (buddies != "")
            {
                sqCommand2.Parameters.AddWithValue("@buddies", buddies);
            }
            else
            {
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

                writer1.WriteAttributeString("u", a1_User.userID.ToString());

                writer1.WriteAttributeString("b", b);

                writer1.WriteEndElement();
                writer1.Flush();
                writer1.Close();
            }

            return System.Text.ASCIIEncoding.ASCII.GetString(responseStream2.ToArray());
        }
    }

    string DeleteBuddyResponse(string r, string n, string b)
    {
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

            writer.WriteAttributeString("u", a1_User.userID.ToString());

            writer.WriteAttributeString("b", b);

            writer.WriteEndElement();
            writer.Flush();
            writer.Close();
        }

        return System.Text.ASCIIEncoding.ASCII.GetString(responseStream.ToArray());
    }

    string InvitePlayer(string av, string gid, string bid, string p, string t, string f)
    {
        var responseStream = new MemoryStream();
        XmlWriterSettings settings = new XmlWriterSettings();
        settings.OmitXmlDeclaration = true;
        settings.ConformanceLevel = ConformanceLevel.Fragment;
        settings.Encoding = Encoding.ASCII;
        using (XmlWriter writer = XmlWriter.Create(responseStream, settings))
        {
            writer.WriteStartElement("u_inv");

            writer.WriteAttributeString("r", "0");

            writer.WriteAttributeString("av", av);

            writer.WriteAttributeString("gid", gid);

            writer.WriteAttributeString("bid", bid);

            writer.WriteAttributeString("p", p);

            writer.WriteAttributeString("t", t);

            writer.WriteAttributeString("f", f);

            writer.WriteEndElement();
            writer.Flush();
            writer.Close();
        }

        string conID = "";
        int buddyID = 0;
        string buddyUsername = "";

        var conB = new MySqlConnection(sqServer);
        string sqlB = "SELECT * FROM user WHERE uID=@userID";
        MySqlCommand sqCommandB = new MySqlCommand(sqlB, conB);
        sqCommandB.Parameters.AddWithValue("@userID", t);
        conB.Open();
        using (MySqlDataReader sqReader = sqCommandB.ExecuteReader())
        {
            FKUser buddyUser = new FKUser();

            while (sqReader.Read())
            {
                buddyID = Convert.ToInt32(sqReader["uID"]);
                buddyUsername = sqReader["u"].ToString();
                buddyUser.isOnline = Convert.ToInt32(sqReader["isOnline"]);
                if (buddyUser.isOnline == 1)
                {
                    conID = sqReader["connectionID"].ToString();
                }
            }

            conB.Close();
        }

        var sql = "INSERT INTO mp_5(username, userID, challenge, challenger, challengerInfo, ready) VALUES(@u, @uID, @c, @cf, @ci, @r)";
        conB.Open();
        using (var cmd = new MySqlCommand(sql, conB))
        {
            cmd.Parameters.AddWithValue("@u", buddyUsername);
            cmd.Parameters.AddWithValue("@uID", buddyID);
            cmd.Parameters.AddWithValue("@c", 1);
            cmd.Parameters.AddWithValue("@cf", f);
            cmd.Parameters.AddWithValue("@ci", av + "|0");
            cmd.Parameters.AddWithValue("@r", 0);
            cmd.Prepare();

            cmd.ExecuteNonQuery();
            cmd.Dispose();
        }
        conB.Close();

        string sql1 = "UPDATE mp_5 SET challenger = @cf WHERE userID=@userID";
        MySqlCommand sqCommand = new MySqlCommand(sql1, conB);
        sqCommand.Parameters.AddWithValue("@userID", a1_User.userID);
        sqCommand.Parameters.AddWithValue("@cf", t);
        conB.Open();
        sqCommand.ExecuteNonQuery();
        conB.Close();

        opponentConID = conID;
        opponentUID = int.Parse(t);

        a1_Sender.SendToUser(this.Server, conID, System.Text.ASCIIEncoding.ASCII.GetString(responseStream.ToArray()));

        return "<notneeded/>";
    }

    // -------------------------------------------------------------------------- \\
    //                               Plugin 2 - Chat                              \\
    // -------------------------------------------------------------------------- \\

    //<jn t="3"><pr dl="1| | " f="000000CD" uid="2" n="FUNKEY" /></jn>1|2|2|0#

    string JoinChat(string t, string dl, string f, string uid, string n)
    {
        //TODO - Figure out the database setup for the chatrooms.

        var con = new MySqlConnection(sqServer);
        string sql = "SELECT COUNT(*) FROM user WHERE currentChat = @chatRoom";
        MySqlCommand sqCommand = new MySqlCommand(sql, con);
        sqCommand.Parameters.AddWithValue("@chatRoom", t);
        con.Open();
        Int64 userCount = (Int64) sqCommand.ExecuteScalar();

        int roomCount = Convert.ToInt32(userCount) / 7 + 1;
        Console.WriteLine(roomCount);

        string sql1 = "UPDATE user SET currentChat = @chatType WHERE uID=@userID";
        MySqlCommand sqCommand1 = new MySqlCommand(sql1, con);
        sqCommand1.Parameters.AddWithValue("@chatType", t);
        sqCommand1.Parameters.AddWithValue("@userID", a1_User.userID);
        sqCommand1.ExecuteNonQuery();

        if(Convert.ToInt32(userCount) % 7 == 0)
        {
            Console.WriteLine("New Room");
            string sql2 = "UPDATE user SET currentRoom = @room WHERE uID=@userID";
            MySqlCommand sqCommand2 = new MySqlCommand(sql2, con);
            sqCommand2.Parameters.AddWithValue("@room", roomCount + 1);
            sqCommand2.Parameters.AddWithValue("@userID", a1_User.userID);
            sqCommand2.ExecuteNonQuery();
        }else{
            Console.WriteLine("Joining Room");
            string sql2 = "UPDATE user SET currentRoom = @room WHERE uID=@userID";
            MySqlCommand sqCommand2 = new MySqlCommand(sql2, con);
            sqCommand2.Parameters.AddWithValue("@room", roomCount);
            sqCommand2.Parameters.AddWithValue("@userID", a1_User.userID);
            sqCommand2.ExecuteNonQuery();
        }
        
        con.Close();

        //! - Placeholder, needs to be filled with all the players.
        var responseStream = new MemoryStream();
        XmlWriterSettings settings = new XmlWriterSettings();
        settings.OmitXmlDeclaration = true;
        settings.ConformanceLevel = ConformanceLevel.Fragment;
        settings.Encoding = Encoding.ASCII;
        using (XmlWriter writer = XmlWriter.Create(responseStream, settings))
        {
            writer.WriteStartElement("h2_0");
            writer.WriteStartElement("pj");

            writer.WriteStartElement("pr");
            writer.WriteAttributeString("f", f);
            writer.WriteAttributeString("uid", a1_User.userID.ToString());
            writer.WriteAttributeString("n", a1_User.username.ToString());
            writer.WriteEndElement();

            writer.WriteEndElement();

            writer.WriteStartElement("jn");
            writer.WriteAttributeString("r", "0");
            writer.WriteAttributeString("id", roomCount.ToString());
            writer.WriteEndElement();
            
            writer.WriteEndElement();
            writer.Flush();
            writer.Close();
        }

        var responseStream1 = new MemoryStream();
        using (XmlWriter writer = XmlWriter.Create(responseStream1, settings))
        {
            writer.WriteStartElement("h2_0");
            writer.WriteStartElement("pj");

            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.Flush();
            writer.Close();
        }

        return System.Text.ASCIIEncoding.ASCII.GetString(responseStream.ToArray());
    }

    // -------------------------------------------------------------------------- \\
    //                            Multiplayer (Generic)                           \\
    // -------------------------------------------------------------------------- \\

    string JoinGame(string pr, string c, string plugin)
    {
        var responseStream = new MemoryStream();
        XmlWriterSettings settings = new XmlWriterSettings();
        settings.OmitXmlDeclaration = true;
        settings.ConformanceLevel = ConformanceLevel.Fragment;
        settings.Encoding = Encoding.ASCII;
        using (XmlWriter writer = XmlWriter.Create(responseStream, settings))
        {
            writer.WriteStartElement("h" + plugin + "_0");

            writer.WriteStartElement("jn");
            writer.WriteAttributeString("r", "0");
            writer.WriteEndElement();
            
            writer.WriteEndElement();
            writer.Flush();
            writer.Close();
        }

        bool exists = true;

        int challenge = 0;
        int challenger = 0;
        teamSide = 5;

        var con = new MySqlConnection(sqServer);
        string sql = "SELECT * FROM mp_5 WHERE userID=@userID";
        MySqlCommand sqCommand = new MySqlCommand(sql, con);
        sqCommand.Parameters.AddWithValue("@userID", a1_User.userID);
        con.Open();
        using (MySqlDataReader sqReader = sqCommand.ExecuteReader())
        {
            while (sqReader.Read())
            {   
                try{
                challenge = int.Parse(sqReader["challenge"].ToString());
                challenger = int.Parse(sqReader["challenger"].ToString());
                }catch{exists = false;}
            }
            con.Close();
        }

        Console.WriteLine(challenge);

        if(!exists || challenger == 0)
        {
            var sql1 = "INSERT INTO mp_5(username, userID, challenge, connectionID, ready) VALUES(@u, @uID, @c, @cID, @r)";
            con.Open();
            using (var cmd = new MySqlCommand(sql1, con))
            {
                cmd.Parameters.AddWithValue("@u", a1_User.username);
                cmd.Parameters.AddWithValue("@uID", a1_User.userID);
                cmd.Parameters.AddWithValue("@c", c);
                cmd.Parameters.AddWithValue("@cID", this.Id);
                cmd.Parameters.AddWithValue("@r", 0);

                cmd.Prepare();

                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
            con.Close();
        }

        if(challenge == 1)
        {
            string conID = "";
            string opponentName = "";
            string opponentInfo = "";

            string sqlB = "SELECT * FROM user WHERE uID=@userID";
            MySqlCommand sqCommandB = new MySqlCommand(sqlB, con);
            sqCommandB.Parameters.AddWithValue("@userID", challenger.ToString());
            con.Open();
            using (MySqlDataReader sqReader = sqCommandB.ExecuteReader())
            {
                FKUser opponent = new FKUser();

                while (sqReader.Read())
                {
                    opponentName = sqReader["u"].ToString();
                    opponent.isOnline = Convert.ToInt32(sqReader["isOnline"]);
                    if (opponent.isOnline == 1)
                    {
                        conID = sqReader["connectionID"].ToString();
                    }
                }

                con.Close();
            }

            string sqlC = "SELECT * FROM mp_5 WHERE userID=@userID";
            MySqlCommand sqCommandC = new MySqlCommand(sqlC, con);
            sqCommandC.Parameters.AddWithValue("@userID", a1_User.userID.ToString());
            con.Open();
            using (MySqlDataReader sqReader = sqCommandC.ExecuteReader())
            {

                while (sqReader.Read())
                {
                    opponentInfo = sqReader["challengerInfo"].ToString();
                }

                con.Close();
            }

            string sql1 = "UPDATE mp_5 SET challengerInfo = @ci WHERE userID=@userID";
            MySqlCommand sqCommand1 = new MySqlCommand(sql1, con);
            sqCommand1.Parameters.AddWithValue("@userID", challenger);
            sqCommand1.Parameters.AddWithValue("@ci", pr);
            con.Open();
            sqCommand1.ExecuteNonQuery();
            con.Close();

            string sql2 = "UPDATE mp_5 SET connectionID = @cID WHERE userID=@userID";
            MySqlCommand sqCommand2 = new MySqlCommand(sql2, con);
            sqCommand2.Parameters.AddWithValue("@userID", a1_User.userID);
            sqCommand2.Parameters.AddWithValue("@cID", this.Id);
            con.Open();
            sqCommand2.ExecuteNonQuery();
            con.Close();

            opponentConID = conID;
            opponentUID = challenger;

            var responseStream1 = new MemoryStream();
            using (XmlWriter writer = XmlWriter.Create(responseStream1, settings))
            {
                writer.WriteStartElement("h" + plugin + "_0");

                writer.WriteStartElement("oj");
                writer.WriteAttributeString("n", a1_User.username);
                writer.WriteAttributeString("pr", pr);
                writer.WriteEndElement();
                
                writer.WriteEndElement();
                writer.Flush();
                writer.Close();
            }
            a1_Sender.SendToUser(this.Server, conID, System.Text.ASCIIEncoding.ASCII.GetString(responseStream1.ToArray()));

            var responseStream2 = new MemoryStream();
            using (XmlWriter writer = XmlWriter.Create(responseStream2, settings))
            {
                writer.WriteStartElement("h" + plugin + "_0");

                writer.WriteStartElement("jn");
                writer.WriteAttributeString("r", "0");
                writer.WriteEndElement();
            
                writer.WriteStartElement("oj");
                writer.WriteAttributeString("n", opponentName);
                writer.WriteAttributeString("pr", opponentInfo);
                writer.WriteEndElement();
                
                writer.WriteEndElement();
                writer.Flush();
                writer.Close();
            }
            return System.Text.ASCIIEncoding.ASCII.GetString(responseStream2.ToArray());
        }

        return System.Text.ASCIIEncoding.ASCII.GetString(responseStream.ToArray());
    }

    string ReadyPlay(string bid, string plugin)
    {
        var con = new MySqlConnection(sqServer);

        string sql1 = "UPDATE mp_5 SET ready = @r WHERE userID=@userID";
        MySqlCommand sqCommand1 = new MySqlCommand(sql1, con);
        sqCommand1.Parameters.AddWithValue("@r", 1);
        sqCommand1.Parameters.AddWithValue("@userID", a1_User.userID);
        con.Open();
        sqCommand1.ExecuteNonQuery();
        con.Close();

        var responseStream = new MemoryStream();
        XmlWriterSettings settings = new XmlWriterSettings();
        settings.OmitXmlDeclaration = true;
        settings.ConformanceLevel = ConformanceLevel.Fragment;
        settings.Encoding = Encoding.ASCII;
        using (XmlWriter writer = XmlWriter.Create(responseStream, settings))
        {
            bool isStartingGame = teamSide == 5;

            writer.WriteStartElement("h" + plugin + "_0");

            if(plugin == "5")
            {   
                writer.WriteStartElement("oc");
                if(teamSide == 5)
                {
                    if(a1_User.userID < opponentUID)
                    {
                        teamSide = 0;
                        writer.WriteAttributeString("c", "0");
                    }else{   
                        teamSide = 1;
                        writer.WriteAttributeString("c", "1");
                    }
                }else{
                    if(roundCount % 5 == 0)
                    {
                        switch(teamSide)
                        {
                            case 0:
                                teamSide = 1;
                                writer.WriteAttributeString("c", "1");
                                break;
                            case 1:
                                teamSide = 0;
                                writer.WriteAttributeString("c", "0");
                                break;
                        }
                    }else
                    {
                        switch(teamSide)
                        {
                            case 0:
                                writer.WriteAttributeString("c", "0");
                                break;
                            case 1:
                                writer.WriteAttributeString("c", "1");
                                break;
                        }
                    }
                }
                writer.WriteEndElement();

                writer.WriteStartElement("cc");
                if(teamSide == 5)
                {
                    if(a1_User.userID < opponentUID)
                    {
                        writer.WriteAttributeString("c", "1");
                    }else
                    {
                        writer.WriteAttributeString("c", "0");
                    }
                }else{
                    switch(teamSide)
                    {
                        case 0:
                            writer.WriteAttributeString("c", "0");
                            break;
                        case 1:
                            writer.WriteAttributeString("c", "1");
                            break;
                    }
                }
                writer.WriteEndElement();
                writer.WriteStartElement("nr");
                writer.WriteEndElement();

                if(isStartingGame)
                {
                    writer.WriteStartElement("sg");
                    writer.WriteEndElement();              
                }

                roundCount += 1;
            }

            writer.WriteEndElement();
            writer.Flush();
            writer.Close();
        }

        int opponentReady = 0;

        while(opponentReady == 0)
        {
            string sql = "SELECT ready FROM mp_5 WHERE userID=@userID";
            MySqlCommand sqCommand = new MySqlCommand(sql, con);
            sqCommand.Parameters.AddWithValue("@userID", opponentUID);
            con.Open();
            using (MySqlDataReader sqReader = sqCommand.ExecuteReader())
            {
                while (sqReader.Read())
                {
                    opponentReady = int.Parse(sqReader["ready"].ToString());
                }
                con.Close();
            }

            System.Threading.Thread.Sleep(500);
        }

        string sql2 = "UPDATE mp_5 SET ready = @r WHERE userID=@userID";
        MySqlCommand sqCommand2 = new MySqlCommand(sql2, con);
        sqCommand2.Parameters.AddWithValue("@r", 0);
        sqCommand2.Parameters.AddWithValue("@userID", a1_User.userID);
        con.Open();
        sqCommand2.ExecuteNonQuery();
        con.Close();

        return System.Text.ASCIIEncoding.ASCII.GetString(responseStream.ToArray());
    }
    
    string MessageOpponent(string m, string bid, string plugin)
    {
        var responseStream = new MemoryStream();
        XmlWriterSettings settings = new XmlWriterSettings();
        settings.OmitXmlDeclaration = true;
        settings.ConformanceLevel = ConformanceLevel.Fragment;
        settings.Encoding = Encoding.ASCII;
        using (XmlWriter writer = XmlWriter.Create(responseStream, settings))
        {
            writer.WriteStartElement("h" + plugin + "_0");
            writer.WriteStartElement("ms");

            writer.WriteAttributeString("n", a1_User.username);
            writer.WriteAttributeString("m", m);

            writer.WriteAttributeString("bid", bid);

            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.Flush();
            writer.Close();
        }

        a1_Sender.SendToUser(this.Server, opponentConID, System.Text.ASCIIEncoding.ASCII.GetString(responseStream.ToArray()));

        return System.Text.ASCIIEncoding.ASCII.GetString(responseStream.ToArray());
    }

    string LeaveGame(string bid, string plugin)
    {
        var responseStream = new MemoryStream();
        XmlWriterSettings settings = new XmlWriterSettings();
        settings.OmitXmlDeclaration = true;
        settings.ConformanceLevel = ConformanceLevel.Fragment;
        settings.Encoding = Encoding.ASCII;
        using (XmlWriter writer = XmlWriter.Create(responseStream, settings))
        {
            writer.WriteStartElement("h" + plugin + "_0");
            writer.WriteStartElement("lv");

            writer.WriteAttributeString("r", "1");
            writer.WriteAttributeString("bid", bid);

            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.Flush();
            writer.Close();
        }

        a1_Sender.SendToUser(this.Server, opponentConID, System.Text.ASCIIEncoding.ASCII.GetString(responseStream.ToArray()));

        var con = new MySqlConnection(sqServer);

        string sql1 = "DELETE FROM mp_5 WHERE userID=@userID";
        MySqlCommand sqCommand1 = new MySqlCommand(sql1, con);
        sqCommand1.Parameters.AddWithValue("@userID", a1_User.userID);
        con.Open();
        sqCommand1.ExecuteNonQuery();
        con.Close();

        return "<notneeded />";
    }

    // -------------------------------------------------------------------------- \\
    //                              Plugin 5 - Soccer                             \\
    // -------------------------------------------------------------------------- \\

    string CharacterMove(string x, string d, string bid)
    {
        var responseStream = new MemoryStream();
        XmlWriterSettings settings = new XmlWriterSettings();
        settings.OmitXmlDeclaration = true;
        settings.ConformanceLevel = ConformanceLevel.Fragment;
        settings.Encoding = Encoding.ASCII;
        using (XmlWriter writer = XmlWriter.Create(responseStream, settings))
        {
            writer.WriteStartElement("h5_0");

            writer.WriteStartElement("cm");

            writer.WriteAttributeString("x", x);

            writer.WriteAttributeString("d", d);

            writer.WriteAttributeString("bid", bid);

            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.Flush();
            writer.Close();
        }

        a1_Sender.SendToUser(this.Server, opponentConID, System.Text.ASCIIEncoding.ASCII.GetString(responseStream.ToArray()));

        return "<notneeded/>";
    }

    string ShotParameters(string p, string z, string y, string x, string bid)
    {
        var responseStream = new MemoryStream();
        XmlWriterSettings settings = new XmlWriterSettings();
        settings.OmitXmlDeclaration = true;
        settings.ConformanceLevel = ConformanceLevel.Fragment;
        settings.Encoding = Encoding.ASCII;
        using (XmlWriter writer = XmlWriter.Create(responseStream, settings))
        {
            writer.WriteStartElement("h5_0");

            writer.WriteStartElement("sp");

            writer.WriteAttributeString("p", p);

            writer.WriteAttributeString("z", z);

            writer.WriteAttributeString("y", y);

            writer.WriteAttributeString("x", x);

            writer.WriteAttributeString("bid", bid);

            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.Flush();
            writer.Close();
        }

        a1_Sender.SendToUser(this.Server, opponentConID, System.Text.ASCIIEncoding.ASCII.GetString(responseStream.ToArray()));

        return "<notneeded/>";
    }

    string BlockShot(string d, string lx, string m, string c, string bid)
    {
        var responseStream = new MemoryStream();
        XmlWriterSettings settings = new XmlWriterSettings();
        settings.OmitXmlDeclaration = true;
        settings.ConformanceLevel = ConformanceLevel.Fragment;
        settings.Encoding = Encoding.ASCII;
        using (XmlWriter writer = XmlWriter.Create(responseStream, settings))
        {
            writer.WriteStartElement("h5_0");

            writer.WriteStartElement("bs");

            writer.WriteAttributeString("d", d);

            writer.WriteAttributeString("lx", lx);

            writer.WriteAttributeString("m", m);

            writer.WriteAttributeString("c", c);

            writer.WriteAttributeString("bid", bid);

            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.Flush();
            writer.Close();
        }

        a1_Sender.SendToUser(this.Server, opponentConID, System.Text.ASCIIEncoding.ASCII.GetString(responseStream.ToArray()));

        return "<notneeded/>";
    }

    // -------------------------------------------------------------------------- \\
    //                              Plugin 7 - Galaxy                             \\
    // -------------------------------------------------------------------------- \\

    string LoadProfileVersion()
    {
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
            if (File.Exists(serverDirectory + a1_User.username + @"/profile"))
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(File.ReadAllText(serverDirectory + a1_User.username + @"/profile"));
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

    string VersionStatisticsRequest(string id)
    {
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

    string SaveProfile(string c)
    {
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

    string SaveProfilePart(string v, string n)
    {
        var responseStream = new MemoryStream();
        XmlWriterSettings settings = new XmlWriterSettings();
        settings.OmitXmlDeclaration = true;
        settings.ConformanceLevel = ConformanceLevel.Fragment;
        settings.Encoding = Encoding.ASCII;

        using (XmlWriter writer = XmlWriter.Create(responseStream, settings))
        {
            writer.WriteStartElement("h7_0");

            saveData = v + saveData;

            if (chunksLeft == 1)
            {
                writer.WriteStartElement("sp");

                saveData = WebUtility.HtmlDecode(saveData);

                XmlDocument save = new XmlDocument();
                save.LoadXml(saveData);
                XmlElement rootSave = save.DocumentElement;

                string profileName = rootSave.GetAttribute("gname");
                if (rootSave.GetAttribute("sid") != "")
                {
                    writer.WriteAttributeString("v", (int.Parse(rootSave.GetAttribute("sid")) + 1).ToString());
                }
                else
                {
                    writer.WriteAttributeString("v", "1");
                }

                if (!Directory.Exists(serverDirectory + profileName))
                {
                    Directory.CreateDirectory(serverDirectory + profileName);
                }
                File.WriteAllText(serverDirectory + profileName + @"/profile", saveData);
            }
            else
            {
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

    string LoadProfile()
    {
        var responseStream = new MemoryStream();
        XmlWriterSettings settings = new XmlWriterSettings();
        settings.OmitXmlDeclaration = true;
        settings.ConformanceLevel = ConformanceLevel.Fragment;
        settings.Encoding = Encoding.ASCII;

        using (XmlWriter writer = XmlWriter.Create(responseStream, settings))
        {
            writer.WriteStartElement("h7_0");

            writer.WriteRaw(File.ReadAllText(serverDirectory + a1_User.username + @"/profile"));

            writer.WriteEndElement();
            writer.Flush();
            writer.Close();
        }

        return System.Text.ASCIIEncoding.ASCII.GetString(responseStream.ToArray());
    }

    string GetLeaderboardStats(string id)
    {
        // TODO - When we get multiplayer working, get Most Played (MULTI) added.

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
            profile.Load(serverDirectory + a1_User.username + @"/profile");
            switch (category)
            {
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

    // -------------------------------------------------------------------------- \\
    //                                    Misc.                                   \\
    // -------------------------------------------------------------------------- \\

    public void SendStatusUpdate(string statusHeader, string shortHeader, string status)
    {
        string buddyList = "";
        var con = new MySqlConnection(sqServer);
        string sql = "SELECT buddyList FROM user WHERE uID=@userID";
        MySqlCommand sqCommand = new MySqlCommand(sql, con);
        sqCommand.Parameters.AddWithValue("@userID", a1_User.userID);
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
        a1_User.rawBuddies = buddyList.Split(',');

        foreach (string buddy in a1_User.rawBuddies)
        {
            if (buddy != "")
            {
                int isOnline = 0;
                string conID = "";

                var conB = new MySqlConnection(sqServer);
                string sqlB = "SELECT * FROM user WHERE uID=@userID";
                MySqlCommand sqCommandB = new MySqlCommand(sqlB, conB);
                sqCommandB.Parameters.AddWithValue("@userID", buddy);
                conB.Open();
                using (MySqlDataReader sqReader = sqCommandB.ExecuteReader())
                {

                    while (sqReader.Read())
                    {
                        isOnline = Convert.ToInt32(sqReader["isOnline"]); ;
                        if (isOnline == 1)
                        {
                            conID = sqReader["connectionID"].ToString();
                        }
                    }

                    conB.Close();
                }

                if (isOnline == 1)
                {
                    var responseStream1 = new MemoryStream();
                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.OmitXmlDeclaration = true;
                    settings.ConformanceLevel = ConformanceLevel.Fragment;
                    settings.Encoding = Encoding.ASCII;
                    using (XmlWriter writer1 = XmlWriter.Create(responseStream1, settings))
                    {
                        writer1.WriteStartElement(statusHeader);

                        writer1.WriteAttributeString(shortHeader, status);

                        writer1.WriteAttributeString("id", a1_User.userID.ToString());

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

        //The client doesn't set the status, so the server has to do it by itself.
        if (a1_User.username != null)
        {
            a1_User.isOnline = 0;
            var con = new MySqlConnection(sqServer);
            string sql1 = "UPDATE user SET isOnline = @online WHERE uID=@userID";
            MySqlCommand onlineSet = new MySqlCommand(sql1, con);
            onlineSet.Parameters.AddWithValue("@userID", a1_User.userID);
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
