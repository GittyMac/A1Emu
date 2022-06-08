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
using System.Xml.Linq;

public class A1_Parser { 

   public string[] ParseReceivedMessage(string xmlCommand){
      List<string> commandsList = new List<string>();

      //Splits raw commands by the null character placed in between. To parse Routing Strings.
      string[] rawCommandsList = xmlCommand.Split("\0").Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

      xmlCommand = xmlCommand.Replace("\0", string.Empty);

      XDocument breakableXMLMessage = XDocument.Parse(@"<A1Command>" + xmlCommand + @"</A1Command>");  

      foreach(var comElement in breakableXMLMessage.Descendants("A1Command").Elements())
      {
         commandsList.Add(comElement.ToString());
      }

      return rawCommandsList.ToArray();
   }

   public string[] ParseCommand(string command){
      List<string> commandInfo = new List<string>();

      string substring = "";

      if(command.EndsWith("#")){
         substring = command.Substring(command.LastIndexOf(">") + 1, command.LastIndexOf("#") - command.LastIndexOf(">") - 1);
         command = command.Substring(0, command.LastIndexOf(">") + 1);
      }

      XDocument commandXML = XDocument.Parse(command);
      XElement commandRoot = commandXML.Root;

      commandInfo.Add(commandRoot.Name.LocalName);
      
      var attrNames = (
         from a in commandRoot.Attributes()
         select a.Value
      );

      foreach(string value in attrNames){
         commandInfo.Add(value);
      }

      if(substring != ""){
         string[] substringData = substring.Split("|");
         foreach(string substringID in substringData){
            commandInfo.Add(substringID);
         }
      }

      return commandInfo.ToArray();
   }

}