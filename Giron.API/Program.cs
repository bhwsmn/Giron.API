using System;
using System.Threading.Tasks;
using Giron.API.DbContexts;
using Giron.API.Models.Constants;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Giron.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            
#if !DEBUG
            using var scope = host.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<GironContext>();
            await db.Database.MigrateAsync();
#endif
            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}