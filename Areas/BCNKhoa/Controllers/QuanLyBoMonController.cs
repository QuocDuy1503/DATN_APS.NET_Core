using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DATN_TMS.Models;
using DATN_TMS.Areas.BCNKhoa.Models;
using X.PagedList;
using X.PagedList.Extensions;

namespace DATN_TMS.Areas.BCNKhoa.Controllers
{
    [Area("BCNKhoa")]
    public class QuanLyBoMonController : Controller
    {
        private readonly QuanLyDoAnTotNghiepContext _context;

        public QuanLyBoMonController(QuanLyDoAnTotNghiepContext context)
        {
            _context = context;
        }

        // GET: Index - Danh sách bộ môn
        public IActionResult Index(int? page, string searchString)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            ViewBag.CurrentFilter = searchString;

            var query = _context.BoMons.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(b => b.TenBoMon.Contains(searchString)
                                      || b.TenVietTat.Contains(searchString));
            }

            query = query.OrderByDescending(b => b.Id);

            var modelQuery = query.Select(b => new BoMonViewModel
            {
                Id = b.Id,
                Stt = b.Stt,
                TenBoMon = b.TenBoMon ?? "",
                TenVietTat = b.TenVietTat ?? "",
                NguoiTao = b.IdNguoiTaoNavigation != null ? b.IdNguoiTaoNavigation.HoTen : "",
                NgayTao = b.NgayTao,
                NguoiSua = b.IdNguoiSuaNavigation != null ? b.IdNguoiSuaNavigation.HoTen : "",
                NgaySua = b.NgaySua
            });

            var pagedList = modelQuery.ToPagedList(pageNumber, pageSize);

            return View(pagedList);
        }

        // GET: ChiTiet - Lấy thông tin chi tiết bộ môn (API cho modal sửa)
        [HttpGet]
        public async Task<IActionResult> ChiTiet(int id)
        {
            var boMon = await _context.BoMons
                .Where(b => b.Id == id)
                .Select(b => new
                {
                    b.Id,
                    b.TenBoMon,
                    b.TenVietTat
                })
                .FirstOrDefaultAsync();

            if (boMon == null)
            {
                return Json(new { success = false, message = "Không tìm thấy bộ môn." });
            }

            return Json(new { success = true, data = boMon });
        }

        // POST: Create - Thêm bộ môn mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string MaBoMon, string TenBoMon)
        {
            try
            {
                // Kiểm tra dữ liệu
                if (string.IsNullOrWhiteSpace(MaBoMon))
                {
                    TempData["ErrorMessage"] = "Vui lòng nhập mã bộ môn.";
                    return RedirectToAction("Index");
                }

                if (string.IsNullOrWhiteSpace(TenBoMon))
                {
                    TempData["ErrorMessage"] = "Vui lòng nhập tên bộ môn.";
                    return RedirectToAction("Index");
                }

                // Kiểm tra mã bộ môn đã tồn tại
                var existingMa = await _context.BoMons
                    .AnyAsync(b => b.TenVietTat.ToLower() == MaBoMon.Trim().ToLower());

                if (existingMa)
                {
                    TempData["ErrorMessage"] = "Mã bộ môn đã tồn tại trong hệ thống.";
                    return RedirectToAction("Index");
                }

                // Kiểm tra tên bộ môn đã tồn tại
                var existingTen = await _context.BoMons
                    .AnyAsync(b => b.TenBoMon.ToLower() == TenBoMon.Trim().ToLower());

                if (existingTen)
                {
                    TempData["ErrorMessage"] = "Tên bộ môn đã tồn tại trong hệ thống.";
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

                var boMon = new BoMon
                {
                    TenVietTat = MaBoMon.Trim(),
                    TenBoMon = TenBoMon.Trim(),
                    IdNguoiTao = idNguoiTao ?? 1,
                    NgayTao = DateOnly.FromDateTime(DateTime.Now)
                };

                _context.BoMons.Add(boMon);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Thêm bộ môn mới thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // POST: Edit - Cập nhật bộ môn
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int Id, string MaBoMon, string TenBoMon)
        {
            try
            {
                // Kiểm tra dữ liệu
                if (string.IsNullOrWhiteSpace(MaBoMon))
                {
                    TempData["ErrorMessage"] = "Vui lòng nhập mã bộ môn.";
                    return RedirectToAction("Index");
                }

                if (string.IsNullOrWhiteSpace(TenBoMon))
                {
                    TempData["ErrorMessage"] = "Vui lòng nhập tên bộ môn.";
                    return RedirectToAction("Index");
                }

                var boMon = await _context.BoMons.FindAsync(Id);

                if (boMon == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy bộ môn cần sửa.";
                    return RedirectToAction("Index");
                }

                // Kiểm tra mã bộ môn trùng (trừ bản ghi hiện tại)
                var existingMa = await _context.BoMons
                    .AnyAsync(b => b.TenVietTat.ToLower() == MaBoMon.Trim().ToLower() && b.Id != Id);

                if (existingMa)
                {
                    TempData["ErrorMessage"] = "Mã bộ môn đã tồn tại trong hệ thống.";
                    return RedirectToAction("Index");
                }

                // Kiểm tra tên bộ môn trùng (trừ bản ghi hiện tại)
                var existingTen = await _context.BoMons
                    .AnyAsync(b => b.TenBoMon.ToLower() == TenBoMon.Trim().ToLower() && b.Id != Id);

                if (existingTen)
                {
                    TempData["ErrorMessage"] = "Tên bộ môn đã tồn tại trong hệ thống.";
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

                boMon.TenVietTat = MaBoMon.Trim();
                boMon.TenBoMon = TenBoMon.Trim();
                boMon.IdNguoiSua = idNguoiSua ?? 1;
                boMon.NgaySua = DateOnly.FromDateTime(DateTime.Now);

                _context.BoMons.Update(boMon);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Cập nhật bộ môn thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // POST: Delete - Xóa bộ môn
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var boMon = await _context.BoMons
                    .Include(b => b.Nganhs)
                    .Include(b => b.ChuyenNganhs)
                    .Include(b => b.GiangViens)
                    .Include(b => b.HoiDongBaoCaos)
                    .FirstOrDefaultAsync(b => b.Id == id);

                if (boMon == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy bộ môn cần xóa.";
                    return RedirectToAction("Index");
                }

                // Kiểm tra ràng buộc
                if (boMon.Nganhs.Any())
                {
                    TempData["ErrorMessage"] = $"Không thể xóa bộ môn này vì đang có {boMon.Nganhs.Count} ngành liên kết.";
                    return RedirectToAction("Index");
                }

                if (boMon.ChuyenNganhs.Any())
                {
                    TempData["ErrorMessage"] = $"Không thể xóa bộ môn này vì đang có {boMon.ChuyenNganhs.Count} chuyên ngành liên kết.";
                    return RedirectToAction("Index");
                }

                if (boMon.GiangViens.Any())
                {
                    TempData["ErrorMessage"] = $"Không thể xóa bộ môn này vì đang có {boMon.GiangViens.Count} giảng viên thuộc bộ môn.";
                    return RedirectToAction("Index");
                }

                if (boMon.HoiDongBaoCaos.Any())
                {
                    TempData["ErrorMessage"] = $"Không thể xóa bộ môn này vì đang có {boMon.HoiDongBaoCaos.Count} hội đồng báo cáo liên kết.";
                    return RedirectToAction("Index");
                }

                _context.BoMons.Remove(boMon);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Xóa bộ môn thành công!";
                return RedirectToAction("Index");
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] = "Không thể xóa bộ môn này vì đang được sử dụng trong hệ thống.";
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