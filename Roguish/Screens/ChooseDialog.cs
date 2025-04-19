using Ninject;
using SadConsole.Components;
using SadConsole.Input;
using SadConsole.UI;
using SadConsole.UI.Controls;

namespace Roguish.Screens;
internal class ChooseDialog : Window
{
    private IList<string> _options;
    private string _title;
    private bool _canChooseMultiple;
    private Action<EcsEntity, List<int>>? _onDismiss;
    private int _selectedIndex;
    private EcsEntity _entity;
    internal List<int> Selected { get; init; }= [];

    private const int ButtonsLength = 23;
    public ChooseDialog(string title, IList<string> options, EcsEntity entity, Action<EcsEntity, List<int>>? onDismiss = null,
        bool canChooseMultiple = false) 
        : base(Math.Max(ButtonsLength, Math.Max(title.Length, options.Select(s => s.Length).Max()) + 4), options.Count + 4)
    {
        var x = (GameSettings.GameWidth - Width) / 2;
        var y = (GameSettings.GameHeight - Height) / 2;
        Position = new Point(x, y);
        _title = title;
        _options = options;
        _canChooseMultiple = canChooseMultiple;
        _onDismiss = onDismiss;
        _entity = entity;
        Surface.UsePrintProcessor = true;
        var btnOkay = new Button(4)
        {
            Text = "Ok",
            Position = new Point(2, Height - 2),
            FocusOnMouseClick = false
        };
        btnOkay.Click += (sender, args) =>
        {
            if (_onDismiss != null)
            {
                _onDismiss(_entity, Selected);
            }
            Close();
            Shutdown();
        };
        var btnCancel = new Button(8)
        {
            Text = "Cancel",
            Position = new Point(7, Height - 2),
            FocusOnMouseClick = false
        };
        btnCancel.Click += (sender, args) =>
        {
            Close();
            Shutdown();
        };
        var btnAll = new Button(5)
        {
            Text = "All",
            Position = new Point(16, Height - 2),
            FocusOnMouseClick = false
        };
        btnAll.Click += (sender, args) =>
        {
            SelectAll();
        };
        var host = _canChooseMultiple ?
            new ControlHost { btnOkay, btnCancel, btnAll } :
            new ControlHost { btnOkay, btnCancel };
        SadComponents.Add(host);
    }

    public void ShowDialog()
    {
        GameHost.Instance.Screen!.Children.Add(this);
        GameHost.Instance.Screen!.Children.MoveToTop(this);
        Title = _title;
        for (var i = 0; i < _options.Count; i++)
        {
            Surface.Print(1, i + 1, _options[i]);
        }

        IsFocused = true;
        FocusedMode = FocusBehavior.Push;

        Show(true);
        HighlightOption();
    }

    private void Shutdown()
    {
        Hide();
        Dispose();
        Kernel.Get<DungeonSurface>().IsFocused = true;
    }

    public override bool ProcessMouse(MouseScreenObjectState state)
    {
        if (state.Mouse.LeftClicked)
        {
            var index = state.CellPosition.Y - 1;
            if (index >= 0 && index < _options.Count)
            {
                _selectedIndex = index;
                HighlightOption();
                SelectOption();
            }
        }
        return base.ProcessMouse(state);
    }

    public override bool ProcessKeyboard(Keyboard keyboard)
    {
        var keysPressed = keyboard.KeysPressed;
        if (keysPressed.Count != 1)
        {
            // Wait for unambiguous single key press
            return base.ProcessKeyboard(keyboard);
        }
        switch (keysPressed[0].Key)
        {
            case Keys.Escape:
                Close();
                Shutdown();
                return true;

            case Keys.Space:
                SelectOption();
                return true;
        }
        if (keyboard.IsKeyPressed(Keys.Up))
        {
            _selectedIndex = (_selectedIndex - 1 + _options.Count) % _options.Count;
            HighlightOption();
            return true;
        }
        if (keyboard.IsKeyPressed(Keys.Down))
        {
            _selectedIndex = (_selectedIndex + 1) % _options.Count;
            HighlightOption();
            return true;
        }
        if (keyboard.IsKeyPressed(Keys.Enter))
        {
            if (_onDismiss != null)
            {
                _onDismiss(_entity, Selected);
            }
            Close();
            Shutdown();
            return true;
        }
        return base.ProcessKeyboard(keyboard);
    }

    public void Close()
    {
        if (Parent != null)
        {
            Parent.Children.Remove(this);
        }
    }

    private void HighlightOption()
    {
        for (var i = 0; i < _options.Count; i++)
        {
            var color = i == _selectedIndex ? Color.Yellow : Color.White;
            Surface.Print(1, i + 1, _options[i], color);
        }
    }

    private void SelectAll()
    {
        var oldSelected = _selectedIndex;
        for (int index = 0; index < _options.Count; index++)
        {
            if (!Selected.Contains(index))
            {
                _selectedIndex = index;
                SelectOption();
            }
        }
        _selectedIndex = oldSelected;
        HighlightOption();
    }

    private void SelectOption()
    {
        if (!_canChooseMultiple)
        {
            foreach (var index in Selected)
            {
                Surface.Print(1, index + 1, _options[index], Color.White, Color.Transparent);
            }
            Selected.Clear();
        }
        var newlySelected = !Selected.Contains(_selectedIndex);
        var bg = newlySelected ? Color.DarkCyan : Color.Transparent;
        Surface.Print(1, _selectedIndex + 1,  _options[_selectedIndex], Color.Yellow, bg);
        if (newlySelected)
        {
            Selected.Add(_selectedIndex);
        }
        else
        {
            Selected.Remove(_selectedIndex);
        }
    }
}

