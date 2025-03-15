using EcsRx.Components;

namespace Roguish.ECS.Components;

internal class DisplayComponent(ColoredGlyph glyph) : IComponent
{
    public ColoredGlyph Glyph { get; set; } = glyph;
}