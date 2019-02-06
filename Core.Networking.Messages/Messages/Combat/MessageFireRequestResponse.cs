namespace Freecon.Core.Networking.Models.Messages
{
    public class MessageFireRequestResponse : MessagePackSerializableObject
    {
        public FiringObjectTypes FiringObjectType { get { return _firingObjectType; } set { _firingObjectTypeSet = true; _firingObjectType = value; } }
        FiringObjectTypes _firingObjectType;
        bool _firingObjectTypeSet;

        public int FiringObjectID { get { return _firingObjectID; } set { _firingObjectIDSet = true; _firingObjectID = value; } }
        int _firingObjectID;
        bool _firingObjectIDSet;

        public bool Approved { get { return _approved; } set { _approvedSet = true; _approved = value; } }
        private bool _approved;
        private bool _approvedSet;

        public byte WeaponSlot { get { return _weaponSlot; } set { _weaponSlotSet = true; _weaponSlot = value; } }
        private byte _weaponSlot;
        private bool _weaponSlotSet;

        public byte NumProjectiles { get { return _numProjectiles; } set { _numProjectilesSet = true; _numProjectiles = value; } }//In case we have variable numbers of fired projectiles in the future
        private byte _numProjectiles;
        private bool _numProjectilesSet;

        public override byte[] Serialize()
        {
            if (!_firingObjectTypeSet)
                throw new RequiredParameterNotInitialized("FiringObjectType", this);

            if (!_firingObjectIDSet)
                throw new RequiredParameterNotInitialized("FiringObjectID", this);

            if (!_approvedSet)
                throw new RequiredParameterNotInitialized("Approved", this);

            if (!_weaponSlotSet)
                throw new RequiredParameterNotInitialized("WeaponSlot", this);

            if (!_numProjectilesSet)
                throw new RequiredParameterNotInitialized("NumProjectiles", this);

            return base.Serialize();
        }



    }

    


}
