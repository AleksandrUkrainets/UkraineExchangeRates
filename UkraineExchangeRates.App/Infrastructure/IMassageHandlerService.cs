using Telegram.Bot.Args;

namespace UkraineExchangeRates.App.Infrastructure
{
    public interface IMassageHandlerService
    {
        public void OnMessageHandler(object sender, MessageEventArgs e);
    }
}