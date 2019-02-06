using Core.Models.Enums;

namespace Freecon.Core.Networking.Models.ServerToServer
{
    public class MessageIDResponse:MessagePackSerializableObject
    {
        /// <summary>
        /// The slave to which these IDs are targetted
        /// </summary>
        public int SlaveServerID { get; set; }

        public int[] IDs { get; set; }

        public IDTypes IDType { get { return _idType; } set { _idTypeSet = true; _idType = value; } }
        IDTypes _idType;
        bool _idTypeSet;


        public override byte[] Serialize()
        {
            if (!_idTypeSet)
                throw new RequiredParameterNotInitialized("IDType", this);

            return base.Serialize();
        }
    }
}
