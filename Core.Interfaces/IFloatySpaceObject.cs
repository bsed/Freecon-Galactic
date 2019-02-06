using Core.Models.Enums;
using Freecon.Core.Networking.Models.Objects;

namespace Freecon.Core.Interfaces
{
    public interface IFloatyAreaObject: IHasPosition
    {
        float PosX { get; set; }

        float PosY { get; set; }

        float Rotation { get; set; }

        int Id { get; }

        FloatyAreaObjectTypes FloatyType { get; }

        int NextAreaID { get; set; }

        FloatyAreaObjectData GetFloatyNetworkData();

    }
}
