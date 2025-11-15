using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DoctorWare.Services.Interfaces;

namespace DoctorWare.Services.Implementation
{
    public class RequestMetricsService : IRequestMetricsService
    {
        private readonly ConcurrentDictionary<string, MetricsEntry> store = new();

        public void Record(string path, int statusCode, long elapsedMilliseconds)
        {
            string key = NormalizePath(path);
            MetricsEntry entry = store.GetOrAdd(key, _ => new MetricsEntry());
            entry.Increment(elapsedMilliseconds);
        }

        public RequestMetricsSnapshot GetSnapshot()
        {
            int totalRequests = store.Values.Sum(e => e.Count);
            long totalDuration = store.Values.Sum(e => e.TotalMilliseconds);
            long maxDuration = store.Values.Select(e => e.MaxMilliseconds).DefaultIfEmpty(0).Max();

            RequestMetricsSnapshot snapshot = new RequestMetricsSnapshot
            {
                TotalRequests = totalRequests,
                AverageMilliseconds = totalRequests == 0 ? 0 : (double)totalDuration / totalRequests,
                MaxMilliseconds = maxDuration,
                RequestsByPath = store.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Count),
                GeneratedAtUtc = DateTime.UtcNow
            };

            return snapshot;
        }

        private static string NormalizePath(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return "/";
            }

            if (path.Length <= 1)
            {
                return path;
            }

            string normalized = path.Split('?', '#')[0];
            return normalized.ToLowerInvariant();
        }

        private sealed class MetricsEntry
        {
            private long totalMilliseconds;
            private int count;
            private long maxMilliseconds;

            public int Count => count;
            public long TotalMilliseconds => totalMilliseconds;
            public long MaxMilliseconds => maxMilliseconds;

            public void Increment(long elapsed)
            {
                Interlocked.Increment(ref count);
                Interlocked.Add(ref totalMilliseconds, elapsed);

                long currentMax;
                do
                {
                    currentMax = maxMilliseconds;
                    if (elapsed <= currentMax)
                    {
                        break;
                    }
                }
                while (Interlocked.CompareExchange(ref maxMilliseconds, elapsed, currentMax) != currentMax);
            }
        }
    }
}
