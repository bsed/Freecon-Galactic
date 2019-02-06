using System;
using System.Collections.Generic;
using Lidgren.Network;
using Microsoft.Xna.Framework;
namespace Freecon.Client.Objects
{
    public class GenerationStar
    {
        public int Angle;
        public List<GenerationStar> Children;
        public List<int> Connections;
        public int ID;
        public int Length;
        public Vector2 Pos;
        public int Weight;
        public int xLocation, yLocation;

        public GenerationStar()
        {
            Children = new List<GenerationStar>();
            Connections = new List<int>();
        }

        //What the fuck is this?
        public GenerationStar ReadStarFromMessage(ref NetIncomingMessage msg)
        {
            Weight = msg.ReadInt32();
            Angle = msg.ReadInt32();
            ID = msg.ReadInt32();
            //Console.WriteLine("Reading Star: " + ID);
            Length = msg.ReadInt32();
            xLocation = msg.ReadInt32();
            yLocation = msg.ReadInt32();
            Pos = new Vector2(xLocation, yLocation);

            int Count = msg.ReadInt32();
            if (Count > 0)
                for (int i = 0; i < Count; i++)
                {
                    var gs = new GenerationStar();
                    Children.Add(gs.ReadStarFromMessage(ref msg));
                }

            Count = msg.ReadInt32();
            if (Count > 0)
                for (int i = 0; i < Count; i++)
                {
                    Connections.Add(msg.ReadInt32());
                }

            return this;
        }

        public void AddIDToDictionary(Dictionary<int, GenerationStar> DictionaryToAdd)
        {
            DictionaryToAdd.Add(ID, this);
        }

        public void AddStarToList(Dictionary<UInt32, GenerationStar> DictionaryToAdd, List<GenerationStar> StarList)
        {
            //AddIDToDictionary(DictionaryToAdd);
            //StarList.Add(this);
            //if (Children.Count > 0)
            //    for (int i = 0; i < Children.Count; i++)
            //    {
            //        Children[i].AddStarToList(DictionaryToAdd, StarList);
            //    }
        }

        public void IncrementStarLocation(Vector2 MoveAmount)
        {
            Pos += MoveAmount;
        }
    }
}