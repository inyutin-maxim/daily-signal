using LinqStatistics;
using Pastel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace DailySignal {

    class Program {

        static void Main(string[] args) {
            var sec = new Sec("shares", "TQTF", "FXUS");
            var history = RocEma.Calculate(HistoryCache.Read(sec));

            //RunExperiment(history, new Thresholds(-1.1m, 2.2m), Console.WriteLine);
            //Environment.Exit(1);

            var tasks = SliceHistory(history)
                .Select(slice => Task.Run(() => BruteThresholds(slice)))
                .ToArray();

            Task.WaitAll(tasks);
            Console.Error.WriteLine();

            var groups = tasks
                .SelectMany(t => t.Result)
                .GroupBy(r => r.Thresholds)
                .Select(g => new {
                    Thresholds = g.Key,
                    Count = g.Count(),
                    AvgXIrr = g.Average(i => i.XIrr),
                    SdXIrr = g.StandardDeviation(i => i.XIrr),
                    AvgTradeCount = g.Average(i => i.TradeCount),
                    AvgSlipCount = g.Average(i => i.SlipCount)
                })
                .ToArray();

            var countTolerance = groups.Max(g => g.Count) - 2;

            groups = groups
                .Where(g => g.Count >= countTolerance)
                .OrderBy(g => g.SdXIrr)
                .ThenByDescending(g => g.AvgXIrr)
                .ThenBy(g => g.Thresholds.High)
                .ThenByDescending(g => g.Thresholds.Low)
                .ToArray();

            var sdXIrrCents = Enumerable.Range(0, 100)
                .Select(i => groups[groups.Length * i / 100].SdXIrr)
                .ToArray();

            Console.WriteLine(String.Join("\t", "t_lo", "t_hi", "count", "m_xirr", "sd_xirr", "m_trade", "m_slip"));

            var bestAvgXIrr = -1d;

            foreach(var g in groups) {
                if(g.AvgXIrr <= bestAvgXIrr)
                    continue;
                else
                    bestAvgXIrr = g.AvgXIrr;

                var sdXIrrCent = sdXIrrCents.TakeWhile(i => g.SdXIrr >= i).Count();

                Console.WriteLine(String.Join("\t",
                    g.Thresholds.Low, g.Thresholds.High,
                    g.Count,
                    Math.Round(100 * g.AvgXIrr, 2),
                    ("p" + sdXIrrCent).Pastel(CentToColor(sdXIrrCent)),
                    Math.Round(g.AvgTradeCount, 1),
                    Math.Round(g.AvgSlipCount, 1)
                ));
            }
        }

        static IEnumerable<IReadOnlyList<Day>> SliceHistory(IReadOnlyList<Day> history) {
            var minFrom = DateTime.Today.AddYears(-3);
            var indicatorWarmupLength = 30;

            var step = 13;
            var length = 250;

            var startIndex = history.Count - length;

            while(true) {
                if(startIndex < indicatorWarmupLength || history[startIndex].Date < minFrom)
                    break;

                yield return history.Skip(startIndex).Take(length).ToArray();

                startIndex -= step;
            }
        }

        static IReadOnlyList<ExperimentResult> BruteThresholds(IReadOnlyList<Day> history) {
            var lows = Enumerable.Range(5, 21).Select(i => -.1m * i);
            var highs = Enumerable.Range(5, 21).Select(i => .1m * i);
            // highs = new[] { decimal.MaxValue };

            var goodResults = new List<ExperimentResult>();

            foreach(var low in lows) {
                foreach(var high in highs) {
                    var result = RunExperiment(history, new Thresholds(low, high));
                    if(result.XIrr > 0)
                        goodResults.Add(result);
                }
            }

            Console.Error.Write(".");
            return goodResults;
        }

        static ExperimentResult RunExperiment(IReadOnlyList<Day> history, Thresholds thresholds, Action<string> logger = null) {
            var tradeCount = 0;
            var slipCount = 0;

            var cash = 0m;
            var depo = new List<decimal>();

            var flowDates = new List<DateTime>();
            var flowValues = new List<decimal>();

            for(var i = 2; i < history.Count; i++) {
                var isLastDay = i == history.Count - 1;
                var day = history[i];
                var date = day.Date.ToString("yyyy-MM-dd");
                var price = day.Close;
                var candidate = new TurnCandidate(history, i - 1);

                if(!isLastDay && candidate.IsDip(thresholds.Low)) {
                    var buyCount = 1;
                    var requiredCash = buyCount * price;

                    if(cash < requiredCash) {
                        var topup = requiredCash - cash;
                        flowDates.Add(day.Date);
                        flowValues.Add(topup);
                        cash += topup;
                        logger?.Invoke($"{date} Top-up {topup}");
                    }

                    cash -= requiredCash;
                    depo.AddRange(Enumerable.Repeat(price, buyCount));
                    tradeCount++;

                    logger?.Invoke($"{date} Buy {buyCount} x {price}");
                }

                if(isLastDay || candidate.IsPeak(thresholds.High)) {
                    if(depo.Any()) {
                        var sellCount = depo.Count;

                        var investedValue = depo.Take(sellCount).Sum();
                        var currentValue = sellCount * price;
                        var earning = currentValue - investedValue;

                        if(isLastDay || earning > 0) {
                            cash += currentValue;
                            depo.RemoveRange(0, sellCount);
                            tradeCount++;

                            logger?.Invoke($"{date} Sell {sellCount} x {price}, earn {earning}");
                        } else {
                            logger?.Invoke($"{date} Hold");
                        }
                    } else {
                        slipCount++;
                        logger?.Invoke($"{date} Nothing to sell");
                    }
                }
            }

            if(depo.Any())
                throw new Exception();

            var xirr = 0d;

            if(flowValues.Any()) {
                flowDates.Add(history.Last().Date);
                flowValues.Add(-cash);
                xirr = Excel.FinancialFunctions.Financial.XIrr(flowValues.Select(Convert.ToDouble), flowDates);
            }

            return new ExperimentResult(thresholds, xirr, tradeCount, slipCount);
        }

        static Color CentToColor(int cent) {
            var h = 4 - cent / 25d;

            int Component(int n) {
                var k = (n + h) % 12;
                var c = Math.Min(k - 3, 9 - k);
                c = Math.Min(c, 1);
                c = Math.Max(c, -1);
                return (int)Math.Round(127.5 * (1 - c));
            }

            return Color.FromArgb(Component(0), Component(8), Component(4));
        }
    }

}
