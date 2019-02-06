namespace Freecon.Core.Configs
{
    public class WebViewSetting
    {
        public string Url { get; private set; }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public WebViewSetting(string url, int width, int height)
        {
            Url = url;

            Width = width;

            Height = height;
        }
    }
}
