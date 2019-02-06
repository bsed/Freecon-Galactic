//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Client.Interfaces;

//namespace Client.Objects.Targets
//{
//    /// <summary>
//    /// Weak reference to an object which can target other objects
//    /// </summary>
//    /// <typeparam name="T"></typeparam>
//    public class WeakTargeter<T> : ITargeter where T : ITargeter
//    {
//        private WeakReference _reference;

//        public WeakTarget<ITargetable> CurrentTarget { get { return ((T)_reference.Target).CurrentTarget; } set { ((T)_reference.Target).CurrentTarget = value; } }

//        public Dictionary<uint, WeakTarget<ITargetable>> PotentialTargets { get { return ((T)_reference.Target).PotentialTargets; } set { ((T)_reference.Target).PotentialTargets = value; } }

//        public HashSet<UInt32> Teams { get { return ((T)_reference.Target).Teams; } set { ((T)_reference.Target).Teams = value; } }

//        public uint ID { get { return ((T)_reference.Target).ID; } }

//        public bool IsAlliedWithPlanetOwner { get; set; }
        
//        public WeakTargeter(T reference)
//        {
//            _reference = new WeakReference(reference);
//        }

//        public T Reference
//        {
//            get
//            {
//                return (T)(_reference.Target);
//            }
//        }
   
       
//        public bool IsValid
//        {
//            get { return _reference.IsAlive && ((T)(_reference.Target)).IsValid; }
//        }


//    }
//}
