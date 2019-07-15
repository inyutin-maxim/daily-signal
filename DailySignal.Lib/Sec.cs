using System;

namespace DailySignal {

    public class Sec {
        public readonly string
            Market,
            Board,
            ID;

        public Sec(string market, string board, string id) {
            Market = market;
            Board = board;
            ID = id;
        }
    }

}
