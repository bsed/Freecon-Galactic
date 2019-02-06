using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.View.JSMarshalling.ToClient;

namespace Client.View.JSMarshalling
{
    public class ChangeZoom:JSMarshallContainer
    {
        public ZoomDirection ZoomDirection;

        public float Amount = .1f;

        public ChangeZoom()
        {
            MethodName = "ChangeZoom";
        }
    }

    public enum ZoomDirection
    {
        In,
        Out
    }
}
