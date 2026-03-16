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

        /// <summary>
        /// Kiểm tra xem có đang trong giai đoạn chấm điểm (giữa kì hoặc cuối kì) không
        /// </summary>
        private async Task<(bool DangTrongGiaiDoan, string ThongBao)> KiemTraGiaiDoanChamDiem()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            // Kiểm tra tất cả các đợt đang hoạt động
            var dotDangHoatDong = await _context.DotDoAns
                .Where(d => d.TrangThai == true)
                .ToListAsync();

            foreach (var dot in dotDangHoatDong)
            {
                // Kiểm tra giai đoạn báo cáo giữa kì
                if (dot.NgayBatDauBaoCaoGiuaKi.HasValue && dot.NgayKetThucBaoCaoGiuaKi.HasValue)
                {
                    if (today >= dot.NgayBatDauBaoCaoGiuaKi.Value && today <= dot.NgayKetThucBaoCaoGiuaKi.Value)
                    {
                        return (true, $"Đang trong giai đoạn báo cáo giữa kì của đợt '{dot.TenDot}' ({dot.NgayBatDauBaoCaoGiuaKi.Value:dd/MM/yyyy} - {dot.NgayKetThucBaoCaoGiuaKi.Value:dd/MM/yyyy}). Không thể chỉnh sửa tiêu chí.");
                    }
                }

                // Kiểm tra giai đoạn báo cáo cuối kì
                if (dot.NgayBatDauBaoCaoCuoiKi.HasValue && dot.NgayKetThucBaoCaoCuoiKi.HasValue)
                {
                    if (today >= dot.NgayBatDauBaoCaoCuoiKi.Value && today <= dot.NgayKetThucBaoCaoCuoiKi.Value)
                    {
                        return (true, $"Đang trong giai đoạn báo cáo cuối kì của đợt '{dot.TenDot}' ({dot.NgayBatDauBaoCaoCuoiKi.Value:dd/MM/yyyy} - {dot.NgayKetThucBaoCaoCuoiKi.Value:dd/MM/yyyy}). Không thể chỉnh sửa tiêu chí.");
                    }
                }
            }

            return (false, string.Empty);
        }

        public async Task<IActionResult> Index(int? loaiPhieuId)
        {
            // Lấy tất cả loại phiếu chấm
            var loaiPhieuOptions = await _context.LoaiPhieuChams
                .OrderBy(lp => lp.TenLoaiPhieu)
                .Select(lp => new SelectListItem
                {
                    Value = lp.Id.ToString(),
                    Text = lp.TenLoaiPhieu ?? $"Loại {lp.Id}"
                })
                .ToListAsync();

            // Nếu chưa chọn loại phiếu, mặc định chọn loại đầu tiên
            if (!loaiPhieuId.HasValue && loaiPhieuOptions.Any())
            {
                loaiPhieuId = int.TryParse(loaiPhieuOptions.First().Value, out var firstId) ? firstId : null;
            }

            // Lấy danh sách tiêu chí theo loại phiếu
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

            // Kiểm tra loại phiếu có phải chỉ nhận xét không
            var chiNhanXet = false;
            if (loaiPhieuId.HasValue)
            {
                chiNhanXet = await _context.LoaiPhieuChams
                    .Where(lp => lp.Id == loaiPhieuId.Value)
                    .Select(lp => lp.ChiNhanXet ?? false)
                    .FirstOrDefaultAsync();
            }

            // Kiểm tra ràng buộc giai đoạn chấm điểm
            var (dangTrongGiaiDoan, thongBaoRangBuoc) = await KiemTraGiaiDoanChamDiem();

            var vm = new TieuChiViewModel
            {
                SelectedLoaiPhieuId = loaiPhieuId,
                ChiNhanXet = chiNhanXet,
                LoaiPhieuOptions = loaiPhieuOptions,
                TieuChis = tieuChis,
                DangTrongGiaiDoanChamDiem = dangTrongGiaiDoan,
                ThongBaoRangBuoc = thongBaoRangBuoc
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAll(List<TieuChiItem> items, int loaiPhieuId)
        {
            // Kiểm tra ràng buộc giai đoạn chấm điểm
            var (dangTrongGiaiDoan, thongBao) = await KiemTraGiaiDoanChamDiem();
            if (dangTrongGiaiDoan)
            {
                TempData["ErrorMessage"] = thongBao;
                return RedirectToAction(nameof(Index), new { loaiPhieuId });
            }

            if (loaiPhieuId == 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn loại phiếu.";
                return RedirectToAction(nameof(Index), new { loaiPhieuId });
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
            return RedirectToAction(nameof(Index), new { loaiPhieuId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TieuChiItem model, int loaiPhieuId)
        {
            // Kiểm tra ràng buộc giai đoạn chấm điểm
            var (dangTrongGiaiDoan, thongBao) = await KiemTraGiaiDoanChamDiem();
            if (dangTrongGiaiDoan)
            {
                TempData["ErrorMessage"] = thongBao;
                return RedirectToAction(nameof(Index), new { loaiPhieuId });
            }

            if (loaiPhieuId == 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn loại phiếu.";
                return RedirectToAction(nameof(Index), new { loaiPhieuId });
            }

            if (string.IsNullOrWhiteSpace(model.TenTieuChi))
            {
                TempData["ErrorMessage"] = "Tên tiêu chí không được để trống.";
                return RedirectToAction(nameof(Index), new { loaiPhieuId });
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
            return RedirectToAction(nameof(Index), new { loaiPhieuId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int? loaiPhieuId)
        {
            // Kiểm tra ràng buộc giai đoạn chấm điểm
            var (dangTrongGiaiDoan, thongBao) = await KiemTraGiaiDoanChamDiem();
            if (dangTrongGiaiDoan)
            {
                TempData["ErrorMessage"] = thongBao;
                return RedirectToAction(nameof(Index), new { loaiPhieuId });
            }

            var entity = await _context.TieuChiChamDiems.FindAsync(id);
            if (entity == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy tiêu chí.";
                return RedirectToAction(nameof(Index), new { loaiPhieuId });
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

            return RedirectToAction(nameof(Index), new { loaiPhieuId });
        }
    }
}
