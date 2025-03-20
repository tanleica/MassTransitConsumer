using System.Text;
using System.Text.Json;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace MassTransitConsumer;
public class OutboxMessageConsumer : IConsumer<OutboxMessage>
{
    private static readonly HttpClient httpClient = new();

    public async Task Consume(ConsumeContext<OutboxMessage> context)
    {
        var message = context.Message;

        Console.WriteLine($" [✔] Received Outbox Message: {message.Message}");

        var url = "https://alpha.histaff.vn/api/WebPush/SendSimpleWebPushNotification";
        var payload = new { message.UserId, message.Message };

        var jsonPayload = JsonSerializer.Serialize(payload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync(url, content);
        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine($" [✔] Push notification sent to {message.UserId}");

            // ✅ Mark message as Processed in DB
            await MarkMessageAsProcessed(message.Id);
        }
        else
        {
            Console.WriteLine($" [!] Failed to send push notification: {response.StatusCode}");
        }
    }

    private static async Task MarkMessageAsProcessed(long messageId)
    {
        try
        {
            using var dbContext = new OutboxDbContext();
            var message = await dbContext.OutboxMessages.FirstOrDefaultAsync(m => m.Id == messageId);

            if (message != null)
            {
                message.Processed = true;
                await dbContext.SaveChangesAsync();
                Console.WriteLine($" [✔] Marked Outbox Message {messageId} as Processed.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($" [!] Failed to update Processed flag: {ex.Message}");
        }
    }
}
