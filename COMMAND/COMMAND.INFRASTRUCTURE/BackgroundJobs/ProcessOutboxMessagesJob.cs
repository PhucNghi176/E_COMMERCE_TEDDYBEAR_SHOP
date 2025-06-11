using COMMAND.PERSISTENCE;
using COMMAND.PERSISTENCE.Outbox;
using CONTRACT.CONTRACT.CONTRACT.Abstractions.Messages;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Quartz;

namespace COMMAND.INFRASTRUCTURE.BackgroundJobs;
[DisallowConcurrentExecution]
public class ProcessOutboxMessagesJob(ApplicationDbContext dbContext, IPublishEndpoint publishEndpoint)
    : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var messages = await dbContext
            .Set<OutboxMessage>()
            .Where(m => m.ProcessedOnUtc == null)
            .OrderBy(m => m.OccurredOnUtc)
            .Take(20)
            .ToListAsync(context.CancellationToken);

        foreach (var outboxMessage in messages)
        {
            var domainEvent = JsonConvert
                .DeserializeObject<IDomainEvent>(
                    outboxMessage.Content,
                    new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.All
                    });

            if (domainEvent is null)
                continue;

            try
            {
                switch (domainEvent.GetType().Name)
                {
                    // case nameof(PostgreMigrateDomainEvent.PostgreMigrate):
                    //     var postgreMigrate =
                    //         JsonConvert.DeserializeObject<PostgreMigrateDomainEvent.PostgreMigrate>(
                    //             outboxMessage.Content,
                    //             new JsonSerializerSettings
                    //             {
                    //                 TypeNameHandling = TypeNameHandling.All
                    //             });
                    //     await _publishEndpoint.Publish(postgreMigrate, context.CancellationToken);
                    //     break;

                }

                outboxMessage.ProcessedOnUtc = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                outboxMessage.Error = ex.Message;
            }
        }

        await dbContext.SaveChangesAsync();
    }
}