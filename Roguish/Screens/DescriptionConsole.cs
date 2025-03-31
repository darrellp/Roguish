using SystemsRx.ReactiveData;

namespace Roguish.Screens;

internal class DescriptionConsole()
    : Console(GameSettings.DescWidth, GameSettings.DescHeight)
{
    public static ReactiveProperty<string> Description = new("Testing!");

    #region Handlers
    public void SetDescription(string description)
    {
        if (description != Description.Value)
        {
            Description.SetValueAndForceNotify(description);
        }
    }
    #endregion
}
