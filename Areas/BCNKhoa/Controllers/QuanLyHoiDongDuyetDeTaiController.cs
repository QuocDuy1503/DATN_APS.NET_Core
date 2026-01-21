using Microsoft.AspNetCore.Mvc;
using DATN_TMS.Areas.BCNKhoa.Models;
using DATN_TMS.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using X.PagedList.Extensions;

namespace DATN_TMS.Areas.BCNKhoa.Controllers
{
    [Area("BCNKhoa")]
    public class QuanLyHoiDongDuyetDeTaiController : Controller
    {
        private readonly QuanLyDoAnTotNghiepContext _context;

        public QuanLyHoiDongDuyetDeTaiController(QuanLyDoAnTotNghiepContext context)
        {
            _context = context;
        }

        // --- DANH SÁCH HỘI ĐỒNG ---
        public IActionResult Index(int? page, int? dotId, int? namHoc, string searchString)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            // 1. Load Dropdown cho Filter và Modal
            ViewBag.ListDot = new SelectList(_context.DotDoAns.OrderByDescending(d => d.Id), "Id", "TenDot", dotId);

            var listNamHoc = _context.HocKis
                .Select(h => new { h.NamBatDau, TenNam = $"{h.NamBatDau}-{h.NamKetThuc}" })
                .Distinct()
                .OrderByDescending(n => n.NamBatDau)
                .ToList();
            ViewBag.ListNamHoc = new SelectList(listNamHoc, "NamBatDau", "TenNam", namHoc);

            // Dữ liệu cho Modal Thêm mới (Dropdown Bộ môn)
            ViewBag.ListBoMon = new SelectList(_context.BoMons.ToList(), "Id", "TenBoMon");

            ViewBag.CurrentDotId = dotId;
            ViewBag.CurrentNamHoc = namHoc;
            ViewBag.CurrentFilter = searchString;

            // 2. Query dữ liệu từ SQL
            var query = _context.HoiDongBaoCaos
                .Include(hd => hd.IdBoMonNavigation)
                .Include(hd => hd.IdNguoiTaoNavigation)
                .Include(hd => hd.IdDotNavigation)
                .ThenInclude(d => d.IdHocKiNavigation)
                .Where(hd => hd.LoaiHoiDong == "DUYET_DE_TAI" || hd.LoaiHoiDong == "CUOI_KY" || hd.LoaiHoiDong == null)
                .AsQueryable();

            // 3. Áp dụng bộ lọc
            if (dotId.HasValue)
            {
                query = query.Where(hd => hd.IdDot == dotId);
            }
            if (namHoc.HasValue)
            {
                query = query.Where(hd => hd.IdDotNavigation.IdHocKiNavigation.NamBatDau == namHoc);
            }
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(hd => hd.MaHoiDong.Contains(searchString) || hd.TenHoiDong.Contains(searchString));
            }

            // 4. Map sang ViewModel (QUAN TRỌNG: Map vào NgayBatDau/NgayKetThuc)
            var modelQuery = query.Select(hd => new QuanLyHoiDongViewModel
            {
                Id = hd.Id,
                MaHoiDong = hd.MaHoiDong,
                TenHoiDong = hd.TenHoiDong,
                TenBoMon = hd.IdBoMonNavigation != null ? hd.IdBoMonNavigation.TenBoMon : "",
                NguoiTao = hd.IdNguoiTaoNavigation != null ? hd.IdNguoiTaoNavigation.HoTen : "",
                IdBoMon = hd.IdBoMon ?? 0,


                NgayBatDau = hd.NgayBatDau.HasValue
                             ? hd.NgayBatDau.Value.ToDateTime(TimeOnly.MinValue)
                             : (hd.NgayBaoCao.HasValue ? hd.NgayBaoCao.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null),

                NgayKetThuc = hd.NgayKetThuc.HasValue
                              ? hd.NgayKetThuc.Value.ToDateTime(TimeOnly.MinValue)
                              : (DateTime?)null
            });


            var pagedList = modelQuery.OrderByDescending(x => x.Id).ToPagedList(pageNumber, pageSize);

            return View(pagedList);
        }

        // --- TẠO MỚI HỘI ĐỒNG ---
        [HttpPost]
        public async Task<IActionResult> Create(QuanLyHoiDongViewModel model)
        {
            if (ModelState.IsValid)
            {
                var hoidong = new HoiDongBaoCao
                {
                    MaHoiDong = model.MaHoiDong,
                    TenHoiDong = model.TenHoiDong,
                    IdBoMon = model.IdBoMon,
                    NgayBatDau = model.NgayBatDau.HasValue ? DateOnly.FromDateTime(model.NgayBatDau.Value) : null,
                    NgayKetThuc = model.NgayKetThuc.HasValue ? DateOnly.FromDateTime(model.NgayKetThuc.Value) : null,
                    NgayBaoCao = model.NgayBatDau.HasValue ? DateOnly.FromDateTime(model.NgayBatDau.Value) : null,
                    IdNguoiTao = 1,
                };

                _context.Add(hoidong);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));
        }

        //[HttpGet]
        //public async Task<IActionResult> Edit(int id)
        //{
        //    var hd = await _context.HoiDongBaoCaos
        //        .Include(h => h.ThanhVienHdBaoCaos)
        //            .ThenInclude(tv => tv.IdGiangVienNavigation)
        //                .ThenInclude(gv => gv.IdNguoiDungNavigation)
        //        .FirstOrDefaultAsync(h => h.Id == id);

        //    if (hd == null) return NotFound();

            //var model = new QuanLyHoiDongEditViewModel
            //{
            //    Id = hd.Id,
            //    MaHoiDong = hd.MaHoiDong,
            //    TenHoiDong = hd.TenHoiDong,
            //    IdBoMon = hd.IdBoMon ?? 0,
            //    NgayBatDau = hd.NgayBatDau,
            //    NgayKetThuc = hd.NgayKetThuc,
            //    TrangThai = hd.TrangThai ?? false,

            //    // Map dữ liệu cẩn thận hơn
            //    ThanhViens = hd.ThanhVienHdBaoCaos.Select(tv => new ThanhVienHoiDongViewModel
            //    {
            //        IdGiangVien = tv.IdGiangVien ?? 0,
            //        // Lấy từ NguoiDungNavigation
            //        TenGiangVien = tv.IdGiangVienNavigation?.IdNguoiDungNavigation?.HoTen ?? "Chưa cập nhật",
            //        Email = tv.IdGiangVienNavigation?.IdNguoiDungNavigation?.Email ?? "",
            //        MaGV = tv.IdGiangVienNavigation?.MaGv ?? "",
            //        VaiTro = tv.VaiTro
            //    }).ToList()
            //};

        //    ViewBag.ListBoMon = new SelectList(_context.BoMons.ToList(), "Id", "TenBoMon", model.IdBoMon);
        //    return View(model);
        //}

        // 2. POST: Lưu thay đổi thông tin chung
        //[HttpPost]
        //public async Task<IActionResult> Edit(QuanLyHoiDongEditViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var hd = await _context.HoiDongBaoCaos.FindAsync(model.Id);
        //        if (hd == null) return NotFound();

        //        hd.TenHoiDong = model.TenHoiDong;
        //        hd.IdBoMon = model.IdBoMon;
        //        hd.NgayBatDau = model.NgayBatDau;
        //        hd.NgayKetThuc = model.NgayKetThuc;

        //        await _context.SaveChangesAsync();

        //        // Gửi thông báo thành công sang View
        //        TempData["Success"] = "Đã lưu thông tin hội đồng thành công!";
        //        return RedirectToAction(nameof(Edit), new { id = model.Id });
        //    }

        //    TempData["Error"] = "Vui lòng kiểm tra lại thông tin nhập vào!";
        //    ViewBag.ListBoMon = new SelectList(_context.BoMons.ToList(), "Id", "TenBoMon", model.IdBoMon);
        //    return View(model);
        //}


        //// 3. API: Tìm kiếm Giảng viên (Dùng cho Javascript)
        //[HttpGet]
        //public IActionResult SearchGiangVien(string term)
        //{
        //    var data = _context.GiangViens
        //        .Include(gv => gv.IdNguoiDungNavigation)
        //        .Where(gv => gv.IdNguoiDungNavigation.HoTen.Contains(term) || gv.MaGv.Contains(term))
        //        .Select(gv => new
        //        {
        //            id = gv.IdNguoiDung,
        //            label = $"{gv.IdNguoiDungNavigation.HoTen} ({gv.MaGv})",
        //            email = gv.IdNguoiDungNavigation.Email,
        //            maGv = gv.MaGv,
        //            hoTen = gv.IdNguoiDungNavigation.HoTen
        //        })
        //        .Take(5)
        //        .ToList();
        //    return Json(data);
        //}

        // 4. API: Thêm thành viên vào hội đồng (Lưu ngay lập tức)
        //[HttpPost]
        //public async Task<IActionResult> AddMember([FromBody] ThanhVienHoiDongViewModel req, int councilId)
        //{
        //    try
        //    {
        //        //var exists = await _context.ThanhVienHdBaoCaos
        //            //.AnyAsync(x => x.IdHdBaocao == councilId && x.IdGiangVien == req.IdGiangVien);
        //        //if (exists) return Json(new { success = false, message = "Giảng viên này đã có trong hội đồng!" });

        //        // Logic kiểm tra Chủ tịch
        //        if (req.VaiTro == "CHU_TICH")
        //        {
        //            var hasPresident = await _context.ThanhVienHdBaoCaos
        //                .AnyAsync(x => x.IdHdBaocao == councilId && x.VaiTro == "CHU_TICH");
        //            if (hasPresident) return Json(new { success = false, message = "Hội đồng này đã có Chủ tịch rồi!" });

        //            //var giangVien = await _context.GiangViens.FindAsync(req.IdGiangVien);
        //            string hocVi = giangVien?.HocVi?.ToLower() ?? "";
        //            bool duDieuKien = hocVi.Contains("tiến sĩ") || hocVi.Contains("phó giáo sư") || hocVi.Contains("giáo sư");
        //            if (!duDieuKien) return Json(new { success = false, message = $"Giảng viên này là '{giangVien.HocVi}'. Chủ tịch phải là Tiến sĩ!" });
        //        }

        //        var tv = new ThanhVienHdBaoCao
        //        {
        //            IdHdBaocao = councilId,
        //            IdGiangVien = req.IdGiangVien,
        //            VaiTro = req.VaiTro
        //        };
        //        _context.Add(tv);
        //        await _context.SaveChangesAsync();
        //        return Json(new { success = true });
        //    }
        //    catch (Exception ex)
        //    {
        //        // Đệ quy lấy thông báo lỗi gốc từ SQL
        //        var msg = ex.Message;
        //        var inner = ex.InnerException;
        //        while (inner != null)
        //        {
        //            msg = inner.Message;
        //            inner = inner.InnerException;
        //        }
        //        return Json(new { success = false, message = "Lỗi SQL: " + msg });
        //    }
        //}
    }
}