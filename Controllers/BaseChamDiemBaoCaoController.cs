using Microsoft.AspNetCore.Mvc;
using DATN_TMS.Areas.GiangVien.Models;
using DATN_TMS.Services;

namespace DATN_TMS.Controllers
{
    /// <summary>
    /// Base controller chứa toàn bộ logic chấm điểm báo cáo dùng chung cho GiangVien, BCNKhoa, GV_BoMon.
    /// Mỗi Area controller chỉ cần kế thừa và override OnActionExecuting để xác thực quyền.
    /// </summary>
    public abstract class BaseChamDiemBaoCaoController : Controller
    {
        protected readonly IChamDiemBaoCaoService _service;

        protected BaseChamDiemBaoCaoController(IChamDiemBaoCaoService service)
        {
            _service = service;
        }

        protected async Task<int> GetCurrentUserId()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            return await _service.GetCurrentUserId(userEmail);
        }

        [HttpGet]
        public async Task<IActionResult> CheckHoiDong()
        {
            var currentUserId = await GetCurrentUserId();
            if (currentUserId == 0)
                return Json(new { coHoiDong = false });

            var coHoiDong = await _service.CheckCoHoiDong(currentUserId);
            return Json(new { coHoiDong });
        }

        public async Task<IActionResult> Index()
        {
            var currentUserId = await GetCurrentUserId();
            if (currentUserId == 0)
                return RedirectToAction("Login", "Account", new { area = "" });

            var viewModels = await _service.GetDanhSachHoiDong(currentUserId);
            return View(viewModels);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var currentUserId = await GetCurrentUserId();
            if (currentUserId == 0)
                return RedirectToAction("Login", "Account", new { area = "" });

            var viewModel = await _service.GetChiTietHoiDong(id, currentUserId);
            if (viewModel == null)
            {
                TempData["ErrorMessage"] = _service.GetValidationError("") ?? "Có lỗi xảy ra.";
                return RedirectToAction(nameof(Index));
            }

            return View(viewModel);
        }

        public async Task<IActionResult> ChamDiem(int phienBaoVeId)
        {
            var currentUserId = await GetCurrentUserId();
            if (currentUserId == 0)
                return RedirectToAction("Login", "Account", new { area = "" });

            var viewModel = await _service.GetChamDiemViewModel(phienBaoVeId, currentUserId);
            if (viewModel == null)
            {
                var error = _service.GetValidationError("") ?? "Có lỗi xảy ra.";
                // Parse REDIRECT_DETAIL format
                if (error.StartsWith("REDIRECT_DETAIL:"))
                {
                    var parts = error.Split(':', 3);
                    if (parts.Length == 3 && int.TryParse(parts[1], out var hdId))
                    {
                        TempData["ErrorMessage"] = parts[2];
                        return RedirectToAction("Detail", new { id = hdId });
                    }
                }
                TempData["ErrorMessage"] = error;
                return RedirectToAction(nameof(Index));
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LuuDiem([FromBody] LuuDiemRequest request)
        {
            var currentUserId = await GetCurrentUserId();
            if (currentUserId == 0)
                return Json(new { success = false, message = "Không xác định được người dùng." });

            try
            {
                var result = await _service.LuuDiem(request, currentUserId);
                if (!result.Success)
                    return Json(new { success = false, message = result.Message });

                // Parse redirect URL
                string? redirectUrl = null;
                if (result.RedirectUrl != null)
                {
                    if (result.RedirectUrl.StartsWith("DETAIL:"))
                    {
                        var hdId = result.RedirectUrl.Replace("DETAIL:", "");
                        redirectUrl = Url.Action("Detail", new { id = int.Parse(hdId) });
                    }
                    else if (result.RedirectUrl.StartsWith("BANGDIEM:"))
                    {
                        var pbvId = result.RedirectUrl.Replace("BANGDIEM:", "");
                        redirectUrl = Url.Action("BangDiemTongHop", new { phienBaoVeId = int.Parse(pbvId) });
                    }
                }

                return Json(new { success = true, message = result.Message, redirectUrl });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThuKyDieuChinhDiem([FromBody] DieuChinhDiemRequest request)
        {
            var currentUserId = await GetCurrentUserId();
            if (currentUserId == 0)
                return Json(new { success = false, message = "Không xác định được người dùng." });

            try
            {
                var result = await _service.ThuKyDieuChinhDiem(request, currentUserId);
                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThuKyChinhSuaDiemThanhVien([FromBody] ThuKyChinhSuaDiemThanhVienRequest request)
        {
            var currentUserId = await GetCurrentUserId();
            if (currentUserId == 0)
                return Json(new { success = false, message = "Không xác định được người dùng." });

            try
            {
                var result = await _service.ThuKyChinhSuaDiemThanhVien(request, currentUserId);
                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChuTichXacNhan([FromBody] XacNhanDiemRequest request)
        {
            var currentUserId = await GetCurrentUserId();
            if (currentUserId == 0)
                return Json(new { success = false, message = "Không xác định được người dùng." });

            try
            {
                var result = await _service.ChuTichXacNhan(request, currentUserId);
                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        public async Task<IActionResult> BangDiemTongHop(int phienBaoVeId)
        {
            var currentUserId = await GetCurrentUserId();
            if (currentUserId == 0)
                return RedirectToAction("Login", "Account", new { area = "" });

            var viewModel = await _service.GetBangDiemTongHop(phienBaoVeId, currentUserId);
            if (viewModel == null)
            {
                TempData["ErrorMessage"] = _service.GetValidationError("") ?? "Không tìm thấy phiên bảo vệ.";
                return RedirectToAction(nameof(Index));
            }

            return View(viewModel);
        }
    }
}
