using ProgrammerAL.JsonSerializerRegistrationGenerator.Attributes;
using ti8m.BeachBreak.CommandApi.Serialization;

namespace ti8m.BeachBreak.CommandApi.Dto;

/// <summary>
/// Simple error response for AOT-compatible JSON serialization.
/// </summary>
[RegisterJsonSerialization(typeof(CommandApiJsonSerializerContext))]
public sealed record ErrorResponse(string Error);

/// <summary>
/// Insufficient permissions error response for AOT-compatible JSON serialization.
/// </summary>
[RegisterJsonSerialization(typeof(CommandApiJsonSerializerContext))]
public sealed record InsufficientPermissionsResponse(
    string Error,
    string[] RequiredPolicies,
    string[] RequiredRoles);