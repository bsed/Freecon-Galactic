using System.Collections.Generic;
using Core.Models.Enums;
using FakeItEasy;
using Freecon.Models.ChatCommands;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Server.Managers;
using Server.Managers.ChatCommands;
using Server.Models;

namespace Freecon.Server.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void AddHelpChat()
        {
            var chatManager = new ChatCommandHandler(new List<IChatCommand>()
            {
                new HelpCommand()
            }, new List<IAsyncChatCommand>());

            var parsed = chatManager.ParseChat(A.Fake<Player>(), "/help");

            Assert.IsTrue(parsed.CommandString == "help");

            var messages = parsed.GetChatMessages().Result;

            Assert.Equals(messages.Count, 1);
            Assert.Equals(messages[0], ChatText.Help);
        }

        [TestMethod]
        public void AddFakeTest()
        {
            var chatManager = new ChatCommandHandler(new List<IChatCommand>()
            {
                new TestCommand()
            }, new List<IAsyncChatCommand>());

            var parsed = chatManager.ParseChat(A.Fake<Player>(), "/test hello");

            Assert.IsTrue(parsed.CommandString == "test");
            Assert.IsTrue(parsed.Arguments == "hello");
            
            var messages = parsed.GetChatMessages().Result;

            Assert.Equals(messages.Count, 1);
        }

        //[TestMethod]
        //public void AddInvalidChat()
        //{
        //    var chatManager = new NewChatManager();

        //    chatManager.AddChat("/asdf");

        //    Assert.IsTrue(chatManager.Chats.Count == 1);

        //    var chat = chatManager.Chats[0];

        //    Assert.AreEqual(chat.ChatFragments[0].Text, ChatText.InvalidChat);
        //}
    }

    public class TestCommand : IChatCommand
    {
        protected List<string> _commandSignatures = new List<string>()
        {
            "test"
        };

        public List<string> CommandSignatures { get { return _commandSignatures; } }

        public List<OutboundChatMessage> ParseChatline(ChatMetaData meta)
        {
            return new List<OutboundChatMessage>()
            {
                new OutboundChatMessage(null, new ChatlineObject(meta.Arguments, ChatlineColor.Blue))
            };
        }
    }
}
