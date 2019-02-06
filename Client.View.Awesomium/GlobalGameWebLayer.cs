using Freecon.Client.Core.Interfaces;
using Freecon.Core.Configs;
using Core.Interfaces;
using Freecon.Client.Core.Managers;
using Freecon.Client.Mathematics;
using Freecon.Client.View.CefSharp.States;

namespace Freecon.Client.View.CefSharp
{
    public class GlobalGameWebLayer : ISynchronousUpdate, IDraw
    {
        IClientWebViewConfig _config;
        GameInterfaceWebView _gameInterfaceWebView;

        private bool _loaded;

        public GlobalGameWebLayer(
            IClientWebViewConfig clientWebViewConfig,
            GameInterfaceWebView gameInterfaceWebView
            )
        {
            _config = clientWebViewConfig;

            _gameInterfaceWebView = gameInterfaceWebView;
        }

        public void Activate()
        {
            // Ensure that we're only loaded once.
            if (_loaded)
            {
                return;
            }

            _loaded = true;

            _gameInterfaceWebView.Load();
        }

        public void Deactivate()
        {
            // Shouldn't do anything because the UI is a singleton.
        }

        public void Update(IGameTimeService gameTime)
        {
            _gameInterfaceWebView.Update(gameTime);
        }

        public void Draw(Camera2D camera)
        {
            _gameInterfaceWebView.Draw(camera);
        }
        
        public void SetUIStateManagerContainer(UIStateManagerContainer container)
        {
            _gameInterfaceWebView.ViewModel.SetUIStateManagerContainer(container);
            _gameInterfaceWebView.SwitchActiveState(container);
        }
    }
}
