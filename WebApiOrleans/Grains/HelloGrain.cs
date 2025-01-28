using Orleans;

namespace WebApiOrleans.Grains;

public interface IHelloGrain : IGrainWithStringKey
{
    ValueTask<string> SayHello(string greeting);
    ValueTask<string> SayHelloUser(string greeting, int userId);
}

public class HelloGrain(ILogger<HelloGrain> logger) : Grain, IHelloGrain
{
    public ValueTask<string> SayHello(string greeting)
    {
        logger.LogInformation("SayHello message received: greeting = '{greeting}'", greeting);
        return new ValueTask<string>($"You said: '{greeting}', I say: Hello!");
    }

    public async ValueTask<string> SayHelloUser(string greeting, int userId)
    {
        logger.LogInformation("SayHello message received: greeting = '{greeting}'", greeting);

        var userGrain = GrainFactory.GetGrain<IUserGrain>(userId);
        var userData = await userGrain.GetUserData();

        return $"You said: '{greeting}' to '{userData}'";
    }
}

