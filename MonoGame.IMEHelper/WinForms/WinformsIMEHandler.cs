using System;

using Microsoft.Xna.Framework;

using Windows.Win32.Foundation;

namespace MonoGame.IMEHelper;

/// <summary>
/// Integrate IME to XNA framework.
/// </summary>
public class WinFormsIMEHandler : IMEHandler, IDisposable
{
    private readonly IMENativeWindow _nativeWnd;
    private bool _disposedValue;

    public WinFormsIMEHandler(Game game) : base(game)
    {
        _nativeWnd = new IMENativeWindow(this, (HWND)game.Window.Handle);

        game.Exiting += (o, e) => Dispose();
    }

    public override bool Enabled { get; protected set; }

    public override void StartTextComposition()
    {
        if (Enabled)
            return;

        Enabled = true;
        _nativeWnd.EnableIME();
    }

    public override void StopTextComposition()
    {
        if (!Enabled)
            return;

        Enabled = false;
        _nativeWnd.DisableIME();
    }

    public override void SetTextInputRect(in Rectangle rect)
    {
        if (!Enabled)
            return;

        _nativeWnd.SetTextInputRect(rect);
    }

    public override string[] Candidates => _nativeWnd.Candidates;
    public override uint CandidatesPageSize => _nativeWnd.CandidatesPageSize;
    public override uint CandidatesPageStart => _nativeWnd.CandidatesPageStart;
    public override uint CandidatesSelection => _nativeWnd.CandidatesSelection;

    public override string Composition => _nativeWnd.CompositionString;
    public override string CompositionClause => _nativeWnd.CompositionClause;
    public override string CompositionRead => _nativeWnd.CompositionReadString;
    public override string CompositionReadClause => _nativeWnd.CompositionReadClause;
    public override int CompositionCursorPos => _nativeWnd.CompositionCursorPos;

    public override string Result => _nativeWnd.ResultString;
    public override string ResultClause => _nativeWnd.ResultClause;
    public override string ResultRead => _nativeWnd.ResultReadString;
    public override string ResultReadClause => _nativeWnd.ResultReadClause;

    public override CompositionAttributes GetCompositionAttr(int charIndex) => _nativeWnd.GetCompositionAttr(charIndex);

    public override CompositionAttributes GetCompositionReadAttr(int charIndex) => _nativeWnd.GetCompositionReadAttr(charIndex);

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _nativeWnd.Dispose();
            }

            // TODO: 释放未托管的资源(未托管的对象)并重写终结器
            // TODO: 将大型字段设置为 null
            _disposedValue = true;
        }
    }

    // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
    // ~WinFormsIMEHandler()
    // {
    //     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}