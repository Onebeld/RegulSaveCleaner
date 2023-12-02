namespace RegulSaveCleaner.Structures;

public struct CleaningResult(float oldSize, float newSize, double totalSecond, string save)
{
    public readonly float OldSize = oldSize;
    public readonly float NewSize = newSize;
    public readonly string Save = save;
    public readonly double TotalSecond = totalSecond;
}