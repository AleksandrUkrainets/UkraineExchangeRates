using System;
using UkraineExchangeRates.App.Domain;

namespace UkraineExchangeRates.App.Infrastructure
{
    public interface ICurrencyRateService
    {
        public ExchangeRate GetExchangeRate(Enum currency, DateTime date);
    }
}