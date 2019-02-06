using Microsoft.Xna.Framework.Graphics;
using Freecon.Client.Managers;
using System;
using Freecon.Client.ViewModel;
using Microsoft.Xna.Framework;
using Freecon.Client.View.CefSharp.States;

namespace Freecon.Client.View.CefSharp
{
    public class PortWebView : WebViewBase<PortViewModel>
    {

        private GlobalGameUISingleton _gameUi;

        public PortWebView(
            GlobalGameUISingleton gameUserInterface,
            PortViewModel viewModel
            ) : base(gameUserInterface, viewModel)
        {
            _gameUi = gameUserInterface;
        }


        public override void Register()
        {
        }

        public override void Unregister()
        {
        }
    }
}
