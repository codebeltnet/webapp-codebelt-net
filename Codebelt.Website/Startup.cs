using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Codebelt.Website.TagHelpers;
using Cuemon.AspNetCore.Diagnostics;
using Cuemon.AspNetCore.Http.Throttling;
using Cuemon.AspNetCore.Razor.TagHelpers;
using Cuemon.Data.Integrity;
using Cuemon.Diagnostics;
using Cuemon.Extensions.AspNetCore.Configuration;
using Cuemon.Extensions.AspNetCore.Diagnostics;
using Cuemon.Extensions.AspNetCore.Http;
using Cuemon.Extensions.AspNetCore.Http.Throttling;
using Cuemon.Security.Cryptography;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;

namespace Codebelt.Website
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.Configure<CdnTagHelperOptions>(Configuration.GetSection("CdnTagHelperOptions:0"));
            services.Configure<AppTagHelperOptions>(Configuration.GetSection("CdnTagHelperOptions:1"));
            services.AddResponseCaching();
            services.AddAssemblyCacheBusting();
            services.AddMemoryThrottlingCache();
            services.AddServerTiming();
            services.AddRazorPages();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseThrottlingSentinel(o =>
            {
                o.ContextResolver = context => context.Connection.RemoteIpAddress.ToString();
                o.Quota = new ThrottleQuota(3600, TimeSpan.FromHours(1));
            });

            app.Use((context, next) =>
            {
                var serverTiming = context.RequestServices.GetRequiredService<IServerTiming>();
                context.Response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue()
                {
                    Public = true,
                    MustRevalidate = true,
                    NoTransform = true
                };
                context.Response.Headers[HeaderNames.Expires] = DateTime.UtcNow.Add(TimeSpan.FromHours(12)).ToString("R");
                using (var ms = new MemoryStream())
                {
                    var body = context.Response.Body;
                    var statusCodeBeforeBodyRead = context.Response.StatusCode;
                    context.Response.Body = ms;
                    var razorTiming = TimeMeasure.WithAction(() => next().GetAwaiter().GetResult());
                    ms.Seek(0, SeekOrigin.Begin);

                    var dynamicCacheTiming = TimeMeasure.WithAction(() =>
                    {
                        if (statusCodeBeforeBodyRead == StatusCodes.Status304NotModified) { context.Response.StatusCode = statusCodeBeforeBodyRead; }
                        var builder = new ChecksumBuilder(ms.ToArray(), () => UnkeyedHashFactory.CreateCryptoMd5());
                        context.Response.AddOrUpdateEntityTagHeader(context.Request, builder);
                        
                    });

                    serverTiming.AddServerTiming("razor", razorTiming.Elapsed);
                    serverTiming.AddServerTiming("dynamic-etag", dynamicCacheTiming.Elapsed);

                    context.Response.Headers.Add(ServerTiming.HeaderName, serverTiming.Metrics.Select(metric => metric.ToString()).ToArray());
                    
                    if (context.Response.StatusCode.IsSuccessStatusCode()) { ms.CopyToAsync(body).GetAwaiter().GetResult(); }

                    return Task.CompletedTask;
                };
            });

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}