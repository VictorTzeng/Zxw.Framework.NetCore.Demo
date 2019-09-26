using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

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
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseKestrel(config =>
                    {
                        config.ListenAnyIP(80);
                    })
                    .ConfigureAppConfiguration((context, config) =>
                    {
                        // Configure the app here.
                    })
                    .UseStartup<Startup>();
                })
                ;
        }
    }
}
