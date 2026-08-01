using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRJ_WAREHOUSE_BIVN.Models;

namespace PRJ_WAREHOUSE_BIVN.Controllers
{
    public class ApprovalController : Controller
    {

        public IActionResult Approval()
        {
         
            return View();
        }
        public IActionResult Condition()
        {
            return View();
        }       
        public IActionResult ListData()
        {
            return View();
        }
        public IActionResult ListData_GA()
        {
            return View();
        }
    }
}
