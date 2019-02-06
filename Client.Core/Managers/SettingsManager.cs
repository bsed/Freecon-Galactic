using System;
using System.IO;
using System.Linq;
using Freecon.Client.Mathematics;

namespace Freecon.Client.Managers
{
    public class SettingsManager
    {
        public static int GameWidth, GameHeight;
        public static bool Fullscreen;
        public static bool ChatAlwaysFocus;
        public static int Quality;
        public static bool isBloom = true;

        public SettingsManager()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory;
            ClientLogger.LogInfo("Base Path: " + path);
            try
            {
                if (!File.Exists(path + "settings.txt"))
                {
                    ClientLogger.LogInfo("Settings file not found, creating default");
                    // Writes settings file with base configuration.
                    var file = new StreamWriter(path + "settings.txt", false);
                    file.WriteLine("Settings File");
                    file.WriteLine("GraphicsWidth: 1024");
                    file.WriteLine("GraphicsHeight: 768");
                    file.WriteLine("Fullscreen: False");
                    file.WriteLine("Quality: High");
                    file.WriteLine("# Controls can be bound to custom keys here. Multiple keys are separated by commas.");
                    file.WriteLine("# Example: KeyBind ThrustUp = Up, W");
                    file.WriteLine("KeyBind: ThrustUp = Up, W");
                    file.Close();
                }
                var strings = File.ReadAllLines(path + "settings.txt");
                    // Reads the whole file, returns each line as a string
                ClientLogger.LogInfo("Read " + strings.Count() + " lines");

                for (int i = 0; i < strings.Count(); i++)
                {
                    if (strings.Count() == 0)
                        continue;
                    ClientLogger.LogInfo("Line: " + strings[i]);

                    if (strings[i].StartsWith("#"))
                    {
                        continue;
                    }
                    else if (strings[i].StartsWith("GraphicsWidth: "))
                    {
                        string[] line = strings[i].Split(' ');
                        try
                        {
                            RenderingMath.WindowSizeX = int.Parse(line[1]);
                            GameWidth = int.Parse(line[1]);
                            RenderingMath.IsChanged = true;
                        }
                        catch
                        {
                            ClientLogger.Log(Log_Type.WARNING, "Invalid GraphicsWidth specified. Defaulting to 1024x768");
                            RenderingMath.WindowSizeX = 1366;
                            RenderingMath.WindowSizeY = 768;
                            RenderingMath.IsChanged = true;
                        }
                    }
                    else if (strings[i].StartsWith("GraphicsHeight: "))
                    {
                        string[] line = strings[i].Split(' ');
                        try
                        {
                            RenderingMath.WindowSizeY = int.Parse(line[1]);
                            GameHeight = int.Parse(line[1]);
                            RenderingMath.IsChanged = true;
                        }
                        catch
                        {
                            ClientLogger.Log(Log_Type.WARNING, "Invalid GraphicsHeight specified. Defaulting to 1024x768");
                            RenderingMath.WindowSizeX = 1366;
                            RenderingMath.WindowSizeY = 768;
                            GameWidth = 1366;
                            GameHeight = 768;
                            RenderingMath.IsChanged = true;
                        }
                    }
                    else if (strings[i].StartsWith("Fullscreen: "))
                    {
                        string[] line = strings[i].Split(' ');
                        try
                        {
                            if (bool.Parse(line[1]))
                            {
                                RenderingMath.Fullscreen = true;
                                Fullscreen = true;
                                RenderingMath.IsChanged = true;
                            }
                            else
                            {
                                RenderingMath.Fullscreen = false;
                                Fullscreen = false;
                                RenderingMath.IsChanged = true;
                            }
                        }
                        catch
                        {
                            ClientLogger.Log(Log_Type.WARNING, "Invalid Fullscreen boolean specified. Defaulting to Windowed");
                            RenderingMath.Fullscreen = false;
                            Fullscreen = false;
                            RenderingMath.IsChanged = true;
                        }
                    }
                    //else if (strings[i].StartsWith("ChatAlwaysFocus: "))
                    //{
                    //    string[] line = strings[i].Split(' ');
                    //    try
                    //    {
                    //        if (bool.Parse(line[1]))
                    //        {
                    //            _chatManager.chatAlwaysFocus = true;
                    //            ChatAlwaysFocus = true;
                    //        }
                    //        else
                    //        {
                    //            _chatManager.chatAlwaysFocus = false;
                    //            ChatAlwaysFocus = false;
                    //            RenderingMath.IsChanged = true;
                    //        }
                    //    }
                    //    catch
                    //    {
                    //        Logger.Log(Log_Type.WARNING, "Invalid Keyboard Focus boolean specified. Defaulting to false");
                    //        _chatManager.chatAlwaysFocus = false;
                    //        ChatAlwaysFocus = false;
                    //    }
                    //}
                    else if (strings[i].StartsWith("KeyBind: "))
                    {
                        KeyboardManager.CheckForKeyBind(strings[i]);
                    }
                }
            }
            catch
            {
                ClientLogger.Log(Log_Type.ERROR, "Couldn't find settings file, or write new one. " + path + "settings.txt");
                throw new FileLoadException("Couldn't find settings file, or write new one", path + "settings.txt");
            }
        }
    }
}