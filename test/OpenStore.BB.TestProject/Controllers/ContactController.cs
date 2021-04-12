using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace OpenStore.BB.TestProject.Controllers
{
    public class ContactController : Controller
    { 
        public IActionResult Index() => View();
    }
}