using System.Collections.Generic;
using Lidgren.Network;
using Freecon.Core.Networking.Models;
using SRServer.Services;
using Server.Models;
using Server.Models.Interfaces;
using Server.Models.Space;
using RedisWrapper;
using Server.Database;
using System.Threading.Tasks;
using Freecon.Core.Networking.ServerToServer;

namespace Server.Managers
{

    /// <summary>
    /// Named global in anticipation of a "local" team manager for temporary games
    /// This class handles teams which are distributed accross all slaves and really only exist in the DB
    /// </summary>
    public class GlobalTeamManager : ITeamLocator, ITeamManager
    {
        private LocalIDManager _teamIDSupplier;
        private int _minIDCount = 1000;

        ConnectionManager _connectionManager;
        RedisServer _redisServer;
        IPlayerLocator _playerLocator;
        IDatabaseManager _databaseManager;
        
        public GlobalTeamManager(LocalIDManager IDSupplier, ConnectionManager cm, RedisServer rs, IPlayerLocator pl, IDatabaseManager dbm)
        {
            _teamIDSupplier = IDSupplier;
            _connectionManager = cm;
            _redisServer = rs;
            _playerLocator = pl;
            _databaseManager = dbm;
        }

                
        /// <summary>
        /// Creates a new team and adds a single player to it.
        /// </summary>
        /// <param name="P"></param>
        /// <returns></returns>
        public int CreateNewTeam(Player P)
        {
            Team t = new Team();
            t.Id = _teamIDSupplier.PopFreeID();
            t.PlayerIDs.Add(P.Id);

            _databaseManager.SaveAsync(t);
            return t.Id;
        }
        
        /// <summary>
        /// /// Creates a new team, fills with ships provided in list
        /// Sends addTeam Command to all passed ships
        /// </summary>
        /// <param name="players"></param>
        /// <returns></returns>
        public int CreateNewTeam(List<Player> players)
        {
            var areas = new HashSet<IArea>(); //Keeps track of all areas which need to recieve new team information

            int teamID = _teamIDSupplier.PopFreeID();
            Team tempTeam = new Team();
            tempTeam.Id = teamID;
            
            
            
            NetOutgoingMessage msg;
            foreach (Player p in players)
            {
                tempTeam.PlayerIDs.Add(p.Id);
                p.AddTeam(tempTeam);


                areas.Add(p.GetArea()); //Hashtables automatically make sure each area is only added once
            }
            //make sure the sent message only contains information about the team in the appropriate system

            _databaseManager.SaveAsync(tempTeam);
            
            foreach (var a in areas)
            {
                if (a.NumOnlinePlayers == 0)
                    continue;

                MessageAddRemoveShipsTeam msgData = new MessageAddRemoveShipsTeam();
                msgData.TeamID = tempTeam.Id;
                msgData.AddOrRemove = true;

                foreach (var s in a.GetShips())
                {
                    if (s.Value.PlayerID != null && tempTeam.PlayerIDs.Contains((int)s.Value.PlayerID))
                        msgData.IDs.Add(s.Value.Id);
                }

                a.BroadcastMessage(new NetworkMessageContainer(msgData, MessageTypes.AddRemoveShipsTeam));
            }


            return tempTeam.Id;
        }


        /// <summary>
        /// Quickly implemented for DBFiller. Do not use.
        /// </summary>
        /// <param name="players"></param>
        /// <returns></returns>
        public int DebugCreateNewTeam(List<Player> players)
        {

            var areas = new HashSet<IArea>(); //Keeps track of all areas which need to recieve new team information

            List<int> IDsToSend = new List<int>();
            int teamID = _teamIDSupplier.PopFreeID();
            Team tempTeam = new Team();
            tempTeam.Id = teamID;


            var sendToThese = new List<NetConnection>(3);
            foreach (Player p in players)
            {
                tempTeam.PlayerIDs.Add(p.Id);
                p.AddTeam(tempTeam);


                areas.Add(p.GetArea()); //Hashtables automatically make sure each area is only added once
            }
            //make sure the sent message only contains information about the team in the appropriate system

            _databaseManager.SaveAsync(tempTeam);

            return tempTeam.Id;
        }
       

        /// <summary>
        /// Adds a IShip to a team appropriately
        /// handles linking team to IShip as well
        /// Sends notice to clients of team addition
        /// Creates new team if teamID is not in use
        /// </summary>
        /// <returns>Returns true if successful</returns>
        public async Task<bool> AddPlayerToTeamAsync(Player p, int teamID)
        {
            var teamModel = await _databaseManager.GetTeamAsync(teamID);

            Team team;

            if(teamModel == null)
            {
                team = new Team();
                team.Id = _teamIDSupplier.PopFreeID();
                await _databaseManager.SaveAsync(team);
            }
            else
            {
                team = new Team(teamModel);
            }
            
            if (team.PlayerIDs.Contains(p.Id))
                return false;

            team.PlayerIDs.Add(p.Id);
            p.AddTeam(team);

            await _databaseManager.SaveAsync(team);

            if (p.CurrentAreaID == null || p.GetArea().GetOnlinePlayers().Count == 0 || p.GetArea() is Limbo)
                return true;


            var area = p.GetArea();
            if (area.NumOnlinePlayers < 2)//If p is the only player in the area, no need to send a message
                return true;

            MessageAddRemoveShipsTeam data = new MessageAddRemoveShipsTeam();
            if (p.ActiveShipId != null)
                data.IDs.Add((int)p.ActiveShipId);
            else
                return true;
            data.TeamID = team.Id;

            area.BroadcastMessage(new NetworkMessageContainer(data, MessageTypes.AddRemoveShipsTeam));


            return true;
        } 
       
        /// <summary>
        /// Returns true if any of the teams in teams1 are contained in teams2
        /// </summary>
        /// <param name="teams1"></param>
        /// <param name="teams2"></param>
        /// <returns></returns>
        public bool AreAllied(ITeamable t1, ITeamable t2)
        {
            HashSet<int> teams1 = t1.GetTeamIDs();
            HashSet<int> teams2 = t2.GetTeamIDs();
            foreach (int id in teams1)
            {                
                if (teams2.Contains(id))
                    return true;
                
            }
            return false;

        }

        /// <summary>
        /// Dissolves the team with the given team number
        /// I kind of half assed this function, needs testing
        /// </summary>
        /// <param name="teamNumber"></param>
        /// <returns></returns>
        public void dissolveTeam(Team team)
        {
            var areas = new HashSet<IArea>(); //Keeps track of all areas which need to recieve notification

           

            HashSet<int> playerIDs = team.PlayerIDs;



            HashSet<int> nonlocalPlayerIDs = new HashSet<int>();

            foreach (int i in team.PlayerIDs)
            {

                Player p = _playerLocator.GetPlayerAsync(i).Result;
                if (p == null)
                    nonlocalPlayerIDs.Add(i);
                else
                {
                    p.RemoveTeam(team.Id);
                    areas.Add(p.GetArea()); 
                }                
            }


            if (nonlocalPlayerIDs.Count != 0)
            {
                NetworkMessageContainer msgc = new NetworkMessageContainer();
                msgc.MessageData = new PlayerRemoveTeam(nonlocalPlayerIDs, team.Id);
                _redisServer.PublishObject(MessageTypes.Redis_PlayerRemoveTeam, msgc);
            }

           

            //Send removeships message to areas in which this team exists
            foreach (var a in areas)
            {
                if(a.NumOnlinePlayers == 0)
                    continue;
                
                
                var data = new MessageAddRemoveShipsTeam();
                data.AddOrRemove = false;
                data.TeamID = team.Id;

                foreach (var s in a.GetShips())
                {
                    if (team.PlayerIDs.Contains(s.Value.Id))
                        data.IDs.Add(s.Value.Id);
                }

                a.BroadcastMessage(new NetworkMessageContainer(data, MessageTypes.AddRemoveShipsTeam));
              
            }

            _databaseManager.DeleteTeam(team.Id);
            _teamIDSupplier.PushFreeID(team.Id);
        }

    }

    
}