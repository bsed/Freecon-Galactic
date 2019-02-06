using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Freecon.Client.Core
{
    public abstract class BaseViewModel<TModel> : IViewModel<TModel>
    {
        public TModel Model { get; protected set; }

        public BaseViewModel(TModel model)
        {
            Model = model;
        }

        public virtual void Update()
        {

        }
    }

    public interface IViewModel<TModel> : IUpdatable
    {
        TModel Model { get; }

        void Update();
    }

    public interface IUpdatable
    {
        void Update();
    }
}
