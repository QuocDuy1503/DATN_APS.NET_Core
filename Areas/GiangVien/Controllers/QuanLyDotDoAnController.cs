using Microsoft.AspNetCore.Mvc;

namespace DATN_TMS.Areas.Giangvien.Controllers
{
    [Area("Giangvien")]
    public class QuanLyDotDoAnController : Controller
    {
        public IActionResult DeXuatDT()
        {
            return View();
        }
        public IActionResult DuyetSV()
        {
            return View();
        }
        public IActionResult XCT_Dexuat(int id)
        {
            return View();
        }
        public IActionResult Duyet_DSSV(int id)
        {
            return View();
        }
    }
}
