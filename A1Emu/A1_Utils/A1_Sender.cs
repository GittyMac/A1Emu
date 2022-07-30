using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NetCoreServer;

public class A1_Sender { 

   public void SendToUser(TcpServer sendingServer, string guid, string message){
      try
      {
         TcpSession session = sendingServer.FindSession(new Guid(guid));

         List<byte[]> d = new List<byte[]>();
         Byte[] reply = Encoding.ASCII.GetBytes(message);
         byte[] b2 = new byte[] {0x00};
         d.Add(reply);
         d.Add(b2);
         byte[] b3 = d.SelectMany(a => a).ToArray();

         Console.WriteLine(message);
         session?.Send(b3, 0, b3.Length);
      }catch(System.FormatException){
         Console.WriteLine("[Error] Couldn't send to user...");
      }
   }

}