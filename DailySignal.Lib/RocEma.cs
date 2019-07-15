using System;
using System.Collections.Generic;

namespace DailySignal {

    public static class RocEma {
        const int
            ROC_PERIOD = 5,
            EMA_PERIOD = 9;

        const decimal ALPHA = 2m / (EMA_PERIOD + 1);

        public static IReadOnlyList<Day> Calculate(IReadOnlyList<Day> history) {
            var result = new Day[history.Count];

            for(var i = 0; i < history.Count; i++) {
                var day = history[i];
                var rocEma = 0m;

                if(i > ROC_PERIOD) {
                    var roc = 100 * (history[i].Close / history[i - ROC_PERIOD].Close - 1);
                    rocEma = ALPHA * roc + (1 - ALPHA) * result[i - 1].Indicator;
                }

                result[i] = new Day(day.Date, day.Close, rocEma);
            }

            return result;
        }

    }

}
