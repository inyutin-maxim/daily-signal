using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace DailySignal {

    public static class MoexApi {
        const string DATE_FORMAT = "yyyy-MM-dd";

        public static IEnumerable<Day> DownloadHistory(Sec sec, DateTime from, DateTime till) {
            var start = 0;
            var limit = 100;

            while(true) {
                var url = $"https://iss.moex.com/iss/history/engines/stock/markets/{sec.Market}/boards/{sec.Board}/securities/{sec.ID}.json"
                    + "?from=" + from.ToString(DATE_FORMAT)
                    + "&till=" + till.ToString(DATE_FORMAT)
                    + "&start=" + start
                    + "&limit=" + limit;

                var obj = JsonConvert.DeserializeObject<JObject>(CURL(url));
                var columns = obj["history"]["columns"].ToObject<string[]>();
                var data = obj["history"]["data"];

                if(data.Count() < 1)
                    break;

                var dateIndex = Array.IndexOf(columns, "TRADEDATE");
                var closeIndex = Array.IndexOf(columns, "CLOSE");

                foreach(var line in data) {
                    var close = line[closeIndex];

                    if(close.Type == JTokenType.Null)
                        continue;

                    yield return new Day(
                        DateTime.ParseExact((string)line[dateIndex], DATE_FORMAT, null),
                        (decimal)close
                    );
                }

                if(data.Count() < limit)
                    break;

                start += limit;
            }
        }

        public static bool HasCandles(Sec sec, DateTime date) {
            var dateString = date.ToString(DATE_FORMAT);
            var url = $"https://iss.moex.com/iss/engines/stock/markets/{sec.Market}/boards/{sec.Board}/securities/{sec.ID}/candles.json"
                + "?from=" + dateString
                + "&till=" + dateString;

            return JsonConvert.DeserializeObject<JObject>(CURL(url))["candles"]["data"].Count() > 0;
        }

        static string CURL(string url) {
            Console.Error.WriteLine(url);

            using(var web = new WebClient()) {
                return web.DownloadString(url);
            }
        }

    }

}
