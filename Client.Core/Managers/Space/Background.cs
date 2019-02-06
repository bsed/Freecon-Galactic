using System;
using System.Collections.Generic;
using FarseerPhysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Freecon.Client.Managers;
using Core.Models.Enums;
using Freecon.Client.Core.Services;

namespace Freecon.Client.Mathematics
{
    public class BackgroundManager
    {
        public static int numberOfStars = 200;
        public static int numberOfDust = 600;
        public static int buffer = 30;

        public static bool disableBackground = true, disableStars = true;
        private readonly Random r;

        private readonly List<star> starList = new List<star>();
        private readonly Texture2D tex_Background;
        private readonly Texture2D tex_Dust1;
        private readonly Texture2D tex_Star1;
        private readonly Texture2D tex_Star2;
        private readonly Texture2D tex_Star3;
        private readonly Texture2D tex_Star4;
        public Vector2 BackgroundSize;
        private bool createdStars;
        private Vector2 shipDifference;//Difference in ship position between updates.
        private Vector2 shipPos;

        private ParticleManager _particleManager;
        private SpriteBatch _spriteBatch;

        CameraService _cameraService;

        public BackgroundManager(ContentManager Content, ParticleManager particleManager, SpriteBatch spriteBatch, CameraService cameraService, Random r)
        {
            _particleManager = particleManager;
            _spriteBatch = spriteBatch;

            _cameraService = cameraService;

            this.r = r;
            tex_Background = Content.Load<Texture2D>(@"Client.Monogame.Content/Space/Starfield");
            tex_Star1 = Content.Load<Texture2D>(@"Client.Monogame.Content/Space/stara");
            tex_Star2 = Content.Load<Texture2D>(@"Client.Monogame.Content/Space/star2a");
            tex_Star3 = Content.Load<Texture2D>(@"Client.Monogame.Content/Space/star3a");
            tex_Star4 = Content.Load<Texture2D>(@"Client.Monogame.Content/Space/star4a");
            tex_Dust1 = Content.Load<Texture2D>(@"Client.Monogame.Content/Space/Dust1");

            
        }

        public virtual void Update(Vector2 shipPos, Vector2 shipDifference)
        {
            this.shipPos = ConvertUnits.ToDisplayUnits(shipPos);
            this.shipDifference = ConvertUnits.ToSimUnits(shipDifference);
        }

        //I swear, I've come to this function 5+ times intending to refactor it and noped the fuck out every time.
        //The problem right now is that the background stars move more quickly than foreground planets
        //We can't use layers either. All stars need to be assigned a random depth which will determine their lateral motion with respect to ship movement
        public virtual void Draw(bool Rotation, Vector2 camSpot)
        {
            // This makes it so that stars respawn when viewport is changed
            if (BackgroundSize.X != _spriteBatch.GraphicsDevice.Viewport.Width ||
                BackgroundSize.Y != _spriteBatch.GraphicsDevice.Viewport.Height)
            {
                var newSize = new Vector2(_spriteBatch.GraphicsDevice.Viewport.Width,
                                          _spriteBatch.GraphicsDevice.Viewport.Height);

                // Number of stars is changed based on area change, or atleast it's increased
                if (BackgroundSize != Vector2.Zero)
                {
                    numberOfStars = (int)((newSize.Length() / BackgroundSize.Length()) * numberOfStars);
                    numberOfDust = (int)((newSize.Length() / BackgroundSize.Length()) * numberOfDust);
                }

                BackgroundSize = newSize;
                starList.Clear();
                createdStars = false;
            }

            // Spritebatches layered for maximum batch efficiency
            if (disableStars)
            {
                _spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.LinearWrap, null, null);

                //spriteBatch.Draw(tex_Background, Vector2.Zero, new Rectangle((int)(shipPos.X / 100f), (int)(shipPos.Y / 100f), spriteBatch.GraphicsDevice.Viewport.Width, spriteBatch.GraphicsDevice.Viewport.Height), Color.White);


                //if ((shipDifference.Length() * .01f) > 0.00005d) // Dont' scale if you're not moving
                    _spriteBatch.Draw(tex_Background, Vector2.Zero,
                                     new Rectangle((int)(shipPos.X / 14f), (int)(shipPos.Y / 14f),
                                                   _spriteBatch.GraphicsDevice.Viewport.Width,
                                                   _spriteBatch.GraphicsDevice.Viewport.Height),
                                     Color.White, 0, Vector2.Zero, 1f + (shipDifference.Length() * .1f),
                                     SpriteEffects.None, 1);
                //else
                //    _spriteBatch.Draw(tex_Background, Vector2.Zero,
                //                     new Rectangle((int)(shipPos.X / 14f), (int)(shipPos.Y / 14f),
                //                                   _spriteBatch.GraphicsDevice.Viewport.Width,
                //                                   _spriteBatch.GraphicsDevice.Viewport.Height),
                //                     Color.White, 0, Vector2.Zero, 1f, SpriteEffects.None, 1);
                _spriteBatch.End();

                //Draws high speed twinkle effect. Should be using ship velocity
                if (shipDifference.Length() > .05)
                    _particleManager.TriggerEffect(shipPos, ParticleEffectType.DustEffect, 1);


                _spriteBatch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend);
                if (!createdStars)
                {
                    for (int n = 0; n < numberOfStars; n++)
                    {
                        var s = new star();
                        s.pos = new Vector2(r.Next(-(_spriteBatch.GraphicsDevice.Viewport.Width / 2) - buffer,
                                                   (_spriteBatch.GraphicsDevice.Viewport.Width / 2) + buffer),
                                            r.Next(-(_spriteBatch.GraphicsDevice.Viewport.Height / 2) - buffer,
                                                   (_spriteBatch.GraphicsDevice.Viewport.Height / 2) + buffer));
                        float temp = r.Next(1, 3);
                        if (temp == 1)
                        {
                            s.scale = (r.Next(2, 10)*.1f);
                        }
                        if (temp >= 2)
                        {
                            s.scale = (r.Next(2, 6)*.1f);
                        }
                        s.color = r.Next(1, 5);
                        s.random = r.Next(90, 101)*0.01f;
                        starList.Add(s);
                    }
                    for (int n = 0; n < numberOfDust; n++)
                    {
                        var s = new star();
                        s.pos = new Vector2(r.Next(-(_spriteBatch.GraphicsDevice.Viewport.Width / 2) - buffer,
                                                   (_spriteBatch.GraphicsDevice.Viewport.Width / 2) + buffer),
                                            r.Next(-(_spriteBatch.GraphicsDevice.Viewport.Height / 2) - buffer,
                                                   (_spriteBatch.GraphicsDevice.Viewport.Height / 2) + buffer));
                        float temp = r.Next(1, 3);
                        s.scale = 1f;
                        s.color = 5;
                        s.random = r.Next(80, 101)*0.01f;
                        starList.Add(s);
                    }
                    createdStars = true;
                }
                //We are apparently shifting each star in each background layer by an amount according to shipDifference (should be using ship velocity, probably)
                if (createdStars)
                {
                    if (starList.Count == 0)
                    {
                        _spriteBatch.End();
                        return;
                    }
                    star s;

                    float scaleFactor = .5f * _cameraService.CurrentZoom;

                    for (int i = 0; i < starList.Count; i++)
                    {
                        s = starList[i];
                        if (s.color == 5)
                        {
                            s.pos.X += shipDifference.X*(60f*s.random) * scaleFactor;
                            s.pos.Y += shipDifference.Y*(60f*s.random) * scaleFactor;
                        }
                        if (s.scale >= .9)
                        {
                            s.pos.X += shipDifference.X*35f*s.random * scaleFactor;
                            s.pos.Y += shipDifference.Y*35f*s.random * scaleFactor;
                        }
                        else if (s.scale < .9 && s.scale >= .7)
                        {
                            s.pos.X += shipDifference.X*30f*s.random * scaleFactor;
                            s.pos.Y += shipDifference.Y*30f*s.random * scaleFactor;
                        }
                        else if (s.scale < .7 && s.scale >= .5)
                        {
                            s.pos.X += shipDifference.X*25f*s.random * scaleFactor;
                            s.pos.Y += shipDifference.Y*25f*s.random * scaleFactor;
                        }
                        else if (s.scale < .5 && s.scale >= .3)
                        {
                            s.pos.X += shipDifference.X*22f*s.random * scaleFactor;
                            s.pos.Y += shipDifference.Y*22f*s.random * scaleFactor;
                        }
                        else if (s.scale < .3)
                        {
                            s.pos.X += shipDifference.X*10f*s.random * scaleFactor;
                            s.pos.Y += shipDifference.Y*10f*s.random * scaleFactor;
                        }

                        //Out of bounds handling???
                        if (s.pos.X < 0 - buffer)
                            s.pos.X = s.pos.X + _spriteBatch.GraphicsDevice.Viewport.Width
                                      + buffer + r.Next(0, buffer + 1);

                        if (s.pos.X > _spriteBatch.GraphicsDevice.Viewport.Width + buffer)
                            s.pos.X = s.pos.X - _spriteBatch.GraphicsDevice.Viewport.Width
                                      - buffer - r.Next(0, buffer + 1);

                        if (s.pos.Y < 0 - buffer)
                            s.pos.Y = s.pos.Y + _spriteBatch.GraphicsDevice.Viewport.Height
                                      + buffer + r.Next(0, buffer + 1);

                        if (s.pos.Y > _spriteBatch.GraphicsDevice.Viewport.Height + buffer)
                            s.pos.Y = s.pos.Y - _spriteBatch.GraphicsDevice.Viewport.Height
                                      - buffer - r.Next(0, buffer + 1);
                        

                        switch (s.color)
                        {
                            case 1:
                                _spriteBatch.Draw(tex_Star1, s.pos, null,
                                                 Color.White, 0, Vector2.Zero, s.scale, SpriteEffects.None, .99f);
                                break;

                            case 2:
                                _spriteBatch.Draw(tex_Star2, s.pos, null,
                                                 Color.White, 0, Vector2.Zero, s.scale, SpriteEffects.None, .99f);
                                break;

                            case 3:
                                _spriteBatch.Draw(tex_Star3, s.pos, null,
                                                 Color.White, 0, Vector2.Zero, s.scale, SpriteEffects.None, .99f);
                                break;

                            case 4:
                                _spriteBatch.Draw(tex_Star4, s.pos, null,
                                                 Color.White, 0, Vector2.Zero, s.scale, SpriteEffects.None, .99f);
                                break;
                            case 5:
                                _spriteBatch.Draw(tex_Dust1, s.pos, null,
                                                 Color.White, 0, Vector2.Zero, s.scale, SpriteEffects.None, .99f);
                                break;
                            default:
                                Console.WriteLine("Drawing default case Star");
                                break;
                        }


                        starList[i] = s;
                    }
                }
                _spriteBatch.End();
            }
            else
            {
                if (!disableBackground)
                {
                    _spriteBatch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend);
                    _spriteBatch.Draw(tex_Background, Vector2.Zero, null,
                                     Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0f);
                    _spriteBatch.End();
                }
            }
        }

        #region Nested type: star

        private struct star
        {
            public int color;
            public Vector2 pos;
            public float random;
            public float scale;
        }

        #endregion
    }
}