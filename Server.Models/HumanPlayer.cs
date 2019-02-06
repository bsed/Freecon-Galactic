using SRServer.Services;


namespace Server.Models
{
    public class HumanPlayer:Player
    {
        
        public HumanPlayer(int playerID, string name, Account account, LocatorService ls):base(playerID, name, account, ls)
        {
            _model.PlayerType = PlayerTypes.Human;
        }


        public HumanPlayer(PlayerModel p, LocatorService locatorService):base(p, locatorService)
        {
            
        }


    }
    
}
