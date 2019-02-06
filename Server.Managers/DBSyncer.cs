using System.Collections.Generic;
using Server.Models.Interfaces;
using System.Diagnostics;
using System.Timers;
using Server.Models;

namespace Server.Managers
{
    public class DBSyncer
    {
        IDBWriter _DBWriter;
        Timer _syncTimer;
        Stopwatch _time;

        GalaxyManager _galaxyManager;
        ShipManager _shipManager;
        PlayerManager _playerManager;
        AccountManager _accountManager;
        StructureManager _structureManager;

  
        public DBSyncer(IDBWriter DBWriter, GalaxyManager gm, ShipManager sm, PlayerManager pm, AccountManager am, StructureManager scm)
        {
            _DBWriter = DBWriter;
            _galaxyManager = gm;
            _shipManager = sm;
            _playerManager = pm;
            _accountManager = am;
            _structureManager = scm;

            _time = new Stopwatch();
            _time.Start();
        }

        public void SyncAllToDB(object sender, ElapsedEventArgs e)
        {
            //CallTimer ct = new CallTimer();


            ////TODO: Add syncing lock to all of these
            ////Note: the .Wait() on all of these is to avoid raping the CPU

            //ConsoleManager.WriteLine("Writing...");
            //ConsoleManager.WriteLine("Areas: " + ct.Time(()=> _DBWriter.SaveAsyncBulk(_galaxyManager.AllAreas.Values).Wait()).ToString());
            //ConsoleManager.WriteLine("Players: " + ct.Time(() => _playerManager.SyncPlayers(_DBWriter).Wait()).ToString());
            //ConsoleManager.WriteLine("Ships: " + ct.Time(() => _DBWriter.SaveAsyncBulk(_shipManager.GetAllShips()).Wait()).ToString());
            //ConsoleManager.WriteLine("Accounts: " + ct.Time(() => _DBWriter.SaveAsyncBulk(_accountManager.GetAllAccounts()).Wait()).ToString());
            //ConsoleManager.WriteLine("Structures: " + ct.Time(() => _DBWriter.SaveAsyncBulk(_structureManager.GetAllObjects()).Wait()).ToString());
            //ConsoleManager.WriteLine("Done.");

            var t = sender as Timer;
            if (t != null)
                t.Start();

        }

    }
}
