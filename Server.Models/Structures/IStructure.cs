using System.Collections.Generic;
using Core.Models.Enums;
using Freecon.Core.Interfaces;
using Freecon.Core.Networking.Models.Objects;
using Freecon.Models.TypeEnums;

using Server.Models.Interfaces;

namespace Server.Models.Structures
{
    public interface IStructure : IHasGalaxyID, ISimulatable, ISerializable, IHasPosition
    {
        int? OwnerID { get; set; }

        int? CurrentAreaId { get; }

        bool IsDead { get; set; }

        /// <summary>
        /// Allows colony owner to disable buildings so that they don't take energy.
        /// </summary>
        bool Enabled { get; }

        StructureTypes StructureType { get; }

        StructureData GetNetworkData();

        /// <summary>
        /// Faster than checking with is keyword
        /// </summary>
        bool IsResourceStructure { get; }

        int Id { get; set; }

        float DamageMultiplier { get; set; }

        int PeopleRequired { get; set; }

        float PowerConsumptionRate { get; set; }

        void Enable();

        void Disable(string message);

        float Update(float elapsedMS);

        void Kill(int projectileId);

        HashSet<ProblemFlagTypes> ProblemFlags { get; }

        /// <summary>
        /// Checks if this structure overlaps with given size and position parameters of another object
        /// </summary>
        /// <param name="sizex"></param>
        /// <param name="sizey"></param>
        /// <param name="posx"></param>
        /// <param name="posy"></param>
        /// <returns></returns>
        bool CheckOverlap(float sizex, float sizey, float posx, float posy);

        Weapon Weapon { get; set; }

        /// <summary>
        /// Returns the maximum available supply rate as a function of distance
        /// </summary>
        /// <param name="distance"></param>
        /// <returns></returns>
        float GetSupplyRate(float distance);

        float CurrentHealth { get; }

        #region Reference Getters and Setters

        IStructure SetArea(IArea newArea);

        IStructure SetPlayer(Player p);

        void SetID(int ID);

        #endregion

        /// <summary>
        /// Recursively returns all hard-referenced, nested objects which need to be registered. Implemented for DB loading.
        /// </summary>
        /// <returns></returns>
        ICollection<IHasGalaxyID> GetRegisterableNestedObjects();

    }
}