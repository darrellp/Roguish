using Ninject;
using SadConsole.UI.Controls;
using SystemsRx.ReactiveData;

namespace Roguish.MVVM;

internal class Binding
{
    public string? Name = null;
    public required Type Screen;
    public required Point Position;
    public required ControlBase Control;
    public EventHandler? Command = null;
    public ScreenSurface Surface => (ScreenSurface)Program.Kernel.Get(Screen);

    public virtual void SetBinding()
    {
        throw new NotImplementedException();
    }
};

internal class Binding<T> : Binding
{
    public ReactiveProperty<T>? BindValue = null;
    public Func<ControlBase, Action<T>>? Observer = null;

    public override void SetBinding()
    {
        if (Observer == null || BindValue == null)
        {
            throw new InvalidOperationException("Bound controls must have both BindValue and Observer");
        }
        var observer = Observer!(Control);
        BindValue!.Subscribe(observer);
    }
}
