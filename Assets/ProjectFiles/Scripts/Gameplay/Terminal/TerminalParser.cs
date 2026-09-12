using System.Collections.Generic;

//Parses raw terminal input into a command name and argument tokens.
public static class TerminalParser
{
    public struct ParsedCommand
    {
        public string CommandName;
        public string[] Args;
        public string RawArgs;
    }

    public static ParsedCommand Parse(string input)
    {
        ParsedCommand result = new ParsedCommand { CommandName = "", Args = new string[0], RawArgs = "" };

        if (string.IsNullOrWhiteSpace(input))
            return result;

        string normalized = System.Text.RegularExpressions.Regex.Replace(input.Trim(), @"\s+", " ");
        int firstSpace = normalized.IndexOf(' ');

        string commandName = firstSpace < 0 ? normalized : normalized.Substring(0, firstSpace);
        string rawArgs = firstSpace < 0 ? "" : normalized.Substring(firstSpace + 1).Trim();

        result.CommandName = commandName.ToLowerInvariant();
        result.RawArgs = rawArgs;
        result.Args = string.IsNullOrEmpty(rawArgs) ? new string[0] : SplitArgs(rawArgs).ToArray();

        return result;
    }

    private static List<string> SplitArgs(string text)
    {
        List<string> tokens = new List<string>();
        bool inQuotes = false;
        System.Text.StringBuilder current = new System.Text.StringBuilder();

        foreach (char c in text)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
                continue;
            }

            if (c == ' ' && !inQuotes)
            {
                if (current.Length > 0)
                {
                    tokens.Add(current.ToString());
                    current.Clear();
                }
                continue;
            }

            current.Append(c);
        }

        if (current.Length > 0)
            tokens.Add(current.ToString());

        return tokens;
    }
}
