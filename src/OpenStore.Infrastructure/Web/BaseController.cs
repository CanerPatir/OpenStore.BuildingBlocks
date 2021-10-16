using Microsoft.AspNetCore.Mvc;

namespace OpenStore.Infrastructure.Web;

public abstract class BaseController : ControllerBase
{
    protected CancellationToken CancellationToken => HttpContext.RequestAborted;
}