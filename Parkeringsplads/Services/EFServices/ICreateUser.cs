namespace Parkeringsplads.Services.EFServices
{
    public interface ICreateUser
    {
        Task<bool> CreateUserAsync(User user, string addressRoad, string addressNumber, int cityId);

    }
}
