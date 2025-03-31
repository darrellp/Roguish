using System.Diagnostics;
using Roguish.Screens;
using SadConsole.UI;
using SadConsole.UI.Controls;
using Label = SadConsole.UI.Controls.Label;

namespace Roguish.MVVM;
internal static class Bindings
{
    public static Dictionary<string, ControlBase?> Controls { get; } = [];

    public static List<Binding> BindingList { get; } = [
        new()
        {
            Screen = typeof(StatusBar), 
            Position = new Point(13, 0), 
            Control = new Button(10) {Text = "Redraw", FocusOnMouseClick = false}, 
            Command = StatusBar.RedrawClick,
        },
        new()
        {
            Screen = typeof(StatusBar),
            Position = new Point(30, 0),
            Control = new CheckBox("Draw Path"),
            Command = StatusBar.DrawPathClick,
        },
        new Binding<Point>
        {
            Screen = typeof(StatusBar),
            Position = new Point(0, 0),
            Control = new Label(12),
            BindValue = StatusBar.MousePosition,
            Observer = StatusBar.GetMousePosObserver,
        },
        new()
        {
            Screen = typeof(StatusBar),
            Position = new Point(45, 0),
            Control = new Button(10) {Text = "FOV", FocusOnMouseClick = false},
            Command = StatusBar.FovClick,
        },
        new Binding<string>
        {
            Screen = typeof(DescriptionConsole),
            Position = new Point(0, 0),
            Control = null,
            BindValue = DescriptionConsole.Description,
            Observer = MoveStringToConsole,
        },
    ];

    public static void Bind()
    {
        Dictionary<ScreenSurface, ControlHost> controlHosts = new();

        foreach (var binding in BindingList)
        {
            var surface = binding.Surface;
            if (binding.Control == null)
            {
                binding.SetBinding();
                continue;
            }

            binding.Control.Position = binding.Position;
            if (binding.Name != null)
            {
                Controls[binding.Name] = binding.Control;
            }

            if (!controlHosts.ContainsKey(surface))
            {
                controlHosts[surface] = [];
            }

            if (binding.GetType().IsGenericType)
            {
                binding.SetBinding();
            }

            switch (binding.Control)
            {
                case Button button:
                    controlHosts[surface].Add(button);
                    button.Click += binding.Command;
                    break;

                case CheckBox checkBox:
                    controlHosts[surface].Add(checkBox);
                    checkBox.Click += binding.Command;
                    break;

                case Label label:
                    controlHosts[surface].Add(label);
                    break;

                case null:
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        foreach (var keyValue in controlHosts)
        {
            keyValue.Key.SadComponents.Add(keyValue.Value);
        }
    }

    // Common handlers
    public static Action<string> MoveStringToLabel(object c)
    {
        var label = c as Label;
        return (string str) => label!.DisplayText = str;
    }

    public static Action<string> MoveStringToConsole(object c)
    {
        var console = c as Console;
        Debug.Assert(console != null, nameof(console) + " != null");
        return (string str) =>
        {
            console.Clear();
            console!.Print(0, 0, str);
        };
    }
}
