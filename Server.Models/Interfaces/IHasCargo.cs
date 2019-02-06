using Core.Models.CargoHandlers;

namespace Server.Models.Interfaces
{
    //I'll move this later. Interfaces make it seem as though our code is excessively fragmented into projects
    public interface IHasCargo
    {
        int Id { get; }

        CargoHandler_ReadOnlyVM<CargoHandlerModel> GetCargo();

    }
   
}
