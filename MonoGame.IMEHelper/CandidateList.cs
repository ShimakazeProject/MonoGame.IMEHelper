namespace MonoGame.IMEHelper;

public struct CandidateList
{
    /// <summary>
    /// Array of the candidates
    /// </summary>
    public string[] Candidates { get; set; }

    /// <summary>
    /// First candidate index of current page
    /// </summary>
    public uint CandidatesPageStart { get; set; }

    /// <summary>
    /// How many candidates should display per page
    /// </summary>
    public uint CandidatesPageSize { get; set; }

    /// <summary>
    /// The selected canddiate index
    /// </summary>
    public uint CandidatesSelection { get; set; }
}
