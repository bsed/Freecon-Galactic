using System;

namespace Freecon.Core.Networking.Models
{
    public class RequiredParameterNotInitialized:Exception
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="containingObject">The object in which the parameter was not set</param>
        public RequiredParameterNotInitialized(string parameterName, object containingObject)
            : base("Error: Parameter " + parameterName + " not set before serialization of " + containingObject.GetType().ToString())
        {
            
        }


    }
}
