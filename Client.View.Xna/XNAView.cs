//using Freecon.Client.Core;

namespace Freecon.Client.View.Xna
{
    // Nuked in the merge. :/
    /// <summary>
    /// Allows Managers to interact with XNA without hard references.
    /// Useful for when we decide to integrate MonoGame or switch to 3d.
    /// </summary>
    /// <typeparam name="TViewModel"></typeparam>
    /// <typeparam name="TModel"></typeparam>
    //public abstract class XNAView<TViewModel, TModel> : IView<TViewModel, TModel>
    //    where TViewModel : IViewModel<TModel>
    //{
    //    protected SpriteBatch _spriteBatch { get; set; }

    //    public TViewModel ViewModel { get; protected set; }

    //    public XNAView(
    //        // Todo: Take in config to specify SpriteBatch args, eg draw depth
    //        SpriteBatch spriteBatch,
    //        TViewModel viewModel)
    //    {
    //        _spriteBatch = spriteBatch;
    //        ViewModel = viewModel;
    //    }

    //    public virtual void Update()
    //    {
    //        // Make calls that are UI specific.
    //        // Like getting if the mouse is clicking our button
    //        // ViewModel.ButtonClicked();
    //    }

    //    public virtual void Draw()
    //    {
    //        // Make calls to spritebatch
    //    }
    //}
}
