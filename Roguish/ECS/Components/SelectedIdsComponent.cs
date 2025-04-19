namespace Roguish.ECS.Components;

////////////////////////////////////////////////////////////////////////////////////////////////////
/// <summary>   An entity identifiers component. </summary>
///
/// <remarks>   This is just a temporary component intended to transmit a selection of entities
///             from a ChooseDialog to a function which will use the results.
///             Darrell Plank, 4/18/2025. </remarks>
///
/// <param name="ids">  The identifiers. </param>
////////////////////////////////////////////////////////////////////////////////////////////////////

internal class SelectedIdsComponent(List<int> ids) : EcsComponent
{
    internal List<int> Ids { get; set; } = ids;
}
