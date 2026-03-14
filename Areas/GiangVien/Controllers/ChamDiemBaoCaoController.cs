using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using DATN_TMS.Models;
using DATN_TMS.Areas.GiangVien.Models;

namespace DATN_TMS.Areas.GiangVien.Controllers
{
    [Area("GiangVien")]
    public class ChamDiemBaoCaoController : Controller
    {
        private readonly QuanLyDoAnTotNghiepContext _context;

        public ChamDiemBaoCaoController(QuanLyDoAnTotNghiepContext context)
        {
            _context = context;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var sessionRole = HttpContext.Session.GetString("Role");
            var isGV = User?.Identity?.IsAuthenticated == true && 
                       (User.IsInRole("GIANG_VIEN") || User.IsInRole("BO_MON") || User.IsInRole("BCN_KHOA"));
            var isGVBySession = sessionRole == "GIANG_VIEN" || sessionRole == "BO_MON" || sessionRole == "BCN_KHOA";

            if (!isGV && !isGVBySession)
            {
                context.Result = RedirectToAction("Login", "Account", new { area = "" });
                return;
            }
            base.OnActionExecuting(context);
        }

        private async Task<int> GetCurrentUserId()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail)) return 0;

            var nguoiDung = await _context.NguoiDungs.FirstOrDefaultAsync(nd => nd.Email == userEmail);
            return nguoiDung?.Id ?? 0;
        }

        // API kiểm tra GV có được phân vào HĐ đã duyệt không (cho sidebar)
        [HttpGet]
        public async Task<IActionResult> CheckHoiDong()
        {
            var currentUserId = await GetCurrentUserId();
            if (currentUserId == 0)
            {
                return Json(new { coHoiDong = false });
            }

            var coHoiDong = await _context.ThanhVienHdBaoCaos
                .Include(tv => tv.IdHdBaocaoNavigation)
                .AnyAsync(tv => tv.IdGiangVien == currentUserId &&
                                tv.IdHdBaocaoNavigation != null &&
                                tv.IdHdBaocaoNavigation.TrangThaiDuyet == "DA_DUYET" &&
                                tv.IdHdBaocaoNavigation.LoaiHoiDong != "DUYET_DE_TAI");

            return Json(new { coHoiDong = coHoiDong });
        }

        // GET: /GiangVien/ChamDiemBaoCao
        // Hiển thị danh sách hội đồng mà GV tham gia (đã được duyệt)
        public async Task<IActionResult> Index()
        {
            var currentUserId = await GetCurrentUserId();
            if (currentUserId == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            // Lấy danh sách hội đồng đã được duyệt mà GV này là thành viên
            var hoiDongs = await _context.HoiDongBaoCaos
                .Include(hd => hd.IdBoMonNavigation)
                .Include(hd => hd.IdDotNavigation)
                .Include(hd => hd.ThanhVienHdBaoCaos)
                    .ThenInclude(tv => tv.IdGiangVienNavigation)
                        .ThenInclude(gv => gv != null ? gv.IdNguoiDungNavigation : null)
                .Include(hd => hd.PhienBaoVes)
                .Where(hd => hd.TrangThaiDuyet == "DA_DUYET" &&
                             hd.LoaiHoiDong != "DUYET_DE_TAI" &&
                             hd.ThanhVienHdBaoCaos.Any(tv => tv.IdGiangVien == currentUserId))
                .OrderByDescending(hd => hd.NgayBaoCao)
                .ToListAsync();

            var viewModels = hoiDongs.Select(hd => new HoiDongChamDiemViewModel
            {
                Id = hd.Id,
                MaHoiDong = hd.MaHoiDong ?? "",
                TenHoiDong = hd.TenHoiDong ?? "",
                LoaiHoiDong = hd.LoaiHoiDong ?? "",
                TenBoMon = hd.IdBoMonNavigation?.TenBoMon ?? "N/A",
                NgayBaoCao = hd.NgayBaoCao,
                DiaDiem = hd.DiaDiem ?? "",
                SoLuongDeTai = hd.PhienBaoVes.Count,
                VaiTroTrongHoiDong = hd.ThanhVienHdBaoCaos
                    .FirstOrDefault(tv => tv.IdGiangVien == currentUserId)?.VaiTro ?? "",
                DaChamDiem = hd.ThanhVienHdBaoCaos
                    .FirstOrDefault(tv => tv.IdGiangVien == currentUserId)?.DaChamDiem ?? false,
                CoDenNgayBaoCao = hd.NgayBaoCao.HasValue && 
                                   DateOnly.FromDateTime(DateTime.Today) >= hd.NgayBaoCao.Value
            }).ToList();

            return View(viewModels);
        }

        // GET: /GiangVien/ChamDiemBaoCao/Detail/{id}
        // Xem chi tiết hội đồng và danh sách đề tài để chấm điểm
        public async Task<IActionResult> Detail(int id)
        {
            var currentUserId = await GetCurrentUserId();
            if (currentUserId == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            var hoiDong = await _context.HoiDongBaoCaos
                .Include(hd => hd.IdBoMonNavigation)
                .Include(hd => hd.IdDotNavigation)
                .Include(hd => hd.ThanhVienHdBaoCaos)
                    .ThenInclude(tv => tv.IdGiangVienNavigation)
                        .ThenInclude(gv => gv != null ? gv.IdNguoiDungNavigation : null)
                .Include(hd => hd.PhienBaoVes)
                    .ThenInclude(pb => pb.IdSinhVienDeTaiNavigation)
                        .ThenInclude(svdt => svdt != null ? svdt.IdSinhVienNavigation : null)
                            .ThenInclude(sv => sv != null ? sv.IdNguoiDungNavigation : null)
                .Include(hd => hd.PhienBaoVes)
                    .ThenInclude(pb => pb.IdSinhVienDeTaiNavigation)
                        .ThenInclude(svdt => svdt != null ? svdt.IdDeTaiNavigation : null)
                            .ThenInclude(dt => dt != null ? dt.IdGvhdNavigation : null)
                                .ThenInclude(gv => gv != null ? gv.IdNguoiDungNavigation : null)
                .FirstOrDefaultAsync(hd => hd.Id == id && hd.TrangThaiDuyet == "DA_DUYET");

            if (hoiDong == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy hội đồng hoặc hội đồng chưa được duyệt.";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra GV có phải là thành viên hội đồng này không
            var thanhVien = hoiDong.ThanhVienHdBaoCaos.FirstOrDefault(tv => tv.IdGiangVien == currentUserId);
            if (thanhVien == null)
            {
                TempData["ErrorMessage"] = "Bạn không phải là thành viên của hội đồng này.";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra đã đến ngày báo cáo chưa
            if (hoiDong.NgayBaoCao.HasValue && DateOnly.FromDateTime(DateTime.Today) < hoiDong.NgayBaoCao.Value)
            {
                TempData["ErrorMessage"] = $"Chưa đến ngày báo cáo ({hoiDong.NgayBaoCao.Value:dd/MM/yyyy}). Không thể xem chi tiết.";
                return RedirectToAction(nameof(Index));
            }

            // Lấy danh sách phiên bảo vệ với thông tin điểm
            var phienBaoVes = new List<PhienBaoVeViewModel>();
            foreach (var pb in hoiDong.PhienBaoVes.OrderBy(p => p.SttBaoCao))
            {
                var svdt = pb.IdSinhVienDeTaiNavigation;
                var sv = svdt?.IdSinhVienNavigation;
                var dt = svdt?.IdDeTaiNavigation;

                // Lấy điểm GVHD
                var diemGvhd = svdt?.DiemGvhd;

                // Lấy điểm của các thành viên hội đồng
                var diemHoiDong = await _context.DiemChiTiets
                    .Include(d => d.IdNguoiChamNavigation)
                        .ThenInclude(gv => gv != null ? gv.IdNguoiDungNavigation : null)
                    .Where(d => d.IdPhienBaoVe == pb.Id && d.IdSinhVien == sv!.IdNguoiDung)
                    .ToListAsync();

                // Kiểm tra xác nhận của chủ tịch
                var xacNhan = await _context.XacNhanDiemChuTichs
                    .FirstOrDefaultAsync(x => x.IdPhienBaoVe == pb.Id);

                phienBaoVes.Add(new PhienBaoVeViewModel
                {
                    Id = pb.Id,
                    SttBaoCao = pb.SttBaoCao ?? 0,
                    IdSinhVienDeTai = pb.IdSinhVienDeTai ?? 0,
                    TenSinhVien = sv?.IdNguoiDungNavigation?.HoTen ?? "N/A",
                    Mssv = sv?.Mssv ?? "",
                    TenDeTai = dt?.TenDeTai ?? "N/A",
                    MaDeTai = dt?.MaDeTai ?? "",
                    TenGVHD = dt?.IdGvhdNavigation?.IdNguoiDungNavigation?.HoTen ?? "N/A",
                    DiemGVHD = diemGvhd,
                    NhanXetGVHD = svdt?.NhanXetGvhd ?? "",
                    LinkTaiLieu = pb.LinkTaiLieu ?? "",
                    DiemHoiDong = diemHoiDong.Select(d => new DiemThanhVienViewModel
                    {
                        IdNguoiCham = d.IdNguoiCham ?? 0,
                        TenNguoiCham = d.IdNguoiChamNavigation?.IdNguoiDungNavigation?.HoTen ?? "",
                        DiemSo = d.DiemSo ?? 0,
                        NhanXet = d.NhanXet ?? ""
                    }).ToList(),
                    TrangThaiXacNhan = xacNhan?.TrangThai ?? "CHO_XAC_NHAN",
                    DiemTongKetCuoi = xacNhan?.DiemTongKetCuoi
                });
            }

            var viewModel = new ChiTietHoiDongChamDiemViewModel
            {
                HoiDong = new HoiDongChamDiemViewModel
                {
                    Id = hoiDong.Id,
                    MaHoiDong = hoiDong.MaHoiDong ?? "",
                    TenHoiDong = hoiDong.TenHoiDong ?? "",
                    LoaiHoiDong = hoiDong.LoaiHoiDong ?? "",
                    TenBoMon = hoiDong.IdBoMonNavigation?.TenBoMon ?? "",
                    NgayBaoCao = hoiDong.NgayBaoCao,
                    DiaDiem = hoiDong.DiaDiem ?? "",
                    VaiTroTrongHoiDong = thanhVien.VaiTro ?? ""
                },
                ThanhViens = hoiDong.ThanhVienHdBaoCaos.Select(tv => new ThanhVienHoiDongChamDiemViewModel
                {
                    IdGiangVien = tv.IdGiangVien ?? 0,
                    TenGiangVien = tv.IdGiangVienNavigation?.IdNguoiDungNavigation?.HoTen ?? "",
                    VaiTro = tv.VaiTro ?? "",
                    DaChamDiem = tv.DaChamDiem ?? false
                }).ToList(),
                PhienBaoVes = phienBaoVes,
                VaiTroHienTai = thanhVien.VaiTro ?? "",
                LaChuTich = thanhVien.VaiTro == "CHU_TICH",
                LaThuKy = thanhVien.VaiTro == "THU_KY"
            };

            return View(viewModel);
        }

        // GET: /GiangVien/ChamDiemBaoCao/ChamDiem/{phienBaoVeId}
        // Trang chấm điểm cho một phiên bảo vệ
        public async Task<IActionResult> ChamDiem(int phienBaoVeId)
        {
            var currentUserId = await GetCurrentUserId();
            if (currentUserId == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            var phien = await _context.PhienBaoVes
                .Include(p => p.IdHdBaocaoNavigation)
                    .ThenInclude(hd => hd != null ? hd.ThanhVienHdBaoCaos : null)
                .Include(p => p.IdSinhVienDeTaiNavigation)
                    .ThenInclude(svdt => svdt != null ? svdt.IdSinhVienNavigation : null)
                        .ThenInclude(sv => sv != null ? sv.IdNguoiDungNavigation : null)
                .Include(p => p.IdSinhVienDeTaiNavigation)
                    .ThenInclude(svdt => svdt != null ? svdt.IdDeTaiNavigation : null)
                .FirstOrDefaultAsync(p => p.Id == phienBaoVeId);

            if (phien == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy phiên bảo vệ.";
                return RedirectToAction(nameof(Index));
            }

            var hoiDong = phien.IdHdBaocaoNavigation;
            if (hoiDong == null || hoiDong.TrangThaiDuyet != "DA_DUYET")
            {
                TempData["ErrorMessage"] = "Hội đồng chưa được duyệt.";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra GV có phải thành viên hội đồng
            var thanhVien = hoiDong.ThanhVienHdBaoCaos?.FirstOrDefault(tv => tv.IdGiangVien == currentUserId);
            if (thanhVien == null)
            {
                TempData["ErrorMessage"] = "Bạn không phải thành viên của hội đồng này.";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra đã đến ngày báo cáo chưa
            if (hoiDong.NgayBaoCao.HasValue && DateOnly.FromDateTime(DateTime.Today) < hoiDong.NgayBaoCao.Value)
            {
                TempData["ErrorMessage"] = "Chưa đến ngày báo cáo.";
                return RedirectToAction("Detail", new { id = hoiDong.Id });
            }

            // Kiểm tra GVHD đã chấm điểm chưa
            var svdt = phien.IdSinhVienDeTaiNavigation;
            if (svdt?.DiemGvhd == null)
            {
                TempData["ErrorMessage"] = "GVHD chưa chấm điểm cho sinh viên này. Vui lòng đợi GVHD chấm điểm trước.";
                return RedirectToAction("Detail", new { id = hoiDong.Id });
            }

            // Lấy tiêu chí chấm điểm theo loại hội đồng
            var loaiPhieu = hoiDong.LoaiHoiDong == "GIUA_KY" ? "GVHD_GIUA_KY" : "HoiDong";
            var cauHinh = await _context.CauHinhPhieuChamDots
                .Include(c => c.IdLoaiPhieuNavigation)
                    .ThenInclude(lp => lp != null ? lp.TieuChiChamDiems : null)
                .FirstOrDefaultAsync(c => c.IdDot == hoiDong.IdDot && c.VaiTroCham == loaiPhieu);

            var tieuChis = cauHinh?.IdLoaiPhieuNavigation?.TieuChiChamDiems?.ToList() 
                           ?? new List<TieuChiChamDiem>();

            // Lấy điểm đã chấm (nếu có)
            var diemDaCham = await _context.DiemChiTiets
                .Where(d => d.IdPhienBaoVe == phienBaoVeId && 
                            d.IdNguoiCham == currentUserId &&
                            d.IdSinhVien == svdt.IdSinhVien)
                .ToListAsync();

            var viewModel = new ChamDiemPhienBaoVeViewModel
            {
                PhienBaoVeId = phien.Id,
                HoiDongId = hoiDong.Id,
                TenHoiDong = hoiDong.TenHoiDong ?? "",
                TenSinhVien = svdt?.IdSinhVienNavigation?.IdNguoiDungNavigation?.HoTen ?? "",
                Mssv = svdt?.IdSinhVienNavigation?.Mssv ?? "",
                TenDeTai = svdt?.IdDeTaiNavigation?.TenDeTai ?? "",
                DiemGVHD = svdt?.DiemGvhd ?? 0,
                NhanXetGVHD = svdt?.NhanXetGvhd ?? "",
                TieuChis = tieuChis.Select(tc => new TieuChiChamDiemViewModel
                {
                    Id = tc.Id,
                    TenTieuChi = tc.TenTieuChi ?? "",
                    MoTaHuongDan = tc.MoTaHuongDan ?? "",
                    TrongSo = tc.TrongSo ?? 0,
                    DiemToiDa = tc.DiemToiDa ?? 0,
                    DiemDaCham = diemDaCham.FirstOrDefault(d => d.IdTieuChi == tc.Id)?.DiemSo ?? 0,
                    NhanXetDaCham = diemDaCham.FirstOrDefault(d => d.IdTieuChi == tc.Id)?.NhanXet ?? ""
                }).ToList(),
                DaChamDiem = diemDaCham.Any()
            };

            return View(viewModel);
        }

        // POST: /GiangVien/ChamDiemBaoCao/LuuDiem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LuuDiem(LuuDiemRequest request)
        {
            var currentUserId = await GetCurrentUserId();
            if (currentUserId == 0)
            {
                return Json(new { success = false, message = "Không xác định được người dùng." });
            }

            try
            {
                var phien = await _context.PhienBaoVes
                    .Include(p => p.IdHdBaocaoNavigation)
                        .ThenInclude(hd => hd != null ? hd.ThanhVienHdBaoCaos : null)
                    .Include(p => p.IdSinhVienDeTaiNavigation)
                    .FirstOrDefaultAsync(p => p.Id == request.PhienBaoVeId);

                if (phien == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy phiên bảo vệ." });
                }

                var svdt = phien.IdSinhVienDeTaiNavigation;
                if (svdt == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin sinh viên." });
                }

                // Xóa điểm cũ nếu có
                var diemCu = await _context.DiemChiTiets
                    .Where(d => d.IdPhienBaoVe == request.PhienBaoVeId &&
                                d.IdNguoiCham == currentUserId &&
                                d.IdSinhVien == svdt.IdSinhVien)
                    .ToListAsync();

                _context.DiemChiTiets.RemoveRange(diemCu);

                // Lưu điểm mới
                foreach (var diem in request.DiemChiTiets)
                {
                    var diemMoi = new DiemChiTiet
                    {
                        IdPhienBaoVe = request.PhienBaoVeId,
                        IdNguoiCham = currentUserId,
                        IdSinhVien = svdt.IdSinhVien,
                        IdTieuChi = diem.IdTieuChi,
                        DiemSo = diem.DiemSo,
                        NhanXet = diem.NhanXet
                    };
                    _context.DiemChiTiets.Add(diemMoi);
                }

                // Đánh dấu thành viên đã chấm điểm
                var thanhVien = phien.IdHdBaocaoNavigation?.ThanhVienHdBaoCaos?
                    .FirstOrDefault(tv => tv.IdGiangVien == currentUserId);
                if (thanhVien != null)
                {
                    thanhVien.DaChamDiem = true;
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Lưu điểm thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // POST: /GiangVien/ChamDiemBaoCao/ThuKyDieuChinhDiem
        // Thư ký điều chỉnh điểm khi chênh lệch lớn
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThuKyDieuChinhDiem(DieuChinhDiemRequest request)
        {
            var currentUserId = await GetCurrentUserId();
            if (currentUserId == 0)
            {
                return Json(new { success = false, message = "Không xác định được người dùng." });
            }

            try
            {
                // Kiểm tra có phải thư ký không
                var phien = await _context.PhienBaoVes
                    .Include(p => p.IdHdBaocaoNavigation)
                        .ThenInclude(hd => hd != null ? hd.ThanhVienHdBaoCaos : null)
                    .FirstOrDefaultAsync(p => p.Id == request.PhienBaoVeId);

                if (phien == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy phiên bảo vệ." });
                }

                var thanhVien = phien.IdHdBaocaoNavigation?.ThanhVienHdBaoCaos?
                    .FirstOrDefault(tv => tv.IdGiangVien == currentUserId);

                if (thanhVien?.VaiTro != "THU_KY")
                {
                    return Json(new { success = false, message = "Chỉ Thư ký mới có quyền điều chỉnh điểm." });
                }

                // Lưu lịch sử cập nhật
                var lichSu = new LichSuCapNhatDiem
                {
                    IdPhienBaoVe = request.PhienBaoVeId,
                    IdSinhVien = request.IdSinhVien,
                    IdNguoiCapNhat = currentUserId,
                    LoaiCapNhat = "THU_KY_DIEU_CHINH",
                    DiemCu = request.DiemCu,
                    DiemMoi = request.DiemMoi,
                    LyDo = request.LyDo,
                    NgayCapNhat = DateTime.Now
                };
                _context.LichSuCapNhatDiems.Add(lichSu);

                // Cập nhật điểm mới (nếu cần)
                // Điều chỉnh theo yêu cầu nghiệp vụ cụ thể

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Đã điều chỉnh điểm thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // POST: /GiangVien/ChamDiemBaoCao/ChuTichXacNhan
        // Chủ tịch xác nhận điểm cuối cùng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChuTichXacNhan(XacNhanDiemRequest request)
        {
            var currentUserId = await GetCurrentUserId();
            if (currentUserId == 0)
            {
                return Json(new { success = false, message = "Không xác định được người dùng." });
            }

            try
            {
                var phien = await _context.PhienBaoVes
                    .Include(p => p.IdHdBaocaoNavigation)
                        .ThenInclude(hd => hd != null ? hd.ThanhVienHdBaoCaos : null)
                    .FirstOrDefaultAsync(p => p.Id == request.PhienBaoVeId);

                if (phien == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy phiên bảo vệ." });
                }

                var thanhVien = phien.IdHdBaocaoNavigation?.ThanhVienHdBaoCaos?
                    .FirstOrDefault(tv => tv.IdGiangVien == currentUserId);

                if (thanhVien?.VaiTro != "CHU_TICH")
                {
                    return Json(new { success = false, message = "Chỉ Chủ tịch mới có quyền xác nhận điểm." });
                }

                // Kiểm tra tất cả thành viên đã chấm điểm chưa
                var tatCaDaCham = phien.IdHdBaocaoNavigation?.ThanhVienHdBaoCaos?
                    .All(tv => tv.DaChamDiem == true) ?? false;

                if (!tatCaDaCham)
                {
                    return Json(new { success = false, message = "Chưa tất cả thành viên hội đồng chấm điểm." });
                }

                // Lưu hoặc cập nhật xác nhận
                var xacNhan = await _context.XacNhanDiemChuTichs
                    .FirstOrDefaultAsync(x => x.IdPhienBaoVe == request.PhienBaoVeId);

                if (xacNhan == null)
                {
                    xacNhan = new XacNhanDiemChuTich
                    {
                        IdPhienBaoVe = request.PhienBaoVeId,
                        IdChuTich = currentUserId,
                        TrangThai = "DA_XAC_NHAN",
                        DiemTongKetCuoi = request.DiemTongKet,
                        GhiChu = request.GhiChu,
                        NgayXacNhan = DateTime.Now
                    };
                    _context.XacNhanDiemChuTichs.Add(xacNhan);
                }
                else
                {
                    xacNhan.TrangThai = "DA_XAC_NHAN";
                    xacNhan.DiemTongKetCuoi = request.DiemTongKet;
                    xacNhan.GhiChu = request.GhiChu;
                    xacNhan.NgayXacNhan = DateTime.Now;
                }

                // Lưu lịch sử
                var lichSu = new LichSuCapNhatDiem
                {
                    IdPhienBaoVe = request.PhienBaoVeId,
                    IdSinhVien = request.IdSinhVien,
                    IdNguoiCapNhat = currentUserId,
                    LoaiCapNhat = "CHU_TICH_XAC_NHAN",
                    DiemMoi = request.DiemTongKet,
                    LyDo = request.GhiChu,
                    NgayCapNhat = DateTime.Now
                };
                _context.LichSuCapNhatDiems.Add(lichSu);

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Đã xác nhận điểm thành công! Có thể chuyển sang đề tài tiếp theo." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // GET: /GiangVien/ChamDiemBaoCao/BangDiemTongHop/{phienBaoVeId}
        // Hiển thị bảng điểm tổng hợp của một đề tài
        public async Task<IActionResult> BangDiemTongHop(int phienBaoVeId)
        {
            var currentUserId = await GetCurrentUserId();
            if (currentUserId == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            var phien = await _context.PhienBaoVes
                .Include(p => p.IdHdBaocaoNavigation)
                    .ThenInclude(hd => hd != null ? hd.ThanhVienHdBaoCaos : null)
                        .ThenInclude(tv => tv != null ? tv.IdGiangVienNavigation : null)
                            .ThenInclude(gv => gv != null ? gv.IdNguoiDungNavigation : null)
                .Include(p => p.IdSinhVienDeTaiNavigation)
                    .ThenInclude(svdt => svdt != null ? svdt.IdSinhVienNavigation : null)
                        .ThenInclude(sv => sv != null ? sv.IdNguoiDungNavigation : null)
                .Include(p => p.IdSinhVienDeTaiNavigation)
                    .ThenInclude(svdt => svdt != null ? svdt.IdDeTaiNavigation : null)
                        .ThenInclude(dt => dt != null ? dt.IdGvhdNavigation : null)
                            .ThenInclude(gv => gv != null ? gv.IdNguoiDungNavigation : null)
                .Include(p => p.DiemChiTiets)
                    .ThenInclude(d => d.IdNguoiChamNavigation)
                        .ThenInclude(gv => gv != null ? gv.IdNguoiDungNavigation : null)
                .FirstOrDefaultAsync(p => p.Id == phienBaoVeId);

            if (phien == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy phiên bảo vệ.";
                return RedirectToAction(nameof(Index));
            }

            var hoiDong = phien.IdHdBaocaoNavigation;
            var svdt = phien.IdSinhVienDeTaiNavigation;
            var sv = svdt?.IdSinhVienNavigation;
            var dt = svdt?.IdDeTaiNavigation;

            // Tính điểm trung bình hội đồng
            var diemHoiDong = phien.DiemChiTiets
                .GroupBy(d => d.IdNguoiCham)
                .Select(g => new
                {
                    IdNguoiCham = g.Key,
                    TongDiem = g.Sum(d => d.DiemSo ?? 0)
                })
                .ToList();

            var diemTBHoiDong = diemHoiDong.Any() ? diemHoiDong.Average(d => d.TongDiem) : 0;

            // Kiểm tra chênh lệch
            var diemGVHD = svdt?.DiemGvhd ?? 0;
            var chenhLechGVHD = Math.Abs(diemGVHD - diemTBHoiDong);
            var coChenhLechLon = false;

            // Kiểm tra chênh lệch giữa các thành viên HĐ
            if (diemHoiDong.Count > 1)
            {
                var diemMax = diemHoiDong.Max(d => d.TongDiem);
                var diemMin = diemHoiDong.Min(d => d.TongDiem);
                if (diemMax - diemMin > 1)
                {
                    coChenhLechLon = true;
                }
            }

            // Kiểm tra chênh lệch GVHD với TB hội đồng (> 2 điểm)
            if (chenhLechGVHD > 2)
            {
                coChenhLechLon = true;
            }

            // Lấy vai trò hiện tại
            var thanhVienHienTai = hoiDong?.ThanhVienHdBaoCaos?
                .FirstOrDefault(tv => tv.IdGiangVien == currentUserId);

            // Lấy xác nhận chủ tịch
            var xacNhan = await _context.XacNhanDiemChuTichs
                .FirstOrDefaultAsync(x => x.IdPhienBaoVe == phienBaoVeId);

            // Lấy lịch sử cập nhật
            var lichSuCapNhat = await _context.LichSuCapNhatDiems
                .Include(l => l.IdNguoiCapNhatNavigation)
                    .ThenInclude(gv => gv != null ? gv.IdNguoiDungNavigation : null)
                .Where(l => l.IdPhienBaoVe == phienBaoVeId)
                .OrderByDescending(l => l.NgayCapNhat)
                .ToListAsync();

            var viewModel = new BangDiemTongHopViewModel
            {
                PhienBaoVeId = phien.Id,
                HoiDongId = hoiDong?.Id ?? 0,
                TenHoiDong = hoiDong?.TenHoiDong ?? "",
                TenSinhVien = sv?.IdNguoiDungNavigation?.HoTen ?? "",
                Mssv = sv?.Mssv ?? "",
                TenDeTai = dt?.TenDeTai ?? "",
                MaDeTai = dt?.MaDeTai ?? "",

                // Điểm GVHD
                TenGVHD = dt?.IdGvhdNavigation?.IdNguoiDungNavigation?.HoTen ?? "",
                DiemGVHD = diemGVHD,
                NhanXetGVHD = svdt?.NhanXetGvhd ?? "",

                // Điểm các thành viên hội đồng
                DiemThanhViens = hoiDong?.ThanhVienHdBaoCaos?.Select(tv => new DiemThanhVienTongHopViewModel
                {
                    IdGiangVien = tv.IdGiangVien ?? 0,
                    TenGiangVien = tv.IdGiangVienNavigation?.IdNguoiDungNavigation?.HoTen ?? "",
                    VaiTro = tv.VaiTro ?? "",
                    DaChamDiem = tv.DaChamDiem ?? false,
                    TongDiem = phien.DiemChiTiets
                        .Where(d => d.IdNguoiCham == tv.IdGiangVien)
                        .Sum(d => d.DiemSo ?? 0)
                }).ToList() ?? new List<DiemThanhVienTongHopViewModel>(),

                DiemTBHoiDong = diemTBHoiDong,
                ChenhLechGVHD = chenhLechGVHD,
                CoChenhLechLon = coChenhLechLon,

                // Xác nhận
                TrangThaiXacNhan = xacNhan?.TrangThai ?? "CHO_XAC_NHAN",
                DiemTongKetCuoi = xacNhan?.DiemTongKetCuoi,
                GhiChuXacNhan = xacNhan?.GhiChu ?? "",

                // Lịch sử
                LichSuCapNhat = lichSuCapNhat.Select(l => new LichSuCapNhatViewModel
                {
                    TenNguoiCapNhat = l.IdNguoiCapNhatNavigation?.IdNguoiDungNavigation?.HoTen ?? "",
                    LoaiCapNhat = l.LoaiCapNhat ?? "",
                    DiemCu = l.DiemCu,
                    DiemMoi = l.DiemMoi,
                    LyDo = l.LyDo ?? "",
                    NgayCapNhat = l.NgayCapNhat
                }).ToList(),

                // Quyền
                LaThuKy = thanhVienHienTai?.VaiTro == "THU_KY",
                LaChuTich = thanhVienHienTai?.VaiTro == "CHU_TICH"
            };

            return View(viewModel);
        }
    }
}
