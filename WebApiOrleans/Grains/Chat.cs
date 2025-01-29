using Orleans.Utilities;

namespace WebApiOrleans.Grains;

// interface of the client that will receive the message.
public interface IChatObserver : IGrainObserver
{
    Task ReceiveMessage(string message);
}

public class ChatObserver(ILogger<ChatObserver> logger) : IChatObserver
{
    public Task ReceiveMessage(string message)
    {
        logger.LogInformation("Received message: {message}", message);
        return Task.CompletedTask;
    }
}

public interface IClientChatGrain : IGrainWithStringKey
{ 
    Task Subscribe(IChatObserver observer); 
    Task UnSubscribe(IChatObserver observer);
    Task SendUpdateMessage(string message);
}

public class ClientChatGrain : Grain, IClientChatGrain
{
    private readonly ObserverManager<IChatObserver> _subsManager;
    private readonly ILogger<ClientChatGrain> _logger;

    public ClientChatGrain(ILogger<ClientChatGrain> logger)
    {
        _subsManager =
            new ObserverManager<IChatObserver>(
                expiration: TimeSpan.FromMinutes(5), logger);
        _logger = logger;
    }

    // Clients call this to subscribe.
    public Task Subscribe(IChatObserver observer)
    {
        _subsManager.Subscribe(observer, observer);
        _logger.LogInformation("Subscribed to chat. User: {user}", this.GetGrainId());
        return Task.CompletedTask;
    }

    //Clients use this to unsubscribe and no longer receive messages.
    public Task UnSubscribe(IChatObserver observer)
    {
        _subsManager.Unsubscribe(observer);
        _logger.LogInformation("Unsubscribed from chat. User: {user}", this.GetGrainId());
        return Task.CompletedTask;
    }

    public Task SendUpdateMessage(string message)
    {
        _subsManager.Notify(s => s.ReceiveMessage(message));
        _logger.LogInformation("Notification sent");
        return Task.CompletedTask;
    }
}