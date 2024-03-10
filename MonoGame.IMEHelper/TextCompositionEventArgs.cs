using System;

namespace MonoGame.IMEHelper;

/// <summary>
/// Arguments for the <see cref="IMEHandler.TextComposition" /> event.
/// </summary>
public class TextCompositionEventArgs : EventArgs
{
    /// <summary>
    /// Initialize a TextCompositionEventArgs
    /// </summary>
    /// <param name="compositedText"></param>
    /// <param name="cursorPosition"></param>
    /// <param name="candidateList"></param>
    public TextCompositionEventArgs(string compositedText, int cursorPosition, CandidateList? candidateList = null)
    {
        CompositedText = compositedText;
        CursorPosition = cursorPosition;
        CandidateList = candidateList;
    }

    /// <summary>
    /// The full string as it's composited by the IMM.
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public string CompositedText { get; private set; }

    /// <summary>
    /// The position of the cursor inside the composited string.
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public int CursorPosition { get; private set; }

    /// <summary>
    /// The suggested alternative texts for the composition.
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public CandidateList? CandidateList { get; private set; }

}