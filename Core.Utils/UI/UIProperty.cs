using System;

namespace Freecon.Models
{
    /// <summary>
    /// Decorator which specifies that a property should be serialized as UI data.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class UIProperty : Attribute
    {
        /// <summary>
        /// If name == "", the property name will be used.
        /// </summary>
        public string DisplayName = "";

        /// <summary>
        /// Specify order of display for UI lists. If order==-1, property will be displayed on order of serialization
        /// </summary>
        public int DisplayOrder = -1;

        /// <summary>
        /// Specify the units with which to display a property, if applicable (e.g. m/s for top speed)
        /// </summary>
        public string Units = "";

        /// <summary>
        /// Set to false for data which must be sent but not displayed (i.e. the galaxyID of a module, which must be sent as an identifier in a purchase request)
        /// </summary>
        public bool IsDisplayed = true;


    }
}
