namespace Freecon.Models
{
    /// <summary>
    /// TODO: Long overdue, will implement soon
    /// </summary>
    public class GalaxyID
    {
        public int Id {get{return _id;}}

        protected int _id;

        public static implicit operator GalaxyID(int value)
        {
            return new GalaxyID(value);
        }

        protected GalaxyID()
        { }

        public GalaxyID(int value)
        {
            _id = value;
        }

    }

    public class ShipID:GalaxyID
    { }
    
    public class PlayerID : GalaxyID
    { }

    public class AreaID : GalaxyID
    { }

    public class StructureID:GalaxyID
    { }


}
