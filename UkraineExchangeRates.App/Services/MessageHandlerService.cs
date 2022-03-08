using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;
using UkraineExchangeRates.App.Domain;
using UkraineExchangeRates.App.Infrastructure;

namespace UkraineExchangeRates.App.Services
{
    public class MessageHandlerService : IMassageHandlerService
    {
        private string _date;
        private ICurrencyRateService _currencyRateService;
        private Dictionary<string, string> _messages;
        private bool _isIntroMessage = false;
        private CultureInfo _culture = new CultureInfo("ru-RU");

        public MessageHandlerService(ICurrencyRateService currencyRateService)
        {
            _currencyRateService = currencyRateService;
            _messages = GetTextMessages();
        }

        public async void OnMessageHandler(object sender, MessageEventArgs e)
        {
            string answerToUser;
            Telegram.Bot.Types.Message message = e.Message;
            TelegramBotClient botClient = sender as TelegramBotClient;

            if (e.Message.Type != Telegram.Bot.Types.Enums.MessageType.Text || message.Text == null)
            {
                answerToUser = _messages["IncorrectFormat"];
            }
            else
            {
                answerToUser = GetAnswerToUser(message);
            }

            if (_isIntroMessage)
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, answerToUser, replyMarkup: GetUserMenu());
                _isIntroMessage = false;
            }
            else
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, answerToUser, replyToMessageId: message.MessageId);
            }
        }

        private Dictionary<string, string> GetTextMessages()
        {
            XDocument doc = XDocument.Load(@".\Services\LibMessages.xml");
            Dictionary<string, string> messages = new Dictionary<string, string>();

            foreach (var message in doc.Descendants("Message"))
            {
                messages.Add(message.Attribute("Key").Value,
                                   message.Attribute("Text").Value);
            };

            return messages;
        }

        private ReplyKeyboardMarkup GetUserMenu()
        {
            return new ReplyKeyboardMarkup(
                new[]
                {
                    new[]
                    {
                        new KeyboardButton("Get rate UAH/USD for today."),
                        new KeyboardButton("Get rate UAH/USD for yesterday.")
                    },
                    new[]
                    {
                        new KeyboardButton("Get rate UAH/EUR for today."),
                        new KeyboardButton("Get rate UAH/EUR for yesterday.")
                    }
                }
                );
        }

        public string GetAnswerToUser(Telegram.Bot.Types.Message message)
        {
            DateTime dateArchive;
            Currencies currency;

            string stringFromUser = message.Text;
            string[] wordsFromUser = stringFromUser.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string requestedExchangeWord = wordsFromUser[0];
            string requestedDateWord = wordsFromUser.LastOrDefault();

            switch (stringFromUser)
            {
                case "/start":
                    _isIntroMessage = true;
                    return _messages["Intro"];

                case "Get rate UAH/USD for today.":
                    requestedExchangeWord = "USD";
                    requestedDateWord = DateTime.Now.ToString(_culture);
                    break;

                case "Get rate UAH/USD for yesterday.":
                    requestedExchangeWord = "USD";
                    requestedDateWord = DateTime.Now.AddDays(-1).ToString(_culture);
                    break;

                case "Get rate UAH/EUR for today.":
                    requestedExchangeWord = "EUR";
                    requestedDateWord = DateTime.Now.ToString(_culture);
                    break;

                case "Get rate UAH/EUR for yesterday.":
                    requestedExchangeWord = "EUR";
                    requestedDateWord = DateTime.Now.AddDays(-1).ToString(_culture);
                    break;
            }

            if (!OnCheckDate(out dateArchive, requestedDateWord)) return _messages["IncorrectDate"];
            if (!OnCheckCurrency(requestedExchangeWord, out currency)) return _messages["IncorrectCurrency"];

            ExchangeRate askedExchangeRate = _currencyRateService.GetExchangeRate(currency, dateArchive);
            if (askedExchangeRate == null) return _messages["EmptyAnswer"];

            double purchaseRate = (double)askedExchangeRate.PurchaseRate;
            double saleRate = (double)askedExchangeRate.SaleRate;

            return "Exchange rates  " + currency + ": " + purchaseRate + "/" + saleRate + " грн.";
        }

        public bool OnCheckDate(out DateTime dateArchive, string requestedDateWord)
        {
            Match machExpression = Regex.Match(requestedDateWord, @"\d\d[.]\d\d[.]\d\d\d\d");
            if (machExpression.Success)
            {
                _date = machExpression.Captures[0].Value.ToString();

                return DateTime.TryParse(_date, _culture.DateTimeFormat, DateTimeStyles.None, out dateArchive);
            }
            else
            {
                dateArchive = new DateTime();
                return false;
            }
        }

        public bool OnCheckCurrency(string requestedExchangeWord, out Currencies currency)
        {
            List<Currencies> currencies = Enum.GetValues(typeof(Currencies)).Cast<Currencies>().ToList();

            try
            {
                currency = currencies.Single(c => c.ToString() == requestedExchangeWord.ToUpper());
            }
            catch (Exception)
            {
                currency = new Currencies();

                return false;
            }

            return true;
        }
    }
}