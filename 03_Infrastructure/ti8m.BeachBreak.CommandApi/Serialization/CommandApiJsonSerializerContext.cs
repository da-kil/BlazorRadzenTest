using System.Text.Json.Serialization;
using ProgrammerAL.JsonSerializerRegistrationGenerator.Attributes;

namespace ti8m.BeachBreak.CommandApi.Serialization;

/// <summary>
/// AOT-compatible JSON serialization context for Command API response types.
/// This context is automatically populated by the JsonSerializerRegistrationGenerator
/// based on [RegisterJsonSerialization] attributes throughout the codebase.
/// </summary>
[GeneratedJsonSerializerContext]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = false,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public partial class CommandApiJsonSerializerContext : JsonSerializerContext
{
    // This class will be automatically populated with [JsonSerializable] attributes
    // by the JsonSerializerRegistrationGenerator source generator.
    //
    // To register a type for serialization, add this attribute to the DTO class:
    // [RegisterJsonSerialization(typeof(CommandApiJsonSerializerContext))]
}