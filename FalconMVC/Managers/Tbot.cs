using FalconMVC.Globals;
using FalconMVC.Models.BotModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;

namespace FalconMVC.Managers
{
    public class Tbot : IBot
    {
        private static ITelegramBotClient telegramBotClient;
        private readonly string baseUri = "https://api.telegram.org/";
        public async Task SendMessageAsync(string message)
        {
            telegramBotClient = new TelegramBotClient(token: Secret.Tbot);
            _ = await telegramBotClient.SendTextMessageAsync(
                chatId: await GetChatIdAsync(),
                text: message,
                disableNotification: false);
        }

        public async Task<long> GetChatIdAsync()
        {
            string reqPath = String.Concat(baseUri, "bot", Secret.Tbot, "/getUpdates");
            Uri request = new(reqPath);
            HttpClient httpClient = new();
            var responce = await httpClient.GetAsync(request);
            var updateResponce = JsonConvert.DeserializeObject<Root>(await responce.Content.ReadAsStringAsync());
            return updateResponce.Result.FirstOrDefault().Message.Chat.Id;
        }

    }
}
