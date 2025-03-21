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
}
