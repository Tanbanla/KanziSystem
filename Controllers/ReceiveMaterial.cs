using Microsoft.AspNetCore.Mvc;

namespace PRJ_WAREHOUSE_BIVN.Controllers
{
    public class ReceiveMaterial : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}
