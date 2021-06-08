﻿using eToroApiScraper.Objects;
using OpenQA.Selenium;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace eToroApiScraper.Services
{
    public interface IeToroService
    {
        Task Login();
        Task GetAllWatchlistsPeopleTrades(Dictionary<string, List<eToroTrade>> tradeData);
    }
}