using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;
using Freecon.Models.TypeEnums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Freecon.Client.Managers;
using System;
using System.Collections.Generic;

namespace Freecon.Client.Objects.Structures
{
    public class CommandCenter: Structure
    {
        Action<int> _sendDomeEntryRequest;


        Func<bool> _isEnterModeOn;

        public CommandCenter(SpriteBatch spriteBatch, Action<int> sendDomeEntryRequest, Func<bool> isEnterModeOn, World w, Texture2D texture, Vector2 position, StructureTypes structureType, float health, int ID, HashSet<int> teams)
            : base(spriteBatch, texture, position.X, position.Y, structureType, health, ID, teams)
        {
            #region _body initialization
            Debugging.AddStack.Push(this.ToString());
            _body = BodyFactory.CreateCircle(w, .5f, 1, position);
            _body.IsStatic = true;
            _body.UserData = new StructureBodyDataObject(BodyTypes.CommandCenter, this);

            _body.OnCollision += Body_OnCollision;

            #endregion

            _sendDomeEntryRequest = sendDomeEntryRequest;
            _isEnterModeOn = isEnterModeOn;
        }

        bool Body_OnCollision(Fixture fixtureA, Fixture fixtureB, Contact contact)
        {
            if(fixtureB.Body.UserData is ShipBodyDataObject)
            {
                Ship s = ((ShipBodyDataObject)fixtureB.UserData).Ship;
                if (s.CanLandWarp && s.GetCurrentEnergy() == s.MaxEnergy && _isEnterModeOn())
                    _sendDomeEntryRequest(s.Id);

            }

            return true;
        }


     
    }
}
