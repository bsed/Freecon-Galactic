using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Freecon.Client.Managers;

namespace Freecon.Client.Mathematics
{
    public class RenderingMath
    {
        private GraphicsDeviceManager _graphics;

        public static int WindowSizeX, WindowSizeY;
        public static bool Fullscreen;
        public static bool IsChanged;

        public RenderingMath(GraphicsDeviceManager graphics)
        {
            _graphics = graphics;
        }

        /// <summary>
        /// Attempt to set the display mode to the desired resolution.  Itterates through the display
        /// capabilities of the default graphics adapter to determine if the graphics adapter supports the
        /// requested resolution.  If so, the resolution is set and the function returns true.  If not,
        /// no change is made and the function returns false.
        /// </summary>
        /// <param name="iWidth">Desired screen width.</param>
        /// <param name="iHeight">Desired screen height.</param>
        /// <param name="bFullScreen">True if you wish to go to Full Screen, false for Windowed Mode.</param>
        public bool InitGraphicsMode(int iWidth, int iHeight, bool bFullScreen, bool doICare)
        {
            IsChanged = false;
            // If we aren't using a full screen mode, the height and width of the window can
            // be set to anything equal to or smaller than the actual screen size.
            if (bFullScreen == false)
            {
                if ((iWidth <= GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width)
                    && (iHeight <= GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height) || doICare)
                {
                    WindowSizeX = iWidth;
                    WindowSizeY = iWidth;
                    _graphics.PreferredBackBufferWidth = iWidth;
                    _graphics.PreferredBackBufferHeight = iHeight;
                    Fullscreen = bFullScreen;
                    _graphics.IsFullScreen = bFullScreen;
                    _graphics.ApplyChanges();

                    ClientLogger.Log(Log_Type.INFO,
                               "Initialized Graphics in Windowed Mode at " + iWidth + "x" + iHeight + " resolution.");
                    return true;
                }
            }
            else
            {
                // If we are using full screen mode, we should check to make sure that the display
                // adapter can handle the video mode we are trying to set.  To do this, we will
                // iterate thorugh the display modes supported by the adapter and check them against
                // the mode we want to set.
                foreach (DisplayMode dm in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
                {
                    // Check the width and height of each mode against the passed values
                    if ((dm.Width == iWidth) && (dm.Height == iHeight))
                    {
                        // The mode is supported, so set the buffer formats, apply changes and return
                        WindowSizeX = iWidth;
                        WindowSizeY = iWidth;
                        _graphics.PreferredBackBufferWidth = iWidth;
                        _graphics.PreferredBackBufferHeight = iHeight;
                        Fullscreen = bFullScreen;
                        _graphics.IsFullScreen = bFullScreen;
                        _graphics.ApplyChanges();

                        ClientLogger.Log(Log_Type.INFO,
                                   "Initialized Graphics in Fullscreen Mode at " + iWidth + "x" + iHeight +
                                   " resolution.");
                        return true;
                    }
                }
            }
            return false;
        }

        public void Update()
        {
            if (IsChanged)
                if (_graphics.PreferredBackBufferWidth != WindowSizeX ||
                    _graphics.PreferredBackBufferHeight != WindowSizeY ||
                    _graphics.IsFullScreen != Fullscreen)
                {
                    _graphics.PreferredBackBufferWidth = WindowSizeX;
                    _graphics.PreferredBackBufferHeight = WindowSizeY;
                    _graphics.IsFullScreen = Fullscreen;
                    _graphics.ApplyChanges();
                    IsChanged = false;
                }
        }
    }
}