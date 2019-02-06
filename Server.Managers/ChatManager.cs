using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Core.Models;
using Core.Models.Enums;
using Freecon.Core.Networking.Models;
using Freecon.Core.Networking.Models.Messages;
using Freecon.Core.Networking.ServerToServer;
using Freecon.Models.ChatCommands;
using Freecon.Models.TypeEnums;
using Lidgren.Network;
using RedisWrapper;
using Server.Managers.ChatCommands;
using Server.Models;
using Freecon.Core.Utils;

using Server.Models.Extensions;

namespace Server.Managers
{
    public class ChatCommandHandler
    {

        public readonly Regex StubCommandRegex = new Regex(@"^/(\S*)$");
        public readonly Regex CommandSplitRegex = new Regex(@"^/(\S*) (.*)$");

        public Dictionary<string, Func<ChatMetaData, List<OutboundChatMessage>>> Commands;
        public Dictionary<string, Func<ChatMetaData, Task<List<OutboundChatMessage>>>> CommandsAsync;

        protected InvalidCommand _invalidCommand;

        public ChatCommandHandler(List<IChatCommand> commandHandlers, List<IAsyncChatCommand> asyncCommandHandlers)
        {
            Commands = new Dictionary<string, Func<ChatMetaData, List<OutboundChatMessage>>>();
            CommandsAsync = new Dictionary<string, Func<ChatMetaData, Task<List<OutboundChatMessage>>>>();

            _invalidCommand = new InvalidCommand();

            // Add chat commands from list.
            commandHandlers.ForEach(command =>
            {
                command.CommandSignatures.ForEach(alias =>
                {
                    // Map all aliases to our callback
                    Commands[alias] = command.ParseChatline;
                });
            });

            asyncCommandHandlers.ForEach(command =>
            {
                command.CommandSignatures.ForEach(alias =>
                {
                    CommandsAsync[alias] = command.ParseChatline;
                });
            });
        }

        public ChatMetaData ParseChat(Player player, string input)
        {
            var single = StubCommandRegex.Match(input);

            if (single.Groups.Count == 2)
            {
                return CreateStubChat(player, input, single.Groups[1].Value);
            }

            var split = CommandSplitRegex.Match(input);

            if (split.Groups.Count == 3)
            {
                return CreateCommandChat(player, input, split.Groups[1].Value, split.Groups[2].Value);
            }

            return CreateInvalidChatMessage(player, input);
        }

        public ChatMetaData CreateStubChat(Player player, string raw, string command)
        {
            // Todo: Add Command specific help or something.
            if (!Commands.ContainsKey(command) && !CommandsAsync.ContainsKey(command))
            {
                return CreateInvalidChatMessage(player, raw);
            }

            return CreateChatMetaData(player, raw, command, Commands[command], null);
        }

        public ChatMetaData CreateCommandChat(Player player, string raw, string command, string arguments)
        {
            // Todo: Add Command specific help or something.
            if (Commands.ContainsKey(command))
            {
                return CreateChatMetaData(player, raw, command, Commands[command], arguments);
            }

            if (CommandsAsync.ContainsKey(command))
            {
                return CreateChatMetaDataAsync(player, raw, command, CommandsAsync[command], arguments);
            }

            return CreateInvalidChatMessage(player, raw);
        }

        public ChatMetaData CreateInvalidChatMessage(Player player, string raw)
        {
            return CreateChatMetaData(player, raw, raw, _invalidCommand.ParseChatline, null);
        }

        public ChatMetaData CreateChatMetaData(
            Player player,
            string input,
            string commandString,
            Func<ChatMetaData, List<OutboundChatMessage>> command,
            string args)
        {
            return new ChatMetaData(player, input, commandString, command, args);
        }

        public ChatMetaData CreateChatMetaDataAsync(
            Player player,
            string input,
            string commandString,
            Func<ChatMetaData, Task<List<OutboundChatMessage>>> command,
            string args)
        {
            return new ChatMetaData(player, input, commandString, command, args);
        }

        public async Task<List<OutboundChatMessage>> FetchOutboundChats(Player player, string input)
        {
            var chatMeta = ParseChat(player, input);

            return await chatMeta.GetChatMessages();
        }
    }

    public class ChatManager
    {
        private readonly PlayerManager _playerManager;
        private ChatCommandHandler _chatCommandHandler;

        public ChatManager(
            List<IChatCommand> commandHandlers,
            List<IAsyncChatCommand> asyncCommandHandlers,
            PlayerManager playerManager,
            MessageManager messageManager,
            RedisServer redisServer)
        {
            _playerManager = playerManager;
            _chatCommandHandler = new ChatCommandHandler(commandHandlers, asyncCommandHandlers);

            redisServer.Subscribe(MessageTypes.Redis_Shout, _handleShout);
        }

        /// <summary>
        ///     The message recieved from the client will have the /t, /s, etc removed
        ///     Function handles chat message according to chat type which is sent as a byte by the client
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="server"></param>
        /// <param name="messageConnection"></param>
        public async void HandleMessage(NetworkMessageContainer message, Player player)
        {
            if (message == null)
            {
                ConsoleManager.WriteLine("Invalid chat message handled. Message is null.",
                    ConsoleMessageType.Error);
            }

            var messageData = message.MessageData as MessageChatMessage;
            
            var chatMessageData = messageData.ChatMessageData;

            if (player == null)
            {
                ConsoleManager.WriteLine("Player connection not found while handling chat message.",
                    ConsoleMessageType.Warning);
                return;
            }

            var outboundChats = await _chatCommandHandler.FetchOutboundChats(player, chatMessageData.ChatJson);
            
            if (outboundChats == null || outboundChats.Count == 0)
            {
                return;
            }

            SendChatList(outboundChats);
        }

        private void _handleShout(object sender, NetworkMessageContainer container)
        {
            var rs = container.MessageData as MessageRedisShout;

            SendChatToPlayers(_playerManager.GetOnlinePlayers(), rs.Message);
        }

        /// <summary>
        ///     Handles a Command sent by a player with isAdmin = true
        ///
        /// <summary>
        ///     Handles a Command sent by a player with isAdmin = true
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="server"></param>
        /// <param name="messageConnection"></param>
        public void AdminCommand(NetIncomingMessage msg, NetPeer server, NetConnection messageConnection)
        {
            ConsoleManager.WriteLine("Received Admin Command; Admin commands are disabled.", ConsoleMessageType.Warning);
            return;
            /*
            var player = _playerManager.connectionToPlayer[msg.SenderConnection]; // You can't spoof an admin

            if (!player.GetAccount().IsAdmin) // If you're not an admin, fuck you
                return;

            var chatType = msg.ReadByte();
            var chatText = msg.ReadString();
            //ConsoleManager.WriteToFreeLine(playerID.ToString() + chatType.ToString() + chatText.ToString());

            //Player tempPlayer = new Player();

            ConsoleManager.WriteLine("Admin Command " + chatType + " received.");
            switch ((AdminCommands) chatType) //Chat message type
            {
                case AdminCommands.killPlayer: // Not implemented
                    _messageManager.SendChatMessage("Killed player Command parsed.", "", ChatTypes.admin, server,
                        player.Connection);
                    break;

                case AdminCommands.makeNPCs:
                    var numToMake = int.Parse(chatText);
                    //SR_connectionManager.Server.Program.createNPCs = numToMake;
                    //SR_connectionManager.Server.Program.NPCCreatePlayer = player;
                    ConsoleManager.WriteLine("makeNPCs Command is broken.");
                    break;

                case AdminCommands.setShip:

                    switch (chatText.ToLower())
                    {
                        case "penguin":
                            AdminShipChange(player, ShipStatManager.TypeToStats[ShipTypes.Penguin]);

                            _messageManager.SendChatMessage("Set IShip to a Penguin.", "", ChatTypes.admin, server,
                                player.Connection);
                            Logger.log(Log_Type.INFO, player.Username + " switched IShip to Penguin");
                            break;
                        case "bc":
                        case "battlecruiser":
                            AdminShipChange(player, ShipStatManager.TypeToStats[ShipTypes.BattleCruiser]);

                            _messageManager.SendChatMessage("Set IShip to a Battlecruiser. Weapon ", "", ChatTypes.admin,
                                server, player.Connection);
                            Logger.log(Log_Type.INFO, player.Username + " switched IShip to Battlecruiser");
                            break;

                        case "reaper":
                        case "jeyth":
                            AdminShipChange(player, ShipStatManager.TypeToStats[ShipTypes.Reaper]);

                            _messageManager.SendChatMessage("Set IShip to a Jeyth. Weapon ", "", ChatTypes.admin, server,
                                player.Connection);
                            Logger.log(Log_Type.INFO, player.Username + " switched IShip to Jeyth");
                            break;
                        case "zy":
                        case "barge":
                            AdminShipChange(player, ShipStatManager.TypeToStats[ShipTypes.Barge]);

                            _messageManager.SendChatMessage("Set IShip to a ZY Barge. Weapon ", "", ChatTypes.admin,
                                server, player.Connection);
                            Logger.log(Log_Type.INFO, player.Username + " switched IShip to ZY Barge");
                            break;

                        case "dread":
                            AdminShipChange(player, ShipStatManager.TypeToStats[ShipTypes.Dread]);

                            _messageManager.SendChatMessage("Set IShip to a Dread class fighter. ", "",
                                ChatTypes.admin, server, player.Connection);
                            Logger.log(Log_Type.INFO, player.Username + " switched IShip to a Dread class fighter.");
                            break;

                        default:

                            _messageManager.SendChatMessage("Invalid IShip Type: ", chatText, ChatTypes.admin,
                                server, player.Connection);
                            Logger.log(Log_Type.INFO,
                                player.Username + " specified invalid IShip type " + chatText + ".");
                            break;
                    }
                    break;
                case AdminCommands.systemStatistics:
                {
                    var p = _playerManager.GetPlayer(messageConnection);
                    if (p.CurrentAreaId == null)
                        _messageManager.SendChatMessage("Player's area is currently null.", "", ChatTypes.admin, server,
                            messageConnection);


                    var a = _galaxyManager.GetArea(p.CurrentAreaId);
                    if (a == null)
                        //a = _galaxyManager.GetNonlocalArea((int)p.CurrentAreaId, _playerManager);//This might be a bad idea, since it registers non-local ships and players, but this is an admin Command, after all
                        return;

                    if (a.AreaType == (byte) AreaTypes.System)
                    {
                        var pp = (PSystem) a;
                        _messageManager.SendChatMessage(
                            "Current System: " + pp.SystemName + ", Is Cluster: " + pp.IsCluster + ", Total Planets: " +
                            pp.PlanetCount + ", Total Moons: " + pp.MoonCount
                            , "", ChatTypes.admin, server, messageConnection);
                        //Logger.log(Log_Type.INFO, player.username + " switched IShip to Penguin"); lol wtf?
                    }
                    break;
                }
                case AdminCommands.whoplayer:
                    break;
            }
             */
        }

        private void AdminShipChange(Player player, ShipStats shs)
        {
            ConsoleManager.WriteLine("Warning: AdminShipChange is currently disabled.", ConsoleMessageType.Warning);
        }


        public void SendRadioToConnections(List<NetConnection> connections, string message)
        {
        }

        public ChatlineObject BuildChatLine(string prefix, string text, ChatlineColor prefixColor, ChatlineColor textColor = ChatlineColor.White)
        {
            return new ChatlineObject(new List<ChatlineFragment>()
            {
                new ChatlineFragment(prefix, prefixColor),
                new ChatlineFragment(text, textColor)
            });
        }

        public void SendChatList(List<OutboundChatMessage> outboundChats)
        {
            outboundChats.ForEach(SendChatToPlayer);
        }

        public void SendChatToPlayer(OutboundChatMessage chat)
        {
            if (!EnsurePlayerIsOnline(chat.DestinationPlayer))
            {
                // We should log here that the message failed.
                return;
            }

            SendChatToPlayer(chat.DestinationPlayer, chat.ChatMessage.ToClientJson(), SerializeChatMetadata(chat.Metadata));
        }

        public void SendChatToPlayers(List<Player> players, string chatJson, string meta)
        {
            foreach (var player in players)
            {
                SendChatToPlayer(player, chatJson, meta);
            }
        }

        public void SendChatToPlayers(List<Player> players, string chatText)
        {
            foreach (var player in players)
            {
                SendChatToPlayer(player, chatText, null);
            }
        }

        void SendChatToPlayer(Player player, string chatJson, string meta)
        {
            var message = new NetworkMessageContainer();

            message.MessageData = CreateChatMessageContainer(chatJson, meta);
            message.MessageType = MessageTypes.ChatMessage;

            player.SendMessage(message);
        }

        MessageChatMessage CreateChatMessageContainer(string chatJson, string metaJson)
        {
            var data = new MessageChatMessage();

            data.ChatMessageData = new ChatMessageData(chatJson, metaJson);

            return data;
        }

        bool EnsurePlayerIsOnline(Player player)
        {
            // Todo: Ensure this is good enough.
            return player.IsOnline;
        }

        string SerializeChatMetadata(Dictionary<string, string> meta)
        {
            if (meta == null)
            {
                return null;
            }

            return LowercaseContractResolver.SerializeObject(meta);
        }

        //public NetOutgoingMessage SerializeChatMessage(string chatJson)
        //{
        //    return SerializeChatMessage(chatJson, null);
        //}

        //public NetOutgoingMessage SerializeChatMessage(string chatJson, string metaJson)
        //{
        //    var data = CreateChatMessageContainer(chatJson, metaJson);

        //    var msg = _connectionManager.Server.CreateMessage();

        //    data.WriteToLidgrenMessage_ToClient(MessageTypes.ChatMessage, msg);

        //    return msg;
        //}

        /// <summary>
        /// Builds and a simple chat text and colors the prefix according to chatType
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="text"></param>
        /// <param name="chatType"></param>
        public void SendSimpleChat(Player p, string prefix, string text, ChatTypes chatType, ChatlineColor textColor=ChatlineColor.White)
        {
            var chat = new OutboundChatMessage(p, BuildChatLine(prefix, text, ChatHelpers.GetColor(chatType), textColor));
            SendChatToPlayer(chat);
        }

        /// <summary>
        /// Builds a simple chat text with the specified colors
        /// </summary>
        /// <param name="p"></param>
        /// <param name="perfix"></param>
        /// <param name="text"></param>
        /// <param name="chatlineColor"></param>
        public void SendSimpleChat(Player p, string prefix, string text, ChatlineColor prefixColor, ChatlineColor textColor=ChatlineColor.White)
        {
            var chat = new OutboundChatMessage(p, BuildChatLine(prefix, text, prefixColor, textColor));
            SendChatToPlayer(chat);
        }

        /// <summary>
        /// Sends a simple chat message to all online players in the area except for the player with the corresponding playerIdToIgnore
        /// </summary>
        /// <param name="area"></param>
        /// <param name="prefix"></param>
        /// <param name="text"></param>
        /// <param name="chatType"></param>
        /// <param name="playerIdToIgnore"></param>
        public void BroadcastSimpleChat(IArea area, string prefix, string text, ChatTypes chatType, int? playerIdToIgnore=null)
        {
            var outboundList = area.BuildReplyAllChatList(playerIdToIgnore, BuildChatLine(prefix, text, ChatHelpers.GetColor(ChatTypes.Alert)));
            SendChatList(outboundList);
        }

        /// <summary>
        /// Sends a chat alert to all players in the area except for the player with the specified playerIdToIgnore
        /// </summary>
        /// <param name="area"></param>
        /// <param name="playerIdToIgnore"></param>
        /// <param name="text"></param>
        public void BroadcastChatAlert(IArea area, int? playerIdToIgnore, string text)
        {
            BroadcastSimpleChat(area, "[ALERT] ", text, ChatTypes.Alert, playerIdToIgnore);
        }
    }
}
