using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using AspectCore.Extensions.DependencyInjection;

namespace Zxw.Framework.Website
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateWebHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(new AspectCoreServiceProviderFactory())
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseKestrel(config =>
                    {
                        config.ListenAnyIP(8188);
                    })
                    //.ConfigureAppConfiguration((context, config) =>
                    //{
                    //    // Configure the app here.
                    //})
                    .UseStartup<Startup>();
                })
                ;
        }
    }
}
