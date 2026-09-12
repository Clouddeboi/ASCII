using System.Collections.Generic;

//Stores the last 10 executed commands and supports up/down navigation.
public class TerminalHistory
{
    private const int MaxHistory = 10;

    private readonly List<string> history = new List<string>();
    private int navigationIndex = -1;

    public IReadOnlyList<string> Entries => history;

    public void Add(string command)
    {
        if (string.IsNullOrWhiteSpace(command))
            return;

        //Avoid consecutive duplicates while still allowing the same command later on.
        if (history.Count > 0 && history[history.Count - 1] == command)
        {
            ResetNavigation();
            return;
        }

        history.Add(command);
        if (history.Count > MaxHistory)
            history.RemoveAt(0);

        ResetNavigation();
    }

    public void ResetNavigation()
    {
        navigationIndex = history.Count;
    }

    public string NavigatePrevious()
    {
        if (history.Count == 0)
            return null;

        navigationIndex = Mathf_Clamp(navigationIndex - 1, 0, history.Count - 1);
        return history[navigationIndex];
    }

    public string NavigateNext()
    {
        if (history.Count == 0)
            return null;

        navigationIndex++;
        if (navigationIndex >= history.Count)
        {
            navigationIndex = history.Count;
            return "";
        }

        return history[navigationIndex];
    }

    private static int Mathf_Clamp(int value, int min, int max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }
}
