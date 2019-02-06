using Core.Interfaces;
using Freecon.Client.Core.Interfaces;
using Freecon.Client.View.CefSharp;
using Freecon.Client.Mathematics;

namespace Freecon.Client.View
{

    public class PlayableGameWebLayer<TViewModel> : IView<TViewModel>
        where TViewModel : IViewModel
    {
        protected GlobalGameWebLayer _globalGameWebLayer;

        public TViewModel ViewModel { get; protected set; }

        public PlayableGameWebLayer(
            GlobalGameWebLayer globalGameWebLayer,
            TViewModel viewModel)
        {
            _globalGameWebLayer = globalGameWebLayer;
            ViewModel = viewModel;
        }

        public virtual void Activate()
        {
            _globalGameWebLayer.Activate();
        }

        public virtual void Deactivate()
        {
            _globalGameWebLayer.Deactivate();
        }

        public virtual void Update(IGameTimeService gameTime)
        {
            _globalGameWebLayer.Update(gameTime);
        }

        public virtual void Draw(Camera2D camera)
        {
            _globalGameWebLayer.Draw(camera);
        }
               
    }
}
