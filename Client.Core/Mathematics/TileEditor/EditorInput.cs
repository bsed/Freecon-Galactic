namespace Freecon.Client.Mathematics.TileEditor
{
    //internal class EditorInput
    //{
    //    // Variables //
    //    private readonly int TileSize;
    //    //private readonly XMLSaver x;
    //    private int delayTime = 5000;
    //    private String[] filenames;
    //    private bool helpMenu;
    //    // Mouse Variable
    //    private int lastScrollValue;
    //    private int loadWait = 600;

    //    // Performance & Update Variables
    //    private int oldTime;
    //    private String saveLocation;
    //    private int speed;
    //    private int waitTime = 24;

    //    public EditorInput(int Tilesize)
    //    {
    //        TileSize = Tilesize;
    //        //x = new XMLSaver();
    //        //filenames = x.LoadConfig("config.txt");
    //        saveLocation = "Save.xml";
    //        speed = 5;
    //    }

    //    public void Update(
    //        ref PlanetLevel level, ref bool levelChanged,
    //        ref Camera2D layoutCamera, ContentManager Content, ref bool exit, ref string State)
    //    {
    //        // Performance & Time Variables
    //        oldTime += _gameTime.ElapsedMilliseconds; // Updates Time since Last Action
    //        delayTime += _gameTime.ElapsedMilliseconds; // Used for Loading.

    //        #region Input Parsing

    //        // ----------- //
    //        // Mouse Input //
    //        // ----------- //

    //        if (MouseManager.currentState.ScrollWheelValue < lastScrollValue)
    //            layoutCamera.Zoom -= 0.06f;
    //        else if (MouseManager.currentState.ScrollWheelValue > lastScrollValue)
    //            layoutCamera.Zoom += 0.06f;

    //        lastScrollValue = MouseManager.currentState.ScrollWheelValue;

    //        // -------------- //
    //        // Keyboard Input //
    //        // -------------- //

    //        if (KeyboardManager.currentState.IsKeyDown(Keys.S))
    //        {
    //            layoutCamera.Zoom -= 0.02f;
    //        } // Zooms out
    //        if (KeyboardManager.currentState.IsKeyDown(Keys.W))
    //        {
    //            layoutCamera.Zoom += 0.02f;
    //        } // Zooms in

    //        if (KeyboardManager.currentState.IsKeyDown(Keys.E))
    //        {
    //            layoutCamera.Zoom = 1f;
    //        } // Resets Zoom
    //        if (KeyboardManager.currentState.IsKeyDown(Keys.Q))
    //        {
    //            layoutCamera.Zoom = 0.05f;
    //        } // Zooms all the way out

    //        //Movement Detection
    //        if (KeyboardManager.ThrustUp.IsBindPressed())
    //            layoutCamera.Pos = new Vector2(layoutCamera.Pos.X, layoutCamera.Pos.Y - speed); // Scrolls Up

    //        if (KeyboardManager.ThrustDown.IsBindPressed())
    //            layoutCamera.Pos = new Vector2(layoutCamera.Pos.X, layoutCamera.Pos.Y + speed); // Scrolls Down

    //        if (KeyboardManager.TurnLeft.IsBindPressed())
    //            layoutCamera.Pos = new Vector2(layoutCamera.Pos.X - speed, layoutCamera.Pos.Y); // Scrolls Left

    //        if (KeyboardManager.TurnRight.IsBindPressed())
    //            layoutCamera.Pos = new Vector2(layoutCamera.Pos.X + speed, layoutCamera.Pos.Y); // Scrolls Right

    //        if (oldTime > waitTime) // Limits to 1000/waitTime actions per turn
    //        {
    //            oldTime = 0;
    //            // Speed Variables
    //            if (KeyboardManager.currentState.IsKeyDown(Keys.PageUp))
    //                speed += 2; // Changes speed of movement

    //            if (KeyboardManager.currentState.IsKeyDown(Keys.PageDown))
    //            {
    //                if (speed > 0) speed -= 2;
    //                else speed = 1;
    //            } // Changes speed of movement

    //            if (KeyboardManager.currentState.IsKeyDown(Keys.Escape))
    //                exit = true; // Exits the Game
    //        }

    //        if (KeyboardManager.currentState.IsKeyDown(Keys.O)) // Saves level
    //        {
    //            if (delayTime > 200)
    //                //x.SaveLevel(level, saveLocation);
    //            delayTime = 0;
    //        }

    //        if (KeyboardManager.currentState.IsKeyDown(Keys.L)) // Loads Level
    //            if (delayTime > loadWait)
    //            {
    //                State = "Loading";
    //                //level = x.LoadLevel(Content, "Save.xml");
    //                delayTime = 0;
    //                State = "Rendering";
    //            }

    //        // If you delete your Config, uncomment this.
    //        /*if (keys.IsKeyDown(Keys.X)) // Saves Config
    //        {
    //            if (delayTime > loadWait)
    //            {
    //                x.SaveConfig("config.txt");
    //            }
    //        }*/

    //        if (KeyboardManager.currentState.IsKeyDown(Keys.OemPeriod)) // Loads Config
    //        {
    //            if (delayTime > loadWait)
    //            {
    //                //filenames = x.LoadConfig("config.txt");
    //            }
    //        }

    //        if (KeyboardManager.currentState.IsKeyDown(Keys.H) && helpMenu == false) // Toggles Help
    //        {
    //            if (delayTime > 50)
    //            {
    //                helpMenu = true;
    //            }
    //            delayTime = 0;
    //        }

    //        if (KeyboardManager.currentState.IsKeyDown(Keys.H) && helpMenu) // Toggles Help
    //        {
    //            if (delayTime > 50)
    //            {
    //                helpMenu = false;
    //            }
    //            delayTime = 0;
    //        }
    //        if (KeyboardManager.currentState.IsKeyDown(Keys.T) && KeyboardManager.oldState.IsKeyUp(Keys.T))
    //            // Test Level
    //        {
    //            if (delayTime > 500)
    //            {
    //                State = "Transitional";
    //            }
    //            delayTime = 0;
    //        }

    //        LoadLevel(Content, ref level, ref layoutCamera);

    //        #endregion

    //        if (layoutCamera.Zoom > 1f) layoutCamera.Zoom = 1f; // Can't zoom in past 1x
    //    }

    //    public void LoadLevel(ContentManager Content, ref PlanetLevel l, ref Camera2D layoutCamera)
    //    {
    //        if (KeyboardManager.currentState.IsKeyDown(Keys.D1)) // Loads Level
    //        {
    //            if (delayTime > loadWait)
    //            {
    //                if (filenames[0] != "FileName")
    //                {
    //                    //l = x.LoadLevel(Content, filenames[0]);
    //                    layoutCamera.Pos = SetToMiddle(l);
    //                    delayTime = 0;
    //                    saveLocation = filenames[0];
    //                }
    //            }
    //        }
    //        if (KeyboardManager.currentState.IsKeyDown(Keys.D2)) // Loads Level
    //        {
    //            if (delayTime > loadWait)
    //            {
    //                if (filenames[1] != "FileName")
    //                {
    //                    //l = x.LoadLevel(Content, filenames[1]);
    //                    layoutCamera.Pos = SetToMiddle(l);
    //                    delayTime = 0;
    //                    saveLocation = filenames[1];
    //                }
    //            }
    //        }
    //        if (KeyboardManager.currentState.IsKeyDown(Keys.D3)) // Loads Level
    //        {
    //            if (delayTime > loadWait)
    //            {
    //                if (filenames[2] != "FileName")
    //                {
    //                    //l = x.LoadLevel(Content, filenames[2]);
    //                    layoutCamera.Pos = SetToMiddle(l);
    //                    delayTime = 0;
    //                    saveLocation = filenames[2];
    //                }
    //            }
    //        }
    //        if (KeyboardManager.currentState.IsKeyDown(Keys.D4)) // Loads Level
    //        {
    //            if (delayTime > loadWait)
    //            {
    //                if (filenames[3] != "FileName")
    //                {
    //                    //l = x.LoadLevel(Content, filenames[3]);
    //                    layoutCamera.Pos = SetToMiddle(l);
    //                    delayTime = 0;
    //                    saveLocation = filenames[3];
    //                }
    //            }
    //        }
    //        if (KeyboardManager.currentState.IsKeyDown(Keys.D5)) // Loads Level
    //        {
    //            if (delayTime > loadWait)
    //            {
    //                if (filenames[4] != "FileName")
    //                {
    //                    //l = x.LoadLevel(Content, filenames[4]);
    //                    layoutCamera.Pos = SetToMiddle(l);
    //                    delayTime = 0;
    //                    saveLocation = filenames[4];
    //                }
    //            }
    //        }
    //        if (KeyboardManager.currentState.IsKeyDown(Keys.D6)) // Loads Level
    //        {
    //            if (delayTime > loadWait)
    //            {
    //                if (filenames[5] != "FileName")
    //                {
    //                    //l = x.LoadLevel(Content, filenames[5]);
    //                    layoutCamera.Pos = SetToMiddle(l);
    //                    delayTime = 0;
    //                    saveLocation = filenames[5];
    //                }
    //            }
    //        }
    //        if (KeyboardManager.currentState.IsKeyDown(Keys.D7)) // Loads Level
    //        {
    //            if (delayTime > loadWait)
    //            {
    //                if (filenames[6] != "FileName")
    //                {
    //                    //l = x.LoadLevel(Content, filenames[6]);
    //                    layoutCamera.Pos = SetToMiddle(l);
    //                    delayTime = 0;
    //                    saveLocation = filenames[6];
    //                }
    //            }
    //        }
    //        if (KeyboardManager.currentState.IsKeyDown(Keys.D8)) // Loads Level
    //        {
    //            if (delayTime > loadWait)
    //            {
    //                if (filenames[7] != "FileName")
    //                {
    //                    //l = x.LoadLevel(Content, filenames[7]);
    //                    layoutCamera.Pos = SetToMiddle(l);
    //                    delayTime = 0;
    //                    saveLocation = filenames[7];
    //                }
    //            }
    //        }
    //        if (KeyboardManager.currentState.IsKeyDown(Keys.D9)) // Loads Level
    //        {
    //            if (delayTime > loadWait)
    //            {
    //                if (filenames[8] != "FileName")
    //                {
    //                    //l = x.LoadLevel(Content, filenames[8]);
    //                    layoutCamera.Pos = SetToMiddle(l);
    //                    delayTime = 0;
    //                    saveLocation = filenames[8];
    //                }
    //            }
    //        }
    //    }

    //    private Vector2 SetToMiddle(PlanetLevel l)
    //    {
    //        return new Vector2(((l.xSize * TileSize) / 2), // Start in Middle
    //                           ((l.ySize * TileSize) / 2));
    //    }
    //}
}