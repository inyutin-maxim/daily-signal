using System;
using System.Collections.Generic;

namespace DailySignal {

    public class TurnCandidate {
        readonly decimal Left, Center, Right;

        public TurnCandidate(IReadOnlyList<Day> history, int centerIndex) {
            Left = history[centerIndex - 1].Indicator;
            Center = history[centerIndex].Indicator;
            Right = history[centerIndex + 1].Indicator;
        }

        public bool IsDip(decimal threshold)
            => Center <= threshold && Left > Center && Right > Center;

        public bool IsPeak(decimal threshold)
            => Center >= threshold && Left < Center && Right < Center;

        public override string ToString()
            => $"{Left:N2} {Center:N2} {Right:N2}";

    }

}
