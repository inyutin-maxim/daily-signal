using System;

namespace DailySignal {

    struct ExperimentResult {
        public readonly Thresholds Thresholds;
        public readonly double XIrr;
        public readonly int TradeCount, SlipCount;

        public ExperimentResult(Thresholds thresholds, double xirr, int tradeCount, int slipCount) {
            Thresholds = thresholds;
            XIrr = xirr;
            TradeCount = tradeCount;
            SlipCount = slipCount;
        }
    }

}
