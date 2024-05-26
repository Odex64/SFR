using HarmonyLib;

namespace SFR.Editor;

/// <summary>
/// This class is used to retain the validation map token, in order to mark a custom map as official through HEX editors.
/// </summary>
[HarmonyPatch]
internal static class MapToken
{
#if DEBUG
    [HarmonyPrefix]
    [HarmonyPatch(typeof(MapInfo), nameof(MapInfo.IsOfficialMap))]
    private static bool GetMapOfficialToken(string header)
    {
        char[] array = "0123456789".ToCharArray();
        int num = array.Length;
        for (int i = 0; i < header.Length; i++)
        {
            array[i % num] = (char)(array[i % num] + header[i % num]);
        }

        array[0] = '1';
        Logger.LogDebug($"Map validation token: {BitConverter.ToString(Encoding.UTF8.GetBytes(new string(array))).Replace("-", string.Empty)}");
        return true;
    }
#endif
}