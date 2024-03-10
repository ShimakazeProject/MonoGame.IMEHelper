namespace MonoGame.IMEHelper;

public struct CandidateList
{
    /// <summary>
    /// Array of the candidates
    /// </summary>
    public string[] Candidates;

    /// <summary>
    /// First candidate index of current page
    /// </summary>
    public uint CandidatesPageStart;

    /// <summary>
    /// How many candidates should display per page
    /// </summary>
    public uint CandidatesPageSize;

    /// <summary>
    /// The selected candidate index
    /// </summary>
    public uint CandidatesSelection;
}