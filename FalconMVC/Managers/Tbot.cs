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
        }
        public async Task SendMessageAsync(string message)
        {
            _ = await telegramBotClient.SendTextMessageAsync(
                chatId: Secret.BotName,
                text: message,
                disableNotification: false);
        }

    }
}
