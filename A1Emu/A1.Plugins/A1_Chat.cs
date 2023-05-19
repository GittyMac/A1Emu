using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using NetCoreServer;
using A1Emu.A1.Utils;

namespace A1Emu.A1.Plugins
{
    public class A1_Chat
    {

        public static TcpServer server;
        static List<FKUser[]> chatRooms_1 = new List<FKUser[]>{ new FKUser[8] };

        public static string JoinChat(FKUser user, string t, string dl, string f, string uid, string n)
        {
            //TODO - Find out how to properly handle joining, as well as tidying up this mess to avoid any awkward errors.

            user.bitty = f;
            user.dl = dl;
            var responseStream = new MemoryStream();
            int lobbyID = 0;
            bool found = false;
            foreach (FKUser[] lobby in chatRooms_1)
            {
                if (!found)
                {
                    for (int i = 0; i < lobby.Length; i++)
                    {
                        if (lobby[i] == null)
                        {
                            user.lobbyID = lobbyID;
                            lobby[i] = user;

                            XmlWriterSettings settings = new XmlWriterSettings();
                            settings.OmitXmlDeclaration = true;
                            settings.ConformanceLevel = ConformanceLevel.Fragment;
                            settings.Encoding = Encoding.ASCII;
                            using (XmlWriter writer = XmlWriter.Create(responseStream, settings))
                            {
                                writer.WriteStartElement("h2_0");
                                writer.WriteStartElement("pj");

                                for (int j = 0; j < lobby.Length; j++)
                                {
                                    if (lobby[j] != null)
                                    {
                                        writer.WriteStartElement("pr");
                                        writer.WriteAttributeString("f", lobby[j].bitty.ToString());
                                        writer.WriteAttributeString("uid", lobby[j].userID.ToString());
                                        writer.WriteAttributeString("n", lobby[j].username.ToString());
                                        writer.WriteAttributeString("dl", lobby[j].dl.ToString());
                                        writer.WriteEndElement();
                                    }
                                }

                                writer.WriteEndElement();

                                writer.WriteStartElement("jn");
                                writer.WriteAttributeString("r", "0");
                                writer.WriteAttributeString("id", lobbyID.ToString());
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

                                writer.WriteStartElement("pr");
                                writer.WriteAttributeString("f", user.bitty.ToString());
                                writer.WriteAttributeString("uid", user.userID.ToString());
                                writer.WriteAttributeString("n", user.username.ToString());
                                writer.WriteAttributeString("dl", user.dl.ToString());
                                writer.WriteEndElement();

                                writer.WriteStartElement("on");
                                writer.WriteAttributeString("id", user.userID.ToString());
                                writer.WriteEndElement();

                                writer.WriteEndElement();
                                writer.WriteEndElement();
                                writer.Flush();
                                writer.Close();
                            }
                            SendMessage(user, System.Text.ASCIIEncoding.ASCII.GetString(responseStream1.ToArray()));
                            found = true;
                            break;
                        }
                    }
                }
                lobbyID++;
            }
            return System.Text.ASCIIEncoding.ASCII.GetString(responseStream.ToArray());
        }

        public static void LeaveChat()
        {

        }

        public static void SendMessage(FKUser user, string message)
        {
            for (int i = 0; i < chatRooms_1[user.lobbyID].Length; i++)
            {
                if (chatRooms_1[user.lobbyID][i] != null)
                {
                    A1_Sender.SendToUser(server, chatRooms_1[user.lobbyID][i].connectionID, message);
                }
            }
        }

        public static void SendSpecialEvent(FKUser user, string se)
        {
            se = se.Substring(0, se.LastIndexOf(">") + 1);
            XDocument breakableXMLMessage = XDocument.Parse(se);

            List<string> eventList = new List<string>();

            foreach (var seElement in breakableXMLMessage.Descendants("se").Elements())
            {
                eventList.Add(seElement.ToString());
            }

            //foreach(string specialEvent in eventList)
            //{
            SendMessage(user, @"<h2_0>" + se + @"</h2_0>");
            //}

        }

    }
}
