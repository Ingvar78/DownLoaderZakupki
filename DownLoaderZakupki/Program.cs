using DownLoaderZakupki.Configurations;
using FluentScheduler;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog.Extensions.Logging;
using System;
using System.IO;
using System.Text;
namespace DownLoaderZakupki
{
    class Program
    {
        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var sProvider = BuildConfig();
            var logger = sProvider.GetService<ILogger<Program>>();
            logger.LogDebug("Init Main");

            JobManager.Initialize(sProvider.GetRequiredService<JobsRegister>());

            Console.ReadLine();
            JobManager.Stop();
        }

        private static IServiceProvider BuildConfig()
        {

            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            var configuration = builder.Build();


            var services = new ServiceCollection();

            services.Configure<CommonSettings>(x => configuration.GetSection("CommonSettings").Bind(x));
            services.Configure<ConnectionDB>(x => configuration.GetSection("ConnectionDB").Bind(x));
            services.Configure<FZSettings44>(x => configuration.GetSection("FzSettings44").Bind(x));
            services.Configure<FZSettings223>(x => configuration.GetSection("FzSettings223").Bind(x));
            services.AddSingleton<ILoggerFactory, LoggerFactory>();
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            services.AddLogging((conf) => conf.SetMinimumLevel(LogLevel.Trace));
            
            //JobRegistry is the custom class
            services.AddTransient<JobsRegister>();

            var serviceProvider = services.BuildServiceProvider();

            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

            //configure NLog                       
            loggerFactory.AddNLog(new NLogProviderOptions { CaptureMessageTemplates = true, CaptureMessageProperties = true });
            NLog.LogManager.LoadConfiguration("nlog.config");
            return serviceProvider;
        }

    }
}
