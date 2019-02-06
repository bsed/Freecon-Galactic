using Core.Models.Enums;
using FarseerPhysics.Dynamics;
using Freecon.Client.Core.Objects;
using Microsoft.Xna.Framework;
using Core.Interfaces;
using Microsoft.Xna.Framework.Graphics;
using Freecon.Client.Interfaces;
using Freecon.Client.Managers;
using Freecon.Client.Managers.Networking;
using Freecon.Client.Mathematics;
using System;
using System.Collections.Generic;
using Freecon.Core.Networking.Models.Objects;
using Freecon.Core.Networking.Models.Messages;

namespace Freecon.Client.Core.Managers
{
    /// <summary>
    /// Given the simplicity of this class, it might be excessive, but it keeps other classes from being polluted.
    /// </summary>
    public class FloatyAreaObjectManager:ISynchronousManager
    {
        Dictionary<int, FloatyAreaObject> _objects = new Dictionary<int, FloatyAreaObject>();

        World _world;
        TextureManager _textureManager;
        SpriteBatch _spriteBatch;
        MessageService_ToServer _messageService;
        private ParticleManager _particleManager;

        public FloatyAreaObjectManager(World world, TextureManager textureManager, MessageService_ToServer messageService, SpriteBatch spriteBatch, ParticleManager particleManager)
        {            
            _world = world;
            _textureManager = textureManager;
            _spriteBatch = spriteBatch;
            _messageService = messageService;
            _particleManager = particleManager;
        }


        public void Update(IGameTimeService gameTime)
        {
            foreach (var kvp in _objects)
            {
                kvp.Value.Update(gameTime);
            }

        }

        public void Draw(SpriteBatch spriteBatch, Camera2D camera)
        {
            foreach (var kvp in _objects)
            {
                kvp.Value.Draw(spriteBatch);
            }
        }

        //Temporary
        public void Draw(Camera2D camera)
        {
           
        }

        /// <summary>
        /// Reads and creates floatyAreaObjects
        /// </summary>
        /// <param name="msg"></param>
        public void InstantiateFloatyAreaObjects(List<FloatyAreaObjectData> objects)
        {

            foreach(var f in objects)
            {

                FloatyAreaObject newobj;

                switch (f.FloatyType)
                {
                    default:
                        newobj = new FloatyAreaObject(f.Id, _world, _messageService, f.FloatyType,
                            GetTexture(f.FloatyType), new Vector2(f.XPos, f.YPos), f.Rotation);
                        break;
                }
                _objects.Add(newobj.Id, newobj);
            }
            

        }

        Texture2D GetTexture(FloatyAreaObjectTypes floatyType, bool blinkTex = false)
        {
            if (_textureManager == null)
                return null;

            switch (floatyType)
            {
                case FloatyAreaObjectTypes.Module:
                    return _textureManager.FloatingModule;
                   
                default:
                    throw new NotImplementedException(floatyType + " not implemented in" + this.ToString() + ".GetTexture");

            }

        }
        
        public void InstantiateFloatyAreaObjects(MessageReceiveFloatyAreaObjects data)
        {            
            List<FloatyAreaObjectData> floatyObjects = new List<FloatyAreaObjectData>();

            foreach(var o in data.FloatyObjects)
            {
                floatyObjects.Add((FloatyAreaObjectData)o);
            }

            InstantiateFloatyAreaObjects(floatyObjects);
        }

        public void RemoveFloatyAreaObject(int id)
        {
            if (_objects.ContainsKey(id))
            {
                _objects[id].Dispose();
                _objects.Remove(id);
            }
        }

        public void RemoveFloatyAreaObjects(MessageRemoveKillRevive data)
        {            
            foreach(var id in data.ObjectIDs)
            {
                RemoveFloatyAreaObject(id);
            }
        }
        public void Clear()
        {
            foreach(var ob in _objects)
            {
                ob.Value.Dispose();
            }

            _objects.Clear();
        }

    }
}
