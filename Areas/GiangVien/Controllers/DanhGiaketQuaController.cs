using DATN_TMS.Areas.GiangVien.Models;
using DATN_TMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace DATN_TMS.Areas.GiangVien.Controllers
{
    [Area("GiangVien")]
    public class DanhGiaketQuaController : Controller
    {
        private readonly QuanLyDoAnTotNghiepContext _context;

        public DanhGiaketQuaController(QuanLyDoAnTotNghiepContext context)
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
                return View(new DanhGiaKetQuaIndexViewModel { CoDot = false });
            }

            // Lấy đợt đồ án hiện tại
            var dot = await _context.DotDoAns
                .Where(d => d.TrangThai == true)
                .OrderByDescending(d => d.Id)
                .FirstOrDefaultAsync();

            if (dot == null)
            {
                return View(new DanhGiaKetQuaIndexViewModel
                {
                    CoDot = false,
                    TenDot = "Chưa có đợt đồ án"
                });
            }

            // Lấy danh sách đề tài mà GV này hướng dẫn trong đợt hiện tại (đã được hội đồng duyệt)
            var deTais = await _context.DeTais
                .Include(dt => dt.IdChuyenNganhNavigation)
                .Include(dt => dt.SinhVienDeTais)
                    .ThenInclude(svdt => svdt.IdSinhVienNavigation)
                        .ThenInclude(sv => sv!.IdNguoiDungNavigation)
                .Include(dt => dt.SinhVienDeTais)
                    .ThenInclude(svdt => svdt.IdSinhVienNavigation)
                        .ThenInclude(sv => sv!.IdKhoaHocNavigation)
                .Where(dt => dt.IdGvhd == giangVien.IdNguoiDung && dt.IdDot == dot.Id && dt.TrangThai == "DA_DUYET")
                .ToListAsync();

            var result = new DanhGiaKetQuaIndexViewModel
            {
                CoDot = true,
                TenDot = dot.TenDot,
                DanhSachDeTai = new List<DanhGiaKetQuaGVItem>()
            };

            foreach (var dt in deTais)
            {
                // Lấy danh sách sinh viên đã được duyệt trong đề tài
                var sinhViensDuyet = dt.SinhVienDeTais
                    .Where(svdt => svdt.TrangThai == "DA_DUYET")
                    .ToList();

                if (!sinhViensDuyet.Any()) continue;

                var idSinhViens = sinhViensDuyet
                    .Where(svdt => svdt.IdSinhVien.HasValue)
                    .Select(svdt => svdt.IdSinhVien!.Value)
                    .ToList();

                // Kiểm tra báo cáo cuối kỳ đã được duyệt
                var baoCaoCuoiKy = await _context.BaoCaoNops
                    .Where(bc => bc.IdDeTai == dt.Id &&
                                 bc.LoaiBaoCao == "CUOI_KY" &&
                                 bc.TrangThai == "DA_DUYET" &&
                                 bc.IdSinhVien.HasValue &&
                                 idSinhViens.Contains(bc.IdSinhVien.Value))
                    .OrderByDescending(bc => bc.NgayNop)
                    .FirstOrDefaultAsync();

                // Lấy điểm GVHD đã chấm (nếu có) - giả sử điểm lưu trong SinhVienDeTai
                var danhSachSV = new List<SinhVienDGItem>();
                bool daChamDiem = false;

                foreach (var svdt in sinhViensDuyet)
                {
                    var sv = svdt.IdSinhVienNavigation;
                    if (sv == null) continue;

                    var svItem = new SinhVienDGItem
                    {
                        IdSinhVien = sv.IdNguoiDung,
                        Mssv = sv.Mssv,
                        HoTen = sv.IdNguoiDungNavigation?.HoTen,
                        KhoaHoc = sv.IdKhoaHocNavigation?.TenKhoa,
                        DiemGVHD = svdt.DiemGvhd, // Giả sử có trường này
                        DaChamDiem = svdt.DiemGvhd.HasValue
                    };

                    if (svdt.DiemGvhd.HasValue)
                    {
                        daChamDiem = true;
                    }

                    danhSachSV.Add(svItem);
                }

                result.DanhSachDeTai.Add(new DanhGiaKetQuaGVItem
                {
                    IdDeTai = dt.Id,
                    MaDeTai = dt.MaDeTai,
                    TenDeTai = dt.TenDeTai,
                    ChuyenNganh = dt.IdChuyenNganhNavigation?.TenChuyenNganh,
                    SoLuongSV = danhSachSV.Count,
                    DaChamDiem = daChamDiem,
                    TrangThaiText = daChamDiem ? "Đã nhập điểm" : "Chưa nhập điểm",
                    TrangThaiCss = daChamDiem ? "status-approved" : "status-pending",
                    CoBaoCaoCuoiKyDuyet = baoCaoCuoiKy != null,
                    FileBaoCaoCuoiKy = baoCaoCuoiKy?.FileBaocao,
                    TenFileBaoCao = baoCaoCuoiKy != null && !string.IsNullOrEmpty(baoCaoCuoiKy.FileBaocao)
                        ? Path.GetFileName(baoCaoCuoiKy.FileBaocao)
                        : null,
                    DanhSachSV = danhSachSV
                });
            }

            return View(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChamDiem([FromBody] ChamDiemRequest request)
        {
            var maGV = HttpContext.Session.GetString("UserCode");
            var giangVien = await _context.GiangViens.FirstOrDefaultAsync(gv => gv.MaGv == maGV);

            if (giangVien == null)
                return Json(new { success = false, message = "Không tìm thấy thông tin giảng viên." });

            // Kiểm tra đề tài
            var deTai = await _context.DeTais.FindAsync(request.IdDeTai);
            if (deTai == null || deTai.IdGvhd != giangVien.IdNguoiDung)
                return Json(new { success = false, message = "Bạn không có quyền chấm điểm đề tài này." });

            // Lấy sinh viên của đề tài
            var svDeTais = await _context.SinhVienDeTais
                .Where(svdt => svdt.IdDeTai == request.IdDeTai && svdt.TrangThai == "DA_DUYET")
                .ToListAsync();

            if (!svDeTais.Any())
                return Json(new { success = false, message = "Không tìm thấy sinh viên trong đề tài." });

            // Cập nhật điểm cho từng sinh viên
            foreach (var diemSV in request.DanhSachDiem)
            {
                var svDeTai = svDeTais.FirstOrDefault(sv => sv.IdSinhVien == diemSV.IdSinhVien);
                if (svDeTai != null)
                {
                    svDeTai.DiemGvhd = diemSV.Diem;
                    svDeTai.NhanXetGvhd = request.NhanXet;
                }
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Chấm điểm thành công!" });
        }

        [HttpGet]
        public async Task<IActionResult> GetDeTaiDetail(int id)
        {
            var maGV = HttpContext.Session.GetString("UserCode");
            var giangVien = await _context.GiangViens.FirstOrDefaultAsync(gv => gv.MaGv == maGV);

            if (giangVien == null)
                return Json(new { success = false, message = "Không tìm thấy thông tin giảng viên." });

            var deTai = await _context.DeTais
                .Include(dt => dt.SinhVienDeTais)
                    .ThenInclude(svdt => svdt.IdSinhVienNavigation)
                        .ThenInclude(sv => sv!.IdNguoiDungNavigation)
                .FirstOrDefaultAsync(dt => dt.Id == id && dt.IdGvhd == giangVien.IdNguoiDung);

            if (deTai == null)
                return Json(new { success = false, message = "Không tìm thấy đề tài." });

            var sinhViens = deTai.SinhVienDeTais
                .Where(svdt => svdt.TrangThai == "DA_DUYET")
                .Select(svdt => new
                {
                    idSinhVien = svdt.IdSinhVien,
                    mssv = svdt.IdSinhVienNavigation?.Mssv,
                    hoTen = svdt.IdSinhVienNavigation?.IdNguoiDungNavigation?.HoTen,
                    diem = svdt.DiemGvhd,
                    nhanXet = svdt.NhanXetGvhd
                })
                .ToList();

            // Lấy nhận xét chung (từ SV đầu tiên nếu có)
            var nhanXetChung = sinhViens.FirstOrDefault()?.nhanXet;

            return Json(new
            {
                success = true,
                data = new
                {
                    idDeTai = deTai.Id,
                    maDeTai = deTai.MaDeTai,
                    tenDeTai = deTai.TenDeTai,
                    sinhViens = sinhViens,
                    nhanXet = nhanXetChung
                }
            });
        }
    }

    public class ChamDiemRequest
    {
        public int IdDeTai { get; set; }
        public string? NhanXet { get; set; }
        public List<DiemSinhVien> DanhSachDiem { get; set; } = new();
    }

    public class DiemSinhVien
    {
        public int IdSinhVien { get; set; }
        public double? Diem { get; set; }
    }
}
