namespace RegulSaveCleaner.Structures;

public struct CleaningResult
{
    public float OldSize;
    public float NewSize;
    public string Save;
    public double TotalSecond;

    public CleaningResult(float oldSize, float newSize, double totalSecond, string save)
    {
        OldSize = oldSize;
        NewSize = newSize;
        Save = save;
        TotalSecond = totalSecond;
    }
}