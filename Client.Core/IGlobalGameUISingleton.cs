using Core.Interfaces;
using Freecon.Client.Managers.GUI;
using Freecon.Client.Mathematics;

namespace Freecon.Client.View.CefSharp
{
    public interface IGlobalGameUISingleton
    {
        void Draw(Camera2D camera);
        void Load();
        void Register();
        void Unload();
        void Unregister();
        void Update(IGameTimeService gameTime);
    }
}