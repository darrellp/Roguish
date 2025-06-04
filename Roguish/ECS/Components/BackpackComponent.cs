using System.Diagnostics;
using EcsRx.Extensions;

namespace Roguish.ECS.Components;

internal class BackpackComponent : EcsComponent
{
    Dictionary<string, List<int>> _itemIdStacks = new();

    internal void AddToBackpack(int id)
    {
        var entity = EcsApp.EntityDatabase.GetEntity(id);
        AddToBackpack(entity);
    }

    internal void AddToBackpack(EcsEntity entity)
    {
        var name = "Unknown item";
        if (entity.HasComponent<DescriptionComponent>())
        {
            // Is the name sufficient disambiguation here?  That's what we're assuming here - if two itens have the same
            // name then they are identical items.
            name = entity.GetComponent<DescriptionComponent>().Name;
        }

        if (!_itemIdStacks.ContainsKey(name))
        {
            _itemIdStacks[name] = new List<int>();
        }
        _itemIdStacks[name].Add(entity.Id);
    }

    internal EcsEntity RemoveFromBackpack(int id)
    {
        var name = "Unknown item";

        var entity = EcsApp.EntityDatabase.GetEntity(id);
        if (entity.HasComponent<DescriptionComponent>())
        {
            name = entity.GetComponent<DescriptionComponent>().Name;
        }

        if (!_itemIdStacks.ContainsKey(name))
        {
            throw new InvalidOperationException($"Item '{name}' not found in backpack.");
        }
        Debug.Assert(_itemIdStacks[name].Count > 0, $"Item '{name}' with id {id} has zero items in it.");

        if (_itemIdStacks[name].Count <= 1)
        {
            _itemIdStacks.Remove(name);
        }
        else
        {
            _itemIdStacks[name].RemoveAt(0);
        }
        return entity;

    }
}
