using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SRClient.Objects;
using SRClient.Mathematics;
using SRClient.GUI;
using FarseerPhysics.Collision;
using FarseerPhysics.Controllers;
using FarseerPhysics.ConvertUnits;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.Common;

namespace SRClient.Managers
{
    class ChatManager
    {

        public struct Chatline
        {
            public string text;
            public int row;
        }

        // FPS Counter
        public int numOfFrames = 0;
        public double FPS = 0;  

        private Vector2 pos_Console;
        private Texture2D tex_DialogueBox;
        private Texture2D tex_Console;
        private Texture2D tex_Radar;
        private Texture2D tex_DotW;
        private Texture2D tex_OrbitalRing;
        private Vector2 pos_Radar;
        private Texture2D tex_OverExitDialogueBox;
        private Rectangle rect_DialogueBox;

        //Radar Draw Variables//
        private RenderTarget2D radarTarget;
        private Camera2D radarCamera;
        private int radarWidth, radarHeight;

        private String Text = "Welcome to Freecon Galactic. I am Centaurion Bevar, your advisor. You are Accensi, MelKaven, humble servant to The Emperor, and slave to The Imperium. You must accept this mortal, as there are no other truths.";
        private String currentString = "";
        private String oldString = "";
        private int increment = 0;
        private SpriteFont textFont;
        private bool hasText = false;
        List<Chatline> chatList = new List<Chatline>();

        public ChatManager(ContentManager Content, SpriteBatch spriteBatch)
        {
            tex_DialogueBox = Content.Load<Texture2D>(@"GUI/DialogueBox");
            tex_Console = Content.Load<Texture2D>(@"GUI/chatbox");
            tex_OverExitDialogueBox = Content.Load<Texture2D>(@"GUI/OverExitDialogueBox");
            textFont = Content.Load<SpriteFont>(@"GUI/drawFont");
            tex_Radar = Content.Load<Texture2D>(@"GUI/radar");
            tex_DotW = Content.Load<Texture2D>(@"GUI/dot_w");
            tex_OrbitalRing = Content.Load<Texture2D>(@"GUI/planetrings");
            rect_DialogueBox = new Rectangle(0, 0, tex_DialogueBox.Width, tex_DialogueBox.Width);
            Text = WrapText(textFont, Text, tex_DialogueBox.Width - 10);

            radarCamera = new Camera2D();

            radarWidth = 300;
            radarHeight = 300;
            radarTarget = new RenderTarget2D(spriteBatch.GraphicsDevice, radarWidth, radarHeight, false, SurfaceFormat.Color, DepthFormat.Depth24);
            
        }

        public virtual void Update(GameTime gameTime)
        {
            if (KeyboardManager.IsTyping)
            {
                if (KeyboardManager.currentState.IsKeyDown(Keys.LeftShift) || (KeyboardManager.currentState.IsKeyDown(Keys.RightShift)))
                {
                    #region Capitals
                    if (KeyboardManager.currentState.IsKeyDown(Keys.A)
                        && KeyboardManager.oldState.IsKeyUp(Keys.A))
                    {
                        currentString += "A";
                        hasText = true;
                    }
                    if (KeyboardManager.currentState.IsKeyDown(Keys.B)
                        && KeyboardManager.oldState.IsKeyUp(Keys.B))
                    {
                        currentString += "B";
                        hasText = true;
                    }
                    if (KeyboardManager.currentState.IsKeyDown(Keys.C)
                        && KeyboardManager.oldState.IsKeyUp(Keys.C))
                    {
                        currentString += "C";
                        hasText = true;
                    }
                    if (KeyboardManager.currentState.IsKeyDown(Keys.D)
                        && KeyboardManager.oldState.IsKeyUp(Keys.D))
                    {
                        currentString += "D";
                        hasText = true;
                    }
                    if (KeyboardManager.currentState.IsKeyDown(Keys.E)
                        && KeyboardManager.oldState.IsKeyUp(Keys.E))
                    {
                        currentString += "E";
                        hasText = true;
                    }
                    if (KeyboardManager.currentState.IsKeyDown(Keys.F)
                        && KeyboardManager.oldState.IsKeyUp(Keys.F))
                    {
                        currentString += "F";
                        hasText = true;
                    }
                    if (KeyboardManager.currentState.IsKeyDown(Keys.G)
                        && KeyboardManager.oldState.IsKeyUp(Keys.G))
                    {
                        currentString += "G";
                        hasText = true;
                    }
                    if (KeyboardManager.currentState.IsKeyDown(Keys.H)
                        && KeyboardManager.oldState.IsKeyUp(Keys.H))
                    {
                        currentString += "H";
                        hasText = true;
                    }
                    if (KeyboardManager.currentState.IsKeyDown(Keys.I)
                        && KeyboardManager.oldState.IsKeyUp(Keys.I))
                    {
                        currentString += "I";
                        hasText = true;
                    }
                    if (KeyboardManager.currentState.IsKeyDown(Keys.J)
                        && KeyboardManager.oldState.IsKeyUp(Keys.J))
                    {
                        currentString += "J";
                        hasText = true;
                    }
                    if (KeyboardManager.currentState.IsKeyDown(Keys.K)
                        && KeyboardManager.oldState.IsKeyUp(Keys.K))
                    {
                        currentString += "K";
                        hasText = true;
                    }
                    if (KeyboardManager.currentState.IsKeyDown(Keys.L)
                        && KeyboardManager.oldState.IsKeyUp(Keys.L))
                    {
                        currentString += "L";
                        hasText = true;
                    }
                    if (KeyboardManager.currentState.IsKeyDown(Keys.M)
                        && KeyboardManager.oldState.IsKeyUp(Keys.M))
                    {
                        currentString += "M";
                        hasText = true;
                    }
                    if (KeyboardManager.currentState.IsKeyDown(Keys.N)
                        && KeyboardManager.oldState.IsKeyUp(Keys.N))
                    {
                        currentString += "N";
                        hasText = true;
                    }
                    if (KeyboardManager.currentState.IsKeyDown(Keys.O)
                        && KeyboardManager.oldState.IsKeyUp(Keys.O))
                    {
                        currentString += "O";
                        hasText = true;
                    }
                    if (KeyboardManager.currentState.IsKeyDown(Keys.P)
                        && KeyboardManager.oldState.IsKeyUp(Keys.P))
                    {
                        currentString += "P";
                        hasText = true;
                    }
                    if (KeyboardManager.currentState.IsKeyDown(Keys.Q)
                        && KeyboardManager.oldState.IsKeyUp(Keys.Q))
                    {
                        currentString += "Q";
                        hasText = true;
                    }
                    if (KeyboardManager.currentState.IsKeyDown(Keys.R)
                        && KeyboardManager.oldState.IsKeyUp(Keys.R))
                    {
                        currentString += "R";
                        hasText = true;
                    }
                    if (KeyboardManager.currentState.IsKeyDown(Keys.S)
                        && KeyboardManager.oldState.IsKeyUp(Keys.S))
                    {
                        currentString += "S";
                        hasText = true;
                    }
                    if (KeyboardManager.currentState.IsKeyDown(Keys.T)
                        && KeyboardManager.oldState.IsKeyUp(Keys.T))
                    {
                        currentString += "T";
                        hasText = true;
                    }
                    if (KeyboardManager.currentState.IsKeyDown(Keys.U)
                        && KeyboardManager.oldState.IsKeyUp(Keys.U))
                    {
                        currentString += "U";
                        hasText = true;
                    }
                    if (KeyboardManager.currentState.IsKeyDown(Keys.V)
                        && KeyboardManager.oldState.IsKeyUp(Keys.V))
                    {
                        currentString += "V";
                        hasText = true;
                    }
                    if (KeyboardManager.currentState.IsKeyDown(Keys.W)
                        && KeyboardManager.oldState.IsKeyUp(Keys.W))
                    {
                        currentString += "W";
                        hasText = true;
                    }
                    if (KeyboardManager.currentState.IsKeyDown(Keys.X)
                        && KeyboardManager.oldState.IsKeyUp(Keys.X))
                    {
                        currentString += "X";
                        hasText = true;
                    }
                    if (KeyboardManager.currentState.IsKeyDown(Keys.Y)
                        && KeyboardManager.oldState.IsKeyUp(Keys.Y))
                    {
                        currentString += "Y";
                        hasText = true;
                    }
                    if (KeyboardManager.currentState.IsKeyDown(Keys.Z)
                        && KeyboardManager.oldState.IsKeyUp(Keys.Z))
                    {
                        currentString += "Z";
                        hasText = true;
                    }
                    #endregion
                }
                else
                {
                    #region Lower Case
                    if (KeyboardManager.currentState.IsKeyDown(Keys.A)
                        && KeyboardManager.oldState.IsKeyUp(Keys.A))
                    {
                        currentString += "a";
                        hasText = true;
                    }
                    if (KeyboardManager.currentState.IsKeyDown(Keys.B)
                        && KeyboardManager.oldState.IsKeyUp(Keys.B))
                    {
                        currentString += "b";
                        hasText = true;
                    }
                    if (KeyboardManager.currentState.IsKeyDown(Keys.C)
                        && KeyboardManager.oldState.IsKeyUp(Keys.C))
                    {
                        currentString += "c";
                        hasText = true;
                    }
                    if (KeyboardManager.currentState.IsKeyDown(Keys.D)
                        && KeyboardManager.oldState.IsKeyUp(Keys.D))
                    {
                        currentString += "d";
                        hasText = true;
                    }
                    if (KeyboardManager.currentState.IsKeyDown(Keys.E)
                        && KeyboardManager.oldState.IsKeyUp(Keys.E))
                    {
                        currentString += "e";
                        hasText = true;
                    }
                    if (KeyboardManager.currentState.IsKeyDown(Keys.F)
                        && KeyboardManager.oldState.IsKeyUp(Keys.F))
                    {
                        currentString += "f";
                        hasText = true;
                    }
                    if (KeyboardManager.currentState.IsKeyDown(Keys.G)
                        && KeyboardManager.oldState.IsKeyUp(Keys.G))
                    {
                        currentString += "g";
                        hasText = true;
                    }
                    if (KeyboardManager.currentState.IsKeyDown(Keys.H)
                        && KeyboardManager.oldState.IsKeyUp(Keys.H))
                    {
                        currentString += "h";
                        hasText = true;
                    }
                    if (KeyboardManager.currentState.IsKeyDown(Keys.I)
                        && KeyboardManager.oldState.IsKeyUp(Keys.I))
                    {
                        currentString += "i";
                        hasText = true;
                    }
                    if (KeyboardManager.currentState.IsKeyDown(Keys.J)
                        && KeyboardManager.oldState.IsKeyUp(Keys.J))
                    {
                        currentString += "j";
                        hasText = true;
                    }
                    if (KeyboardManager.currentState.IsKeyDown(Keys.K)
                        && KeyboardManager.oldState.IsKeyUp(Keys.K))
                    {
                        currentString += "k";
                        hasText = true;
                    }
                    if (KeyboardManager.currentState.IsKeyDown(Keys.L)
                        && KeyboardManager.oldState.IsKeyUp(Keys.L))
                    {
                        currentString += "l";
                        hasText = true;
                    }
                    if (KeyboardManager.currentState.IsKeyDown(Keys.M)
                        && KeyboardManager.oldState.IsKeyUp(Keys.M))
                    {
                        currentString += "m";
                        hasText = true;
                    }
                    if (KeyboardManager.currentState.IsKeyDown(Keys.N)
                        && KeyboardManager.oldState.IsKeyUp(Keys.N))
                    {
                        currentString += "n";
                        hasText = true;
                    }
                    if (KeyboardManager.currentState.IsKeyDown(Keys.O)
                        && KeyboardManager.oldState.IsKeyUp(Keys.O))
                    {
                        currentString += "o";
                        hasText = true;
                    }
                    if (KeyboardManager.currentState.IsKeyDown(Keys.P)
                        && KeyboardManager.oldState.IsKeyUp(Keys.P))
                    {
                        currentString += "p";
                        hasText = true;
                    }
                    if (KeyboardManager.currentState.IsKeyDown(Keys.Q)
                        && KeyboardManager.oldState.IsKeyUp(Keys.Q))
                    {
                        currentString += "q";
                        hasText = true;
                    }
                    if (KeyboardManager.currentState.IsKeyDown(Keys.R)
                        && KeyboardManager.oldState.IsKeyUp(Keys.R))
                    {
                        currentString += "r";
                        hasText = true;
                    }
                    if (KeyboardManager.currentState.IsKeyDown(Keys.S)
                        && KeyboardManager.oldState.IsKeyUp(Keys.S))
                    {
                        currentString += "s";
                        hasText = true;
                    }
                    if (KeyboardManager.currentState.IsKeyDown(Keys.T)
                        && KeyboardManager.oldState.IsKeyUp(Keys.T))
                    {
                        currentString += "t";
                        hasText = true;
                    }
                    if (KeyboardManager.currentState.IsKeyDown(Keys.U)
                        && KeyboardManager.oldState.IsKeyUp(Keys.U))
                    {
                        currentString += "u";
                        hasText = true;
                    }
                    if (KeyboardManager.currentState.IsKeyDown(Keys.V)
                        && KeyboardManager.oldState.IsKeyUp(Keys.V))
                    {
                        currentString += "v";
                        hasText = true;
                    }
                    if (KeyboardManager.currentState.IsKeyDown(Keys.W)
                        && KeyboardManager.oldState.IsKeyUp(Keys.W))
                    {
                        currentString += "w";
                        hasText = true;
                    }
                    if (KeyboardManager.currentState.IsKeyDown(Keys.X)
                        && KeyboardManager.oldState.IsKeyUp(Keys.X))
                    {
                        currentString += "x";
                        hasText = true;
                    }
                    if (KeyboardManager.currentState.IsKeyDown(Keys.Y)
                        && KeyboardManager.oldState.IsKeyUp(Keys.Y))
                    {
                        currentString += "y";
                        hasText = true;
                    }
                    if (KeyboardManager.currentState.IsKeyDown(Keys.Z)
                        && KeyboardManager.oldState.IsKeyUp(Keys.Z))
                    {
                        currentString += "z";
                        hasText = true;
                    }
                    #endregion
                }
                #region Other
                if (KeyboardManager.currentState.IsKeyDown(Keys.Space)
                        && KeyboardManager.oldState.IsKeyUp(Keys.Space))
                {
                    currentString += " ";
                }
                if (KeyboardManager.currentState.IsKeyDown(Keys.OemPeriod)
                        && KeyboardManager.oldState.IsKeyUp(Keys.OemPeriod))
                {
                    currentString += ".";
                    hasText = true;
                }
                #endregion
            }
            if (KeyboardManager.currentState.IsKeyDown(Keys.Enter) && KeyboardManager.oldState.IsKeyUp(Keys.Enter) && !KeyboardManager.IsTyping)
            {
                KeyboardManager.IsTyping = true;
            }
            else if (KeyboardManager.currentState.IsKeyUp(Keys.Enter) && KeyboardManager.oldState.IsKeyDown(Keys.Enter) && KeyboardManager.IsTyping)
            {
                KeyboardManager.IsTyping = false;
            }
            else if (KeyboardManager.currentState.IsKeyDown(Keys.Enter) && KeyboardManager.IsTyping)
            {
                if (currentString != null)
                {
                    oldString = currentString;
                    currentString = "";
                    if (hasText)
                    {
                        Chatline c = new Chatline();
                        c.row = increment;
                        oldString = WrapText(textFont, oldString, (tex_Console.Width - 40));
                        c.text = oldString;
                        chatList.Add(c);
                        increment++;
                        hasText = false;
                    }
                }
                KeyboardManager.IsTyping = false;
            }

            //Console.WriteLine(chatList.Count());

        }

        /// <summary>
        /// Draws Radar and Chat GUI elements
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="planetList"></param>
        /// <param name="shipPos"></param>
        public virtual void Draw(SpriteBatch spriteBatch, List<Planet> planetList, Vector2 shipPos)
        {

            pos_Console.Y = spriteBatch.GraphicsDevice.Viewport.Height - tex_Console.Height;

            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);

            spriteBatch.Draw(tex_Console, pos_Console, null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 1f);
            for (int i = 0; i < chatList.Count(); i++)
            {
                Chatline c = chatList[i];
                spriteBatch.DrawString(textFont, c.text, new Vector2(pos_Console.X + 10, pos_Console.Y + (textFont.LineSpacing * c.row)),
                    Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, .9f);
                chatList[i] = c;
            }

            spriteBatch.DrawString(textFont, currentString, new Vector2(pos_Console.X + 10, pos_Console.Y + tex_Console.Height - textFont.LineSpacing - 5),
                Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, .9f);

            DebugTextManager.DrawTextToScreenLeft(spriteBatch, 12, "" + KeyboardManager.IsTyping);

            spriteBatch.End();
        }

        /// <summary>
        ///  This function splits up the String and adds lines if the String is longer (in pixels) than the maxLineWidth.
        /// </summary>
        /// <param name="spriteFont"></param>
        /// <param name="text"></param>
        /// <param name="maxLineWidth"></param>
        /// <returns></returns>
        public string WrapText(SpriteFont spriteFont, string text, float maxLineWidth)
        {
            string[] words = text.Split(' ');

            StringBuilder sb = new StringBuilder();

            float lineWidth = 0f;

            float spaceWidth = spriteFont.MeasureString(" ").X;

            foreach (string word in words)
            {
                Vector2 size = spriteFont.MeasureString(word);

                if (lineWidth + size.X < maxLineWidth)
                {
                    sb.Append(word + " ");
                    lineWidth += size.X + spaceWidth;
                }
                else
                {
                    sb.Append("\n" + word + " ");
                    lineWidth = size.X + spaceWidth;
                }
            }

            return sb.ToString();
        }

        public void DrawMinimap(SpriteBatch spriteBatch2, GraphicsDevice graphics, List<Planet> planetList, Vector2 shipPos)
        {
            //Grab a square of the Track image that is around the Car
            SpriteBatch spriteBatch = new SpriteBatch(graphics);
            graphics.SetRenderTarget(radarTarget);
            graphics.Clear(ClearOptions.Target, Color.Transparent, 0, 0);

            radarCamera.Pos = shipPos*(10/3f); // Divides scale for the Radar

            // Draws all Moving Radar Elements
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend,
                null,
                null,
                null,
                null,
                radarCamera.get_transformation(spriteBatch.GraphicsDevice));

            for (int i = 0; i < planetList.Count; i++) // Draws Planets and Moons
            {
                Planet p = planetList[i];
                if (i == 4)
                {
                    spriteBatch.Draw(tex_OrbitalRing, Vector2.Zero, null,
                        Color.Yellow, 0, new Vector2(tex_OrbitalRing.Width / 2, tex_OrbitalRing.Height / 2), (p.distance * (.00026f)), SpriteEffects.None, .91f);
                    spriteBatch.Draw(tex_DotW, ConvertUnits.ToDisplayUnits(p.pos) / 30f, null,
                        Color.Red, 0, new Vector2(tex_DotW.Width / 2, tex_DotW.Height / 2), 1, SpriteEffects.None, .9f);
                }
                else
                {
                    spriteBatch.Draw(tex_OrbitalRing, Vector2.Zero, null,
                        Color.White, 0, new Vector2(tex_OrbitalRing.Width / 2, tex_OrbitalRing.Height / 2), (p.distance * (.00026f)), SpriteEffects.None, .91f);
                    spriteBatch.Draw(tex_DotW, ConvertUnits.ToDisplayUnits(p.pos) / 30f, null,
                        Color.White, 0, new Vector2(tex_DotW.Width / 2, tex_DotW.Height / 2), 1, SpriteEffects.None, .9f);
                }
                

                // Draws moons (If Any) stored inside of each planet.
                if (p.hasMoons)
                {
                    for (int moons = 0; moons < p.moonList.Count; moons++)
                    {
                        Planet m = p.moonList[moons];

                        spriteBatch.Draw(tex_OrbitalRing, ConvertUnits.ToDisplayUnits(p.pos) / 30f, null,
                            Color.White, 0, new Vector2(tex_OrbitalRing.Width / 2, tex_OrbitalRing.Height / 2), (m.distance * (.00026f)), SpriteEffects.None, .91f);

                        spriteBatch.Draw(tex_DotW, ConvertUnits.ToDisplayUnits(m.pos) / 30, null,
                            Color.White, 0, new Vector2(tex_DotW.Width / 2, tex_DotW.Height / 2), 1, SpriteEffects.None, .9f);
                        
                        p.moonList[moons] = m;
                    }
                    planetList[i] = p;
                }
            }
            spriteBatch.End();

            // Draw Static Elements (Outlines, Ship Dot)
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);

            spriteBatch.Draw(tex_DotW, new Vector2(150,150), null,
                Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 1f);
            spriteBatch.Draw(tex_Radar, pos_Radar, null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 1f);

            spriteBatch.End();

            graphics.SetRenderTarget(null);


        }

        public void DrawMinimapToScreen(SpriteBatch spriteBatch)
        {
            Texture2D drawMe = (Texture2D)radarTarget;
            spriteBatch.Begin();
            spriteBatch.Draw(drawMe, new Vector2(spriteBatch.GraphicsDevice.Viewport.Width - drawMe.Width, 0), Color.White);

            numOfFrames++;
            DebugTextManager.DrawTextToScreenRight(spriteBatch, 0, "FPS: " + FPS);
            spriteBatch.End();
        }

    }
}
