using System;

namespace Freecon.Models
{
    /// <summary>
    /// Used to notify UIHelper that the marked member is a IHasUIData object and should be sent as a subset of ui data
    /// E.G. when sending data for a ship, ShipStats would be marked [UICollection] and the UI data dictionary would contain a UIGroup representing the ShipStats
    /// The type of a member decorated with this attribute must inherit from IHasUIData for it to be serialized, or it must be a List<t> where t:IHasUIData
    /// </summary>
    public class UICollection : Attribute
    {
        public string DisplayName = "";

    }

    public class UIDictionary : Attribute
    {
        public string DisplayName = "";

    }
}
