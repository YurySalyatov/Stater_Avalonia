using System;
using System.Collections.Generic;

namespace Stater.Utils;

public static class FindUtils
{
    public static List<List<int>> FindAllSubstings(string text, string substring)
    {
        List<List<int>> result = [];
        var pos = text.IndexOf(substring, StringComparison.Ordinal);
        while (pos != -1)
        {
            result.Add(new() {pos, pos + substring.Length});
            pos += substring.Length;
            pos = text.IndexOf(substring, pos, StringComparison.Ordinal);
        }
        return result;
    }
}