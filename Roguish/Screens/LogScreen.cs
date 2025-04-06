using SadConsole.UI;
using SadConsole.UI.Controls;
using SadConsole.Input;

namespace Roguish.Screens;

////////////////////////////////////////////////////////////////////////////////////////////////////
/// <summary>   The log screen. </summary>
///
/// <remarks>   Shamelessly stolen from the SadConsole demo
///             Jar Czar, 4/3/2025. </remarks>
////////////////////////////////////////////////////////////////////////////////////////////////////

internal class LogScreen : ControlsConsole
{
    private const int MaxLines = 100;
    private readonly ScrollBar _scrollBar;
    private int _scrollOffset;
    private int _lastCursorY;

    public Console MessageBuffer { get; }

    public LogScreen() : base(GameSettings.LogWidth, GameSettings.LogHeight)
    {
        Position = GameSettings.LogPosition;

        // Input settings
        UseKeyboard = false;
        UseMouse = true;
        FocusOnMouseClick = false;

        // Create the message buffer console
        MessageBuffer = new Console(Width - 1, Height, Width - 1, MaxLines);
        MessageBuffer.UseMouse = false;
        MessageBuffer.UseKeyboard = false;

        // Reassign the message buffer cursor to this object
        SadComponents.Remove(Cursor);
        Cursor = MessageBuffer.Cursor;
        Cursor.IsVisible = false;
        Cursor.IsEnabled = true;

        // Remove the surface renderer, we don't care what this surface has on itself
        Renderer!.Steps.RemoveAll(p => p.Name == SadConsole.Renderers.Constants.RenderStepNames.Surface);
        Renderer.Steps.RemoveAll(p => p.Name == SadConsole.Renderers.Constants.RenderStepNames.Tint);

        // Handle the scroll bar control
        _scrollBar = new ScrollBar(Orientation.Vertical, Height);
        _scrollBar.IsEnabled = false;
        _scrollBar.ValueChanged += (sender, e) => MessageBuffer.ViewPosition = (0, _scrollBar.Value);
        _scrollBar.Position = (Width - 1, 0);
        Controls.Add(_scrollBar);

        Children.Add(MessageBuffer);
        MessageBuffer.Surface.UsePrintProcessor = true;
    }

    internal void PrintProcessedString(string str, bool fNewLine = true)
    {
        var coloredString = ColoredString.Parser.Parse(str);
        if (fNewLine)
        {
            Cursor.Print(coloredString).NewLine();
        }
        else
        {
            Cursor.Print(coloredString);
        }
    }

    internal void Clear()
    {
        MessageBuffer.Clear();
        _scrollOffset = 0;
        Cursor.Position = (0, 0);
        _scrollBar.IsEnabled = false;
    }

    public override void Update(TimeSpan delta)
    {
        // If cursor has moved below the visible area, track the difference
        if (MessageBuffer.Cursor.Position.Y > _scrollOffset + MessageBuffer.ViewHeight - 1)
            _scrollOffset = MessageBuffer.Cursor.Position.Y - MessageBuffer.ViewHeight + 1;

        // Adjust the scroll bar
        _scrollBar.IsEnabled = _scrollOffset != 0;
        _scrollBar.MaximumValue = _scrollOffset;

        // If autoscrolling is enabled, scroll
        if (_scrollBar.IsEnabled && _lastCursorY != MessageBuffer.Cursor.Position.Y)
        {
            _scrollBar.Value = _scrollBar.MaximumValue;
            _lastCursorY = MessageBuffer.Cursor.Position.Y;
        }

        // Update the base class which includes the controls
        base.Update(delta);
    }

    public override bool ProcessMouse(MouseScreenObjectState state)
    {
        if (state.Mouse.ScrollWheelValueChange != 0)
            return _scrollBar.ProcessMouseWheel(state);

        return base.ProcessMouse(state);
    }

}
