using FalconMVC.Globals;
using FalconMVC.Models.BotModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;

namespace FalconMVC.Managers
{
    public class Tbot : IBot
    {
        private static ITelegramBotClient telegramBotClient;
        //private readonly string baseUri = "https://api.telegram.org/";

        public Tbot()
        {
            telegramBotClient = new TelegramBotClient(token: Secret.Tbot);
            telegramBotClient.StartReceiving();
        }
        public async Task SendMessageAsync(string message)
        {
            var result = await telegramBotClient.SendTextMessageAsync(
                chatId: Secret.GIRAChatId,
                text: message,
                disableNotification: false);
        }

    }
}
