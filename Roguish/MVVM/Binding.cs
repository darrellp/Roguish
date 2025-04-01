using Ninject;
using SadConsole.UI.Controls;
using SystemsRx.ReactiveData;

namespace Roguish.MVVM;

internal class Binding
{
    public EventHandler? Command = null;
    public required ControlBase? Control;
    public readonly string? Name = null;
    public required Point Position;
    public required Type Screen;
    public ScreenSurface Surface => (ScreenSurface)Kernel.Get(Screen);

    public virtual void SetBinding()
    {
        throw new NotImplementedException();
    }
}

internal class Binding<T> : Binding
{
    public ReactiveProperty<T>? BindValue = null;
    public Func<object, Action<T>>? Observer = null;

    public override void SetBinding()
    {
        if (Observer == null || BindValue == null)
            throw new InvalidOperationException("Bound controls must have both BindValue and Observer");

        var observer = Observer!((object?)Control ?? Surface);
        BindValue!.Subscribe(observer);
    }
}