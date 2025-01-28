// https://learn.microsoft.com/en-us/dotnet/orleans/grains/grain-persistence/?pivots=orleans-7-0

namespace WebApiOrleans.Grains;
public interface IUserGrain : IGrainWithIntegerKey
{
    Task SetUserData(string firstName, string lastName);
    Task<string> GetUserData();
}


public class UserGrain : Grain, IUserGrain
{
    private readonly IPersistentState<UserGrainState> _state;

    public UserGrain(
         [PersistentState("state", "db")] IPersistentState<UserGrainState> state)
    {
        _state = state;
    }

    public async Task SetUserData(string firstName, string lastName)
    {
        _state.State.FirstName = firstName;
        _state.State.LastName = lastName;

        await _state.WriteStateAsync();
    }

    public Task<string> GetUserData()
    {
        return Task.FromResult($"{_state.State.FirstName} {_state.State.LastName}");
    }
}

[Serializable]
public class UserGrainState
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}
