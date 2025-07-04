﻿using DotTiingo.Model.Rest;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotTiingo.Extensions;

internal static class DateTimeExtensions
{
    public static string ToTiingoString(this DateTime dttm) =>
        dttm.ToUniversalTime().ToString("yyyy-MM-dd");

    public static string ToTiingoString(this DateTimeOffset dttm) =>
        dttm.UtcDateTime.ToString("yyyy-MM-dd");

    public static (string StartDate, string EndDate) ToTiingoString(this DateTimeInterval dttmInt) =>
        (dttmInt.Start.UtcDateTime.ToTiingoString(), dttmInt.End.UtcDateTime.ToTiingoString());
}
