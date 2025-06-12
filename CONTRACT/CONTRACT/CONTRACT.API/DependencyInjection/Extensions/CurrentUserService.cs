using CONTRACT.CONTRACT.DOMAIN.Abstractions.Repositories;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace CONTRACT.CONTRACT.API.DependencyInjection.Extensions;
public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private readonly ClaimsPrincipal? _claimsPrincipal = httpContextAccessor?.HttpContext?.User;

    public Guid UserId => Guid.Parse(_claimsPrincipal?.FindFirst("userId")?.Value ?? Guid.Empty.ToString());
    public string Role => _claimsPrincipal?.FindFirst(ClaimTypes.Role)?.Value ?? Guid.Empty.ToString();

    public string UserName => _claimsPrincipal?.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
}