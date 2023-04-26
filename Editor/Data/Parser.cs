using System;
using System.Collections.Generic;
using System.IO;

namespace Editor.Data;

public static class Parser
{
    public static List<DataNode> ReadData(string path)
    {
        string text = File.ReadAllText(path);
        text = RemoveStringParts(text, " ");
        text = RemoveStringComments(text);
        text = RemoveStringParts(text, "\r\n");
        text = RemoveStringParts(text, "\t");
        text = RemoveStringParts(text, "\r");
        text = RemoveStringParts(text, "\n");
        List<DataNode> list;
        try
        {
            list = ProcessData(text);
        }
        catch (Exception ex)
        {
            throw new Exception("Error: Parser ProcessData failed\r\n" + ex);
        }

        return list;
    }

    private static List<DataNode> ProcessData(string dataString)
    {
        var list = new List<DataNode>();
        string text = string.Empty;
        string text2 = string.Empty;
        try
        {
            for (int num = dataString.IndexOfAny(new[] { '(', '=' }); num != -1; num = dataString.IndexOfAny(new[] { '(', '=' }))
            {
                text = dataString.Substring(0, num);
                dataString = dataString.Substring(num, dataString.Length - num);
                if (dataString[0] == '=')
                {
                    int num2 = dataString.IndexOf(';');
                    if (num2 == -1)
                    {
                        throw new Exception("Parse Error: Expected ';' at end of value for property '" + text + "'");
                    }

                    text2 = dataString.Substring(1, num2 - 1);
                    dataString = dataString.Substring(num2 + 1, dataString.Length - num2 - 1);
                    list.Add(HandleChildNode(text, text2, string.Empty));
                }
                else
                {
                    int num3 = dataString.IndexOf(')');
                    if (num3 == -1)
                    {
                        throw new Exception("Parse Error: Expected ')' at end of value for property '" + text + "'");
                    }

                    text2 = dataString.Substring(1, num3 - 1);
                    dataString = dataString.Substring(num3 + 1, dataString.Length - num3 - 1);
                    if (dataString[0] != '{')
                    {
                        throw new Exception("Parse Error: Expected start of data '{' after value for property '" + text + "'");
                    }

                    num3 = -1;
                    int num4 = 1;
                    for (int i = 1; i < dataString.Length; i++)
                    {
                        if (dataString[i] == '{')
                        {
                            num4++;
                        }
                        else if (dataString[i] == '}')
                        {
                            num4--;
                            if (num4 == 0)
                            {
                                num3 = i;
                                break;
                            }
                        }
                    }

                    if (num3 == -1)
                    {
                        throw new Exception("Parse Error: Expected '}' not found for property '" + text + "'");
                    }

                    string text3 = dataString.Substring(1, num3 - 1);
                    dataString = dataString.Substring(num3 + 1, dataString.Length - num3 - 1);
                    list.Add(HandleChildNode(text, text2, text3));
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Parse Error: Error at property '{text}' with value '{text2}'\r\n{ex}");
        }

        return list;
    }

    private static DataNode HandleChildNode(string property, string value, string childNodes)
    {
        char[] array = { ' ', '{', '}', '=', ';' };
        int num = property.IndexOfAny(array);
        if (num != -1)
        {
            throw new Exception("Parse Error: Property '" + property + "' contains invalid chars at index " + num);
        }

        num = value.IndexOfAny(array);
        if (num != -1)
        {
            throw new Exception(string.Concat("Parse Error: Value '", value, "' for property '", property, "' contains invalid chars at index ", num.ToString()));
        }

        var dataNode = new DataNode(property, value);
        if (childNodes == string.Empty)
        {
            return dataNode;
        }

        dataNode.ChildNodes = ProcessData(childNodes);
        return dataNode;
    }

    private static string RemoveStringComments(string dataString)
    {
        for (int num = dataString.IndexOf("//", StringComparison.Ordinal); num != -1; num = dataString.IndexOf("//", StringComparison.Ordinal))
        {
            int num2 = dataString.IndexOf("\r\n", num, StringComparison.Ordinal);
            if (num2 == -1)
            {
                num2 = dataString.IndexOf("\r", num, StringComparison.Ordinal);
            }

            if (num2 == -1)
            {
                num2 = dataString.IndexOf("\n", num, StringComparison.Ordinal);
            }

            dataString = num2 == -1 ? dataString.Remove(num, dataString.Length - num) : dataString.Remove(num, num2 - num);
        }

        return dataString;
    }

    private static string RemoveStringParts(string dataString, string partsToRemove)
    {
        int num = dataString.Length;
        dataString = dataString.Replace(partsToRemove, "");
        while (dataString.Length != num)
        {
            num = dataString.Length;
            dataString = dataString.Replace(partsToRemove, "");
        }

        return dataString;
    }
}