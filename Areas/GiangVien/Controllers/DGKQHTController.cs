using Microsoft.AspNetCore.Mvc;

namespace DATN_TMS.Areas.Giangvien.Controllers
{
    [Area("Giangvien")]
    public class DGKQHTController : Controller
    {
        public IActionResult DS_DGKQHT()
        {
            return View();
        }
    }
}
