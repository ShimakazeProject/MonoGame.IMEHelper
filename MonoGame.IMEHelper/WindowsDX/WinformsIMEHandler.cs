using System;

using Microsoft.Xna.Framework;

namespace MonoGame.IMEHelper;

/// <summary>
/// Integrate IME to XNA framework.
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
public class WinFormsIMEHandler : IMEHandler, IDisposable
{
    private IMENativeWindow? _nativeWnd;
    private IMENativeWindow NativeWnd => _nativeWnd ?? throw new InvalidOperationException();

    public WinFormsIMEHandler(Game game, bool showDefaultIMEWindow = false) : base(game, showDefaultIMEWindow)
    {
    }

    protected override void PlatformInitialize()
    {
        _nativeWnd = new IMENativeWindow(this, GameInstance.Window.Handle, ShowDefaultIMEWindow);

        GameInstance.Exiting += (_, _) => this.Dispose();
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

    public override string[] Candidates { get { return NativeWnd.Candidates; } }
    public override uint CandidatesPageSize { get { return NativeWnd.CandidatesPageSize; } }
    public override uint CandidatesPageStart { get { return NativeWnd.CandidatesPageStart; } }
    public override uint CandidatesSelection { get { return NativeWnd.CandidatesSelection; } }

    public override string Composition { get { return NativeWnd.CompositionString; } }
    public override string CompositionClause { get { return NativeWnd.CompositionClause; } }
    public override string CompositionRead { get { return NativeWnd.CompositionReadString; } }
    public override string CompositionReadClause { get { return NativeWnd.CompositionReadClause; } }
    public override int CompositionCursorPos { get { return NativeWnd.CompositionCursorPos; } }

    public override string Result { get { return NativeWnd.ResultString; } }
    public override string ResultClause { get { return NativeWnd.ResultClause; } }
    public override string ResultRead { get { return NativeWnd.ResultReadString; } }
    public override string ResultReadClause { get { return NativeWnd.ResultReadClause; } }

    public override CompositionAttributes GetCompositionAttr(int charIndex)
    {
        return NativeWnd.GetCompositionAttr(charIndex);
    }

    public override CompositionAttributes GetCompositionReadAttr(int charIndex)
    {
        return NativeWnd.GetCompositionReadAttr(charIndex);
    }

    /// <summary>
    /// Dispose everything
    /// </summary>
    public void Dispose()
    {
        NativeWnd.Dispose();
    }
}