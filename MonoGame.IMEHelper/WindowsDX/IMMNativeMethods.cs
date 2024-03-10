using System;
using System.Runtime.InteropServices;

namespace MonoGame.IMEHelper;

internal static class IMM
{
    #region Constants

    public const int KeyDown = 0x0100, Char = 0x0102;

    // ReSharper disable InconsistentNaming
    public const int
        GCSCompReadStr = 0x0001,
        GCSCompReadAttr = 0x0002,
        GCSCompReadClause = 0x0004,
        GCSCompStr = 0x0008,
        GCSCompAttr = 0x0010,
        GCSCompClause = 0x0020,
        GCSCursorPos = 0x0080,
        GCSDeltaStart = 0x0100,
        GCSResultReadStr = 0x0200,
        GCSResultReadClause = 0x0400,
        GCSResultStr = 0x0800,
        GCSResultClause = 0x1000;
    // ReSharper restore InconsistentNaming

    public const int
        ImeStartComposition = 0x010D,
        ImeEndComposition = 0x010E,
        ImeComposition = 0x010F,
        ImeKeyLast = 0x010F,
        ImeSetContext = 0x0281,
        ImeNotify = 0x0282,
        ImeControl = 0x0283,
        ImeCompositionFull = 0x0284,
        ImeSelect = 0x0285,
        ImeChar = 0x286,
        ImeRequest = 0x0288,
        ImeKeyDown = 0x0290,
        ImeKeyUp = 0x0291;

    public const int
        ImnCloseStatusWindow = 0x0001,
        ImnOpenStatusWindow = 0x0002,
        ImnChangeCandidate = 0x0003,
        ImnCloseCandidate = 0x0004,
        ImnOpenCandidate = 0x0005,
        ImnSetConversionMode = 0x0006,
        ImnSetSentenceMode = 0x0007,
        ImnSetOpenStatus = 0x0008,
        ImnSetCandidatePos = 0x0009,
        ImnSetCompositionFont = 0x000A,
        ImnSetCompositionWindow = 0x000B,
        ImnSetStatusWindowPos = 0x000C,
        ImnGuideLine = 0x000D,
        ImnPrivate = 0x000E;

    public const int InputLanguageChange = 0x0051;

    #endregion

    [DllImport("imm32.dll", SetLastError = true)]
    public static extern IntPtr ImmAssociateContext(IntPtr hWnd, IntPtr hIMC);

    [DllImport("imm32.dll", SetLastError = true)]
    public static extern IntPtr ImmReleaseContext(IntPtr hWnd, IntPtr hIMC);

    [DllImport("imm32.dll", CharSet = CharSet.Unicode)]
    public static extern uint ImmGetCandidateList(IntPtr hIMC, uint deIndex, IntPtr candidateList, uint dwBufLen);

    [DllImport("imm32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern int ImmGetCompositionString(IntPtr hIMC, int compositionStringFlag, IntPtr buffer, int bufferLength);

    [DllImport("imm32.dll", SetLastError = true)]
    public static extern IntPtr ImmGetContext(IntPtr hWnd);

    [DllImport("imm32.dll", SetLastError = true)]
    public static extern bool ImmGetOpenStatus(IntPtr hIMC);

    [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
    public static extern bool TranslateMessage(IntPtr message);

    [DllImport("user32.dll")]
    // ReSharper disable once IdentifierTypo
    public static extern short VkKeyScanEx(char ch, IntPtr dwhkl);

    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once InconsistentNaming
    // ReSharper disable once IdentifierTypo
    public const int CFS_CANDIDATEPOS = 64;

    [DllImport("imm32.dll", SetLastError = true)]
    public static extern bool ImmSetCandidateWindow(IntPtr hIMC, ref CandidateForm candidateForm);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool CreateCaret(IntPtr hWnd, IntPtr hBitmap, int nWidth, int nHeight);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool DestroyCaret();

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool SetCaretPos(int x, int y);

    [StructLayout(LayoutKind.Sequential)]
    public struct CandidateList
    {
        // ReSharper disable MemberCanBePrivate.Global
        public readonly uint dwSize;
        public readonly uint dwStyle;
        public readonly uint dwCount;
        public readonly uint dwSelection;
        public readonly uint dwPageStart;
        public readonly uint dwPageSize;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1, ArraySubType = UnmanagedType.U4)]
        public readonly uint[] dwOffset;
        // ReSharper restore MemberCanBePrivate.Global
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Point
    {
        // ReSharper disable MemberCanBePrivate.Global
        public readonly int X;
        public readonly int Y;
        // ReSharper restore MemberCanBePrivate.Global
        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Rect
    {
        // ReSharper disable MemberCanBePrivate.Global
        public readonly int left;
        public readonly int top;
        public readonly int right;
        public readonly int bottom;
        // ReSharper restore MemberCanBePrivate.Global
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CandidateForm
    {
        // ReSharper disable MemberCanBePrivate.Global
        public readonly uint dwIndex;
        public readonly uint dwStyle;
        public Point ptCurrentPos;
        public Rect rcArea;
        // ReSharper restore MemberCanBePrivate.Global


        public CandidateForm(Point pos)
        {
            this.dwIndex = 0;
            this.dwStyle = CFS_CANDIDATEPOS;
            this.ptCurrentPos = pos;
            this.rcArea = new Rect();
        }
    }
}