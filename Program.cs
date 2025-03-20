using MassTransit;

class Program
{
    static async Task Main()
    {
        Console.WriteLine(" [*] MassTransitConsumer Starting...");

        var busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
        {
            cfg.Host("rabbitmq", h =>
            {
                h.Username("admin");
                h.Password("A123231312a@");
            });

            cfg.ReceiveEndpoint("outbox-queue", e =>
            {
                e.Consumer<OutboxMessageConsumer>();
            });
        });

        await busControl.StartAsync();
        Console.WriteLine(" [✔] MassTransitConsumer Listening... Press Enter to exit.");
        
        Console.ReadLine();
        await busControl.StopAsync();
    }
}