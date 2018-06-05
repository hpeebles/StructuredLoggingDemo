using System;
using System.Net;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Sinks.Elasticsearch;

namespace Hotels
{
    public class LoggerFactory
    {
        private readonly IConfiguration _configuration;

        public LoggerFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        public ILogger Build()
        {
            var elasticSearchOptions = new ElasticsearchSinkOptions(new Uri(_configuration["elasticSearchEndpoint"]));

            return new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Facility", "HotelsSample")
                .Enrich.WithProperty("InstanceId", Amazon.Util.EC2InstanceMetadata.InstanceId)
                .WriteTo.Elasticsearch(elasticSearchOptions)
                .WriteTo.Console()
                .WriteTo.Udp(IPAddress.Loopback, 7071, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}{NewLine}{Properties}")
                .CreateLogger();
        }
    }
}