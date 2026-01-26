using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DATN_TMS.Models;
using DATN_TMS.Areas.BCNKhoa.Models; // Nhớ sửa namespace cho đúng project của bạn
using X.PagedList;
using X.PagedList.Extensions; // Dùng cho bản X.PagedList mới

namespace DATN_TMS.Areas.BCNKhoa.Controllers
{
    [Area("BCNKhoa")]
    public class QuanLyHocKiController : Controller
    {
        private readonly QuanLyDoAnTotNghiepContext _context;

        public QuanLyHocKiController(QuanLyDoAnTotNghiepContext context)
        {
            _context = context;
        }

        // GET: Index
        public IActionResult Index(string searchString, int? page)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            ViewBag.CurrentFilter = searchString;

            var query = _context.HocKis.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                // Tìm theo Mã học kì hoặc Năm bắt đầu/Kết thúc
                query = query.Where(h => h.MaHocKi.Contains(searchString) ||
                                         h.NamBatDau.ToString().Contains(searchString) ||
                                         h.NamKetThuc.ToString().Contains(searchString));
            }

            // Sắp xếp: Năm bắt đầu giảm dần, sau đó đến Mã học kì
            query = query.OrderByDescending(h => h.NamBatDau).ThenByDescending(h => h.MaHocKi);

            var modelQuery = query.Select(h => new HocKiViewModel
            {
                Id = h.Id,
                MaHocKi = h.MaHocKi,
                NamBatDau = h.NamBatDau,
                NamKetThuc = h.NamKetThuc,
                TuanBatDau = h.TuanBatDau,
                NgayBatDau = h.NgayBatDau,
                TrangThai = h.TrangThai
            });

            var pagedList = modelQuery.ToPagedList(pageNumber, pageSize);
            return View(pagedList);
        }

        // GET: ChiTiet (API cho modal sửa)
        [HttpGet]
        public async Task<IActionResult> ChiTiet(int id)
        {
            var hocki = await _context.HocKis.FindAsync(id);
            if (hocki == null) return Json(new { success = false, message = "Không tìm thấy học kì." });
            return Json(new { success = true, data = hocki });
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string MaHocKi, int? NamBatDau, int? NamKetThuc, int? TuanBatDau, DateOnly? NgayBatDau)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(MaHocKi))
                {
                    TempData["ErrorMessage"] = "Vui lòng nhập tên/mã học kì.";
                    return RedirectToAction("Index");
                }

                // Kiểm tra trùng: Cùng Mã học kì và Cùng Năm bắt đầu coi như trùng
                var exists = await _context.HocKis.AnyAsync(h => h.MaHocKi == MaHocKi && h.NamBatDau == NamBatDau);
                if (exists)
                {
                    TempData["ErrorMessage"] = $"Học kì {MaHocKi} năm {NamBatDau} đã tồn tại.";
                    return RedirectToAction("Index");
                }

                var hk = new HocKi
                {
                    MaHocKi = MaHocKi.Trim(),
                    NamBatDau = NamBatDau,
                    NamKetThuc = NamKetThuc,
                    TuanBatDau = TuanBatDau,
                    NgayBatDau = NgayBatDau,
                    TrangThai = true // Mặc định mới tạo là đang kích hoạt
                };

                _context.HocKis.Add(hk);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Thêm học kì thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int Id, string MaHocKi, int? NamBatDau, int? NamKetThuc, int? TuanBatDau, DateOnly? NgayBatDau, bool TrangThai)
        {
            try
            {
                var hk = await _context.HocKis.FindAsync(Id);
                if (hk == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy học kì.";
                    return RedirectToAction("Index");
                }

                // Kiểm tra trùng (trừ bản ghi hiện tại)
                var exists = await _context.HocKis.AnyAsync(h => h.MaHocKi == MaHocKi && h.NamBatDau == NamBatDau && h.Id != Id);
                if (exists)
                {
                    TempData["ErrorMessage"] = "Thông tin học kì bị trùng với dữ liệu đã có.";
                    return RedirectToAction("Index");
                }

                hk.MaHocKi = MaHocKi.Trim();
                hk.NamBatDau = NamBatDau;
                hk.NamKetThuc = NamKetThuc;
                hk.TuanBatDau = TuanBatDau;
                hk.NgayBatDau = NgayBatDau;
                hk.TrangThai = TrangThai;

                _context.HocKis.Update(hk);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Cập nhật học kì thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // POST: Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var hk = await _context.HocKis.Include(h => h.DotDoAns).FirstOrDefaultAsync(h => h.Id == id);
                if (hk == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy dữ liệu.";
                    return RedirectToAction("Index");
                }

                // Kiểm tra ràng buộc dữ liệu (Nếu đã có đợt đồ án thì không cho xóa)
                if (hk.DotDoAns.Any())
                {
                    TempData["ErrorMessage"] = $"Không thể xóa học kì này vì đã có {hk.DotDoAns.Count} đợt đồ án liên kết.";
                    return RedirectToAction("Index");
                }

                _context.HocKis.Remove(hk);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Xóa học kì thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi hệ thống: " + ex.Message;
                return RedirectToAction("Index");
            }
        }
    }
}