using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DATN_TMS.Models;
using DATN_TMS.Areas.BCNKhoa.Models;
using X.PagedList;
using X.PagedList.Extensions;

namespace DATN_TMS.Areas.BCNKhoa.Controllers
{
    [Area("BCNKhoa")]
    public class QuanLyNganhController : Controller
    {
        private readonly QuanLyDoAnTotNghiepContext _context;

        public QuanLyNganhController(QuanLyDoAnTotNghiepContext context)
        {
            _context = context;
        }

        // GET: Index - Danh sách ngành
        public IActionResult Index(string searchString, int? page)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            ViewBag.CurrentFilter = searchString;

            // Load danh sách bộ môn cho dropdown
            ViewBag.ListBoMon = new SelectList(_context.BoMons.OrderBy(b => b.TenBoMon), "Id", "TenBoMon");

            var query = _context.Nganhs.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(n => n.MaNganh.Contains(searchString) ||
                                         n.TenNganh.Contains(searchString));
            }

            query = query.OrderByDescending(n => n.Id);

            var modelQuery = query.Select(n => new NganhViewModel
            {
                Id = n.Id,
                MaNganh = n.MaNganh,
                TenNganh = n.TenNganh ?? "",
                TenBoMon = n.IdBoMonNavigation != null ? n.IdBoMonNavigation.TenBoMon : "",
                NguoiTao = n.IdNguoiTaoNavigation != null ? n.IdNguoiTaoNavigation.HoTen : "",
                NgayTao = n.NgayTao,
                NguoiSua = n.IdNguoiSuaNavigation != null ? n.IdNguoiSuaNavigation.HoTen : "",
                NgaySua = n.NgaySua
            });

            var pagedList = modelQuery.ToPagedList(pageNumber, pageSize);

            return View(pagedList);
        }

        // GET: ChiTiet - Lấy thông tin chi tiết ngành (API cho modal sửa)
        [HttpGet]
        public async Task<IActionResult> ChiTiet(int id)
        {
            var nganh = await _context.Nganhs
                .Where(n => n.Id == id)
                .Select(n => new
                {
                    n.Id,
                    n.MaNganh,
                    n.TenNganh,
                    n.TenVietTat,
                    n.IdBoMon
                })
                .FirstOrDefaultAsync();

            if (nganh == null)
            {
                return Json(new { success = false, message = "Không tìm thấy ngành." });
            }

            return Json(new { success = true, data = nganh });
        }

        // POST: Create - Thêm ngành mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string MaNganh, string TenNganh, string TenVietTat, int? IdBoMon)
        {
            try
            {
                // Kiểm tra dữ liệu
                if (string.IsNullOrWhiteSpace(MaNganh))
                {
                    TempData["ErrorMessage"] = "Vui lòng nhập mã ngành.";
                    return RedirectToAction("Index");
                }

                if (string.IsNullOrWhiteSpace(TenNganh))
                {
                    TempData["ErrorMessage"] = "Vui lòng nhập tên ngành.";
                    return RedirectToAction("Index");
                }

                // Kiểm tra mã ngành đã tồn tại
                var existingNganh = await _context.Nganhs
                    .AnyAsync(n => n.MaNganh.ToLower() == MaNganh.Trim().ToLower());

                if (existingNganh)
                {
                    TempData["ErrorMessage"] = "Mã ngành đã tồn tại trong hệ thống.";
                    return RedirectToAction("Index");
                }

                // Lấy IdNguoiTao từ session
                int? idNguoiTao = null;
                var userEmail = HttpContext.Session.GetString("UserEmail");
                if (!string.IsNullOrEmpty(userEmail))
                {
                    var nguoiDung = await _context.NguoiDungs
                        .FirstOrDefaultAsync(nd => nd.Email == userEmail);
                    idNguoiTao = nguoiDung?.Id;
                }

                var nganh = new Nganh
                {
                    MaNganh = MaNganh.Trim(),
                    TenNganh = TenNganh.Trim(),
                    TenVietTat = TenVietTat?.Trim(),
                    IdBoMon = IdBoMon,
                    IdNguoiTao = idNguoiTao ?? 1,
                    NgayTao = DateOnly.FromDateTime(DateTime.Now)
                };

                _context.Nganhs.Add(nganh);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Thêm ngành mới thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // POST: Edit - Cập nhật ngành
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int Id, string MaNganh, string TenNganh, string TenVietTat, int? IdBoMon)
        {
            try
            {
                // Kiểm tra dữ liệu
                if (string.IsNullOrWhiteSpace(MaNganh))
                {
                    TempData["ErrorMessage"] = "Vui lòng nhập mã ngành.";
                    return RedirectToAction("Index");
                }

                if (string.IsNullOrWhiteSpace(TenNganh))
                {
                    TempData["ErrorMessage"] = "Vui lòng nhập tên ngành.";
                    return RedirectToAction("Index");
                }

                var nganh = await _context.Nganhs.FindAsync(Id);

                if (nganh == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy ngành cần sửa.";
                    return RedirectToAction("Index");
                }

                // Kiểm tra mã ngành trùng (trừ bản ghi hiện tại)
                var existingNganh = await _context.Nganhs
                    .AnyAsync(n => n.MaNganh.ToLower() == MaNganh.Trim().ToLower() && n.Id != Id);

                if (existingNganh)
                {
                    TempData["ErrorMessage"] = "Mã ngành đã tồn tại trong hệ thống.";
                    return RedirectToAction("Index");
                }

                // Lấy IdNguoiSua từ session
                int? idNguoiSua = null;
                var userEmail = HttpContext.Session.GetString("UserEmail");
                if (!string.IsNullOrEmpty(userEmail))
                {
                    var nguoiDung = await _context.NguoiDungs
                        .FirstOrDefaultAsync(nd => nd.Email == userEmail);
                    idNguoiSua = nguoiDung?.Id;
                }

                nganh.MaNganh = MaNganh.Trim();
                nganh.TenNganh = TenNganh.Trim();
                nganh.TenVietTat = TenVietTat?.Trim();
                nganh.IdBoMon = IdBoMon;
                nganh.IdNguoiSua = idNguoiSua ?? 1;
                nganh.NgaySua = DateOnly.FromDateTime(DateTime.Now);

                _context.Nganhs.Update(nganh);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Cập nhật ngành thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // POST: Delete - Xóa ngành
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var nganh = await _context.Nganhs
                    .Include(n => n.ChuyenNganhs)
                    .Include(n => n.ChuongTrinhDaoTaos)
                    .FirstOrDefaultAsync(n => n.Id == id);

                if (nganh == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy ngành cần xóa.";
                    return RedirectToAction("Index");
                }

                // Kiểm tra ràng buộc
                if (nganh.ChuyenNganhs.Any())
                {
                    TempData["ErrorMessage"] = $"Không thể xóa ngành này vì đang có {nganh.ChuyenNganhs.Count} chuyên ngành liên kết.";
                    return RedirectToAction("Index");
                }

                if (nganh.ChuongTrinhDaoTaos.Any())
                {
                    TempData["ErrorMessage"] = $"Không thể xóa ngành này vì đang có {nganh.ChuongTrinhDaoTaos.Count} chương trình đào tạo liên kết.";
                    return RedirectToAction("Index");
                }

                _context.Nganhs.Remove(nganh);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Xóa ngành thành công!";
                return RedirectToAction("Index");
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] = "Không thể xóa ngành này vì đang được sử dụng trong hệ thống.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
                return RedirectToAction("Index");
            }
        }
    }
}