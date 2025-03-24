using System.Collections.ObjectModel;
using SadConsole.Input;

namespace Roguish.ECS.Events;
internal class KeyboardEvent(ReadOnlyCollection<AsciiKey> keys, bool retrieveFromQueue = false, bool shutdownQueue = false)
{
    public bool ShutdownQueue { get; set; } = shutdownQueue;
    public bool RetrieveFromQueue { get; set; } = retrieveFromQueue;
    public ReadOnlyCollection<AsciiKey>? Keys { get; set; } = keys;
}

