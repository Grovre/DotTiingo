using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotTiingo.Model.Rest;

public record EndOfDayMeta(
    string Ticker,
    string Name,
    string ExchangeCode,
    string Description,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate);