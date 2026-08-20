using System;

namespace DotTiingo.Model.Rest;

/// <summary>
/// Represents daily fundamental metrics for a security.
/// </summary>
public record FundamentalDailyMetric(
    DateTimeOffset Date,
    double? MarketCap,
    double? EnterpriseVal,
    double? PeRatio,
    double? PbRatio,
    double? TrailingPEG1Y);
