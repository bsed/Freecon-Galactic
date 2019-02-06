using Freecon.Client.Core.Interfaces;
//using Freecon.Client.Core;

namespace Freecon.Client.View
{
    // This got nuked in the merge. :/
    public interface IView<TViewModel> : IDraw, ISynchronousUpdate
        where TViewModel : IViewModel
    {
        TViewModel ViewModel { get; }
    }
}
