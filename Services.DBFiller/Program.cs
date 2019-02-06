using System;
using System.Diagnostics;
using System.IO;
using Server.Database;
using Server.Models;
using Server.Managers;

namespace DBFiller
{
    public partial class Program
    {
        private static DBFillerConfig _config;

        public static void Main()
        {
            _config = new DBFillerConfig();

            MongoDatabaseManager _databaseManager = new MongoDatabaseManager();

            _databaseManager.ResetDB();
            //_databaseManager.DeleteCollection("all");

            //_databaseManager.ClearDatabase();            
            DBFiller dbf = new DBFiller(_config, _databaseManager);          
            Helpers.RegisterStateLoader(dbf, new StateLoader());

            //((MongoDatabaseManager)_databaseManager).ClearCollection<ShipModel>("all");
            var t = dbf.FillDB();

            try
            {
                t.Wait();//This occasionally throws missing key exceptions. Probably a race condition. Just try running again.
            }
            catch (Exception e)
            {

            }

            if (t.Exception != null)
                throw t.Exception;
            

            dbf.MockServer.KillThreads();

            ConsoleManager.WriteLine("\n\n\nDatabase filled. You may close this window.", ConsoleMessageType.Notification);
            Console.ReadKey();
            



            return;

        }
        
    }


}
