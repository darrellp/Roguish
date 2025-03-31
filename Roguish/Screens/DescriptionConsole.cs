using System.Data.SqlTypes;
using SystemsRx.ReactiveData;
using SadConsole.StringParser;

namespace Roguish.Screens;

internal class DescriptionConsole : Console
{
    public DescriptionConsole() : base(GameSettings.DescWidth, GameSettings.DescHeight)
    {
        this.Surface.UsePrintProcessor = true;
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
