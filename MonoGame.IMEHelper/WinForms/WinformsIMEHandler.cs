using System;

using Microsoft.Xna.Framework;

using Windows.Win32.Foundation;

namespace MonoGame.IMEHelper;

/// <summary>
/// Integrate IME to XNA framework.
/// </summary>
internal sealed class WinFormsIMEHandler(Game game) : IMEHandler(game), IDisposable
{
    private IMENativeWindow? _nativeWnd;
    private IMENativeWindow NativeWnd => _nativeWnd ?? throw new InvalidOperationException();

    protected override void PlatformInitialize()
    {
        _nativeWnd = new IMENativeWindow((HWND)GameInstance.Window.Handle);

        GameInstance.Exiting += (_, _) => Dispose();
    }

    public override bool Enabled { get; protected set; }

    public override void StartTextComposition()
    {
        if (Enabled)
            return;

        Enabled = true;
        NativeWnd.EnableIME();
    }

    public override void StopTextComposition()
    {
        if (!Enabled)
            return;

        Enabled = false;
        NativeWnd.DisableIME();
    }

    public override void SetTextInputRect(ref Rectangle rect)
    {
        if (!Enabled)
            return;

        NativeWnd.SetTextInputRect(ref rect);
    }

    public void Dispose() => _nativeWnd?.Dispose();
}
