using System;
using Microsoft.Xna.Framework.Input;

namespace MonoGame.IMEHelper;

public class TextInputEventArgs : EventArgs
{
    public TextInputEventArgs(char character, Keys key = Keys.None)
    {
        Character = character;
        Key = key;
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public readonly char Character;
    // ReSharper disable once MemberCanBePrivate.Global
    public readonly Keys Key;
}