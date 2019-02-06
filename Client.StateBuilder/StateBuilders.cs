using Freecon.Client.Managers;
using Freecon.Client.Managers.States;
using Microsoft.Xna.Framework.Graphics;
using Freecon.Client.Core.Factories;
using Freecon.Client.Core.Managers;
using Freecon.Client.Core.States;
using Freecon.Client.Managers.Networking;
using Freecon.Client.Managers.Space;
using Core.Networking;
using Freecon.Client.View.CefSharp;
using Freecon.Core.Configs;
using Freecon.Core.Networking.Interfaces;
using RedisWrapper;

namespace Freecon.Client.StateBuilders
{

    public class SimulatorStateBuilder
    {
        public static SpaceStateManager BuildSpaceStateManager(int areaID, RedisServer _redisServer, INetworkingService _networkingService)
        {
            return DrawlessStateBuilder.BuildSpaceStateManager(areaID, new RedisMessenger(_redisServer, areaID),
                _networkingService);

        }

        public static PlanetStateManager BuildPlanetStateManager(int areaID, RedisServer _redisServer, INetworkingService _networkingService, PhysicsConfig _physicsConfig)
        {
            return DrawlessStateBuilder.BuildPlanetStateManager(areaID, new RedisMessenger(_redisServer, areaID),
                _networkingService, _physicsConfig);

        }

    }

    public class BotStateBuilder
    {
        public static SpaceStateManager BuildSpaceStateManager(ClientManager clientManager, INetworkingService _networkingService)
        {
            return DrawlessStateBuilder.BuildSpaceStateManager(null, new LidgrenMessenger(clientManager), 
                _networkingService);

        }

        public static PlanetStateManager BuildPlanetStateManager(ClientManager clientManager, INetworkingService _networkingService, PhysicsConfig _physicsConfig)
        {
            return DrawlessStateBuilder.BuildPlanetStateManager(null, new LidgrenMessenger(clientManager),
                _networkingService, _physicsConfig);

        }

    }

    class DrawlessStateBuilder
    {
        public static SpaceStateManager BuildSpaceStateManager(int? areaID, IMessenger messenger, INetworkingService _networkingService)
        {
            //Null references, unused by the Simulator
            TextureManager textureManager = null;
            SpriteBatch spriteBatch = null;
            ParticleManager particleManager = null;
            Texture2D warpholeTexture = null;
            Texture2D borderTexture = null;
            SelectionManager selectionManager = null;
            IGlobalGameUISingleton globalGameUISingleton = null;

            
            MessageService_ToServer messageService = new MessageService_ToServer(messenger);
            SimulationManager simulationManager = new SimulationManager();
            CollisionManager collisionManager = new CollisionManager(messageService);
            PlayerShipManager playerShipManager = new PlayerShipManager(messageService);
            IClientPlayerInfoManager clientPlayerInfoManager = new PlayablePlayerInfoManager(playerShipManager);
            TargetingService targetingService = new TargetingService();
            TeamManager teamManager = new TeamManager(targetingService);
            PhysicsManager physicsManager = new PhysicsManager();
            BorderManager borderManager = new BorderManager(borderTexture, spriteBatch, physicsManager);
            ProjectileManager projectileManager = new ProjectileManager(particleManager, physicsManager.World, spriteBatch, targetingService, simulationManager, messageService, collisionManager);
            ClientShipManager clientShipManager = new ClientShipManager(particleManager, playerShipManager, spriteBatch, textureManager, simulationManager, targetingService, teamManager, projectileManager, messageService, clientPlayerInfoManager, true);
            clientShipManager.SendPositionUpdates = true;
            StructureFactoryManager structureFactoryManager = new StructureFactoryManager(messageService, physicsManager.World, projectileManager, targetingService, teamManager, null, clientShipManager, null, true);
            WarpHoleManager warpholeManager = new WarpHoleManager(messageService, particleManager, physicsManager, clientShipManager, warpholeTexture);
            GravityManager gravityManager = new GravityManager(physicsManager);
            SpaceObjectManager spaceObjectManager = new SpaceObjectManager(textureManager, messageService, spriteBatch, particleManager, physicsManager);
            SpaceManager spaceManager = new SpaceManager(spriteBatch, borderManager, gravityManager, physicsManager, spaceObjectManager, warpholeManager);
            FloatyAreaObjectManager floatyAreaObjectManager = new FloatyAreaObjectManager(physicsManager.World, textureManager, messageService, spriteBatch, particleManager);

            var mhi = areaID == null ? new MessageHandlerID() : new MessageHandlerID((int)areaID);

           

            SpaceStateManager spaceStateManager = new SpaceStateManager(mhi,
            clientPlayerInfoManager,
            globalGameUISingleton,
            collisionManager,
            _networkingService,
            physicsManager,
            playerShipManager,
            projectileManager,
            selectionManager,
            simulationManager,
            clientShipManager,
            spaceManager,
            structureFactoryManager,
            targetingService,
            teamManager,
            warpholeManager,
            messageService,
            floatyAreaObjectManager);

            spaceStateManager.SetAreaId(areaID);

            return spaceStateManager;

        }

        public static PlanetStateManager BuildPlanetStateManager(int? areaID, IMessenger messenger, INetworkingService _networkingService, PhysicsConfig _physicsConfig)
        {
            //Null references, unused by the Simulator
            TextureManager textureManager = null;
            SpriteBatch spriteBatch = null;
            ParticleManager particleManager = null;
            Texture2D warpholeTexture = null;
            SelectionManager selectionManager = null;

            
            MessageService_ToServer messageService = new MessageService_ToServer(messenger);
            SimulationManager simulationManager = new SimulationManager();
            CollisionManager collisionManager = new CollisionManager(messageService);
            PlayerShipManager playerShipManager = new PlayerShipManager(messageService);
            IClientPlayerInfoManager clientPlayerInfoManager = new PlayablePlayerInfoManager(playerShipManager);
            TargetingService targetingService = new TargetingService();
            TeamManager teamManager = new TeamManager(targetingService);
            PhysicsManager physicsManager = new PhysicsManager();
            ProjectileManager projectileManager = new ProjectileManager(particleManager, physicsManager.World, spriteBatch, targetingService, simulationManager, messageService, collisionManager);
            ClientShipManager clientShipManager = new ClientShipManager(particleManager, playerShipManager, spriteBatch, textureManager, simulationManager, targetingService, teamManager, projectileManager, messageService, clientPlayerInfoManager, true);
            clientShipManager.SendPositionUpdates = true;
            StructureFactoryManager structureFactoryManager = new StructureFactoryManager(messageService, physicsManager.World, projectileManager, targetingService, teamManager, null, clientShipManager, null, true);
            WarpHoleManager warpholeManager = new WarpHoleManager(messageService, particleManager, physicsManager, clientShipManager, warpholeTexture);

            FloatyAreaObjectManager floatyAreaObjectManager = new FloatyAreaObjectManager(physicsManager.World, textureManager, messageService, spriteBatch, particleManager);

            var mhi = areaID == null ? new MessageHandlerID() : new MessageHandlerID((int)areaID);

            PlanetStateManager planetStateManager = new PlanetStateManager(
                mhi,
                clientPlayerInfoManager,
                null,
                collisionManager,
                physicsManager,
                playerShipManager,
                projectileManager,
                clientShipManager,
                structureFactoryManager,
                warpholeManager,
                _networkingService,
                selectionManager,
                simulationManager,
                targetingService,
                teamManager,
                GameStateType.Planet,
                floatyAreaObjectManager, messageService,
                _physicsConfig.PlanetTileWidth, _physicsConfig.PlanetTileHeight);
            planetStateManager.SetAreaId(areaID);

            return planetStateManager;

        }


    }
}
