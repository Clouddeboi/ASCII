using System.Collections.Generic;
using System.Linq;

//Case-insensitive registry of terminal commands, keyed by name and aliases.
public class TerminalCommandRegistry
{
    private readonly Dictionary<string, ITerminalCommand> commandsByToken = new Dictionary<string, ITerminalCommand>();
    private readonly List<ITerminalCommand> allCommands = new List<ITerminalCommand>();

    public IReadOnlyList<ITerminalCommand> AllCommands => allCommands;

    public void Register(ITerminalCommand command)
    {
        if (command == null) return;

        allCommands.Add(command);
        commandsByToken[command.Name.ToLowerInvariant()] = command;

        if (command.Aliases != null)
        {
            foreach (string alias in command.Aliases)
            {
                if (!string.IsNullOrEmpty(alias))
                    commandsByToken[alias.ToLowerInvariant()] = command;
            }
        }
    }

    public bool TryGet(string nameOrAlias, out ITerminalCommand command)
    {
        return commandsByToken.TryGetValue(nameOrAlias.ToLowerInvariant(), out command);
    }

    public IEnumerable<string> AllTokens => commandsByToken.Keys;

    public IEnumerable<ITerminalCommand> GetUnlocked(TerminalContext context)
    {
        return allCommands.Where(c => c.IsUnlocked(context));
    }
}
