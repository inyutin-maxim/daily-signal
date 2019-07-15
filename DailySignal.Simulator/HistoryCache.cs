using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace DailySignal {

    class HistoryCache {
        static readonly string CACHE_DIR = Path.Combine(Path.GetTempPath(), "com.github.alekseymartynov.daily-signal.simulator");

        public static IReadOnlyList<Day> Read(Sec sec) {
            if(!Directory.Exists(CACHE_DIR))
                Directory.CreateDirectory(CACHE_DIR);

            var cacheFile = Path.Combine(CACHE_DIR, sec.ID + ".json");

            if(!File.Exists(cacheFile) || (DateTime.Now - new FileInfo(cacheFile).LastWriteTime) > TimeSpan.FromHours(1)) {
                var from = new DateTime(2015, 1, 1);
                var till = DateTime.Today;
                File.WriteAllText(cacheFile, JsonConvert.SerializeObject(MoexApi.DownloadHistory(sec, from, till)));
            }

            return JsonConvert.DeserializeObject<Day[]>(File.ReadAllText(cacheFile));
        }
    }

}
