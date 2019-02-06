namespace Freecon.Core.Interfaces
{
    public interface IDBObject
    {
        int Id { get; set; }

        ModelTypes ModelType { get; }
    
    }
}
