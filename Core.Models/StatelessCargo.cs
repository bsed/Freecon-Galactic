using System;
using Freecon.Models.TypeEnums;
using System.ComponentModel.DataAnnotations;
using Freecon.Core.Interfaces;
using Freecon.Models;
using Freecon.Models.UI;

namespace Core.Models
{
    public class StatelessCargo:IHasUIData
    {
        public virtual string UIDisplayName { get { return Type.ToString().SplitCamelCase(); } }

        [UIProperty(DisplayName="Cargo Type")]
        public StatelessCargoTypes Type;

        [UIProperty]
        public float Quantity = 0;

        [Key]
        public int dbID { get; set; }

        public StatelessCargo()
        {
        }

        public StatelessCargo(StatelessCargoTypes type, float quantity)
        {
            Type = type;
            Quantity = quantity;
        }

        public StatelessCargo(StatelessCargo c)
        {
            Type = c.Type;
            Quantity = c.Quantity;
        }

        /// <summary>
        /// Returns the number of holds each cargo object occupies
        /// Needs to be synced client side
        /// </summary>
        public static float SpacePerObject(StatelessCargoTypes type)
        {
            switch (type)
            {
                case StatelessCargoTypes.Biodome:
                    return 50;


                case StatelessCargoTypes.AmbassadorMissile:
                    return 1f;
                case StatelessCargoTypes.HellHoundMissile:
                    return 1f;
                case StatelessCargoTypes.MissileType1:
                    return 1f;
                case StatelessCargoTypes.MissileType2:
                    return 1f;
                case StatelessCargoTypes.MissileType3:
                    return 1f;
                case StatelessCargoTypes.MissileType4:
                    return 1f;
                case StatelessCargoTypes.Hydrocarbons:
                    return .1f;

                case StatelessCargoTypes.Organics:
                    return .1f;

                default:
                    Console.WriteLine("SpacePerObject not implemented for this cargo of type " + type.ToString());
                    return 1;

            }



        }       


    }    

}
