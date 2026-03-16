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
                return View(new DanhGiaKetQuaIndexViewModel { CoDot = false });

            var dot = await _context.DotDoAns
                .Where(d => d.TrangThai == true)
                .OrderByDescending(d => d.Id)
                .FirstOrDefaultAsync();

            if (dot == null)
                return View(new DanhGiaKetQuaIndexViewModel { CoDot = false, TenDot = "Chưa có đợt đồ án" });

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
                var sinhViensDuyet = dt.SinhVienDeTais
                    .Where(svdt => svdt.TrangThai == "DA_DUYET")
                    .ToList();

                if (!sinhViensDuyet.Any()) continue;

                var danhSachSV = new List<SinhVienDGItem>();
                bool daChamDiem = false;

                // Báo cáo cuối kỳ được nộp đại diện cho cả đề tài (1 người nộp = cả nhóm)
                var baoCaoCuoiKy = await _context.BaoCaoNops
                    .Where(bc => bc.IdDeTai == dt.Id &&
                                 bc.LoaiBaoCao == "CUOI_KY" &&
                                 bc.TrangThai == "DA_DUYET")
                    .OrderByDescending(bc => bc.NgayNop)
                    .FirstOrDefaultAsync();

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
                        DiemGVHD = svdt.DiemGvhd,
                        DaChamDiem = svdt.DiemGvhd.HasValue,
                        CoBaoCaoCuoiKyDuyet = baoCaoCuoiKy != null,
                        FileBaoCaoCuoiKy = baoCaoCuoiKy?.FileBaocao,
                        TenFileBaoCao = baoCaoCuoiKy != null && !string.IsNullOrEmpty(baoCaoCuoiKy.FileBaocao)
                            ? Path.GetFileName(baoCaoCuoiKy.FileBaocao) : null
                    };

                    if (svdt.DiemGvhd.HasValue) daChamDiem = true;
                    danhSachSV.Add(svItem);
                }

                var tatCaCoBaoCao = danhSachSV.All(sv => sv.CoBaoCaoCuoiKyDuyet);

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
                    TatCaSVCoBaoCaoDuyet = tatCaCoBaoCao,
                    DanhSachSV = danhSachSV
                });
            }

            return View(result);
        }

        // Trang chấm điểm theo tiêu chí GVHD
        [HttpGet]
        public async Task<IActionResult> ChamDiemDeTai(int id)
        {
            var maGV = HttpContext.Session.GetString("UserCode");
            var giangVien = await _context.GiangViens
                .Include(gv => gv.IdNguoiDungNavigation)
                .FirstOrDefaultAsync(gv => gv.MaGv == maGV);
            if (giangVien == null)
                return RedirectToAction("Login", "Account", new { area = "" });

            var dot = await _context.DotDoAns
                .Where(d => d.TrangThai == true)
                .OrderByDescending(d => d.Id)
                .FirstOrDefaultAsync();

            var deTai = await _context.DeTais
                .Include(dt => dt.SinhVienDeTais)
                    .ThenInclude(svdt => svdt.IdSinhVienNavigation)
                        .ThenInclude(sv => sv!.IdNguoiDungNavigation)
                .FirstOrDefaultAsync(dt => dt.Id == id && dt.IdGvhd == giangVien.IdNguoiDung);

            if (deTai == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đề tài hoặc bạn không có quyền chấm.";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra đề tài đã có báo cáo cuối kỳ được duyệt (1 người nộp đại diện = cả nhóm)
            var sinhViensDuyet = deTai.SinhVienDeTais.Where(s => s.TrangThai == "DA_DUYET").ToList();
            var coBaoCaoCuoiKy = await _context.BaoCaoNops.AnyAsync(bc =>
                bc.IdDeTai == id &&
                bc.LoaiBaoCao == "CUOI_KY" &&
                bc.TrangThai == "DA_DUYET");

            if (!coBaoCaoCuoiKy)
            {
                TempData["ErrorMessage"] = "Đề tài chưa có báo cáo cuối kỳ được duyệt. Sinh viên cần nộp và được GVHD duyệt trước khi chấm điểm.";
                return RedirectToAction(nameof(Index));
            }

            // Lấy cấu hình phiếu chấm GVHD
            var cauHinh = await _context.CauHinhPhieuChamDots
                .Include(c => c.IdLoaiPhieuNavigation)
                    .ThenInclude(lp => lp != null ? lp.TieuChiChamDiems : null)
                .Where(c => c.VaiTroCham == "GVHD" && (c.IdDot == dot!.Id || c.IdDot == deTai.IdDot))
                .FirstOrDefaultAsync();

            if (cauHinh == null)
            {
                cauHinh = await _context.CauHinhPhieuChamDots
                    .Include(c => c.IdLoaiPhieuNavigation)
                        .ThenInclude(lp => lp != null ? lp.TieuChiChamDiems : null)
                    .Where(c => c.VaiTroCham == "GVHD")
                    .FirstOrDefaultAsync();
            }

            var tieuChis = cauHinh?.IdLoaiPhieuNavigation?.TieuChiChamDiems?
                .OrderBy(tc => tc.SttHienThi).ToList() ?? new List<TieuChiChamDiem>();

            if (!tieuChis.Any())
            {
                // Fallback: tìm trực tiếp từ LoaiPhieuCham
                var loaiPhieu = await _context.LoaiPhieuChams
                    .Include(lp => lp.TieuChiChamDiems)
                    .FirstOrDefaultAsync(lp => lp.TenLoaiPhieu != null && lp.TenLoaiPhieu.Contains("hướng dẫn"));
                tieuChis = loaiPhieu?.TieuChiChamDiems?.OrderBy(tc => tc.SttHienThi).ToList() ?? new List<TieuChiChamDiem>();
            }

            if (!tieuChis.Any())
            {
                TempData["ErrorMessage"] = "Chưa có tiêu chí chấm điểm cho phiếu GVHD. Vui lòng liên hệ BCN Khoa.";
                return RedirectToAction(nameof(Index));
            }

            // Lấy điểm đã chấm
            var idGiangVien = giangVien.IdNguoiDung;
            var diemDaCham = await _context.DiemChiTiets
                .Where(d => d.IdNguoiCham == idGiangVien &&
                            d.IdSinhVien.HasValue &&
                            sinhViensDuyet.Select(s => s.IdSinhVien).Contains(d.IdSinhVien) &&
                            tieuChis.Select(tc => tc.Id).Contains(d.IdTieuChi ?? 0))
                .ToListAsync();

            var viewModel = new ChamDiemGVHDViewModel
            {
                IdDeTai = deTai.Id,
                MaDeTai = deTai.MaDeTai,
                TenDeTai = deTai.TenDeTai,
                TenDot = dot?.TenDot,
                TenLoaiPhieu = cauHinh?.IdLoaiPhieuNavigation?.TenLoaiPhieu ?? "Phiếu chấm GVHD",
                DaChamDiem = sinhViensDuyet.Any(s => s.DiemGvhd.HasValue),
                DanhSachSinhVien = new List<SinhVienChamDiemGVHDInfo>(),
                TieuChis = new List<TieuChiGVHDViewModel>()
            };

            // Lấy báo cáo cuối kỳ của đề tài (nộp đại diện, dùng chung cho cả nhóm)
            var baoCaoChung = await _context.BaoCaoNops
                .Where(bc => bc.IdDeTai == id && bc.LoaiBaoCao == "CUOI_KY" && bc.TrangThai == "DA_DUYET")
                .OrderByDescending(bc => bc.NgayNop).FirstOrDefaultAsync();

            foreach (var svdt in sinhViensDuyet.OrderBy(s => s.IdSinhVienNavigation?.Mssv))
            {
                var sv = svdt.IdSinhVienNavigation;

                viewModel.DanhSachSinhVien.Add(new SinhVienChamDiemGVHDInfo
                {
                    IdSinhVien = sv!.IdNguoiDung,
                    IdSinhVienDeTai = svdt.Id,
                    Mssv = sv.Mssv,
                    HoTen = sv.IdNguoiDungNavigation?.HoTen,
                    FileBaoCao = baoCaoChung?.FileBaocao
                });
            }

            foreach (var tc in tieuChis)
            {
                var tcVM = new TieuChiGVHDViewModel
                {
                    Id = tc.Id,
                    TenTieuChi = tc.TenTieuChi,
                    MoTaHuongDan = tc.MoTaHuongDan,
                    TrongSo = tc.TrongSo ?? 0,
                    DiemToiDa = tc.DiemToiDa ?? 10,
                    Stt = tc.SttHienThi ?? 0
                };

                foreach (var sv in viewModel.DanhSachSinhVien)
                {
                    var diem = diemDaCham.FirstOrDefault(d => d.IdTieuChi == tc.Id && d.IdSinhVien == sv.IdSinhVien);
                    if (diem != null)
                    {
                        tcVM.DiemDaCham[sv.IdSinhVien] = diem.DiemSo ?? 0;
                        tcVM.NhanXetDaCham[sv.IdSinhVien] = diem.NhanXet ?? "";
                    }
                }

                viewModel.TieuChis.Add(tcVM);
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LuuDiemGVHD([FromBody] LuuDiemGVHDRequest request)
        {
            var maGV = HttpContext.Session.GetString("UserCode");
            var giangVien = await _context.GiangViens
                .Include(gv => gv.IdNguoiDungNavigation)
                .FirstOrDefaultAsync(gv => gv.MaGv == maGV);
            if (giangVien == null)
                return Json(new { success = false, message = "Không tìm thấy thông tin giảng viên." });

            var deTai = await _context.DeTais.FindAsync(request.IdDeTai);
            if (deTai == null || deTai.IdGvhd != giangVien.IdNguoiDung)
                return Json(new { success = false, message = "Bạn không có quyền chấm điểm đề tài này." });

            var svDeTais = await _context.SinhVienDeTais
                .Where(svdt => svdt.IdDeTai == request.IdDeTai && svdt.TrangThai == "DA_DUYET")
                .ToListAsync();

            // Lấy danh sách tiêu chí GVHD
            var cauHinh = await _context.CauHinhPhieuChamDots
                .Include(c => c.IdLoaiPhieuNavigation)
                    .ThenInclude(lp => lp != null ? lp.TieuChiChamDiems : null)
                .Where(c => c.VaiTroCham == "GVHD")
                .FirstOrDefaultAsync();

            var tieuChis = cauHinh?.IdLoaiPhieuNavigation?.TieuChiChamDiems?.ToList();
            if (tieuChis == null || !tieuChis.Any())
            {
                var loaiPhieu = await _context.LoaiPhieuChams
                    .Include(lp => lp.TieuChiChamDiems)
                    .FirstOrDefaultAsync(lp => lp.TenLoaiPhieu != null && lp.TenLoaiPhieu.Contains("hướng dẫn"));
                tieuChis = loaiPhieu?.TieuChiChamDiems?.ToList();
            }

            if (tieuChis == null || !tieuChis.Any())
                return Json(new { success = false, message = "Không tìm thấy tiêu chí chấm điểm." });

            var idGV = giangVien.IdNguoiDung;

            foreach (var diemSV in request.DanhSachDiem)
            {
                var svDeTai = svDeTais.FirstOrDefault(s => s.IdSinhVien == diemSV.IdSinhVien);
                if (svDeTai == null) continue;

                // Xóa điểm chi tiết cũ của GV này cho SV này
                var diemCu = await _context.DiemChiTiets
                    .Where(d => d.IdNguoiCham == idGV &&
                                d.IdSinhVien == diemSV.IdSinhVien &&
                                d.IdTieuChi.HasValue &&
                                tieuChis.Select(tc => tc.Id).Contains(d.IdTieuChi.Value))
                    .ToListAsync();
                _context.DiemChiTiets.RemoveRange(diemCu);

                double tongDiem = 0;

                foreach (var diemTC in diemSV.DiemTieuChis)
                {
                    _context.DiemChiTiets.Add(new DiemChiTiet
                    {
                        IdNguoiCham = idGV,
                        IdSinhVien = diemSV.IdSinhVien,
                        IdTieuChi = diemTC.IdTieuChi,
                        DiemSo = diemTC.DiemSo,
                        NhanXet = diemTC.NhanXet
                    });

                    tongDiem += diemTC.DiemSo;
                }

                // Cập nhật tổng điểm vào SinhVienDeTai
                svDeTai.DiemGvhd = Math.Round(tongDiem, 1);
                svDeTai.NhanXetGvhd = diemSV.NhanXetChung;
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Lưu điểm thành công!" });
        }
    }
}
