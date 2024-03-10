using System;

using Microsoft.Xna.Framework.Input;

namespace MonoGame.IMEHelper;

public class TextInputEventArgs(char character, Keys key = Keys.None) : EventArgs
{
    public char Character { get; } = character;
    public Keys Key { get; } = key;
}