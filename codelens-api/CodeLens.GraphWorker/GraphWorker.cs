namespace CodeLens.GraphWorker;
using StackExchange.Redis;

public class GraphWorker(
    ILogger<GraphWorker> logger,
    IConnectionMultiplexer redis,
    IServiceScopeFactory scopeFactory
    ) : BackgroundService
{

    private const string StreamName ="graph-jobs";
    private const string GroupName = "graph-group";

    private const string ConsumerName ="graph-worker-1";
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        var db = redis.GetDatabase();

        try
        {
            await db.StreamCreateConsumerGroupAsync(StreamName, GroupName, "0", createStream:true);
        }
        catch (RedisException)
        {
            
        }

        logger.LogInformation("Graph worker started, listening for jobs");

        await RecoverPendingAsync(db,ct);

        while (!ct.IsCancellationRequested)
        {
            try
            {
                var messages = await db.StreamReadGroupAsync(
                    StreamName, GroupName, ConsumerName, ">",count:1
                );
                if(messages == null || messages.Length == 0)
                {
                    await Task.Delay(1000,ct);
                    continue;
                }
                foreach(var message in messages)
                {
                    await ProcessMessageAsync(db,message, ct);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "Graph worker error, retrying in 5s");
                await Task.Delay(5000, ct);
            }
        }
    }

    private async Task RecoverPendingAsync(IDatabase db, CancellationToken ct)
    {
        var pending = await db.StreamReadGroupAsync(
            StreamName, GroupName, ConsumerName, "0", count:100
        );
        if(pending?.Length > 0)
        {
            logger.LogInformation("Recovering {count} pending graph jobs", pending.Length);
            foreach(var message in pending)
            {
                await ProcessMessageAsync(db, message, ct);
            }
        }
    }

        private async Task ProcessMessageAsync(IDatabase db, StreamEntry message, CancellationToken ct)
    {
        if(!Guid.TryParse(message["repoId"].ToString(),out var repoId))
        {
            logger.LogError("Invalid repoId in message");
            await db.StreamAcknowledgeAsync(StreamName, GroupName, message.Id);
            return;

        }

    

        try
        {
            using var scope = scopeFactory.CreateScope();
            // graph processor goes here next
            logger.LogInformation("Processing graph job for repo {RepoId}", repoId);

            await db.StreamAcknowledgeAsync(StreamName, GroupName, message.Id);
            logger.LogInformation("Graph job completed for repo {RepoId}", repoId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process graph job for repo {RepoId}", repoId);
            // no ACK → stays in pending → recovered on restart
        }
}
