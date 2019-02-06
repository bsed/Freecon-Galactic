//using CEFSharpUI Lib;
//using Freecon.Client.Core.Interfaces;
//using Microsoft.Xna.Framework; using Core.Interfaces;
//using Microsoft.Xna.Framework.Graphics;

//namespace Freecon.Client.View.CefSharp
//{
//    public abstract class CefSharpWebView<TWebView, TViewModel> : IGameWebView<TViewModel>
//        where TWebView : CEFSharpUI
//        where TViewModel : IViewModel
//    {
//        protected SpriteBatch _spriteBatch;

//        public virtual CEFSharpUI WebLayer { get; protected set; }

//        /// <summary>
//        /// Like WebLayer but you don't have to cast.
//        /// </summary>
//        public virtual TWebView WebView { get; protected set; }

//        public virtual TViewModel ViewModel { get; protected set; }

//        public CefSharpWebView(TWebView webLayer, TViewModel viewModel, SpriteBatch spriteBatch)
//        {
//            _spriteBatch = spriteBatch;
//            WebLayer = webLayer;
//            WebView = webLayer;
//            ViewModel = viewModel;
//        }

//        public virtual void Draw(Camera2D camera)
//        {
//            WebLayer.Draw(camera);
//        }

//        public virtual void Update(IGameTimeService gameTime)
//        {
//            WebLayer.Update(gameTime);
//        }

//        public virtual void Load(int renderTargetWidth, int renderTargetHeight, Point position, string url)
//        {
//            WebLayer.Load(renderTargetWidth, renderTargetHeight, position, url);
//        }

//        public virtual void Unload()
//        {
//            WebLayer.Unload();
//        }
//    }
//}
