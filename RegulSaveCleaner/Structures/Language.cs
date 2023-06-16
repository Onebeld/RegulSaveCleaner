namespace RegulSaveCleaner.Structures;

public struct Language
{
    public string Name { get; }
    public string Key { get; }
    public string[] AdditionalKeys { get; }

    public Language(string name, string key, params string[] additionalKeys)
    {
        Name = name;
        Key = key;
        AdditionalKeys = additionalKeys;
    }
}