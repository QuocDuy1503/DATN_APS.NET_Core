using Microsoft.AspNetCore.Mvc;

namespace DATN_TMS.Areas.GiangVien.Controllers
{
    [Area("GiangVien")]
    public class QLSVController : Controller
    {
        public IActionResult DSSV()
        {
            return View();
        }
        public IActionResult Nhatky()
        {
            return View();
        }
        public IActionResult Kehoach()
        {
            return View();
        }
        public IActionResult XCT_KH()
        {
            return View();
        }
    }
}
