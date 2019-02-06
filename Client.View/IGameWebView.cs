using Freecon.Client.Core.Interfaces;

namespace Freecon.Client.View
{
    public interface IGameWebView<TViewModel> : IView<TViewModel>, IHasGameWebView
        where TViewModel : IViewModel
    {
    }

    public interface IHasGameWebView : ISynchronousUpdate, IDraw
    {
        void Register();

        void Unregister();
    }


}
