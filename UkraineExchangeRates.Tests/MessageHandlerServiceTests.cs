using Moq;
using System;
using System.Globalization;
using UkraineExchangeRates.App.Domain;
using UkraineExchangeRates.App.Infrastructure;
using UkraineExchangeRates.App.Services;
using Xunit;

namespace UkraineExchangeRates.Tests
{
    public class MessageHandlerServiceTests
    {
        private readonly MessageHandlerService _massageHandlerService;
        private readonly Mock<ICurrencyRateService> _currencyRateService;
        private CultureInfo _culture = new CultureInfo("ru-RU");

        public MessageHandlerServiceTests()
        {
            _currencyRateService = new Mock<ICurrencyRateService>();
            _massageHandlerService = new MessageHandlerService(_currencyRateService.Object);
        }

        [Fact]
        public void GetValidExchangeRate_USD_Today_Button_CallsByGetAnswerToUser()
        {
            Telegram.Bot.Types.Message message = new Telegram.Bot.Types.Message() { Text = "Get rate UAH/USD for today." };
            var currency = Currencies.USD;
            var dateNow = DateTime.Now;
            double purchaseRate = 26.75;
            double saleRate = 27.15;

            DateTime dateArchive;
            _massageHandlerService.OnCheckDate(out dateArchive, dateNow.ToString(_culture));
            ExchangeRate mockExchangeRate = new ExchangeRate() { Currency = currency.ToString(), PurchaseRate = purchaseRate, SaleRate = saleRate };

            string expectedMessage = $"Exchange rates  USD: {purchaseRate}/{saleRate} грн.";

            _currencyRateService.Setup(x => x.GetExchangeRate(currency, dateArchive)).Returns(mockExchangeRate);
            string actualMessage = _massageHandlerService.GetAnswerToUser(message);

            Assert.Equal(expectedMessage, actualMessage);
        }

        [Fact]
        public void GetValidExchangeRate_USD_01_01_2021_CallsByGetAnswerToUser()
        {
            Telegram.Bot.Types.Message message = new Telegram.Bot.Types.Message() { Text = "USD / 01.01.2021" };
            var requestedExchangeWord = Currencies.USD.ToString();
            var dateNewYear = new DateTime(2021, 1, 1);
            double purchaseRate = 26.75;
            double saleRate = 27.15;

            DateTime dateArchive;
            _massageHandlerService.OnCheckDate(out dateArchive, dateNewYear.ToString(_culture));

            Currencies currency;
            _massageHandlerService.OnCheckCurrency(requestedExchangeWord, out currency);

            ExchangeRate mockExchangeRate = new ExchangeRate() { Currency = requestedExchangeWord, PurchaseRate = purchaseRate, SaleRate = saleRate };
            string expectedMessage = $"Exchange rates  USD: {purchaseRate}/{saleRate} грн.";

            _currencyRateService.Setup(x => x.GetExchangeRate(currency, dateArchive)).Returns(mockExchangeRate);
            string actualMessage = _massageHandlerService.GetAnswerToUser(message);

            Assert.Equal(expectedMessage, actualMessage);
        }

        [Theory]
        [InlineData("USD/ 01.01.2021")]
        [InlineData("USD/01.01.2021")]
        [InlineData("eur / 01.01.2021")]
        [InlineData("qwer / 01.01.2021")]
        [InlineData("122 / 01.01.2021")]
        [InlineData("3245324")]
        public void GetInvalidExchangeRate_EUR_01_01_2021_CallsByGetAnswerToUser(string request)
        {
            Telegram.Bot.Types.Message message = new Telegram.Bot.Types.Message() { Text = request };
            var requestedExchangeWord = Currencies.USD.ToString();
            var dateNewYear = new DateTime(2021, 1, 1);
            double purchaseRate = 26.75;
            double saleRate = 27.15;

            DateTime dateArchive;
            _massageHandlerService.OnCheckDate(out dateArchive, dateNewYear.ToString(_culture));

            Currencies currency;
            _massageHandlerService.OnCheckCurrency(requestedExchangeWord, out currency);

            ExchangeRate mockExchangeRate = new ExchangeRate() { Currency = requestedExchangeWord, PurchaseRate = purchaseRate, SaleRate = saleRate };
            string expectedMessage = $"Exchange rates  USD: {purchaseRate}/{saleRate} грн.";

            _currencyRateService.Setup(x => x.GetExchangeRate(currency, dateArchive)).Returns(mockExchangeRate);
            string actualMessage = _massageHandlerService.GetAnswerToUser(message);

            Assert.NotEqual(expectedMessage, actualMessage);
        }

        [Fact]
        public void ValidCurrency_CallsByOnCheckCurrency()
        {
            const string requestedExchangeWord = "USD";
            Currencies currency;

            bool actualResultOfCurrencyChecking = _massageHandlerService.OnCheckCurrency(requestedExchangeWord, out currency);

            Assert.True(actualResultOfCurrencyChecking);
        }

        [Theory]
        [InlineData(" ")]
        [InlineData("")]
        [InlineData(" usd")]
        [InlineData(" USD ")]
        [InlineData("USDD")]
        [InlineData("USD EUR")]
        public void InvalidCurrency_CallsByOnCheckCurrency(string word)
        {
            string requestedExchangeWord = word;
            Currencies currency;

            bool actualResultOfCurrencyChecking = _massageHandlerService.OnCheckCurrency(requestedExchangeWord, out currency);

            Assert.False(actualResultOfCurrencyChecking);
        }

        [Fact]
        public void ValidDateWord_CallsByOnCheckDate()
        {
            const string requestedDateWord = "21.06.2021";
            DateTime dateArchive;

            bool actualResultOfDateChecking = _massageHandlerService.OnCheckDate(out dateArchive, requestedDateWord);

            Assert.True(actualResultOfDateChecking);
            Assert.NotEqual(dateArchive, new DateTime());
        }

        [Theory]
        [InlineData("21.21.2021")]
        [InlineData("21/12/2021")]
        [InlineData("21.12.21")]
        [InlineData("21-12-2021")]
        [InlineData("")]
        [InlineData("21_12_21")]
        [InlineData(" ")]
        [InlineData("21122021")]
        public void InValidDateWord_CallsByOnCheckDate(string dateWord)
        {
            string requestedDateWord = dateWord;

            DateTime actualDateArchive;
            bool actualResultOfDateChecking = _massageHandlerService.OnCheckDate(out actualDateArchive, requestedDateWord);

            Assert.False(actualResultOfDateChecking);
            Assert.Equal(actualDateArchive, new DateTime());
        }
    }
}