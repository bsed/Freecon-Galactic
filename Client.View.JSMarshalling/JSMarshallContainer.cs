using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Client.View.JSMarshalling
{
    public class JSMarshallContainer
    {
        public string MethodName;

        public List<object> Arguments;

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
