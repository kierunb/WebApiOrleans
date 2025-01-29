using Microsoft.Extensions.Logging;

namespace WebApiOrleans.Filters;

public class LoggingCallFilter : IIncomingGrainCallFilter
{
    private readonly Logger<LoggingCallFilter> _logger;

    public LoggingCallFilter(Factory<string, Logger<LoggingCallFilter>> loggerFactory)
    {
        _logger = loggerFactory(nameof(LoggingCallFilter));
    }

    public async Task Invoke(IIncomingGrainCallContext context)
    {
        try
        {
            await context.Invoke();
            var msg = string.Format(
                "{0}.{1}({2}) returned value {3}",
                context.Grain.GetType(),
                context.InterfaceMethod.Name,
                string.Join(", ", context.Request.GetArgumentCount()),
                context.Result);
            _logger.LogInformation(msg);
        }
        catch (Exception exception)
        {
            var msg = string.Format(
                "{0}.{1}({2}) threw an exception: {3}",
                context.Grain.GetType(),
                context.InterfaceMethod.Name,
                string.Join(", ", context.Request.GetArgumentCount()),
                exception);
            _logger.LogInformation(msg);

            // If this exception is not re-thrown, it is considered to be
            // handled by this filter.
            throw;
        }
    }
}

// registration: siloHostBuilder.AddIncomingGrainCallFilter<LoggingCallFilter>();