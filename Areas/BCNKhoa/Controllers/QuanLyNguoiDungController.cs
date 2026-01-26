#nullable enable
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DATN_TMS.Areas.BCNKhoa.Models;
using DATN_TMS.Models;
using X.PagedList.Extensions;

namespace DATN_TMS.Areas.BCNKhoa.Controllers
{
    [Area("BCNKhoa")]
    public class QuanLyNguoiDungController : Controller
    {
        private readonly QuanLyDoAnTotNghiepContext _context;

        public QuanLyNguoiDungController(QuanLyDoAnTotNghiepContext context)
        {
            _context = context;
        }

        // GET: Index - Danh sách người dùng
        public async Task<IActionResult> Index(int? page, string? role, string? searchString)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            ViewBag.CurrentRole = role;
            ViewBag.CurrentFilter = searchString;

            // Load dropdown data cho modal
            await LoadDropdownData();

            // Query người dùng
            var query = _context.NguoiDungs
                .Include(nd => nd.IdVaiTros)
                .Include(nd => nd.SinhVien)
                    .ThenInclude(sv => sv != null ? sv.IdChuyenNganhNavigation : null)
                .Include(nd => nd.SinhVien)
                    .ThenInclude(sv => sv != null ? sv.IdKhoaHocNavigation : null)
                .Include(nd => nd.GiangVien)
                    .ThenInclude(gv => gv != null ? gv.IdBoMonNavigation : null)
                .AsQueryable();

            // Filter by role / status
            if (!string.IsNullOrEmpty(role) && role != "all")
            {
                if (role == "Sinh viên")
                {
                    query = query.Where(nd => nd.SinhVien != null);
                }
                else if (role == "Giảng viên")
                {
                    query = query.Where(nd => nd.GiangVien != null && !nd.IdVaiTros.Any(v => v.MaVaiTro == "BCN_KHOA" || v.MaVaiTro == "BO_MON"));
                }
                else if (role == "BCN Khoa")
                {
                    query = query.Where(nd => nd.IdVaiTros.Any(v => v.MaVaiTro == "BCN_KHOA"));
                }
                else if (role == "Bộ môn")
                {
                    query = query.Where(nd => nd.IdVaiTros.Any(v => v.MaVaiTro == "BO_MON"));
                }
                else if (role == "Chưa phân quyền")
                {
                    query = query.Where(nd => !nd.IdVaiTros.Any());
                }
                else if (role == "Tạm khóa")
                {
                    query = query.Where(nd => nd.TrangThai == 0);
                }
            }

            // Search
            if (!string.IsNullOrEmpty(searchString))
            {
                var search = searchString.ToLower();
                query = query.Where(nd =>
                    (nd.HoTen != null && nd.HoTen.ToLower().Contains(search)) ||
                    nd.Email.ToLower().Contains(search) ||
                    (nd.SinhVien != null && nd.SinhVien.Mssv != null && nd.SinhVien.Mssv.ToLower().Contains(search)) ||
                    (nd.GiangVien != null && nd.GiangVien.MaGv != null && nd.GiangVien.MaGv.ToLower().Contains(search))
                );
            }

            // Map to ViewModel
            var result = query.Select(nd => new QuanLyNguoiDungViewModel
            {
                Id = nd.Id,
                MaSo = nd.SinhVien != null ? nd.SinhVien.Mssv : (nd.GiangVien != null ? nd.GiangVien.MaGv : null),
                HoTen = nd.HoTen,
                Email = nd.Email,
                Sdt = nd.Sdt,
                AvatarUrl = nd.AvatarUrl,
                LoaiNguoiDung = nd.SinhVien != null ? "Sinh viên" : (nd.GiangVien != null ? "Giảng viên" : "Chưa xác định"),
                VaiTros = nd.IdVaiTros.Select(v => v.TenVaiTro ?? "").ToList(),
                HocVi = nd.GiangVien != null ? nd.GiangVien.HocVi : null,
                TenBoMon = nd.GiangVien != null && nd.GiangVien.IdBoMonNavigation != null ? nd.GiangVien.IdBoMonNavigation.TenBoMon : null,
                TenChuyenNganh = nd.SinhVien != null && nd.SinhVien.IdChuyenNganhNavigation != null ? nd.SinhVien.IdChuyenNganhNavigation.TenChuyenNganh : null,
                TenKhoaHoc = nd.SinhVien != null && nd.SinhVien.IdKhoaHocNavigation != null ? nd.SinhVien.IdKhoaHocNavigation.TenKhoa : null,
                TrangThai = nd.TrangThai ?? 1
            }).OrderByDescending(x => x.Id);

            var pagedList = result.ToPagedList(pageNumber, pageSize);

            // Đếm số lượng theo vai trò (cho stat cards)
            ViewBag.CountSV = await _context.SinhViens.CountAsync();
            ViewBag.CountGV = await _context.GiangViens.CountAsync();
            ViewBag.CountBCN = await _context.NguoiDungs.CountAsync(nd => nd.IdVaiTros.Any(v => v.MaVaiTro == "BCN_KHOA"));
            ViewBag.CountBM = await _context.NguoiDungs.CountAsync(nd => nd.IdVaiTros.Any(v => v.MaVaiTro == "BO_MON"));
            ViewBag.CountNone = await _context.NguoiDungs.CountAsync(nd => !nd.IdVaiTros.Any());
            ViewBag.CountTamKhoa = await _context.NguoiDungs.CountAsync(nd => nd.TrangThai == 0);

            return View(pagedList);
        }

        // POST: Edit - Cập nhật người dùng từ Modal
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(NguoiDungFormViewModel model)
        {
            var nguoiDung = await _context.NguoiDungs
                .Include(nd => nd.IdVaiTros)
                .Include(nd => nd.SinhVien)
                .Include(nd => nd.GiangVien)
                .FirstOrDefaultAsync(nd => nd.Id == model.Id);

            if (nguoiDung == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy người dùng!";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Cập nhật thông tin cơ bản
                nguoiDung.HoTen = model.HoTen;
                nguoiDung.Sdt = model.Sdt;
                nguoiDung.TrangThai = model.TrangThai;

                // Cập nhật thông tin sinh viên
                if (nguoiDung.SinhVien != null)
                {
                    nguoiDung.SinhVien.Mssv = model.Mssv;
                    nguoiDung.SinhVien.IdChuyenNganh = model.IdChuyenNganh;
                    nguoiDung.SinhVien.IdKhoaHoc = model.IdKhoaHoc;
                }

                // Cập nhật thông tin giảng viên
                if (nguoiDung.GiangVien != null)
                {
                    nguoiDung.GiangVien.MaGv = model.MaGv;
                    nguoiDung.GiangVien.HocVi = model.HocVi;
                    nguoiDung.GiangVien.IdBoMon = model.IdBoMon;

                    // Cập nhật vai trò cho giảng viên
                    nguoiDung.IdVaiTros.Clear();
                    if (model.SelectedVaiTroId.HasValue)
                    {
                        var selectedVaiTro = await _context.VaiTros.FindAsync(model.SelectedVaiTroId.Value);
                        if (selectedVaiTro != null)
                        {
                            nguoiDung.IdVaiTros.Add(selectedVaiTro);
                        }
                    }
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Create - Thêm người dùng mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NguoiDungFormViewModel model)
        {
            try
            {
                // Kiểm tra email đã tồn tại
                var existEmail = await _context.NguoiDungs.AnyAsync(nd => nd.Email == model.Email);
                if (existEmail)
                {
                    TempData["ErrorMessage"] = "Email đã tồn tại trong hệ thống!";
                    return RedirectToAction(nameof(Index));
                }

                // Kiểm tra mã số đã tồn tại
                if (model.LoaiNguoiDung == "SINH_VIEN" && !string.IsNullOrEmpty(model.Mssv))
                {
                    var existMssv = await _context.SinhViens.AnyAsync(sv => sv.Mssv == model.Mssv);
                    if (existMssv)
                    {
                        TempData["ErrorMessage"] = "MSSV đã tồn tại trong hệ thống!";
                        return RedirectToAction(nameof(Index));
                    }
                }
                else if (model.LoaiNguoiDung == "GIANG_VIEN" && !string.IsNullOrEmpty(model.MaGv))
                {
                    var existMaGv = await _context.GiangViens.AnyAsync(gv => gv.MaGv == model.MaGv);
                    if (existMaGv)
                    {
                        TempData["ErrorMessage"] = "Mã GV đã tồn tại trong hệ thống!";
                        return RedirectToAction(nameof(Index));
                    }
                }

                // Tạo người dùng mới
                var nguoiDung = new NguoiDung
                {
                    HoTen = model.HoTen,
                    Email = model.Email,
                    Sdt = model.Sdt,
                    TrangThai = model.TrangThai,
                    // TODO: Xử lý mật khẩu theo giải pháp được chọn
                    MatKhau = null // Tạm thời để null
                };

                _context.NguoiDungs.Add(nguoiDung);
                await _context.SaveChangesAsync();

                // Thêm vai trò theo loại người dùng
                if (model.LoaiNguoiDung == "SINH_VIEN")
                {
                    // Sinh viên: Tự động gán vai trò "Sinh viên" (dựa vào MaVaiTro hoặc TenVaiTro)
                    var vaiTroSinhVien = await _context.VaiTros
                        .FirstOrDefaultAsync(v => v.MaVaiTro == "SINH_VIEN" || (v.TenVaiTro ?? "").ToLower().Contains("sinh viên"));
                    if (vaiTroSinhVien != null)
                    {
                        nguoiDung.IdVaiTros.Add(vaiTroSinhVien);
                    }
                }
                else if (model.LoaiNguoiDung == "GIANG_VIEN" && model.SelectedVaiTroId.HasValue)
                {
                    // Giảng viên: Gán vai trò được chọn (chỉ 1)
                    var selectedVaiTro = await _context.VaiTros.FindAsync(model.SelectedVaiTroId.Value);
                    if (selectedVaiTro != null)
                    {
                        nguoiDung.IdVaiTros.Add(selectedVaiTro);
                    }
                }

                // Tạo thông tin chi tiết theo loại - Sử dụng full namespace để tránh conflict
                if (model.LoaiNguoiDung == "SINH_VIEN")
                {
                    var newSinhVien = new DATN_TMS.Models.SinhVien
                    {
                        IdNguoiDung = nguoiDung.Id,
                        Mssv = model.Mssv,
                        IdChuyenNganh = model.IdChuyenNganh,
                        IdKhoaHoc = model.IdKhoaHoc,
                        TinChiTichLuy = model.TinChiTichLuy ?? 0
                    };
                    _context.SinhViens.Add(newSinhVien);
                }
                else if (model.LoaiNguoiDung == "GIANG_VIEN")
                {
                    var newGiangVien = new DATN_TMS.Models.GiangVien
                    {
                        IdNguoiDung = nguoiDung.Id,
                        MaGv = model.MaGv,
                        HocVi = model.HocVi,
                        IdBoMon = model.IdBoMon
                    };
                    _context.GiangViens.Add(newGiangVien);
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm người dùng thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Toggle Status - Bật/tắt trạng thái
        [HttpPost]
        public async Task<IActionResult> ToggleStatus([FromBody] ToggleStatusRequest? req)
        {
            try
            {
                if (req == null)
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ!" });

                var nguoiDung = await _context.NguoiDungs.FindAsync(req.UserId);
                if (nguoiDung == null)
                    return Json(new { success = false, message = "Không tìm thấy người dùng!" });

                nguoiDung.TrangThai = req.Status;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = req.Status == 1 ? "Đã kích hoạt tài khoản!" : "Đã vô hiệu hóa tài khoản!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // POST: Delete - Xóa người dùng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var nguoiDung = await _context.NguoiDungs
                    .Include(nd => nd.SinhVien)
                    .Include(nd => nd.GiangVien)
                    .Include(nd => nd.IdVaiTros)
                    .FirstOrDefaultAsync(nd => nd.Id == id);

                if (nguoiDung == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy người dùng!";
                    return RedirectToAction(nameof(Index));
                }

                // Kiểm tra ràng buộc dữ liệu
                if (nguoiDung.SinhVien != null)
                {
                    // Kiểm tra sinh viên có đề tài không
                    var hasDeTai = await _context.SinhVienDeTais.AnyAsync(svdt => svdt.IdSinhVien == nguoiDung.Id);
                    if (hasDeTai)
                    {
                        TempData["ErrorMessage"] = "Không thể xóa! Sinh viên này đã được gán đề tài.";
                        return RedirectToAction(nameof(Index));
                    }
                }

                if (nguoiDung.GiangVien != null)
                {
                    // Kiểm tra giảng viên có hướng dẫn đề tài không
                    var hasDeTaiHD = await _context.DeTais.AnyAsync(dt => dt.IdGvhd == nguoiDung.Id);
                    if (hasDeTaiHD)
                    {
                        TempData["ErrorMessage"] = "Không thể xóa! Giảng viên này đang hướng dẫn đề tài.";
                        return RedirectToAction(nameof(Index));
                    }

                    // Kiểm tra có trong hội đồng không
                    var inHoiDong = await _context.ThanhVienHdBaoCaos.AnyAsync(tv => tv.IdGiangVien == nguoiDung.Id);
                    if (inHoiDong)
                    {
                        TempData["ErrorMessage"] = "Không thể xóa! Giảng viên này đang là thành viên hội đồng.";
                        return RedirectToAction(nameof(Index));
                    }
                }

                // Xóa quan hệ vai trò
                nguoiDung.IdVaiTros.Clear();

                // Xóa sinh viên/giảng viên trước
                if (nguoiDung.SinhVien != null)
                    _context.SinhViens.Remove(nguoiDung.SinhVien);

                if (nguoiDung.GiangVien != null)
                    _context.GiangViens.Remove(nguoiDung.GiangVien);

                // Xóa người dùng
                _context.NguoiDungs.Remove(nguoiDung);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Xóa người dùng thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi xóa: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // API: Search users cho autocomplete
        [HttpGet]
        public async Task<IActionResult> SearchUsers(string? term)
        {
            if (string.IsNullOrEmpty(term) || term.Length < 2)
                return Json(new List<object>());

            var users = await _context.NguoiDungs
                .Include(nd => nd.SinhVien)
                .Include(nd => nd.GiangVien)
                .Where(nd =>
                    (nd.HoTen != null && nd.HoTen.Contains(term)) ||
                    nd.Email.Contains(term) ||
                    (nd.SinhVien != null && nd.SinhVien.Mssv != null && nd.SinhVien.Mssv.Contains(term)) ||
                    (nd.GiangVien != null && nd.GiangVien.MaGv != null && nd.GiangVien.MaGv.Contains(term))
                )
                .Take(10)
                .Select(nd => new
                {
                    id = nd.Id,
                    hoTen = nd.HoTen ?? "",
                    email = nd.Email,
                    maSo = nd.SinhVien != null ? nd.SinhVien.Mssv : (nd.GiangVien != null ? nd.GiangVien.MaGv : null),
                    loai = nd.SinhVien != null ? "SV" : (nd.GiangVien != null ? "GV" : "")
                })
                .ToListAsync();

            return Json(users);
        }

        // API: Lấy thông tin user để edit
        [HttpGet]
        public async Task<IActionResult> GetUserForEdit(int id)
        {
            var nguoiDung = await _context.NguoiDungs
                .Include(nd => nd.IdVaiTros)
                .Include(nd => nd.SinhVien)
                .Include(nd => nd.GiangVien)
                .FirstOrDefaultAsync(nd => nd.Id == id);

            if (nguoiDung == null)
                return Json(new { success = false, message = "Không tìm thấy người dùng!" });

            var result = new
            {
                success = true,
                user = new
                {
                    id = nguoiDung.Id,
                    hoTen = nguoiDung.HoTen,
                    email = nguoiDung.Email,
                    sdt = nguoiDung.Sdt,
                    trangThai = nguoiDung.TrangThai ?? 1,
                    isSinhVien = nguoiDung.SinhVien != null,
                    isGiangVien = nguoiDung.GiangVien != null,
                    // Sinh viên
                    mssv = nguoiDung.SinhVien?.Mssv,
                    idKhoaHoc = nguoiDung.SinhVien?.IdKhoaHoc,
                    idChuyenNganh = nguoiDung.SinhVien?.IdChuyenNganh,
                    // Giảng viên
                    maGv = nguoiDung.GiangVien?.MaGv,
                    hocVi = nguoiDung.GiangVien?.HocVi,
                    idBoMon = nguoiDung.GiangVien?.IdBoMon,
                    selectedVaiTroId = nguoiDung.IdVaiTros.FirstOrDefault()?.Id
                }
            };

            return Json(result);
        }

        // Helper: Load dropdown data
        private async Task LoadDropdownData()
        {
            ViewBag.ListVaiTro = await _context.VaiTros.ToListAsync();
            ViewBag.ListBoMon = new SelectList(await _context.BoMons.ToListAsync(), "Id", "TenBoMon");
            ViewBag.ListChuyenNganh = new SelectList(await _context.ChuyenNganhs.ToListAsync(), "Id", "TenChuyenNganh");
            ViewBag.ListKhoaHoc = new SelectList(await _context.KhoaHocs.ToListAsync(), "Id", "TenKhoa");
            ViewBag.ListHocVi = new SelectList(new List<string> { "Cử nhân", "Thạc sĩ", "Tiến sĩ", "Phó Giáo sư", "Giáo sư" });
        }
    }
}