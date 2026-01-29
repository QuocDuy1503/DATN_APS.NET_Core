using DATN_TMS.Areas.BCNKhoa.Models;
using DATN_TMS.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DATN_TMS.Areas.BCNKhoa.Controllers
{
    [Area("BCNKhoa")]
    public class QuanLyTieuChiController : Controller
    {
        private readonly QuanLyDoAnTotNghiepContext _context;

        public QuanLyTieuChiController(QuanLyDoAnTotNghiepContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? loaiPhieuId, int? dotId)
        {
            var dotOptions = await _context.DotDoAns
                .OrderByDescending(d => d.Id)
                .Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = d.TenDot ?? $"Đợt {d.Id}"
                })
                .ToListAsync();

            var loaiPhieuQuery = _context.LoaiPhieuChams.AsQueryable();
            if (dotId.HasValue)
            {
                var loaiIdsTheoDot = await _context.CauHinhPhieuChamDots
                    .Where(x => x.IdDot == dotId.Value && x.IdLoaiPhieu.HasValue)
                    .Select(x => x.IdLoaiPhieu!.Value)
                    .Distinct()
                    .ToListAsync();

                if (loaiIdsTheoDot.Any())
                {
                    loaiPhieuQuery = loaiPhieuQuery.Where(lp => loaiIdsTheoDot.Contains(lp.Id));
                }
            }

            var loaiPhieuOptions = await loaiPhieuQuery
                .OrderBy(lp => lp.TenLoaiPhieu)
                .Select(lp => new SelectListItem
                {
                    Value = lp.Id.ToString(),
                    Text = lp.TenLoaiPhieu ?? $"Loại {lp.Id}"
                })
                .ToListAsync();

            if (!loaiPhieuId.HasValue && loaiPhieuOptions.Any())
            {
                loaiPhieuId = int.TryParse(loaiPhieuOptions.First().Value, out var firstId) ? firstId : null;
            }

            var tieuChis = await _context.TieuChiChamDiems
                .AsNoTracking()
                .Where(tc => !loaiPhieuId.HasValue || tc.IdLoaiPhieu == loaiPhieuId)
                .OrderBy(tc => tc.SttHienThi ?? tc.Id)
                .Select(tc => new TieuChiItem
                {
                    Id = tc.Id,
                    Stt = tc.SttHienThi,
                    TenTieuChi = tc.TenTieuChi,
                    MoTa = tc.MoTaHuongDan,
                    TrongSo = tc.TrongSo,
                    DiemToiDa = tc.DiemToiDa
                })
                .ToListAsync();

            var vm = new TieuChiViewModel
            {
                SelectedDotId = dotId,
                SelectedLoaiPhieuId = loaiPhieuId,
                LoaiPhieuOptions = loaiPhieuOptions,
                DotOptions = dotOptions,
                TieuChis = tieuChis
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAll(List<TieuChiItem> items, int loaiPhieuId, int? dotId)
        {
            if (loaiPhieuId == 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn loại phiếu.";
                return RedirectToAction(nameof(Index), new { loaiPhieuId, dotId });
            }

            var list = items ?? new List<TieuChiItem>();
            var ids = list.Where(x => x.Id > 0).Select(x => x.Id).ToList();

            var existing = await _context.TieuChiChamDiems
                .Where(x => x.IdLoaiPhieu == loaiPhieuId && ids.Contains(x.Id))
                .ToListAsync();

            foreach (var item in list)
            {
                if (string.IsNullOrWhiteSpace(item.TenTieuChi))
                {
                    continue;
                }

                if (item.Id > 0)
                {
                    var entity = existing.FirstOrDefault(x => x.Id == item.Id);
                    if (entity != null)
                    {
                        entity.TenTieuChi = item.TenTieuChi.Trim();
                        entity.MoTaHuongDan = item.MoTa;
                        entity.TrongSo = item.TrongSo;
                        entity.DiemToiDa = item.DiemToiDa;
                        entity.SttHienThi = item.Stt ?? 0;
                    }
                }
                else
                {
                    var entity = new TieuChiChamDiem
                    {
                        IdLoaiPhieu = loaiPhieuId,
                        TenTieuChi = item.TenTieuChi.Trim(),
                        MoTaHuongDan = item.MoTa,
                        TrongSo = item.TrongSo,
                        DiemToiDa = item.DiemToiDa,
                        SttHienThi = item.Stt ?? 0
                    };
                    _context.TieuChiChamDiems.Add(entity);
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã lưu cấu hình tiêu chí.";
            return RedirectToAction(nameof(Index), new { loaiPhieuId, dotId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TieuChiItem model, int loaiPhieuId, int? dotId)
        {
            if (loaiPhieuId == 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn loại phiếu.";
                return RedirectToAction(nameof(Index), new { loaiPhieuId, dotId });
            }

            if (string.IsNullOrWhiteSpace(model.TenTieuChi))
            {
                TempData["ErrorMessage"] = "Tên tiêu chí không được để trống.";
                return RedirectToAction(nameof(Index), new { loaiPhieuId, dotId });
            }

            var entity = new TieuChiChamDiem
            {
                IdLoaiPhieu = loaiPhieuId,
                TenTieuChi = model.TenTieuChi?.Trim(),
                MoTaHuongDan = model.MoTa,
                TrongSo = model.TrongSo,
                DiemToiDa = model.DiemToiDa,
                SttHienThi = model.Stt ?? 0
            };

            _context.TieuChiChamDiems.Add(entity);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã thêm tiêu chí thành công.";
            return RedirectToAction(nameof(Index), new { loaiPhieuId, dotId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int? loaiPhieuId, int? dotId)
        {
            var entity = await _context.TieuChiChamDiems.FindAsync(id);
            if (entity == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy tiêu chí.";
                return RedirectToAction(nameof(Index), new { loaiPhieuId, dotId });
            }

            _context.TieuChiChamDiems.Remove(entity);
            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa tiêu chí.";
            }
            catch
            {
                TempData["ErrorMessage"] = "Không thể xóa tiêu chí (đang được sử dụng).";
            }

            return RedirectToAction(nameof(Index), new { loaiPhieuId, dotId });
        }
    }
}
