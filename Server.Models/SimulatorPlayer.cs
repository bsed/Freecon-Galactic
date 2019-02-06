//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Freecon.Core.Networking.Models;

//namespace Server.Models
//{
//    /// <summary>
//    /// Player class corresponding to a SimulatorService
//    /// </summary>
//    public class SimulatorPlayer:Player//TODO: add Player class, refactor Player into clean base class
//    {
//        public HashSet<int> SimulatedIDs;

//        public int? SimulatedAreaID { get { return _model.CurrentAreaId; } set { _model.CurrentAreaId = value; } }

//        public SimulatorPlayer():base()
//        {
//            _model.PlayerType = PlayerTypes.Simulator;
//        }

//        public override void SendMessage(NetworkMessageContainer msg)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
