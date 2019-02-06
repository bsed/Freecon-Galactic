using Freecon.Client.ViewModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Freecon.Client.Managers;
using System;

namespace Freecon.Client.View.CefSharp
{
    public class SpaceWebView : WebViewBase<SpaceViewModel>
    {
        protected SpaceViewModel ViewModel;

        public SpaceWebView(
            GlobalGameUISingleton gameUserInterface,
            SpaceViewModel viewModel
            ) : base(gameUserInterface, viewModel)
        {
            
        }

        public override void Register()
        {
            //WebLayer.RegisterCallbackVoid("ToggleStructurePlacementMode", ViewModel.ToggleStructurePlacementMode);
        }

        public override void Unregister()
        {
        }
    }
}
