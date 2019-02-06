namespace Server.Balancers
{
    public class NPCBalancer
    {
        public float TargetNPCPerPlayer { get; protected set; }

        public int _NPCDeficit { get; protected set; }
    
        public NPCBalancer(float targetNPCPerPlayer, int rateAvgQueueLength)
        {


        }


        /// <summary>
        /// Returns a spawn rate multiplier based on the given players/npcs ratio
        /// </summary>
        /// <param name="numPlayers"></param>
        /// <param name="numNPCs"></param>
        /// <returns></returns>
        public float GetSpawnRateMultiplier(int numPlayers, int numNPCs)
        {

            return 0;

        
        }

       


    }
}
