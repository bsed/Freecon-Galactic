namespace Freecon.Core.Interfaces
{
    public interface ISerializable
    {
        int Id { get; }

        IDBObject GetDBObject();
    }
}
