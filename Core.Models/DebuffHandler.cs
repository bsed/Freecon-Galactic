using System.Collections.Generic;
using Core.Models.Enums;

namespace Freecon.Models
{
    public class DebuffHandler
    {
        protected DebuffHandlerModel _model;

        //IReadOnlyDictionary<DebuffTypes, int> Counts { get { return _model.Counts; } }
        
        public float this[DebuffTypes type] { get { return _model.Counts[type] * DebuffHandlerModel.EffectValues[type]; } }

        public DebuffHandler()
        {
            _model = new DebuffHandlerModel();
        }

        /// <summary>
        /// Decrements expired debuffs
        /// </summary>
        /// <param name="currentMS"></param>
        public void Update(float currentMS)
        {
            foreach (var kvp in _model.Counts)
            {
                if (kvp.Value > 0 && currentMS - _model.LastModifyTimes[kvp.Key] > DebuffHandlerModel.LifeTimes[kvp.Key])
                {
                    _model.Counts[kvp.Key]--;
                    _model.LastModifyTimes[kvp.Key] = currentMS;
                }
            }
        }

        /// <summary>
        /// Adds numToAdd to the specified debuff, up to the maximum debuff count. Resets removal timer
        /// </summary>
        /// <param name="type"></param>
        public virtual void AddDebuff(DebuffTypes type, int numToAdd, float currentTimeMS)
        {            
            _model.Counts[type]+=numToAdd;
            _model.LastModifyTimes[type] = currentTimeMS;
            if(_model.Counts[type] > DebuffHandlerModel.MaxCounts[type])
                _model.Counts[type] = DebuffHandlerModel.MaxCounts[type];
        }

        /// <summary>
        /// Removes numToRemove debuffs of the specified type. Does not reset removal timer.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="numToRemove"></param>
        public virtual void RemoveDebuff(DebuffTypes type, int numToRemove)
        {
            _model.Counts[type] -= numToRemove;
            if (_model.Counts[type] < 0)
                _model.Counts[type] = 0;
        }

        public ICollection<KeyValuePair<DebuffTypes, int>> GetNonZeroCounts()
        {
            List<KeyValuePair<DebuffTypes, int>> retList = new List<KeyValuePair<DebuffTypes,int>>();
            foreach(var kvp in _model.Counts)
            {
                if(kvp.Value > 0)
                {
                    retList.Add(kvp);
                }
            }

            return retList;

        }
        /// <summary>
        /// Resets all debuff counts to 0
        /// </summary>
        public void Reset()
        {
            _model.Reset();
        }
    }

    /// <summary>
    /// Debuffs are implemented as simple counters.
    /// </summary>
    public class DebuffHandlerModel
    {
        /// <summary>
        /// Milliseconds
        /// </summary>
        static public IReadOnlyDictionary<DebuffTypes, float> LifeTimes { get{return _lifeTimes;} }
        static Dictionary<DebuffTypes, float> _lifeTimes;


        static public IReadOnlyDictionary<DebuffTypes, int> MaxCounts { get { return _maxCounts; } }
        static Dictionary<DebuffTypes, int> _maxCounts;

        static public IReadOnlyDictionary<DebuffTypes, float> EffectValues { get { return _effectValues; } }
        static Dictionary<DebuffTypes, float> _effectValues;

        public Dictionary<DebuffTypes, int> Counts { get; set; }

        /// <summary>
        /// Last time that the debuff was incremented/decremented. Debuff removal timer resets every time a new debuff is added
        /// </summary>
        public Dictionary<DebuffTypes, float> LastModifyTimes { get; set; }
        

        static DebuffHandlerModel()
        {
            _lifeTimes = new Dictionary<DebuffTypes, float>();
            _maxCounts = new Dictionary<DebuffTypes, int>();
            _effectValues = new Dictionary<DebuffTypes, float>();

            _lifeTimes.Add(DebuffTypes.AttackRate, 10000);
            _lifeTimes.Add(DebuffTypes.Damage, 10000);
            _lifeTimes.Add(DebuffTypes.Defense, 10000);
            _lifeTimes.Add(DebuffTypes.EnergyRegen, 10000);
            _lifeTimes.Add(DebuffTypes.HullPlague, 10000);
            _lifeTimes.Add(DebuffTypes.ShieldRegen, 10000);
            _lifeTimes.Add(DebuffTypes.Thrust, 10000);
            _lifeTimes.Add(DebuffTypes.TopSpeed, 10000);
            _lifeTimes.Add(DebuffTypes.None, 10000);

            _maxCounts.Add(DebuffTypes.AttackRate, 8);
            _maxCounts.Add(DebuffTypes.Damage, 8);
            _maxCounts.Add(DebuffTypes.Defense, 8);
            _maxCounts.Add(DebuffTypes.EnergyRegen, 8);
            _maxCounts.Add(DebuffTypes.HullPlague, 8);
            _maxCounts.Add(DebuffTypes.ShieldRegen, 8);
            _maxCounts.Add(DebuffTypes.Thrust, 8);
            _maxCounts.Add(DebuffTypes.TopSpeed, 8);
            _maxCounts.Add(DebuffTypes.None, 8);

            _effectValues.Add(DebuffTypes.AttackRate, .05f);
            _effectValues.Add(DebuffTypes.Damage, .05f);
            _effectValues.Add(DebuffTypes.Defense, .05f);
            _effectValues.Add(DebuffTypes.EnergyRegen, .05f);
            _effectValues.Add(DebuffTypes.HullPlague, 15);
            _effectValues.Add(DebuffTypes.ShieldRegen, .05f);
            _effectValues.Add(DebuffTypes.Thrust, .05f);
            _effectValues.Add(DebuffTypes.TopSpeed, .05f);
            _effectValues.Add(DebuffTypes.None, .05f);

        }
        
        public DebuffHandlerModel()
        {
            Counts = new Dictionary<DebuffTypes, int>();
            LastModifyTimes = new Dictionary<DebuffTypes, float>();

            Counts.Add(DebuffTypes.AttackRate, 0);
            Counts.Add(DebuffTypes.Damage, 0);
            Counts.Add(DebuffTypes.Defense, 0);
            Counts.Add(DebuffTypes.EnergyRegen, 0);
            Counts.Add(DebuffTypes.HullPlague, 0);
            Counts.Add(DebuffTypes.ShieldRegen, 0);
            Counts.Add(DebuffTypes.Thrust, 0);
            Counts.Add(DebuffTypes.TopSpeed, 0);


            LastModifyTimes.Add(DebuffTypes.AttackRate, 0);
            LastModifyTimes.Add(DebuffTypes.Damage, 0);
            LastModifyTimes.Add(DebuffTypes.Defense, 0);
            LastModifyTimes.Add(DebuffTypes.EnergyRegen, 0);
            LastModifyTimes.Add(DebuffTypes.HullPlague, 0);
            LastModifyTimes.Add(DebuffTypes.ShieldRegen, 0);
            LastModifyTimes.Add(DebuffTypes.Thrust, 0);
            LastModifyTimes.Add(DebuffTypes.TopSpeed, 0);

        }


        public void Reset()
        {
            foreach (var kvp in Counts)
                Counts[kvp.Key] = 0;

            foreach (var kvp in LastModifyTimes)
                LastModifyTimes[kvp.Key] = 0;
        }






    }

}
