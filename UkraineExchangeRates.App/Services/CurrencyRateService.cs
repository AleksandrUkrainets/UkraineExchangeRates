using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net;
using UkraineExchangeRates.App.Domain;
using UkraineExchangeRates.App.Infrastructure;

namespace UkraineExchangeRates.App.Services
{
    public class CurrencyRateService : ICurrencyRateService
    {
        public ExchangeRate GetExchangeRate(Enum currency, DateTime date)
        {
            string bankAnswer = GetArchiveFromBank(date);
            ArchiveExchangeRate archiveExchangeRateOnDate = JsonConvert.DeserializeObject<ArchiveExchangeRate>(bankAnswer);
            List<ExchangeRate> exchangeRatesOnDate = archiveExchangeRateOnDate.ExchangeRate;

            return exchangeRatesOnDate.FirstOrDefault(x => x.Currency == currency.ToString());
        }

        private string GetArchiveFromBank(DateTime date)
        {
            string bankAnswer;
            var getBankAPI = ConfigurationManager.AppSettings.Get("GetBankAPI");
            if (getBankAPI == null)
            {
                throw new ArgumentNullException("Link to bank API in the config file cannot be empty!");
            }

            using (WebClient webClient = new WebClient())
            {
                string pathJson = getBankAPI + "&date=" + date.ToString("d", CultureInfo.CreateSpecificCulture("ru-RU"));
                bankAnswer = webClient.DownloadString(pathJson);
            }

            return bankAnswer;
        }
    }
}