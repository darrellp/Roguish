using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguish;
internal static class Utility
{
    public static Color DimmedColor(Color color)
    {
        var (h, s, l) = (
            color.GetHSLHue(),
            color.GetHSLSaturation(),
            color.GetHSLLightness());
        return Color.FromHSL(h, s * 0.5f, l * 0.5f);
    }

    public static bool IsVowel(char c)
    {
        return "AEIOUaeiou".Contains(c);
    }

    public static string PrefixWithAorAn(string str)
    {
        return IsVowel(str[0]) ? "an " + str : "a " + str;
    }

    public static string PrefixWithAorAnColored(string str, string color)
    {
        var colorCode = $"[c:r f:{color}]" + str + "[c:undo]";
        return IsVowel(str[0]) ? "an " + colorCode: "a " + colorCode;
    }

}
