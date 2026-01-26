#nullable enable
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DATN_TMS.Areas.GV_BoMon.Models;
using DATN_TMS.Models;
using X.PagedList.Extensions;

namespace DATN_TMS.Areas.GV_BoMon.Controllers
{
    [Area("GV_BoMon")]
    public class QuanLyDangKyController : Controller
    {
        private readonly QuanLyDoAnTotNghiepContext _context;

        public QuanLyDangKyController(QuanLyDoAnTotNghiepContext context)
        {
            _context = context;
        }

        // GET: Danh sách đăng ký nguyện vọng
        public IActionResult Index(int? page, int? dotId, int? khoaId, string? chuyenNganhId, string? searchString)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            // Load Dropdown
            ViewBag.ListDot = new SelectList(_context.DotDoAns.OrderByDescending(d => d.Id), "Id", "TenDot", dotId);
            ViewBag.ListKhoa = new SelectList(_context.KhoaHocs.OrderByDescending(k => k.Id), "Id", "TenKhoa", khoaId);

            ViewBag.CurrentDotId = dotId;
            ViewBag.CurrentKhoaId = khoaId;
            ViewBag.CurrentChuyenNganh = chuyenNganhId;
            ViewBag.CurrentFilter = searchString;

            var query = _context.DangKyNguyenVongs
                .Include(dk => dk.IdSinhVienNavigation)
                    .ThenInclude(sv => sv != null ? sv.IdChuyenNganhNavigation : null)
                .Include(dk => dk.IdSinhVienNavigation)
                    .ThenInclude(sv => sv != null ? sv.IdKhoaHocNavigation : null)
                .Include(dk => dk.IdSinhVienNavigation)
                    .ThenInclude(sv => sv != null ? sv.IdNguoiDungNavigation : null)
                .AsQueryable();

            // Filter by search
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(dk =>
                    (dk.IdSinhVienNavigation != null && dk.IdSinhVienNavigation.Mssv != null && dk.IdSinhVienNavigation.Mssv.Contains(searchString)) ||
                    (dk.IdSinhVienNavigation != null && dk.IdSinhVienNavigation.IdNguoiDungNavigation != null && dk.IdSinhVienNavigation.IdNguoiDungNavigation.HoTen != null && dk.IdSinhVienNavigation.IdNguoiDungNavigation.HoTen.Contains(searchString))
                );
            }

            // Filter by Đợt
            if (dotId.HasValue)
            {
                query = query.Where(dk => dk.IdDot == dotId);
            }

            // Filter by Khóa
            if (khoaId.HasValue)
            {
                query = query.Where(dk => dk.IdSinhVienNavigation != null && dk.IdSinhVienNavigation.IdKhoaHoc == khoaId);
            }

            // Filter by Chuyên ngành
            if (!string.IsNullOrEmpty(chuyenNganhId))
            {
                query = query.Where(dk =>
                    dk.IdSinhVienNavigation != null &&
                    dk.IdSinhVienNavigation.IdChuyenNganhNavigation != null &&
                    dk.IdSinhVienNavigation.IdChuyenNganhNavigation.TenChuyenNganh != null &&
                    dk.IdSinhVienNavigation.IdChuyenNganhNavigation.TenChuyenNganh.Contains(chuyenNganhId)
                );
            }

            // Map to ViewModel
            var modelQuery = query.Select(dk => new QuanLyDangKyViewModel
            {
                Id = dk.Id,
                MaSinhVien = dk.IdSinhVienNavigation != null ? dk.IdSinhVienNavigation.Mssv : null,
                HoTen = dk.IdSinhVienNavigation != null && dk.IdSinhVienNavigation.IdNguoiDungNavigation != null
                    ? dk.IdSinhVienNavigation.IdNguoiDungNavigation.HoTen : null,
                IdSinhVien = dk.IdSinhVienNavigation != null ? dk.IdSinhVienNavigation.IdNguoiDung.ToString() : null,
                TenKhoa = dk.IdSinhVienNavigation != null && dk.IdSinhVienNavigation.IdKhoaHocNavigation != null
                    ? dk.IdSinhVienNavigation.IdKhoaHocNavigation.TenKhoa : null,
                TenChuyenNganh = dk.IdSinhVienNavigation != null && dk.IdSinhVienNavigation.IdChuyenNganhNavigation != null
                    ? dk.IdSinhVienNavigation.IdChuyenNganhNavigation.TenChuyenNganh : null,
                TinChiTichLuy = dk.IdSinhVienNavigation != null ? (int)(dk.IdSinhVienNavigation.TinChiTichLuy ?? 0) : 0,
                TrangThai = dk.TrangThai ?? 0
            });

            var pagedList = modelQuery.OrderBy(x => x.MaSinhVien).ToPagedList(pageNumber, pageSize);

            return View(pagedList);
        }

        // POST: Duyệt đăng ký
        [HttpPost]
        public async Task<IActionResult> DuyetDangKy(int id)
        {
            var dangKy = await _context.DangKyNguyenVongs.FindAsync(id);
            if (dangKy == null)
                return Json(new { success = false, message = "Không tìm thấy bản ghi!" });

            try
            {
                dangKy.TrangThai = 1;
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Đã duyệt sinh viên vào đợt đồ án!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // POST: Từ chối đăng ký
        [HttpPost]
        public async Task<IActionResult> TuChoiDangKy(int id)
        {
            var dangKy = await _context.DangKyNguyenVongs.FindAsync(id);
            if (dangKy == null)
                return Json(new { success = false, message = "Không tìm thấy bản ghi!" });

            try
            {
                dangKy.TrangThai = 2;
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Đã từ chối đăng ký!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // GET: Xem kết quả học tập
        public async Task<IActionResult> KetQuaHocTap(int id)
        {
            var sinhVien = await _context.SinhViens
                .Include(sv => sv.IdNguoiDungNavigation)
                .Include(sv => sv.IdChuyenNganhNavigation)
                .FirstOrDefaultAsync(sv => sv.IdNguoiDung == id);

            if (sinhVien == null)
                return NotFound();

            // Lấy danh sách bảng điểm
            var listDiem = await _context.KetQuaHocTaps
                .Where(kq => kq.IdSinhVien == id)
                .OrderBy(kq => kq.Stt)
                .ToListAsync();

            // Lấy thông tin tổng kết
            var lastRecord = listDiem.LastOrDefault();
            double currentGPA = lastRecord?.Gpa ?? 0;
            double currentTinChi = lastRecord?.TongSoTinChi ?? 0;

            var model = new KetQuaHocTapModel
            {
                HoTen = sinhVien.IdNguoiDungNavigation?.HoTen,
                MSSV = sinhVien.Mssv,
                ChuyenNganh = sinhVien.IdChuyenNganhNavigation?.TenChuyenNganh,
                TongTinChi = currentTinChi,
                GPA = currentGPA,
                BangDiem = listDiem.Select(d => new BangDiemItem
                {
                    Stt = d.Stt ?? 0,
                    MaHocPhan = d.MaHocPhan,
                    TenHocPhan = d.TenHocPhan,
                    SoTc = d.SoTc ?? 0,
                    DiemSo = d.DiemSo ?? 0,
                    DiemChu = d.DiemChu,
                    KetQua = d.KetQua ?? false
                }).ToList()
            };

            return View(model);
        }
    }
}