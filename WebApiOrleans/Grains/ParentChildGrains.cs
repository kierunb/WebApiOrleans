namespace WebApiOrleans.Grains;

public interface IParentGrain : IGrainWithIntegerKey
{
    Task SayHello(string greeting);
}

public interface IChildGrain : IGrainWithIntegerKey
{
    Task SayHello(string greeting, IParentGrain parentGrain);
}

public interface IGrandChild : IGrainWithIntegerKey
{
    Task SayHello(string greeting);
}

public class ParentGrain : Grain, IParentGrain
{
    private readonly IGrainFactory _grainFactory;
    private readonly ILogger<ParentGrain> _logger;

    public ParentGrain(IGrainFactory grainFactory, ILogger<ParentGrain> logger)
    {
        _grainFactory = grainFactory;
        _logger = logger;
    }

    public async Task SayHello(string greeting)
    {
        _logger.LogInformation("Parent Grain message received: greeting = '{greeting}'", greeting);

        var reference = this.AsReference<IParentGrain>();

        var tasks = Enumerable
            .Range(0, 5)
            .Select(async _ =>
            {
                var randomChildId = Rand.Next();
                _logger.LogInformation(
                    "Parent Grain sending message to Child Grain: randomChildId = '{randomChildId}'",
                    randomChildId
                );
                var childGrain = _grainFactory.GetGrain<IChildGrain>(randomChildId);
                await childGrain.SayHello(greeting, reference);
            });

        await Task.WhenAll(tasks);
    }
}

public class ChildGrain : Grain, IChildGrain
{
    private readonly IGrainFactory _grainFactory;
    private readonly ILogger<ChildGrain> _logger;

    public ChildGrain(IGrainFactory grainFactory, ILogger<ChildGrain> logger)
    {
        _grainFactory = grainFactory;
        _logger = logger;
    }

    public async Task SayHello(string greeting, IParentGrain parentGrain)
    {
        _logger.LogInformation(
            "Child Grain '{childGrain}' message received: greeting = '{greeting} from Parent: '{parentId}'",
            this.GetGrainId().GetIntegerKey(),
            greeting,
            parentGrain.GetGrainId().GetIntegerKey()
        );

        var tasks = Enumerable
            .Range(0, 5)
            .Select(async _ =>
            {
                var randomChildId = Rand.Next();
                _logger.LogInformation(
                    "Child Grain sending message to Grand Child Grain: randomChildId = '{randomChildId}'",
                    randomChildId
                );
                var grandChildGrain = _grainFactory.GetGrain<IGrandChild>(randomChildId);
                await grandChildGrain.SayHello(greeting);
            });

        await Task.WhenAll(tasks);
    }
}

public class GrandChildGrain : Grain, IGrandChild
{
    private readonly ILogger<GrandChildGrain> _logger;

    public GrandChildGrain(ILogger<GrandChildGrain> logger)
    {
        _logger = logger;
    }

    public Task SayHello(string greeting)
    {
        _logger.LogInformation(
            "Grand Child '{grandChildId}' message received: greeting = '{greeting}'",
            this.GetGrainId().GetIntegerKey(),
            greeting
        );
        return Task.CompletedTask;
    }
}

public static class Rand
{
    private static readonly Random _random = new();

    public static int Next()
    {
        return _random.Next(1, 100);
    }
}
