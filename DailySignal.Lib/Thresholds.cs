using System;

namespace DailySignal {

    public struct Thresholds : IEquatable<Thresholds> {
        public readonly decimal Low, High;

        public Thresholds(decimal low, decimal high) {
            Low = low;
            High = high;
        }

        public override bool Equals(object obj) {
            if(obj is Thresholds other)
                return Equals(other);
            return false;
        }

        public bool Equals(Thresholds other)
            => other.Low == Low && other.High == High;

        public override int GetHashCode()
            => HashCode.Combine(Low, High);
    }

}
