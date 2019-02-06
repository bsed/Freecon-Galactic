using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Core.Interfaces;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
//using ProjectMercury;
//using ProjectMercury.Emitters;
//using ProjectMercury.Renderers;

using MonoGame.Extended.Particles;
using MonoGame.Extended.Sprites;
using MonoGame.Framework;
using Freecon.Client.Objects;
using Freecon.Client.Core.Interfaces;
using Freecon.Client.Mathematics;
using Core.Models.Enums;
using Server.Managers;
using ProjectMercury.Renderers;

namespace Freecon.Client.Managers
{

    public class ParticleManager : ISynchronousUpdate, IDraw
    {
        //particle effects
        Dictionary<ParticleEffectType, ParticleEffect> _particleEffects;
        private SpriteBatchRenderer renderer; //used to render all of the particles
        //decals

        //base decal elements
        //static public Stack<int> decalStack = new Stack<int>();
        public List<Decal> decalList = new List<Decal>(); // List holding Decal data
        public Decal deadDecal = new Decal(); //Decal used for reseting decals to default values

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private ContentManager _content;
        private TextureManager _textureManager;

        public ParticleManager(GraphicsDeviceManager graphics, 
                               SpriteBatch spriteBatch,
                               ContentManager Content, 
                               TextureManager textureManager)
        {
            _graphics = graphics;
            _spriteBatch = spriteBatch;
            _content = Content;
            _textureManager = textureManager;

            //renderer = new SpriteBatchRenderer
            //{
            //    GraphicsDeviceService = graphics
            //};
            //_particleEffects = new Dictionary<ParticleEffectType, ParticleEffect>();
            //_particleEffects.Add(ParticleEffectType.MissileEffect, Content.Load<ParticleEffect>(@"EffectLibrary\MissileEngines"));
            //_particleEffects.Add(ParticleEffectType.ExplosionEffect, Content.Load<ParticleEffect>(@"EffectLibrary\Explosion"));
            //_particleEffects.Add(ParticleEffectType.ProjectileExplosionEffect, Content.Load<ParticleEffect>(@"EffectLibrary\ProjectileExplosion"));
            //_particleEffects.Add(ParticleEffectType.DustEffect, Content.Load<ParticleEffect>(@"EffectLibrary\SpaceDust"));
            //_particleEffects.Add(ParticleEffectType.EngineEffect, Content.Load<ParticleEffect>(@"EffectLibrary\EngineParticles"));
            //_particleEffects.Add(ParticleEffectType.WarpHoleEffect, Content.Load<ParticleEffect>(@"EffectLibrary\WarpTest2"));
            //_particleEffects.Add(ParticleEffectType.LaserWaveEffect, Content.Load<ParticleEffect>(@"EffectLibrary\laserWaveTest"));
            //_particleEffects.Add(ParticleEffectType.ChargeWeaponEffect, Content.Load<ParticleEffect>(@"EffectLibrary\WeaponCharge"));
            //_particleEffects.Add(ParticleEffectType.GlowEffect, Content.Load<ParticleEffect>(@"EffectLibrary\Glow"));
            //_particleEffects.Add(ParticleEffectType.SmokeTrailEffect, Content.Load<ParticleEffect>(@"EffectLibrary\small smoke"));
            //_particleEffects.Add(ParticleEffectType.SmallFlameExplosionEffect, Content.Load<ParticleEffect>(@"EffectLibrary\flame explosion small"));


            //foreach (var kvp in _particleEffects)
            //{
            //    kvp.Value.Initialise();
            //    kvp.Value.LoadContent(Content);
            //}

            //renderer.LoadContent(Content);
        }


        public void Update(IGameTimeService gameTime)
        {
            var SecondsPassed = (float)gameTime.ElapsedMilliseconds / 1000f;

            //foreach (var pe in _particleEffects)
            //{
            //    pe.Value.Update(SecondsPassed);
            //}


            //Decal decal;
            //for (int i = 0; i < decalList.Count; i++)
            //{
            //    decal = decalList[i];
            //    switch (decal.type)
            //    {
            //        case (byte) DecalType.shields:

            //            if (_clientShipManager._shipList[decal.ID] != null)
            //            {
            //                decal.location = ConvertUnits.ToDisplayUnits(_clientShipManager._shipList[decal.ID].body.Position);
            //            }
            //            decal.transparency += 1;

            //            decalList[i] = decal;
            //            if (decal.transparency > 254)
            //            {
            //                decalList.RemoveAt(i);
            //            }
            //            break;
            //    }
            //}
        }


        public void TriggerEffect(Vector2 positionOfTrigger, ParticleEffectType effectType, float scale)
        {
        //    if (!_particleEffects.ContainsKey(effectType))
        //    {
        //        ConsoleManager.WriteLine("Error: effect type " + effectType.ToString() + " not found in particle effects container. Be sure to add it in the ParticleEffectManager constructior.", ConsoleMessageType.Error);
        //    }

        //    switch (effectType)
        //    {
        //        case ParticleEffectType.GlowEffect:
        //            for (int i = 0; i < _particleEffects[ParticleEffectType.GlowEffect].Count; i++)
        //            {
        //                ParticleEmitter glowEmitter = _particleEffects[ParticleEffectType.GlowEffect][i];
        //                glowEmitter.ReleaseScale = scale;
        //                _particleEffects[ParticleEffectType.GlowEffect][i] = glowEmitter;
        //            }
        //            _particleEffects[ParticleEffectType.GlowEffect].Trigger(positionOfTrigger);
        //            break;
        //        default:
        //            {
        //                _particleEffects[effectType].Trigger(positionOfTrigger);
        //                break;
        //            }
        //    }
        }

        public void Reset()
        {
            //foreach (var kvp in _particleEffects)
            //    kvp.Value.Terminate();
        }

        public void TriggerDecal(byte typeOfDecal, UInt32 ID, float angle, float scale)
        {
            //if (typeOfDecal == 1)
            //{
            //    var decal = new Decal();
            //    decal.ID = ID;
            //    decal.location = ConvertUnits.ToDisplayUnits(_clientShipManager._shipList[decal.ID].body.Position);
            //    //Console.WriteLine("Angle: " + angle);
            //    decal.angle = angle;
            //    decal.scale = 1;
            //    decal.transparency = 0;
            //    decal.color = Color.DeepSkyBlue;
            //    decal.type = typeOfDecal;
            //    decal.tex = _textureManager.shield_sfx;
            //    decalList.Add(decal);
            //}
        }


        public void Draw(Camera2D camera)
        {
            //var ge = _particleEffects[ParticleEffectType.GlowEffect];
            //for (int i = 0; i < ge.Count; i++)
            //{
            //    Emitter glowEmitter = ge[i];
            //    glowEmitter.ReleaseQuantity = 4;
            //    ge[i] = glowEmitter;
            //}

            //foreach(var pe in _particleEffects)
            //{
            //    renderer.RenderEffect(pe.Value, _spriteBatch);

            //}



            //for (int i = 0; i < decalList.Count; i++)
            //{
            //    DrawDecal(decalList[i]);
            //}
        }

        private void DrawDecal(Decal decal)
        {
            _spriteBatch.Draw(decal.tex, decal.location, null, decal.color * (Math.Abs(decal.transparency - 255) / 255f),
                             decal.angle,
                             new Vector2(decal.tex.Width / 2, decal.tex.Height / 2), decal.scale, SpriteEffects.None, 0f);
        }
        
    }


}