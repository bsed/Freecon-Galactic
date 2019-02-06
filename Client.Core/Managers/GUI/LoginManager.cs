using System.Collections.Generic;
using Microsoft.Xna.Framework; using Core.Interfaces;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace Freecon.Client.Managers.GUI
{
    /// <summary>
    /// Modular Login GameWindow designed to allow future windows to be easily implemented.
    /// Potential options include, Login Page, Options, Updates, etc.
    /// This does not include receiving login messages/packets, for that check Main_Network.cs
    /// </summary>
    public class LoginManagerGui
    {
        public static string[] LoginInfo;
        private readonly Texture2D LoginBackground;
        private readonly SpriteFont LoginFont;

        private readonly List<Window> WindowList;
        public bool ButtonClicked = false;
        private Texture2D DebugDot;

        private int ViewportHeight;
        private int ViewportWidth;
        private bool changed = true; // Possible performance improvement by checking viewport size changed, very small.
        private bool debugView = false; // Toggles debug features

        public LoginManagerGui(ContentManager Content, GraphicsDevice graphics)
        {
            LoginBackground = Content.Load<Texture2D>(@"GUI/Login/LoginBackground");
            LoginFont = Content.Load<SpriteFont>(@"GUI/LoginFont");

            DebugDot = Content.Load<Texture2D>(@"GUI/dot_w");

            ViewportWidth = graphics.Viewport.Width;
            ViewportHeight = graphics.Viewport.Height;

            WindowList = new List<Window>();

            var SizeOfWindow = new Vector2(478, 422);
            var w = new Window(Content, "Login", // Spawns base login window.
                               new Vector2(ViewportWidth/2, ViewportHeight/2),
                               //new Vector2((ViewportWidth / 2) - (SizeOfWindow.X / 2), (ViewportHeight / 2) - (SizeOfWindow.Y / 2)),
                               new Vector2(478, 422));
            WindowList.Add(w);
        }

        public void Update(IGameTimeService gameTime)
        {
            if (WindowList.Count != 0)
            {
                for (int i = 0; i < WindowList.Count; i++)
                {
                    WindowList[i].Update(new Vector2(ViewportWidth, ViewportHeight));
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Calculate everything based on top-left of main window, but we need Viewport to size that up.
            ViewportWidth = spriteBatch.GraphicsDevice.Viewport.Width;
            ViewportHeight = spriteBatch.GraphicsDevice.Viewport.Height;

            spriteBatch.Begin();
            // Draws only what's on screen. Apparently these needed to be negative values...
            spriteBatch.Draw(LoginBackground,
                             new Rectangle(-(LoginBackground.Width - (spriteBatch.GraphicsDevice.Viewport.Width))/2,
                                           -(LoginBackground.Height - (spriteBatch.GraphicsDevice.Viewport.Height))/2,
                                           LoginBackground.Width, LoginBackground.Height), Color.White);

            if (WindowList.Count != 0)
                for (int i = 0; i < WindowList.Count; i++)
                    WindowList[i].Draw(spriteBatch);


            spriteBatch.End();

            if (debugView)
            {
                spriteBatch.Begin();
                spriteBatch.DrawString(LoginFont,
                                       "Mouse: (" + (MouseManager.CurrentPosition.X - ViewportWidth/2) + ", " +
                                       (MouseManager.CurrentPosition.Y - ViewportHeight/2) + ")",
                                       new Vector2(40, 40), Color.Aquamarine);
                spriteBatch.DrawString(LoginFont, "Input: (" + WindowList[0].SelectedButton + ")",
                                       new Vector2(40, 70), Color.Aquamarine);
                spriteBatch.End();
            }
        }
    }

    internal class Button
    {
        public string ButtonText;
        public Vector2 Location, Size;

        public bool hasText;
        public int type;

        public Button(Vector2 Location, Vector2 Size, int buttonType)
        {
            this.Location = Location;
            this.Size = Size;
            ButtonText = "";
            hasText = false;
            type = buttonType;
        }

        /// <summary>
        /// Checks if a button is being clicked. Returns true or false.
        /// </summary>
        /// <param name="Origin">The top left of the window.</param>
        /// <returns>If Button clicked</returns>
        public bool IsClicked(Vector2 Origin)
        {
            // If inside of window
            if (MouseManager.CurrentPosition.X > Origin.X + Location.X &&
                MouseManager.CurrentPosition.Y > Origin.Y + Location.Y
                && MouseManager.CurrentPosition.X < Origin.X + Location.X + Size.X &&
                MouseManager.CurrentPosition.Y < Origin.Y + Location.Y + Size.Y
                && MouseManager.LeftButtonPressed)
            {
                return true;
            }
            return false;
        }
    }

    internal class Window
    {
        private readonly List<Button> Buttons;
        private readonly SpriteFont InputRenderFont;
        private readonly string Type;
        private readonly Texture2D tex_Selected;
        private readonly Texture2D tex_Window;
        private Vector2 DrawWindowOrigin;
        private int HorizontalBuffer = 10; // Space used to buffer input fields

        public int SelectedButton = -1;
        private Vector2 SizeOfWindow;
        private int VerticalBuffer = 9; // Space used to buffer input fields
        private Vector2 WindowContentOrigin;
        private Texture2D debugDot;

        public Window(ContentManager Content, string WindowType, Vector2 DrawOrigin, Vector2 WindowSize)
        {
            SizeOfWindow = WindowSize;

            Buttons = new List<Button>(); // Holds clickable areas
            Button b; // Used as a GC-friendly temp variable

            switch (WindowType)
            {
                case "Login":
                    // Initialize Textures
                    tex_Window = Content.Load<Texture2D>(@"GUI/Login/LoginScreen0.04");
                    tex_Selected = Content.Load<Texture2D>(@"GUI/Login/SelectedButtonOverlay");
                    InputRenderFont = Content.Load<SpriteFont>(@"GUI/LoginFont");

                    // Origin of top-left, including shadow
                    DrawWindowOrigin = new Vector2((DrawOrigin.X) - (tex_Window.Width/2),
                                                   (DrawOrigin.Y) - (tex_Window.Height/2));
                    // Origin of top-left, only contentbox (not shadow)
                    WindowContentOrigin = new Vector2((DrawOrigin.X) - (SizeOfWindow.X/2),
                                                      (DrawOrigin.Y) - (SizeOfWindow.Y/2));

                    b = new Button(LoginWindow.UsernameOrigin, LoginWindow.UsernameSize, (int) ButtonType.UsernameInput);
                        // Top left of Username
                    Buttons.Add(b);
                    b = new Button(LoginWindow.PasswordOrigin, LoginWindow.PasswordSize, (int) ButtonType.PasswordInput);
                        // Top left of Password
                    Buttons.Add(b);
                    b = new Button(LoginWindow.LoginOrigin, LoginWindow.LoginSize, (int) ButtonType.Clickbox);
                        // Top left of Login
                    Buttons.Add(b);
                    b = new Button(LoginWindow.OptionsOrigin, LoginWindow.OptionsSize, (int) ButtonType.Clickbox);
                        // Top left of Options
                    Buttons.Add(b);
                    break;
            }
            Type = WindowType;
        }

        public void Update(Vector2 ViewportSize)
        {
            // Used to draw from center
            DrawWindowOrigin = new Vector2((ViewportSize.X/2) - (tex_Window.Width/2),
                                           (ViewportSize.Y/2) - (tex_Window.Height/2));
            WindowContentOrigin = new Vector2((ViewportSize.X/2) - (SizeOfWindow.X/2),
                                              (ViewportSize.Y/2) - (SizeOfWindow.Y/2));

            // Checks if anything is clicked
            for (int i = 0; i < Buttons.Count; i++)
            {
                if (Buttons[i].IsClicked(WindowContentOrigin))
                    SelectedButton = i;
            }

            // If anything is clicked, or has been clicked, perform key input check
            if (SelectedButton != -1)
            {
                KeyboardManager.GetTyping(ref Buttons[SelectedButton].ButtonText, ref Buttons[SelectedButton].hasText);
            }

            // Used to toggle between Username and Password
            if (Type == "Login" && KeyboardManager.IsTapped(Keys.Tab))
            {
                SelectedButton++;
                if (SelectedButton > 1)
                    SelectedButton = 0;
            }
                // If enter is pressed or login is clicked, perform login
            else if (Type == "Login" && KeyboardManager.EnterChatline.IsBindTapped() || SelectedButton == 2)
            {
                LoginManagerGui.LoginInfo = new string[2];
                LoginManagerGui.LoginInfo[0] = Buttons[0].ButtonText;
                LoginManagerGui.LoginInfo[1] = Buttons[1].ButtonText;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(tex_Window, DrawWindowOrigin, Color.White);
            for (int i = 0; i < Buttons.Count; i++)
            {
                if (Buttons.Count == 0) // Avoid NullReferenceExceptions!
                    continue;
                Button b = Buttons[i];

                switch ((ButtonType) b.type)
                {
                    case ButtonType.UsernameInput:
                        if (SelectedButton == i)
                            spriteBatch.Draw(tex_Selected, WindowContentOrigin + b.Location, Color.White);

                        Vector2 UsernameSize = InputRenderFont.MeasureString(b.ButtonText);
                        spriteBatch.DrawString(InputRenderFont, b.ButtonText,
                                               WindowContentOrigin + new Vector2(
                                                                         b.Location.X + b.Size.X - UsernameSize.X -
                                                                         HorizontalBuffer,
                                                                         b.Location.Y + VerticalBuffer),
                                               Color.GhostWhite);
                        break;
                    case ButtonType.PasswordInput:
                        if (SelectedButton == i)
                            spriteBatch.Draw(tex_Selected, WindowContentOrigin + b.Location, Color.White);

                        if (b.ButtonText.Length > 0)
                        {
                            string PasswordCryptic = "";
                            for (int s = 0; s < b.ButtonText.Length; s++)
                                PasswordCryptic += "*";

                            Vector2 PasswordSize = InputRenderFont.MeasureString(PasswordCryptic);

                            spriteBatch.DrawString(InputRenderFont, PasswordCryptic,
                                                   WindowContentOrigin + new Vector2(
                                                                             b.Location.X + b.Size.X - PasswordSize.X -
                                                                             HorizontalBuffer,
                                                                             b.Location.Y + VerticalBuffer),
                                                   Color.GhostWhite);
                        }
                        break;
                }
            }
        }
    }

    public enum ButtonType
    {
        UsernameInput,
        PasswordInput,
        Clickbox
    }

    public class LoginWindow
    {
        public static readonly Vector2
            UsernameOrigin = new Vector2(14, 172),
            UsernameSize = new Vector2(455, 55),
            PasswordOrigin = new Vector2(14, 247),
            PasswordSize = new Vector2(455, 55),
            LoginOrigin = new Vector2(252, 324),
            LoginSize = new Vector2(180, 55),
            OptionsOrigin = new Vector2(48, 324),
            OptionsSize = new Vector2(180, 55);
    }
}