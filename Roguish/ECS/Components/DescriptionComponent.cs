namespace Roguish.ECS.Components;

internal class DescriptionComponent(string name, string description) : EcsComponent
{
    public string Name = name;
    public string Description = description;
}