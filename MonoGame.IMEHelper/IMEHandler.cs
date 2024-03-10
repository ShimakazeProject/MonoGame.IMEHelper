using System;

using Microsoft.Xna.Framework;

namespace MonoGame.IMEHelper;

public abstract class IMEHandler
{
    public static IMEHandler Create(Game game)
    {
#if WINFORMS
        return new WinFormsIMEHandler(game);
#else
        return new SdlIMEHandler(game);
#endif
    }

    /// <summary>
    /// Game Instance
    /// </summary>
    protected Game GameInstance { get; }

    protected IMEHandler(Game game)
    {
        GameInstance = game;

        PlatformInitialize();
    }


    /// <summary>
    /// Platform specific initialization.
    /// </summary>
    protected abstract void PlatformInitialize();

    /// <summary>
    /// Check if text composition is enabled currently.
    /// </summary>
    public abstract bool Enabled { get; protected set; }

    /// <summary>
    /// Enable the system IMM service to support composited character input.
    /// This should be called when you expect text input from a user and you support languages
    /// that require an IME (Input Method Editor).
    /// </summary>
    /// <seealso cref="StopTextComposition" />
    /// <seealso cref="TextComposition" />
    public abstract void StartTextComposition();

    /// <summary>
    /// Stop the system IMM service.
    /// </summary>
    /// <seealso cref="StartTextComposition" />
    /// <seealso cref="TextComposition" />
    public abstract void StopTextComposition();

    /// <summary>
    /// Use this function to set the rectangle used to type Unicode text inputs if IME supported.
    /// In SDL2, this method call gives the OS a hint for where to show the candidate text list,
    /// since the OS doesn't know where you want to draw the text you received via `SDL_TEXTEDITING` event.
    /// </summary>
    public virtual void SetTextInputRect(ref Rectangle rect) { }

    /// <summary>
    /// Invoked when the IMM service emit character input event.
    /// </summary>
    /// <seealso cref="StartTextComposition" />
    /// <seealso cref="StopTextComposition" />
    public event EventHandler<TextInputEventArgs>? TextInput;

    /// <summary>
    /// Redirect a sdl text input event.
    /// </summary>
    public virtual void OnTextInput(TextInputEventArgs args) => TextInput?.Invoke(this, args);
}