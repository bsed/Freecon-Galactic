using System;
using System.Collections.Generic;
using Freecon.Client.Objects;

namespace Freecon.Client.Managers
{
    public class GalaxyManager
    {
        public static List<GenerationStar> starList;
        public static Dictionary<UInt32, GenerationStar> IDtoStar;

        public GalaxyManager()
        {
            starList = new List<GenerationStar>();
            IDtoStar = new Dictionary<UInt32, GenerationStar>();
            //GenerationStar gs = new GenerationStar();
            //gs.xLocation = 10;
            //gs.yLocation = 10;
            //gs.Pos = new Microsoft.Xna.Framework.Vector2(10, 10);
            //gs.Length = 0;
            //gs.ID = 0;
            //gs.Angle = 0;
            //gs.Children = new List<GenerationStar>();
            //GenerationStar gs2 = new GenerationStar();
            //gs2.Pos = new Microsoft.Xna.Framework.Vector2(140, 50);
            //gs2.Length = 100;
            //gs2.ID = 0;
            //gs2.Angle = 30;
            //gs2.Children = new List<GenerationStar>();
            //GenerationStar gs3 = new GenerationStar();
            //gs3.Pos = new Microsoft.Xna.Framework.Vector2(-50, 80);
            //gs3.Length = 120;
            //gs3.ID = 0;
            //gs3.Angle = 190;
            //gs3.Children = new List<GenerationStar>();
            //GenerationStar gs4 = new GenerationStar();
            //gs4.Pos = new Microsoft.Xna.Framework.Vector2(50, -100);
            //gs4.Length = 80;
            //gs4.ID = 0;
            //gs4.Angle = 250;
            //gs4.Children = new List<GenerationStar>();

            //gs.Children.Add(gs2);
            //gs.Children.Add(gs3);
            //gs.Children.Add(gs4);
            //starList.Add(gs);
            //starList.Add(gs2);
            //starList.Add(gs3);
            //starList.Add(gs4);
        }

        public void AddStar(GenerationStar star)
        {
            //starList.Add(star);
            //star.AddIDToDictionary(IDtoStar);
        }


    }
}