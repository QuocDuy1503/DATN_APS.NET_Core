using Microsoft.AspNetCore.Mvc;
using DATN_TMS.Areas.BCNKhoa.Models;
using DATN_TMS.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using X.PagedList.Extensions;
using System.Globalization;

namespace DATN_TMS.Areas.BCNKhoa.Controllers
{
    [Area("BCNKhoa")]
    public class QuanLyHoiDongBaoCaoController : Controller
    {
        private readonly QuanLyDoAnTotNghiepContext _context;
        private object model;

        public QuanLyHoiDongBaoCaoController(QuanLyDoAnTotNghiepContext context)
        {
            _context = context;
        }

        public IActionResult Index(int? page, int? dotId, int? namHoc, string searchString)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            ViewBag.ListBoMon = new SelectList(_context.BoMons.ToList(), "Id", "TenBoMon");
            ViewBag.ListDot = new SelectList(_context.DotDoAns.OrderByDescending(d => d.Id), "Id", "TenDot", dotId);

            var listNamHoc = _context.HocKis
                .Select(h => new { h.NamBatDau, TenNam = $"{h.NamBatDau}-{h.NamKetThuc}" })
                .Distinct().OrderByDescending(n => n.NamBatDau).ToList();
            ViewBag.ListNamHoc = new SelectList(listNamHoc, "NamBatDau", "TenNam", namHoc);

            ViewBag.CurrentDotId = dotId;
            ViewBag.CurrentNamHoc = namHoc;
            ViewBag.CurrentFilter = searchString;

            var query = _context.HoiDongBaoCaos
                .Include(hd => hd.IdBoMonNavigation)
                .Include(hd => hd.IdNguoiTaoNavigation)
                .Where(hd => hd.LoaiHoiDong != "DUYET_DE_TAI")
                .AsQueryable();

            if (dotId.HasValue) query = query.Where(hd => hd.IdDot == dotId);
            if (!string.IsNullOrEmpty(searchString))
                query = query.Where(hd => hd.TenHoiDong.Contains(searchString) || hd.MaHoiDong.Contains(searchString));

            var modelQuery = query.Select(hd => new QuanLyHoiDongBaoCaoViewModel
            {
                Id = hd.Id,
                MaHoiDong = hd.MaHoiDong,
                TenHoiDong = hd.TenHoiDong,
                LoaiHoiDong = hd.LoaiHoiDong,
                TenBoMon = hd.IdBoMonNavigation != null ? hd.IdBoMonNavigation.TenBoMon : "",
                NguoiTao = hd.IdNguoiTaoNavigation != null ? hd.IdNguoiTaoNavigation.HoTen : "",
                NgayBaoCao = hd.NgayBaoCao,
                GioBatDau = hd.ThoiGianDuKien,
                DiaDiem = hd.DiaDiem,
                TrangThai = hd.TrangThai ?? false
            });

            var pagedList = modelQuery.OrderByDescending(x => x.Id).ToPagedList(pageNumber, pageSize);
            return View(pagedList);
        }

        [HttpPost]
        public async Task<IActionResult> Create(string TenHoiDong, string LoaiHoiDong, int? IdBoMon,
                                                 string NgayBaoCao, string GioBatDau, string DiaDiem)
        {
            try
            {
                DateOnly? ngayBaoCao = null;
                TimeOnly? gioBatDau = null;

                if (!string.IsNullOrEmpty(NgayBaoCao))
                {
                    if (DateOnly.TryParse(NgayBaoCao, out DateOnly parsedDate))
                    {
                        ngayBaoCao = parsedDate;
                    }
                }

                if (!string.IsNullOrEmpty(GioBatDau))
                {
                    if (TimeOnly.TryParse(GioBatDau, out TimeOnly parsedTime))
                    {
                        gioBatDau = parsedTime;
                    }
                }

                var hoidong = new HoiDongBaoCao
                {
                    MaHoiDong = "HDBV" + DateTime.Now.Ticks.ToString().Substring(10),
                    TenHoiDong = TenHoiDong,
                    IdBoMon = IdBoMon,
                    LoaiHoiDong = LoaiHoiDong,
                    NgayBaoCao = ngayBaoCao,
                    ThoiGianDuKien = gioBatDau,
                    DiaDiem = DiaDiem,
                    IdNguoiTao = 1, // Giả định ID người tạo
                    TrangThai = false
                };

                _context.Add(hoidong);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Thêm hội đồng báo cáo thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        //[HttpGet]
        //public async Task<IActionResult> Edit(int id)
        //{
        //    var hd = await _context.HoiDongBaoCaos
        //        .Include(h => h.ThanhVienHdBaoCaos)
        //            .ThenInclude(tv => tv.IdGiangVienNavigation)
        //                .ThenInclude(gv => gv.IdNguoiDungNavigation)
        //        .Include(h => h.PhienBaoVes)
        //            .ThenInclude(pb => pb.IdSinhVienDeTaiNavigation)
        //                .ThenInclude(svdt => svdt.IdSinhVienNavigation)
        //                    .ThenInclude(sv => sv.IdNguoiDungNavigation)
        //        .Include(h => h.PhienBaoVes)
        //            .ThenInclude(pb => pb.IdSinhVienDeTaiNavigation)
        //                .ThenInclude(svdt => svdt.IdDeTaiNavigation)
        //                    .ThenInclude(dt => dt.IdChuyenNganhNavigation)
        //        .Include(h => h.PhienBaoVes)
        //            .ThenInclude(pb => pb.IdSinhVienDeTaiNavigation)
        //                .ThenInclude(svdt => svdt.IdDeTaiNavigation)
        //                    .ThenInclude(dt => dt.IdGvhdNavigation)
        //                        .ThenInclude(gv => gv.IdNguoiDungNavigation)
        //        .FirstOrDefaultAsync(h => h.Id == id);

        //    if (hd == null) return NotFound();

        //    //var model = new QuanLyHoiDongBaoCaoEditViewModel
        //    //{
        //    //    Id = hd.Id,
        //    //    MaHoiDong = hd.MaHoiDong,
        //    //    TenHoiDong = hd.TenHoiDong,
        //    //    LoaiHoiDong = hd.LoaiHoiDong,
        //    //    IdBoMon = hd.IdBoMon ?? 0,
        //    //    NgayBaoCao = hd.NgayBaoCao,
        //    //    ThoiGianDuKien = hd.ThoiGianDuKien,
        //    //    DiaDiem = hd.DiaDiem,
        //    //    TrangThai = hd.TrangThai ?? false,

               
        //    //     ThanhViens = hd.ThanhVienHdBaoCaos
        //    //    .Include(tv => tv.IdGiangVienNavigation)
        //    //    .ThenInclude(gv => gv.IdNguoiDungNavigation)

        //    //    .Select(tv => new ThanhVienHoiDongViewModel
        //    //    {
        //    //        IdNguoiDung = tv.IdGiangVien ?? 0,
        //    //        MaGV = tv.IdGiangVienNavigation.MaGv,
        //    //        HocVi = tv.IdGiangVienNavigation.HocVi,
        //    //        TenGiangVien = tv.IdGiangVienNavigation.IdNguoiDungNavigation.HoTen,
        //    //        Email = tv.IdGiangVienNavigation.IdNguoiDungNavigation.Email,

        //    //        VaiTro = tv.VaiTro
        //    //    }).ToList()
        //    //};

        //    var validPhienBaoVes = hd.PhienBaoVes
        //        .Where(pb => pb.IdSinhVienDeTaiNavigation != null
        //                  && pb.IdSinhVienDeTaiNavigation.IdDeTaiNavigation != null)
        //        .ToList();

        //    var groupDeTai = validPhienBaoVes
        //        .GroupBy(pb => pb.IdSinhVienDeTaiNavigation.IdDeTaiNavigation)
        //        .Select(g => new DeTaiHoiDongViewModel
        //        {
        //            IdDeTai = g.Key.Id,
        //            TenDeTai = g.Key.TenDeTai ?? "",
        //            MaDeTai = g.Key.MaDeTai ?? "",
        //            ChuyenNganh = g.Key.IdChuyenNganhNavigation?.TenVietTat ?? "",
        //            GVHD = g.Key.IdGvhdNavigation?.IdNguoiDungNavigation?.HoTen ?? "",
        //            SinhViens = g
        //                .Where(pb => pb.IdSinhVienDeTaiNavigation?.IdSinhVienNavigation != null)
        //                .Select(pb => new SinhVienBaoVeViewModel
        //                {
        //                    IdPhienBaoVe = pb.Id,
        //                    MSSV = pb.IdSinhVienDeTaiNavigation.IdSinhVienNavigation?.Mssv ?? "",
        //                    HoTen = pb.IdSinhVienDeTaiNavigation.IdSinhVienNavigation?.IdNguoiDungNavigation?.HoTen ?? "N/A",
        //                    LopKhoa = GetLopKhoa(pb.IdSinhVienDeTaiNavigation.IdSinhVienNavigation?.Mssv),
        //                    ChuyenNganh = g.Key.IdChuyenNganhNavigation?.TenVietTat ?? ""
        //                }).ToList()
        //        }).ToList();

        //    //model.DeTais = groupDeTai;
        //    //ViewBag.ListBoMon = new SelectList(_context.BoMons.ToList(), "Id", "TenBoMon", model.IdBoMon);
        //    //return View(model);
        //}

        private string GetLopKhoa(string? mssv)
        {
            if (string.IsNullOrEmpty(mssv) || mssv.Length < 2)
                return "N/A";
            return "K" + mssv.Substring(0, 2);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int Id, string TenHoiDong, int IdBoMon,
                                               string NgayBaoCao, string ThoiGianDuKien, string DiaDiem)
        {
            try
            {
                var hd = await _context.HoiDongBaoCaos.FindAsync(Id);
                if (hd != null)
                {
                    hd.TenHoiDong = TenHoiDong;
                    hd.IdBoMon = IdBoMon;

                    if (!string.IsNullOrEmpty(NgayBaoCao))
                    {
                        if (DateOnly.TryParse(NgayBaoCao, out DateOnly parsedDate))
                        {
                            hd.NgayBaoCao = parsedDate;
                        }
                    }
                    else
                    {
                        hd.NgayBaoCao = null;
                    }

                    if (!string.IsNullOrEmpty(ThoiGianDuKien))
                    {
                        if (TimeOnly.TryParse(ThoiGianDuKien, out TimeOnly parsedTime))
                        {
                            hd.ThoiGianDuKien = parsedTime;
                        }
                    }
                    else
                    {
                        hd.ThoiGianDuKien = null;
                    }

                    hd.DiaDiem = DiaDiem;
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Đã lưu thông tin hội đồng!";
                }
                else
                {
                    TempData["Error"] = "Không tìm thấy hội đồng!";
                }
                return RedirectToAction(nameof(Edit), new { id = Id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction(nameof(Edit), new { id = Id });
            }
        }

        [HttpGet]
        public IActionResult SearchMembers(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Json(new List<object>());

            var members = _context.GiangViens
                .Include(gv => gv.IdNguoiDungNavigation)
                .Where(gv => gv.IdNguoiDungNavigation != null &&
                            (gv.IdNguoiDungNavigation.HoTen.Contains(term)
                          || gv.IdNguoiDungNavigation.Email.Contains(term)
                          || gv.MaGv.Contains(term)))
                .Select(gv => new
                {
                    id = gv.IdNguoiDung,
                    label = gv.IdNguoiDungNavigation.HoTen,
                    email = gv.IdNguoiDungNavigation.Email ?? "",
                    maGV = gv.MaGv ?? "",
                    hocVi = gv.HocVi ?? ""
                })
                .Take(10)
                .ToList();

            return Json(members);
        }

        [HttpPost]
        public async Task<IActionResult> AddMember(int councilId, int teacherId, string role)
        {
            try
            {
                // Validate input
                if (councilId <= 0 || teacherId <= 0)
                {
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ!" });
                }

                // Kiểm tra hội đồng tồn tại
                var council = await _context.HoiDongBaoCaos.FindAsync(councilId);
                if (council == null)
                {
                    return Json(new { success = false, message = "Hội đồng không tồn tại!" });
                }

                // Kiểm tra xem giảng viên đã có trong hội đồng chưa
                var exists = await _context.ThanhVienHdBaoCaos
                    .AnyAsync(tv => tv.IdHdBaocao == councilId && tv.IdGiangVien == teacherId);

                if (exists)
                {
                    return Json(new { success = false, message = "Giảng viên đã có trong hội đồng!" });
                }

                // Kiểm tra vai trò chủ tịch - mỗi hội đồng chỉ có 1 chủ tịch
                if (role == "CHU_TICH")
                {
                    var existingChairman = await _context.ThanhVienHdBaoCaos
                        .AnyAsync(tv => tv.IdHdBaocao == councilId && tv.VaiTro == "CHU_TICH");
                    if (existingChairman)
                    {
                        return Json(new { success = false, message = "Hội đồng đã có Chủ tịch!" });
                    }
                }

                // Kiểm tra vai trò thư ký - mỗi hội đồng chỉ có 1 thư ký
                if (role == "THU_KY")
                {
                    var existingSecretary = await _context.ThanhVienHdBaoCaos
                        .AnyAsync(tv => tv.IdHdBaocao == councilId && tv.VaiTro == "THU_KY");
                    if (existingSecretary)
                    {
                        return Json(new { success = false, message = "Hội đồng đã có Thư ký!" });
                    }
                }

                var thanhVien = new ThanhVienHdBaoCao
                {
                    IdHdBaocao = councilId,
                    IdGiangVien = teacherId,
                    VaiTro = role ?? "UY_VIEN"
                };

                _context.ThanhVienHdBaoCaos.Add(thanhVien);
                await _context.SaveChangesAsync();

                // FIX: Lấy thông tin giảng viên bằng IdNguoiDung
                var gv = await _context.GiangViens
                    .Include(g => g.IdNguoiDungNavigation)
                    .FirstOrDefaultAsync(g => g.IdNguoiDung == teacherId);

                if (gv == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin giảng viên!" });
                }

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        // [FIXED] Trả về key idGiangVien để khớp với JS bên View
                        idGiangVien = gv.IdNguoiDung,
                        maGV = gv.MaGv ?? "",
                        tenGiangVien = gv.IdNguoiDungNavigation?.HoTen ?? "N/A",
                        email = gv.IdNguoiDungNavigation?.Email ?? "",
                        hocVi = gv.HocVi ?? "",
                        vaiTro = role
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi hệ thống: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveMember(int councilId, int teacherId)
        {
            try
            {
                var thanhVien = await _context.ThanhVienHdBaoCaos
                    .FirstOrDefaultAsync(tv => tv.IdHdBaocao == councilId && tv.IdGiangVien == teacherId);

                if (thanhVien != null)
                {
                    _context.ThanhVienHdBaoCaos.Remove(thanhVien);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Đã xóa thành viên khỏi hội đồng!" });
                }
                return Json(new { success = false, message = "Không tìm thấy thành viên." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        [HttpGet]
        public IActionResult SearchTopics(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Json(new List<object>());

            var topics = _context.DeTais
                .Include(dt => dt.IdGvhdNavigation).ThenInclude(gv => gv.IdNguoiDungNavigation)
                .Include(dt => dt.IdChuyenNganhNavigation)
                .Where(dt => dt.TenDeTai.Contains(term) || dt.MaDeTai.Contains(term))
                .Select(dt => new
                {
                    id = dt.Id,
                    label = dt.TenDeTai ?? "",
                    maDeTai = dt.MaDeTai ?? "",
                    gvhd = dt.IdGvhdNavigation != null && dt.IdGvhdNavigation.IdNguoiDungNavigation != null
                           ? dt.IdGvhdNavigation.IdNguoiDungNavigation.HoTen : "N/A",
                    chuyenNganh = dt.IdChuyenNganhNavigation != null ? dt.IdChuyenNganhNavigation.TenVietTat : ""
                })
                .Take(10)
                .ToList();

            return Json(topics);
        }

        [HttpPost]
        public async Task<IActionResult> AddTopic(int councilId, int topicId)
        {
            try
            {
                if (councilId <= 0 || topicId <= 0)
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ!" });

                var council = await _context.HoiDongBaoCaos.FindAsync(councilId);
                if (council == null)
                    return Json(new { success = false, message = "Hội đồng không tồn tại!" });

                var svDeTais = await _context.SinhVienDeTais
                    .Where(svdt => svdt.IdDeTai == topicId)
                    .ToListAsync();

                if (!svDeTais.Any())
                    return Json(new { success = false, message = "Đề tài này chưa có sinh viên!" });

                var svDeTaiIds = svDeTais.Select(sv => sv.Id).ToList();
                var exists = await _context.PhienBaoVes
                    .AnyAsync(pb => pb.IdHdBaocao == councilId && svDeTaiIds.Contains(pb.IdSinhVienDeTai ?? 0));

                if (exists)
                    return Json(new { success = false, message = "Đề tài này đã có trong hội đồng!" });

                int stt = 1;
                foreach (var svdt in svDeTais)
                {
                    var phien = new PhienBaoVe
                    {
                        IdHdBaocao = councilId,
                        IdSinhVienDeTai = svdt.Id,
                        SttBaoCao = stt++
                    };
                    _context.PhienBaoVes.Add(phien);
                }

                await _context.SaveChangesAsync();

                var deTai = await _context.DeTais
                    .Include(dt => dt.IdGvhdNavigation).ThenInclude(gv => gv.IdNguoiDungNavigation)
                    .Include(dt => dt.IdChuyenNganhNavigation)
                    .FirstOrDefaultAsync(dt => dt.Id == topicId);

                var sinhViens = await _context.SinhVienDeTais
                    .Include(svdt => svdt.IdSinhVienNavigation).ThenInclude(sv => sv.IdNguoiDungNavigation)
                    .Where(svdt => svdt.IdDeTai == topicId)
                    .Select(svdt => new
                    {
                        mssv = svdt.IdSinhVienNavigation != null ? svdt.IdSinhVienNavigation.Mssv : "",
                        hoTen = svdt.IdSinhVienNavigation != null && svdt.IdSinhVienNavigation.IdNguoiDungNavigation != null
                                ? svdt.IdSinhVienNavigation.IdNguoiDungNavigation.HoTen : "N/A",
                        lopKhoa = svdt.IdSinhVienNavigation != null && !string.IsNullOrEmpty(svdt.IdSinhVienNavigation.Mssv) && svdt.IdSinhVienNavigation.Mssv.Length >= 2
                                  ? "K" + svdt.IdSinhVienNavigation.Mssv.Substring(0, 2) : "N/A",
                        chuyenNganh = deTai.IdChuyenNganhNavigation != null ? deTai.IdChuyenNganhNavigation.TenVietTat : ""
                    })
                    .ToListAsync();

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        idDeTai = deTai.Id,
                        tenDeTai = deTai.TenDeTai ?? "",
                        maDeTai = deTai.MaDeTai ?? "",
                        chuyenNganh = deTai.IdChuyenNganhNavigation?.TenVietTat ?? "",
                        gvhd = deTai.IdGvhdNavigation?.IdNguoiDungNavigation?.HoTen ?? "",
                        sinhViens = sinhViens
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveTopic(int councilId, int topicId)
        {
            try
            {
                var phienBaoVes = await _context.PhienBaoVes
                    .Include(pb => pb.IdSinhVienDeTaiNavigation)
                    .Where(pb => pb.IdHdBaocao == councilId && pb.IdSinhVienDeTaiNavigation.IdDeTai == topicId)
                    .ToListAsync();

                if (phienBaoVes.Any())
                {
                    _context.PhienBaoVes.RemoveRange(phienBaoVes);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Đã xóa đề tài khỏi hội đồng!" });
                }
                return Json(new { success = false, message = "Không tìm thấy đề tài trong hội đồng." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ApproveCouncil(int councilId)
        {
            try
            {
                var hd = await _context.HoiDongBaoCaos
                    .Include(h => h.ThanhVienHdBaoCaos)
                    .Include(h => h.PhienBaoVes)
                    .FirstOrDefaultAsync(h => h.Id == councilId);

                if (hd == null) return Json(new { success = false, message = "Không tìm thấy hội đồng." });

                if (!hd.ThanhVienHdBaoCaos.Any()) return Json(new { success = false, message = "Hội đồng chưa có thành viên nào!" });

                var hasChairman = hd.ThanhVienHdBaoCaos.Any(tv => tv.VaiTro == "CHU_TICH");
                if (!hasChairman) return Json(new { success = false, message = "Hội đồng phải có Chủ tịch!" });

                var hasSecretary = hd.ThanhVienHdBaoCaos.Any(tv => tv.VaiTro == "THU_KY");
                if (!hasSecretary) return Json(new { success = false, message = "Hội đồng phải có Thư ký!" });

                if (!hd.PhienBaoVes.Any()) return Json(new { success = false, message = "Hội đồng chưa có đề tài nào!" });

                hd.TrangThai = true;
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Đã duyệt hội đồng thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UnapproveCouncil(int councilId)
        {
            try
            {
                var hd = await _context.HoiDongBaoCaos.FindAsync(councilId);
                if (hd != null)
                {
                    hd.TrangThai = false;
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Đã hủy duyệt hội đồng!" });
                }
                return Json(new { success = false, message = "Không tìm thấy hội đồng." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var hd = await _context.HoiDongBaoCaos
                    .Include(h => h.ThanhVienHdBaoCaos)
                    .Include(h => h.PhienBaoVes)
                    .FirstOrDefaultAsync(h => h.Id == id);

                if (hd == null) return Json(new { success = false, message = "Không tìm thấy hội đồng." });

                if (hd.TrangThai == true) return Json(new { success = false, message = "Không thể xóa hội đồng đã được duyệt!" });

                _context.ThanhVienHdBaoCaos.RemoveRange(hd.ThanhVienHdBaoCaos);
                _context.PhienBaoVes.RemoveRange(hd.PhienBaoVes);
                _context.HoiDongBaoCaos.Remove(hd);

                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa hội đồng thành công!";
                return Json(new { success = true, message = "Đã xóa hội đồng thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }
    }
}