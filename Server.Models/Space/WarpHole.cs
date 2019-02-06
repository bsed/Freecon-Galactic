using Freecon.Core.Interfaces;
using System.ComponentModel.DataAnnotations;
namespace Server.Models
{
    public class Warphole:IHasPosition
    {
        public int DestinationAreaID { get; set; }
        public int ParentAreaID { get; set; }
        public byte warpIndex { get; set; } //Index in Warpholes array in the PSystem class
        public float PosX { get; set; }
        public float PosY { get; set; }

        [Key]
        public int Id { get; set; }

        //WARNING: Needs to be initiated properly when the Warpholes are added to the system

        public Warphole()
        {
        }

        public Warphole(float posX, float posY)
        {
            PosX = posX;
            PosY = posY;
        }

        public Warphole(float posX, float posY, int parentAreaID, int destinationAreaID, byte warpIndex)
        {
            PosX = posX;
            PosY = posY;
            this.ParentAreaID = parentAreaID;
            this.warpIndex = warpIndex;
            this.DestinationAreaID = destinationAreaID;
        }
    }
}