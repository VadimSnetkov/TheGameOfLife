using FluentValidation;
using GameOfLife.ConsoleApp.Models;
using GameOfLife.ConsoleApp.UI;
using GameOfLife.ConsoleApp.Validator;
using GameOfLife.Core.Models;
using GameOfLife.Core.Service;
using GameOfLife.Core.Validator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GameOfLife.ConsoleApp
{
    internal class Program
    {
        /// Summary:
        /// Registers configuration and services, then starts the console application.
        static async Task Main()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddSingleton<IConfiguration>(_ => new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .Build());

            services.AddSingleton<IValidator<BoardRequest>, BoardRequestValidator>();
            services.AddSingleton<IValidator<MenuRequest>, MenuRequestValidator>();
            services.AddSingleton<IValidator<GameSaveData>, GameSaveDataValidator>();

            services.AddSingleton<IValidator<SessionRequest>, SessionRequestValidator>();
            services.AddSingleton<IValidator<SessionSaveData>, SessionSaveDataValidator>();
            services.AddSingleton<IValidator<ViewSelection>, ViewSelectionValidator>();
            services.AddSingleton<SessionService>();

            services.AddSingleton<Random>(_ => Random.Shared);
            services.AddSingleton<BoardService>();
            services.AddSingleton<BoardFactory>();
            services.AddSingleton<GameService>();
            services.AddSingleton<GameFileService>();
            services.AddSingleton<ConsoleRenderer>();
            services.AddSingleton<SessionRunner>();
            services.AddSingleton<ConsoleApplication>();

            using ServiceProvider provider = services.BuildServiceProvider();

            var application = provider.GetRequiredService<ConsoleApplication>();
            await application.RunAsync();
        }
    }
}
