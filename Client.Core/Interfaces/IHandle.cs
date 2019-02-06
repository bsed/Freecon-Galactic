namespace Freecon.Client.Interfaces
{
    public interface IHandle<in T>
    {
        void Handle(T message);
    }

}
