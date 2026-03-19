using DATN_TMS.Areas.BCNKhoa.Models;
using DATN_TMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using X.PagedList.Extensions;

namespace DATN_TMS.Areas.BCNKhoa.Controllers
{
    [Area("BCNKhoa")]
    public class QuanLyBaoCaoThongKeController : Controller
    {
        private readonly QuanLyDoAnTotNghiepContext _context;

        public QuanLyBaoCaoThongKeController(QuanLyDoAnTotNghiepContext context)
        {
            _context = context;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var sessionRole = HttpContext.Session.GetString("Role");
            var isBCNKhoa = User?.Identity?.IsAuthenticated == true &&
                            (User.IsInRole("BCN_KHOA") || User.IsInRole("ADMIN"));
            var isBCNKhoaBySession = sessionRole == "BCN_KHOA" || sessionRole == "ADMIN";

            if (!isBCNKhoa && !isBCNKhoaBySession)
            {
                context.Result = RedirectToAction("Login", "Account", new { area = "" });
                return;
            }
            base.OnActionExecuting(context);
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? dotId, int? page)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            // Lấy danh sách đợt đồ án
            var dots = await _context.DotDoAns
                .Include(d => d.IdHocKiNavigation)
                .OrderByDescending(d => d.Id)
                .ToListAsync();

            var dotOptions = dots.Select(d => new SelectListItem
            {
                Value = d.Id.ToString(),
                Text = d.TenDot ?? $"HK {d.IdHocKiNavigation?.NamBatDau}-{d.IdHocKiNavigation?.NamKetThuc}"
            }).ToList();

            // Chọn đợt: ưu tiên dotId → đợt đang hoạt động → đợt mới nhất
            var selectedDotId = dotId
                ?? dots.FirstOrDefault(d => d.TrangThai == true)?.Id
                ?? dots.FirstOrDefault()?.Id;

            // Thống kê tổng quan cho đợt được chọn
            int tongSinhVien = 0;
            int tongDeTai = 0;
            int totalTaskDone = 0;
            int totalTaskAll = 0;
            var summaryList = new List<DeTaiSummaryItem>();

            if (selectedDotId.HasValue)
            {
                // Đề tài đã duyệt trong đợt
                var deTais = await _context.DeTais
                    .Include(dt => dt.SinhVienDeTais)
                        .ThenInclude(svdt => svdt.IdSinhVienNavigation)
                            .ThenInclude(sv => sv != null ? sv.IdNguoiDungNavigation : null)
                    .Where(dt => dt.IdDot == selectedDotId && dt.TrangThai == "DA_DUYET")
                    .ToListAsync();

                tongDeTai = deTais.Count;

                // Tổng sinh viên (đã duyệt) trong đợt
                tongSinhVien = deTais
                    .SelectMany(dt => dt.SinhVienDeTais)
                    .Where(svdt => svdt.TrangThai == "DA_DUYET")
                    .Select(svdt => svdt.IdSinhVien)
                    .Distinct()
                    .Count();

                // Lấy tất cả sinh viên đã duyệt trong đợt
                var allSvIds = deTais
                    .SelectMany(dt => dt.SinhVienDeTais)
                    .Where(svdt => svdt.TrangThai == "DA_DUYET")
                    .Select(svdt => svdt.IdSinhVien)
                    .Distinct()
                    .ToList();

                // Lấy task cho tất cả sinh viên trong đợt
                var allTasks = await _context.KeHoachCongViecs
                    .Where(k => k.IdDot == selectedDotId && allSvIds.Contains(k.IdSinhVien))
                    .ToListAsync();

                // Tính tiến độ theo đề tài
                foreach (var dt in deTais)
                {
                    var svDaDuyet = dt.SinhVienDeTais
                        .Where(svdt => svdt.TrangThai == "DA_DUYET")
                        .ToList();

                    var svIds = svDaDuyet.Select(svdt => svdt.IdSinhVien).ToList();
                    var tasks = allTasks.Where(t => svIds.Contains(t.IdSinhVien)).ToList();
                    var done = tasks.Count(t => t.TrangThai == "Đã duyệt");

                    totalTaskAll += tasks.Count;
                    totalTaskDone += done;

                    var svNames = svDaDuyet
                        .Select(svdt => svdt.IdSinhVienNavigation?.IdNguoiDungNavigation?.HoTen ?? "N/A")
                        .ToList();

                    summaryList.Add(new DeTaiSummaryItem
                    {
                        MaDeTai = dt.MaDeTai ?? "",
                        TenDeTai = dt.TenDeTai ?? "",
                        SinhVien = string.Join(", ", svNames),
                        TrangThai = dt.TrangThai ?? "",
                        TaskDone = done,
                        TaskTotal = tasks.Count
                    });
                }
            }

            double tienDo = totalTaskAll == 0 ? 0 : (double)totalTaskDone / totalTaskAll * 100;

            var model = new BaoCaoThongKeViewModel
            {
                SelectedDotId = selectedDotId?.ToString(),
                DotOptions = dotOptions,
                TongSinhVien = tongSinhVien,
                TongDeTai = tongDeTai,
                TienDoPhanTram = tienDo,
                DeTaiSummaries = summaryList.ToPagedList(pageNumber, pageSize)
            };

            ViewBag.CurrentDotId = selectedDotId;
            ViewBag.CurrentPage = pageNumber;

            return View(model);
        }
    }
}
