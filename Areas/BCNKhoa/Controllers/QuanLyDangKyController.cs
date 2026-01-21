using Microsoft.AspNetCore.Mvc;
using DATN_TMS.Areas.BCNKhoa.Models;
using DATN_TMS.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using X.PagedList.Extensions;

namespace DATN_TMS.Areas.BCNKhoa.Controllers
{
    [Area("BCNKhoa")]
    public class QuanLyDangKyController : Controller
    {
        private readonly QuanLyDoAnTotNghiepContext _context;

        public QuanLyDangKyController(QuanLyDoAnTotNghiepContext context)
        {
            _context = context;
        }

        // GET: Danh sách đăng ký
        public IActionResult Index(int? page, int? dotId, int? namHocId, int? khoaId, string chuyenNganhId, string searchString)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            // Load Dropdown 
            ViewBag.ListDot = new SelectList(_context.DotDoAns.OrderByDescending(d => d.Id), "Id", "TenDot", dotId);
            ViewBag.ListKhoa = new SelectList(_context.KhoaHocs.OrderByDescending(k => k.Id), "Id", "TenKhoa", khoaId);

            ViewBag.CurrentDotId = dotId;
            ViewBag.CurrentKhoaId = khoaId;
            ViewBag.CurrentChuyenNganh = chuyenNganhId;

            var query = _context.DangKyNguyenVongs
                .Include(dk => dk.IdSinhVienNavigation)
                    .ThenInclude(sv => sv.IdChuyenNganhNavigation) 
                .Include(dk => dk.IdSinhVienNavigation)
                    .ThenInclude(sv => sv.IdKhoaHocNavigation)    
                .Include(dk => dk.IdSinhVienNavigation)
                    .ThenInclude(sv => sv.IdNguoiDungNavigation)  
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(dk => dk.IdSinhVienNavigation.Mssv.Contains(searchString)
                                       || dk.IdSinhVienNavigation.IdNguoiDungNavigation.HoTen.Contains(searchString));
            }

            if (dotId.HasValue)
            {
                query = query.Where(dk => dk.IdDot == dotId);
            }

            if (khoaId.HasValue)
            {
                query = query.Where(dk => dk.IdSinhVienNavigation.IdKhoaHoc == khoaId);
            }

            if (!string.IsNullOrEmpty(chuyenNganhId))
            {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                query = query.Where(dk => dk.IdSinhVienNavigation.IdChuyenNganhNavigation.TenChuyenNganh.Contains(chuyenNganhId));
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            }

            var modelQuery = query.Select(dk => new QuanLyDangky
            {
                Id = dk.Id,
                MaSinhVien = dk.IdSinhVienNavigation.Mssv,

                HoTen = dk.IdSinhVienNavigation.IdNguoiDungNavigation.HoTen,

                IdSinhVien = dk.IdSinhVienNavigation.IdNguoiDung.ToString(), 
                TenKhoa = dk.IdSinhVienNavigation.IdKhoaHocNavigation.TenKhoa,
                TenChuyenNganh = dk.IdSinhVienNavigation.IdChuyenNganhNavigation.TenChuyenNganh,
                TinChiTichLuy = (int)(dk.IdSinhVienNavigation.TinChiTichLuy ?? 0),
                TrangThai = dk.TrangThai ?? 0
            });

            var pagedList = modelQuery.OrderBy(x => x.MaSinhVien).ToPagedList(pageNumber, pageSize);


            return View(pagedList);
        }

        [HttpPost]
        public async Task<IActionResult> DuyetDangKy(int id)
        {
            var dangKy = await _context.DangKyNguyenVongs.FindAsync(id);
            if (dangKy == null) return Json(new { success = false, message = "Không tìm thấy bản ghi!" });

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

        [HttpPost]
        public async Task<IActionResult> TuChoiDangKy(int id)
        {
            var dangKy = await _context.DangKyNguyenVongs.FindAsync(id);
            if (dangKy == null) return Json(new { success = false, message = "Không tìm thấy bản ghi!" });

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

        // Action Xem chi tiết kết quả học tập
        public async Task<IActionResult> KetQuaHocTap(int id)
        {
            var sinhVien = await _context.SinhViens
                .Include(sv => sv.IdNguoiDungNavigation)
                .Include(sv => sv.IdChuyenNganhNavigation)
                .FirstOrDefaultAsync(sv => sv.IdNguoiDung == id);

            if (sinhVien == null) return NotFound();

            // Lấy danh sách bảng điểm
            var listDiem = await _context.KetQuaHocTaps
                .Where(kq => kq.IdSinhVien == id)
                .OrderBy(kq => kq.Stt)
                .ToListAsync();

            // Lấy thông tin tổng kết
            var lastRecord = listDiem.LastOrDefault();
            double currentGPA = lastRecord?.Gpa ?? 0;
            double currentTinChi = lastRecord?.TongSoTinChi ?? 0;

            // Map sang Model MỚI (KetQuaHocTapModel)
            var model = new KetQuaHocTapModel
            {
                HoTen = sinhVien.IdNguoiDungNavigation.HoTen,
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