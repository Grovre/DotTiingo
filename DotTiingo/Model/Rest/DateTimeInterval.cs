using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotTiingo.Model.Rest;

public record DateTimeInterval(DateTimeOffset Start, DateTimeOffset End)
{
}
