namespace WebApiOrleans.Grains;

// https://learn.microsoft.com/en-us/dotnet/orleans/grains/timers-and-reminders

public interface ITimerGrain : IGrainWithGuidKey
{
    void StartTimer();
    void StopTimer();
}

public class TimerGrain : Grain, ITimerGrain
{
    private IDisposable _timer;

    public void StartTimer()
    {
        Console.WriteLine($"Timer Started {DateTime.Now}");
    }

    public void StopTimer()
    {

        Console.WriteLine($"Timer Stopped {DateTime.Now}");
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        int count = 1;

        //_timer = RegisterTimer()

        //_timer = RegisterTimer(state =>
        //{
        //    Console.WriteLine($"Timer number: {count}");
        //    count++;
        //    return base.OnActivateAsync(cancellationToken);
        //}, null,
        //TimeSpan.FromSeconds(3),
        //TimeSpan.FromSeconds(2));

        return base.OnActivateAsync(cancellationToken);

    }
}
