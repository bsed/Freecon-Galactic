using Core.Models.Enums;
using Freecon.Models.TypeEnums;
using Microsoft.Xna.Framework.Graphics;
using Freecon.Client.Managers;

namespace Freecon.Client.Objects.Projectiles
{
    //Keep these readonly.
    //All objects should reference the same Stats object, I.E. don't call New


    public abstract class ProjectileStats//Holds stats which are constant for all projectiles of a certain type
    {
        public float Lifetime;//ms, time that a projectile exists before automatically terminating
        public float BaseSpeed;//Speed of a projectile, before adding ship speed upon firing
        public Texture2D Texture;//Drawing texture
        public ProjectileTypes ProjectileType;
    }


    public class LaserProjectileStats : ProjectileStats
    {
        public LaserProjectileStats()
        {
            Lifetime = 2000;
            BaseSpeed = 10;
            Texture = TextureManager.Laser;
            ProjectileType = ProjectileTypes.Laser;
        }

    }

    public class BC_LaserProjectileStats : ProjectileStats
    {
        public BC_LaserProjectileStats()
        {
            Lifetime = 2000;
            BaseSpeed = 10;
            Texture = TextureManager.Laser;
            ProjectileType = ProjectileTypes.BC_Laser;
        }

    }
    public class HullPlagueProjectileStats : LaserProjectileStats
    {
        public HullPlagueProjectileStats()
        {
            Lifetime = 2000;
            BaseSpeed = 10;
            Texture = TextureManager.Laser;
            ProjectileType = ProjectileTypes.HullPlague;
        }

    }

    public class LaserWaveProjectileStats : LaserProjectileStats
    {
        public ParticleEffectType DrawEffectType;
        public float DrawPeriod;

        /// <summary>
        /// Time spent decelerating to 0 relative to lifetime
        /// </summary>
        public float ZeroFraction;

        public LaserWaveProjectileStats()
        {
            Lifetime = 4000;
            BaseSpeed = 5;
            Texture = TextureManager.Laser;
            DrawEffectType = ParticleEffectType.LaserWaveEffect;
            DrawPeriod = 40;
            ProjectileType = ProjectileTypes.LaserWave;
            ZeroFraction = .25f;
        }

    }

    public class OrbProjectileStats : ProjectileStats
    {
        public OrbProjectileStats()
        {
            Lifetime = 5000;
            BaseSpeed = 3;
            Texture = TextureManager.Orb;
            ProjectileType = ProjectileTypes.Orb;

        }

    }

    public class PlasmaCannonProjectileStats : ProjectileStats
    {
        public PlasmaCannonProjectileStats()
        {
            Lifetime = 2000;
            BaseSpeed = 12;
            Texture = TextureManager.PlasmaCannon;
            ProjectileType = ProjectileTypes.PlasmaCannon;
        }

    }

    public class NaniteLauncherProjectileStats : ProjectileStats
    {
        public NaniteLauncherProjectileStats()
        {
            Lifetime = 2000;
            BaseSpeed = 12;
            Texture = TextureManager.PlasmaCannon;
            ProjectileType = ProjectileTypes.NaniteLauncher;
        }

    }

    public class GravityBombProjectileStats : ProjectileStats
    {
        public float NonGravityFraction;//Amount of time before gravity activates 
        public float InitialGravityVal;
        public float FinalGravityVal;
        public int NumFlips;


        public GravityBombProjectileStats()
        {
            Lifetime = 3000;
            BaseSpeed = 10;
            NonGravityFraction = .25f;
            InitialGravityVal = 30;
            FinalGravityVal = 60;
            Texture = TextureManager.Laser;
            ProjectileType = ProjectileTypes.GravityBomb;
            NumFlips = 2;
        }

    }

    public abstract class MissileProjectileStats : ProjectileStats
    {     
        public float BaseThrust;
        public float SpeedDampValue;//Damp value when missile exceeds max speed
        public float BaseTurnRate;
        public float SplashRadius;//We can add fancy radius dropoff tweaks later
        public float KnockForce;//Impulse applied on explosion to knock away other objects
    }

    public class AmbassadorProjectileStats : MissileProjectileStats
    {

        public AmbassadorProjectileStats()
        {
            Lifetime = 3000;
            BaseSpeed = 20;
            Texture = TextureManager.Ambassador;
            BaseThrust = 2f;
            SpeedDampValue = 3;
            BaseTurnRate = 2;
            ProjectileType = ProjectileTypes.AmbassadorMissile;
            SplashRadius = 1f;
            KnockForce = 2f;
        }
    }

    public class HellhoundProjectileStats : MissileProjectileStats
    {
        public HellhoundProjectileStats()
        {
            Lifetime = 4000;
            BaseSpeed = 15;
            Texture = TextureManager.Hellhound;
            BaseThrust = 2f;
            SpeedDampValue = 3;
            BaseTurnRate = 3;
            ProjectileType = ProjectileTypes.HellHoundMissile;
            SplashRadius = 2f;
            KnockForce = 5f;
        }
    }

    public class MissileType1ProjectileStats : MissileProjectileStats
    {
        public MissileType1ProjectileStats()
        {
            Lifetime = 4000;
            BaseSpeed = 15;
            Texture = TextureManager.MissileType1;
            BaseThrust = 2f;
            SpeedDampValue = 3;
            BaseTurnRate = 3;
            ProjectileType = ProjectileTypes.MissileType1;
            SplashRadius = 2f;
            KnockForce = 5f;
        }
    }

    public class MissileType2ProjectileStats : MissileProjectileStats
    {
        public MissileType2ProjectileStats()
        {
            Lifetime = 4000;
            BaseSpeed = 15;
            Texture = TextureManager.MissileType2;
            BaseThrust = 2f;
            SpeedDampValue = 3;
            BaseTurnRate = 3;
            ProjectileType = ProjectileTypes.MissileType2;
            SplashRadius = 2f;
            KnockForce = 5f;
        }
    }

    public class MissileType3ProjectileStats : MissileProjectileStats
    {
        public MissileType3ProjectileStats()
        {
            Lifetime = 4000;
            BaseSpeed = 15;
            Texture = TextureManager.MissileType3;
            BaseThrust = 2f;
            SpeedDampValue = 3;
            BaseTurnRate = 3;
            ProjectileType = ProjectileTypes.MissileType3;
            SplashRadius = 2f;
            KnockForce = 5f;
        }
    }

    public class MissileType4ProjectileStats : MissileProjectileStats
    {
        public MissileType4ProjectileStats()
        {
            Lifetime = 4000;
            BaseSpeed = 15;
            Texture = TextureManager.MissileType4;
            BaseThrust = 2f;
            SpeedDampValue = 3;
            BaseTurnRate = 3;
            ProjectileType = ProjectileTypes.MissileType4;
            SplashRadius = 2f;
            KnockForce = 5f;
        }
    }

    public class MineSplashProjectileStats : ProjectileStats
    {
        public float SplashRadius;
        public MineSplashProjectileStats()
        {
            SplashRadius = 3;
            Lifetime = 0;
            BaseSpeed = 0;
            Texture = TextureManager.greenPoint;//TODO: Add a "null" clear texture
            ProjectileType = ProjectileTypes.MineSplash;
        }

    }
}
