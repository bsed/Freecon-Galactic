using Core.Models.CargoHandlers;

namespace Freecon.Client.Core.Objects
{
    class CargoHandler_ClientVM<ModelType> : CargoHandler_ReadAddRemoveVM<ModelType>
        where ModelType:CargoHandlerModel, new()
    {

    }
}
