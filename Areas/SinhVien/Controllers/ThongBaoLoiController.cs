using DATN_TMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DATN_TMS.Areas.SinhVien.Controllers
{
    /// <summary>
    /// Controller xử lý các thông báo lỗi cho sinh viên
    /// Không kế thừa BaseSinhVienController để tránh vòng lặp redirect
    /// </summary>
    [Area("SinhVien")]
    public class ThongBaoLoiController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Chỉ kiểm tra đăng nhập, không kiểm tra nguyện vọng
            var sessionRole = HttpContext.Session.GetString("Role");
            var isStudentByClaim = User?.Identity?.IsAuthenticated == true && (User.IsInRole("SINH_VIEN") || User.IsInRole("SV"));
            var isStudentBySession = sessionRole == "SINH_VIEN" || sessionRole == "SV";

            if (!isStudentByClaim && !isStudentBySession)
            {
                context.Result = RedirectToAction("Login", "Account", new { area = "" });
                return;
            }
            base.OnActionExecuting(context);
        }

        /// <summary>
        /// Trang thông báo khi sinh viên chưa được duyệt nguyện vọng
        /// </summary>
        public IActionResult ChuaDuyetNguyenVong()
        {
            ViewBag.ErrorMessage = TempData["ErrorMessage"]?.ToString() 
                ?? "Bạn chưa đủ điều kiện để sử dụng chức năng này.";
            ViewBag.ErrorType = TempData["ErrorType"]?.ToString() ?? "CHUA_DANG_KY";
            
            return View();
        }
    }
}
