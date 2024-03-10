using System;
using System.Windows.Forms;

using Microsoft.Xna.Framework;

using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Globalization;
using Windows.Win32.UI.Input.Ime;

namespace MonoGame.IMEHelper;

/// <summary>
/// Native window class that Handles IME.
/// </summary>
internal sealed class IMENativeWindow : NativeWindow, IDisposable
{
    private bool _disposed;

    private HIMC _context;

    /// <summary>
    /// Gets the state if the IME should be enabled
    /// </summary>
    public bool IsEnabled { get; private set; }

    public bool IsIMEOpen { get; private set; }

    /// <summary>
    /// Constructor, must be called when the window create.
    /// </summary>
    /// <param name="imeHandler"></param>
    /// <param name="hwnd">Handle of the window</param>
    internal IMENativeWindow(HWND hwnd)
    {
        AssignHandle(hwnd);
    }

    /// <summary>
    /// Enable the IME
    /// </summary>
    public void EnableIME()
    {
        IsEnabled = true;

        PInvoke.DestroyCaret();
        PInvoke.CreateCaret((HWND)Handle, null, 1, 1);

        _context = PInvoke.ImmGetContext((HWND)Handle);
        if (_context != IntPtr.Zero)
        {
            PInvoke.ImmAssociateContext((HWND)Handle, _context);
            PInvoke.ImmReleaseContext((HWND)Handle, _context);
            return;
        }

        // This fix _context is 0 on fullscreen mode.
        ImeContext.Enable((HWND)Handle);
    }

    /// <summary>
    /// Disable the IME
    /// </summary>
    public void DisableIME()
    {
        IsEnabled = false;

        PInvoke.DestroyCaret();

        PInvoke.ImmAssociateContext((HWND)Handle, default);

        ImeContext.Disable((HWND)Handle);
    }

    public void SetTextInputRect(ref Rectangle rect)
    {
        _context = PInvoke.ImmGetContext((HWND)Handle);

        CANDIDATEFORM candidateForm = new()
        {
            dwIndex = 0,
            dwStyle = PInvoke.CFS_CANDIDATEPOS,
            ptCurrentPos =
            {
                X = rect.X,
                Y = rect.Y,
            },
            rcArea = new(),
        };

        PInvoke.ImmSetCandidateWindow(_context, candidateForm);
        PInvoke.SetCaretPos(rect.X, rect.Y);

        PInvoke.ImmReleaseContext((HWND)Handle, _context);
    }

    /// <summary>
    /// Dispose everything
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            ReleaseHandle();
            _disposed = true;
        }
    }

    protected override void WndProc(ref Message msg)
    {
        switch ((uint)msg.Msg)
        {
            case PInvoke.WM_IME_SETCONTEXT:
                if (msg.WParam.ToInt32() == 1)
                {
                    if (IsEnabled)
                        EnableIME();
                }

                break;
            case PInvoke.WM_INPUTLANGCHANGE:
                return;
            case PInvoke.WM_IME_NOTIFY:
                IMENotify((uint)msg.WParam);
                break;
            case PInvoke.WM_IME_STARTCOMPOSITION:
                return;
            case PInvoke.WM_IME_COMPOSITION:
                PInvoke.ImmReleaseContext((HWND)Handle, _context);
                break;
            case PInvoke.WM_IME_ENDCOMPOSITION:
                PInvoke.ImmReleaseContext((HWND)Handle, _context);
                break;
            case PInvoke.WM_IME_CHAR:
                break;
        }

        base.WndProc(ref msg);
    }


    private void IMENotify(uint wParam)
    {
        switch (wParam)
        {
            case PInvoke.IMN_SETOPENSTATUS:
                _context = PInvoke.ImmGetContext((HWND)Handle);
                IsIMEOpen = PInvoke.ImmGetOpenStatus(_context);
                System.Diagnostics.Trace.WriteLine($"IsIMEOpen: {IsIMEOpen}");
                break;
            case PInvoke.IMN_OPENCANDIDATE:
            case PInvoke.IMN_CHANGECANDIDATE:
                break;
            case PInvoke.IMN_CLOSECANDIDATE:
                break;
            case PInvoke.IMN_PRIVATE:
                break;
        }
    }
}