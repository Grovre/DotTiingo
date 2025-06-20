using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotTiingo.Model.Rest;

public record IexHistoricalPrice(
    DateTimeOffset Date,
    float Open,
    float High,
    float Low,
    float Close,
    float IexVolume);