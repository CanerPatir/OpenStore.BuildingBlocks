using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace OpenStore.BB.TestProject.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IStringLocalizer<HomeController> _localizer;

    public HomeController(ILogger<HomeController> logger, IStringLocalizer<HomeController> localizer)
    {
        _logger = logger;
        _localizer = localizer;
    }

    public IActionResult Index() => View("Index", _localizer["ControllerKey"].ToString());
}