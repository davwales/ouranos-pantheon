using System.Reflection;
using MassTransit;
using Ouranos.Pantheon.Core.Application;
using Ouranos.Pantheon.Core.Common.AsyncLocks;
using Ouranos.Pantheon.Core.Infra.Mongo;
using Ouranos.Pantheon.Core.Infra.RabbitMq;
using Serilog;

namespace Ouranos.Pantheon.DataLoader.Plutus.Consumer.Startup;

public static class HostingExtensions
{
    public static IHost ConfigureBuilder(this HostApplicationBuilder builder)
    {
        var loggerConfig = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration);
        Log.Logger = loggerConfig.CreateLogger();

        builder.Services
            .AddSingleton<IKeyedAsyncLock<string>, KeyedAsyncLock<string>>()
            .AddCoreApplicationModule()
            .AddCoreMongo(builder.Configuration)
            .AddCoreRabbitMqModule(builder.Configuration, x =>
            {
                x.AddConsumer<TradeConsumer>();
                x.AddMediator(m => m.AddConsumers(Assembly.GetExecutingAssembly()));
            });

        return builder.Build();
    }
}