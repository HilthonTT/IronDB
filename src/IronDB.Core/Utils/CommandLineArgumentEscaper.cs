using System.Text;

namespace IronDB.Core.Utils;

// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// This file has been modified from the original form. See Notice.txt in the project root for more information.

/// <summary>
/// A utility for escaping arguments for new processes.
/// </summary>
internal static class CommandLineArgumentEscaper
{
    public static string EscapeAndConcatenate(IEnumerable<string> args)
    {
        return string.Join(" ", args.Select(EscapeSingleArg));
    }

    public static string EscapeSingleArg(string argument)
    {
        var sb = new StringBuilder();

        var needsQuotes = ContainsWhitespace(argument);
        var isQuoted = needsQuotes || IsSurroundedWithQuotes(argument);

        if (needsQuotes)
        {
            sb.Append('"');
        }

        for (int i = 0; i < argument.Length; i++)
        {
            int backslashes = 0;

            while (i < argument.Length && argument[i] == '\\')
            {
                backslashes++;
                i++;
            }

            if (i == argument.Length && isQuoted)
            {
                // Escape any backslashes at the end of the arg when the argument is also quoted.
                // This ensures the outside quote is interpreted as an argument delimiter
                sb.Append('\\', 2 * backslashes);
            }
            else if (i == argument.Length)
            {
                // At then end of the arg, which isn't quoted,
                // just add the backslashes, no need to escape
                sb.Append('\\', backslashes);
            }
            else if (argument[i] == '"')
            {
                // Escape any preceding backslashes and the quote
                sb.Append('\\', (2 * backslashes) + 1);
                sb.Append('"');
            }
            else
            {
                // Output any consumed backslashes and the character
                sb.Append('\\', backslashes);
                sb.Append(argument[i]);
            }
        }

        if (needsQuotes)
        {
            sb.Append('"');
        }

        return sb.ToString();
    }

    private static bool IsSurroundedWithQuotes(string argument)
    {
        if (string.IsNullOrWhiteSpace(argument))
        {
            return false;
        }

        return argument[0] == '"' && argument[^1] == '"';
    }

    private static bool ContainsWhitespace(string argument)
    {
        return argument.IndexOfAny([' ', '\t', '\n']) >= 0;
    }
}
