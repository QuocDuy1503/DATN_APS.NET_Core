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
            var giangVien = await _context.GiangViens
                .Include(gv => gv.IdNguoiDungNavigation)
                .FirstOrDefaultAsync(gv => gv.MaGv == maGV);

            if (giangVien == null)
            {
                return View(new QuanLyNopBaoCaoIndexViewModel { CoDot = false });
            }

            // Lấy đợt đồ án hiện tại
            var dot = await _context.DotDoAns
                .Include(d => d.IdHocKiNavigation)
                .Where(d => d.TrangThai == true)
                .OrderByDescending(d => d.Id)
                .FirstOrDefaultAsync();

            if (dot == null)
            {
                return View(new QuanLyNopBaoCaoIndexViewModel
                {
                    CoDot = false,
                    TenDot = "Chưa có đợt đồ án"
                });
            }

            // Lấy danh sách đề tài mà GV này hướng dẫn trong đợt hiện tại
            var deTais = await _context.DeTais
                .Include(dt => dt.IdChuyenNganhNavigation)
                .Where(dt => dt.IdGvhd == giangVien.IdNguoiDung && dt.IdDot == dot.Id && dt.TrangThai == "DA_DUYET")
                .ToListAsync();

            var result = new QuanLyNopBaoCaoIndexViewModel
            {
                CoDot = true,
                TenDot = dot.TenDot,
                HocKi = dot.IdHocKiNavigation != null ? dot.IdHocKiNavigation.MaHocKi : "",
                DanhSachDeTai = new List<DeTaiBaoCaoItem>()
            };

            foreach (var dt in deTais)
            {
                // Lấy danh sách sinh viên đã được duyệt trong đề tài
                var sinhViens = await _context.SinhVienDeTais
                    .Include(svdt => svdt.IdSinhVienNavigation)
                        .ThenInclude(sv => sv!.IdNguoiDungNavigation)
                    .Where(svdt => svdt.IdDeTai == dt.Id && svdt.TrangThai == "DA_DUYET")
                    .Select(svdt => new
                    {
                        IdSinhVien = svdt.IdSinhVien,
                        HoTen = svdt.IdSinhVienNavigation != null && svdt.IdSinhVienNavigation.IdNguoiDungNavigation != null
                            ? svdt.IdSinhVienNavigation.IdNguoiDungNavigation.HoTen : ""
                    })
                    .ToListAsync();

                var idSinhViens = sinhViens.Select(sv => sv.IdSinhVien).ToList();

                // Đếm báo cáo theo loại
                var baoCaos = await _context.BaoCaoNops
                    .Where(bc => bc.IdDeTai == dt.Id && bc.IdSinhVien.HasValue && idSinhViens.Contains(bc.IdSinhVien.Value))
                    .ToListAsync();

                var soDeCuong = baoCaos.Count(bc => bc.LoaiBaoCao == "DE_CUONG");
                var soGiuaKy = baoCaos.Count(bc => bc.LoaiBaoCao == "GIUA_KY");
                var soCuoiKy = baoCaos.Count(bc => bc.LoaiBaoCao == "CUOI_KY");
                var soChoDuyet = baoCaos.Count(bc => bc.TrangThai == "CHO_DUYET");

                string trangThaiTongQuat;
                string trangThaiCss;

                if (soChoDuyet > 0)
                {
                    trangThaiTongQuat = $"Có {soChoDuyet} báo cáo chờ duyệt";
                    trangThaiCss = "status-pending";
                }
                else if (baoCaos.Count == 0)
                {
                    trangThaiTongQuat = "Chưa có báo cáo";
                    trangThaiCss = "status-default";
                }
                else
                {
                    trangThaiTongQuat = "Đã xử lý";
                    trangThaiCss = "status-approved";
                }

                result.DanhSachDeTai.Add(new DeTaiBaoCaoItem
                {
                    IdDeTai = dt.Id,
                    MaDeTai = dt.MaDeTai,
                    TenDeTai = dt.TenDeTai,
                    TenChuyenNganh = dt.IdChuyenNganhNavigation?.TenChuyenNganh,
                    SoSinhVien = sinhViens.Count,
                    DanhSachSinhVien = string.Join(", ", sinhViens.Select(sv => sv.HoTen)),
                    SoBaoCaoDeCuong = soDeCuong,
                    SoBaoCaoGiuaKy = soGiuaKy,
                    SoBaoCaoCuoiKy = soCuoiKy,
                    TongBaoCaoChoDuyet = soChoDuyet,
                    TrangThaiTongQuat = trangThaiTongQuat,
                    TrangThaiCss = trangThaiCss
                });
            }

            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var maGV = HttpContext.Session.GetString("UserCode");
            var giangVien = await _context.GiangViens.FirstOrDefaultAsync(gv => gv.MaGv == maGV);

            if (giangVien == null)
                return RedirectToAction("Index");

            var deTai = await _context.DeTais
                .Include(dt => dt.IdChuyenNganhNavigation)
                .Include(dt => dt.IdDotNavigation)
                .FirstOrDefaultAsync(dt => dt.Id == id && dt.IdGvhd == giangVien.IdNguoiDung);

            if (deTai == null)
                return RedirectToAction("Index");

            var dot = deTai.IdDotNavigation;

            // Lấy danh sách sinh viên trong đề tài
            var sinhViens = await _context.SinhVienDeTais
                .Include(svdt => svdt.IdSinhVienNavigation)
                    .ThenInclude(sv => sv!.IdNguoiDungNavigation)
                .Where(svdt => svdt.IdDeTai == id && svdt.TrangThai == "DA_DUYET")
                .Select(svdt => new SinhVienBaoCaoItem
                {
                    IdSinhVien = svdt.IdSinhVien ?? 0,
                    Mssv = svdt.IdSinhVienNavigation != null ? svdt.IdSinhVienNavigation.Mssv : "",
                    HoTen = svdt.IdSinhVienNavigation != null && svdt.IdSinhVienNavigation.IdNguoiDungNavigation != null
                        ? svdt.IdSinhVienNavigation.IdNguoiDungNavigation.HoTen : "",
                    Email = svdt.IdSinhVienNavigation != null && svdt.IdSinhVienNavigation.IdNguoiDungNavigation != null
                        ? svdt.IdSinhVienNavigation.IdNguoiDungNavigation.Email : ""
                })
                .ToListAsync();

            var idSinhViens = sinhViens.Select(sv => sv.IdSinhVien).ToList();

            // Lấy tất cả báo cáo của đề tài này
            var allBaoCaos = await _context.BaoCaoNops
                .Include(bc => bc.IdSinhVienNavigation)
                    .ThenInclude(sv => sv!.IdNguoiDungNavigation)
                .Where(bc => bc.IdDeTai == id && bc.IdSinhVien.HasValue && idSinhViens.Contains(bc.IdSinhVien.Value))
                .OrderByDescending(bc => bc.NgayNop)
                .ToListAsync();

            // Phân loại báo cáo
            var baoCaoDeCuong = MapBaoCaoToDetail(allBaoCaos.Where(bc => bc.LoaiBaoCao == "DE_CUONG").ToList());
            var baoCaoGiuaKy = MapBaoCaoToDetail(allBaoCaos.Where(bc => bc.LoaiBaoCao == "GIUA_KY").ToList());
            var baoCaoCuoiKy = MapBaoCaoToDetail(allBaoCaos.Where(bc => bc.LoaiBaoCao == "CUOI_KY").ToList());

            var model = new QuanLyNopBaoCaoDetailViewModel
            {
                IdDeTai = deTai.Id,
                MaDeTai = deTai.MaDeTai,
                TenDeTai = deTai.TenDeTai,
                TenChuyenNganh = deTai.IdChuyenNganhNavigation?.TenChuyenNganh,
                TenDot = dot?.TenDot,
                DanhSachSinhVien = sinhViens,
                BaoCaoDeCuong = baoCaoDeCuong,
                BaoCaoGiuaKy = baoCaoGiuaKy,
                BaoCaoCuoiKy = baoCaoCuoiKy,
                DeadlineDeCuong = dot?.NgayKetThucNopDeCuong?.ToString("dd/MM/yyyy"),
                DeadlineGiuaKy = dot?.NgayKetThucBaoCaoGiuaKi?.ToString("dd/MM/yyyy"),
                DeadlineCuoiKy = dot?.NgayKetThucBaoCaoCuoiKi?.ToString("dd/MM/yyyy")
            };

            return View(model);
        }

        private List<BaoCaoNopGVDetailItem> MapBaoCaoToDetail(List<BaoCaoNop> baoCaos)
        {
            var result = new List<BaoCaoNopGVDetailItem>();

            // Group by sinh viên để xác định version
            var grouped = baoCaos.GroupBy(bc => bc.IdSinhVien);

            foreach (var group in grouped)
            {
                var orderedList = group.OrderByDescending(bc => bc.NgayNop).ToList();
                for (int i = 0; i < orderedList.Count; i++)
                {
                    var bc = orderedList[i];
                    var trangThaiText = bc.TrangThai switch
                    {
                        "DA_DUYET" => "Đã duyệt",
                        "TU_CHOI" => "Từ chối",
                        "CHO_DUYET" => "Chờ duyệt",
                        _ => "Chưa xử lý"
                    };

                    var trangThaiCss = bc.TrangThai switch
                    {
                        "DA_DUYET" => "status-approved",
                        "TU_CHOI" => "status-rejected",
                        "CHO_DUYET" => "status-pending",
                        _ => "status-default"
                    };

                    result.Add(new BaoCaoNopGVDetailItem
                    {
                        IdBaoCao = bc.Id,
                        IdSinhVien = bc.IdSinhVien,
                        TenSinhVien = bc.IdSinhVienNavigation?.IdNguoiDungNavigation?.HoTen,
                        Mssv = bc.IdSinhVienNavigation?.Mssv,
                        TenBaoCao = bc.TenBaoCao,
                        FilePath = bc.FileBaocao,
                        TenFile = !string.IsNullOrEmpty(bc.FileBaocao) ? Path.GetFileName(bc.FileBaocao) : null,
                        NgayNop = bc.NgayNop?.ToString("dd/MM/yyyy HH:mm"),
                        GhiChuGui = bc.GhiChuGui,
                        NhanXet = bc.NhanXet,
                        TrangThai = bc.TrangThai,
                        TrangThaiText = trangThaiText,
                        TrangThaiCss = trangThaiCss,
                        PhienBan = orderedList.Count - i,
                        LaBanMoiNhat = i == 0
                    });
                }
            }

            return result.OrderByDescending(r => r.NgayNop).ToList();
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
            return Json(new { success = true, message = request.Action == "approve" ? "Duyệt báo cáo thành công!" : "Đã từ chối báo cáo." });
        }

        [HttpGet]
        public async Task<IActionResult> GetBaoCaoDetail(int id)
        {
            var baoCao = await _context.BaoCaoNops
                .Include(bc => bc.IdSinhVienNavigation)
                    .ThenInclude(sv => sv!.IdNguoiDungNavigation)
                .FirstOrDefaultAsync(bc => bc.Id == id);

            if (baoCao == null)
                return Json(new { success = false, message = "Không tìm thấy báo cáo." });

            var trangThaiText = baoCao.TrangThai switch
            {
                "DA_DUYET" => "Đã duyệt",
                "TU_CHOI" => "Từ chối",
                "CHO_DUYET" => "Chờ duyệt",
                _ => "Chưa xử lý"
            };

            return Json(new
            {
                success = true,
                data = new
                {
                    id = baoCao.Id,
                    tenBaoCao = baoCao.TenBaoCao,
                    loaiBaoCao = baoCao.LoaiBaoCao,
                    tenSinhVien = baoCao.IdSinhVienNavigation?.IdNguoiDungNavigation?.HoTen,
                    mssv = baoCao.IdSinhVienNavigation?.Mssv,
                    ngayNop = baoCao.NgayNop?.ToString("dd/MM/yyyy HH:mm"),
                    filePath = baoCao.FileBaocao,
                    tenFile = !string.IsNullOrEmpty(baoCao.FileBaocao) ? Path.GetFileName(baoCao.FileBaocao) : null,
                    ghiChuGui = baoCao.GhiChuGui,
                    nhanXet = baoCao.NhanXet,
                    trangThai = baoCao.TrangThai,
                    trangThaiText = trangThaiText
                }
            });
        }
    }

    public class DuyetBaoCaoRequest
    {
        public int Id { get; set; }
        public string Action { get; set; } = "";
        public string? NhanXet { get; set; }
    }
}
