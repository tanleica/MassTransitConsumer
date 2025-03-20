using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace MassTransitConsumer;
class Program
{
    static async Task Main()
    {
        Console.WriteLine(" [*] MassTransitConsumer is starting...");

        // 🔹 Set up Dependency Injection (DI)
        var services = new ServiceCollection();

        // 🔹 Register MassTransit and Consumer
        services.AddMassTransit(cfg =>
        {
            cfg.AddConsumer<OutboxMessageConsumer>(); // ✅ Register the consumer

            cfg.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host("159.223.59.17", h =>
                {
                    h.Username("admin");
                    h.Password("A123231312a@");
                });

                cfg.ReceiveEndpoint("outbox-queue_skipped", e =>
                {
                    e.ConfigureConsumer<OutboxMessageConsumer>(context); // ✅ Fix: use DI `context`
                });
            });
        });

        var serviceProvider = services.BuildServiceProvider();

        var busControl = serviceProvider.GetRequiredService<IBusControl>();

        await busControl.StartAsync();
        Console.WriteLine(" [*] MassTransitConsumer is now listening on 'outbox-queue'...");

        await Task.Delay(-1); // ✅ Keeps the app running
    }
}
