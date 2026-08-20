using System;

namespace DotTiingo.Model.Rest;

/// <summary>
/// Represents a data point with code and numeric value within a financial statement.
/// </summary>
public record FundamentalDataPoint(
    string DataCode,
    double? Value);

/// <summary>
/// Represents the statement data broken out across financial statements.
/// </summary>
public record FundamentalStatementData(
    FundamentalDataPoint[]? BalanceSheet,
    FundamentalDataPoint[]? IncomeStatement,
    FundamentalDataPoint[]? CashFlow,
    FundamentalDataPoint[]? Overview);

/// <summary>
/// Represents quarterly or annual statement data for a security.
/// </summary>
public record FundamentalStatement(
    DateTimeOffset Date,
    int Quarter,
    int Year,
    FundamentalStatementData StatementData);
