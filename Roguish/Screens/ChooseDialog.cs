using Ninject;
using SadConsole.Input;
using SadConsole.UI;

namespace Roguish.Screens;
internal class ChooseDialog : Window
{
    private IList<string> _options;
    private string _title;
    private bool _canChooseMultiple;
    private Action<EcsEntity, List<int>> _onDismiss;
    private int _selectedIndex;
    private EcsEntity _entity;
    internal List<int> Selected { get; init; }= [];

    public ChooseDialog(string title, IList<string> options, Action<EcsEntity, List<int>> onDismiss, EcsEntity entity, bool canChooseMultiple = false) 
        : base(Math.Max(title.Length, options.Select(s => s.Length).Max()) + 4, options.Count + 3)
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
            _selectedIndex = state.CellPosition.Y - 1;
            HighlightOption();
            SelectOption();
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
                if (_canChooseMultiple)
                {
                    SelectOption();
                }
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

    private void SelectOption()
    {
        var newlySelected = !Selected.Contains(_selectedIndex);
        var bg = Color.DarkCyan;
        if (!newlySelected)
        {
            bg = Color.Transparent;
        }
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

