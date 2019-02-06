using System;
using System.Collections.Generic;
using System.Linq;
using FarseerPhysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Freecon.Client.Managers.Space;
using Freecon.Client.Mathematics;
using Freecon.Client.Objects;
using Freecon.Models.TypeEnums;


namespace Freecon.Client.Managers.GUI
{
    public class RadarSpaceManager
    {
        public Texture2D tex_DotW;
        public Texture2D tex_Radar;
        private readonly List<Texture2D> moonRings;
        private readonly List<Texture2D> planetRings;
        private readonly Camera2D _radarCamera;
        private readonly int radarHeight;
        private readonly RenderTarget2D radarTarget;
        private readonly int radarWidth;
        private readonly Texture2D tex_EdgeOfRadar;
        private readonly Texture2D tex_NetworkShip;
        private readonly Texture2D tex_OrbitalRing;
        private readonly Texture2D tex_Ship;
        private readonly Texture2D tex_Sun;
        private readonly Texture2D tex_Warp;
        private Vector2 pos_Radar;

        private SpriteFont textFont;

        private ContentManager _content;
        private SpriteBatch _spriteBatch;
        private GraphicsDevice _graphics;
        private BorderManager _borderManager;
        private PlayerShipManager _playerShipManager;
        private ProjectileManager _projectileManager;
        private ClientShipManager _clientShipManager;
        private SpaceObjectManager _spaceObjectManager;
        private WarpHoleManager _warpholeManager;

        public RadarSpaceManager(ContentManager Content,
                                 SpriteBatch spriteBatch,
                                 GraphicsDevice graphics,
                                 BorderManager borderManager,
                                 PlayerShipManager playerShipManager,
                                 ProjectileManager projectileManager,
                                 SpaceObjectManager spaceObjectManager,
                                 ClientShipManager clientShipManager,
                                 WarpHoleManager warpholeManager,
                                 GameWindow gameWindow)
        {
            textFont = Content.Load<SpriteFont>(@"GUI/drawFont");
            tex_Radar = Content.Load<Texture2D>(@"GUI/radar");
            tex_DotW = Content.Load<Texture2D>(@"GUI/dot_w");
            tex_OrbitalRing = Content.Load<Texture2D>(@"GUI/planetrings");
            tex_Ship = Content.Load<Texture2D>(@"GUI/Minimap/Hud_ShipRadar");
            tex_NetworkShip = Content.Load<Texture2D>(@"GUI/Minimap/otherShips");
            tex_EdgeOfRadar = Content.Load<Texture2D>(@"GUI/Minimap/edgeofradar");
            tex_Sun = Content.Load<Texture2D>(@"GUI/Minimap/sunMinimap");
            tex_Warp = Content.Load<Texture2D>(@"GUI/Minimap/warp3");

            _graphics = graphics;
            _content = Content;
            _borderManager = borderManager;
            _playerShipManager = playerShipManager;
            _projectileManager = projectileManager;
            _clientShipManager = clientShipManager;
            _radarCamera = new Camera2D(gameWindow);
            _spaceObjectManager = spaceObjectManager;
            _spriteBatch = spriteBatch;
            _warpholeManager = warpholeManager;

            radarWidth = 300;
            radarHeight = 300;
            radarTarget = new RenderTarget2D(spriteBatch.GraphicsDevice, radarWidth, radarHeight, false,
                                             SurfaceFormat.Color, DepthFormat.Depth24);

            // Adds all of the dynamically sized rings for Minimap.
            planetRings = new List<Texture2D>();
            planetRings.Add(Content.Load<Texture2D>(@"GUI/Minimap/BaseRing_50px"));
            planetRings.Add(Content.Load<Texture2D>(@"GUI/Minimap/BaseRing_68px"));
            planetRings.Add(Content.Load<Texture2D>(@"GUI/Minimap/BaseRing_80px"));
            planetRings.Add(Content.Load<Texture2D>(@"GUI/Minimap/BaseRing_100px"));
            planetRings.Add(Content.Load<Texture2D>(@"GUI/Minimap/BaseRing_120px"));
            planetRings.Add(Content.Load<Texture2D>(@"GUI/Minimap/BaseRing_150px"));
            planetRings.Add(Content.Load<Texture2D>(@"GUI/Minimap/BaseRing_200px"));
            planetRings.Add(Content.Load<Texture2D>(@"GUI/Minimap/BaseRing_250px"));
            planetRings.Add(Content.Load<Texture2D>(@"GUI/Minimap/BaseRing_300px"));

            moonRings = new List<Texture2D>();
            moonRings.Add(Content.Load<Texture2D>(@"GUI/Minimap/BaseMoonRing_30px"));
            moonRings.Add(Content.Load<Texture2D>(@"GUI/Minimap/BaseMoonRing_40px"));
            moonRings.Add(Content.Load<Texture2D>(@"GUI/Minimap/BaseMoonRing_50px"));
            moonRings.Add(Content.Load<Texture2D>(@"GUI/Minimap/BaseMoonRing_80px"));
            moonRings.Add(Content.Load<Texture2D>(@"GUI/Minimap/BaseMoonRing_100px"));
            moonRings.Add(Content.Load<Texture2D>(@"GUI/Minimap/BaseMoonRing_120px"));
            moonRings.Add(Content.Load<Texture2D>(@"GUI/Minimap/BaseMoonRing_150px"));

            _radarCamera._zoom = 1.2f;
        }

        public void Update()
        {
            if (IsMouseOver())
            {
                if (MouseManager.ScrolledUp)
                {
                    _radarCamera._zoom += 0.1f;
                }
                else if (MouseManager.ScrolledDown)
                {
                    _radarCamera._zoom -= 0.1f;
                }

                _radarCamera._zoom = MathHelper.Clamp(_radarCamera._zoom, 0.8f, 2f);
            }
        }

        public bool IsMouseOver()
        {
            if (MouseManager.CurrentPosition.X > _spriteBatch.GraphicsDevice.Viewport.Width - radarTarget.Width &&
                MouseManager.CurrentPosition.Y < radarTarget.Height
                && MouseManager.CurrentPosition.X < _spriteBatch.GraphicsDevice.Viewport.Width && MouseManager.CurrentPosition.Y > 0)
            {
                return true;
            }
            return false;
        }

        public void DrawMinimap()
        {
            //Grab a square of the Track image that is around the Car
            _spriteBatch.GraphicsDevice.SetRenderTarget(radarTarget);
            _spriteBatch.GraphicsDevice.Clear(ClearOptions.Target, new Color(2, 0, 10, 100), 0, 0);

            _radarCamera.Pos = new Vector2((float)Math.Round((_playerShipManager.PlayerShip.Position.X * (10 / 3f))),
                                          (float)Math.Round((_playerShipManager.PlayerShip.Position.Y * (10 / 3f)))); // Divides scale for the Radar
            // Draws all Moving Radar Elements
            _spriteBatch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend,
                               null,
                               null,
                               null,
                               null,
                               _radarCamera.GetTransformation(_spriteBatch.GraphicsDevice));

            // WARNING Gross, Hard-coded code. We need to develop a way of drawing primitives (Circles) in order to not use scaling sprites.
            foreach (var p in _spaceObjectManager.planetList) // Draws Planets and Moons
            {
                float texNumber = 100 * (p.distance * (0.00026f));
                if ((int)(texNumber) <= 25 / _radarCamera._zoom)
                {
                    DrawMinimapPlanet(p, 0, 0.00026f * 5.12f);
                }
                else if ((int)(texNumber) <= 34 / _radarCamera._zoom)
                {
                    DrawMinimapPlanet(p, 1, 0.00026f * 3.764705882352941f);
                }
                else if ((int)(texNumber) <= 40 / _radarCamera._zoom)
                {
                    DrawMinimapPlanet(p, 2, 0.00026f * 3.2f);
                }
                else if ((int)(texNumber) <= 50 / _radarCamera._zoom)
                {
                    DrawMinimapPlanet(p, 3, 0.00026f * 2.56f);
                }
                else if ((int)(texNumber) <= 60 / _radarCamera._zoom)
                {
                    DrawMinimapPlanet(p, 4, 0.00026f * 2.133333333333333f);
                }
                else if ((int)(texNumber) <= 75 / _radarCamera._zoom)
                {
                    DrawMinimapPlanet(p, 5, 0.00026f * 1.706666666666667f);
                }
                else if ((int)(texNumber) <= 100 / _radarCamera._zoom)
                {
                    DrawMinimapPlanet(p, 6, 0.00026f * 1.28f);
                }
                else if ((int)(texNumber) <= 125 / _radarCamera._zoom)
                {
                    DrawMinimapPlanet(p, 7, 0.00026f * 1.024f);
                }
                else
                {
                    DrawMinimapPlanet(p, 8, 0.00026f * 0.8533333333333333f);
                }

                _spriteBatch.Draw(tex_DotW, GetPositionInPixels(ConvertUnits.ToDisplayUnits(p.pos)), null,
                                    Color.White, 0, new Vector2(tex_DotW.Width / 2, tex_DotW.Height / 2), p.scale * 1.2f,
                                    SpriteEffects.None, .82f);

                // Draws moons (If Any) stored inside of each planet.
                if (p.hasMoons)
                {
                    foreach (var m in p.moonList)
                    {
                        if (m.planetType == PlanetTypes.Port)
                        {
                            DrawMoon(p, m, Color.Fuchsia);
                        }
                        else
                        {
                            DrawMoon(p, m, Color.White);
                        }
                    }
                }
            }
            
            foreach (Ship s in _clientShipManager.GetAllShips())
            {

                if (s.playerID != _clientShipManager.PlayerShip.playerID)
                    _spriteBatch.Draw(tex_NetworkShip,
                                        GetPositionInPixels(ConvertUnits.ToDisplayUnits(s.Position)), null,
                                        Color.White, s.Rotation,
                                        new Vector2(tex_NetworkShip.Width / 2, tex_NetworkShip.Height / 2), 0.3f,
                                        SpriteEffects.None, 0.8f);
            }

            for (int v = 0; v < _borderManager.borderList.Count(); v++)
                if (v + 1 < _borderManager.borderList.Count())
                {
                    float angle = MathHelper.ToRadians((float)Math.Atan2(
                                                            _borderManager.borderList[v + 1].Y - _borderManager.borderList[v].Y,
                                                            _borderManager.borderList[v + 1].X - _borderManager.borderList[v].X) *
                                                (float)(180 / Math.PI) + 90);
                    if (_borderManager.borderList.Count() > 2)
                        _spriteBatch.Draw(tex_EdgeOfRadar, ConvertUnits.ToDisplayUnits(_borderManager.borderList[v]) / 30f,
                                            null,
                                            Color.White, angle,
                                            new Vector2(tex_EdgeOfRadar.Width / 2, tex_EdgeOfRadar.Height), 1,
                                            SpriteEffects.None, 0.9f);
                }

            // 300px is the scale at which the Radar's Sun is drawn to represent
            _spriteBatch.Draw(tex_Sun, new Vector2(0, 0), null, Color.White, 0f,
                                new Vector2(tex_Sun.Width / 2f, tex_Sun.Height / 2f),
                                (tex_Sun.Width - 70) / 400f, SpriteEffects.None, 0.7f);
                                

            // Draws Warpholes to screen.
            for (int w = 0; w < _warpholeManager.Warpholes.Count; w++)
                _spriteBatch.Draw(tex_Warp,
                                    ConvertUnits.ToDisplayUnits(_warpholeManager.Warpholes[w].body.Position) / 30f, null,
                                    Color.White, 0, new Vector2(tex_Warp.Width / 2, tex_Warp.Height / 2), 1,
                                    SpriteEffects.None, 1f);

            for (int i = 0; i < _projectileManager._projectileList.Count; i++)
            {
                _spriteBatch.Draw(tex_DotW,
                                    GetPositionInPixels(
                                        ConvertUnits.ToDisplayUnits(_projectileManager._projectileList[i].Position)), null,
                                    Color.BlanchedAlmond, _projectileManager._projectileList[i].Rotation,
                                    new Vector2(tex_DotW.Width / 2, tex_DotW.Height / 2), 0.5f, SpriteEffects.None, .72f);
            }

            //foreach (var turret in _spaceStateManager.FindAllTurrets())
            //{
            //    _spriteBatch.Draw(tex_DotW,
            //                        GetPositionInPixels(ConvertUnits.ToDisplayUnits(turret.Position)), null,
            //                        Color.Orange, turret.Rotated,
            //                        new Vector2(tex_DotW.Width / 2, tex_DotW.Height / 2), 0.8f, SpriteEffects.None, .72f);
            //}

            _spriteBatch.End();

            // Draw Elements (Outlines, Ship Dot)
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);


            _spriteBatch.Draw(tex_Ship, new Vector2(tex_Radar.Width / 2, tex_Radar.Height / 2), null,
                              Color.White, _playerShipManager.PlayerShip.Rotation, new Vector2(tex_Ship.Width / 2, tex_Ship.Height / 2),
                              _radarCamera._zoom / 4f, SpriteEffects.None, 0.9f);
            _spriteBatch.Draw(tex_Radar, pos_Radar, null, Color.White, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);

            _spriteBatch.End();

            _spriteBatch.GraphicsDevice.SetRenderTarget(null);
        }

        private void DrawMoon(Planet p, Planet m, Color color)
        {
            _spriteBatch.Draw(tex_OrbitalRing, GetPositionInPixels(ConvertUnits.ToDisplayUnits(p.pos)), null,
                                Color.White, 0,
                                new Vector2(tex_OrbitalRing.Width / 2, tex_OrbitalRing.Height / 2),
                                (m.distance * (.00026f)), SpriteEffects.None, .91f);

            _spriteBatch.Draw(tex_DotW, GetPositionInPixels(ConvertUnits.ToDisplayUnits(m.pos)), null,
                                color, 0, new Vector2(tex_DotW.Width / 2, tex_DotW.Height / 2), 0.8f,
                                SpriteEffects.None, .82f);
        }

        private void DrawMinimapPlanet(Planet p, int ringIndex, float scale)
        {
            _spriteBatch.Draw(planetRings[ringIndex], Vector2.Zero, null,
                                Color.White, 0, new Vector2(planetRings[ringIndex].Width / 2, planetRings[ringIndex].Height / 2),
                                (p.distance * scale), SpriteEffects.None, .91f);
        }

        private static Vector2 GetPositionInPixels(Vector2 Position)
        {
            Vector2 returnMe;
            returnMe.X = (float)Math.Round(Position.X / 30f);
            returnMe.Y = (float)Math.Round(Position.Y / 30f);
            return returnMe;
        }

        public void DrawMinimapToScreen(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(radarTarget, new Vector2(spriteBatch.GraphicsDevice.Viewport.Width - radarTarget.Width, 0),
                             Color.White);
        }
    }
}