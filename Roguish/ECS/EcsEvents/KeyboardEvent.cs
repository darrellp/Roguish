using System.Collections.ObjectModel;
using SadConsole.Input;

namespace Roguish.ECS.EcsEvents;
internal class KeyboardEvent(ReadOnlyCollection<AsciiKey> keys)
{
    public ReadOnlyCollection<AsciiKey> Keys { get; set; } = keys;
}

