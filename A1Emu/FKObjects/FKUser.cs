using System;
using System.Collections.Generic;
using System.Text;

public class FKUser{
    public string connectionID;

    //A1Data

    //User ID in Database
    public int userID;
    //Username
    public string username;
    //Password (SENT VIA PLAIN TEXT --- FK USES REALLY BAD SECURITY!!!)
    public string password;
    public int status;
    public int isOnline;
    //Buddy List
    public string[] rawBuddies;
    public List<FKUser> buddies;
}