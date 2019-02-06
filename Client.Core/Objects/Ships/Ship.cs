using System;
using System.Collections.Generic;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Core.Interfaces;
using Microsoft.Xna.Framework.Graphics;
using Freecon.Client.Managers;
using System.Linq;
using Freecon.Client.Core.Behaviors;
using Freecon.Client.Interfaces;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;
using Freecon.Client.Objects.Weapons;
using Freecon.Client.Objects.Pilots;
using Freecon.Models.TypeEnums;
using Core.Models;
using Core.Models.Enums;
using Core.Models.CargoHandlers;
using Freecon.Client.Mathematics;
using Freecon.Models;
using Freecon.Client.Core.Interfaces;
using Freecon.Core.Networking.Models.Objects;
using Freecon.Core.Utils;
using Server.Managers;

namespace Freecon.Client.Objects
{
	public class Ship : IShip, ITargetable, ITargeter, ISelectable, ICommandable, ICanFire, ICollidable, IDraw, ISynchronousUpdate
	{
		protected ParticleManager _particleManager;
		protected SpriteBatch _spriteBatch;

		protected Random _rand;

		public CollisionDataObject BodyData
		{
			get { return (ShipBodyDataObject)Body.UserData; }
			set { Body.UserData = value; }
		}

		public Color DrawColor { get; set; }

		public bool Enabled { get { return Body.Enabled; } }

		public event EventHandler<BodyPositionUpdatedEventArgs> BodyPositionUpdated;

		public List<IBodyBehavior> BodyBehaviors { get; set; } 
		
		#region State Variables (current health, current shields...

		public ShieldHandler Shields { get; protected set; }
		public bool IsSelected { get; set; }

		/// <summary>
		/// Required to reuse class for simulator, where any simulated ship can land/warp, while on the client only the client's ship can land/warp
		/// </summary>
		public bool CanLandWarp { get; set; }

		/// <summary>
		/// If true, ship will attempt an area change on certain collisions (planets, ports, etc)
		/// </summary>
		public bool EnterMode { get; set; }

		public bool IsLocalSim { get; set; }

		public int CurrentHealth;

		public bool IsTurningClockwise = false;
		public bool IsTurningCounterClockwise = false;

		public float oldRotation = 0; //Old rotation, in radians

		public bool Thrusting;
		public bool waitingForDockResponse = false;
		public bool WaitingForFireResponse = false; //Used when sending fire request to server to prevent request spam

		public bool SendPositionUpdates = true;
		public float PositionUpdateDisableTimestamp;
		public float PositionUpdateDisableDuration = 50;//TODO: Move to config file

		public bool IsAlive { get { return Pilot.IsAlive; } set { Pilot.IsAlive = value; } }

		public int Id { get; protected set; }

		public Dictionary<int, ITargetable> PotentialTargets { get; set; }

		public ITargetable CurrentTarget { get { return Pilot.CurrentTarget; } set { Pilot.CurrentTarget = value; } }

		public float AngularVelocity
		{
			get { return Body.AngularVelocity; }
			set { if (!Body.IsDisposed) Body.AngularVelocity = value; }
		}

		public Vector2 LinearVelocity
		{
			get { return Body.LinearVelocity; }
			set { if (!Body.IsDisposed) Body.LinearVelocity = value; }
		}

		public float Mass
		{
			get { return Body.Mass; }
			set { if (!Body.IsDisposed) Body.Mass = value; }
		}

		public Vector2 Position
		{
			get { return Body.Position; }
			set { if(!Body.IsDisposed) Body.Position = value; }
		}

		public float Rotation
		{
			get { return Body.Rotation; }
			set { if(!Body.IsDisposed) Body.Rotation = value; }
		}

		public float LinearDamping
		{
			get { return Body.LinearDamping; }
			set { if (!Body.IsDisposed) Body.LinearDamping = value; }
		}

		public int MaxHealth { get { return (int)(ShipStats.MaxHealth + StatBonuses[StatBonusTypes.MaxHealth]); } }
		public int MaxShields { get { return (int)(ShipStats.MaxShields + StatBonuses[StatBonusTypes.MaxShields]); } }
		public int MaxEnergy { get { return (int)(ShipStats.Energy + StatBonuses[StatBonusTypes.MaxEnergy]); } }

		#endregion
		
		#region Physical Ship Options (Shield type, weapons, modules, etc)

		public Dictionary<int, Module> Modules;

	    private List<Weapon> _weapons;

	    public MissileLauncher MissileLauncher{ get { return _weapons.Count > 0 ? (MissileLauncher) _weapons[0] : null; } }

	    public string shipName;
		public CargoHandler_ReadAddRemoveVM<CargoHandlerModel> Cargo { get; set; }

		#endregion

		#region Obfuscated Energy

		private int energyKey;

		private Dictionary<int, int> energyToKey;
									 //This dictionary is a reverse of the other. Need it because there is no support for bidirectional dictionaries in c#

		private Dictionary<int, int> keyToEnergy;

		private int maxEnergyKey;

		#endregion

		public ShipStats ShipStats { get { return _shipStats; } set { _shipStats = value; Shields.ShipStats = value; } }
		ShipStats _shipStats;

		public ShipBonusHandler StatBonuses { get; protected set; }
		
		public DebuffHandler Debuffs { get; protected set; }

		#region Timestamps
		public double lastTimeStamp; //In milliseconds
		public double LastLandRequestTime;
		public double LastWarpRequestTime;
		public double LastDockRequestTime;//TODO: Implement

		#endregion

		#region Damage Particle Effects

		//positions are offsets from body center in sim units

		//indices for positions and trigger times are correspondent

		private float lastFlameTriggerTime;
		private Vector2 plasmaEffectPosition;
		private float timeToNextPlasmaTrigger; //Used to randomize time between effects


		//private List<Vector2> smokeEffectPositions = new List<Vector2>(5);
		//private List<float> lastSmokeTriggerTimes = new List<float>(5);

		//private List<Vector2> plasmaEffectPositions = new List<Vector2>(5);
		//private List<float> lastPlasmaTriggerTimes = new List<float>(5);

		#endregion

		#region ICommandable Methods

		public virtual void HoldPosition()
		{
			Pilot.HoldPosition();
		}
		public virtual void GoToPosition(Vector2 destination)
		{
			Pilot.GoToPosition(destination);
		}
		public virtual void AttackToPosition(Vector2 destination)
		{
			Pilot.AttackToPosition(destination);
		}

		public virtual void Stop()
		{
			Pilot.Stop();
		}

		public virtual void AttackTarget(ITargetable target)
		{
			Pilot.AttackTarget(target);
		}

		#endregion

		#region Everything Else

		public Body Body { get; protected set; }

		public float BodyHeight { get; protected set; }
		public float BodyWidth { get; protected set; }

		public Texture2D currentDrawTex; //The currently drawn ship, depending on whether it is alive or dead

		public Texture2D deadTex; //The texture for the ship when it is dead

		public Color drawColor = Color.White;
		protected float engineOffset;
		protected Vector2 engineSpot;

		/// <summary>
		/// The texture for the ship when it is alive
		/// </summary>
		public Texture2D Texture { get; set; }

		public Pilot Pilot;
		public int playerID = int.MaxValue;

		public string playerName;
		public Color teamColor;
		public HashSet<int> Teams { get; set; } //List of teams to which this ship belongs

		public bool ThrustStatusForServer = false; // Use this to send thrusting to the server, since thrusting is set to false on every update. Value is set to false when ship update is sent to server.

		float _speedDampValue = 2;//value _body.LinearDamping set to when ship exceeds top speed.


		#endregion

		public TargetTypes TargetType
		{
			get { return TargetTypes.Moving; }
		}


		public bool IsBodyValid
		{
			get { return (Body != null && !Body.IsDisposed); }
		}

		/// <summary>
		/// Used only for tempShips
		/// </summary>
		//public Ship()
		//{
		//    ShipStats = new ShipStats();
		//    StatBonuses = new ShipBonusHandler();

		//    Debuffs = new DebuffHandler();
		//    Shields = new QuickRegenShieldHandler(ShipStats, StatBonuses, Debuffs);
		//}

		public Ship(int shipID,
					int playerID,
					string playerName,
					ShipStats stats,
					ParticleManager particleManager,
					SpriteBatch spriteBatch,
					HashSet<int> teams)
		{			
			_particleManager = particleManager;
			_spriteBatch = spriteBatch;
			_rand = new Random(7747);

            _weapons = new List<Weapon>();

			BodyBehaviors = new List<IBodyBehavior>();

			this.Id = shipID;
			this.playerID = playerID;
			this.playerName = playerName;
			Teams = teams;
			

			lastTimeStamp = 0;

			IsSelected = false;

			PotentialTargets = new Dictionary<int, ITargetable>();

			Cargo = new CargoHandler_ReadAddRemoveVM<CargoHandlerModel>();

			Modules = new Dictionary<int, Module>();

			StatBonuses = new ShipBonusHandler();

			Debuffs = new DebuffHandler();

			switch(stats.ShieldType)
			{
				case ShieldTypes.QuickRegen:
					Shields = new QuickRegenShieldHandler(stats, StatBonuses, Debuffs);
					break;

				case ShieldTypes.SlowRegen:
					Shields = new SlowRegenShieldHandler(stats, StatBonuses, Debuffs);
					break;

				case ShieldTypes.NoRegen:
					Shields = new NoRegenShieldHandler(stats, StatBonuses, Debuffs);
					break;
			}

			ShipStats = stats;

			RecalculateModuleBonuses();

			CreateDictionaries(ShipStats.Energy);

           

		}

		

		/// <summary>
		/// numEdges must be less than 8 or farseer throws up
		/// </summary>
		/// <param name="position"></param>
		/// <param name="velocity"></param>
		/// <param name="rotation"></param>
		/// <param name="w"></param>
		/// <param name="size"></param>
		/// <param name="shape"></param>
		/// <param name="size2"></param>
		/// <param name="numEdges"></param>
		protected virtual void AssignBody(Vector2 position, Vector2 velocity, float rotation, World w, float size, BodyShapes shape = BodyShapes.Circle, float size2 = 1, int numEdges = 8)
		{

			switch (shape)
			{
				case BodyShapes.Circle:
					{
						Debugging.AddStack.Push(this.ToString());
						Body = BodyFactory.CreateCircle(w, ConvertUnits.ToSimUnits(size), 1);
						break;
					}
			
				case BodyShapes.Oval:
					{
						Debugging.AddStack.Push(this.ToString());
						Body = BodyFactory.CreateEllipse(w, ConvertUnits.ToSimUnits(size), ConvertUnits.ToSimUnits(size2), numEdges, 1);
						break;
					}
			}

			Body.BodyType = BodyType.Dynamic;
			Body.Mass = _shipStats.BaseWeight;
			Body.IsBullet = true;
			Body.Friction = 0;
			Body.Restitution = 0.5f;
			//_body.CollisionCategories = Category.Cat10; // This is nice to have but oh man is it brittle
			Body.OnCollision += _body_OnCollision;
			Body.LinearDamping = 0.00001f;
			Body.Position = position;
			Body.LinearVelocity = velocity;
			Body.Rotation = rotation;
		}

		bool _body_OnCollision(Fixture fixtureA, Fixture fixtureB, Contact contact)
		{
			if (fixtureB.Body.UserData is ShipBodyDataObject)
				return false;

			return true;
		}
			 

		#region Energy Stuff

		public void CreateDictionaries(int maxEnergy)
		{
			//TODO: make generic, use for shields and health

			var intList = new List<int>();
			Random rand = _rand;
			int tempVal = 0;
			int index = 0;
			keyToEnergy = new Dictionary<int, int>();
			energyToKey = new Dictionary<int, int>();


			//Fill the list with ints from 0 to maxEnergy * 10
			for (int i = 0; i <= maxEnergy*10; i++)
				intList.Add(i);

			//Shuffle the list
			for (int i = maxEnergy*10; i >= 0; i--)
			{
				//"Pick" a random number in the array
				index = rand.Next(i);
				tempVal = intList[index];

				//Swap with last unswapped number
				intList[index] = intList[i];
				intList[i] = tempVal;
			}

			for (int i = 0; i <= maxEnergy*10; i++)
			{
				keyToEnergy.Add(i, intList[i]);
				energyToKey.Add(intList[i], i);
			}
			energyToKey.TryGetValue(maxEnergy*10, out maxEnergyKey); //Gets the key for the maximum energy value
		}


		/// <summary>
		/// Use this function to increase or decrease energy of the ship.
		/// If amount is negative, energy will be decreased. 
		/// This function automatically prevents energy from increasing past maxEnergy
		/// </summary>
		/// <param name="amount"></param>
		public void ChangeEnergy(int amount)
		{
			if (keyToEnergy == null)
				return;//WARNING: THIS IS A TEMPORARY HACK

			Random rand = _rand;
			int currentEnergy;
			int maxEnergy;
			//amount *= 10;//Because the energy is stored with an extra point of precision

			keyToEnergy.TryGetValue(maxEnergyKey, out maxEnergy);
				//Get the value for the maximum energy using the maxEnergyKey

			if (energyKey > keyToEnergy.Count)
				currentEnergy = maxEnergy;
					//Used with the random function below to keep energy from sitting at a set value
			else
				keyToEnergy.TryGetValue(energyKey, out currentEnergy);

			maxEnergy /= 10;
				//Because energy in the dictionary is stored with an extra point of precision (I.E. 1000 energy is represented as 10000)
			currentEnergy /= 10;
				//Because energy in the dictionary is stored with an extra point of precision (I.E. 1000 energy is represented as 10000)

			if (amount >= maxEnergy - currentEnergy)
				energyKey = rand.Next(energyToKey.Count + 1, energyToKey.Count + 20000);
					//this line ensures that the key does not stay the same when energy sits at 1000, which could make it easy to find with cheat engine
			else if (GetCurrentEnergy() + amount <= 0)
			{
				currentEnergy = 0;
				energyToKey.TryGetValue(currentEnergy*10, out energyKey);
			}
			else
			{
				currentEnergy += amount;
				energyToKey.TryGetValue(currentEnergy*10, out energyKey);
			}
		}


		public int GetCurrentEnergy()
		{
			if (energyKey > keyToEnergy.Count)//If true, ship is at max energy
			{
				int val;
				keyToEnergy.TryGetValue(maxEnergyKey, out val);
				return val / 10; //Because energy is stored with an extra point of precision
			}
			else
			{
				int val;
				keyToEnergy.TryGetValue(energyKey, out val);
				return val / 10;
			}
			
		}

		public void SetCurrentEnergy(int value)
		{
			if (value >= MaxEnergy)
				energyKey = maxEnergyKey;
			else
			{
				// The energy returned from get is based on the value of energyKey
				energyToKey.TryGetValue(value * 10, out energyKey);
			}
		}

		#endregion
		

		public virtual void Draw(Camera2D camera)
		{
			Color c = drawColor;
			if (IsSelected)
				c = Color.Green;

			_spriteBatch.Draw(currentDrawTex, ConvertUnits.ToDisplayUnits(Position),
							 new Rectangle(0, 0, currentDrawTex.Width, currentDrawTex.Height),
							 c, Rotation,
							 new Vector2(currentDrawTex.Width / 2, currentDrawTex.Height / 2),
							 1, SpriteEffects.None, 0.2f);

			BodyBehaviors.ForEach(b => b.Draw(camera));

			#region Damage Based Particle Effects

			if (CurrentHealth <= (.2) * (MaxHealth) && CurrentHealth > 0)
			{
				if (Thrusting) //Smoke trail
					_particleManager.TriggerEffect(engineSpot, ParticleEffectType.SmokeTrailEffect, 1);


				plasmaEffectPosition.X = _rand.Next(-3, 3) / 10f;
				plasmaEffectPosition.Y = _rand.Next(-3, 3) / 10f;

				if (TimeKeeper.MsSinceInitialization >= (lastFlameTriggerTime + timeToNextPlasmaTrigger))
				{
					_particleManager.TriggerEffect(ConvertUnits.ToDisplayUnits(Position + plasmaEffectPosition),
												  ParticleEffectType.SmallFlameExplosionEffect, .4f);
					lastFlameTriggerTime = (float)TimeKeeper.MsSinceInitialization;
					if (CurrentHealth > 200)
						timeToNextPlasmaTrigger = _rand.Next(200, CurrentHealth);
					else
						timeToNextPlasmaTrigger = 200;
				}
			}

			#endregion

			engineSpot.X = (float)Math.Cos(Rotation + MathHelper.PiOver2) * engineOffset +
						   ConvertUnits.ToDisplayUnits(Position.X);
			engineSpot.Y = (float)Math.Sin(Rotation + MathHelper.PiOver2) * engineOffset +
						   ConvertUnits.ToDisplayUnits(Position.Y);

			if (Thrusting)
			{
				_particleManager.TriggerEffect(engineSpot, ParticleEffectType.EngineEffect, 1);
				//if (pilot == _clientShipManager.PlayerPilot || localSim)
				//    thrusting = false;
			}
		}

		public virtual void UpdatePosition(PositionUpdateData data)
		{
			if (BodyPositionUpdated == null)
			{
				return;
			}

			BodyPositionUpdated(this, new BodyPositionUpdatedEventArgs(data, DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond));
		}

		public virtual void Update(IGameTimeService gameTime)
		{
			Pilot.Update(gameTime);

			BodyBehaviors.ForEach(b => b.Update(gameTime));

			// Time dependent updates (e.g. shields)

			var energyRegexRate = ShipStats.EnergyRegenRate * StatBonuses[StatBonusTypes.EnergyRegen] /
								  (1 + Debuffs[DebuffTypes.EnergyRegen]);

			var amount = (int)(gameTime.ElapsedMilliseconds * energyRegexRate);

			ChangeEnergy(amount);
			//WARNING: Might have to fix on fast computers, if update is too fast, int might be rounded down

			Shields.Update(gameTime.TotalMilliseconds);
			Debuffs.Update(gameTime.TotalMilliseconds);

			// Turning
			// Divided because units are radians/second
			var elapsedTime = ShipStats.TurnRate * StatBonuses[StatBonusTypes.TurnRate];

			if (IsTurningCounterClockwise)
			{
				AngularVelocity = -elapsedTime;
			}
			else if (IsTurningClockwise)
			{
				AngularVelocity = elapsedTime;
			}
			else
			{
				AngularVelocity = 0;
			}

			// Increase Drag if ship is going faster than top speed

			float limitVal = (ShipStats.TopSpeed + StatBonuses[StatBonusTypes.TopSpeed]) / (1 + Debuffs[DebuffTypes.TopSpeed]);

			if (Thrusting)
			{
				limitVal *= ShipStats.BoostBonus;
			}

			LinearDamping = LinearVelocity.Length() >= limitVal ? _speedDampValue : 0.0001f;

			// Check if Outside System

			//// Check if you're outside of the wall // Broken :(
			//if (Vector2.Distance(Vector2.Zero, ConvertUnits.ToDisplayUnits(Position)) > _borderManager.sizeOfSystem
			//    && GameStateManager.getState() == GameStates.space)
			//{
			//    Console.WriteLine("Outside of System");
			//    Vector2 pos;
			//    //pos.X = (float) (Math.Cos(MathHelper.ToRadians(Position.X))*(BorderManager.sizeOfSystem + 700));
			//    //pos.Y = (float) (Math.Sin(MathHelper.ToRadians(Position.Y))*(BorderManager.sizeOfSystem + 700));
			//    //Position = ConvertUnits.ToSimUnits(pos);
			//    Position = new Vector2(1,1);
			//    LinearVelocity *= -1;
			//}

			foreach (Weapon w in _weapons)
			{
				w.Update(gameTime);
			}

			if (!SendPositionUpdates &&
				gameTime.TotalMilliseconds - PositionUpdateDisableTimestamp > PositionUpdateDisableDuration)
			{
				SendPositionUpdates = true;
			}

			lastTimeStamp = gameTime.ElapsedMilliseconds;
		}

		/// <summary>
		/// Disables the ship, disables the body.
		/// </summary>
		public virtual void Kill()
		{
			if (_particleManager != null)
			{
				_particleManager.TriggerEffect(ConvertUnits.ToDisplayUnits(Position),
					ParticleEffectType.ExplosionEffect,
					320);
			}
		

			drawColor = Color.Transparent;
			IsAlive = false;
			//LinearVelocity = Vector2.Zero;
			//_body.BodyType = BodyType.Static;
			if (Pilot is PlayerPilot)
				((PlayerPilot)Pilot).IsAlive = false;


			Body.Enabled = false;
			CurrentHealth = 0;
		}

		/// <summary>
		/// Revives the ship, modifying body and pilot appropriately, restoring health to value according to percentHealth
		/// </summary>
		/// <param name="percentHealth"></param>
		public virtual void Revive(int currentHealth, int currentShields)
		{
			Body.Enabled = true;
			IsAlive = true;
			Pilot.IsAlive = true;

			currentDrawTex = Texture;
			drawColor = Color.White;
			CurrentHealth = currentHealth;
			Shields.CurrentShields = currentShields;
			
		}
				
		public void Thrust(ThrustTypes direction)
		{
			float forceX = 0;
			float forceY = 0;
			ThrustStatusForServer = true;

			//Toggle damping according to top speed
			if (Body.LinearVelocity.Length() >= (ShipStats.TopSpeed + StatBonuses[StatBonusTypes.TopSpeed]))
			{
				Body.LinearDamping = _speedDampValue;//Give this a more extreme value to limit top speed

			}
			else
			{
				Body.LinearDamping = 0;
			}

			switch (direction)
			{
				case (ThrustTypes.Forward):
					{
						forceX = (float)((ShipStats.BaseThrustForward) * Math.Sin(Rotation)) * StatBonuses[StatBonusTypes.Thrust];
						forceY = (float)((ShipStats.BaseThrustForward) * Math.Cos(Rotation)) * StatBonuses[StatBonusTypes.Thrust];
					}
					break;
				case (ThrustTypes.BoostForward):
					{
						forceX = (float)((ShipStats.BaseThrustForward) * Math.Sin(Rotation)) * ShipStats.BoostBonus * StatBonuses[StatBonusTypes.Thrust];
						forceY = (float)((ShipStats.BaseThrustForward) * Math.Cos(Rotation)) * ShipStats.BoostBonus * StatBonuses[StatBonusTypes.Thrust];
					}
					break;
				case (ThrustTypes.Backward):
					{
						forceX = -(float)((ShipStats.BaseThrustReverse) * Math.Sin(Rotation)) * StatBonuses[StatBonusTypes.Thrust];
						forceY = -(float)((ShipStats.BaseThrustReverse) * Math.Cos(Rotation)) * StatBonuses[StatBonusTypes.Thrust];
					}
					break;
				case (ThrustTypes.BoostBackward):
					{
						forceX = -(float)((ShipStats.BaseThrustReverse) * Math.Sin(Rotation)) * ShipStats.BoostBonus * StatBonuses[StatBonusTypes.Thrust];
						forceY = -(float)((ShipStats.BaseThrustReverse) * Math.Cos(Rotation)) * ShipStats.BoostBonus * StatBonuses[StatBonusTypes.Thrust];
					}
					break;
				case (ThrustTypes.RightLateral):
					{
						forceY = -(float)((ShipStats.BaseThrustLateral) * Math.Sin(Rotation)) * StatBonuses[StatBonusTypes.LateralThrust];
						forceX = (float)((ShipStats.BaseThrustLateral) * Math.Cos(Rotation)) * StatBonuses[StatBonusTypes.LateralThrust];
					}
					break;
				case (ThrustTypes.BoostRightLateral):
					{
						forceY = -(float)((ShipStats.BaseThrustLateral) * Math.Sin(Rotation)) * ShipStats.BoostBonus * StatBonuses[StatBonusTypes.LateralThrust];
						forceX = (float)((ShipStats.BaseThrustLateral) * Math.Cos(Rotation)) * ShipStats.BoostBonus * StatBonuses[StatBonusTypes.LateralThrust];
					}
					break;
				case (ThrustTypes.LeftLateral):
					{
						forceY = (float)((ShipStats.BaseThrustLateral) * Math.Sin(Rotation)) * StatBonuses[StatBonusTypes.LateralThrust];
						forceX = -(float)((ShipStats.BaseThrustLateral) * Math.Cos(Rotation)) * StatBonuses[StatBonusTypes.LateralThrust];
					}
					break;
				case (ThrustTypes.BoostLeftLateral):
					{
						forceY = (float)((ShipStats.BaseThrustLateral) * Math.Sin(Rotation)) * ShipStats.BoostBonus * StatBonuses[StatBonusTypes.LateralThrust];
						forceX = -(float)((ShipStats.BaseThrustLateral) * Math.Cos(Rotation)) * ShipStats.BoostBonus * StatBonuses[StatBonusTypes.LateralThrust];
					}
					break;


			}

			forceX /= (1 + Debuffs[DebuffTypes.Thrust]);
			forceY /= (1 + Debuffs[DebuffTypes.Thrust]);


#if ADMIN
			forceX *= 2;
			forceY *= 2;
#endif
			Body.ApplyForce(new Vector2((forceX), (-forceY)));

		}		
				
		#region _weapons and Damage

		public virtual void TakeDamage(int damageAmount)
		{

			if (Shields.CurrentShields - damageAmount < 0) //If there are not enough shields to take the damage
				Shields.CurrentShields = 0;
			else if (Shields.CurrentShields == 0) //If shields are already at 0, player takes damage
				CurrentHealth -= damageAmount;
			else
				Shields.CurrentShields -= damageAmount;
		}
		
		public bool TryFireWeapon(IGameTimeService gameTime, int slot)
		{
			if (_weapons.Count <= slot)
			{
				ConsoleManager.WriteLine("Error: invalid slot passed to TryFireWeapon.", ConsoleMessageType.Error);
				return false;
			}

			if (!_weapons[slot].CanFire())
			{
				return false;
			}

			byte charge = 0;
			if (_weapons[slot] is IChargable)
				charge = ((IChargable)_weapons[slot]).CurrentPctCharge;

			_weapons[slot].Fire_LocalOrigin(Rotation, charge, true);

			_weapons[slot].WaitingForFireResponse = true;


#if ADMIN
			if (Debugging.DisableNetworking)
			{
				_weapons[slot].WaitingForFireResponse = false;
			}
#endif

			_weapons[slot].TimeOfWaitStart = gameTime.TotalMilliseconds;

			return true;
		}
				

		#endregion

		#region Modules

		public void AddModule(Module m)
		{
			if (Modules.ContainsKey(m.Id))
			{
				ConsoleManager.WriteLine("Error: Module with id " + m.Id + " has already been added.", ConsoleMessageType.Error);
				return;
			}

			Modules.Add(m.Id, m);
			StatBonuses.AddBonus(m);

			if (m.ModuleType == ModuleTypes.MaxEnergyModule)
			{
				CreateDictionaries(ShipStats.Energy + (int)StatBonuses[StatBonusTypes.MaxEnergy]);
			}
		}

		public void RecalculateModuleBonuses()
		{
			StatBonuses.Reset();

			foreach (var m in Modules)
			{
				StatBonuses.AddBonus(m.Value);
			}
		}

		public void RemoveModule(int moduleID)
		{
			if (!Modules.ContainsKey(moduleID))
				return;

			Module m = Modules[moduleID];
			Modules.Remove(moduleID);
			StatBonuses.RemoveBonus(m);
			if (m.ModuleType == ModuleTypes.MaxEnergyModule)
			{
				CreateDictionaries((int)ShipStats.Energy + (int)StatBonuses[StatBonusTypes.MaxEnergy]);
			}
		}



		#endregion

		#region Helper Functions

		/// <summary>
		/// Converts an angle from normal cartesian coordinates to farseer coordinates
		/// </summary>
		/// <param name="angle">angle in radians</param>
		/// <returns>angle in radans</returns>
		public float convertToFarseerAngle(float angle)
		{
			return (float)(-angle + Math.PI / 2);
		}

		/// <summary>
		/// Calculates the angle of separation between the two passed vectors
		/// Returns angle in radians
		/// </summary>
		/// <returns></returns>
		private double calcAngSep(Vector2 vec1, Vector2 vec2)
		{
			//Calculate angle

			return Math.Acos(vec1.X * vec2.X + vec1.Y * vec2.Y);
		}

		/// <summary>
		/// Converts an angle from farseer coordinates to Math library friendly coordinates
		/// </summary>
		/// <param name="angle">angle in radians</param>
		/// <returns>angle in radians, bounded between -PI and PI</returns>
		public float ConvertToCartesianAngle(float angle)
		{
			angle = -angle + (float)Math.PI / 2;
			angle = angle % (2 * (float)Math.PI);

			if (angle > Math.PI)
			{
				return angle - (float)(Math.PI * 2);
			}

			if (angle < -Math.PI)
			{
				return angle + (float)(Math.PI * 2);
			}

			return angle;
		}

		#endregion

		public void SetWeapon(Weapon w, int slot)
		{
			if (slot == 0 && w.Stats.WeaponType != WeaponTypes.MissileLauncher)
			{
				throw new OhShitWhatTheFuckAreYouDoingYouAreNotGoodWithComputers("Error: Ship._weapons slot 0 is currently reserved for MissileLauncher.");//This is likely to change.
			}

            while (_weapons.Count <= slot)
			{
				_weapons.Add(new NullWeapon(this));
			}

			_weapons[slot] = w;

		}

		public Weapon GetWeapon(int slot)
		{
			return _weapons.Count > slot ? _weapons[slot] : null;
		}

		public List<WeaponTypes> GetWeaponTypes()
		{
			return _weapons.Select(w => w.Stats.WeaponType).ToList();
		}

		public void SetPilotData(CollisionDataObject userdata, bool ignoreGravity)
		{
			SetUserData(userdata);
			Body.OnCollision += body_OnCollision;
			Body.IgnoreGravity = ignoreGravity;
		}

		private bool body_OnCollision(Fixture fixtureA, Fixture fixtureB, Contact contact)
		{
			//Don't collide with other network/player ships
			//if (((BodyDataObject)fixtureB._body.UserData).type == BodyTypes.NetworkShip || ((BodyDataObject)fixtureB._body.UserData).type == BodyTypes.PlayerShip)
			//return false;

			if (fixtureB.Body.UserData is ProjectileBodyDataObject) //For now, make sure there is no momentum transfer
			{
				return false;
			}

			return true;
		}

		public void SetUserData(object userdata)
		{
			Body.UserData = userdata;

			foreach (Fixture f in Body.FixtureList)
			{
				f.UserData = Body.UserData; //Needed to set fixture userdata for collisions
			}
		}

		public virtual void Simulate(IGameTimeService gameTime)
		{
			Pilot.Simulate(gameTime);

		}



	}

	public enum BodyShapes
	{
		Circle,
		Oval
	}

	
}
