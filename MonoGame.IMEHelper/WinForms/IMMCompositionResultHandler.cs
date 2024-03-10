using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using Windows.Win32;
using Windows.Win32.Globalization;
using Windows.Win32.UI.Input.Ime;

namespace MonoGame.IMEHelper;

internal abstract class IMMCompositionResultHandler
{
    internal HIMC IMEHandle { get; set; }

    public IME_COMPOSITION_STRING Flag { get; private set; }

    internal IMMCompositionResultHandler(IME_COMPOSITION_STRING flag)
    {
        Flag = flag;
        IMEHandle = (HIMC)0;
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

internal class IMMCompositionString : IMMCompositionResultHandler, IEnumerable<byte>
{
    public int Length { get; private set; }

    public byte[] Values { get; private set; } = [];

    public byte this[int index] => Values[index];

    internal IMMCompositionString(IME_COMPOSITION_STRING flag) : base(flag)
    {
        Clear();
    }

    public IEnumerator<byte> GetEnumerator() => ((IEnumerable<byte>)Values).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override string ToString() => Length switch
    {
        <= 0 => string.Empty,
        _ => Encoding.Unicode.GetString(Values, 0, Length)
    };

    internal void Clear()
    {
        Values = [];
        Length = 0;
    }

    internal override unsafe void Update()
    {
        Length = PInvoke.ImmGetCompositionString(IMEHandle, Flag, null, 0);
        Values = new byte[Length];
        fixed (byte* buffer = Values)
            Length = PInvoke.ImmGetCompositionString(IMEHandle, Flag, buffer, (uint)Length);
    }
}

internal class IMMCompositionInt : IMMCompositionResultHandler
{
    public int Value { get; private set; }

    internal IMMCompositionInt(IME_COMPOSITION_STRING flag) : base(flag) { }

    public override string ToString() => Value.ToString();

    internal override unsafe void Update()
    {
        Value = PInvoke.ImmGetCompositionString(IMEHandle, Flag, null, 0);
    }
}