using DATN_TMS.Areas.GiangVien.Models;
using DATN_TMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace DATN_TMS.Areas.GiangVien.Controllers
{
    [Area("GiangVien")]
    public class QuanLyNopBaoCaoController : Controller
    {
        private readonly QuanLyDoAnTotNghiepContext _context;

        public QuanLyNopBaoCaoController(QuanLyDoAnTotNghiepContext context)
        {
            _context = context;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var sessionRole = HttpContext.Session.GetString("Role");
            var isLecturerByClaim = User?.Identity?.IsAuthenticated == true && (User.IsInRole("GIANG_VIEN") || User.IsInRole("GV"));
            var isLecturerBySession = sessionRole == "GIANG_VIEN" || sessionRole == "GV";

            if (!isLecturerByClaim && !isLecturerBySession)
            {
                context.Result = RedirectToAction("Login", "Account", new { area = "" });
                return;
            }
            base.OnActionExecuting(context);
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var maGV = HttpContext.Session.GetString("UserCode");
            var giangVien = await _context.GiangViens.FirstOrDefaultAsync(gv => gv.MaGv == maGV);

            if (giangVien == null)
                return View(new List<NopBaoCaoGVItem>());

            var idDeTais = await _context.DeTais
                .Where(dt => dt.IdGvhd == giangVien.IdNguoiDung)
                .Select(dt => dt.Id)
                .ToListAsync();

            var data = await _context.BaoCaoNops
                .Include(bc => bc.IdSinhVienNavigation)
                    .ThenInclude(sv => sv!.IdNguoiDungNavigation)
                .Include(bc => bc.IdDeTaiNavigation)
                .Where(bc => bc.IdDeTai.HasValue && idDeTais.Contains(bc.IdDeTai.Value) && bc.LoaiBaoCao != "MINH_CHUNG")
                .OrderByDescending(bc => bc.NgayNop)
                .Select(bc => new NopBaoCaoGVItem
                {
                    Id = bc.Id,
                    Stt = bc.Stt,
                    TenBaoCao = bc.TenBaoCao,
                    TenSinhVien = bc.IdSinhVienNavigation != null && bc.IdSinhVienNavigation.IdNguoiDungNavigation != null
                        ? bc.IdSinhVienNavigation.IdNguoiDungNavigation.HoTen : "",
                    Mssv = bc.IdSinhVienNavigation != null ? bc.IdSinhVienNavigation.Mssv : "",
                    TenDeTai = bc.IdDeTaiNavigation != null ? bc.IdDeTaiNavigation.TenDeTai : "",
                    NgayNop = bc.NgayNop.HasValue ? bc.NgayNop.Value.ToString("dd/MM/yyyy HH:mm") : "",
                    TrangThai = bc.TrangThai,
                    LoaiBaoCao = bc.LoaiBaoCao,
                    FilePath = bc.FileBaocao,
                    GhiChuGui = bc.GhiChuGui
                })
                .ToListAsync();

            return View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DuyetBaoCao([FromBody] DuyetBaoCaoRequest request)
        {
            var baoCao = await _context.BaoCaoNops.FindAsync(request.Id);
            if (baoCao == null)
                return Json(new { success = false, message = "Không tìm thấy báo cáo." });

            var maGV = HttpContext.Session.GetString("UserCode");
            var giangVien = await _context.GiangViens.FirstOrDefaultAsync(gv => gv.MaGv == maGV);
            if (giangVien == null)
                return Json(new { success = false, message = "Không tìm thấy thông tin giảng viên." });

            var deTai = await _context.DeTais.FindAsync(baoCao.IdDeTai);
            if (deTai == null || deTai.IdGvhd != giangVien.IdNguoiDung)
                return Json(new { success = false, message = "Bạn không có quyền duyệt báo cáo này." });

            if (request.Action == "approve")
            {
                baoCao.TrangThai = "DA_DUYET";
                baoCao.NhanXet = request.NhanXet;
            }
            else if (request.Action == "reject")
            {
                baoCao.TrangThai = "TU_CHOI";
                baoCao.NhanXet = request.NhanXet;
            }
            else
            {
                return Json(new { success = false, message = "Hành động không hợp lệ." });
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = request.Action == "approve" ? "Duyệt thành công!" : "Đã từ chối báo cáo." });
        }
    }

    public class DuyetBaoCaoRequest
    {
        public int Id { get; set; }
        public string Action { get; set; } = "";
        public string? NhanXet { get; set; }
    }
}
