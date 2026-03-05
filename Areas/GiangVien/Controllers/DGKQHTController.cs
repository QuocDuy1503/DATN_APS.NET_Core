using Microsoft.AspNetCore.Mvc;

namespace DATN_TMS.Areas.GiangVien.Controllers
{
    [Area("GiangVien")]
    public class DGKQHTController : Controller
    {
        public IActionResult DS_DGKQHT()
        {
            return View();
        }
    }
}
