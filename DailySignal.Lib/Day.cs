using Newtonsoft.Json;
using System;
using System.ComponentModel;

namespace DailySignal {

    public struct Day {
        public readonly DateTime Date;
        public readonly decimal Close;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public readonly decimal Indicator;

        [JsonConstructor]
        public Day(DateTime date, decimal close, decimal indicator = 0) {
            Date = date;
            Close = close;
            Indicator = indicator;
        }
    }

}
