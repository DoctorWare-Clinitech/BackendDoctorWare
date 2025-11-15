using System;
using System.Collections.Generic;

namespace DoctorWare.Services.Interfaces
{
    public interface IRequestMetricsService
    {
        void Record(string path, int statusCode, long elapsedMilliseconds);
        RequestMetricsSnapshot GetSnapshot();
    }

    public class RequestMetricsSnapshot
    {
        public int TotalRequests { get; set; }
        public double AverageMilliseconds { get; set; }
        public long MaxMilliseconds { get; set; }
        public Dictionary<string, int> RequestsByPath { get; set; } = new();
        public DateTime GeneratedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
