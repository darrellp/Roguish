namespace Roguish.ECS.Components;

////////////////////////////////////////////////////////////////////////////////////////////////////
/// <summary>   A task component. </summary>
///
/// <remarks>   Jar C, 3/29/2025. </remarks>
///
/// <param name="fireOn">   The tick count to fire on. </param>
/// <param name="entityId"> Identifier for the entity. </param>
/// <param name="action">   The action to take with arg of entity ID. </param>
////////////////////////////////////////////////////////////////////////////////////////////////////

internal class TaskComponent(ulong fireOn, Action<EcsEntity>? action) : EcsComponent
{
    public ulong FireOn
    {
        get => fireOn;
        set => fireOn = value;
    }

    public Action<EcsEntity>? Action
    {
        get => action;
        set => action = value;
    }
}