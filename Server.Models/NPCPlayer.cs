using Server.Models.Interfaces;
using SRServer.Services;

namespace Server.Models
{
    public class NPCPlayer : Player
    {

        public override bool IsOnline { get { return true; } }

        public NPCPlayer() {

            _model = new PlayerModel();
            _model.PlayerType = PlayerTypes.NPC;
        }

        public NPCPlayer(PlayerModel p, LocatorService ls):base(p, ls)
        {

        }


        public NPCPlayer(int playerID, string name, LocatorService ls)
        {
            _model = new PlayerModel();

            _model.Id = playerID;
            Username = name;

            _areaLocator = ls.AreaLocator;
            _shipLocator = ls.ShipLocator;
            _accountLocator = ls.AccountLocator;
            _teamLocator = ls.TeamLocator;

            _model.DefaultTeamID = ls.TeamManager.CreateNewTeam(this);
            _model.TeamIDs.Add((int)_model.DefaultTeamID);
            _model.PlayerType = PlayerTypes.NPC;
            
        }

        public override void SetArea(IArea newArea)
        {
            if (newArea != null)
            {   
                _model.CurrentAreaID = newArea.Id;
            }
            else
                _model.CurrentAreaID = null;




        }

        /// <summary>
        /// SHOULD ONLY BE CALLED BY a PlayerManager instance
        /// Logs the player out and pushes the player, account, and all ships to the DB
        /// </summary>
        public override void LogOut(IDBWriter dbWriter)
        {
            return;
        }
        
        public override Account GetAccount()
        {
            return null;
        }

       
    }
}
