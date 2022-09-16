﻿using System;
using System.IO;
using System.Reflection;
using MySqlConnector;

class Program
{
    static void Main(string[] args)
    {
        //Configuration
        int port = 2000;
        string directory = "";
        string sqServer = "";
        string ip = "localhost";

        string[] t_cleaning_ids = {"70021a"};

        string[] t_familiar_ids = {"80036a", "80035a", "80034a", "80033a", "80032a", "80031a", "80030a", "80029a", 
                                   "80028a", "80027a", "80026a", "80025a", "80017a", "80016a", "80015a", "80007a", 
                                   "80006a", "80005a", "80004a", "80003a", "80002a", "80001a", "80000a"};

        int[] t_jammer_quantities = {1, 5, 10, 25, 50, 100};

        string[] t_mood_ids =     {"80041a", "80040a", "80039a", "80038a", "80037a", "80024a", "80023a", "80022a", 
                                   "80021a", "80020a", "80019a", "80018a", "80013a", "80012a", "80011a", "80010a", 
                                   "80009a", "80008a"};

        string[] t_item_ids =     {"70001a", "70002a", "70003a", "70003b", "70003c", "70003d", "70003e", "70004a", 
                                   "70005a", "70006a", "70006b", "70007a", "70007b", "70007c", "70007d", "70008a", 
                                   "70008b", "70008c", "70008d", "70009a", "70010a", "70011a", "70012a", "70012b", 
                                   "70012c", "70013a", "70014a", "70015a", "70016a", "70017a", "70018a", "70019a", 
                                   "70020a", "70022a", "70022b", "70022c", "70023a", "70024a", "70025a", "70025b",
                                   "70025c", "70026a", "70100a", "70100b", "70100c", "70100d", "70100e", "70100f", 
                                   "70100g", "70101a", "70101b", "70101c", "70101d", "70101e", "70101f", "70101g", 
                                   "70102a", "70102b", "70102c", "70102d", "70102e", "70102f", "70102g", "70103a", 
                                   "70103b", "70103c", "70103d", "70103e", "70103f", "70103g", "70104a", "70104b", 
                                   "70104c", "70104d", "70104e", "70104f", "70104g", "70105a", "70105b", "70105c", 
                                   "70105d", "70105e", "70105f", "70105g", "70106a", "70106b", "70106c", "70106d", 
                                   "70106e", "70106f", "70106g", "70107a", "70107b", "70107c", "70107d", "70107e", 
                                   "70107f", "70107g", "70108a", "70108b", "70108c", "70108d", "70108e", "70108f", 
                                   "70108g", "70109a", "70109b", "70109c", "70109d", "70109e", "70109f", "70109g", 
                                   "70110a", "70110b", "70110c", "70110d", "70110e", "70110f", "70110g", "70111a", 
                                   "70111b", "70111c", "70111d", "70111e", "70111f", "70111g", "70112a", "70112b",
                                   "70112c", "70112d", "70112e", "70112f", "70112g", "70113a", "70113b", "70113c", 
                                   "70113d", "70113e", "70113f", "70113g", "70114a", "70114b", "70114c", "70114d", 
                                   "70114e", "70114f", "70114g", "70115a", "70115b", "70115c", "70115d", "70115e", 
                                   "70115f", "70115g", "70116a", "70116b", "70116c", "70116d", "70116e", "70116f", 
                                   "70116g", "70117a", "70117b", "70117c", "70117d", "70117e", "70117f", "70117g", 
                                   "70118a", "70118b", "70118c", "70118d", "70118e", "70118f", "70118g", "70119a", 
                                   "70119b", "70119c", "70119d", "70119e", "70119f", "70119g", "70120a", "70120b", 
                                   "70120c", "70120d", "70120e", "70120f", "70120g", "70121a", "70121b", "70121c", 
                                   "70121d", "70121e", "70121f", "70121g", "70122a", "70122b", "70122c", "70122d", 
                                   "70122e", "70122f", "70122g", "70123a", "70123b", "70123c", "70123d", "70123e", 
                                   "70123f", "70123g", "70124a", "70124b", "70124c", "70124d", "70124e", "70124f",
                                   "70124g", "70125a", "70125b", "70125c", "70125d", "70125e", "70125f", "70125g", 
                                   "70126a", "70126b", "70126c", "70126d", "70126e", "70126f", "70126g", "70127a", 
                                   "70127b", "70127c", "70127d", "70127e", "70127f", "70127g", "70128a", "70128b", 
                                   "70128c", "70128d", "70128e", "70128f", "70128g", "70129a", "70129b", "70129c", 
                                   "70129d", "70129e", "70129f", "70129g", "70130a", "70130b", "70130c", "70130d", 
                                   "70130e", "70130f", "70130g", "70131a", "70131b", "70131c", "70131d", "70131e", 
                                   "70131f", "70131g", "70132a", "70132b", "70132c", "70132d", "70132e", "70132f", 
                                   "70132g", "70133a", "70133b", "70133c", "70133d", "70133e", "70133f", "70133g", 
                                   "70134a", "70134b", "70134c", "70134d", "70134e", "70134f", "70134g", "70135a", 
                                   "70135b", "70135c", "70135d", "70135e", "70135f", "70135g", "70136a", "70136b", 
                                   "70136c", "70136d", "70136e", "70136f", "70136g", "70137a", "70137b", "70137c", 
                                   "70137d", "70137e", "70137f", "70137g", "70138a", "70138b", "70138c", "70138d", 
                                   "70138e", "70138f", "70138g", "70139a", "70139b", "70139c", "70139d", "70139e", 
                                   "70139f", "70139g", "70140a", "70140b", "70140c", "70140d", "70140e", "70140f", 
                                   "70140g", "70141a", "70141b", "70141c", "70141d", "70141e", "70141f", "70141g", 
                                   "70142a", "70142b", "70142c", "70142d", "70142e", "70142f", "70142g", "70143a", 
                                   "70143b", "70143c", "70143d", "70143e", "70143f", "70143g", "70144a", "70144b", 
                                   "70144c", "70144d", "70144e", "70144f", "70144g", "70145a", "70145b", "70145c", 
                                   "70145d", "70145e", "70145f", "70145g", "70146a", "70146b", "70146c", "70146d", 
                                   "70146e", "70146f", "70146g", "70147a", "70147b", "70147c", "70147d", "70147e", 
                                   "70147f", "70147g", "70148a", "70148b", "70148c", "70148d", "70148e", "70148f", 
                                   "70148g", "70149a", "70149b", "70149c", "70149d", "70149e", "70149f", "70149g", 
                                   "70150a", "70150b", "70150c", "70150d", "70150e", "70150f", "70150g", "70151a", 
                                   "70151b", "70151c", "70151d", "70151e", "70151f", "70151g", "70152a", "70152b", 
                                   "70152c", "70152d", "70152e", "70152f", "70152g", "70153a", "70153b", "70153c", 
                                   "70153d", "70153e", "70153f", "70153g", "70154a", "70154b", "70154c", "70154d", 
                                   "70154e", "70154f", "70154g", "70155a", "70155b", "70155c", "70155d", "70155e", 
                                   "70155f", "70155g", "70156a", "70156b", "70156c", "70156d", "70156e", "70156f", 
                                   "70156g", "70157a", "70157b", "70157c", "70157d", "70157e", "70157f", "70157g", 
                                   "70158a", "70158b", "70158c", "70158d", "70158e", "70158f", "70158g", "70159a", 
                                   "70159b", "70159c", "70159d", "70159e", "70159f", "70159g", "70160a", "70160b", 
                                   "70160c", "70160d", "70160e", "70160f", "70160g", "70161a", "70161b", "70161c", 
                                   "70161d", "70161e", "70161f", "70161g", "70162a", "70162b", "70162c", "70163a", 
                                   "70163b", "70163c", "70163d", "70164a", "70164b", "70165a", "70165b", "70165c", 
                                   "70167a", "70166a", "70168a"};

        if (!File.Exists(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/config"))
        {
            Console.WriteLine("[ERROR] A1Emu did not find a config file in the application's directory.");
        }

        try
        {
            foreach (string line in File.ReadLines(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/config"))
            {
                if (line.Contains("port="))
                {
                    port = Int32.Parse(line.Replace("port=", ""));
                }
                else if (line.Contains("directory="))
                {
                    directory = line.Replace("directory=", "");
                }
                else if (line.Contains("sq="))
                {
                    sqServer = line.Replace("sq=", "");
                }
                else if (line.Contains("ip="))
                {
                    ip = line.Replace("ip=", "");
                }
            }
        }
        catch
        {
            Console.WriteLine("[A1Emu] ERROR! Failed to load the config file.");
        }

        var con = new MySqlConnection(sqServer);

        //TODO - Generate a SQL DB. (Will probably be an external script or a manual README instruction)

        //TODO - Truncate all the multiplayer tables whenever they get added.

        Console.WriteLine("[A1Emu] Updating Trunk DB...");

        int idNum = 0;
        foreach(string cleaning in t_cleaning_ids)
        {
            con.Open();
            var sql = "INSERT IGNORE INTO t_cleaning(id, cost, rid) VALUES(@id, @cost, @rid)";
            using (var cmd = new MySqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@id", idNum);
                cmd.Parameters.AddWithValue("@cost", 100);
                cmd.Parameters.AddWithValue("@rid", cleaning);
                cmd.Prepare();
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
            con.Close();

            idNum += 1;
        }

        idNum = 0;
        foreach(string familiar in t_familiar_ids)
        {
            con.Open();
            var sql = "INSERT IGNORE INTO t_familiar(id, cost, discountedCost, duration, rid) VALUES(@id, @cost, @discountedCost, @duration, @rid)";
            using (var cmd = new MySqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@id", idNum);
                cmd.Parameters.AddWithValue("@cost", 100);
                cmd.Parameters.AddWithValue("@discountedCost", 50);
                cmd.Parameters.AddWithValue("@duration", 720);
                cmd.Parameters.AddWithValue("@rid", familiar);
                cmd.Prepare();
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
            con.Close();

            idNum += 1;
        }

        idNum = 0;
        foreach(string item in t_item_ids)
        {
            con.Open();
            var sql = "INSERT IGNORE INTO t_items(id, cost, rid) VALUES(@id, @cost, @rid)";
            using (var cmd = new MySqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@id", idNum);
                cmd.Parameters.AddWithValue("@cost", 100);
                cmd.Parameters.AddWithValue("@rid", item);
                cmd.Prepare();
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
            con.Close();

            idNum += 1;
        }

        idNum = 0;
        foreach(int jammerQ in t_jammer_quantities)
        {
            con.Open();
            var sql = "INSERT IGNORE INTO t_jammer(id, cost, quantity, rid) VALUES(@id, @cost, @quantity, @rid)";
            using (var cmd = new MySqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@id", idNum);
                cmd.Parameters.AddWithValue("@cost", 100);
                cmd.Parameters.AddWithValue("@quantity", jammerQ);
                cmd.Parameters.AddWithValue("@rid", "80014a");
                cmd.Prepare();
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
            con.Close();

            idNum += 1;
        }

        idNum = 0;
        foreach(string mood in t_mood_ids)
        {
            con.Open();
            var sql = "INSERT IGNORE INTO t_mood(id, cost, rid) VALUES(@id, @cost, @rid)";
            using (var cmd = new MySqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@id", idNum);
                cmd.Parameters.AddWithValue("@cost", 100);
                cmd.Parameters.AddWithValue("@rid", mood);
                cmd.Prepare();
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
            con.Close();

            idNum += 1;
        }

        Console.WriteLine("[A1Emu] Performing SQL cleanup...");

        string sqlResetOnineStatus = "UPDATE user SET isOnline = 0;";
        string sqlTruncateMP3 = "TRUNCATE TABLE mp_3;";
        string sqlTruncateMP5 = "TRUNCATE TABLE mp_5;";

        MySqlCommand resetOnlineStatus = new MySqlCommand(sqlResetOnineStatus, con);
        MySqlCommand truncateMP3 = new MySqlCommand(sqlTruncateMP3, con);
        MySqlCommand truncateMP5 = new MySqlCommand(sqlTruncateMP5, con);
        
        con.Open();
        resetOnlineStatus.ExecuteNonQuery();
        truncateMP3.ExecuteNonQuery();
        truncateMP5.ExecuteNonQuery();
        con.Close();

        var A1_Plugin0 = new ServerSystem(System.Net.IPAddress.Parse(ip), port, sqServer, directory);
        A1_Plugin0.Start();

        Console.WriteLine("[A1Emu] Started!");

        for (;;)
        {
            string line = Console.ReadLine();
            if (string.IsNullOrEmpty(line))
                break;
        }

        A1_Plugin0.Stop();
    }
}
