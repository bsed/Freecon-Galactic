using CefSharp;
using CefSharp.OffScreen;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Drawing;
using System.Linq;
using Client.View.JSMarshalling;
using Server.Managers;
using Core.Interfaces;
using Microsoft.Xna.Framework.Graphics;
using XPoint = Microsoft.Xna.Framework.Point;
using System.IO;

namespace Freecon.Client.CefSharpWrapper
{
    public class CEFSharpUI
    {
        protected ChromiumWebBrowser _browser;

        protected InstructionRegistry _instructionRegistry;

        WrapperInputHandler _inputHandler;

        public bool IsLoaded { get; protected set; }

        public Bitmap Bitmap { get { return _browser.Bitmap; } }

        string _currentUrl;

        public XPoint Position { get; protected set; }

        Size _browserSize;
        
        public Texture2D UITexture { get; protected set; }

        public EventHandler<LoadingStateChangedEventArgs> LoadCompleted;

        Queue<Tuple<object, string>> _methodHandlerQueue;
        

        private Graphics _graphics;

        //debug
        private double _lastUpdateTicks;
        private int counter = 0;
        Stopwatch _initTimer = new Stopwatch();

        public CEFSharpUI()
        {
            _instructionRegistry = new InstructionRegistry();
            _methodHandlerQueue = new Queue<Tuple<object, string>>();
        }

        ~CEFSharpUI()
        {
            InputSystem.ForceStop = true;
        }

        public void Load(IntPtr windowHandle, Texture2D renderTexture, XPoint position, string url)
        {
            _currentUrl = url;
            UITexture = renderTexture;
            _browserSize = new Size(renderTexture.Width, renderTexture.Height);

            var settings = new CefSettings();
            
            var osVersion = Environment.OSVersion;
            //Disable GPU for Windows 7
            if (osVersion.Version.Major == 6 && osVersion.Version.Minor == 1)
            {
                // Disable GPU in WPF and Offscreen examples until #1634 has been resolved
                settings.CefCommandLineArgs.Add("disable-gpu", "1");

            }

            _initTimer.Start();

            if (!Cef.IsInitialized)
            {
                //Perform dependency check to make sure all relevant resources are in our output directory.
                Cef.Initialize(settings, true, null);
            }

            _graphics = Graphics.FromHwnd(windowHandle);

            // Leave last argument false, otherwise an exception will be thrown when registering callbacks
            _browser = new ChromiumWebBrowser("", null, null, false);

            // Register callbacks for Javascript to use
            _instructionRegistry.RegisterBrowser(_browser);

            // _browser.ConsoleMessage += _browser_ConsoleMessage;

            _browser.Size = _browserSize;
            
            _browser.LoadError += _browser_LoadError;
            _browser.BrowserInitialized += _browser_BrowserInitialized;
            _browser.LoadingStateChanged += _browser_LoadingStateChanged;

            _browser.CreateBrowser(windowHandle);

            Position = position;
            _inputHandler = new WrapperInputHandler(_browser, Position);
            _initInputHandlers(windowHandle);
            
        }

        private void _browser_ConsoleMessage(object sender, ConsoleMessageEventArgs e)
        {
            Console.WriteLine("{0}: {1}", e.Line, e.Message);
        }

        private void _browser_BrowserInitialized(object sender, EventArgs e)
        {
            try
            {
                ConsoleManager.WriteLine("Browser took " + _initTimer.ElapsedMilliseconds);
                
                _browser.Load(_currentUrl);

                //_browser.ShowDevTools();
            }
            catch (Exception ex)
            {
                ConsoleManager.WriteLine(ex.ToString(), ConsoleMessageType.Error);
            }
        }

        private void _browser_LoadError(object sender, LoadErrorEventArgs e)
        {
            string message = "Failed to load " + e.FailedUrl + ". " + e.ErrorText + " error code " + e.ErrorCode;
            ConsoleManager.WriteLine(message, ConsoleMessageType.Error);            
        }

        private void _browser_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            if (!e.IsLoading)
            {
                IsLoaded = true;
                LoadCompleted?.Invoke(this, e);
                //_browser.Size = _browserSize;
            }
        }
        
        public void ShutDown()
        {
            _instructionRegistry.UnregisterEverything();

            // Clean up Chromium objects.  You need to call this in your application otherwise
            // you will get a crash when closing.
            Cef.Shutdown();
        }

        protected void _initInputHandlers(IntPtr windowHandle)
        {
            try
            {
                if (!InputSystem.Initialized)
                {
                    InputSystem.Initialize(windowHandle);
                }

                InputSystem.FullKeyHandler += _inputHandler.FullKeyHandler;
                InputSystem.MouseMove += _inputHandler.MouseMoveHandler;
                InputSystem.MouseDown += _inputHandler.MouseDownHandler;
                InputSystem.MouseUp += _inputHandler.MouseUpHandler;
                InputSystem.MouseWheel += _inputHandler.MouseWheelHandler;
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLine(e.ToString(), ConsoleMessageType.Error);
            }
        }

        public async Task<object> CallJavascriptFunction(string functionName, params object[] arguments)
        {
            //debug
            if (_browser == null)
            {
                ConsoleManager.WriteLine("Attempted to call " + functionName + " before browser was initialized.");
                return null;

            }

            if (!_browser.IsBrowserInitialized || _browser.IsLoading)
            {
                return null;
            }

            return _browser.EvaluateScriptAsync(functionName, arguments);
        }
        
        public async Task<object> CallJavascriptFunctionAsync(string functionName, params string[] arguments)
        {
            //debug
            if(_browser == null)
            {
                ConsoleManager.WriteLine("Attempted to call " + functionName + " before browser was initialized.");
                return null;

            }                  
            
            if (!_browser.IsBrowserInitialized || _browser.IsLoading)
            {
                return null;
            }

            return await _browser.EvaluateScriptAsync(functionName, arguments);
        }

        /// <summary>
        /// Register a function with a name for Javascript to call.
        /// Function must accept 1 argument type string and return type string.
        /// </summary>
        /// <param name="name">Method name for Javascript to call.</param>
        /// <param name="callback">The function to call.</param>
        public void RegisterCallback(string name, Func<JSMarshallContainer, string> callback)
        {
            _instructionRegistry.RegisterInstruction(name, callback);
        }

        /// <summary>
        /// Register a function with a name for Javascript to call.
        /// Function must accept 1 argument type string.
        /// </summary>
        /// <param name="name">Method name for Javascript to call.</param>
        /// <param name="callback">The function to call.</param>
        public void RegisterCallbackVoid(string name, Action<JSMarshallContainer> callback)
        {
            _instructionRegistry.RegisterInstructionVoid(name, callback);
        }

        /// <summary>
        /// Register an async function with a name for Javascript to call.
        /// Function must accept 1 argument type string and async return type string.
        /// </summary>
        /// <param name="name">Method name for Javascript to call.</param>
        /// <param name="callback">The async function to call.</param>
        public void RegisterCallbackAsync(string name, Func<JSMarshallContainer, Task<string>> callback)
        {
            _instructionRegistry.RegisterInstruction(name, callback);
        }

        /// <summary>
        /// Sets UITexture to renderTexture and changes browser size to match renderTexture size
        /// </summary>
        /// <param name="renderTexture"></param>
        public void Resize(Texture2D renderTexture)
        {
            UITexture = renderTexture;
            _browserSize = new Size(renderTexture.Width, renderTexture.Height);
            _browser.Size = _browserSize;
            _inputHandler = new WrapperInputHandler(_browser, Position);
        }

        /// <summary>
        /// Must be set is browser rendering position within the window is changed
        /// </summary>
        /// <param name="newPosition"></param>
        public void Move(XPoint newPosition)
        {
            Position = newPosition;
            _inputHandler = new WrapperInputHandler(_browser, Position);
        }

        public virtual void UpdateBitmap(GraphicsDevice device)
        {
            if (_browser?.Bitmap != null && _browser.Bitmap.Size.Width == UITexture.Width && _browser.Bitmap.Size.Height == UITexture.Height)
            {
                //UITexture.SetData(_browser.Bitmap);
                var ms = new MemoryStream();
                _browser.Bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                //UITexture = Texture2D.FromStream(device, ms);
                


                //UITexture.SetDataDirectBufferCopy(_browser.Bitmap);

                //_graphics.DrawImage(_browser.Bitmap, 0, 0);
            }
        }

        public virtual void Update(IGameTimeService gameTime)
        {
            
        }
    }
}
