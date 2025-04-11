namespace Roguish.Serialization;
internal record EntityInfo(int OriginalId, List<EcsComponent> Components)
{
    public T? FindComponent<T>() where T : class
    {
        return Components.FirstOrDefault(c => c is T) as T;
    }

    public int ComponentIndex<T>()
    {
        int? i = Components.Select((c, i) => (c, i)).Where(t => t.c is T).Select(t => t.i).FirstOrDefault();
        return i ?? -1;
    }
}

