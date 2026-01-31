using Microsoft.AspNetCore.Mvc;

namespace PRJ_WAREHOUSE_BIVN.Controllers
{
    public class ExportController : Controller
    {
        public IActionResult Export_material()
        {
            return View();
        }
    }
}
