using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using PRJ_WAREHOUSE_BIVN.Models;
using System.Data;
using System.Xml.Linq;

namespace PRJ_WAREHOUSE_BIVN.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public PartialViewResult _modal()
        {
            return PartialView();
        }
        [HttpPost]
        public JsonResult Load_User(Search_param para)
        {
            List<MST_USER> dt = MST_USER.user_process(para );
            return Json(dt);
        }
        [HttpPost]
        public JsonResult Insert_Edit_User(string name, string adid, string staffcode, string dept, string role, string mail)      
        {
           string result =  MST_USER.insert_update_users(name, adid, staffcode, dept, role, mail, adid, "1");
           return Json(result);
        }
        public ActionResult ThemUser_Phongtiepnhan()
        {
            return View();
        }
    }
}
