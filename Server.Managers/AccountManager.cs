using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Collections.Concurrent;
using SRServer.Services;
using Server.Models;
using System.Threading.Tasks;
using Server.Database;
using Core.Models.Enums;

namespace Server.Managers
{
    public class AccountManager : IAccountLocator
    {        

        protected ConcurrentDictionary<string, Account> _usernameToAccount = new ConcurrentDictionary<string,Account>();
        protected ConcurrentDictionary<int, Account> _accounts = new ConcurrentDictionary<int,Account>();

        protected ILocalIDManager _accountIDManager;
        protected IDatabaseManager _databaseManager;

        public AccountManager(ILocalIDManager idm, IDatabaseManager dbm)
        {
            _accountIDManager = idm;
            _databaseManager = dbm;

            if (idm.IDType != IDTypes.AccountID)
                throw new Exception("Error: " + idm.GetType().ToString() + " must be of type " + IDTypes.AccountID + " in " + this.GetType().ToString());
        }

        /// <summary>
        /// AddOrUpdates the account with the ID corresponding to acc.
        /// </summary>
        /// <param name="acc"></param>
        public void RegisterAccount(Account acc)
        {
            _accounts.AddOrUpdate(acc.Id, acc, (k, v) => { return v; });
            _usernameToAccount.AddOrUpdate(acc.Username, acc, (k, v) => { return v; });
        }

        public void DeregisterAccount(Account acc)
        {
            Account tempacc;
            _accounts.TryRemove(acc.Id, out tempacc);
            _usernameToAccount.TryRemove(acc.Username, out tempacc);
        }

        /// <summary>
        /// Initializes account manager by filling accounts with password and user data from file at fileName
        /// DEPRECIATED
        /// </summary>
        /// <param name="fileName"></param>
        public AccountManager(string fileName)
        {
            
            _accounts = new ConcurrentDictionary<int, Account>();

            string[] accountData;
            accountData = File.ReadAllLines(fileName); //Reads the whole file, returns each line as a string

            int spaceLoc = 0;

            foreach (string s in accountData)
            {
                //Finds the first space, sets username to be whatever chars are before first space
                spaceLoc = s.IndexOf(' ');
                string[] strings = s.Split(' ');
                List<string> stringList = strings.ToList();
                _accounts.TryAdd(int.Parse(stringList[2]), new Account(stringList[0], stringList[1], int.Parse(stringList[2]),
                                         bool.Parse(stringList[3])));
                //password is from spaceloc+1 to end
            }
            _usernameToAccount = new ConcurrentDictionary<string, Account>();
            foreach (var a in _accounts)
            {
                _usernameToAccount.TryAdd(a.Value.Username, a.Value);
            }



        }
        
        /// <summary>
        /// Sets all account connections to null
        /// </summary>
        private void setAllOffline()
        {
            foreach (var a in _accounts)
                a.Value.connection = null;
        }                  

        /// <summary>
        /// 
        /// </summary>
        /// <param name="accountID"></param>
        /// <param name="fetchFromDB">If true, current account is read from db</param>
        /// <param name="persistFetch">If true and fetchFromDB is true, account is read from DB and instance is stored in manager's list of accounts</param>
        /// <returns></returns>
        public async Task<Account> GetAccountAsync(int? accountID, bool fetchFromDB = true, bool persistFetch = true)
        {
            if (accountID == null)
                return null;//Might do away with nullable accountIDs and force all Player objects to have an account. Thinking about NPCs here.

            Account a = null;//Default value if a isn't found

            if (fetchFromDB)
            {
                a = new Account(await _databaseManager.GetAccountAsync((int)accountID));
                if (persistFetch && a != null)
                {
                    RegisterAccount(a);
                }
            }
            else
            {
                if (_accounts.ContainsKey((int)accountID))
                    a = _accounts[(int)accountID];
            }

            return a;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="fetchFromDB">If true, accounts are read from db</param>
        /// <param name="persistFetch">If true and fetchFromDB is true, accounts are read from DB and instances are stored in manager's list of accounts</param>
        /// <returns></returns>
        public async Task<ICollection<Account>> GetAccountsAsync(IEnumerable<int> ids, bool fetchFromDB = true, bool persistFetch = true)
        {
            ICollection<Account> retAccounts = new List<Account>();

            if (fetchFromDB)
            {
                var accountModels = await _databaseManager.GetAccountsAsync(ids);

                foreach (var a in accountModels)
                {
                    Account newAccount = new Account(a);
                    retAccounts.Add(newAccount);
                    if (persistFetch)
                    {
                        RegisterAccount(newAccount);
                    }

                }
            }
            else
            {
                foreach (var id in ids)
                {
                    if (_accounts.ContainsKey(id))
                    {
                        retAccounts.Add(_accounts[id]);
                    }
                }
            }

            return retAccounts;
        }
        
        /// <summary>
        /// Returns a list of all currently registered accounts.
        /// </summary>
        /// <returns></returns>
        public List<Account> GetAllAccounts()
        {
            return _accounts.Values.ToList();
        }



        #region Account Creation

        /// <summary>
        /// Performs a bunch of checks and returns a created account if succesful
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="persistAccount">If false, account will not be stored in the manager or the db</param>
        /// <returns>Check flags if returned account is null</returns>
        public async Task<Tuple<Account, AccountCreationFailFlags>> CreateAccountAsync(string username, string password, bool persistAccount = true)
        {
            AccountCreationFailFlags failFlags = 0;
            // Checks
            _usernameCharCheck(username, ref failFlags);
            _passwordCheck(password, ref failFlags);
            var res = await _usernameExistsAsync(username, failFlags);
            failFlags = res.Item2;

            Account retAccount = null;
            if (failFlags == 0)
            {

#if DEBUG
                //ConsoleManager.WriteToFreeLine("Creating Account with username " + username);
#endif
                retAccount = new Account(username, password, _accountIDManager.PopFreeID(), false);
                if (persistAccount)
                {                   
                    await _databaseManager.SaveAsync(retAccount);

                    var writeResult = await _usernameExistsAsync(username, 0);
                    if (writeResult.Item1)
                    {
#if DEBUG
                        ConsoleManager.WriteLine("DB write failed when creating a new account.");
#endif
                        _accountIDManager.PushFreeID(retAccount.Id);
                        retAccount = null;
                        failFlags = failFlags | AccountCreationFailFlags.DBWriteFailed;

                    }

                }
            }

            if (persistAccount && retAccount != null)
            {
                RegisterAccount(retAccount);
            }

            return new Tuple<Account, AccountCreationFailFlags>(retAccount, failFlags);
        }

        bool _usernameCharCheck(string username, ref AccountCreationFailFlags failFlags)
        {
            List<char> acceptableUsernameChars = new List<char> { '_', '-', '=' };// We should move this if we want to keep it.

            foreach (char c in username)
            {
                if (!(char.IsLetterOrDigit(c) || acceptableUsernameChars.Contains(c)))
                {
                    failFlags = failFlags | AccountCreationFailFlags.InvalidCharactersUsername;
                    return false;
                }

            }

            return true;


            //return username.All(
            //   c=>{return char.IsLetterOrDigit(c) || acceptableUsernameChars.Contains(c);
            //    });
        }

        bool _passwordCheck(string password, ref AccountCreationFailFlags failFlags)
        {
            // Keeping it simple for now. Complex passwords introduce insecurity.
            if (password.Length < 6)
            {
                failFlags = failFlags | AccountCreationFailFlags.PasswordTooShort;
                return false;
            }

            return true;

        }

        /// <summary>
        /// returned value .Item2 adds to any flags in argument message.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        async Task<Tuple<bool, AccountCreationFailFlags>> _usernameExistsAsync(string username, AccountCreationFailFlags message)
        {

            var loadedAccount = await _databaseManager.GetAccountAsync(username);

            if (loadedAccount != null)
            {
                return new Tuple<bool, AccountCreationFailFlags>(false, message | AccountCreationFailFlags.UsernameExists);
            }

            return new Tuple<bool, AccountCreationFailFlags>(true, message);
        }

        #endregion
    }

    [Flags]
    public enum AccountCreationFailFlags
    {

        UsernameExists = 1,
        InvalidCharactersUsername = 1 << 1,
        InvalidCharactersPassword = 1 << 2,
        PasswordTooShort = 1 << 3,
        DBWriteFailed = 1 << 4,
        NameIsGay = 1 << 5,


    }

}
