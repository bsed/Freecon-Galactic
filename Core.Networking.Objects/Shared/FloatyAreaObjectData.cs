using Core.Models.Enums;
using System.Collections.Generic;

namespace Freecon.Core.Networking.Models.Objects
{
    public class FloatyAreaObjectData
    {
        public float XPos { get; set; }
        public float YPos { get; set; }
        public float Rotation { get; set; }
        public int Id { get; set; }
        public FloatyAreaObjectTypes FloatyType { get; set; }

        /// <summary>
        /// Not used for all floaty types
        /// </summary>
        public HashSet<int> TeamIDs { get; set; }

        /// <summary>
        /// Not used for all floaty types
        /// </summary>
        public int? OwnerID { get; set; }

    }
}
