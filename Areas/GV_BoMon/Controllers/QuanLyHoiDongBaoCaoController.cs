using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using DATN_TMS.Areas.BCNKhoa.Models;
using DATN_TMS.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using X.PagedList.Extensions;

namespace DATN_TMS.Areas.GV_BoMon.Controllers
{
    [Area("GV_BoMon")]
    public class QuanLyHoiDongBaoCaoController : Controller
    {
        private readonly QuanLyDoAnTotNghiepContext _context;

        public QuanLyHoiDongBaoCaoController(QuanLyDoAnTotNghiepContext context)
        {
            _context = context;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var sessionRole = HttpContext.Session.GetString("Role");
            var isBoMonByClaim = User?.Identity?.IsAuthenticated == true && (User.IsInRole("BO_MON") || User.IsInRole("BCN_KHOA"));
            var isBoMonBySession = sessionRole == "BO_MON" || sessionRole == "BCN_KHOA";

            if (!isBoMonByClaim && !isBoMonBySession)
            {
                context.Result = RedirectToAction("Login", "Account", new { area = "" });
                return;
            }

            base.OnActionExecuting(context);
        }

        private async Task<int> GetCurrentUserId()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail)) return 1;

            var nguoiDung = await _context.NguoiDungs.FirstOrDefaultAsync(nd => nd.Email == userEmail);
            return nguoiDung?.Id ?? 1;
        }

        private static string GetLopKhoa(string? mssv)
        {
            if (string.IsNullOrEmpty(mssv) || mssv.Length < 2) return "N/A";
            return "K" + mssv.Substring(0, 2);
        }

        #region Index

        public async Task<IActionResult> Index(int? page, int? dotId, int? namHoc, string? searchString)
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
                .Include(hd => hd.IdDotNavigation)
                    .ThenInclude(d => d != null ? d.IdHocKiNavigation : null)
                .Where(hd => hd.LoaiHoiDong != "DUYET_DE_TAI")
                .AsQueryable();

            if (dotId.HasValue)
                query = query.Where(hd => hd.IdDot == dotId);
            if (namHoc.HasValue)
                query = query.Where(hd => hd.IdDotNavigation != null &&
                                         hd.IdDotNavigation.IdHocKiNavigation != null &&
                                         hd.IdDotNavigation.IdHocKiNavigation.NamBatDau == namHoc);
            if (!string.IsNullOrEmpty(searchString))
                query = query.Where(hd => (hd.TenHoiDong != null && hd.TenHoiDong.Contains(searchString)) ||
                                         (hd.MaHoiDong != null && hd.MaHoiDong.Contains(searchString)));

            var modelQuery = query.Select(hd => new QuanLyHoiDongBaoCaoViewModel
            {
                Id = hd.Id,
                MaHoiDong = hd.MaHoiDong ?? "",
                TenHoiDong = hd.TenHoiDong ?? "",
                LoaiHoiDong = hd.LoaiHoiDong ?? "",
                TenBoMon = hd.IdBoMonNavigation != null ? hd.IdBoMonNavigation.TenBoMon ?? "" : "",
                NguoiTao = hd.IdNguoiTaoNavigation != null ? hd.IdNguoiTaoNavigation.HoTen ?? "" : "",
                NgayBaoCao = hd.NgayBaoCao,
                ThoiGianDuKien = hd.ThoiGianDuKien,
                DiaDiem = hd.DiaDiem ?? "",
                TrangThai = hd.TrangThai ?? false
            });

            var pagedList = modelQuery.OrderByDescending(x => x.Id).ToPagedList(pageNumber, pageSize);
            return View(pagedList);
        }

        #endregion

        #region Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string? TenHoiDong, string? LoaiHoiDong, int? IdBoMon, int? IdDot,
                                                 string? NgayBaoCao, string? GioBatDau, string? DiaDiem)
        {
            try
            {
                DateOnly? ngayBaoCao = null;
                TimeOnly? gioBatDau = null;

                if (!string.IsNullOrEmpty(NgayBaoCao) && DateOnly.TryParse(NgayBaoCao, out DateOnly parsedDate))
                    ngayBaoCao = parsedDate;

                if (!string.IsNullOrEmpty(GioBatDau) && TimeOnly.TryParse(GioBatDau, out TimeOnly parsedTime))
                    gioBatDau = parsedTime;

                var hoidong = new HoiDongBaoCao
                {
                    MaHoiDong = "HDBV" + DateTime.Now.Ticks.ToString().Substring(10),
                    TenHoiDong = TenHoiDong ?? "",
                    IdBoMon = IdBoMon,
                    IdDot = IdDot,
                    LoaiHoiDong = LoaiHoiDong ?? "",
                    NgayBaoCao = ngayBaoCao,
                    ThoiGianDuKien = gioBatDau,
                    DiaDiem = DiaDiem ?? "",
                    IdNguoiTao = await GetCurrentUserId(),
                    TrangThai = false
                };

                _context.Add(hoidong);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm h?i ??ng báo cáo thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"L?i: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region Edit

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var hd = await _context.HoiDongBaoCaos
                .Include(h => h.ThanhVienHdBaoCaos)
                    .ThenInclude(tv => tv.IdGiangVienNavigation)
                        .ThenInclude(gv => gv != null ? gv.IdNguoiDungNavigation : null)
                .Include(h => h.PhienBaoVes)
                    .ThenInclude(pb => pb.IdSinhVienDeTaiNavigation)
                        .ThenInclude(svdt => svdt != null ? svdt.IdSinhVienNavigation : null)
                            .ThenInclude(sv => sv != null ? sv.IdNguoiDungNavigation : null)
                .Include(h => h.PhienBaoVes)
                    .ThenInclude(pb => pb.IdSinhVienDeTaiNavigation)
                        .ThenInclude(svdt => svdt != null ? svdt.IdDeTaiNavigation : null)
                            .ThenInclude(dt => dt != null ? dt.IdChuyenNganhNavigation : null)
                .Include(h => h.PhienBaoVes)
                    .ThenInclude(pb => pb.IdSinhVienDeTaiNavigation)
                        .ThenInclude(svdt => svdt != null ? svdt.IdDeTaiNavigation : null)
                            .ThenInclude(dt => dt != null ? dt.IdGvhdNavigation : null)
                                .ThenInclude(gv => gv != null ? gv.IdNguoiDungNavigation : null)
                .FirstOrDefaultAsync(h => h.Id == id);

            if (hd == null) return NotFound();

            var thanhViens = hd.ThanhVienHdBaoCaos.Select(tv => new ThanhVienHoiDongViewModel
            {
                IdNguoiDung = tv.IdGiangVien ?? 0,
                MaGV = tv.IdGiangVienNavigation?.MaGv ?? "",
                HocVi = tv.IdGiangVienNavigation?.HocVi ?? "",
                TenGiangVien = tv.IdGiangVienNavigation?.IdNguoiDungNavigation?.HoTen ?? "N/A",
                Email = tv.IdGiangVienNavigation?.IdNguoiDungNavigation?.Email ?? "",
                VaiTro = tv.VaiTro ?? ""
            }).ToList();

            var validPhienBaoVes = hd.PhienBaoVes
                .Where(pb => pb.IdSinhVienDeTaiNavigation?.IdDeTaiNavigation != null)
                .ToList();

            var groupDeTai = validPhienBaoVes
                .GroupBy(pb => pb.IdSinhVienDeTaiNavigation!.IdDeTaiNavigation!)
                .Select(g => new DeTaiHoiDongViewModel
                {
                    IdDeTai = g.Key.Id,
                    TenDeTai = g.Key.TenDeTai ?? "",
                    MaDeTai = g.Key.MaDeTai ?? "",
                    ChuyenNganh = g.Key.IdChuyenNganhNavigation?.TenVietTat ?? "",
                    GVHD = g.Key.IdGvhdNavigation?.IdNguoiDungNavigation?.HoTen ?? "",
                    SinhViens = g
                        .Where(pb => pb.IdSinhVienDeTaiNavigation?.IdSinhVienNavigation != null)
                        .Select(pb => new SinhVienBaoVeViewModel
                        {
                            IdPhienBaoVe = pb.Id,
                            MSSV = pb.IdSinhVienDeTaiNavigation!.IdSinhVienNavigation?.Mssv ?? "",
                            HoTen = pb.IdSinhVienDeTaiNavigation!.IdSinhVienNavigation?.IdNguoiDungNavigation?.HoTen ?? "N/A",
                            LopKhoa = GetLopKhoa(pb.IdSinhVienDeTaiNavigation!.IdSinhVienNavigation?.Mssv),
                            ChuyenNganh = g.Key.IdChuyenNganhNavigation?.TenVietTat ?? ""
                        }).ToList()
                }).ToList();

            var model = new QuanLyHoiDongBaoCaoEditViewModel
            {
                Id = hd.Id,
                MaHoiDong = hd.MaHoiDong ?? "",
                TenHoiDong = hd.TenHoiDong ?? "",
                LoaiHoiDong = hd.LoaiHoiDong ?? "",
                IdBoMon = hd.IdBoMon ?? 0,
                NgayBaoCao = hd.NgayBaoCao,
                ThoiGianDuKien = hd.ThoiGianDuKien,
                DiaDiem = hd.DiaDiem ?? "",
                TrangThai = hd.TrangThai ?? false,
                ThanhViens = thanhViens,
                DeTais = groupDeTai
            };

            ViewBag.ListBoMon = new SelectList(_context.BoMons.ToList(), "Id", "TenBoMon", model.IdBoMon);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int Id, string? TenHoiDong, int IdBoMon,
                                               string? NgayBaoCao, string? ThoiGianDuKien, string? DiaDiem)
        {
            try
            {
                var hd = await _context.HoiDongBaoCaos.FindAsync(Id);
                if (hd == null)
                {
                    TempData["ErrorMessage"] = "Không tìm th?y h?i ??ng!";
                    return RedirectToAction(nameof(Index));
                }

                hd.TenHoiDong = TenHoiDong ?? "";
                hd.IdBoMon = IdBoMon;
                hd.DiaDiem = DiaDiem ?? "";

                if (!string.IsNullOrEmpty(NgayBaoCao) && DateOnly.TryParse(NgayBaoCao, out DateOnly parsedDate))
                    hd.NgayBaoCao = parsedDate;
                else
                    hd.NgayBaoCao = null;

                if (!string.IsNullOrEmpty(ThoiGianDuKien) && TimeOnly.TryParse(ThoiGianDuKien, out TimeOnly parsedTime))
                    hd.ThoiGianDuKien = parsedTime;
                else
                    hd.ThoiGianDuKien = null;

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "?ã l?u thông tin h?i ??ng!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"L?i: {ex.Message}";
            }
            return RedirectToAction(nameof(Edit), new { id = Id });
        }

        #endregion

        #region API - Thành viên

        [HttpGet]
        public IActionResult SearchMembers(string? term)
        {
            if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
                return Json(new List<object>());

            var members = _context.GiangViens
                .Include(gv => gv.IdNguoiDungNavigation)
                .Where(gv => gv.IdNguoiDungNavigation != null &&
                            ((gv.IdNguoiDungNavigation.HoTen != null && gv.IdNguoiDungNavigation.HoTen.Contains(term)) ||
                             (gv.IdNguoiDungNavigation.Email != null && gv.IdNguoiDungNavigation.Email.Contains(term)) ||
                             (gv.MaGv != null && gv.MaGv.Contains(term))))
                .Select(gv => new
                {
                    id = gv.IdNguoiDung,
                    hoTen = gv.IdNguoiDungNavigation != null ? gv.IdNguoiDungNavigation.HoTen ?? "" : "",
                    email = gv.IdNguoiDungNavigation != null ? gv.IdNguoiDungNavigation.Email ?? "" : "",
                    maGV = gv.MaGv ?? "",
                    hocVi = gv.HocVi ?? ""
                })
                .Take(10)
                .ToList();

            return Json(members);
        }

        [HttpPost]
        public async Task<IActionResult> AddMember([FromBody] AddMemberBaoCaoRequest req)
        {
            try
            {
                if (req.CouncilId <= 0 || req.TeacherId <= 0)
                    return Json(new { success = false, message = "D? li?u không h?p l?!" });

                var council = await _context.HoiDongBaoCaos.FindAsync(req.CouncilId);
                if (council == null)
                    return Json(new { success = false, message = "H?i ??ng không t?n t?i!" });

                var gv = await _context.GiangViens
                    .Include(g => g.IdNguoiDungNavigation)
                    .FirstOrDefaultAsync(g => g.IdNguoiDung == req.TeacherId);

                if (gv == null)
                    return Json(new { success = false, message = "Không tìm th?y gi?ng viên!" });

                var exists = await _context.ThanhVienHdBaoCaos
                    .AnyAsync(tv => tv.IdHdBaocao == req.CouncilId && tv.IdGiangVien == req.TeacherId);
                if (exists)
                    return Json(new { success = false, message = "Gi?ng viên ?ã có trong h?i ??ng!" });

                if (req.Role == "CHU_TICH")
                {
                    var hasChairman = await _context.ThanhVienHdBaoCaos
                        .AnyAsync(tv => tv.IdHdBaocao == req.CouncilId && tv.VaiTro == "CHU_TICH");
                    if (hasChairman)
                        return Json(new { success = false, message = "H?i ??ng ?ã có Ch? t?ch!" });

                    var hocVi = (gv.HocVi ?? "").ToLower();
                    var validHocVi = new[] { "ti?n s?", "ts", "ts.", "phó giáo s?", "pgs", "pgs.", "giáo s?", "gs", "gs." };
                    var isValidHocVi = validHocVi.Any(v => hocVi.Contains(v));
                    if (!isValidHocVi)
                        return Json(new { success = false, message = "Ch? t?ch h?i ??ng ph?i là gi?ng viên có h?c v? Ti?n s? tr? lên!" });
                }

                if (req.Role == "THU_KY")
                {
                    var hasSecretary = await _context.ThanhVienHdBaoCaos
                        .AnyAsync(tv => tv.IdHdBaocao == req.CouncilId && tv.VaiTro == "THU_KY");
                    if (hasSecretary)
                        return Json(new { success = false, message = "H?i ??ng ?ã có Th? ký!" });
                }

                var thanhVien = new ThanhVienHdBaoCao
                {
                    IdHdBaocao = req.CouncilId,
                    IdGiangVien = req.TeacherId,
                    VaiTro = req.Role ?? "UY_VIEN"
                };

                _context.ThanhVienHdBaoCaos.Add(thanhVien);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Thêm thành viên thành công!",
                    data = new
                    {
                        idGiangVien = gv.IdNguoiDung,
                        maGV = gv.MaGv ?? "",
                        tenGiangVien = gv.IdNguoiDungNavigation?.HoTen ?? "N/A",
                        email = gv.IdNguoiDungNavigation?.Email ?? "",
                        hocVi = gv.HocVi ?? "",
                        vaiTro = req.Role ?? "UY_VIEN"
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"L?i: {ex.Message}" });
            }
        }

public class AddMemberBaoCaoRequest
{
    public int CouncilId { get; set; }
    public int TeacherId { get; set; }
    public string? Role { get; set; } = "UY_VIEN";
}

public class RemoveMemberBaoCaoRequest
{
    public int CouncilId { get; set; }
    public int TeacherId { get; set; }
}

public class AddTopicRequest
{
    public int CouncilId { get; set; }
    public int TopicId { get; set; }
}

public class RemoveTopicRequest
{
    public int CouncilId { get; set; }
    public int TopicId { get; set; }
}

        [HttpPost]
        public async Task<IActionResult> RemoveMember([FromBody] RemoveMemberBaoCaoRequest req)
        {
            try
            {
                var thanhVien = await _context.ThanhVienHdBaoCaos
                    .FirstOrDefaultAsync(tv => tv.IdHdBaocao == req.CouncilId && tv.IdGiangVien == req.TeacherId);

                if (thanhVien == null)
                    return Json(new { success = false, message = "Không tìm th?y thành viên." });

                _context.ThanhVienHdBaoCaos.Remove(thanhVien);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "?ã xóa thành viên kh?i h?i ??ng!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"L?i: {ex.Message}" });
            }
        }

        #endregion

        #region API - ?? tài

        [HttpGet]
        public IActionResult SearchTopics(string? term)
        {
            if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
                return Json(new List<object>());

            var topics = _context.DeTais
                .Include(dt => dt.IdGvhdNavigation)
                    .ThenInclude(gv => gv != null ? gv.IdNguoiDungNavigation : null)
                .Include(dt => dt.IdChuyenNganhNavigation)
                .Where(dt => ((dt.TenDeTai != null && dt.TenDeTai.Contains(term)) ||
                             (dt.MaDeTai != null && dt.MaDeTai.Contains(term)))
                          && dt.TrangThai == "DA_DUYET")
                .Select(dt => new
                {
                    id = dt.Id,
                    tenDeTai = dt.TenDeTai ?? "",
                    maDeTai = dt.MaDeTai ?? "",
                    gvhd = dt.IdGvhdNavigation != null && dt.IdGvhdNavigation.IdNguoiDungNavigation != null
                           ? dt.IdGvhdNavigation.IdNguoiDungNavigation.HoTen ?? "" : "N/A",
                    chuyenNganh = dt.IdChuyenNganhNavigation != null ? dt.IdChuyenNganhNavigation.TenVietTat ?? "" : ""
                })
                .Take(10)
                .ToList();

            return Json(topics);
        }

        [HttpPost]
        public async Task<IActionResult> AddTopic([FromBody] AddTopicRequest req)
        {
            try
            {
                if (req.CouncilId <= 0 || req.TopicId <= 0)
                    return Json(new { success = false, message = "D? li?u không h?p l?!" });

                var council = await _context.HoiDongBaoCaos.FindAsync(req.CouncilId);
                if (council == null)
                    return Json(new { success = false, message = "H?i ??ng không t?n t?i!" });

                var svDeTais = await _context.SinhVienDeTais
                    .Where(svdt => svdt.IdDeTai == req.TopicId)
                    .ToListAsync();

                if (!svDeTais.Any())
                    return Json(new { success = false, message = "?? tài này ch?a có sinh viên!" });

                var svDeTaiIds = svDeTais.Select(sv => sv.Id).ToList();
                var exists = await _context.PhienBaoVes
                    .AnyAsync(pb => pb.IdHdBaocao == req.CouncilId && svDeTaiIds.Contains(pb.IdSinhVienDeTai ?? 0));

                if (exists)
                    return Json(new { success = false, message = "?? tài này ?ã có trong h?i ??ng!" });

                int stt = await _context.PhienBaoVes.Where(p => p.IdHdBaocao == req.CouncilId).CountAsync() + 1;
                foreach (var svdt in svDeTais)
                {
                    var phien = new PhienBaoVe
                    {
                        IdHdBaocao = req.CouncilId,
                        IdSinhVienDeTai = svdt.Id,
                        SttBaoCao = stt++
                    };
                    _context.PhienBaoVes.Add(phien);
                }

                await _context.SaveChangesAsync();

                var deTai = await _context.DeTais
                    .Include(dt => dt.IdGvhdNavigation)
                        .ThenInclude(gv => gv != null ? gv.IdNguoiDungNavigation : null)
                    .Include(dt => dt.IdChuyenNganhNavigation)
                    .FirstOrDefaultAsync(dt => dt.Id == req.TopicId);

                return Json(new
                {
                    success = true,
                    message = "Thêm ?? tài thành công!",
                    data = new
                    {
                        idDeTai = deTai?.Id ?? 0,
                        tenDeTai = deTai?.TenDeTai ?? "",
                        maDeTai = deTai?.MaDeTai ?? "",
                        chuyenNganh = deTai?.IdChuyenNganhNavigation?.TenVietTat ?? "",
                        gvhd = deTai?.IdGvhdNavigation?.IdNguoiDungNavigation?.HoTen ?? ""
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"L?i: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveTopic([FromBody] RemoveTopicRequest req)
        {
            try
            {
                var phienBaoVes = await _context.PhienBaoVes
                    .Include(pb => pb.IdSinhVienDeTaiNavigation)
                    .Where(pb => pb.IdHdBaocao == req.CouncilId &&
                                pb.IdSinhVienDeTaiNavigation != null &&
                                pb.IdSinhVienDeTaiNavigation.IdDeTai == req.TopicId)
                    .ToListAsync();

                if (phienBaoVes.Any())
                {
                    _context.PhienBaoVes.RemoveRange(phienBaoVes);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "?ã xóa ?? tài kh?i h?i ??ng!" });
                }
                return Json(new { success = false, message = "Không tìm th?y ?? tài trong h?i ??ng." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"L?i: {ex.Message}" });
            }
        }

        #endregion

        #region Delete

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var hd = await _context.HoiDongBaoCaos
                    .Include(h => h.ThanhVienHdBaoCaos)
                    .Include(h => h.PhienBaoVes)
                    .FirstOrDefaultAsync(h => h.Id == id);

                if (hd == null)
                {
                    TempData["ErrorMessage"] = "Không tìm th?y h?i ??ng!";
                    return RedirectToAction(nameof(Index));
                }

                _context.ThanhVienHdBaoCaos.RemoveRange(hd.ThanhVienHdBaoCaos);
                _context.PhienBaoVes.RemoveRange(hd.PhienBaoVes);
                _context.HoiDongBaoCaos.Remove(hd);

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "?ã xóa h?i ??ng thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"L?i: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        #endregion
    }
}
