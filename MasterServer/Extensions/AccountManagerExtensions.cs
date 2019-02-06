namespace MasterServer.Extensions
{
    //    public class AccountManagerExtensions
    //    {
    //        /// <summary>
    //        /// Retrieves all accounts from the database. If persistFetch, stores them in the manager
    //        /// </summary>
    //        /// <returns></returns>
    //        public static async Task<ICollection<Account>> GetAllAccountsFromDBAsync(this AccountManager_MasterServer acm, IDatabaseManager dbm, bool persistFetch = true)
    //        {
    //            var accountModels = await dbm.GetAllAccountsAsync();
    //            ICollection<Account> retAccounts = new List<Account>(accountModels.Count());

    //            foreach (var a in accountModels)
    //            {
    //                Account newAccount = new Account(a);
    //                retAccounts.Add(newAccount);
    //                if (persistFetch)
    //                {
    //                    acm.RegisterAccount(newAccount);
    //                }

    //            }

    //            return retAccounts;

    //        }

    //    }
    //}
}
