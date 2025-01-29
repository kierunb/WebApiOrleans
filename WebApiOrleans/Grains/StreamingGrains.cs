using System.IO;
using Orleans.Streams;

namespace WebApiOrleans.Grains;

public interface IStreamProducerGrain : IGrainWithIntegerKey
{
    Task StartStreaming(string streamProviderName, string streamNS, Guid streamGuid);
}

public interface IStreamConsumerGrain : IGrainWithIntegerKey
{
    Task StartConsume(string streamProviderName, string streamNS, Guid streamGuid);
}

public class StreamProducerGrain(ILogger<StreamProducerGrain> logger) : Grain, IStreamProducerGrain
{
    public Task StartStreaming(string streamProviderName, string streamNS, Guid streamGuid)
    {
        var streamProvider = this.GetStreamProvider(streamProviderName);
        var streamId = StreamId.Create(streamNS, streamGuid);
        var stream = streamProvider.GetStream<int>(streamId);

        logger.LogInformation("Start Streaming");

        this.RegisterGrainTimer(
            callback: async _ =>
            {
                int item = Random.Shared.Next();
                logger.LogInformation("Producing item: '{item}'", item);
                await stream.OnNextAsync(item);
            },
            dueTime: TimeSpan.FromSeconds(1),
            period: TimeSpan.FromSeconds(1)
        );

        return Task.CompletedTask;
    }
}

public class StreamConsumerGrain(ILogger<StreamConsumerGrain> logger) : Grain, IStreamConsumerGrain
{
    private StreamSubscriptionHandle<int> consumerHandle;

    public async Task StartConsume(string streamProviderName, string streamNS, Guid streamGuid)
    {
        var streamProvider = this.GetStreamProvider(streamProviderName);
        var streamId = StreamId.Create(streamNS, streamGuid);
        var stream = streamProvider.GetStream<int>(streamId);

        consumerHandle = await stream.SubscribeAsync(OnNextAsync, OnErrorAsync, OnCompletedAsync);
    }

    public async Task StopConsuming()
    {
        logger.LogInformation("StopConsuming");
        if (consumerHandle != null)
        {
            await consumerHandle.UnsubscribeAsync();
            //consumerHandle.Dispose();
            consumerHandle = null;
        }
    }

    public Task OnNextAsync(int item, StreamSequenceToken token = default)
    {
        logger.LogInformation(
            "Consuming: ({Item}{Token})",
            item,
            token != null ? token.ToString() : "null"
        );
        return Task.CompletedTask;
    }

    public Task OnCompletedAsync()
    {
        logger.LogInformation("OnCompletedAsync()");
        return Task.CompletedTask;
    }

    public Task OnErrorAsync(Exception ex)
    {
        logger.LogInformation(ex, "OnErrorAsync()");
        return Task.CompletedTask;
    }
}
