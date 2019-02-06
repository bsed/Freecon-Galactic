using System.Collections.Generic;

namespace Freecon.Core.Networking.Models.Messages
{
    /// <summary>
    /// WARNING: While this class is convenient, it introduces extra overhead and might be a little slower than the old direct lidgren writes when many objects are firing. Consider optimizing.
    /// </summary>
    public class MessageObjectFired:MessagePackSerializableObject
    {
        public List<int> ProjectileIDs { get; set; }
        public int FiringObjectID { get { return _firingObjectID; } set { _firingObjectIDSet = true; _firingObjectID = value; } }
        int _firingObjectID;
        bool _firingObjectIDSet;

        public float Rotation { get; set; }
        public byte PercentCharge { get; set; }
        public byte WeaponSlot { get { return _weaponSlot; } set { _weaponSlotSet = true; _weaponSlot = value; } }
        byte _weaponSlot;
        bool _weaponSlotSet;

        public FiringObjectTypes ObjectType { get { return _objectType; } set { _objectTypeSet = true; _objectType = value; } }
        FiringObjectTypes _objectType;
        bool _objectTypeSet;

        public MessageObjectFired()
        {
            ProjectileIDs = new List<int>();
        }

        public override byte[] Serialize()
        {
            if (!_objectTypeSet)
                throw new RequiredParameterNotInitialized("ObjectType", this);

            if(!_weaponSlotSet)
                throw new RequiredParameterNotInitialized("WeaponSlot", this);

            if (!_firingObjectIDSet)
                throw new RequiredParameterNotInitialized("FiringObjectID", this);

            return base.Serialize();
        }


    }

    public enum FiringObjectTypes:byte
    {
        Structure,
        Ship
    }


}
