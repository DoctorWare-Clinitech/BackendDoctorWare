using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using DoctorWare.Services.Interfaces;

namespace DoctorWare.Middleware
{
    public class RequestMetricsMiddleware
    {
        private readonly RequestDelegate next;

        public RequestMetricsMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context, IRequestMetricsService metricsService)
        {
            Stopwatch sw = Stopwatch.StartNew();
            try
            {
                await next(context);
            }
            finally
            {
                sw.Stop();
                metricsService.Record(context.Request.Path.HasValue ? context.Request.Path.Value! : "/", context.Response?.StatusCode ?? 0, sw.ElapsedMilliseconds);
            }
        }
    }
}
