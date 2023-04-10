using System.Collections.Generic;

public class FKUser
{
    public string connectionID;

    //A1Data

    //User ID in Database
    public int userID;

    //Username
    public string username;

    //Password (SENT VIA PLAIN TEXT --- FK USES REALLY BAD SECURITY!!!)
    public string password;

    //Chat Status
    //0 - Ready to Party
    //1 - Do Not Disturb
    //2 - Playing
    //3 - Partying
    public int status;

    //If the user owns a phone.
    public int phoneStatus;

    //If the user is online.
    public int isOnline;

    //Buddy Lists
    public string[] rawBuddies;
    public List<FKUser> buddies;

    //Chat
    public string bitty;
    public string dl;
    public int lobbyID;
}
