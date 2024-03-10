using Microsoft.Xna.Framework;

namespace MonoGame.IMEHelper;

/// <summary>
/// Integrate IME to DesktopGL(SDL2) platform.
/// </summary>
internal sealed class SdlIMEHandler : IMEHandler
{
    public override bool Enabled { get; protected set; }

    public SdlIMEHandler(Game game) : base(game)
    {
        GameInstance.Window.TextInput += Window_TextInput;
    }


    private void Window_TextInput(object? sender, Microsoft.Xna.Framework.TextInputEventArgs e) => OnTextInput(new TextInputEventArgs(e.Character, e.Key));

    public override void StartTextComposition()
    {
        if (Enabled)
            return;

        Sdl.StartTextInput();
        Enabled = true;
    }

    public override void StopTextComposition()
    {
        if (!Enabled)
            return;

        Sdl.StopTextInput();
        Enabled = false;
    }

    public override void SetTextInputRect(in Rectangle rect)
    {
        var sdlRect = new Sdl.Rectangle() { X = rect.X, Y = rect.Y, Width = rect.Width, Height = rect.Height };
        Sdl.SetTextInputRect(sdlRect);
    }
}