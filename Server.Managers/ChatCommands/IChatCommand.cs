using System.Collections.Generic;
using System.Threading.Tasks;

namespace Server.Managers.ChatCommands
{
    public interface IChatCommand
    {
        List<string> CommandSignatures { get; }

        List<OutboundChatMessage> ParseChatline(ChatMetaData meta);
    }

    public interface IAsyncChatCommand
    {
        List<string> CommandSignatures { get; }

        Task<List<OutboundChatMessage>> ParseChatline(ChatMetaData meta);
    }
}
