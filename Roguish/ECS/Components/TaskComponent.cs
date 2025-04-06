using System.Diagnostics;
using Roguish.ECS.Events;
using Roguish.Map_Generation;

namespace Roguish.ECS.Components;

////////////////////////////////////////////////////////////////////////////////////////////////////
/// <summary>   A task component. </summary>
/// 
/// <remarks>   Darrell Plank, 3/29/2025. </remarks>
////////////////////////////////////////////////////////////////////////////////////////////////////

internal class TaskComponent : EcsComponent
{
    internal List<RogueTask> Tasks { get; set; }

    /// <summary>   A task component. </summary>
    ///
    /// <remarks>   Darrell Plank, 3/29/2025. </remarks>
    ///
    /// <param name="fireOn">   The tick count to fire on. </param>
    /// <param name="action">   The action to take with arg of entity ID. </param>
    public TaskComponent(ulong fireOn, Action<EcsEntity, RogueTask>? action)
    {
        Tasks = [new RogueTask(fireOn, action)];
    }

    public TaskComponent(params RogueTask[] tasks)
    {
        Tasks = tasks.ToList();
    }

    internal IEnumerable<RogueTask> NextTasks()
    {
        if (Tasks.Count == 0)
        {
            return Enumerable.Empty<RogueTask>();
        }
        if (Tasks.Count == 1)
        {
            return Enumerable.Repeat(Tasks[0], 1);
        }
        var ticksNext = Tasks.Select(t => t.FireOn).Min();
        return Tasks.Where(t => t.FireOn == ticksNext);
    }
}

////////////////////////////////////////////////////////////////////////////////////////////////////
/// <summary>   A task. </summary>
///
/// <remarks>   A task is a timed action which occurs when the clock ticks have reached
///             or surpassed FireOn.  When a task fires it is it's own responsibility
///             to update itself with a new action/FireOn time
///             Darrell Plank, 4/5/2025. </remarks>
///
/// <param name="fireOn">   The fire on. </param>
/// <param name="action">   The action. </param>
////////////////////////////////////////////////////////////////////////////////////////////////////

internal class RogueTask(ulong fireOn, Action<EcsEntity, RogueTask>? action)
{
    internal ulong FireOn { get; set; } = fireOn;
    internal Action<EcsEntity, RogueTask>? Action { get; set; } = action;
}
