using Freecon.Client.ViewModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Freecon.Client.Managers;
using System;
namespace Freecon.Client.View.CefSharp
{
    public class ColonyWebView : WebViewBase<ColonyViewModel>
    {
        public ColonyWebView(
            GlobalGameUISingleton gameUiSingleton,
            ColonyViewModel colonyViewModel
            ):base(gameUiSingleton, colonyViewModel)
        {
        }

        public override void Register()
        {
            WebLayer.RegisterCallbackVoid("LeaveToPlanet", s => { });
            WebLayer.RegisterCallbackVoid("LeaveToSpace", s => { });
        }

        public override void Unregister()
        {
            
        }
    }
}
