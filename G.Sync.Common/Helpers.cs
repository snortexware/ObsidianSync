using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace G.Sync.Common
{
    public static class Helpers
    {
        public async static Task<bool> HasFileSettledAsync(string path, int delayMs = 1000)
        {
            DateTime lastWrite = File.GetLastWriteTimeUtc(path);
            await Task.Delay(delayMs);
            return File.GetLastWriteTimeUtc(path) == lastWrite;
        }

        public static bool IsFileInUse(string path)
        {
            var lastWrite = File.GetLastWriteTimeUtc(path);
            var now = DateTime.UtcNow;

            var lastWriteIsLess = (now - lastWrite).Seconds;

            var fiveSeconds = TimeSpan.FromSeconds(3).Seconds;

            if (lastWriteIsLess < fiveSeconds)
            {
                Console.WriteLine($"File recently modified in less than {fiveSeconds} seconds, waiting...");
                return true;
            }
            return false;
        }
    }
}
