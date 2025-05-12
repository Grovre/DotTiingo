using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotTiingo.Model;

public record CryptoMeta(
    string Ticker,
    string BaseCurrency,
    string QuoteCurrency,
    string Name,
    string? Description);