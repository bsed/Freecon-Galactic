using Core.Models.Enums;
namespace Server.Managers
{
    public interface ILocalIDManager
    {
        IDTypes IDType { get; }
        int PopFreeID();
        void PushFreeID(int ID);
        void ReceiveServerIDs(int[] newIDs);
    }
}
