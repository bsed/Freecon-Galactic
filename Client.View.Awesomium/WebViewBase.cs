using System;
using CefSharp;
using Core.Interfaces;
using Freecon.Client.CefSharpWrapper;
using Freecon.Client.Core.Interfaces;
using Freecon.Client.Managers.GUI;
using Freecon.Client.Mathematics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Freecon.Client.View.CefSharp
{
    public abstract class WebViewBase<TViewModel> : IGameWebView<TViewModel>
        where TViewModel : IViewModel
    {
        protected readonly GlobalGameUISingleton _gameUserInterface;
        
        [JavascriptIgnore]
        public GlobalGameUI WebLayer
        {
            get { return _gameUserInterface.GameUI; }
        }

        public bool IsLoaded
        {
            get { return WebLayer.IsLoaded; }
        }

        public TViewModel ViewModel { get; set; }
        
        public WebViewBase(
            GlobalGameUISingleton gameUiSingleton,
            TViewModel colonyViewModel
            )
        {
            _gameUserInterface = gameUiSingleton;
            ViewModel = colonyViewModel;
        }

        public abstract void Register();
        public abstract void Unregister();

        public virtual void Draw(Camera2D camera)
        {
        }

        public virtual void Update(IGameTimeService gameTime)
        {
        }
    }
}
