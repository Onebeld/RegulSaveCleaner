using Avalonia.Collections;

namespace RegulSaveCleaner.Structures;

public class GameSaveResource
{
    public string Id { get; set; } = string.Empty;
    public AvaloniaList<ProhibitedResource> ProhibitedResources { get; set; } = new();
}