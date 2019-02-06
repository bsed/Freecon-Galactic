using Freecon.Core.Networking.Models.Objects;

namespace Freecon.Core.Networking.Models.Messages
{
    public class MessageColonizeRequestApproval:MessagePackSerializableObject
    {
        public CommandCenterData ColonyStructure { get { return _colonyStructure; } set { _structureDataSet = true; _colonyStructure = value; } }
        CommandCenterData _colonyStructure;
        bool _structureDataSet;


        public override byte[] Serialize()
        {
            if (!_structureDataSet)
                throw new RequiredParameterNotInitialized("ColonyStructure", this);

            return base.Serialize();
        }

    }
}
