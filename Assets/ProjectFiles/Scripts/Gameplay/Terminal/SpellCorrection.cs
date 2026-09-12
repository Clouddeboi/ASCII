using System.Collections.Generic;
using System.Linq;

//Lightweight Levenshtein-distance based command name suggestion.
public static class SpellCorrection
{
    public static string FindClosestCommand(string input, IEnumerable<string> candidates, int maxDistance = 2)
    {
        if (string.IsNullOrEmpty(input) || candidates == null)
            return null;

        string best = null;
        int bestDistance = int.MaxValue;

        foreach (string candidate in candidates)
        {
            if (string.IsNullOrEmpty(candidate))
                continue;

            int distance = Distance(input, candidate);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = candidate;
            }
        }

        return bestDistance <= maxDistance ? best : null;
    }

    public static int Distance(string a, string b)
    {
        a = a.ToLowerInvariant();
        b = b.ToLowerInvariant();

        int[,] dp = new int[a.Length + 1, b.Length + 1];

        for (int i = 0; i <= a.Length; i++) dp[i, 0] = i;
        for (int j = 0; j <= b.Length; j++) dp[0, j] = j;

        for (int i = 1; i <= a.Length; i++)
        {
            for (int j = 1; j <= b.Length; j++)
            {
                int cost = a[i - 1] == b[j - 1] ? 0 : 1;
                dp[i, j] = new[]
                {
                    dp[i - 1, j] + 1,
                    dp[i, j - 1] + 1,
                    dp[i - 1, j - 1] + cost
                }.Min();
            }
        }

        return dp[a.Length, b.Length];
    }
}
