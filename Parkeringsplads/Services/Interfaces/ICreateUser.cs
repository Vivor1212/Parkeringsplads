namespace Parkeringsplads.Services.Interfaces
{
    public interface ICreateUser
    {
        Task<bool> CreateUserAsync(User user, string addressRoad, string addressNumber, int cityId);

    }
}
