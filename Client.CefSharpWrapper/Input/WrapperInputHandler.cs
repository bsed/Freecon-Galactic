using System;
using CefSharp;
using CefSharp.OffScreen;
using Microsoft.Xna.Framework;

namespace Freecon.Client.CefSharpWrapper
{
    class WrapperInputHandler
    {
        int _deltaMultiplier = 50;

        //TODO: Remove circular reference
        public ChromiumWebBrowser Browser;

        IBrowserHost _browserHost { get { return Browser.GetBrowser().GetHost(); } }

        public Point Position;

        public WrapperInputHandler(ChromiumWebBrowser browser, Point position)
        {
            Browser = browser;
            Position = position;
        }

        public void FullKeyHandler(object sender, KeyEventType keyEventType, IntPtr wParam, IntPtr lParam)
        {
            if (!Browser.IsBrowserInitialized || _browserHost == null || Browser.IsLoading)
            {
                return;
            }

            KeyEvent ke = new KeyEvent {Type = keyEventType, WindowsKeyCode = (int)wParam, Modifiers = GetModifiers() };

            _browserHost.SendKeyEvent(ke);
        }
          
     
        public void MouseMoveHandler(object sender, MouseEventArgs e)
        {
            if (!Browser.IsBrowserInitialized || _browserHost == null || Browser.IsLoading)
            {
                return;
            }

            _browserHost.SendMouseMoveEvent(e.Location.X - Position.X, e.Location.Y - Position.Y, false, GetModifiers());
        }

        public void MouseDownHandler(object sender, MouseEventArgs e)
        {
            if (!Browser.IsBrowserInitialized || _browserHost == null || Browser.IsLoading)
            {
                return;
            }

            _browserHost.SendMouseClickEvent(e.Location.X - Position.X, e.Location.Y - Position.Y, e.Button, false, e.Clicks, GetModifiers());
        }

        public void MouseUpHandler(object sender, MouseEventArgs e)
        {
            if (!Browser.IsBrowserInitialized || _browserHost == null || Browser.IsLoading)
            {
                return;
            }

            _browserHost.SendMouseClickEvent(e.Location.X - Position.X, e.Location.Y - Position.Y, e.Button, true, e.Clicks, GetModifiers());
                        
        }

        public void MouseWheelHandler(object sender, MouseEventArgs e)
        {
            if (!Browser.IsBrowserInitialized || _browserHost == null || Browser.IsLoading)
            {
                return;
            }

            _browserHost.SendMouseWheelEvent(e.Location.X - Position.X, e.Location.Y - Position.Y, 0, e.Delta * _deltaMultiplier, GetModifiers());

        }
        CefEventFlags GetModifiers()
        {
            CefEventFlags modifiers = new CefEventFlags();

            modifiers |= InputSystem.ShiftDown ? CefEventFlags.ShiftDown : 0;
            modifiers |= InputSystem.CtrlDown ? CefEventFlags.ControlDown : 0;
            modifiers |= InputSystem.AltDown ? CefEventFlags.AltDown : 0;

            return modifiers;
        }
        
    }
}
