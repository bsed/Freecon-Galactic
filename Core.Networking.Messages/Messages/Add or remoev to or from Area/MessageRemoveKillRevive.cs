using System.Collections.Generic;

namespace Freecon.Core.Networking.Models.Messages
{
    public class MessageRemoveKillRevive:MessagePackSerializableObject
    {
        /// <summary>
        /// If true, remove object. If false, kill object.
        /// </summary>
        public ActionType ActionType { get { return _actionType; } set { _actionTypeSet = true; _actionType = value; } }
        ActionType _actionType;
        bool _actionTypeSet;

        public RemovableObjectTypes ObjectType { get { return _objectType; } set { _objectTypeSet = true; _objectType = value; } }
        RemovableObjectTypes _objectType;
        bool _objectTypeSet;
        
        public HashSet<int> ObjectIDs { get; set; }

        /// <summary>
        /// Optional, used if ActionType == ActionType.Revive. If left null, client defaults to 1.
        /// </summary>
        public float? HealthMultiplier { get; set; }

        /// <summary>
        /// Optional, used if ActionType == ActionType.Revive. If left null, client defaults to 1.
        /// </summary>
        public float? ShieldMultiplier { get; set; }
                
        public MessageRemoveKillRevive()
        {
            ObjectIDs = new HashSet<int>();            
        }

        public override byte[] Serialize()
        {
            if(!_actionTypeSet)
                throw new RequiredParameterNotInitialized("ActionType", this);
            if (!_objectTypeSet)
                throw new RequiredParameterNotInitialized("ObjectType", this);

            return base.Serialize();
        }


    }

    public enum ActionType:byte
    {
        Remove,
        Kill,
        Revive
    }

    public enum RemovableObjectTypes:byte
    {
        Structure,
        Ship,
        PortShip,
        FloatyAreaObject,
    }

}
