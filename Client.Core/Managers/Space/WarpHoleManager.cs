using System.Collections.Generic;
using FarseerPhysics;
using Microsoft.Xna.Framework;
using Core.Interfaces;
using Freecon.Client.Objects;
using Freecon.Client.Managers.Networking;
using Freecon.Client.Core.Interfaces;
using Core.Models.Enums;
using Microsoft.Xna.Framework.Graphics;

namespace Freecon.Client.Managers
{
    public class WarpHoleManager : ISynchronousUpdate 
    {
        protected MessageService_ToServer _messageManager;
        protected PhysicsManager _physicsManager;
        protected ClientShipManager _clientShipManager;
        public Texture2D WarpholeTexture;
        protected ParticleManager _particleManager;

        public List<WarpHole> Warpholes;
     
        public WarpHoleManager(MessageService_ToServer messageManager,
                               ParticleManager particleManager,
                               PhysicsManager physicsManager,
                               ClientShipManager clientShipManager,
                               Texture2D warpholeTexture)
        {
            _messageManager = messageManager;
            _particleManager = particleManager;
            _physicsManager = physicsManager;
            _clientShipManager = clientShipManager;
            WarpholeTexture = warpholeTexture;
            Warpholes = new List<WarpHole>();
        }

        public void Update(IGameTimeService gameTime)
        {
            for (int i = 0; i < Warpholes.Count; i++)
            {
                Warpholes[i].Update(gameTime);

                if (_particleManager != null)
                {
                    _particleManager.TriggerEffect(ConvertUnits.ToDisplayUnits(Warpholes[i].body.Position),
                                                   ParticleEffectType.WarpHoleEffect, 1);
                }
            }
        }

        public void CreateWarphole(float xpos, float ypos, byte warpIndex, int destinationAreaID)
        {
            // Warps aren't allowed to be in the sun. This prevents any oddities from occuring.
            if (xpos == 0 && ypos == 0)
            {
                // Todo: Log this somehow?
                return;
            }

            var location = new Vector2(xpos, ypos);

            var w = new WarpHole(_messageManager, _physicsManager.World, WarpholeTexture, location, warpIndex, destinationAreaID);
            Warpholes.Add(w);
        }

        public void Clear()
        {
            Warpholes.Clear();
        }

        
    }
}