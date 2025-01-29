using System.Globalization;

namespace WebApiOrleans.Extensions;

public interface IGrainDeactivateExtension : IGrainExtension
{
    Task Deactivate(string msg);
}

public sealed class GrainDeactivateExtension : IGrainDeactivateExtension
{
    private IGrainContext _context;

    public GrainDeactivateExtension(IGrainContext context)
    {
        _context = context;
    }

    public Task Deactivate(string msg)
    {
        var reason = new DeactivationReason(DeactivationReasonCode.ApplicationRequested, msg);
        _context.Deactivate(reason);
        return Task.CompletedTask;
    }
}

// registration
// siloBuilder.AddGrainExtension<IGrainDeactivateExtension, GrainDeactivateExtension>();

// usage
//var grain = client.GetGrain<SomeExampleGrain>(someKey);
//var grainReferenceAsInterface = grain.AsReference<IGrainDeactivateExtension>();

//await grainReferenceAsInterface.Deactivate("Because, I said so...");