global using Microsoft.AspNetCore.Mvc;

namespace AiSearch;

public class HomeController : Controller
{
    [Route("/")]
    public ActionResult Index() => View();
}
