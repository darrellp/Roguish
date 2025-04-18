﻿using EcsRx.Extensions;
using Roguish.ECS.Components;

namespace Roguish;
internal static class Utility
{
    internal static Color DimmedColor(Color color)
    {
        var (h, s, l) = (
            color.GetHSLHue(),
            color.GetHSLSaturation(),
            color.GetHSLLightness());
        return Color.FromHSL(h, s * 0.5f, l * 0.5f);
    }

    internal static bool IsVowel(char c)
    {
        return "AEIOUaeiou".Contains(c);
    }

    internal static string PrefixWithAorAn(string str)
    {
        return IsVowel(str[0]) ? "an " + str : "a " + str;
    }

    internal static string PrefixWithAorAnColored(string str, string color)
    {
        var colorCode = $"[c:r f:{color}]" + str + "[c:undo]";
        return IsVowel(str[0]) ? "an " + colorCode: "a " + colorCode;
    }

    internal static string GetName(EcsEntity item)
    {
        var name = item.HasComponent<DescriptionComponent>()
            ? Utility.PrefixWithAorAnColored(item.GetComponent<DescriptionComponent>().Name, "Yellow")
            : "an [c:r f:Yellow]unnamed object";
        return name;
    }

}
