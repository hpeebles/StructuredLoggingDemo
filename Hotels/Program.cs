using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using ILogger = Serilog.ILogger;

namespace Hotels
{
    public class Program
    {
        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += HandleException;
            
            var host = new HostBuilder()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile("appsettings.json");
                })
                .ConfigureLogging((context, logging) =>
                {
                    var loggerFactory = new LoggerFactory(context.Configuration);

                    Log.Logger = loggerFactory.Build();

                    logging.AddSerilog();
                })
                .ConfigureServices((context, services) =>
                {
                    Log.Information("Service starting");

                    services.AddSingleton(Log.Logger);
                    services.AddSingleton(p => new SearchEngine(BuildSuppliers(), p.GetService<ILogger>()));

                    for (var i = 1; i < 10; i++)
                    {
                        var visitSourceId = i;
                        
                        services.AddSingleton<IHostedService>(p => new SearchTrigger(
                            visitSourceId,
                            p.GetService<SearchEngine>(),
                            p.GetService<ILogger>()));
                    }
                })
                .UseConsoleLifetime()
                .Build();
            
            Log.Information("Service started");

            try
            {
                host.Run();
                
                Log.Information("Service stopped");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Service terminated unexpectedly");
                throw;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static void HandleException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            
            Log.Error(exception, "Unhandled exception");
        }

        private static IList<Supplier> BuildSuppliers()
        {
            return Enumerable
                .Range(1, 10)
                .Select(id => new Supplier(id, TimeSpan.FromSeconds(id), Math.Pow(id, 0.5), id, Log.Logger))
                .ToList();
        }
    }
}