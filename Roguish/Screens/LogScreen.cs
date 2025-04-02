using Ninject;
using Roguish.ECS.Systems;
using SadConsole.UI.Controls;
using SystemsRx.ReactiveData;

namespace Roguish.Screens;
internal class LogScreen : ScreenSurface
{
    public LogScreen() : base(GameSettings.LogWidth, GameSettings.LogHeight)
    {
    }
}
