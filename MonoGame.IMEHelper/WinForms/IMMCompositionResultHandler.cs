using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

using Windows.Win32;
using Windows.Win32.Globalization;
using Windows.Win32.UI.Input.Ime;

namespace MonoGame.IMEHelper;

internal abstract class IMMCompositionResultHandler
{
    internal HIMC HIMC { get; set; }

    public IME_COMPOSITION_STRING Flag { get; private set; }

    internal IMMCompositionResultHandler(IME_COMPOSITION_STRING flag)
    {
        Flag = flag;
        HIMC = (HIMC)0;
    }

    internal virtual void Update() { }

    internal bool Update(IME_COMPOSITION_STRING lParam)
    {
        if ((lParam & Flag) == Flag)
        {
            Update();
            return true;
        }
        return false;
    }
}

internal sealed class IMMCompositionString : IMMCompositionResultHandler, IEnumerable<byte>
{
    public int Length { get; private set; }

    public byte[] Values { get; private set; }

    public byte this[int index] => Values[index];

    internal IMMCompositionString(IME_COMPOSITION_STRING flag) : base(flag) => Clear();

    public IEnumerator<byte> GetEnumerator()
    {
        foreach (byte b in Values)
            yield return b;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override string ToString() => Length switch
    {
        <= 0 => string.Empty,
        _ => Encoding.Unicode.GetString(Values, 0, Length)
    };

    [MemberNotNull(nameof(Values))]
    internal void Clear()
    {
        Values = [];
        Length = 0;
    }

    internal override unsafe void Update()
    {
        Length = PInvoke.ImmGetCompositionString(HIMC, Flag, null, 0);
        Values = new byte[Length];
        fixed (byte* ptr = Values)
            _ = PInvoke.ImmGetCompositionString(HIMC, Flag, ptr, (uint)Length);
    }
}

internal sealed class IMMCompositionInt : IMMCompositionResultHandler
{
    public int Value { get; private set; }

    internal IMMCompositionInt(IME_COMPOSITION_STRING flag) : base(flag) { }

    public override string ToString() => $"{Value}";

    internal override unsafe void Update() => Value = PInvoke.ImmGetCompositionString(HIMC, Flag, null, 0);
}