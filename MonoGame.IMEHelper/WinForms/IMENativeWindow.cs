using Microsoft.Xna.Framework;

using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Globalization;
using Windows.Win32.UI.Input.Ime;
using Windows.Win32.UI.TextServices;

namespace MonoGame.IMEHelper;

/// <summary>
/// Native window class that Handles IME.
/// </summary>
internal sealed class IMENativeWindow : NativeWindow, IDisposable
{
    private readonly WinFormsIMEHandler _imeHandler;

    private readonly IMMCompositionString
        _compstr,
        _compclause,
        _compattr,
        _compread,
        _compreadclause,
        _compreadattr,
        _resstr,
        _resclause,
        _resread,
        _resreadclause;

    private readonly IMMCompositionInt _compcurpos;

    private bool _disposed;

    private HIMC _context;

    /// <summary>
    /// Gets the state if the IME should be enabled
    /// </summary>
    public bool IsEnabled { get; private set; }

    public bool IsIMEOpen { get; private set; }

    /// <summary>
    /// Composition String
    /// </summary>
    public string CompositionString => _compstr.ToString();

    /// <summary>
    /// Composition Clause
    /// </summary>
    public string CompositionClause => _compclause.ToString();

    /// <summary>
    /// Composition String Reads
    /// </summary>
    public string CompositionReadString => _compread.ToString();

    /// <summary>
    /// Composition Clause Reads
    /// </summary>
    public string CompositionReadClause => _compreadclause.ToString();

    /// <summary>
    /// Result String
    /// </summary>
    public string ResultString => _resstr.ToString();

    /// <summary>
    /// Result Clause
    /// </summary>
    public string ResultClause => _resclause.ToString();

    /// <summary>
    /// Result String Reads
    /// </summary>
    public string ResultReadString => _resread.ToString();

    /// <summary>
    /// Result Clause Reads
    /// </summary>
    public string ResultReadClause => _resreadclause.ToString();

    /// <summary>
    /// Caret position of the composition
    /// </summary>
    public int CompositionCursorPos => _compcurpos.Value;

    /// <summary>
    /// Array of the candidates
    /// </summary>
    public string?[] Candidates { get; private set; }

    /// <summary>
    /// First candidate index of current page
    /// </summary>
    public uint CandidatesPageStart { get; private set; }

    /// <summary>
    /// How many candidates should display per page
    /// </summary>
    public uint CandidatesPageSize { get; private set; }

    /// <summary>
    /// The selected candidate index
    /// </summary>
    public uint CandidatesSelection { get; private set; }

    /// <summary>
    /// Get the composition attribute at character index.
    /// </summary>
    /// <param name="index">Character Index</param>
    /// <returns>Composition Attribute</returns>
    public CompositionAttributes GetCompositionAttr(int index) => (CompositionAttributes)_compattr[index];

    /// <summary>
    /// Get the composition read attribute at character index.
    /// </summary>
    /// <param name="index">Character Index</param>
    /// <returns>Composition Attribute</returns>
    public CompositionAttributes GetCompositionReadAttr(int index) => (CompositionAttributes)_compreadattr[index];

    /// <summary>
    /// Constructor, must be called when the window create.
    /// </summary>
    /// <param name="imeHandler"></param>
    /// <param name="hwnd">Handle of the window</param>
    internal IMENativeWindow(WinFormsIMEHandler imeHandler, HWND hwnd)
    {
        _imeHandler = imeHandler;

        Candidates = [];
        _compcurpos = new IMMCompositionInt(IME_COMPOSITION_STRING.GCS_CURSORPOS);
        _compstr = new IMMCompositionString(IME_COMPOSITION_STRING.GCS_COMPSTR);
        _compclause = new IMMCompositionString(IME_COMPOSITION_STRING.GCS_COMPREADCLAUSE);
        _compattr = new IMMCompositionString(IME_COMPOSITION_STRING.GCS_COMPATTR);
        _compread = new IMMCompositionString(IME_COMPOSITION_STRING.GCS_COMPREADSTR);
        _compreadclause = new IMMCompositionString(IME_COMPOSITION_STRING.GCS_COMPREADCLAUSE);
        _compreadattr = new IMMCompositionString(IME_COMPOSITION_STRING.GCS_COMPREADATTR);
        _resstr = new IMMCompositionString(IME_COMPOSITION_STRING.GCS_RESULTSTR);
        _resclause = new IMMCompositionString(IME_COMPOSITION_STRING.GCS_RESULTCLAUSE);
        _resread = new IMMCompositionString(IME_COMPOSITION_STRING.GCS_RESULTREADSTR);
        _resreadclause = new IMMCompositionString(IME_COMPOSITION_STRING.GCS_RESULTREADCLAUSE);

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

        //var candidateForm = new CANDIDATEFORM(new PInvoke.Point(rect.X, rect.Y));
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
                IMEStartComposition();
                return;
            case PInvoke.WM_IME_COMPOSITION:
                IMESetContextForAll();
                IMEComposition((IME_COMPOSITION_STRING)msg.LParam);
                PInvoke.ImmReleaseContext((HWND)Handle, _context);
                break;
            case PInvoke.WM_IME_ENDCOMPOSITION:
                IMESetContextForAll();
                IMEEndComposition((IME_COMPOSITION_STRING)msg.LParam);
                PInvoke.ImmReleaseContext((HWND)Handle, _context);
                break;
            case PInvoke.WM_IME_CHAR:
                CharEvent(msg.WParam.ToInt32());
                break;
        }

        base.WndProc(ref msg);
    }

    private void ClearComposition()
    {
        _compstr.Clear();
        _compclause.Clear();
        _compattr.Clear();
        _compread.Clear();
        _compreadclause.Clear();
        _compreadattr.Clear();
    }

    private void ClearResult()
    {
        _resstr.Clear();
        _resclause.Clear();
        _resread.Clear();
        _resreadclause.Clear();
    }

    #region IME Message Handlers

    private void IMESetContextForAll()
    {
        _context = PInvoke.ImmGetContext((HWND)Handle);

        _compcurpos.IMEHandle = _context;
        _compstr.IMEHandle = _context;
        _compclause.IMEHandle = _context;
        _compattr.IMEHandle = _context;
        _compread.IMEHandle = _context;
        _compreadclause.IMEHandle = _context;
        _compreadattr.IMEHandle = _context;
        _resstr.IMEHandle = _context;
        _resclause.IMEHandle = _context;
        _resread.IMEHandle = _context;
        _resreadclause.IMEHandle = _context;
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
                IMEChangeCandidate();
                break;
            case PInvoke.IMN_CLOSECANDIDATE:
                IMECloseCandidate();
                break;
            case PInvoke.IMN_PRIVATE:
                break;
        }
    }

    private unsafe void IMEChangeCandidate()
    {
        _context = PInvoke.ImmGetContext((HWND)Handle);

        CANDIDATELIST candiDataList = new();
        CANDIDATELIST* pCandiDataList = &candiDataList;

        uint length = PInvoke.ImmGetCandidateList(_context, 0, pCandiDataList, 0);
        if (length > 0)
        {
            CandidatesSelection = candiDataList.dwSelection;
            CandidatesPageStart = candiDataList.dwPageStart;
            CandidatesPageSize = candiDataList.dwPageSize;

            if (candiDataList.dwCount > 1)
            {
                Candidates = new string?[candiDataList.dwCount];

                for (int i = 0; i < candiDataList.dwCount; i++)
                {
                    uint sOffset = candiDataList.dwOffset[i];
                    Candidates[i] = Marshal.PtrToStringUni((nint)(pCandiDataList + sOffset));
                }

                _imeHandler.OnTextComposition(CompositionString, CompositionCursorPos,
                    new CandidateList
                    {
                        Candidates = Candidates,
                        CandidatesPageStart = CandidatesPageStart,
                        CandidatesPageSize = CandidatesPageSize,
                        CandidatesSelection = CandidatesSelection
                    });
            }
            else
                IMECloseCandidate();
        }

        PInvoke.ImmReleaseContext((HWND)Handle, _context);
    }

    private void IMECloseCandidate()
    {
        CandidatesSelection = CandidatesPageStart = CandidatesPageSize = 0;
        Candidates = [];

        _imeHandler.OnTextComposition(CompositionString, CompositionCursorPos);
    }

    private void IMEStartComposition()
    {
        ClearComposition();
        ClearResult();

        _imeHandler.OnTextComposition(string.Empty, 0);
    }

    private void IMEComposition(IME_COMPOSITION_STRING lParam)
    {
        if (!_compstr.Update(lParam))
            return;

        _compclause.Update();
        _compattr.Update();
        _compread.Update();
        _compreadclause.Update();
        _compreadattr.Update();
        _compcurpos.Update();

        _imeHandler.OnTextComposition(CompositionString, CompositionCursorPos);
    }

    private void IMEEndComposition(IME_COMPOSITION_STRING lParam)
    {
        ClearComposition();

        if (!_resstr.Update(lParam))
            return;

        _resclause.Update();
        _resread.Update();
        _resreadclause.Update();

        _imeHandler.OnTextComposition(string.Empty, 0);
    }

    private void CharEvent(int wParam)
    {
        var charInput = (char)wParam;

        var key = Microsoft.Xna.Framework.Input.Keys.None;
        if (!char.IsSurrogate(charInput))
            key = (Microsoft.Xna.Framework.Input.Keys)(PInvoke.VkKeyScanEx(charInput, (HKL)InputLanguage.CurrentInputLanguage.Handle) & 0xff);

        _imeHandler.OnTextInput(charInput, key);

        if (IsEnabled)
            IMECloseCandidate();
    }

    #endregion
}