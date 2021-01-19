using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RknList.Config;
using RknList.Processing.RnkListObserve;
using System;

namespace RknList.Startup
{
    public class StartupConfig
    {
        private AppConfig _appConfig;

        public StartupConfig(IConfiguration configuration)
        {
            _appConfig = AppConfig.FromConfigFile() ?? throw new NullReferenceException("App config is null");
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(_appConfig);
            services.AddSingleton<RnkListObserver>();
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(name: "default", pattern: "{controller=RknList}/{action=Get}");
            });
        }
    }
}
