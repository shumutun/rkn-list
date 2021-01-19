using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog;
using NLog.Web;
using RknList.Config;
using RknList.Processing.RnkListObserve;
using RknList.Startup;
using System;
using System.Net;
using System.Text;

namespace RknList
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var logger = NLogBuilder.ConfigureNLog("NLog.config").GetCurrentClassLogger();
            try
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                var host = CreateHostBuilder(args).ConfigureServices(services => services.AddSingleton<ILogger>(logger)).Build();

                host.Services.GetService<RnkListObserver>().StartObserve();

                logger.Info("Host configured");
                host.Run();
                logger.Info("Web server stoped");
            }
            catch (Exception exception)
            {
                logger.Error(exception, "Program stopped because of exception");
                throw;
            }
            finally
            {
                LogManager.Shutdown(); //Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
            }
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<StartupConfig>();
                    webBuilder.UseKestrel(options => ConfigureKestrel(options, args));
                })
                .UseNLog();

        private static void ConfigureKestrel(KestrelServerOptions options, string[] args)
        {
            if (args == null || args.Length == 0 || !int.TryParse(args[0], out int port))
                throw new ArgumentNullException("webface port is not specified");
            options.Listen(IPAddress.Any, port);
        }
    }
}
