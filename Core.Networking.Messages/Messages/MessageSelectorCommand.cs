using MsgPack.Serialization;
using System.Collections.Generic;

namespace Freecon.Core.Networking.Models.Messages
{
    public class MessageSelectorCommand : MessagePackSerializableObject
    {
        public SelectorCommands CommandType { get; set; }

        [MessagePackRuntimeType]
        public CommandData CommandData { get; set; }
        public List<int> SelectedIDs { get; set; }

        public int AreaID { get; set; }

        public MessageSelectorCommand()
        {
            SelectedIDs = new List<int>();
        }
    
    }


    public class CommandData
    {        
                
    }

    public class AttackTargetData:CommandData
    {
        public int TargetID { get; set; }

        public AttackTargetData(int targetID)
        {
            TargetID = targetID;
        }
    }

    public class GoToPositionData:CommandData
    {
        public float XPos { get; set; }
        public float YPos { get; set; }

        public GoToPositionData(float xPos, float yPos)
        {
            XPos = xPos;
            YPos = yPos;
        }
    }

    public class AttackToPositionData : CommandData
    {
        public float XPos { get; set; }
        public float YPos { get; set; }

        public AttackToPositionData(float xPos, float yPos)
        {
            XPos = xPos;
            YPos = yPos;
        }
    }

    
}
