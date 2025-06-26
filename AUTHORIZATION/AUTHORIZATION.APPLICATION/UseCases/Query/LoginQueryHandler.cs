using System.Security.Claims;
using AUTHORIZATION.CONTRACT.Identity;
using CONTRACT.CONTRACT.CONTRACT.Abstractions.Messages;
using CONTRACT.CONTRACT.CONTRACT.Abstractions.Shared;
using CONTRACT.CONTRACT.DOMAIN.Abstractions.Repositories;
using CONTRACT.CONTRACT.DOMAIN.Entities;
using CONTRACT.CONTRACT.DOMAIN.Exceptions;
using CONTRACT.CONTRACT.APPLICATION.Abstractions;

namespace AUTHORIZATION.APPLICATION.UseCases.Query;
internal sealed class LoginQueryHandler(IRepositoryBase<User, int> userRepositoryBase, IJwtTokenService jwtTokenService)
    : IQueryHandler<CONTRACT.Identity.Query.LoginQuery, Response.Authenticated>
{
    public async Task<Result<Response.Authenticated>> Handle(CONTRACT.Identity.Query.LoginQuery request,
        CancellationToken cancellationToken)
    {
        var user = await userRepositoryBase.FindSingleAsync(x => x.Email.Equals(request.Email), cancellationToken);

        if (user is null)
        {
            throw new UserException.NotFound();
        }

        if (user.Password != request.Password)
        {
            throw new UserException.PasswordMismatch();
        }

        var claims = new List<Claim>(2)
        {
            new("name", user.UserName),
            new("email", user.Email)
        };
        var token = jwtTokenService.GenerateAccessToken(claims);
        var refreshToken = jwtTokenService.GenerateRefreshToken();
        return Result.Success(new Response.Authenticated(token, refreshToken));
    }
}