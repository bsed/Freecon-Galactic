using Server.Models.Database;
using Server.Utilities;
using System.Collections.Generic;
using Freecon.Core.Interfaces;


namespace Server.Models
{

    //public class Team
    //{
    //    HashSet<int> _playerIDs;
    //    public int ID { get; private set; }

    //    ITeamManager _teamManager;


    //    private Team() { }

    //    public Team(int teamNumber, ITeamManager teamManager)
    //    {
    //        this.ID = teamNumber;
    //        _playerIDs = new HashSet<int>();
    //        _teamManager = teamManager;
    //    }

    //    public HashSet<int> GetPlayerIDs()
    //    {
    //        return new HashSet<int>(_playerIDs);
    //    }

    //    public void AddPlayer(Player p)
    //    {
    //        _teamManager.AddPlayerToTeamAsync(p, this.ID);
    //    }

    //    public void RemovePlayer(Player p)
    //    {
    //        _playerIDs.Remove(p.Id);
    //    }
    //}

    /// <summary>
    /// Teams only "exist" on the database. They are temporarily read into memory to be accessed and then returned to the db.
    /// </summary>
    public class Team : ISerializable
    {
        public HashSet<int> PlayerIDs { get; set; }

        public int Id { get; set; }

        public Team()
        {
            PlayerIDs = new HashSet<int>();
        }

        public Team(TeamModel t)
        {
            Id = t.Id;
            PlayerIDs = new HashSet<int>();
            foreach (int i in SerializationUtilities.DeserializeIntList(t.PlayerIDs))
            {
                PlayerIDs.Add(i);
            }

        }


        public virtual IDBObject GetDBObject()
        {
            TeamModel t = new TeamModel();
            t.Id = Id;
            t.PlayerIDs = SerializationUtilities.SerializeList(new List<int>(PlayerIDs));

            return t;
        }

        

    }
    
}
