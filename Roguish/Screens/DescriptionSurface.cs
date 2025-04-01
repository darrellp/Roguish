using SystemsRx.ReactiveData;

namespace Roguish.Screens;

internal class DescriptionSurface : ScreenSurface
{
    public DescriptionSurface() : base(GameSettings.DescWidth, GameSettings.DescHeight)
    {
        Surface.UsePrintProcessor = true;
    }

    public static ReactiveProperty<string> Description = new("");

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
