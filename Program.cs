using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;

class Program
{
    private static readonly HttpClient httpClient = new();

    static void Main()
    {
        var factory = new ConnectionFactory()
        {
            // 🔹 Use "rabbitmq" if running in Docker Compose or container network
            // 🔹 Use host machine's IP if running the consumer on the host
            HostName = "159.223.59.17",  // Replace with "rabbitmq" if running in Docker
            Port = 5672,                 // Ensure this port is correctly mapped
            UserName = "admin",
            Password = "A123231312a@",
            VirtualHost = "/"
        };

        while (true) // Auto-reconnect loop
        {
            try
            {
                using var connection = factory.CreateConnection();
                using var channel = connection.CreateModel();

                channel.QueueDeclare(queue: "test_queue",
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += async (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine($" [x] Received: {message}");

                    // ✅ Save message using EF Core
                    SaveMessageToDatabase(message);

                    // Extract userId from the message if applicable
                    string userId = "b2e05ec4-6022-4f35-baea-ceb7fa2ee9dd"; // Replace with extracted userId

                    // ✅ Send Push Notification
                    await SendPushNotificationAsync(userId, message);

                    // ✅ Acknowledge message
                    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                };

                channel.BasicConsume(queue: "test_queue",
                                     autoAck: false,
                                     consumer: consumer);

                Console.WriteLine(" [*] Waiting for messages. Press [ENTER] to exit.");

                bool isDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";

                if (isDocker)
                {
                    Thread.Sleep(Timeout.Infinite); // ✅ Keeps the process alive without blocking
                }
                else
                {
                    Console.ReadLine();
                    break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($" [!] RabbitMQ connection failed: {ex.Message}. Retrying in 5 seconds...");
                Thread.Sleep(5000);
            }
        }
    }

    // ✅ Save Message using EF Core
    static void SaveMessageToDatabase(string message)
    {
        try
        {
            using var db = new MessageDbContext();
            db.Messages.Add(new MessageLog { Message = message });
            db.SaveChanges();
            Console.WriteLine(" [✔] Message saved to SQL Server.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[!] Database Save Error: {ex.Message}");
            Console.WriteLine($"[!] StackTrace: {ex.StackTrace}");
        }
    }

        // ✅ Web Push Notification Sender
    static async Task SendPushNotificationAsync(string userId, string message)
    {
        try
        {
            var url = "https://alpha.histaff.vn/api/WebPush/SendSimpleWebPushNotification";
            var payload = new
            {
                userId,
                message
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($" [✔] Push notification sent to {userId}");
            }
            else
            {
                Console.WriteLine($" [!] Failed to send push notification: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Push Notification Error: {ex.Message}");
        }
    }
}
