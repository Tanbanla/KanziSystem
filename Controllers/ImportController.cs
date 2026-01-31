using Microsoft.AspNetCore.Mvc;
using PRJ_WAREHOUSE_BIVN.Models;

namespace PRJ_WAREHOUSE_BIVN.Controllers
{
    public class ImportController : Controller
    {
        public IActionResult Import_material()
        {
            return View();
        }
        public IActionResult Export_material()
        {
            return View();
        }
        public IActionResult Re_enter()
        {
            return View();
        }
        [HttpPost]
        public JsonResult _load_inv(MST_INVENTORY para)
        {
            List<MST_INVENTORY> dt = MST_INVENTORY.inventory_process(para);
            return Json(dt);
        }
        // lấy tên mã hàng hiển thị trong combobox
        public JsonResult _load_material(string group_code)
        {
            List<string> material = MST_INVENTORY._getname_material(group_code);
            return Json(material);
        }
        // lấy thông tin adid để đọc ra các mã cost, phòng ban
        public JsonResult _load_user_info()
        {
            //string us = User.Identity?.Name?.Contains("\\") == true ? User.Identity.Name.Split('\\')[1] : (User.Identity?.Name ?? "Unknown");

            string us = "loandt";
            List<User_Info> lst_cost = User_Info._info_adid(us);
            return Json(lst_cost);
        }
        public JsonResult _load_dept(string dept)
        {
            string us = "loandt";
            string cost = dept.Split(':')[0];
            string ss = dept.Split(':')[1];
            List<User_Info> lst_cost = User_Info._info_adid(us);
            var sec = lst_cost.Where(x => x.Name == ss && x.Cost_Center == cost).First();
            return Json(sec);
        }
        public JsonResult _info_material(PARAS para)
        { 
            List<PARAS> mst = MATERIA.material_process(para);
            return Json(mst);
        }
    }
}
