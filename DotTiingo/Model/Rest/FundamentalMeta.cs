using System;
using System.Text.Json.Serialization;

namespace DotTiingo.Model.Rest;

/// <summary>
/// Represents metadata for a ticker available in the fundamentals dataset.
/// </summary>
public record FundamentalMeta(
    string PermaTicker,
    string Ticker,
    string Name,
    bool IsActive,
    bool? IsADR,
    string? Sector,
    string? Industry,
    [property: JsonConverter(typeof(LenientNullableInt32Converter))]
    int? SicCode,
    string? SicSector,
    string? SicIndustry,
    string? ReportingCurrency,
    string? Location,
    string? CompanyWebsite,
    string? SecFilingWebsite,
    DateTimeOffset? StatementLastUpdated,
    DateTimeOffset? DailyLastUpdated);
