using DATN_TMS.Areas.SinhVien.Models;
using DATN_TMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace DATN_TMS.Areas.SinhVien.Controllers
{
    [Area("SinhVien")]
    public class NopBaoCaoController : Controller
    {
        private readonly QuanLyDoAnTotNghiepContext _context;
        private readonly IWebHostEnvironment _env;

        public NopBaoCaoController(QuanLyDoAnTotNghiepContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
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

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var mssv = HttpContext.Session.GetString("UserCode");
            var sinhVien = await _context.SinhViens
                .FirstOrDefaultAsync(sv => sv.Mssv == mssv);

            if (sinhVien == null)
                return View(new NopBaoCaoIndexViewModel());

            var dot = await _context.DotDoAns
                .Include(d => d.IdHocKiNavigation)
                .FirstOrDefaultAsync(d => d.TrangThai == true);

            if (dot == null)
                return View(new NopBaoCaoIndexViewModel());

            var svDeTai = await _context.SinhVienDeTais
                .Include(svdt => svdt.IdDeTaiNavigation)
                .FirstOrDefaultAsync(svdt =>
                    svdt.IdSinhVien == sinhVien.IdNguoiDung &&
                    svdt.IdDeTaiNavigation != null &&
                    svdt.IdDeTaiNavigation.IdDot == dot.Id &&
                    svdt.TrangThai == "DA_DUYET");

            var baoCaos = await _context.BaoCaoNops
                .Where(b => b.IdSinhVien == sinhVien.IdNguoiDung &&
                            b.IdDot == dot.Id &&
                            b.LoaiBaoCao != null)
                .ToListAsync();

            var today = DateOnly.FromDateTime(DateTime.Now);
            var tenDot = dot.TenDot ?? "---";

            var vm = new NopBaoCaoIndexViewModel
            {
                TenDot = tenDot,
                HocKi = dot.IdHocKiNavigation?.MaHocKi,
                DeCuong = BuildBoxItem(baoCaos, "DE_CUONG", $"Nộp đề cương", dot.NgayBatDauNopDeCuong, dot.NgayKetThucNopDeCuong, today),
                GiuaKy = BuildBoxItem(baoCaos, "GIUA_KY", $"Nộp báo cáo giữa kỳ", dot.NgayBatDauBaoCaoGiuaKi, dot.NgayBatDauBaoCaoGiuaKi?.AddDays(2), today),
                CuoiKy = BuildBoxItem(baoCaos, "CUOI_KY", $"Nộp báo cáo cuối kỳ", dot.NgayBatDauBaoCaoCuoiKi, dot.NgayBatDauBaoCaoCuoiKi?.AddDays(2), today)
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Detail(string loai)
        {
            if (string.IsNullOrEmpty(loai) || !new[] { "DE_CUONG", "GIUA_KY", "CUOI_KY" }.Contains(loai))
                return RedirectToAction("Index");

            var mssv = HttpContext.Session.GetString("UserCode");
            var sinhVien = await _context.SinhViens
                .FirstOrDefaultAsync(sv => sv.Mssv == mssv);

            if (sinhVien == null) return RedirectToAction("Index");

            var dot = await _context.DotDoAns
                .FirstOrDefaultAsync(d => d.TrangThai == true);

            if (dot == null) return RedirectToAction("Index");

            var svDeTai = await _context.SinhVienDeTais
                .Include(svdt => svdt.IdDeTaiNavigation)
                .FirstOrDefaultAsync(svdt =>
                    svdt.IdSinhVien == sinhVien.IdNguoiDung &&
                    svdt.IdDeTaiNavigation != null &&
                    svdt.IdDeTaiNavigation.IdDot == dot.Id &&
                    svdt.TrangThai == "DA_DUYET");

            var baoCao = await _context.BaoCaoNops
                .FirstOrDefaultAsync(b => b.IdSinhVien == sinhVien.IdNguoiDung &&
                                          b.IdDot == dot.Id &&
                                          b.LoaiBaoCao == loai);

            var today = DateOnly.FromDateTime(DateTime.Now);
            DateOnly? batDau = null, ketThuc = null;
            string tieuDe = "";

            switch (loai)
            {
                case "DE_CUONG":
                    tieuDe = "Nộp đề cương";
                    batDau = dot.NgayBatDauNopDeCuong;
                    ketThuc = dot.NgayKetThucNopDeCuong;
                    break;
                case "GIUA_KY":
                    tieuDe = "Nộp báo cáo giữa kỳ";
                    batDau = dot.NgayBatDauBaoCaoGiuaKi;
                    ketThuc = dot.NgayBatDauBaoCaoGiuaKi?.AddDays(2);
                    break;
                case "CUOI_KY":
                    tieuDe = "Nộp báo cáo cuối kỳ";
                    batDau = dot.NgayBatDauBaoCaoCuoiKi;
                    ketThuc = dot.NgayBatDauBaoCaoCuoiKi?.AddDays(2);
                    break;
            }

            var dangMo = batDau.HasValue && ketThuc.HasValue && today >= batDau && today <= ketThuc;
            var trangThai = baoCao?.TrangThai ?? "CHUA_NOP";

            var vm = new NopBaoCaoDetailViewModel
            {
                IdBaoCaoNop = baoCao?.Id,
                LoaiBaoCao = loai,
                TieuDe = tieuDe,
                TenDot = dot.TenDot,
                TrangThaiGui = baoCao?.FileBaocao != null ? "Đã nộp" : "Chưa nộp",
                TrangThai = trangThai,
                TrangThaiText = GetTrangThaiText(trangThai),
                TrangThaiCss = GetTrangThaiCss(trangThai),
                TenFile = GetFileName(baoCao?.FileBaocao),
                FilePath = baoCao?.FileBaocao,
                NgaySuaDoiCuoi = baoCao?.NgaySuaDoiCuoi?.ToString("dd/MM/yyyy HH:mm"),
                GhiChuGui = baoCao?.GhiChuGui,
                NhanXetGVHD = baoCao?.NhanXet,
                ThoiGianBatDau = batDau?.ToString("dd/MM/yyyy"),
                ThoiGianKetThuc = ketThuc?.ToString("dd/MM/yyyy"),
                DangMo = dangMo,
                IdDot = dot.Id,
                IdDeTai = svDeTai?.IdDeTai,
                IdSinhVien = sinhVien.IdNguoiDung
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NopFile(string loai, IFormFile? file, string? ghiChu)
        {
            if (string.IsNullOrEmpty(loai))
                return RedirectToAction("Index");

            var mssv = HttpContext.Session.GetString("UserCode");
            var sinhVien = await _context.SinhViens
                .FirstOrDefaultAsync(sv => sv.Mssv == mssv);
            if (sinhVien == null) return RedirectToAction("Index");

            var dot = await _context.DotDoAns
                .FirstOrDefaultAsync(d => d.TrangThai == true);
            if (dot == null) return RedirectToAction("Index");

            var svDeTai = await _context.SinhVienDeTais
                .Include(svdt => svdt.IdDeTaiNavigation)
                .FirstOrDefaultAsync(svdt =>
                    svdt.IdSinhVien == sinhVien.IdNguoiDung &&
                    svdt.IdDeTaiNavigation != null &&
                    svdt.IdDeTaiNavigation.IdDot == dot.Id &&
                    svdt.TrangThai == "DA_DUYET");

            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Vui lòng chọn file để nộp.";
                return RedirectToAction("Detail", new { loai });
            }

            var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "baocao");
            if (!Directory.Exists(uploadsDir))
                Directory.CreateDirectory(uploadsDir);

            var uniqueName = $"{mssv}_{loai}_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsDir, uniqueName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativePath = $"/uploads/baocao/{uniqueName}";
            var now = DateTime.Now;

            var baoCao = await _context.BaoCaoNops
                .FirstOrDefaultAsync(b => b.IdSinhVien == sinhVien.IdNguoiDung &&
                                          b.IdDot == dot.Id &&
                                          b.LoaiBaoCao == loai);

            if (baoCao == null)
            {
                baoCao = new BaoCaoNop
                {
                    IdDot = dot.Id,
                    IdDeTai = svDeTai?.IdDeTai,
                    IdSinhVien = sinhVien.IdNguoiDung,
                    Stt = loai == "DE_CUONG" ? 1 : loai == "GIUA_KY" ? 2 : 3,
                    TenBaoCao = loai switch
                    {
                        "DE_CUONG" => "Đề cương đồ án",
                        "GIUA_KY" => "Báo cáo giữa kỳ",
                        "CUOI_KY" => "Báo cáo cuối kỳ",
                        _ => "Báo cáo"
                    },
                    FileBaocao = relativePath,
                    NgayNop = now,
                    TrangThai = "CHO_DUYET",
                    LoaiBaoCao = loai,
                    GhiChuGui = ghiChu,
                    NgaySuaDoiCuoi = now
                };
                _context.BaoCaoNops.Add(baoCao);
            }
            else
            {
                baoCao.FileBaocao = relativePath;
                baoCao.NgayNop = now;
                baoCao.TrangThai = "CHO_DUYET";
                baoCao.GhiChuGui = ghiChu;
                baoCao.NgaySuaDoiCuoi = now;
                baoCao.NhanXet = null;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Nộp file thành công!";
            return RedirectToAction("Detail", new { loai });
        }

        private NopBaoCaoBoxItem BuildBoxItem(List<BaoCaoNop> baoCaos, string loai, string tieuDe,
            DateOnly? batDau, DateOnly? ketThuc, DateOnly today)
        {
            var bc = baoCaos.FirstOrDefault(b => b.LoaiBaoCao == loai);
            var trangThai = bc?.TrangThai ?? "CHUA_NOP";
            var dangMo = batDau.HasValue && ketThuc.HasValue && today >= batDau && today <= ketThuc;

            return new NopBaoCaoBoxItem
            {
                IdBaoCaoNop = bc?.Id,
                LoaiBaoCao = loai,
                TieuDe = tieuDe,
                ThoiGianBatDau = batDau?.ToString("dd/MM/yyyy"),
                ThoiGianKetThuc = ketThuc?.ToString("dd/MM/yyyy"),
                TrangThai = trangThai,
                TrangThaiText = GetTrangThaiText(trangThai),
                TrangThaiCss = GetTrangThaiCss(trangThai),
                DangMo = dangMo
            };
        }

        private static string GetTrangThaiText(string? trangThai) => trangThai switch
        {
            "DA_DUYET" => "Đã duyệt",
            "CHO_DUYET" => "Chờ duyệt",
            "TU_CHOI" => "Từ chối",
            "CHUA_NOP" => "Chưa nộp",
            _ => "Chưa nộp"
        };

        private static string GetTrangThaiCss(string? trangThai) => trangThai switch
        {
            "DA_DUYET" => "status-approved",
            "CHO_DUYET" => "status-pending",
            "TU_CHOI" => "status-rejected",
            _ => "status-default"
        };

        private static string? GetFileName(string? filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return null;
            return filePath.Contains('/') ? filePath[(filePath.LastIndexOf('/') + 1)..] : filePath;
        }
    }
}
