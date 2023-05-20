using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using NetCoreServer;
using A1Emu.A1.Utils;
using A1Emu.A1.Objects;

namespace A1Emu.A1.Plugins.MP
{
    public class A1_Game
    {
      private FKUser[] users = new FKUser[2];
      private TcpServer server;
      private int p1_score = 0;
      private int p2_score = 0;

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

            A1_Sender.SendToUser(server, users[1].connectionID, System.Text.ASCIIEncoding.ASCII.GetString(responseStream.ToArray()));

            return "<notneeded />";
        }

    }
}
