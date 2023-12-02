using System.Text.Json.Serialization;

namespace RegulSaveCleaner.Core;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(RegulSettings))]
internal partial class RegulSettingsGenerationContext : JsonSerializerContext;