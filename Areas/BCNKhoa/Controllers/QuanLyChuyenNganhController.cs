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
    public class QuanLyChuyenNganhController : Controller
    {
        private readonly QuanLyDoAnTotNghiepContext _context;

        public QuanLyChuyenNganhController(QuanLyDoAnTotNghiepContext context)
        {
            _context = context;
        }

        // GET: Index - Danh sách chuyên ngành
        public IActionResult Index(string searchString, int? page)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            ViewBag.CurrentFilter = searchString;

            // Load danh sách cho Dropdown trong Modal (Create/Edit)
            ViewBag.ListBoMon = new SelectList(_context.BoMons.OrderBy(b => b.TenBoMon), "Id", "TenBoMon");
            ViewBag.ListNganh = new SelectList(_context.Nganhs.OrderBy(n => n.TenNganh), "Id", "TenNganh");

            var query = from cn in _context.ChuyenNganhs
                        join nt in _context.NguoiDungs on cn.IdNguoiTao equals nt.Id into ntGroup
                        from nt in ntGroup.DefaultIfEmpty() // Left Join Người tạo

                        join ns in _context.NguoiDungs on cn.IdNguoiSua equals ns.Id into nsGroup
                        from ns in nsGroup.DefaultIfEmpty() // Left Join Người sửa

                        join n in _context.Nganhs on cn.IdNganh equals n.Id into nGroup
                        from n in nGroup.DefaultIfEmpty() // Left Join Ngành

                        join bm in _context.BoMons on cn.IdBoMon equals bm.Id into bmGroup
                        from bm in bmGroup.DefaultIfEmpty() // Left Join Bộ môn

                        select new { cn, nt, ns, n, bm };

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(x => x.cn.TenChuyenNganh.Contains(searchString) ||
                                         x.cn.TenVietTat.Contains(searchString));
            }

            // SẮP XẾP MỚI NHẤT LÊN ĐẦU
            query = query.OrderByDescending(x => x.cn.Id);

            // CHUYỂN ĐỔI SANG VIEW MODEL
            var modelQuery = query.Select(x => new ChuyenNganhViewModel
            {
                Id = x.cn.Id,
                Stt = x.cn.Stt,
                TenChuyenNganh = x.cn.TenChuyenNganh ?? "",
                TenVietTat = x.cn.TenVietTat ?? "",

                // Lấy tên từ bảng đã Join
                TenNganh = x.n != null ? x.n.TenNganh : "",
                TenBoMon = x.bm != null ? x.bm.TenBoMon : "",
                NguoiTao = x.nt != null ? x.nt.HoTen : "",
                NgayTao = x.cn.NgayTao,
                NguoiSua = x.ns != null ? x.ns.HoTen : "",
                NgaySua = x.cn.NgaySua
            });

            var pagedList = modelQuery.ToPagedList(pageNumber, pageSize);

            return View(pagedList);
        }

        // GET: ChiTiet - Lấy thông tin chi tiết (API cho modal sửa)
        [HttpGet]
        public async Task<IActionResult> ChiTiet(int id)
        {
            var chuyenNganh = await _context.ChuyenNganhs
                .Where(n => n.Id == id)
                .Select(n => new
                {
                    n.Id,
                    n.Stt,
                    n.TenChuyenNganh,
                    n.TenVietTat,
                    n.IdNganh,
                    n.IdBoMon
                })
                .FirstOrDefaultAsync();

            if (chuyenNganh == null)
            {
                return Json(new { success = false, message = "Không tìm thấy chuyên ngành." });
            }

            return Json(new { success = true, data = chuyenNganh });
        }

        // POST: Create - Thêm chuyên ngành mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int? Stt, string TenChuyenNganh, string TenVietTat, int? IdNganh, int? IdBoMon)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(TenChuyenNganh))
                {
                    TempData["ErrorMessage"] = "Vui lòng nhập tên chuyên ngành.";
                    return RedirectToAction("Index");
                }

                // Kiểm tra trùng mã (STT) nếu cần thiết
                if (Stt.HasValue)
                {
                    var exists = await _context.ChuyenNganhs.AnyAsync(x => x.Stt == Stt);
                    if (exists)
                    {
                        TempData["ErrorMessage"] = $"Mã chuyên ngành (STT) '{Stt}' đã tồn tại.";
                        return RedirectToAction("Index");
                    }
                }

                int? idNguoiTao = null;
                var userEmail = HttpContext.Session.GetString("UserEmail");
                if (!string.IsNullOrEmpty(userEmail))
                {
                    var nguoiDung = await _context.NguoiDungs.FirstOrDefaultAsync(nd => nd.Email == userEmail);
                    idNguoiTao = nguoiDung?.Id;
                }

                var chuyenNganh = new ChuyenNganh
                {
                    Stt = Stt,
                    TenChuyenNganh = TenChuyenNganh.Trim(),
                    TenVietTat = TenVietTat?.Trim(),
                    IdNganh = IdNganh,
                    IdBoMon = IdBoMon,
                    IdNguoiTao = idNguoiTao ?? 1,
                    NgayTao = DateOnly.FromDateTime(DateTime.Now)
                };

                _context.ChuyenNganhs.Add(chuyenNganh);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Thêm chuyên ngành thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // POST: Edit - Cập nhật chuyên ngành
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int Id, int? Stt, string TenChuyenNganh, string TenVietTat, int? IdNganh, int? IdBoMon)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(TenChuyenNganh))
                {
                    TempData["ErrorMessage"] = "Vui lòng nhập tên chuyên ngành.";
                    return RedirectToAction("Index");
                }

                var chuyenNganh = await _context.ChuyenNganhs.FindAsync(Id);
                if (chuyenNganh == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy chuyên ngành cần sửa.";
                    return RedirectToAction("Index");
                }

                // Kiểm tra trùng mã (STT) trừ bản ghi hiện tại
                if (Stt.HasValue)
                {
                    var exists = await _context.ChuyenNganhs.AnyAsync(x => x.Stt == Stt && x.Id != Id);
                    if (exists)
                    {
                        TempData["ErrorMessage"] = $"Mã chuyên ngành (STT) '{Stt}' đã tồn tại.";
                        return RedirectToAction("Index");
                    }
                }

                int? idNguoiSua = null;
                var userEmail = HttpContext.Session.GetString("UserEmail");
                if (!string.IsNullOrEmpty(userEmail))
                {
                    var nguoiDung = await _context.NguoiDungs.FirstOrDefaultAsync(nd => nd.Email == userEmail);
                    idNguoiSua = nguoiDung?.Id;
                }

                chuyenNganh.Stt = Stt;
                chuyenNganh.TenChuyenNganh = TenChuyenNganh.Trim();
                chuyenNganh.TenVietTat = TenVietTat?.Trim();
                chuyenNganh.IdNganh = IdNganh;
                chuyenNganh.IdBoMon = IdBoMon;
                chuyenNganh.IdNguoiSua = idNguoiSua ?? 1;
                chuyenNganh.NgaySua = DateOnly.FromDateTime(DateTime.Now);

                _context.ChuyenNganhs.Update(chuyenNganh);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Cập nhật chuyên ngành thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // POST: Delete - Xóa chuyên ngành
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var chuyenNganh = await _context.ChuyenNganhs
                    .Include(cn => cn.SinhViens)
                    .Include(cn => cn.DeTais)
                    .FirstOrDefaultAsync(n => n.Id == id);

                if (chuyenNganh == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy chuyên ngành cần xóa.";
                    return RedirectToAction("Index");
                }

                if (chuyenNganh.SinhViens.Any())
                {
                    TempData["ErrorMessage"] = $"Không thể xóa vì đang có {chuyenNganh.SinhViens.Count} sinh viên thuộc chuyên ngành này.";
                    return RedirectToAction("Index");
                }

                if (chuyenNganh.DeTais.Any())
                {
                    TempData["ErrorMessage"] = $"Không thể xóa vì đang có {chuyenNganh.DeTais.Count} đề tài thuộc chuyên ngành này.";
                    return RedirectToAction("Index");
                }

                _context.ChuyenNganhs.Remove(chuyenNganh);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Xóa chuyên ngành thành công!";
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