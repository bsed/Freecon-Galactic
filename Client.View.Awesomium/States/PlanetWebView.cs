using Core.Interfaces;
using Freecon.Client.View.CefSharp.States;
using Freecon.Client.ViewModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Freecon.Client.Managers;

namespace Freecon.Client.View.CefSharp
{
    public class PlanetWebView : WebViewBase<PlanetViewModel>
    {
        private GlobalGameUISingleton _gameUi;

        public PlanetWebView(
            GlobalGameUISingleton gameUi,
            PlanetViewModel viewModel
            ) : base(gameUi, viewModel)
        {
            _gameUi = gameUi;
        }

        // C# wrapper, so that calling code doesn't have to know.
        public void EnterModeActivate(bool enterModeActivated)
        {
            _gameUi.GameUI.CallJavascriptFunction("EnterModeActivate", enterModeActivated);
        }
        
        public override void Register()
        {
        }

        public override void Unregister()
        {
        }


    }
}
