using System.Collections.Generic;
using Freecon.Client.Managers.Networking;
using Core.Interfaces;
using Freecon.Models.TypeEnums;
using Freecon.Client.Core.Interfaces;

namespace Freecon.Client.Managers
{
    public class CollisionManager : ISynchronousUpdate
    {
        
        MessageService_ToServer _messageManager;

        //May want to convert to dictionary later, if we have valid/invalid collisions to report
        List<Collision> _collisionsToReport = new List<Collision>();

        double _reportInterval = 250;//ms, time between collision reports
        double _lastUpdateTime = 0;

        public CollisionManager(MessageService_ToServer messageManager)
        {
            _messageManager = messageManager;

        }

        public void Reset()
        {
            _collisionsToReport.Clear();

        }

        public void ReportCollision(int colideeID, int projectileID, ProjectileTypes projectileType, byte pctCharge, byte weaponSlot)
        {
            _collisionsToReport.Add(new Collision(colideeID, projectileID, projectileType, pctCharge, weaponSlot));
        }

        public void Update(IGameTimeService gameTime)
        {
            if (gameTime.TotalMilliseconds - _lastUpdateTime > _reportInterval)
            {
                _messageManager.ReportCollisions(_collisionsToReport);
                _collisionsToReport.Clear();
            }
        }
        

        /// <summary>
        /// Sends report containing all current collisions to the server
        /// </summary>
        void _flushCollisions()
        {

        }
        

        public class Collision
        {
            public int HitObjectID { get; private set; }

            public int ProjectileID { get; private set; }

            public ProjectileTypes ProjectileType { get; private set; }

            public byte PctCharge { get; private set; }

            public byte WeaponSlot { get; private set; }

            public Collision(int hitObjectID, int projectileID, ProjectileTypes projectileType, byte pctCharge, byte weaponSlot)
            {
                HitObjectID = hitObjectID;

                ProjectileID = projectileID;

                ProjectileType = projectileType;

                PctCharge = pctCharge;

                WeaponSlot = weaponSlot;

            }
        }
    }
}
