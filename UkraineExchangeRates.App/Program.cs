using System;
using System.Configuration;
using Telegram.Bot;
using UkraineExchangeRates.App.Infrastructure;
using UkraineExchangeRates.App.Services;

namespace UkraineExchangeRates.App
{
    public class Program
    {
        private static ITelegramBotClient _botClient;
        private static IMassageHandlerService _massageHandlerService;

        public static void Main()
        {
            _massageHandlerService = new MessageHandlerService(new CurrencyRateService());

            var token = ConfigurationManager.AppSettings.Get("Token");
            if (token == null)
                throw new ArgumentNullException("Token value in the config file cannot be empty!");

            _botClient = new TelegramBotClient(token);
            _botClient.StartReceiving();
            _botClient.OnMessage += _massageHandlerService.OnMessageHandler;
            Console.WriteLine("[PrivatCurrencyExchange1_bot]: Bot started");
            Console.ReadKey();
            _botClient.StopReceiving();
        }
    }
}