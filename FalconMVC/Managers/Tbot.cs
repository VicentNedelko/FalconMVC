using FalconMVC.Globals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;

namespace FalconMVC.Managers
{
    public class Tbot : IBot
    {
        private static ITelegramBotClient telegramBotClient;
        public async Task SendMessageAsync(string message)
        {
            telegramBotClient = new TelegramBotClient(token: Secret.Tbot);
            _ = await telegramBotClient.SendTextMessageAsync(
                chatId: 481679093,
                text: message,
                disableNotification: false);
        }

    }
}
