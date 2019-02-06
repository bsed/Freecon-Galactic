using System.Collections.Concurrent;
using Server.Models;
using Server.Managers;
using Server.Database;
using Freecon.Core.Utils;

namespace MasterServer
{
    public class AccountManager_MasterServer: AccountManager
    {

        ConcurrentDictionary<string, Account> _usernameToAccount = new ConcurrentDictionary<string, Account>();
        ConcurrentDictionary<int, Account> _accounts = new ConcurrentDictionary<int, Account>();     
        


        /// <summary>
        /// Initializes account manager by filling accounts with passed List
        /// </summary>
        /// <param name="fileName"></param>
        public AccountManager_MasterServer(ILocalIDManager idm, IDatabaseManager dbm, bool fetchAccountsFromDB = true)
            : base(idm, dbm)
        {
            
            if(fetchAccountsFromDB)
            {
                var accountModels = _databaseManager.GetAllAccountsAsync().Result;

                foreach (var a in accountModels)
                {
                    Account newAccount = new Account(a);
                    RegisterAccount(newAccount);
                }
            }
        }

               

        /// <summary>
        /// index is index of player, to be used as player ID
        /// message is used to return resulting messages, i.e. login successful
        /// returns false if login fails
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool TryLogin(string username, string password, out string message, out int lastSystemID, out int accountID)
        {
            message = "";
            lastSystemID = 0;
            accountID = 0;
            if (!_usernameToAccount.ContainsKey(username))
            {
                message = "Username not found";
                return false;
            }
            Account a = _usernameToAccount[username];

            if (a.IsOnline && TimeKeeper.MsSinceInitialization > a.LastLoginTime + 4000)//Allow for a timeout in case first message isn't received in time by client
            {
                message = a.Username + " is already online!";
                ConsoleManager.WriteLine(message, ConsoleMessageType.Notification);
                return false;
            }
            else if (a.Password != password)
            {
                message = "Incorrect Password";
                return false;
            }
            else
            {
                a.IsOnline = true;
                a.LastLoginTime = TimeKeeper.MsSinceInitialization;
                message = "Login successful";
                lastSystemID = a.LastSystemID;
                accountID = a.Id;
                return true;
            }
        }

        /// <summary>
        /// Finds the index of the player in PM with the username
        /// sets isOnline to false.
        /// </summary>
        /// <param name="username"></param>
        public void LogOut(string username)
        {
            if (_usernameToAccount.ContainsKey(username))
                _usernameToAccount[username].IsOnline = false;
        }

        public void LogOut(int ID)
        {
            //_accounts[ID].IsOnline = false;

        }            

     
    }

    
}