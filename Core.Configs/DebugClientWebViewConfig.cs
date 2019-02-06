namespace Freecon.Core.Configs
{
    public class DebugClientWebViewConfig : IClientWebViewConfig
    {
        public WebViewSetting GlobalGameInterface
        {
            get { return new WebViewSetting("http://freecon-dev.spacerambles.com:3080/game-interface", 0, 0); }
            //get { return new WebViewSetting("http://localhost:3080/game-interface", 0, 0); }
        }
    }
}
