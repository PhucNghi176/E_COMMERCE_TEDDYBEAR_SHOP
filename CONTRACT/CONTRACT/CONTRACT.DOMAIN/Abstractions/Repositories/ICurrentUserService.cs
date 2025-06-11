namespace CONTRACT.CONTRACT.DOMAIN.Abstractions.Repositories;
public interface ICurrentUserService
{
    Guid UserId { get; }
    string UserName { get; }
    string Role { get; }

}