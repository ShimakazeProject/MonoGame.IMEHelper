using System;
using System.Diagnostics;
using System.Windows.Forms;

using Microsoft.Xna.Framework;

using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Globalization;
using Windows.Win32.UI.Input.Ime;

namespace MonoGame.IMEHelper;

/// <summary>
/// Native window class that handles IME.
/// </summary>
internal sealed class IMENativeWindow : NativeWindow, IDisposable
{
    private readonly WinFormsIMEHandler _imeHandler;
    private readonly IMMCompositionString _compstr;
    private readonly IMMCompositionString _compclause;
    private readonly IMMCompositionString _compattr;
    private readonly IMMCompositionString _compread;
    private readonly IMMCompositionString _compreadclause;
    private readonly IMMCompositionString _compreadattr;
    private readonly IMMCompositionString _resstr;
    private readonly IMMCompositionString _resclause;
    private readonly IMMCompositionString _resread;
    private readonly IMMCompositionString _resreadclause;
    private readonly IMMCompositionInt _compcurpos;

    private bool _disposed;

    private HIMC _hIMC;

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
    public string[] Candidates { get; private set; }

    /// <summary>
    /// First candidate index of current page
    /// </summary>
    public uint CandidatesPageStart { get; private set; }

    /// <summary>
    /// How many candidates should display per page
    /// </summary>
    public uint CandidatesPageSize { get; private set; }

    /// <summary>
    /// The selected canddiate index
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
    /// <param name="handle">Handle of the window</param>
    internal IMENativeWindow(WinFormsIMEHandler imeHandler, IntPtr handle)
    {
        _imeHandler = imeHandler;

        _hIMC = (HIMC)0;
        Candidates = [];
        _compcurpos = new IMMCompositionInt(IME_COMPOSITION_STRING.GCS_CURSORPOS);
        _compstr = new IMMCompositionString(IME_COMPOSITION_STRING.GCS_COMPSTR);
        _compclause = new IMMCompositionString(IME_COMPOSITION_STRING.GCS_COMPCLAUSE);
        _compattr = new IMMCompositionString(IME_COMPOSITION_STRING.GCS_COMPATTR);
        _compread = new IMMCompositionString(IME_COMPOSITION_STRING.GCS_COMPREADSTR);
        _compreadclause = new IMMCompositionString(IME_COMPOSITION_STRING.GCS_COMPREADCLAUSE);
        _compreadattr = new IMMCompositionString(IME_COMPOSITION_STRING.GCS_COMPREADATTR);
        _resstr = new IMMCompositionString(IME_COMPOSITION_STRING.GCS_RESULTSTR);
        _resclause = new IMMCompositionString(IME_COMPOSITION_STRING.GCS_RESULTCLAUSE);
        _resread = new IMMCompositionString(IME_COMPOSITION_STRING.GCS_RESULTREADSTR);
        _resreadclause = new IMMCompositionString(IME_COMPOSITION_STRING.GCS_RESULTREADCLAUSE);


        AssignHandle(handle);
    }

    /// <summary>
    /// Enable the IME
    /// </summary>
    public void EnableIME()
    {
        IsEnabled = true;

        PInvoke.DestroyCaret();
        PInvoke.CreateCaret((HWND)Handle, null, 1, 1);

        _hIMC = PInvoke.ImmGetContext((HWND)Handle);
        if (_hIMC != 0)
        {
            PInvoke.ImmAssociateContext((HWND)Handle, _hIMC);
            PInvoke.ImmReleaseContext((HWND)Handle, _hIMC);
            return;
        }

        // This fix the bug that _context is 0 on fullscreen mode.
        ImeContext.Enable(Handle);
    }

    /// <summary>
    /// Disable the IME
    /// </summary>
    public void DisableIME()
    {
        IsEnabled = false;

        PInvoke.DestroyCaret();

        PInvoke.ImmAssociateContext((HWND)Handle, (HIMC)0);
    }

    public void SetTextInputRect(in Rectangle rect)
    {
        _hIMC = PInvoke.ImmGetContext((HWND)Handle);

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

        PInvoke.ImmSetCandidateWindow(_hIMC, candidateForm);
        PInvoke.SetCaretPos(rect.X, rect.Y);

        PInvoke.ImmReleaseContext((HWND)Handle, _hIMC);
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

    private volatile uint _isIMEChar;
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
                IMEStartComposion(msg.LParam.ToInt32());
                return;
            case PInvoke.WM_IME_COMPOSITION:
                IMESetContextForAll();
                IMEComposition(msg.LParam.ToInt32());
                PInvoke.ImmReleaseContext((HWND)Handle, _hIMC);
                break;
            case PInvoke.WM_IME_ENDCOMPOSITION:
                IMESetContextForAll();
                IMEEndComposition(msg.LParam.ToInt32());
                PInvoke.ImmReleaseContext((HWND)Handle, _hIMC);
                break;
            case PInvoke.WM_IME_CHAR: // 输入法合成
                _isIMEChar++;
                CharEvent(msg.WParam.ToInt32());
                break;
            case PInvoke.WM_CHAR: // 直接输入
                if (_isIMEChar is 0)
                    CharEvent(msg.WParam.ToInt32());
                else
                    _isIMEChar--;
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
        _hIMC = PInvoke.ImmGetContext((HWND)Handle);

        _compcurpos.HIMC = _hIMC;
        _compstr.HIMC = _hIMC;
        _compclause.HIMC = _hIMC;
        _compattr.HIMC = _hIMC;
        _compread.HIMC = _hIMC;
        _compreadclause.HIMC = _hIMC;
        _compreadattr.HIMC = _hIMC;
        _resstr.HIMC = _hIMC;
        _resclause.HIMC = _hIMC;
        _resread.HIMC = _hIMC;
        _resreadclause.HIMC = _hIMC;
    }

    private void IMENotify(uint wParam)
    {
        switch (wParam)
        {
            case PInvoke.IMN_SETOPENSTATUS:
                _hIMC = PInvoke.ImmGetContext((HWND)Handle);
                IsIMEOpen = PInvoke.ImmGetOpenStatus(_hIMC);
                Debug.WriteLine($"IsIMEOpen: {IsIMEOpen}");
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
            default:
                break;
        }
    }

    private unsafe void IMEChangeCandidate()
    {
        _hIMC = PInvoke.ImmGetContext((HWND)Handle);

        uint length = PInvoke.ImmGetCandidateList(_hIMC, 0, (CANDIDATELIST*)0, 0);
        CANDIDATELIST cList = new();
        if (length > 0)
        {
            _ = PInvoke.ImmGetCandidateList(_hIMC, 0, &cList, length);

            CandidatesSelection = cList.dwSelection;
            CandidatesPageStart = cList.dwPageStart;
            CandidatesPageSize = cList.dwPageSize;

            if (cList.dwCount > 1)
            {
                Candidates = new string[cList.dwCount];
                for (int i = 0; i < cList.dwCount; i++)
                    Candidates[i] = new((char*)(&cList + cList.dwOffset[i]));

                _imeHandler.OnTextComposition(CompositionString, CompositionCursorPos, new CandidateList
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

        PInvoke.ImmReleaseContext((HWND)Handle, _hIMC);
    }

    private void IMECloseCandidate()
    {
        CandidatesSelection = CandidatesPageStart = CandidatesPageSize = 0;
        Candidates = [];

        _imeHandler.OnTextComposition(CompositionString, CompositionCursorPos, null);
    }

    private void IMEStartComposion(int lParam)
    {
        ClearComposition();
        ClearResult();

        _imeHandler.OnTextComposition(string.Empty, 0);
    }

    private void IMEComposition(int lParam)
    {
        if (_compstr.Update((IME_COMPOSITION_STRING)lParam))
        {
            _compclause.Update();
            _compattr.Update();
            _compread.Update();
            _compreadclause.Update();
            _compreadattr.Update();
            _compcurpos.Update();

            _imeHandler.OnTextComposition(CompositionString, CompositionCursorPos);
        }
    }

    private void IMEEndComposition(int lParam)
    {
        ClearComposition();

        if (_resstr.Update((IME_COMPOSITION_STRING)lParam))
        {
            _resclause.Update();
            _resread.Update();
            _resreadclause.Update();

            _imeHandler.OnTextComposition(string.Empty, 0);
        }
    }

    private void CharEvent(int wParam)
    {
        var charInput = (char)wParam;

        var key = Microsoft.Xna.Framework.Input.Keys.None;
        if (!char.IsSurrogate(charInput))
            key = (Microsoft.Xna.Framework.Input.Keys)(PInvoke.VkKeyScanEx(charInput, (Windows.Win32.UI.TextServices.HKL)InputLanguage.CurrentInputLanguage.Handle) & 0xff);

        Debug.WriteLine(key);

        _imeHandler.OnTextInput(charInput, key);

        if (IsEnabled)
            IMECloseCandidate();
    }

    #endregion
}
