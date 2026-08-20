using System;

namespace DotTiingo.Model.Rest;

/// <summary>
/// Represents definition metadata for a fundamental data field.
/// </summary>
public record FundamentalDefinition(
    string DataCode,
    string Name,
    string Description,
    string StatementType,
    string? Units);
