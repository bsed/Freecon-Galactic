using Freecon.Client.Interfaces;
using Freecon.Client.Objects.Structures;
using Freecon.Client.Objects;
using Freecon.Models.TypeEnums;
using Freecon.Client.Core.Objects;
using Core.Models.Enums;

namespace Freecon.Client.Managers
{
    internal class UserDataManager
    {
        //Empty for now
        //WARNING: consider preallocating UserDataObjects for efficiency
    }


    //TODO: Could probably be simplified by having a single CollisionDataObject class and casting the object based on BodyType

    /// <summary>
    /// This class is used to hold information in userdata for collisions
    /// </summary>
    public class CollisionDataObject
    {
        public int ID;
        public BodyTypes BodyType;

        /// <summary>
        /// The object that this data applies to
        /// </summary>
        public ICollidable Object;

        public CollisionDataObject(ICollidable collidableObject, BodyTypes type)
        {
            BodyType = type;
            ID = collidableObject.Id;
            Object = collidableObject;
        }
    }
    
    public class StructureBodyDataObject : CollisionDataObject
    {
        public Structure Structure;

        public StructureBodyDataObject(BodyTypes type, Structure structure)
            : base(structure, type)
        {          
            Structure = structure;
        }



    }

    public class FloatyAreaBodyDataObject : CollisionDataObject
    {       
        public FloatyAreaObject FloatyObject {get; protected set;}

        public FloatyAreaObjectTypes FloatyType { get { return FloatyObject.Type; } }

        public FloatyAreaBodyDataObject(FloatyAreaObject obj)
            : base(obj, BodyTypes.FloatyAreaObject)
        {
                       
        }



    }

    public class ShipBodyDataObject : CollisionDataObject
    {
        public Ship Ship { get; set; }


        public ShipBodyDataObject(BodyTypes type, int ID, Ship ship):base(ship, type)
        {
            this.Ship = ship;
        }
    }

    public class ProjectileBodyDataObject : CollisionDataObject
    {
        public ICanFire FiringObj { get; set; }
        public IProjectile Bullet { get; set; }

        public ProjectileBodyDataObject(BodyTypes type, int ID, ICanFire ship, IProjectile bullet)
            : base(bullet, type)
        {           
            this.FiringObj = ship;
            this.Bullet = bullet;
        }
    }  

    public class WarpholeBodyDataObject : CollisionDataObject
    {
        public int DestinationAreaID;
        public WarpholeBodyDataObject(WarpHole w, int destinationAreaID):base(w, BodyTypes.WarpHole)
        {
            DestinationAreaID = destinationAreaID;
        }
    }
}