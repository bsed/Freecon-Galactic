using Freecon.Core.Networking.Models.Objects;
using Freecon.Models.TypeEnums;

namespace Server.Models.Structures
{
    public class CommandCenter : Structure<CommandCenterModel, CommandCenterStats>
    {
        public int OwnerTeamID { get { return _model.OwnerTeamID; } set { _model.OwnerTeamID = value; } }

        private CommandCenter()
        {
            _model = new CommandCenterModel();
        }

        public CommandCenter(float xPos, float yPos, int galaxyID, Player owner, int currentAreaID)            
        {
            _model.StructureType = StructureTypes.CommandCenter;            
            _model.PowerGenerated = 50;            
            _model.Stats.PowerConsumptionRate = 0;
            PosX = xPos;
            PosY = yPos;


            Id = galaxyID;

            _model.OwnerTeamID = owner.DefaultTeamID;
        }

        public CommandCenter(CommandCenterModel cm)
        {
            _model = cm;
        }

        public override Freecon.Core.Networking.Models.Objects.StructureData GetNetworkData()
        {
            return new CommandCenterData(base.GetNetworkData(), OwnerTeamID);
        }

        public float Generate(float elapsedMs)
        {
            return _model.PowerGenerated * elapsedMs;
        }
    
    }

    public class CommandCenterModel : StructureModel<CommandCenterStats>
    {

        public int OwnerTeamID { get; set; }
        public float PowerGenerated { get; set; }

        public CommandCenterModel()
        {
            StructureType = StructureTypes.CommandCenter;
        }
            
        public CommandCenterModel(CommandCenterModel cm):base(cm)
        {
            PowerGenerated = cm.PowerGenerated;
            OwnerTeamID = cm.OwnerTeamID;
            
        }

        public CommandCenterModel GetClone()
        {
            return (CommandCenterModel)MemberwiseClone();
        }
        

    }
}
