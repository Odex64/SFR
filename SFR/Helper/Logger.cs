using System;

namespace SFR.Helper;

/// <summary>
/// Use this class to print messages to the console.
/// Debug messages are available only when running a debug build.
/// </summary>
internal static class Logger
{
    private static string _lastPrintedLine = string.Empty;
    private static uint _identicalLines = 1;

    private static void Print<T>(T message, bool timestamp)
    {
        string text = message.ToString();
        if (text == _lastPrintedLine)
        {
            _identicalLines++;
            Console.SetCursorPosition(0, Console.CursorTop - 1);
        }
        else
        {
            _lastPrintedLine = text;
            if (_identicalLines > 1)
            {
                _identicalLines = 1;
            }
        }

        if (timestamp)
        {
            Console.WriteLine(@"[{0:HH:mm:ss}] {1} {2}", DateTime.Now, _lastPrintedLine, _identicalLines > 1 ? _identicalLines + "x" : string.Empty);
        }
        else
        {
            Console.WriteLine(@"{0} {1}", _lastPrintedLine, _identicalLines > 1 ? _identicalLines + "x" : string.Empty);
        }
    }

    internal static void LogError<T>(T message, bool inline = false, bool timestamp = true) => CheckAndPrint(message, ConsoleColor.Red, inline, timestamp);

    internal static void LogWarn<T>(T message, bool inline = false, bool timestamp = true) => CheckAndPrint(message, ConsoleColor.Yellow, inline, timestamp);

    internal static void LogInfo<T>(T message, bool inline = false, bool timestamp = true) => CheckAndPrint(message, ConsoleColor.Green, inline, timestamp);

    internal static void LogDebug<T>(T message, bool inline = false, bool timestamp = true)
    {
#if DEBUG
        CheckAndPrint(message, ConsoleColor.DarkGray, inline, timestamp);
#endif
    }


    private static void CheckAndPrint<T>(T message, ConsoleColor color, bool inline, bool timestamp)
    {
        Console.ForegroundColor = color;
        if (message is not null)
        {
            if (inline)
            {
                if (timestamp)
                {
                    Console.Write(@"[{0:HH:mm:ss}] {1}", DateTime.Now, message);
                }
                else
                {
                    Console.Write(@"{0}", message);
                }
            }
            else
            {
                Print(message, timestamp);
            }
        }
        else
        {
            Print("Trying to print null object!", timestamp);
        }

        Console.ForegroundColor = ConsoleColor.Gray;
    }

    internal static string Truncate(this string value, int maxChars, string truncateChars = "..") => value.Length <= maxChars ? value : value.Substring(0, maxChars) + truncateChars;
}