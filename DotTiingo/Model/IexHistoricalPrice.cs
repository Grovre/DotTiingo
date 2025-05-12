using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotTiingo.Model;

public record IexHistoricalPrice(
    DateTime Date,
    float Open,
    float High,
    float Low,
    float Close,
    long Volume);