using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using OpenStore.Application;

namespace OpenStore.Infrastructure;

public class OpenStoreHttpUserContextAccessor : IOpenStoreUserContextAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public OpenStoreHttpUserContextAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetUserEmail()
    {
        return _httpContextAccessor
            .HttpContext?
            .User?.Claims
            .FirstOrDefault(x => x.Type == "email" || x.Type == ClaimTypes.Email)?
            .Value;
    }
}