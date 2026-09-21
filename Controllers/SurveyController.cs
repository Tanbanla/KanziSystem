using Microsoft.AspNetCore.Mvc;

namespace PRJ_WAREHOUSE_BIVN.Controllers
{
    public class SurveyController : BaseAuthController
    {
        public IActionResult CreateSurvey()
        {
            return View();
        }
        public IActionResult ReceiveSurvey()
        {
            return View();
        }
        public IActionResult ManageSurvey()
        {
            return View();
        }
        public IActionResult ApproveSurvey()
        {
            return View();

        }
    }
}
