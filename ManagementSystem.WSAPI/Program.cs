using Microsoft.AspNetCore.Hosting;
using System.Security.Cryptography.X509Certificates;

namespace ManagementSystem.WSAPI
{
    public class Program
    {

        public static void Main(string[] args)
        {

            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            CreateHostBuilder(args).Build().Run();
            
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .UseWindowsService();
    }
}
