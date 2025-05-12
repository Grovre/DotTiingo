using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotTiingo.Model;

public record PriceData(
    float Open,
    float High,
    float Low,
    float Close,
    DateTime Date,
    float TradesDone,
    float Volume,
    float VolumeNotional);