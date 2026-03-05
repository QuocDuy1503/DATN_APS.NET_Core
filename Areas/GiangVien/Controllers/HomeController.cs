using Microsoft.AspNetCore.Mvc;

namespace DATN_TMS.Areas.GiangVien.Controllers
{
    [Area("GiangVien")]
    public class HomeController : Controller
    {
        public IActionResult DeXuatDT()
        {
            return View("~/Areas/Giangvien/Views/QuanLyDotDoAn/DeXuatDT.cshtml");
        }
    }

}
