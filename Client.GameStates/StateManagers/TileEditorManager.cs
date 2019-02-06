using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Freecon.Client.Mathematics;
using Freecon.Client.Mathematics.TileEditor;
using Freecon.Client.Objects;
using System;
using Microsoft.Xna.Framework;

// Local Usings

namespace Freecon.Client.Managers
{
    public class TileEditorManager
    {
        // ----------- //
        // Sub Classes //
        // ----------- //
        private readonly int baseSize;
        private readonly SelectionHUD hud;
        //private readonly EditorInput input;
        private readonly RenderLevel render;
        public SelectTile select;

        private SpriteBatch _spriteBatch;
        private TextureManager _textureManager;

        // Loading //
        private string State;
        private Camera2D layoutCam;
        private int leftTool;

        // Planet Level //
        private PlanetLevel planetLevel;
        public PlanetLevel publicLevel;
        private int rightTool;

        public TileEditorManager(
            ContentManager Content,
            SpriteBatch spriteBatch,
            TextureManager textureManager,
            GameWindow gameWindow)
        {
            _textureManager = textureManager;
            _spriteBatch = spriteBatch;

            State = "Loading";
            var baseSize = _textureManager.Wall;
            this.baseSize = baseSize.Width;

            hud = new SelectionHUD(Content);
            layoutCam = new Camera2D(gameWindow);
            render = new RenderLevel(_textureManager);
            select = new SelectTile(this.baseSize);

            layoutCam.Pos = planetLevel.SetToMiddle(planetLevel);
            State = "Rendering";
        }

        public TileEditorManager(ContentManager Content, PlanetLevel inputLevel, GameWindow gameWindow)
        {
            State = "Loading";
            var baseSize = Content.Load<Texture2D>(@"Tileset/Earth/Tile_Earth_Wall");
            this.baseSize = baseSize.Width;
            hud = new SelectionHUD(Content);
            layoutCam = new Camera2D(gameWindow);
            //input = new EditorInput(baseSize.Width);
            render = new RenderLevel(_textureManager);
            select = new SelectTile(this.baseSize);
            planetLevel = inputLevel;
            State = "Rendering";
        }

        public virtual void Update(ContentManager Content, bool isActive)
        {
            switch (State)
            {
                case "Loading":
                    break;
                case "Rendering":

                    if (isActive)
                    {
                        hud.Update(ref leftTool, ref rightTool); // Updates tools via HUD

                        //input.Update(ref planetLevel, ref planetChanged, ref layoutCam, Content, ref exit, ref State);

                        render.Update();
                    }
                    break;
                case "Transitional":
                    throw new Exception("Oh god Jesus Christ Free");
                    //_gameStateManager.SetState(GameStates.testLevel);
                    publicLevel = planetLevel;
                    break;
            }
        }

        public virtual void Draw(bool isActive)
        {
            switch (State)
            {
                case "Loading":
                    _spriteBatch.Begin();

                    //textDrawingService.DrawTextToScreenLeft(_spriteBatch, 3, "Loading Level");

                    _spriteBatch.End();
                    break;

                case "Rendering":
                    _spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend,
                                      null,
                                      null,
                                      null,
                                      null,
                                      layoutCam.GetTransformation(_spriteBatch.GraphicsDevice));

                    if (isActive && isInWindow(_spriteBatch))
                        select.setTileFromClick(_spriteBatch, baseSize, layoutCam, ref planetLevel, hud.currentTool,
                                                hud.currentRightTool, hud.isHudUp());

                    render.Draw(_spriteBatch, layoutCam, baseSize, planetLevel);
                    _spriteBatch.End();

                    _spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);

                    hud.Draw(_spriteBatch); // Draws HUD on screen.

                    _spriteBatch.End();

                    break;
            }
        }

        private bool isInWindow(SpriteBatch spriteBatch)
        {
            if (MouseManager.CurrentPosition.X >= 0 && MouseManager.CurrentPosition.Y >= 0
                && MouseManager.CurrentPosition.X < spriteBatch.GraphicsDevice.Viewport.Width
                && MouseManager.CurrentPosition.Y < spriteBatch.GraphicsDevice.Viewport.Height)
            {
                return true;
            }
            return false;
        }
    }
}
