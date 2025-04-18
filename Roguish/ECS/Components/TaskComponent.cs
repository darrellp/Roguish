using Newtonsoft.Json;
using Roguish.ECS.Tasks;

namespace Roguish.ECS.Components;

////////////////////////////////////////////////////////////////////////////////////////////////////
/// <summary>   A task component. </summary>
/// 
/// <remarks>   Darrell Plank, 3/29/2025. </remarks>
////////////////////////////////////////////////////////////////////////////////////////////////////

public class TaskComponent : EcsComponent
{
    public List<RogueTask> Tasks { get; set; }

    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   A task component. </summary>
    ///
    /// <remarks>   Darrell Plank, 3/29/2025. </remarks>
    ///
    /// <param name="fireOn">   The tick count to fire on. </param>
    /// <param name="taskType"> The action to take with arg of entity ID. </param>
    /// <param name="ptArg">    (Optional) The point argument. </param>
    ////////////////////////////////////////////////////////////////////////////////////////////////////

    [JsonConstructor]
    public TaskComponent(ulong fireOn, TaskType taskType, Point ptArg = default)
    {
        Tasks = [new RogueTask(fireOn, taskType, ptArg)];
    }

    public TaskComponent(params RogueTask[] tasks)
    {
        Tasks = tasks.ToList();
    }

    internal IEnumerable<RogueTask> NextTasks()
    {
        if (Tasks.Count == 0)
        {
            return [];
        }
        if (Tasks.Count == 1)
        {
            return [Tasks[0]];
        }
        var ticksNext = Tasks.Select(t => t.FireOn).Min();
        return Tasks.Where(t => t.FireOn == ticksNext);
    }
}

////////////////////////////////////////////////////////////////////////////////////////////////////
/// <summary>   A task. </summary>
///
/// <remarks>
/// A task is a timed action which occurs when the clock ticks have reached or surpassed FireOn.
/// When a task fires it is it's own responsibility to update itself with a new action/FireOn
/// time Darrell Plank, 4/5/2025.
/// </remarks>
///
/// <param name="fireOn">   The fire on. </param>
/// <param name="taskType"> The action. </param>
/// <param name="ptArg">    The point argument. </param>
////////////////////////////////////////////////////////////////////////////////////////////////////

public class RogueTask(ulong fireOn, TaskType taskType, Point ptArg = default)
{
    public ulong FireOn { get; set; } = fireOn;
    public TaskType TaskType { get; set; } = taskType;

    // Some actions need this, some ignore it
    public Point PointArg = ptArg;

    public void Action(EcsEntity entity)
    {
        TaskGetter.ActionTable[(int)TaskType](entity, this);
    }
}
