using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

public class A1_Parser
{

    //Breaks each message into their own XML string.
    public string[] ParseReceivedMessage(string xmlCommand)
    {
        List<string> commandsList = new List<string>();

        //Splits raw commands by the null character placed in between. To parse Routing Strings.
        string[] rawCommandsList = xmlCommand.Split("\0").Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

        xmlCommand = xmlCommand.Replace("\0", string.Empty);

        XDocument breakableXMLMessage = XDocument.Parse(@"<A1Command>" + xmlCommand + @"</A1Command>");

        foreach (var comElement in breakableXMLMessage.Descendants("A1Command").Elements())
        {
            commandsList.Add(comElement.ToString());
        }

        return rawCommandsList.ToArray();
    }

    //Breaks the command into an array of attributes.
    public string[] ParseCommand(string command)
    {
        List<string> commandInfo = new List<string>();

        string routingString = "";

        if (command.EndsWith("#"))
        {
            routingString = command.Substring(command.LastIndexOf(">") + 1, command.LastIndexOf("#") - command.LastIndexOf(">") - 1);
            command = command.Substring(0, command.LastIndexOf(">") + 1);
        }

        XDocument commandXML = XDocument.Parse(command);
        XElement commandRoot = commandXML.Root;

        commandInfo.Add(commandRoot.Name.LocalName);

        var attrNames = (
           from a in commandRoot.Attributes()
           select a.Value
        );

        foreach (string value in attrNames)
        {
            commandInfo.Add(value);
        }

        //Allows parsing of descendant elements.
        foreach(XElement element in commandRoot.Descendants()){
            var desAttrNames = (
                from a in element.Attributes()
                select a.Value
            );

            foreach (string value in desAttrNames)
            {
                commandInfo.Add(value);
            }
        }

        if (routingString != "")
        {
            string[] routingData = routingString.Split("|");
            foreach (string routeID in routingData)
            {
                commandInfo.Add(routeID);
            }
        }

        return commandInfo.ToArray();
    }

    //Gets the Routing String from the command.
    public string[] ParseRoutingStrings(string command)
    {

        List<string> routeInfo = new List<string>();
        string routingString = "";

        if (command.EndsWith("#"))
        {
            routingString = command.Substring(command.LastIndexOf(">") + 1, command.LastIndexOf("#") - command.LastIndexOf(">") - 1);
            command = command.Substring(0, command.LastIndexOf(">") + 1);
        }

        if (routingString != "")
        {
            string[] routingData = routingString.Split("|");
            foreach (string routeID in routingData)
            {
                routeInfo.Add(routeID);
            }
        }

        return routeInfo.ToArray();
    }

}