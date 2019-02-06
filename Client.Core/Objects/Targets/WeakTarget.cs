//using Microsoft.Xna.Framework; using Core.Interfaces;
//using Client.Interfaces;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using FarseerPhysics.Dynamics;

//namespace Client.Objects.Targets
//{
//    /// <summary>
//    /// Creates a weak reference to the target so that if the target dissapears, our target does too.
//    /// </summary>
//    public class WeakTarget<T> : ITargetable where T : ITargetable
//    {
//        private WeakReference _reference;

//        public bool IsAlliedWithPlanetOwner { get { return ((T)_reference.Target).IsAlliedWithPlanetOwner; } }

//        public HashSet<uint> Teams { get { return ((T)_reference.Target).Teams; } set { ((T)_reference.Target).Teams = value; } }

//        public uint ID { get { return ((T)_reference.Target).ID; } }
        
//        public WeakTarget(T reference)
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

//        public Vector2 Position
//        {
//            get
//            {
//                return ((T)(_reference.Target)).Position;
//            }
//        }

//        public Vector2 LinearVelocity
//        {
//            get
//            {
//                return ((T)(_reference.Target)).LinearVelocity;
//            }
//        }

//        public TargetTypes TargetType
//        {
//            get { return ((T)(_reference.Target)).TargetType; }
//        }

//        public bool IsValid
//        {
//            get { return _reference.IsAlive && ((T)(_reference.Target)).IsValid;}
//        }

        
//    }
//}
