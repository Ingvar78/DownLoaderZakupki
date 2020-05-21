using DownLoaderZakupki.Configurations;
using DownLoaderZakupki.Core;
using FluentScheduler;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;



namespace DownLoaderZakupki
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        //https://go.microsoft.com/fwlink/?LinkID=398940


        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CommonSettings>(Configuration.GetSection("CommonSettings"));
            services.Configure<ConnectionDB>(Configuration.GetSection("ConnectionDB"));
            services.Configure<FZSettings44>(Configuration.GetSection("FzSettings44"));
            services.Configure<FZSettings223>(Configuration.GetSection("FzSettings223"));
            services.AddSingleton<ILoggerFactory, LoggerFactory>();
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            services.AddLogging((conf) => conf.SetMinimumLevel(LogLevel.Trace));
            services.AddEntityFrameworkNpgsql();


            InjectorBootStrapper.RegisterServices(services);

        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory, IServiceProvider services)
        {

            if (loggerFactory != null)
            {
                var logger = services.GetService<ILogger<Startup>>();
                JobManager.Initialize(services.GetService<JobsRegister>());
                logger.LogInformation(env.EnvironmentName);

                Console.ReadLine();
                JobManager.Stop();
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }


            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });
            });


        }

    }
}
