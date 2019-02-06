using Lidgren.Network;
using System.Collections.Generic;
using Core.Cryptography;
using Freecon.Core.Interfaces;
using MongoDB.Bson.Serialization.Attributes;

namespace Server.Models
{
    public class Account : ISerializable
    {
        AccountModel _model;

        public NetConnection connection;

        public List<int?> DBIDList { get {return _model.DBIDList;} set{_model.DBIDList = value;} }
        
        public int Id { get{return _model.Id;} set{_model.Id = value;} }
        public bool IsAdmin { get{return _model.IsAdmin;} set{_model.IsAdmin = value; }}

        public int PlayerID { get{return _model.PlayerID;} set{_model.PlayerID = value; }}

        public int LastSystemID { get{return _model.LastSystemID;} set{_model.LastSystemID = value;} }//PSystem AreaID of player on logout, used by Master Server to route clients to the appropriate slave server

        public string Username { get{return _model.Username;} set{_model.Username = value;} }

        public string Password { get{return _model.Password;} set{_model.Password = value;} }

        public bool IsOnline { get{return _model.IsOnline;} set{_model.IsOnline = value;} }

        public bool IsLoginPending { get; set; }

        public float LastLoginTime { get{return _model.LastLoginTime;} set{_model.LastLoginTime = value;} }

        [BsonIgnore]
        public IVKey IVKey { get; set; }

        private Account()
        {
            _model = new AccountModel();
        }

        public Account(AccountModel a) 
        {
            _model = new AccountModel();

            Id = a.Id;
            DBIDList = a.DBIDList;
            IsAdmin = a.IsAdmin;

            PlayerID = a.PlayerID;
            LastSystemID = a.LastSystemID;
            Username = a.Username;
            Password = a.Password;
        }

        public Account(string username, string password, int id, bool isAdmin)
        {
            _model = new AccountModel();
            Username = username;
            Password = password;
            Id = id;
            IsAdmin = isAdmin;
        }

        public IDBObject GetDBObject()
        {
            return _model.GetClone();
        }

    }
    public class AccountModel : IDBObject
    {
        public List<int?> DBIDList { get; set; }

        public int Id { get; set; }
        public bool IsAdmin { get; set; }

        public int PlayerID { get; set; }

        public int LastSystemID { get; set; }//PSystem AreaID of player on logout, used by Master Server to route clients to the appropriate slave server

        public string Username { get; set; }

        public string Password { get; set; }

        public bool IsOnline { get; set; }

        public float LastLoginTime { get; set; }

        public ModelTypes ModelType { get { return ModelTypes.AccountModel; } }
        
        public AccountModel GetClone()
        {
            return (AccountModel)MemberwiseClone();
        }
    }
}