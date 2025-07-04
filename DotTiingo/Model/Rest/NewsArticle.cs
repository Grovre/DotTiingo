﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotTiingo.Model.Rest;

public record NewsArticle(
    int Id,
    string Title,
    string Url,
    string Description,
    DateTimeOffset PublishedDate,
    DateTimeOffset CrawlDate,
    string Source,
    string[] Tickers,
    string[] Tags)
{
}
