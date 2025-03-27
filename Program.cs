using Microsoft.Extensions.Configuration;
using NavalBattle.Domain.Config;
using NavalBattle.Domain.Enums;
using NavalBattle.Domain.Models;
using NavalBattle.Domain.Services.Implementations;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile($"appsettings.Development.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

var settings = configuration.Get<AppSettings>();

var messageService = new MessageService(
    settings.ServiceBus.ConnectionString,
    settings.ServiceBus.TopicName,
    settings.ServiceBus.SubscriptionName,
    new CryptoService(settings.Ship.CryptoKey));

var battleService = new BattleService(
    messageService,
    new CryptoService(settings.Ship.CryptoKey),
    settings.Ship.Name);

var coordinator = new BattleCoordinator(battleService, messageService, settings.Ship.Name);
await coordinator.StartAsync();

Console.WriteLine("Batalha iniciada! Pressione qualquer tecla para sair...");
Console.ReadKey();
