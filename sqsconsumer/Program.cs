using Amazon;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Amazon.SQS;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace sqsconsumer
{
    public class Program
    {
        public static void Main(string[] args)
        {


            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostingContext, services) =>
                {
                    var appConfig = new ConfigurationBuilder()
                        .AddJsonFile(@"Properties/appsettings.json", optional: false)
                        .Build();

                    var options = appConfig.GetAWSOptions();
                    //var options = hostingContext.Configuration.GetAWSOptions();
                    
                    
                    services.AddDefaultAWSOptions(options);
                    services.AddAWSService<IAmazonSQS>();                
          

                    //worker service
                    services.AddHostedService<Worker>();
                });
    }
}
