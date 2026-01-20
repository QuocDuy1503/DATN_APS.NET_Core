using Microsoft.AspNetCore.Mvc;

namespace DATN_TMS.Areas.Giangvien.Controllers
{
    [Area("Giangvien")]
    public class HomeController : Controller
    {
        public IActionResult DeXuatDT()
        {
            return View("~/Areas/Giangvien/Views/QuanLyDotDoAn/DeXuatDT.cshtml");
        }
    }

}
