using System;
using System.Net;

static class TelegramApi {

    public static void SendMessage(string key, string chat, string message) {
        if(!String.IsNullOrEmpty(key) && !String.IsNullOrEmpty(chat)) {
            try {
                using(var web = new WebClient()) {
                    web.DownloadData($"https://api.telegram.org/bot{key}/sendMessage"
                        + "?chat_id=" + WebUtility.UrlEncode(chat)
                        + "&text=" + WebUtility.UrlEncode(message)
                    );
                }
            } catch {
                throw new Exception("Telegram API failed");
            }
        } else {
            Console.WriteLine("Telegram not configured");
        }
    }

}
