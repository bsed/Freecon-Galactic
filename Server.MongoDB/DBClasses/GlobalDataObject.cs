using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Freecon.Core.Interfaces;

namespace Server.MongoDB
{
    public class GlobalDataObject
    {
        public int Id;

        public ModelTypes ModelType { get { return ModelTypes.GlobalDataObject; } }

        public int SolID;

    }
}
