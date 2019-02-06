using System;

using Lidgren.Network;
using Microsoft.Xna.Framework.Input;
using SRClient.Managers;
using SRClient.Managers.Networking;
using SRClient.Managers.States;
using Freecon.Models.TypeEnums;
using Freecon.Core.Networking.Models;


namespace SRClient.GUI
{
    public partial class HudElementManager
    {
        private void buyButton_OnClickEvent(BaseUI sender)
        {
            // My easy way to prevent spamming purchases. Implement a cooldown or something
            // A confirmation screen would be ideal
            if (MouseManager.LeftButtonPressed)
                return;

            var button = (Button)sender;

            // This is where you get to have your heyday Ilaan
            // ^^^Does anybody remember wtf this means?
            var g = (ShipGood)button.data;
            
            NetOutgoingMessage msg = _clientManager.Client.CreateMessage(16);
            msg.Write((byte) MessageTypes.PurchaseRequest);
            msg.Write((byte) PurchaseRequestType.Ship);
            msg.Write((byte) g.ID);
            _clientManager.Client.SendMessage(msg, _clientManager.CurrentSlaveConnection, NetDeliveryMethod.ReliableOrdered);
        }
    }
}