using DATN_TMS.Areas.SinhVien.Models;
using DATN_TMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace DATN_TMS.Areas.SinhVien.Controllers
{
    /// <summary>
    /// Controller xem kết quả báo cáo cho sinh viên
    /// Kế thừa BaseSinhVienController để kiểm tra nguyện vọng đã duyệt
    /// </summary>
    public class KetQuaBaoCaoController : BaseSinhVienController
    {
        public KetQuaBaoCaoController(QuanLyDoAnTotNghiepContext context) : base(context)
        {
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var mssv = HttpContext.Session.GetString("UserCode");
            var sinhVien = await _context.SinhViens
                .Include(sv => sv.IdNguoiDungNavigation)
                .FirstOrDefaultAsync(sv => sv.Mssv == mssv);

            if (sinhVien == null)
                return View(new KetQuaBaoCaoViewModel());

            var dotHienTai = await _context.DotDoAns
                .FirstOrDefaultAsync(d => d.TrangThai == true);

            if (dotHienTai == null)
                return View(new KetQuaBaoCaoViewModel
                {
                    MaSinhVien = sinhVien.Mssv,
                    HoTen = sinhVien.IdNguoiDungNavigation?.HoTen
                });

            var svDeTai = await _context.SinhVienDeTais
                .Include(svdt => svdt.IdDeTaiNavigation)
                    .ThenInclude(dt => dt!.IdGvhdNavigation)
                        .ThenInclude(gv => gv!.IdNguoiDungNavigation)
                .FirstOrDefaultAsync(svdt =>
                    svdt.IdSinhVien == sinhVien.IdNguoiDung &&
                    svdt.IdDeTaiNavigation != null &&
                    svdt.IdDeTaiNavigation.IdDot == dotHienTai.Id &&
                    svdt.TrangThai == "DA_DUYET");

            var vm = new KetQuaBaoCaoViewModel
            {
                MaSinhVien = sinhVien.Mssv,
                HoTen = sinhVien.IdNguoiDungNavigation?.HoTen,
                TenDot = dotHienTai.TenDot,
                TenDeTai = svDeTai?.IdDeTaiNavigation?.TenDeTai,
                MaDeTai = svDeTai?.IdDeTaiNavigation?.MaDeTai,
                TenGVHD = svDeTai?.IdDeTaiNavigation?.IdGvhdNavigation?.IdNguoiDungNavigation?.HoTen
            };

            if (svDeTai == null)
                return View(vm);

            var phienBaoVes = await _context.PhienBaoVes
                .Include(p => p.IdHdBaocaoNavigation)
                    .ThenInclude(hd => hd!.ThanhVienHdBaoCaos)
                        .ThenInclude(tv => tv.IdGiangVienNavigation)
                            .ThenInclude(gv => gv!.IdNguoiDungNavigation)
                .Include(p => p.DiemChiTiets)
                    .ThenInclude(d => d.IdTieuChiNavigation)
                .Include(p => p.DiemChiTiets)
                    .ThenInclude(d => d.IdNguoiChamNavigation)
                        .ThenInclude(gv => gv!.IdNguoiDungNavigation)
                .Include(p => p.KetQuaBaoVeSinhViens)
                .Where(p => p.IdSinhVienDeTai == svDeTai.Id)
                .ToListAsync();

            foreach (var phien in phienBaoVes)
            {
                var hd = phien.IdHdBaocaoNavigation;
                if (hd == null) continue;

                var ketQuaSv = phien.KetQuaBaoVeSinhViens
                    .FirstOrDefault(kq => kq.IdSinhVien == sinhVien.IdNguoiDung);

                var item = new KetQuaHoiDongItem
                {
                    TenHoiDong = hd.TenHoiDong,
                    LoaiHoiDong = hd.LoaiHoiDong,
                    DiaDiem = hd.DiaDiem,
                    NgayBaoCao = hd.NgayBaoCao?.ToString("dd/MM/yyyy"),
                    ThoiGian = hd.ThoiGianDuKien?.ToString("HH:mm"),
                    DiemTongKet = ketQuaSv?.DiemTongKet,
                    DiemChu = ketQuaSv?.DiemChu,
                    KetQua = ketQuaSv?.KetQua,
                    DanhSachDiemTieuChi = phien.DiemChiTiets
                        .Where(d => d.IdSinhVien == sinhVien.IdNguoiDung)
                        .OrderBy(d => d.IdTieuChiNavigation?.SttHienThi)
                        .Select((d, idx) => new DiemTieuChiItem
                        {
                            Stt = idx + 1,
                            TenTieuChi = d.IdTieuChiNavigation?.TenTieuChi,
                            TrongSo = d.IdTieuChiNavigation?.TrongSo,
                            DiemToiDa = d.IdTieuChiNavigation?.DiemToiDa,
                            DiemSo = d.DiemSo,
                            NhanXet = d.NhanXet,
                            TenNguoiCham = d.IdNguoiChamNavigation?.IdNguoiDungNavigation?.HoTen
                        })
                        .ToList(),
                    DanhSachThanhVien = hd.ThanhVienHdBaoCaos
                        .OrderBy(tv => tv.VaiTro == "CHU_TICH" ? 0 : tv.VaiTro == "THU_KY" ? 1 : tv.VaiTro == "PHAN_BIEN" ? 2 : 3)
                        .Select(tv => new ThanhVienHoiDongItem
                        {
                            HoTen = tv.IdGiangVienNavigation?.IdNguoiDungNavigation?.HoTen,
                            VaiTro = tv.VaiTro switch
                            {
                                "CHU_TICH" => "Chủ tịch",
                                "THU_KY" => "Thư ký",
                                "PHAN_BIEN" => "Phản biện",
                                "UY_VIEN" => "Ủy viên",
                                _ => tv.VaiTro
                            }
                        })
                        .ToList()
                };

                if (hd.LoaiHoiDong != null && hd.LoaiHoiDong.Contains("GIUA_KY"))
                    vm.KetQuaGiuaKy = item;
                else if (hd.LoaiHoiDong != null && hd.LoaiHoiDong.Contains("CUOI_KY"))
                    vm.KetQuaCuoiKy = item;
            }

            return View(vm);
        }
    }
}
