namespace LerpVisualTest
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (LerpVisualTest game = new LerpVisualTest())
            {
                game.Run();
            }
        }
    }
#endif
}

