using System;
using System.Collections.Generic;

namespace Server.Models.Space
{
    public class GenerationStar
    {
        public int Angle;
        public int AreaID;
        public List<GenerationStar> Children;
        public List<int> ChildrenIDs;
        public List<int> Connections;
        public int ID;
        public int Length;
        public int ParentID;
        public int Weight;
        public int xLocation, yLocation;

        Random r = new Random(777546);

        public GenerationStar()
        {
            Children = new List<GenerationStar>();
            Connections = new List<int>();
            ChildrenIDs = new List<int>();
        }

        public void GenerateChildren(Random r)
        {
            int ChildrenNumber;
            GenerationStar childStar;
            if (ID == 0)
            {
                ChildrenNumber = r.Next((int) Math.Round(Weight/4f), (int) Math.Round(Weight/2f) + 1);
            }
            else
            {
                // If we're not Sol, generate normally.
                ChildrenNumber = r.Next(0, (int) Math.Round(Weight/2f) + 1);
                //Logger.log(Log_Type.INFO, "Layerdeep Children: " + ChildrenNumber);
            }
            for (int i = 0; i < ChildrenNumber; i++)
            {
                childStar = new GenerationStar();
                childStar.Weight = Weight - r.Next((int) Math.Round(Weight/2f), (int) Math.Round(Weight/2f) + 1);
                //Breaking this because I'm pretty sure it's useless anyway
                //childStar.ID = StarsystemGenerator.DatabaseOfIDs.Count;
                childStar.ID = r.Next();
                
                
                                
                //StarsystemGenerator.DatabaseOfIDs.Push(childStar.ID);
                //StarsystemGenerator.DatabaseOfStars.Add(childStar);
                
                
                
                childStar.ParentID = ID;
                ChildrenIDs.Add(childStar.ID);

                if (ID == 0)
                    childStar.Angle = r.Next(0, 360);
                else
                {
                    childStar.Angle = Angle + r.Next(-90, 90 + 1); // Relative angle to parent, ideally
                }

                childStar.Length = r.Next(r.Next(40, 80), r.Next(80, 120));

                // Calculate the position of the star, away from the parent at a given length.
                childStar.xLocation =
                    (int)
                    Math.Round(Math.Cos((childStar.Angle*0.0174532925) + (Math.PI/2f))*childStar.Length + xLocation);
                childStar.yLocation =
                    (int)
                    Math.Round(Math.Sin((childStar.Angle*0.0174532925) + (Math.PI/2f))*childStar.Length + yLocation);


                childStar.Connections.Add(ID);
                Children.Add(childStar);
                Connections.Add(childStar.ID);

                if (ID != 0) // Only generate one layer deep
                    return;

                for (int u = 0; u < Children.Count; u++)
                {
                    Children[u].GenerateChildren(r);
                }
            }
        }

    }
}