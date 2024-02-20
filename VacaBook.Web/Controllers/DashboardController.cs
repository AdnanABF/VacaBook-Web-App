using Microsoft.AspNetCore.Mvc;

namespace VacaBook.Web.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
