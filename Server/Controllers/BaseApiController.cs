using iSportsRecruiting.Shared.Services;
using Microsoft.AspNetCore.Mvc;

namespace iSportsRecruiting.Server.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public abstract class BaseApiController<T> : ControllerBase
    {
        //private ITokenService _identityInstance;
        private IDatabaseContext _contextInstance;
        // private IMediator _mediatorInstance;
        // private ILogger<T> _loggerInstance;
        // IHubContext<SignalRHub> _hubContextInstance;
        //
        //protected ITokenService _identityService => _identityInstance ??= HttpContext.RequestServices.GetService<ITokenService>();
        // protected IMediator _mediator => _mediatorInstance ??= HttpContext.RequestServices.GetService<IMediator>();
        // protected ILogger<T> _logger => _loggerInstance ??= HttpContext.RequestServices.GetService<ILogger<T>>();
        protected IDatabaseContext _context => _contextInstance ??= HttpContext.RequestServices.GetService<IDatabaseContext>();
        // protected IHubContext<SignalRHub> _hubContext => _hubContextInstance ??= HttpContext.RequestServices.GetRequiredService<IHubContext<SignalRHub>>();
    }
}