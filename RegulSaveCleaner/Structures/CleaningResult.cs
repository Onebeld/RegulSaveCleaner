namespace RegulSaveCleaner.Structures;

public struct CleaningResult
{
    public readonly float OldSize;
    public readonly float NewSize;
    public readonly string Save;
    public readonly double TotalSecond;

    public CleaningResult(float oldSize, float newSize, double totalSecond, string save)
    {
        OldSize = oldSize;
        NewSize = newSize;
        Save = save;
        TotalSecond = totalSecond;
    }
}