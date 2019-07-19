using System;
using System.Globalization;
using System.Linq;

namespace DailySignal {

    class Program {

        static void Main(string[] args) {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            var (sec, thresholds) = ParseArgs(args);

            var date = DateTime.Now;
            Console.WriteLine($"Now: {date:s}");

            date = date.Date.AddDays(date < date.Date.AddHours(20) ? -1 : 0);
            //date = new DateTime(2018, 10, 1);
            Console.WriteLine($"Trade date: {date:s}");

            var history = MoexApi.DownloadHistory(sec, date.AddDays(-60), date).ToArray();

            if(history.Last().Date != date) {
                if(MoexApi.HasCandles(sec, date))
                    throw new Exception($"No history entry for {date:yyyy-MM-dd}");

                Console.WriteLine($"No trades on {date:yyyy-MM-dd}");
                return;
            }

            var indicator = RocEma.Calculate(history);
            var candidate = new TurnCandidate(indicator, indicator.Count - 2);

            Console.WriteLine(candidate);

            if(candidate.IsDip(thresholds.Low))
                Notify($"📈 '{sec.ID}' DIP on {date:yyyy-MM-dd}");

            if(candidate.IsPeak(thresholds.High))
                Notify($"📉 '{sec.ID}' PEAK on {date:yyyy-MM-dd}");

            Console.WriteLine();
        }

        static (Sec sec, Thresholds thresholds) ParseArgs(string[] args) {
            var secSegments = (args.FirstOrDefault() ?? "shares/TQTF/FXUS").Split("/");

            return (
                new Sec(secSegments[0], secSegments[1], secSegments[2]),
                new Thresholds(
                    args.Length > 1 ? Convert.ToDecimal(args[1]) : -1m,
                    args.Length > 2 ? Convert.ToDecimal(args[2]) : Decimal.MaxValue
                )
            );
        }

        static void Notify(string message) {
            Console.WriteLine(message);
            TelegramApi.SendMessage(
                Environment.GetEnvironmentVariable("TELEGRAM_KEY"),
                Environment.GetEnvironmentVariable("TELEGRAM_CHAT"),
                message
            );
        }


    }

}
