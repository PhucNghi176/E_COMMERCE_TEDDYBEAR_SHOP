using CONTRACT.CONTRACT.CONTRACT.Abstractions.Messages;

namespace COMMAND.CONTRACT.Services.Tags;
public static class Commands
{
    public record CreateTagCommand(string Name) : ICommand;
}