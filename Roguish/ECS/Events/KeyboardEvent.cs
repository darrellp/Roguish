using System.Collections.ObjectModel;
using SadConsole.Input;

namespace Roguish.ECS.Events;
internal class KeyboardEvent(ReadOnlyCollection<AsciiKey>? keys, bool retrieveFromQueue = false)
{
    public bool RetrieveFromQueue { get; init; } = retrieveFromQueue;
    public ReadOnlyCollection<AsciiKey>? Keys { get; } = keys;
}

