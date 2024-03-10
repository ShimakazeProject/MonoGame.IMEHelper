using System;

using Microsoft.Xna.Framework;

using Windows.Win32.Foundation;

namespace MonoGame.IMEHelper;

/// <summary>
/// Integrate IME to XNA framework.
/// </summary>
public class WinFormsIMEHandler(Game game) : IMEHandler(game), IDisposable
{
    private IMENativeWindow? _nativeWnd;
    private IMENativeWindow NativeWnd => _nativeWnd ?? throw new InvalidOperationException();

    protected override void PlatformInitialize()
    {
        _nativeWnd = new IMENativeWindow(this, (HWND)GameInstance.Window.Handle);

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

    public override string?[] Candidates => NativeWnd.Candidates;
    public override uint CandidatesPageSize => NativeWnd.CandidatesPageSize;
    public override uint CandidatesPageStart => NativeWnd.CandidatesPageStart;
    public override uint CandidatesSelection => NativeWnd.CandidatesSelection;

    public override string Composition => NativeWnd.CompositionString;
    public override string CompositionClause => NativeWnd.CompositionClause;
    public override string CompositionRead => NativeWnd.CompositionReadString;
    public override string CompositionReadClause => NativeWnd.CompositionReadClause;
    public override int CompositionCursorPos => NativeWnd.CompositionCursorPos;

    public override string Result => NativeWnd.ResultString;
    public override string ResultClause => NativeWnd.ResultClause;
    public override string ResultRead => NativeWnd.ResultReadString;
    public override string ResultReadClause => NativeWnd.ResultReadClause;

    public override CompositionAttributes GetCompositionAttr(int charIndex) => NativeWnd.GetCompositionAttr(charIndex);

    public override CompositionAttributes GetCompositionReadAttr(int charIndex) => NativeWnd.GetCompositionReadAttr(charIndex);

    public void Dispose() => _nativeWnd?.Dispose();
}