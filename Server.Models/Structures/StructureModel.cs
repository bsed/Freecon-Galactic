using System.Collections.Generic;
using Core.Models.Enums;
using Freecon.Core.Interfaces;
using Freecon.Models.TypeEnums;
using MongoDB.Bson.Serialization.Attributes;

namespace Server.Models.Structures
{
    public abstract class StructureModel<T> : IStructureModel
        where T : StructureStats, new()
    {
        public StructureTypes StructureType { get; set; }

        public float XPos { get; set; }
        public float YPos { get; set; }

        /// <summary>
        /// Message set to explain why a building has been disabled.
        /// </summary>
        /// <value>
        /// The disable message.
        /// </value>
        public string DisableMessage { get; set; }       

        public float TimeOfLastUpdate { get; set; }

        [BsonIgnore]
        public HashSet<ProblemFlagTypes> ProblemFlags { get; set; }

        // These should be set on each update and only stored in the colony. Can change later if necessary

        public float CurrentHealth { get; set; }
        public float DamageMultiplier { get; set; } //Damage multiplier

        public T Stats { get; set; }

        public bool Enabled { get; set; }

        [BsonId]
        public int Id { get; set; }

        public Weapon Weapon { get; set; }

        public int? OwnerID { get; set; }
        public int? SimulatingPlayerID { get; set; }

        public int? CurrentAreaID { get; set; }

        public bool IsDead { get; set; }

        // Used to prevent killing twice, in case a collision is handled after turret dies

        public ModelTypes ModelType
        {
            get { return ModelTypes.StructureModel; }
        }

        protected StructureModel()
        {
            ProblemFlags = new HashSet<ProblemFlagTypes>();
            Stats = new T();
            Enabled = true;
        }

        protected StructureModel(StructureModel<T> s)
        {
            ProblemFlags = new HashSet<ProblemFlagTypes>();
            StructureType = s.StructureType;
            CurrentHealth = s.CurrentHealth;
            XPos = s.XPos;
            YPos = s.YPos;

            DisableMessage = s.DisableMessage;
            TimeOfLastUpdate = s.TimeOfLastUpdate;

            Stats = s.Stats;

            Enabled = s.Enabled;
            Id = s.Id;
            Weapon = s.Weapon;
            OwnerID = s.OwnerID;
            SimulatingPlayerID = s.SimulatingPlayerID;
            CurrentAreaID = s.CurrentAreaID;
            IsDead = s.IsDead;
        }

        public StructureModel<T> GetClone()
        {
            return (StructureModel<T>)MemberwiseClone();
        }
    }
}