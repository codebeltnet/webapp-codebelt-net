using System;
using Cuemon.AspNetCore.Http.Throttling;
using Cuemon.AspNetCore.Razor.TagHelpers;
using Cuemon.Extensions.AspNetCore.Configuration;
using Cuemon.Extensions.AspNetCore.Diagnostics;
using Cuemon.Extensions.AspNetCore.Http.Headers;
using Cuemon.Extensions.AspNetCore.Http.Throttling;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
            services.AddResponseCompression();
            services.AddAssemblyCacheBusting();
            services.AddMemoryThrottlingCache();
            services.AddServerTiming();
            services.AddRazorPages();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseHsts();
            app.UseHttpsRedirection();

            app.UseThrottlingSentinel(o =>
            {
                o.ContextResolver = context => context.Connection.RemoteIpAddress.ToString();
                o.Quota = new ThrottleQuota(3600, TimeSpan.FromHours(1));
            });

            app.UseServerTiming();

            app.UseResponseCompression();

            app.UseResponseCaching();

            app.UseCacheControl(o => o.Validators.Add(new EntityTagCacheableValidator()));

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}
