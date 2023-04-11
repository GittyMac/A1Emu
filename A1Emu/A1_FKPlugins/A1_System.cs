using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Xml;
using MySqlConnector;
using NetCoreServer;

//ArkONE Plugin 0
//System

class A1_System : TcpSession
{
    A1_Parser a1_Parser;

    FKUser a1_User;

    int sessionPort = 80;

    string sqServer = "";

    string serverDirectory = "";

    int chunksLeft = 0;

    string saveData = "";

    string opponentConID = "";
    int opponentUID = 0;

    FKGamePlayer mpPlayer;
    FKGamePlayer mpRival;

    public A1_System(TcpServer server, int port, string sqServerInput, string directory) : base(server)
    {
        a1_Parser = new A1_Parser();
        a1_User = new FKUser();
        A1_Chat.server = server;
        sessionPort = port; sqServer = sqServerInput;
        serverDirectory = directory;
    }

    protected override async void OnReceived(byte[] buffer, long offset, long size)
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
        foreach (string command in commands)
        {
            string[] commandInfo = a1_Parser.ParseCommand(command);
            string[] routingString = a1_Parser.ParseRoutingStrings(command);
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
                case "u_inr":
                    responses.Add(InviteBuddyResponse(commandInfo[1], commandInfo[2], commandInfo[3], commandInfo[4], commandInfo[5], commandInfo[6]));
                    break;

                // ----------------------------- Plugin 2 (Chat) ---------------------------- \\
                case "se":
                    A1_Chat.SendSpecialEvent(a1_User, command);
                    break;

                // ---------------------------- Plugin 3 (Boxing) --------------------------- \\
                case "pe":
                    responses.Add(PlayerEvent(commandInfo[1], commandInfo[2], commandInfo[3]));
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
                case "spp":
                    responses.Add(SaveProfilePart(commandInfo[1], commandInfo[2]));
                    break;
                case "lp":
                    responses.Add(LoadProfile());
                    break;
                case "gls":
                    responses.Add(GetLeaderboardStats(commandInfo[1]));
                    break;

                // ---------------------------- Plugin 10 (Trunk) --------------------------- \\
                case "gua":
                    responses.Add(GetUserAssets());
                    break;

                case "glb":
                    responses.Add(GetLootBalance());
                    break;

                case "gsl":
                    //GetSplashList (Not needed)
                    break;
                
                case "gil":
                    responses.Add(GetProductList("i"));
                    break;

                case "gfl":
                    responses.Add(GetProductList("f"));
                    break;

                case "gcl":
                    responses.Add(GetProductList("c"));
                    break;

                case "gjl":
                    responses.Add(GetProductList("j"));
                    break;
                
                case "gml":
                    responses.Add(GetProductList("m"));
                    break;

                case "gutc":
                    responses.Add(GetUserTransactionCount());
                    break;

                case "bf":
                    responses.Add(BuyProduct(commandInfo[1], commandInfo[2], "f"));
                    break;

                case "bi":
                    responses.Add(BuyProduct(commandInfo[1], commandInfo[2], "i"));
                    break;

                case "bc":
                    responses.Add(BuyProduct(commandInfo[1], commandInfo[2], "c"));
                    break;

                case "bj":
                    responses.Add(BuyProduct(commandInfo[1], commandInfo[2], "j"));
                    break;

                case "bm":
                    responses.Add(BuyProduct(commandInfo[1], commandInfo[2], "m"));
                    break;

                case "gut":
                    responses.Add(GetUserTransactions());
                    break;

                case "asp":
                    responses.Add(AssetSendParameter(commandInfo[1], commandInfo[2]));
                    break;

                // ----------------------- Multiplayer (Shared by all) ---------------------- \\
                case "lv":
                    responses.Add(LeaveGame(commandInfo[1], routingString[1]));
                    break;
                case "rp":
                    responses.Add(await ReadyPlay(commandInfo[1], routingString[1]));
                    break;
                case "ms":
                    responses.Add(MessageOpponent(commandInfo[1], commandInfo[2], routingString[1]));
                    break;
                case "pa":
                    responses.Add(PlayAgain(commandInfo[1], routingString[1]));
                    break;

                // ---------------------------- Conflict Commands --------------------------- \\
                case "jn":
                    switch(routingString[1])
                    {
                        case "2":
                            responses.Add(A1_Chat.JoinChat(a1_User, commandInfo[1], commandInfo[2], commandInfo[3], commandInfo[4], commandInfo[5]));
                            break;
                        default:
                            responses.Add(await JoinGame(commandInfo[1], commandInfo[2], routingString[1]));
                            break;
                    }
                    break;
                case "sp":
                    switch(routingString[1])
                    {
                        case "7":
                            responses.Add(SaveProfile(commandInfo[1]));
                            break;
                        case "5":
                            responses.Add(ShotParameters(commandInfo[1], commandInfo[2], commandInfo[3], commandInfo[4], commandInfo[5]));
                            break;
                    }
                    break;

                // -------------------------------------------------------------------------- \\
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
            SendAsync(b3, 0, b3.Length);
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
        MySqlCommand getUserFromUsername = new MySqlCommand(sql1, con1);
        getUserFromUsername.Parameters.AddWithValue("@Uname", l);
        con1.Open();
        using (MySqlDataReader sqReader = getUserFromUsername.ExecuteReader())
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
        MySqlCommand getUserInfoFromUsername = new MySqlCommand(sql, con);
        getUserInfoFromUsername.Parameters.AddWithValue("@Uname", n);
        con.Open();
        using (MySqlDataReader sqReader = getUserInfoFromUsername.ExecuteReader())
        {
            while (sqReader.Read())
            {
                a1_User.username = sqReader["u"].ToString();
                a1_User.password = sqReader["p"].ToString();
                a1_User.userID = Int32.Parse(sqReader["uID"].ToString());
                a1_User.isOnline = Int32.Parse(sqReader["isOnline"].ToString());
                a1_User.connectionID = this.Id.ToString();
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
                    if(a1_User.isOnline == 1)
                    {
                        //Already logged in!
                        resultCode = 1;
                    }else{
                        resultCode = 0;
                        string sqlID = "UPDATE user SET connectionID = @cID WHERE uID=@userID";
                        MySqlCommand setConnectionID = new MySqlCommand(sqlID, con);
                        setConnectionID.Parameters.AddWithValue("@userID", a1_User.userID);
                        setConnectionID.Parameters.AddWithValue("@cID", this.Id);
                        con.Open();
                        setConnectionID.ExecuteNonQuery();
                        con.Close();
                    }
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
            }else
            {
                //Prevents issues/collisions with the user.
                a1_User = new FKUser();
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
            MySqlCommand getBuddyList = new MySqlCommand(sql, con);
            getBuddyList.Parameters.AddWithValue("@userID", a1_User.userID);
            con.Open();
            using (MySqlDataReader sqReader = getBuddyList.ExecuteReader())
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
                    MySqlCommand getBuddyInfo = new MySqlCommand(sqlB, conB);
                    getBuddyInfo.Parameters.AddWithValue("@userID", buddy);
                    conB.Open();
                    using (MySqlDataReader sqReader = getBuddyInfo.ExecuteReader())
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
        MySqlCommand setChatStatus = new MySqlCommand(sql1, con);
        setChatStatus.Parameters.AddWithValue("@userID", a1_User.userID);
        setChatStatus.Parameters.AddWithValue("@ccs", a1_User.status);
        con.Open();
        setChatStatus.ExecuteNonQuery();
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
        string sql1 = "UPDATE user SET phoneStatus = @ccs WHERE uID=@userID";
        MySqlCommand setPhoneStatus = new MySqlCommand(sql1, con);
        setPhoneStatus.Parameters.AddWithValue("@userID", a1_User.userID);
        setPhoneStatus.Parameters.AddWithValue("@ccs", a1_User.phoneStatus);
        con.Open();
        setPhoneStatus.ExecuteNonQuery();
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
        MySqlCommand selectBuddyConID = new MySqlCommand(sqlB, conB);
        selectBuddyConID.Parameters.AddWithValue("@userID", t);
        conB.Open();
        using (MySqlDataReader sqReader = selectBuddyConID.ExecuteReader())
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

        A1_Sender.SendToUser(this.Server, conID, System.Text.ASCIIEncoding.ASCII.GetString(responseStream.ToArray()));

        return System.Text.ASCIIEncoding.ASCII.GetString(responseStream.ToArray());
    }

    string AddBuddy(string n)
    {
        FKUser buddy = new FKUser();
        var con = new MySqlConnection(sqServer);

        string sql = "SELECT * FROM user WHERE u=@Uname";
        MySqlCommand getUserInfo = new MySqlCommand(sql, con);
        getUserInfo.Parameters.AddWithValue("@Uname", n);
        con.Open();
        using (MySqlDataReader sqReader = getUserInfo.ExecuteReader())
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
            A1_Sender.SendToUser(this.Server, buddy.connectionID, System.Text.ASCIIEncoding.ASCII.GetString(responseStream1.ToArray()));
            return "<notneeded/>";
        }
    }

    string AddBuddyResponse(string r, string n)
    {
        bool accepted = false;

        FKUser buddy = new FKUser();
        var con = new MySqlConnection(sqServer);

        string sql = "SELECT * FROM user WHERE u=@Uname";
        MySqlCommand getUserInfo = new MySqlCommand(sql, con);
        getUserInfo.Parameters.AddWithValue("@Uname", n);
        con.Open();
        using (MySqlDataReader sqReader = getUserInfo.ExecuteReader())
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
                MySqlCommand updatePlayersBuddies = new MySqlCommand(sql1, con);
                updatePlayersBuddies.Parameters.AddWithValue("@userID", a1_User.userID.ToString());
                updatePlayersBuddies.Parameters.AddWithValue("@buddy", buddy.userID.ToString());
                con.Open();
                updatePlayersBuddies.ExecuteNonQuery();
                con.Close();

                string sql2 = "UPDATE user SET buddyList = CONCAT_WS(',', buddyList, @userID) WHERE uID=@buddy";
                MySqlCommand updateBuddysBuddies = new MySqlCommand(sql2, con);
                updateBuddysBuddies.Parameters.AddWithValue("@userID", a1_User.userID.ToString());
                updateBuddysBuddies.Parameters.AddWithValue("@buddy", buddy.userID.ToString());
                con.Open();
                updateBuddysBuddies.ExecuteNonQuery();
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
            A1_Sender.SendToUser(this.Server, buddy.connectionID, System.Text.ASCIIEncoding.ASCII.GetString(responseStream1.ToArray()));

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
        MySqlCommand getUserInfo = new MySqlCommand(sql, con);
        getUserInfo.Parameters.AddWithValue("@userID", b);
        con.Open();
        using (MySqlDataReader sqReader = getUserInfo.ExecuteReader())
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
                A1_Sender.SendToUser(this.Server, buddy.connectionID, System.Text.ASCIIEncoding.ASCII.GetString(responseStream1.ToArray()));
            }


            var buddyList = new List<string>(a1_User.rawBuddies);
            buddyList.Remove(buddy.userID.ToString());
            a1_User.rawBuddies = buddyList.ToArray();
            string buddies = String.Join(",", a1_User.rawBuddies);
            string sql1 = "UPDATE user SET buddyList = @buddies WHERE uID=@userID";
            MySqlCommand removeBuddyFromPlayersList = new MySqlCommand(sql1, con);
            removeBuddyFromPlayersList.Parameters.AddWithValue("@userID", a1_User.userID.ToString());
            if (buddies != "")
            {
                removeBuddyFromPlayersList.Parameters.AddWithValue("@buddies", buddies);
            }
            else
            {
                removeBuddyFromPlayersList.Parameters.AddWithValue("@buddies", DBNull.Value);
            }
            con.Open();
            removeBuddyFromPlayersList.ExecuteNonQuery();
            con.Close();

            buddyList = new List<string>(buddy.rawBuddies);
            buddyList.Remove(a1_User.userID.ToString());
            buddy.rawBuddies = buddyList.ToArray();
            buddies = String.Join(",", buddy.rawBuddies);
            string sql2 = "UPDATE user SET buddyList = @buddies WHERE uID=@buddy";
            MySqlCommand removeBuddyFromBuddysList = new MySqlCommand(sql2, con);
            if (buddies != "")
            {
                removeBuddyFromBuddysList.Parameters.AddWithValue("@buddies", buddies);
            }
            else
            {
                removeBuddyFromBuddysList.Parameters.AddWithValue("@buddies", DBNull.Value);
            }
            removeBuddyFromBuddysList.Parameters.AddWithValue("@buddy", buddy.userID.ToString());
            con.Open();
            removeBuddyFromBuddysList.ExecuteNonQuery();
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
        MySqlCommand getRecipientInfo = new MySqlCommand(sqlB, conB);
        getRecipientInfo.Parameters.AddWithValue("@userID", t);
        conB.Open();
        using (MySqlDataReader sqReader = getRecipientInfo.ExecuteReader())
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

        var sql = "INSERT INTO mp_" + p + "(username, userID, challenge, challenger, challengerInfo, ready, score) VALUES(@u, @uID, @c, @cf, @ci, 0, 0)";
        conB.Open();
        using (var cmd = new MySqlCommand(sql, conB))
        {
            cmd.Parameters.AddWithValue("@u", buddyUsername);
            cmd.Parameters.AddWithValue("@uID", buddyID);
            cmd.Parameters.AddWithValue("@c", 1);
            cmd.Parameters.AddWithValue("@cf", f);
            cmd.Parameters.AddWithValue("@ci", av + "|0");
            cmd.Prepare();

            cmd.ExecuteNonQuery();
            cmd.Dispose();
        }
        conB.Close();

        string sql1 = "UPDATE mp_" + p + " SET challenger = @cf WHERE userID=@userID";
        MySqlCommand setChallenger = new MySqlCommand(sql1, conB);
        setChallenger.Parameters.AddWithValue("@userID", a1_User.userID);
        setChallenger.Parameters.AddWithValue("@cf", t);
        conB.Open();
        setChallenger.ExecuteNonQuery();
        conB.Close();

        opponentConID = conID;
        opponentUID = int.Parse(t);

        A1_Sender.SendToUser(this.Server, conID, System.Text.ASCIIEncoding.ASCII.GetString(responseStream.ToArray()));

        return "<notneeded/>";
    }

    string InviteBuddyResponse(string gid, string bid, string p, string a, string t, string f)
    {
        var responseStream = new MemoryStream();
        XmlWriterSettings settings = new XmlWriterSettings();
        settings.OmitXmlDeclaration = true;
        settings.ConformanceLevel = ConformanceLevel.Fragment;
        settings.Encoding = Encoding.ASCII;
        using (XmlWriter writer = XmlWriter.Create(responseStream, settings))
        {
            writer.WriteStartElement("u_inr");

            writer.WriteAttributeString("r", "0");
            writer.WriteAttributeString("f", f);
            writer.WriteAttributeString("t", t);
            writer.WriteAttributeString("a", a);
            writer.WriteAttributeString("p", p);

            writer.WriteEndElement();
            writer.Flush();
            writer.Close();
        }

        string conID = "";

        var conB = new MySqlConnection(sqServer);
        string sqlB = "SELECT * FROM user WHERE uID=@userID";
        MySqlCommand getConnectionID = new MySqlCommand(sqlB, conB);
        getConnectionID.Parameters.AddWithValue("@userID", f);
        conB.Open();
        using (MySqlDataReader sqReader = getConnectionID.ExecuteReader())
        {
            while (sqReader.Read())
            {
                if (Convert.ToInt32(sqReader["isOnline"].ToString()) == 1)
                {
                    conID = sqReader["connectionID"].ToString();
                }
            }

            conB.Close();
        }

        A1_Sender.SendToUser(this.Server, conID, System.Text.ASCIIEncoding.ASCII.GetString(responseStream.ToArray()));

        string sql1 = "DELETE FROM mp_" + p + " WHERE userID=@userID";
        MySqlCommand removePlayerFromMPTable = new MySqlCommand(sql1, conB);
        removePlayerFromMPTable.Parameters.AddWithValue("@userID", a1_User.userID);
        conB.Open();
        removePlayerFromMPTable.ExecuteNonQuery();
        conB.Close();

        return "<notneeded/>";
    }

    // -------------------------------------------------------------------------- \\
    //                               Plugin 2 - Chat                              \\
    // -------------------------------------------------------------------------- \\

    //<jn t="3"><pr dl="1| | " f="000000CD" uid="2" n="FUNKEY" /></jn>1|2|2|0#

    string JoinChat(string t, string dl, string f, string uid, string n)
    {
        //ALL OF THIS IS PLACEHOLDER TESTING!
        //Most of this will be deleted and replaced with a better solution.
        //* - It currently is!

        //TODO - Figure out the database setup for the chatrooms.

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

    async Task<String> JoinGame(string pr, string c, string plugin)
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

        mpPlayer = new FKGamePlayer();
        mpRival = new FKGamePlayer();

        var con = new MySqlConnection(sqServer);
        string sql = "SELECT * FROM mp_" + plugin + " WHERE userID=@userID";
        MySqlCommand getMPInfo = new MySqlCommand(sql, con);
        getMPInfo.Parameters.AddWithValue("@userID", a1_User.userID);
        con.Open();
        using (MySqlDataReader sqReader = getMPInfo.ExecuteReader())
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
            var sql1 = "INSERT INTO mp_" + plugin + "(username, userID, challenge, connectionID, playerInfo, ready, score) VALUES(@u, @uID, @c, @cID, @pi, 0, 0)";
            con.Open();
            using (var cmd = new MySqlCommand(sql1, con))
            {
                cmd.Parameters.AddWithValue("@u", a1_User.username);
                cmd.Parameters.AddWithValue("@uID", a1_User.userID);
                cmd.Parameters.AddWithValue("@c", c);
                cmd.Parameters.AddWithValue("@cID", this.Id);
                cmd.Parameters.AddWithValue("@pi", pr);

                cmd.Prepare();

                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
            con.Close();
        }

        //If random matchmaking.
        if(c == "0" && challenge != 1)
        {
            string opponentName = "";
            string opponentInfo = "";
            int isPlayerFound = 0;
            int i = 0;
            while(i < 30)
            {
                string findOpenPlayerCommand = "SELECT * FROM mp_" + plugin + " WHERE userID!=@userID AND challenge=0";
                MySqlCommand findOpenPlayer = new MySqlCommand(findOpenPlayerCommand, con);
                findOpenPlayer.Parameters.AddWithValue("@userID", a1_User.userID.ToString());
                con.Open();
                using (MySqlDataReader sqReader = findOpenPlayer.ExecuteReader())
                {

                    while (sqReader.Read())
                    {
                        opponentUID = int.Parse(sqReader["userID"].ToString());
                        opponentConID = sqReader["connectionID"].ToString();
                        opponentName = sqReader["username"].ToString();
                        opponentInfo = sqReader["playerInfo"].ToString();
                    }

                    con.Close();
                }

                string findFoundPlayersCommand = "SELECT * FROM mp_" + plugin + " WHERE userID=@userID";
                MySqlCommand findFoundPlayers = new MySqlCommand(findFoundPlayersCommand, con);
                findFoundPlayers.Parameters.AddWithValue("@userID", a1_User.userID.ToString());
                con.Open();
                using (MySqlDataReader sqReader = findFoundPlayers.ExecuteReader())
                {

                    while (sqReader.Read())
                    {
                        isPlayerFound = int.Parse(sqReader["challenge"].ToString());
                    }

                    con.Close();
                }
                if(opponentConID == "" && isPlayerFound == 0)
                {
                    await Task.Delay(1000);
                    i += 1;
                }else {i = 30;}
            }

            //If player found an opponent.
            if(opponentConID != ""){
                string setChallengeStatusCommand = "UPDATE mp_" + plugin + " SET challenge = 1, challenger = @challenger WHERE userID=@userID";
                MySqlCommand setChallengeStatusForPlayer = new MySqlCommand(setChallengeStatusCommand, con);
                setChallengeStatusForPlayer.Parameters.AddWithValue("@userID", a1_User.userID);
                setChallengeStatusForPlayer.Parameters.AddWithValue("@challenger", opponentUID);
                MySqlCommand setChallengeStatusForOpponent = new MySqlCommand(setChallengeStatusCommand, con);
                setChallengeStatusForOpponent.Parameters.AddWithValue("@userID", opponentUID);
                setChallengeStatusForOpponent.Parameters.AddWithValue("@challenger", a1_User.userID);
                con.Open();
                setChallengeStatusForPlayer.ExecuteNonQuery();
                setChallengeStatusForOpponent.ExecuteNonQuery();
                con.Close();

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
                A1_Sender.SendToUser(this.Server, opponentConID, System.Text.ASCIIEncoding.ASCII.GetString(responseStream1.ToArray()));

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
            }else if(isPlayerFound == 1) //If found by another player.
            {
                string getChallengerIDCommand = "SELECT * FROM mp_" + plugin + " WHERE userID=@userID";
                MySqlCommand getChallengerID = new MySqlCommand(getChallengerIDCommand, con);
                getChallengerID.Parameters.AddWithValue("@userID", a1_User.userID.ToString());
                con.Open();
                using (MySqlDataReader sqReader = getChallengerID.ExecuteReader())
                {

                    while (sqReader.Read())
                    {
                        opponentUID = int.Parse(sqReader["challenger"].ToString());
                    }

                    con.Close();
                }

                string getChallengerCIDCommand = "SELECT * FROM mp_" + plugin + " WHERE userID=@userID";
                MySqlCommand getChallengerCID = new MySqlCommand(getChallengerCIDCommand, con);
                getChallengerCID.Parameters.AddWithValue("@userID", opponentUID);
                con.Open();
                using (MySqlDataReader sqReader = getChallengerCID.ExecuteReader())
                {

                    while (sqReader.Read())
                    {
                        opponentConID = sqReader["connectionID"].ToString();
                    }

                    con.Close();
                }
                return "<mm_found />";
            }
            else {return "<mm_timeout />"; }
        }

        //If joining from invite.
        if(challenge == 1)
        {
            string conID = "";
            string opponentName = "";
            string opponentInfo = "";

            string sqlB = "SELECT * FROM user WHERE uID=@userID";
            MySqlCommand getOpponentConnectionID = new MySqlCommand(sqlB, con);
            getOpponentConnectionID.Parameters.AddWithValue("@userID", challenger.ToString());
            con.Open();
            using (MySqlDataReader sqReader = getOpponentConnectionID.ExecuteReader())
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

            string sqlC = "SELECT * FROM mp_" + plugin + " WHERE userID=@userID";
            MySqlCommand getChallengerInfo = new MySqlCommand(sqlC, con);
            getChallengerInfo.Parameters.AddWithValue("@userID", a1_User.userID.ToString());
            con.Open();
            using (MySqlDataReader sqReader = getChallengerInfo.ExecuteReader())
            {

                while (sqReader.Read())
                {
                    opponentInfo = sqReader["challengerInfo"].ToString();
                }

                con.Close();
            }

            string sql1 = "UPDATE mp_" + plugin + " SET challengerInfo = @ci WHERE userID=@userID";
            MySqlCommand setPlayerInfoForOpponent = new MySqlCommand(sql1, con);
            setPlayerInfoForOpponent.Parameters.AddWithValue("@userID", challenger);
            setPlayerInfoForOpponent.Parameters.AddWithValue("@ci", pr);
            con.Open();
            setPlayerInfoForOpponent.ExecuteNonQuery();
            con.Close();

            string sql2 = "UPDATE mp_" + plugin + " SET connectionID = @cID WHERE userID=@userID";
            MySqlCommand setConnectionIDForUser = new MySqlCommand(sql2, con);
            setConnectionIDForUser.Parameters.AddWithValue("@userID", a1_User.userID);
            setConnectionIDForUser.Parameters.AddWithValue("@cID", this.Id);
            con.Open();
            setConnectionIDForUser.ExecuteNonQuery();
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
            A1_Sender.SendToUser(this.Server, conID, System.Text.ASCIIEncoding.ASCII.GetString(responseStream1.ToArray()));

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

    async Task<String> ReadyPlay(string bid, string plugin)
    {
        var con = new MySqlConnection(sqServer);

        string sql1 = "UPDATE mp_" + plugin + " SET ready = @r WHERE userID=@userID";
        MySqlCommand setReady = new MySqlCommand(sql1, con);
        setReady.Parameters.AddWithValue("@r", 1);
        setReady.Parameters.AddWithValue("@userID", a1_User.userID);
        con.Open();
        setReady.ExecuteNonQuery();
        con.Close();

        //It seems that each game has it's own unique way of handling this.

        var responseStream = new MemoryStream();
        XmlWriterSettings settings = new XmlWriterSettings();
        settings.OmitXmlDeclaration = true;
        settings.ConformanceLevel = ConformanceLevel.Fragment;
        settings.Encoding = Encoding.ASCII;
        using (XmlWriter writer = XmlWriter.Create(responseStream, settings))
        {
            writer.WriteStartElement("h" + plugin + "_0");

            if(plugin == "3")
            {
                //TODO - Add DB mp_3 columns for these attributes in addition to the score.
                mpPlayer.health = 200;
                mpPlayer.lives = 3;
                mpRival.health = 200;
                mpRival.lives = 3;

                if(mpPlayer.round == 0)
                {
                    string sql5 = "UPDATE mp_3 SET lives = @l, score = 0 WHERE userID=@userID";
                    MySqlCommand setRivalLives = new MySqlCommand(sql5, con);
                    setRivalLives.Parameters.AddWithValue("@l", 3);
                    setRivalLives.Parameters.AddWithValue("@userID", opponentUID);

                    MySqlCommand setPlayerLives = new MySqlCommand(sql5, con);
                    setPlayerLives.Parameters.AddWithValue("@l", 3);
                    setPlayerLives.Parameters.AddWithValue("@userID", opponentUID);

                    con.Open();
                    setPlayerLives.ExecuteNonQuery();
                    setRivalLives.ExecuteNonQuery();
                    con.Close();    
                }else
                {
                    string sql6 = "SELECT lives FROM mp_3 WHERE userID=@userID";
                    MySqlCommand getPlayerLives = new MySqlCommand(sql6, con);
                    getPlayerLives.Parameters.AddWithValue("@userID", a1_User.userID);

                    MySqlCommand getOpponentLives = new MySqlCommand(sql6, con);
                    getOpponentLives.Parameters.AddWithValue("@userID", opponentUID);

                    con.Open();
                    using (MySqlDataReader sqReader = getPlayerLives.ExecuteReader())
                    {
                        while (sqReader.Read())
                        {
                            mpPlayer.lives = int.Parse(sqReader["lives"].ToString());
                        }
                    }
                    using (MySqlDataReader sqReader = getOpponentLives.ExecuteReader())
                    {
                        while (sqReader.Read())
                        {
                            mpRival.lives = int.Parse(sqReader["lives"].ToString());
                        }
                    }
                    con.Close();
                }

                writer.WriteStartElement("nr");
                writer.WriteAttributeString("ph", "200");
                writer.WriteAttributeString("oh", "200");
                writer.WriteAttributeString("pl", mpPlayer.lives.ToString());
                writer.WriteAttributeString("ol", mpRival.lives.ToString());
                writer.WriteEndElement();

                string sql4 = "UPDATE mp_3 SET health = @h WHERE userID=@userID";
                MySqlCommand setPlayerHealth = new MySqlCommand(sql4, con);
                setPlayerHealth.Parameters.AddWithValue("@h", 200);
                setPlayerHealth.Parameters.AddWithValue("@userID", a1_User.userID);
                con.Open();
                setPlayerHealth.ExecuteNonQuery();
                con.Close();

                //Seems that the game uses parameters to set timers, most likely for ping/lag.
                //They're stored in Number classes, so they must be fairly long ints, maybe in millis?
                //TODO - Find out the proper values/offsets for these. 
                writer.WriteStartElement("pr");
                writer.WriteAttributeString("k", "1000");
                writer.WriteAttributeString("d", "1000");
                writer.WriteAttributeString("z", "1000");
                writer.WriteEndElement();   

                if(mpPlayer.round == 0)
                {
                    writer.WriteStartElement("sg");
                    writer.WriteEndElement();    

                    writer.WriteStartElement("pe");
                    writer.WriteAttributeString("e", "0");
                    writer.WriteAttributeString("bid", bid);   
                    writer.WriteEndElement();

                    writer.WriteStartElement("oe");
                    writer.WriteAttributeString("e", "0");
                    writer.WriteAttributeString("bid", bid);   
                    writer.WriteEndElement();
                }     

                mpPlayer.round += 1;
            }

            if(plugin == "4")
            {
                //TODO - Find out why the table isn't being placed by the client.

                //Player Info
                writer.WriteStartElement("pi");
                writer.WriteAttributeString("s", "0");
                writer.WriteAttributeString("l", "3");
                writer.WriteEndElement();   

                //Opponent Info
                writer.WriteStartElement("oi");
                writer.WriteAttributeString("s", "0");
                writer.WriteAttributeString("l", "3");
                writer.WriteEndElement();  

                //The array of tables the game uses.  
                string[] tables = new string[] {"NDABFNDBOANEACDNEBOAMCAFDMCBEBMDAFDMDBFDMDCBFMEABEMEBBCMECECMFADGMFBDGLBADGLBBDHLCAGBLCBBGLCCGCLFABFLFBMALFCCFLGAIALGBBDKAACCKABJAKACNAKBADBKBBCDKBCDFKGAIAKGBDFKGCDJKHACIKHBECJAAGDJABLAJACBIJDAEDJDBGEJDCECJDDBCJEALAJEBHEJECBHJEDFDJHAFCJHBCEJHCFBIAAEBIABNAIACLAIADEBIBADCIBBEEIBCCBIDADBIDBDGIDCBGIEADEIEBEDIECDFIGACGIGBDBIGCDHIHADDIHBEBIHCBCIHDIAHAACDHABBIHACEDHBACGHBBCBHBCEDHBDEEHCAIAHCBBIHCCBHHDABBHDBDCHDCDCHDDCJHEADIHEBCEHECCJHEDCFHFACEHFBHDHFCEEHGAKAHGBJAHGCFCHGDKAHHABDHHBCHHHCBHGAACCGABDIGACBCGADDDGBAECGBBBEGBCCHGDACGGDBFBGDCCIGEACHGEBKAGECCCGGADDGGBDDGGCBBGHAFCGHBBEGHCCBGHDDEFAACEFABCGFACDHFDAJAFDBBJFDCLAFDDMAFEADJFEBBGFECHCFEDDFFHACIFHBDJFHCDJEAADCEABFCEACCJEBABJEBBDHEBCFBEGACHEGBBBEGCCFEHAEEEHBDBDBACJDBBCIDCACBDCBDEDCCKADFABHDFBDIDFCBEDGABFDGBFBCCABJCCBBDCDABJCDBHBCDCBICEAJACEBDICECBBCFACFCFBBDBDADEBDBCCBEACDBEBBG",
                                                "NCADJNDAFBNDBBJNEAEDNEBMANFALAMBAFCMBBDCMBCDDMCADEMCBCFMCCCGMCDBEMFABFMFBCJMFCFBMFDCIMGAGDMGBBEMGCBDLBACHLBBBHLDAJALDBECLEACCLEBDGLECBELEDKALGACGLGBDJKAAHDKABDHKACBGKCAEEKCBCHKCCDDKFADIKFBCCKFCBGKHACGKHBEDKHCEEJBACEJBBHEJBCBHJBDCBJDACEJDBBBJDCDEJDDBIJEADBJEBBCJECKAJGAJAJGBGCJGCECJGDKAIAACJIABFCICACDICBMAICCDIICDKAIDAIAIDBFDIDCCHIEAIAIEBBBIECBJIEDJAIFAFDIFBFCIFCECIHACIHAABHHABBIHACGEHBADGHBBFCHDAEBHDBGBHDCBFHDDIAHEADIHEBBHHECBIHEDDBHGAFBHGBDBHHANAHHBCBHHCDFGAAFBGABBJGCACEGCBBCGCCCFGCDDJGDAFDGDBEDGDCEBGEACCGEBCDGECEEGEDBGGFABBGFBBCGFCIAGHABIFBABJFBBBFFBCDGFBDDFFDADGFDBDIFDCDEFDDEDFEACBFEBBCFECCGFGAFDFGBOAFGCBEFGDCDEAABDEABCDEACJAECABBECBLAECCCJEFADEEFBDCEFCDHEHACIEHBNAEHCCCDBADJDBBEBDDAEEDDBDFDEADCDEBHCDECLADEDBDDGADCDGBCICBADHCBBDDCBCCHCCAEBCCBCFCCCDFCCDHBCFACECFBCBCFCCFCFDECCGABGCGBCJCGCOABCABDBCBDBBDABFBEALABFADHBFBDD",
                                                "NCAEBNCBCINCCHDNCDDFNDADHNDBBCNDCDJNDDECNEAGCNEBBINECDCNFACGMCADBMCBDBMCCBFMCDCEMDAKAMDBLAMDCBFMEAJAMEBBHMECGEMEDDIMFABGMFBHCLAAEBLABDILBABBLCACBLCBCBLCCCFLCDDDLDACDLDBCHLDCCJLDDBELEAFCLEBCJLECDELFAKALGAEELHACCKAACIKABBBKHACCJAABDJABDGJBAEDJBBDIJBCJAJCAJAJDABDJEACFJFACEJGADHJGBCHJGCEEJHAKAIBADBIBBBHICABGICBBBICCEDICDFBIDAGBIDBCBIDCDEIDDCBIEANAIEBFCIECDJIEDFDIFAHBIFBBJIFCGDIFDCEIGACFHBAJAHBBBFHCABGHCBECHCCDIHCDBJHDADGHDBLAHDCBJHEADCHEBIAHFABEHFBCJHFCBDHFDCEHGAOAGBABGGBBIAGCAFBGCBCDGCCBBGCDCDGDADDGDBDJGDCCHGDDDCGEAFDGEBBIGECBIGEDIAGFALAGFBDEGFCDFGFDCJGGACGFAAFCFABBEFBADGFBBBHFBCBIFCABCFDADHFEADDFFACIFGACGFGBDFFGCDBFHADCEAACFEABCIEHAFCDAACCDABBFDBABJDCAECDCBECDCCCCDDALADDBDHDDCEBDEADDDEBFDDECEEDFAFDDFBMADGAMADHABCCCAEDCCBCHCCCDFCDAFBCDBCDCDCIACEADECEBDGCECKACEDHECFABEBCANABCBFBBCCBCBDAOABDBDJBDCEBBEAEEBEBCGBECBDBFAEDBFBBH",
                                                "NCAHDNDADGNDBDDNDCGDNDDEDNEALANEBDJNECCENEDFDNFAFBMBABCMBBFDMCAECMCBDHMDAGEMDBCHMDCBFMEALAMEBBBMECCDMFACBMFBBDMGADCMGBBJLAADGLHADDKAAFDKBAGCKCADFKGABGKHAFCJAAFCJBACIJBBJAJCADIJCBKAJCCCCJCDEBJDANAJDBHCJEAEDJEBOAJFABIJFBBDJFCBGJFDCIJGABHJGBCEJHACCIAAEBIABJAIBACCIBBDFIBCIAIBDDCICALAICBDBICCDIIDAEEIDBECIDCEEIEACFIEBCFIECBFIFADHIFBEEIFCBBIGAOAIGBCJIGCCDIGDCJIHACJIHBBEHAACBHABDHHBABCHBBBIHBCCGHCACCHCBBIHCCDIHDAEDHDBCFHEANAHEBCFHFADJHFBDEHFCEBHGAFCHGBBJHGCCHHHAECHHBMAGAABCGABFDGBADIGBBHBGBCDEGBDLAGCAEEGCBBGGCCHEGDAGBGDBBEGDCJAGEAEDGEBCDGECFBGFABGGFBKAGFCCBGGACEGGBDCGGCKAGGDCJGHAIAGHBFBFAAIAFBACHFBBBIFCABHFCBDEFCCBHFCDDBFDADDFDBBJFEABHFEBBJFFADFFFBBBFFCDEFFDCDFGABCFGBEBFHADCEAABEEBABDECAFCEGADBEHADGDAAMADHABDCBABFCBBDGCCADFCCBCHCDACICDBDJCDCBBCEACICEBECCECCECFADJCFBIACGAFBCGBJABCACGBDACGBDBDBBDCDDBDDDHBEABEBEBCBBECBFBEDKABFACG",
                                                "MCAEBMCBBHMCCEEMCDBDMDACIMDBCFMDCGDMDDDBMEADIMEBDIMECBCMEDFDMFAEDMFBFDMFCEEMFDCGLBADELBBMALBCHCLBDBHLGACDLGBDJLGCBFLGDMAKAACCKABJAKACDBKADBCKCADEKCBLAKCCEDKCDBBKDADJKDBCEKDCLAKDDJAKEALAKEBCBKECKAKEDBGKFACJKFBHEKFCBGKHAOAKHBCCKHCCBKHDGEJAAKAJABDGJACBFJCAIAJCBIAJCCEDJCDDCJFACDJFBBHJFCDEJFDHDJHABGJHBDIJHCDBIAAFCIABBDICADJICBBFICCFDICDBCIDACJIDBCEIDCDBIEADCIEBCBIECDFIFAFDIFBBDIFCIAIFDFCIHAEDIHBBJHDAFBHDBECHDCCFHDDECHEAGCHEBCCHECBJHEDDHGAACDGABCJGCADFGCBBIGCCCHGCDEBGDAEBGDBLAGDCCCGEADCGEBBEGECBCGFAFCGFBEBGFCDFGFDBDGHACHGHBBIFAABEFABCEFACJAFCAFBFCBBIFCCBBFCDDGFFADGFFBCIFFCBFFFDKAFHAECFHBFBFHCFBEAADHEABBGEACBJEADBEECACFECBCBECCCGECDBEEDAFCEDBCGEDCBJEDDCEEEACGEEBKAEECNAEEDNAEFAHBEFBCDEFCDJEHADFEHBDDEHCCJEHDDHDBADDDBBCHDBCDDDBDCFDGADGDGBDIDGCIADGDDDCCADCCCBOACCCBBCCDCICDACHCDBECCDCBBCDDGBCEADECEBDHCECCICEDBHCFAEECFBJACFCEECFDBI",
                                                "LBABDLBBDDLBCHCLBDCFLCABDLCBBGLCCDJLDABFLDBEELDCOAKAAKAKABCGKACIAKBACHKBBDDKBCCBKBDGBKCADJKCBHDKCCCBKCDCCKDACDKDBBJKDCBBKDDCFKEABJKEBGDKECECJAADFJABEDJACDHJBABEJBBKAJBCCEJBDCEJCABBJCBLAJCCDBJCDCEJDACCJDBFBJDCDDJDDDCJEAKAJEBBEJECCIJEDBFJFANAJFBBFJFCEBIBACGIBBEEIBCDJICABBICBDCICCDHICDJAIDADEIDBBCIDCBCIDDBBIEACBIEBFDIECDCIEDDJIFAFBIFBFBIFCIAIFDCHIGACGIGBCIIGCBHHCAFCHCBBCHCCLAHDACJHDBDIHDCGEHDDCJHEAECHEBFBHECOAHEDFDHFAEDHFBDHHFCJAHFDMAHGADBHGBEEHGCFCHGDBIHHACDHHBCFHHCBHGBABHGBBBGGBCCBGCAFCGCBFCGCCDIGCDDIGDADEGDBLAGDCDBGDDCFGEADFGEBBJGECDFGEDDGGFACHGFBJAGFCDGGFDBHGGAKAGGBHBGGCIAFAABIFABHEFACBFFBABEFBBDDFBCDFFBDMAFCACJFCBCDFCCDBFCDDEFDAEBFDBDGFDCDEFDDDIFEACGFEBEDFECEBFEDBCFFADHFFBBEFFCEEEAACDEABBGEACEDEBABIEBBEBEBCCHEBDBDECABGECBCIECCFDECDECEDABDEDBCCEDCCEEDDFDEEADGEEBCCEECGCDBACJDBBBIDBCNADBDLADCAECDCBCIDCCDCDDABJDDBJADDCIA",
                                                "NCAEDNEADJMBADGMBBBGMBCBEMBDCDMFAFCMFBJAMFCCEMFDECLAAGDLABDFLBAGCLBBJALBCIALCALALCBDCLCCDDLCDCJLDACILDBKALDCCBLEACJLEBBHLECCELEDDILFADCLFBEBLFCIALGABFLGBDJKAADIKBACIKBBCBKCAEEKCBDDKCCIAKDADJKDBBDKDCDGKDDCEKEABBKEBBIKECFBKFACHKFBBDKGADHJAABEJABBIJBADBJCABDJCBECJDABGJDBCCJDCDEJEAEBJEBFCJFABFJGADIJGBCJIAADFIABDBIACCHIBACGIBBCGICADHIDALAIDBBJIEACBIFAFBIFBFBIGAEEIGBBEIGCBFHAACFHABDEHACCBHADEBHBADFHBBBJHBCECHCABHHCBCCHDAEDHEACGHEBBJHFAHEHFBBDHFCEEHGACFHGBKAHGCHDHGDBGGAAMAGABCEGACCJGBABCGBBCDGCADEGDABJGDBDHGEACCGFADCGFBOAGGACFGGBDGGGCGBFAAFBFABBEFBAJAFCADEFCBGEFDANAFDBDIFDCCIFEAMAFEBCDFFAEBFGADGFGBKAEAACCEBABFEBBNAECAKAECBEDEDAFDEDBLAEDCDDEDDBCEEADDEEBOAEECJAEFACHEFBIAEGADHDAACDDABDCDBACFDBBBBDBCDFDCACGDCBBIDCCDBDCDLADDACHDDBEDDDCHCDEAFDDEBFCDECFDDEDBGDFABCDFBDJDFCEEDGAFCDGBECCBABCCBBDBCBCBHCBDCICFAHBCFBBBCFCBHCFDFDBCABIBEABB"};

                //Table Set
                writer.WriteStartElement("ts");
                Random random = new Random();
                writer.WriteAttributeString("s", tables[random.Next(tables.Length)]);
                writer.WriteEndElement();

                if(mpPlayer.round == 0)
                { 
                    writer.WriteStartElement("rp");
                    writer.WriteAttributeString("bid", "6008");
                    writer.WriteEndElement();    

                    writer.WriteStartElement("sg");
                    writer.WriteAttributeString("t", "0");
                    writer.WriteEndElement();     

                    /* Initally assigning teams based on the userID of the players as it is
                    an easy and predictable way to assign the teams for both sides. */
                    if(a1_User.userID < opponentUID)
                    {
                        //Player Turn
                        mpPlayer.isKicker = true;
                        writer.WriteStartElement("pt");
                        writer.WriteAttributeString("t", "10000");
                        writer.WriteEndElement();    
                    }else
                    {
                        //Opponent Turn
                        mpPlayer.isKicker = false;
                        writer.WriteStartElement("ot");
                        writer.WriteAttributeString("t", "10000");
                        writer.WriteEndElement();    
                    }  
                } 

                mpPlayer.round += 1;
            }

            if(plugin == "5")
            {   
                if(mpPlayer.round < 10)
                {
                    if(mpPlayer.round == 0 || mpPlayer.round == 4)
                    { 
                        writer.WriteStartElement("cc");
                        if(mpPlayer.round == 0)
                        {
                            /* Initally assigning teams based on the userID of the players as it is
                            an easy and predictable way to assign the teams for both sides. */
                            if(a1_User.userID < opponentUID)
                            {
                                mpPlayer.isKicker = true;
                                writer.WriteAttributeString("c", "1");
                            }else
                            {
                                mpPlayer.isKicker = false;
                                writer.WriteAttributeString("c", "0");
                            }
                        }else{
                            //Swaps teams for second round.
                            switch(mpPlayer.isKicker)
                            {
                                case false:
                                    writer.WriteAttributeString("c", "1");
                                    break;
                                case true:
                                    writer.WriteAttributeString("c", "0");
                                    break;
                            }
                        }
                        writer.WriteEndElement();
                    }

                    writer.WriteStartElement("nr");
                    writer.WriteEndElement();

                    if(mpPlayer.round == 0)
                    {
                        writer.WriteStartElement("sg");
                        writer.WriteEndElement();              
                    }

                    mpPlayer.round += 1;
                }else
                {
                    //Get the player's score.
                    string sql3 = "SELECT score FROM mp_5 WHERE userID=@userID";
                    MySqlCommand getPlayerScore = new MySqlCommand(sql3, con);
                    getPlayerScore.Parameters.AddWithValue("@userID", a1_User.userID);

                    MySqlCommand getOpponentScore = new MySqlCommand(sql3, con);
                    getOpponentScore.Parameters.AddWithValue("@userID", opponentUID);

                    con.Open();
                    using (MySqlDataReader sqReader = getPlayerScore.ExecuteReader())
                    {
                        while (sqReader.Read())
                        {
                            mpPlayer.score = int.Parse(sqReader["score"].ToString());
                        }
                    }
                    using (MySqlDataReader sqReader = getOpponentScore.ExecuteReader())
                    {
                        while (sqReader.Read())
                        {
                            mpRival.score = int.Parse(sqReader["score"].ToString());
                        }
                    }
                    con.Close();

                    writer.WriteStartElement("go");
                    //The result attribute seems to determine the coin distribution.
                    if(mpPlayer.score > mpRival.score)
                    {
                        writer.WriteAttributeString("r", "6");
                    }else if(mpPlayer.score < mpRival.score)
                    {
                        writer.WriteAttributeString("r", "7");
                    }else
                    {
                       writer.WriteAttributeString("r", "8"); 
                    }
                
                    writer.WriteEndElement();      
                }
            }

            writer.WriteEndElement();
            writer.Flush();
            writer.Close();
        }

        int opponentReady = 0;
        int i = 0;

        while(opponentReady == 0)
        {
            string sql = "SELECT ready FROM mp_" + plugin + " WHERE userID=@userID";
            MySqlCommand getOpponentReady = new MySqlCommand(sql, con);
            getOpponentReady.Parameters.AddWithValue("@userID", opponentUID);
            con.Open();
            using (MySqlDataReader sqReader = getOpponentReady.ExecuteReader())
            {
                while (sqReader.Read())
                {
                    opponentReady = int.Parse(sqReader["ready"].ToString());
                }
                con.Close();
            }

            i += 1;
            await Task.Delay(500);

            if(i == 20){
                opponentReady = 1;
            }
        }

        string sql2 = "UPDATE mp_" + plugin + " SET ready = @r WHERE userID=@userID";
        MySqlCommand setReadyToZero = new MySqlCommand(sql2, con);
        setReadyToZero.Parameters.AddWithValue("@r", 0);
        setReadyToZero.Parameters.AddWithValue("@userID", a1_User.userID);
        con.Open();
        setReadyToZero.ExecuteNonQuery();
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

        A1_Sender.SendToUser(this.Server, opponentConID, System.Text.ASCIIEncoding.ASCII.GetString(responseStream.ToArray()));

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

            writer.WriteStartElement("go");
            writer.WriteAttributeString("r", "5");
            writer.WriteEndElement();    

            writer.WriteEndElement();
            writer.Flush();
            writer.Close();
        }

        A1_Sender.SendToUser(this.Server, opponentConID, System.Text.ASCIIEncoding.ASCII.GetString(responseStream.ToArray()));

        opponentConID = "";
        opponentUID = 0;
        mpPlayer = null;
        mpRival = null;

        var con = new MySqlConnection(sqServer);

        try{
            string sql1 = "DELETE FROM mp_" + plugin + " WHERE userID=@userID";
            MySqlCommand removePlayerFromMPTable = new MySqlCommand(sql1, con);
            removePlayerFromMPTable.Parameters.AddWithValue("@userID", a1_User.userID);
            con.Open();
            removePlayerFromMPTable.ExecuteNonQuery();
            con.Close();
        }catch(MySqlConnector.MySqlException)
        {
            Console.WriteLine("[Error] Couldn't find user in MP table!");
        }

        return "<notneeded />";
    }

    string PlayAgain(string bid, string plugin)
    {
        var responseStream = new MemoryStream();
        XmlWriterSettings settings = new XmlWriterSettings();
        settings.OmitXmlDeclaration = true;
        settings.ConformanceLevel = ConformanceLevel.Fragment;
        settings.Encoding = Encoding.ASCII;
        using (XmlWriter writer = XmlWriter.Create(responseStream, settings))
        {
            writer.WriteStartElement("h" + plugin + "_0");

            bool isOpponentStillWaiting = false;

            using (var con = new MySqlConnection(sqServer))
            {
                con.Open();

                using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM mp_" + plugin + " WHERE userID=@uID", con))
                {
                    cmd.Parameters.AddWithValue("@uID", opponentUID);
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    if(count == 1)
                    {
                        isOpponentStillWaiting = true;
                    }
                }

                con.Close();
            }

            if(isOpponentStillWaiting)
            {
                writer.WriteStartElement("pa");
                writer.WriteEndElement();
            }else
            {
                writer.WriteStartElement("go");
                writer.WriteAttributeString("r", "5");
                writer.WriteEndElement();   
            }

            writer.WriteEndElement();
            writer.Flush();
            writer.Close();
        }

        A1_Sender.SendToUser(this.Server, opponentConID, System.Text.ASCIIEncoding.ASCII.GetString(responseStream.ToArray()));

        return System.Text.ASCIIEncoding.ASCII.GetString(responseStream.ToArray());
    }

    // -------------------------------------------------------------------------- \\
    //                              Plugin 3 - Boxing                             \\
    // -------------------------------------------------------------------------- \\

    string PlayerEvent(string h, string e, string bid)
    {
        //TODO - Figure out how to get the game to trigger damages and proper health calculations.

        var con = new MySqlConnection(sqServer);

        //Get the player's health.
        string sql2 = "SELECT health FROM mp_3 WHERE userID=@userID";
        MySqlCommand getPlayerHealth = new MySqlCommand(sql2, con);
        getPlayerHealth.Parameters.AddWithValue("@userID", a1_User.userID);

        string sql3 = "SELECT health FROM mp_3 WHERE userID=@userID";
        MySqlCommand getOpponentHealth = new MySqlCommand(sql3, con);
        getOpponentHealth.Parameters.AddWithValue("@userID", opponentUID);

        con.Open();
        using (MySqlDataReader sqReader = getPlayerHealth.ExecuteReader())
        {
            while (sqReader.Read())
            {
                mpPlayer.health = int.Parse(sqReader["health"].ToString());
            }
        }
        using (MySqlDataReader sqReader = getOpponentHealth.ExecuteReader())
        {
            while (sqReader.Read())
            {
                mpRival.health = int.Parse(sqReader["health"].ToString());
                mpRival.health -= int.Parse(h);
            }
        }

        mpPlayer.score += int.Parse(h);

        string sql = "UPDATE mp_3 SET health = @h WHERE userID=@userID";
        MySqlCommand setPlayerHealth = new MySqlCommand(sql, con);
        setPlayerHealth.Parameters.AddWithValue("@h", mpRival.health);
        setPlayerHealth.Parameters.AddWithValue("@userID", opponentUID);
        setPlayerHealth.ExecuteNonQuery();

        string sql5 = "UPDATE mp_3 SET score = @s WHERE userID=@userID";
        MySqlCommand setPlayerScore = new MySqlCommand(sql5, con);
        setPlayerScore.Parameters.AddWithValue("@s", mpPlayer.score);
        setPlayerScore.Parameters.AddWithValue("@userID", a1_User.userID);
        setPlayerScore.ExecuteNonQuery();

        con.Close();

        var opponentStream = new MemoryStream();
        XmlWriterSettings settings = new XmlWriterSettings();
        settings.OmitXmlDeclaration = true;
        settings.ConformanceLevel = ConformanceLevel.Fragment;
        settings.Encoding = Encoding.ASCII;
        using (XmlWriter writer = XmlWriter.Create(opponentStream, settings))
        {
            writer.WriteStartElement("h3_0");

            writer.WriteStartElement("oe");

            writer.WriteAttributeString("h", mpRival.health.ToString());

            writer.WriteAttributeString("e", e);

            writer.WriteAttributeString("bid", bid);
            
            writer.WriteEndElement();

            if(mpRival.health <= 0)
            {
                writer.WriteStartElement("pe");

                writer.WriteAttributeString("e", "10");

                writer.WriteAttributeString("bid", bid);
                
                mpRival.lives -= 1;

                string sql4 = "UPDATE mp_3 SET lives = @l WHERE userID=@userID";
                MySqlCommand setRivalLives = new MySqlCommand(sql4, con);
                setRivalLives.Parameters.AddWithValue("@l", mpRival.lives);
                setRivalLives.Parameters.AddWithValue("@userID", opponentUID);
                con.Open();
                setRivalLives.ExecuteNonQuery();
                con.Close();

                writer.WriteEndElement();

                if(mpRival.lives <= 0)
                {
                    string sql6 = "SELECT score FROM mp_3 WHERE userID=@userID";
                    MySqlCommand getPlayerScore = new MySqlCommand(sql6, con);
                    getPlayerScore.Parameters.AddWithValue("@userID", a1_User.userID);

                    MySqlCommand getOpponentScore = new MySqlCommand(sql6, con);
                    getOpponentScore.Parameters.AddWithValue("@userID", opponentUID);

                    con.Open();
                    using (MySqlDataReader sqReader = getPlayerScore.ExecuteReader())
                    {
                        while (sqReader.Read())
                        {
                            mpPlayer.score = int.Parse(sqReader["score"].ToString());
                        }
                    }
                    using (MySqlDataReader sqReader = getOpponentScore.ExecuteReader())
                    {
                        while (sqReader.Read())
                        {
                            mpRival.score = int.Parse(sqReader["score"].ToString());
                        }
                    }

                    mpPlayer.score *= 2;
                    mpRival.score *= 2;

                    writer.WriteStartElement("ps");
                    writer.WriteAttributeString("s", mpRival.score.ToString());
                    writer.WriteEndElement();

                    writer.WriteStartElement("os");
                    writer.WriteAttributeString("s", mpPlayer.score.ToString());
                    writer.WriteEndElement();

                    writer.WriteStartElement("go");
                    //The result attribute seems to determine the coin distribution.
                    if(mpPlayer.score < mpRival.score)
                    {
                        writer.WriteAttributeString("r", "6");
                    }else if(mpPlayer.score > mpRival.score)
                    {
                        writer.WriteAttributeString("r", "7");
                    }else
                    {
                       writer.WriteAttributeString("r", "8"); 
                    }
                
                    writer.WriteEndElement(); 
                }
            }

            writer.WriteEndElement();
            writer.Flush();
            writer.Close();
        }

        A1_Sender.SendToUser(this.Server, opponentConID, System.Text.ASCIIEncoding.ASCII.GetString(opponentStream.ToArray()));

        if(mpRival.health <= 0)
        {
            var playerStream = new MemoryStream();
            using (XmlWriter writer = XmlWriter.Create(playerStream, settings))
            {
                writer.WriteStartElement("h3_0");

                writer.WriteStartElement("oe");

                writer.WriteAttributeString("h", mpRival.health.ToString());

                writer.WriteAttributeString("e", "10");

                writer.WriteAttributeString("bid", bid);

                writer.WriteEndElement();

                if(mpRival.lives <= 0)
                {
                    writer.WriteStartElement("ps");
                    writer.WriteAttributeString("s", mpPlayer.score.ToString());
                    writer.WriteEndElement();

                    writer.WriteStartElement("os");
                    writer.WriteAttributeString("s", mpRival.score.ToString());
                    writer.WriteEndElement();

                    writer.WriteStartElement("go");
                    //The result attribute seems to determine the coin distribution.
                    if(mpPlayer.score > mpRival.score)
                    {
                        writer.WriteAttributeString("r", "6");
                    }else if(mpPlayer.score < mpRival.score)
                    {
                        writer.WriteAttributeString("r", "7");
                    }else
                    {
                       writer.WriteAttributeString("r", "8"); 
                    }
                
                    writer.WriteEndElement(); 
                }

                writer.WriteEndElement();
                writer.Flush();
                writer.Close();
            }
            return System.Text.ASCIIEncoding.ASCII.GetString(playerStream.ToArray());
        }else
        {
            return "<notneeded/>";
        }
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

        A1_Sender.SendToUser(this.Server, opponentConID, System.Text.ASCIIEncoding.ASCII.GetString(responseStream.ToArray()));

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

        A1_Sender.SendToUser(this.Server, opponentConID, System.Text.ASCIIEncoding.ASCII.GetString(responseStream.ToArray()));

        return "<notneeded/>";
    }

    string BlockShot(string d, string lx, string m, string c, string bid)
    {
        bool blocked = false;
        int playerScore = 0;
        int opponentScore = 0;

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

            // For some reason, the Block Shot commmand is in charge of setting the score.
            switch(c)
            {
                //Missed (Above net)
                case "0":
                    blocked = true;
                    break;
                //Blocked
                case "1":
                    blocked = true;
                    break;
                //Missed (Beside net)
                case "3":
                    blocked = true;
                    break;
            }

            //Award a point to the winner of the round.
            var con = new MySqlConnection(sqServer);
            string sql1 = "UPDATE mp_5 SET score = score + 1 WHERE userID=@userID";
            MySqlCommand updatePlayerScore = new MySqlCommand(sql1, con);
            if(blocked)
                updatePlayerScore.Parameters.AddWithValue("@userID", a1_User.userID);
            else
                updatePlayerScore.Parameters.AddWithValue("@userID", opponentUID);

            //Get the player's score.
            string sql2 = "SELECT score FROM mp_5 WHERE userID=@userID";
            MySqlCommand getPlayerScore = new MySqlCommand(sql2, con);
            getPlayerScore.Parameters.AddWithValue("@userID", a1_User.userID);

            string sql3 = "SELECT score FROM mp_5 WHERE userID=@userID";
            MySqlCommand getOpponentScore = new MySqlCommand(sql3, con);
            getOpponentScore.Parameters.AddWithValue("@userID", opponentUID);

            con.Open();
            updatePlayerScore.ExecuteNonQuery();
            using (MySqlDataReader sqReader = getPlayerScore.ExecuteReader())
            {
                while (sqReader.Read())
                {
                    playerScore = int.Parse(sqReader["score"].ToString());
                }
            }
            using (MySqlDataReader sqReader = getOpponentScore.ExecuteReader())
            {
                while (sqReader.Read())
                {
                    opponentScore = int.Parse(sqReader["score"].ToString());
                }
            }
            con.Close();

            writer.WriteStartElement("ps");
            writer.WriteAttributeString("s", opponentScore.ToString());
            writer.WriteEndElement();

            writer.WriteStartElement("os");
            writer.WriteAttributeString("s", playerScore.ToString());
            writer.WriteEndElement();

            writer.WriteEndElement();
            writer.Flush();
            writer.Close();
        }

        A1_Sender.SendToUser(this.Server, opponentConID, System.Text.ASCIIEncoding.ASCII.GetString(responseStream.ToArray()));

        var responseStream1 = new MemoryStream();
        using (XmlWriter writer1 = XmlWriter.Create(responseStream1, settings))
        {
            writer1.WriteStartElement("h5_0");

            writer1.WriteStartElement("os");
            writer1.WriteAttributeString("s", opponentScore.ToString());
            writer1.WriteEndElement();

            writer1.WriteStartElement("ps");
            writer1.WriteAttributeString("s", playerScore.ToString());
            writer1.WriteEndElement();

            writer1.WriteEndElement();
            writer1.Flush();
            writer1.Close();
        }

        return System.Text.ASCIIEncoding.ASCII.GetString(responseStream1.ToArray());
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
    //                              Plugin 10 - Trunk                             \\
    // -------------------------------------------------------------------------- \\

    public string GetUserAssets()
    {
        var responseStream = new MemoryStream();
        XmlWriterSettings settings = new XmlWriterSettings();
        settings.OmitXmlDeclaration = true;
        settings.ConformanceLevel = ConformanceLevel.Fragment;
        settings.Encoding = Encoding.ASCII;

        var con = new MySqlConnection(sqServer);

        using (XmlWriter writer = XmlWriter.Create(responseStream, settings))
        {
            writer.WriteStartElement("h10_0");

            writer.WriteStartElement("gua");

            XmlDocument profile = new XmlDocument();
            profile.Load(serverDirectory + a1_User.username + @"/profile");

            var familiarNodes = profile.SelectNodes("/profile/trunk/familiars/familiar");
            foreach (XmlNode node in familiarNodes)
            {
                writer.WriteStartElement("f");
                writer.WriteAttributeString("id", node.Attributes["id"].Value);
                writer.WriteAttributeString("p", node.Attributes["start"].Value);
                writer.WriteAttributeString("c", (int.Parse(node.Attributes["time"].Value) / 60).ToString());
                writer.WriteEndElement();
            }

            string sql = "SELECT * FROM user WHERE uID=@uid";

            string jammersTotal = "";
            string jammersUsed = "";
            
            MySqlCommand getJammerInfo = new MySqlCommand(sql, con);
            getJammerInfo.Parameters.AddWithValue("@uid", a1_User.userID);
            con.Open();
            using (MySqlDataReader sqReader = getJammerInfo.ExecuteReader())
            {
                while (sqReader.Read())
                {
                    jammersTotal = sqReader["jammersTotal"].ToString();
                    jammersUsed = sqReader["jammersUsed"].ToString();
                }
                con.Close();
            }

            if(jammersTotal == "")
            {
                jammersTotal = "0";
            }

            if(jammersUsed == "")
            {
                jammersUsed = "0";
            }

            if(int.Parse(jammersTotal) > 0)
            {
                writer.WriteStartElement("j");
                writer.WriteAttributeString("id", "80014a");
                writer.WriteAttributeString("p", jammersUsed);
                writer.WriteAttributeString("c", jammersTotal);
                writer.WriteEndElement();   
            }

            var moodNodes = profile.SelectNodes("/profile/trunk/moods/mood");
            foreach (XmlNode node in moodNodes)
            {
                writer.WriteStartElement("m");
                writer.WriteAttributeString("id", node.Attributes["id"].Value);
                writer.WriteEndElement();
            }
            
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.Flush();
            writer.Close();
        }
        return System.Text.ASCIIEncoding.ASCII.GetString(responseStream.ToArray());
    }

    public string GetLootBalance()
    {
        var responseStream = new MemoryStream();
        XmlWriterSettings settings = new XmlWriterSettings();
        settings.OmitXmlDeclaration = true;
        settings.ConformanceLevel = ConformanceLevel.Fragment;
        settings.Encoding = Encoding.ASCII;

        using (XmlWriter writer = XmlWriter.Create(responseStream, settings))
        {
            writer.WriteStartElement("h10_0");
            writer.WriteStartElement("glb");

            //Loot permanently set to 2500.
            writer.WriteAttributeString("b", "2500");

            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.Flush();
            writer.Close();
        }
        return System.Text.ASCIIEncoding.ASCII.GetString(responseStream.ToArray());
    }

    public string GetProductList(string productType)
    {
        var responseStream = new MemoryStream();
        XmlWriterSettings settings = new XmlWriterSettings();
        settings.OmitXmlDeclaration = true;
        settings.ConformanceLevel = ConformanceLevel.Fragment;
        settings.Encoding = Encoding.ASCII;


        var con = new MySqlConnection(sqServer);

        using (XmlWriter writer = XmlWriter.Create(responseStream, settings))
        {
            writer.WriteStartElement("h10_0");
            
            string sql = "SELECT * FROM t_items";

            switch(productType)
            {
                case "i":
                    writer.WriteStartElement("gil");
                    sql = "SELECT * FROM t_items";
                    MySqlCommand getItems = new MySqlCommand(sql, con);
                    con.Open();
                    using (MySqlDataReader sqReader = getItems.ExecuteReader())
                    {
                        while (sqReader.Read())
                        {   writer.WriteStartElement("i");
                            writer.WriteAttributeString("rid", sqReader["rid"].ToString());
                            writer.WriteAttributeString("id", sqReader["id"].ToString());
                            writer.WriteAttributeString("c", sqReader["cost"].ToString());
                            writer.WriteAttributeString("q", "1");
                            writer.WriteAttributeString("d", "");
                            writer.WriteEndElement();
                        }
                        con.Close();
                    }
                    break;
                
                case "f":
                    writer.WriteStartElement("gfl");
                    sql = "SELECT * FROM t_familiar";
                    MySqlCommand getFamiliars = new MySqlCommand(sql, con);
                    con.Open();
                    using (MySqlDataReader sqReader = getFamiliars.ExecuteReader())
                    {
                        while (sqReader.Read())
                        {   writer.WriteStartElement("f");
                            writer.WriteAttributeString("rid", sqReader["rid"].ToString());
                            writer.WriteAttributeString("id", sqReader["id"].ToString());
                            writer.WriteAttributeString("c", sqReader["cost"].ToString());
                            writer.WriteAttributeString("dc", sqReader["discountedCost"].ToString());
                            writer.WriteAttributeString("h", sqReader["duration"].ToString());
                            writer.WriteAttributeString("d", "");
                            writer.WriteEndElement();
                        }
                        con.Close();
                    }
                    break;

                case "j":
                    writer.WriteStartElement("gjl");
                    sql = "SELECT * FROM t_jammer";
                    MySqlCommand getJammers = new MySqlCommand(sql, con);
                    con.Open();
                    using (MySqlDataReader sqReader = getJammers.ExecuteReader())
                    {
                        while (sqReader.Read())
                        {   writer.WriteStartElement("j");
                            writer.WriteAttributeString("rid", sqReader["rid"].ToString());
                            writer.WriteAttributeString("id", sqReader["id"].ToString());
                            writer.WriteAttributeString("c", sqReader["cost"].ToString());
                            writer.WriteAttributeString("q", sqReader["quantity"].ToString());
                            writer.WriteAttributeString("d", "");
                            writer.WriteEndElement();
                        }
                        con.Close();
                    }
                    break;

                case "m":
                    writer.WriteStartElement("gml");
                    sql = "SELECT * FROM t_mood";
                    MySqlCommand getMoods = new MySqlCommand(sql, con);
                    con.Open();
                    using (MySqlDataReader sqReader = getMoods.ExecuteReader())
                    {
                        while (sqReader.Read())
                        {   writer.WriteStartElement("m");
                            writer.WriteAttributeString("rid", sqReader["rid"].ToString());
                            writer.WriteAttributeString("id", sqReader["id"].ToString());
                            writer.WriteAttributeString("c", sqReader["cost"].ToString());
                            writer.WriteAttributeString("q", "1");
                            writer.WriteAttributeString("d", "");
                            writer.WriteEndElement();
                        }
                        con.Close();
                    }
                    break;
                
                case "c":
                    writer.WriteStartElement("gcl");
                    sql = "SELECT * FROM t_cleaning";
                    MySqlCommand getCleanings = new MySqlCommand(sql, con);
                    con.Open();
                    using (MySqlDataReader sqReader = getCleanings.ExecuteReader())
                    {
                        while (sqReader.Read())
                        {   writer.WriteStartElement("c");
                            writer.WriteAttributeString("rid", sqReader["rid"].ToString());
                            writer.WriteAttributeString("id", sqReader["id"].ToString());
                            writer.WriteAttributeString("c", sqReader["cost"].ToString());
                            writer.WriteAttributeString("q", "1");
                            writer.WriteAttributeString("d", "");
                            writer.WriteEndElement();
                        }
                        con.Close();
                    }
                    break;
            }
            

            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.Flush();
            writer.Close();
        }
        return System.Text.ASCIIEncoding.ASCII.GetString(responseStream.ToArray());
    }

    public string GetUserTransactionCount()
    {
        var responseStream = new MemoryStream();
        XmlWriterSettings settings = new XmlWriterSettings();
        settings.OmitXmlDeclaration = true;
        settings.ConformanceLevel = ConformanceLevel.Fragment;
        settings.Encoding = Encoding.ASCII;

        var con = new MySqlConnection(sqServer);

        using (XmlWriter writer = XmlWriter.Create(responseStream, settings))
        {
            writer.WriteStartElement("h10_0");
            
            string sql = "SELECT * FROM user WHERE uID=@uid";

            writer.WriteStartElement("gutc");

            string count = "0";
            
            MySqlCommand getTransactions = new MySqlCommand(sql, con);
            getTransactions.Parameters.AddWithValue("@uid", a1_User.userID);
            con.Open();
            using (MySqlDataReader sqReader = getTransactions.ExecuteReader())
            {
                while (sqReader.Read())
                {
                    count = sqReader["transactionCount"].ToString();
                }
                con.Close();
            }

            if(count == ""){
                count = "0";
            }

            writer.WriteAttributeString("c", count);

            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.Flush();
            writer.Close();
        }
        return System.Text.ASCIIEncoding.ASCII.GetString(responseStream.ToArray());
    }

    public string BuyProduct(string id, string b, string productType)
    {
        var responseStream = new MemoryStream();
        XmlWriterSettings settings = new XmlWriterSettings();
        settings.OmitXmlDeclaration = true;
        settings.ConformanceLevel = ConformanceLevel.Fragment;
        settings.Encoding = Encoding.ASCII;

        var con = new MySqlConnection(sqServer);

        using (XmlWriter writer = XmlWriter.Create(responseStream, settings))
        {
            writer.WriteStartElement("h10_0");

            string sql = "SELECT * FROM t_items WHERE id=@id";

            string rid = "";
            string cost = "";
            string quantity = "";

            switch(productType)
            {
                case "f":  
                    writer.WriteStartElement("bf");
                    sql = "SELECT * FROM t_familiar WHERE id=@id";
                    break;

                case "i":  
                    writer.WriteStartElement("bi");
                    sql = "SELECT * FROM t_items WHERE id=@id";
                    break;

                case "j":  
                    writer.WriteStartElement("bj");
                    sql = "SELECT * FROM t_jammer WHERE id=@id";
                    break;
                
                case "c":  
                    writer.WriteStartElement("bc");
                    sql = "SELECT * FROM t_cleaning WHERE id=@id";
                    break;

                case "m":  
                    writer.WriteStartElement("bm");
                    sql = "SELECT * FROM t_mood WHERE id=@id";
                    break;
            }
            MySqlCommand getProductInfo = new MySqlCommand(sql, con);
            getProductInfo.Parameters.AddWithValue("@id", id);
            con.Open();
            using (MySqlDataReader sqReader = getProductInfo.ExecuteReader())
            {
                while (sqReader.Read())
                {
                    rid = sqReader["rid"].ToString();
                    cost = sqReader["cost"].ToString();
                    if(productType == "j")
                    {
                        quantity = sqReader["quantity"].ToString();
                    }
                }
                con.Close();
            }

            var responseStreamEntry = new MemoryStream();

            using (XmlWriter writer2 = XmlWriter.Create(responseStreamEntry, settings))
            {
                writer2.WriteStartElement("t");

                writer2.WriteAttributeString("id", id);
                writer2.WriteAttributeString("rid", rid);
                writer2.WriteAttributeString("d", DateTime.Now.ToString("MM/dd/yyyy HH:mm"));
                writer2.WriteAttributeString("c", cost);
                writer2.WriteAttributeString("b", "2500");

                writer2.WriteEndElement();
                writer2.Flush();
                writer2.Close();
            }

            string sql2 = "UPDATE user SET transactionHistory = IFNULL(CONCAT(transactionHistory, @transaction), @transaction) WHERE uID=@uID";
            MySqlCommand updateTransactions = new MySqlCommand(sql2, con);
            updateTransactions.Parameters.AddWithValue("@transaction", System.Text.ASCIIEncoding.ASCII.GetString(responseStreamEntry.ToArray()));
            updateTransactions.Parameters.AddWithValue("@uID", a1_User.userID.ToString());
            con.Open();
            updateTransactions.ExecuteNonQuery();
            con.Close();

            string sql4 = "UPDATE user SET transactionCount = IFNULL(transactionCount,0) + 1 WHERE uID=@uID";
            MySqlCommand updateCount = new MySqlCommand(sql4, con);
            updateCount.Parameters.AddWithValue("@uID", a1_User.userID.ToString());
            con.Open();
            updateCount.ExecuteNonQuery();
            con.Close();

            if(productType == "j")
            {
                string sql3 = "UPDATE user SET jammersTotal = IFNULL(jammersTotal,0) + @quantity WHERE uID=@uID";
                MySqlCommand updateJammers = new MySqlCommand(sql3, con);
                updateJammers.Parameters.AddWithValue("@quantity", int.Parse(quantity));
                updateJammers.Parameters.AddWithValue("@uID", a1_User.userID.ToString());
                con.Open();
                updateJammers.ExecuteNonQuery();
                con.Close();
            }

            writer.WriteAttributeString("id", id);
            writer.WriteAttributeString("b", "2500"); //Supposed to reflect remaining balance, but A1Emu uses infinite funds.

            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.Flush();
            writer.Close();
        }
        return System.Text.ASCIIEncoding.ASCII.GetString(responseStream.ToArray());
    }

    string GetUserTransactions()
    {
        var responseStream = new MemoryStream();
        XmlWriterSettings settings = new XmlWriterSettings();
        settings.OmitXmlDeclaration = true;
        settings.ConformanceLevel = ConformanceLevel.Fragment;
        settings.Encoding = Encoding.ASCII;

        var con = new MySqlConnection(sqServer);

        using (XmlWriter writer = XmlWriter.Create(responseStream, settings))
        {
            writer.WriteStartElement("h10_0");
            writer.WriteStartElement("gut");

            string transactionHistory = "";

            string sql = "SELECT * FROM user WHERE uID=@id";
            MySqlCommand getProductInfo = new MySqlCommand(sql, con);
            getProductInfo.Parameters.AddWithValue("@id", a1_User.userID.ToString());
            con.Open();
            using (MySqlDataReader sqReader = getProductInfo.ExecuteReader())
            {
                while (sqReader.Read())
                {
                    transactionHistory = sqReader["transactionHistory"].ToString();
                }
                con.Close();
            }

            writer.WriteRaw(transactionHistory);

            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.Flush();
            writer.Close();
        }
        return System.Text.ASCIIEncoding.ASCII.GetString(responseStream.ToArray());
    }

    string AssetSendParameter(string p, string id)
    {
        var responseStream = new MemoryStream();
        XmlWriterSettings settings = new XmlWriterSettings();
        settings.OmitXmlDeclaration = true;
        settings.ConformanceLevel = ConformanceLevel.Fragment;
        settings.Encoding = Encoding.ASCII;

        var con = new MySqlConnection(sqServer);

        using (XmlWriter writer = XmlWriter.Create(responseStream, settings))
        {
            writer.WriteStartElement("h10_0");
            writer.WriteStartElement("asp");

            if(id == "80014a")
            {
                string sql3 = "UPDATE user SET jammersUsed = IFNULL(jammersUsed,0) + 1 WHERE uID=@uID";
                MySqlCommand updateJammers = new MySqlCommand(sql3, con);
                updateJammers.Parameters.AddWithValue("@uID", a1_User.userID.ToString());
                con.Open();
                updateJammers.ExecuteNonQuery();
                con.Close();
            }

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
        MySqlCommand selectBuddyList = new MySqlCommand(sql, con);
        selectBuddyList.Parameters.AddWithValue("@userID", a1_User.userID);
        con.Open();
        using (MySqlDataReader sqReader = selectBuddyList.ExecuteReader())
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
                MySqlCommand getBuddy = new MySqlCommand(sqlB, conB);
                getBuddy.Parameters.AddWithValue("@userID", buddy);
                conB.Open();
                using (MySqlDataReader sqReader = getBuddy.ExecuteReader())
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
                    A1_Sender.SendToUser(this.Server, conID, System.Text.ASCIIEncoding.ASCII.GetString(responseStream1.ToArray()));
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
