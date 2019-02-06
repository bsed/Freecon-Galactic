using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Freecon.Client.Objects
{
    public struct Decal
    {
        public UInt32 ID;
        public float angle;
        public Color color;
        public bool isActive;
        public Vector2 location;
        public float scale;
        public Texture2D tex;
        public int transparency;
        public byte type;
    }
}