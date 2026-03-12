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
                return View(new KetQuaBaoCaoViewModel 
                { 
                    CoDot = false,
                    ThongBaoDot = "Không tìm thấy thông tin sinh viên."
                });

            // Tìm đợt đồ án hiện tại (đang mở và chưa kết thúc)
            var today = DateOnly.FromDateTime(DateTime.Now);
            var dotHienTai = await _context.DotDoAns
                .Include(d => d.IdHocKiNavigation)
                .Where(d => d.TrangThai == true && 
                           (!d.NgayKetThucDot.HasValue || d.NgayKetThucDot.Value >= today))
                .OrderByDescending(d => d.NgayBatDauDot)
                .FirstOrDefaultAsync();

            if (dotHienTai == null)
            {
                return View(new KetQuaBaoCaoViewModel
                {
                    MaSinhVien = sinhVien.Mssv,
                    HoTen = sinhVien.IdNguoiDungNavigation?.HoTen,
                    CoDot = false,
                    ThongBaoDot = "Không tìm thấy đợt đồ án đang mở. Vui lòng liên hệ Ban chủ nhiệm khoa."
                });
            }

            // Tạo thông báo đợt
            var thoiGianDot = "";
            if (dotHienTai.NgayBatDauDot.HasValue && dotHienTai.NgayKetThucDot.HasValue)
                thoiGianDot = $"{dotHienTai.NgayBatDauDot.Value:dd/MM/yyyy} - {dotHienTai.NgayKetThucDot.Value:dd/MM/yyyy}";

            var svDeTai = await _context.SinhVienDeTais
                .Include(svdt => svdt.IdDeTaiNavigation)
                    .ThenInclude(dt => dt!.IdGvhdNavigation)
                        .ThenInclude(gv => gv!.IdNguoiDungNavigation)
                .FirstOrDefaultAsync(svdt =>
                    svdt.IdSinhVien == sinhVien.IdNguoiDung &&
                    svdt.IdDeTaiNavigation != null &&
                    svdt.IdDeTaiNavigation.IdDot == dotHienTai.Id &&
                    (svdt.TrangThai == "DA_DUYET" || svdt.TrangThai == "Đã duyệt"));

            var vm = new KetQuaBaoCaoViewModel
            {
                MaSinhVien = sinhVien.Mssv,
                HoTen = sinhVien.IdNguoiDungNavigation?.HoTen,
                TenDot = dotHienTai.TenDot,
                HocKi = dotHienTai.IdHocKiNavigation?.MaHocKi,
                ThoiGianDot = thoiGianDot,
                CoDot = true,
                CoDeTai = svDeTai != null,
                TenDeTai = svDeTai?.IdDeTaiNavigation?.TenDeTai,
                MaDeTai = svDeTai?.IdDeTaiNavigation?.MaDeTai,
                TenGVHD = svDeTai?.IdDeTaiNavigation?.IdGvhdNavigation?.IdNguoiDungNavigation?.HoTen,
                TrangThaiDeTai = svDeTai?.TrangThai
            };

            if (svDeTai == null)
            {
                vm.ThongBaoDot = "Bạn chưa có đề tài được duyệt trong đợt này.";
                return View(vm);
            }

            // Lấy thông tin phiên bảo vệ
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
                    LoaiHoiDongText = GetLoaiHoiDongText(hd.LoaiHoiDong),
                    DiaDiem = hd.DiaDiem,
                    NgayBaoCao = hd.NgayBaoCao?.ToString("dd/MM/yyyy"),
                    ThoiGian = hd.ThoiGianDuKien?.ToString("HH:mm"),
                    DiemTongKet = ketQuaSv?.DiemTongKet,
                    DiemChu = ketQuaSv?.DiemChu,
                    KetQua = ketQuaSv?.KetQua,
                    KetQuaCss = GetKetQuaCss(ketQuaSv?.KetQua),
                    DaCongBo = ketQuaSv != null && ketQuaSv.DiemTongKet.HasValue,
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
                        .OrderBy(tv => GetVaiTroOrder(tv.VaiTro))
                        .Select(tv => new ThanhVienHoiDongItem
                        {
                            HoTen = tv.IdGiangVienNavigation?.IdNguoiDungNavigation?.HoTen,
                            VaiTro = GetVaiTroText(tv.VaiTro),
                            VaiTroCss = GetVaiTroCss(tv.VaiTro)
                        })
                        .ToList()
                };

                if (hd.LoaiHoiDong != null && hd.LoaiHoiDong.Contains("GIUA_KY"))
                    vm.KetQuaGiuaKy = item;
                else if (hd.LoaiHoiDong != null && hd.LoaiHoiDong.Contains("CUOI_KY"))
                {
                    vm.KetQuaCuoiKy = item;
                    vm.KetQuaBaoVe = item; // Hội đồng bảo vệ = Hội đồng cuối kỳ
                }
            }

            // Lấy điểm GVHD từ bảng đánh giá (nếu có)
            // Giả sử điểm GVHD được lưu trong KetQuaBaoVeSinhVien với loại riêng hoặc từ GVHD
            // Tạm thời lấy từ Giữa kỳ nếu có, hoặc từ nguồn khác
            if (vm.KetQuaGiuaKy?.DiemTongKet != null)
            {
                vm.DiemGVHD = vm.KetQuaGiuaKy.DiemTongKet;
            }

            // Tính điểm trung bình chung theo trọng số:
            // GVHD: 30% + Hội đồng bảo vệ: 70%
            if (vm.DiemGVHD.HasValue && vm.KetQuaBaoVe?.DiemTongKet != null)
            {
                vm.DiemTrungBinhChung = Math.Round(
                    (vm.DiemGVHD.Value * KetQuaBaoCaoViewModel.TRONG_SO_GVHD) + 
                    (vm.KetQuaBaoVe.DiemTongKet.Value * KetQuaBaoCaoViewModel.TRONG_SO_HOI_DONG), 2);
                vm.XepLoaiTotNghiep = GetXepLoai(vm.DiemTrungBinhChung.Value);
            }
            // Trường hợp chỉ có điểm hội đồng bảo vệ (chưa có điểm GVHD riêng)
            else if (vm.KetQuaBaoVe?.DiemTongKet != null && !vm.DiemGVHD.HasValue)
            {
                // Hiển thị điểm hội đồng bảo vệ như điểm tạm
                vm.DiemTrungBinhChung = vm.KetQuaBaoVe.DiemTongKet;
                vm.XepLoaiTotNghiep = GetXepLoai(vm.DiemTrungBinhChung.Value) + " (tạm tính)";
            }

            return View(vm);
        }

        #region Helper Methods

        private static string GetLoaiHoiDongText(string? loaiHoiDong) => loaiHoiDong switch
        {
            "GIUA_KY" => "Hội đồng báo cáo giữa kỳ",
            "CUOI_KY" => "Hội đồng báo cáo cuối kỳ",
            _ => loaiHoiDong ?? "Hội đồng"
        };

        private static string GetKetQuaCss(string? ketQua) => ketQua switch
        {
            "DAT" or "Đạt" => "result-passed",
            "KHONG_DAT" or "Không đạt" => "result-failed",
            "BAO_LUU" or "Bảo lưu" => "result-pending",
            _ => "result-unknown"
        };

        private static int GetVaiTroOrder(string? vaiTro) => vaiTro switch
        {
            "CHU_TICH" => 0,
            "THU_KY" => 1,
            "PHAN_BIEN" => 2,
            "UY_VIEN" => 3,
            _ => 4
        };

        private static string GetVaiTroText(string? vaiTro) => vaiTro switch
        {
            "CHU_TICH" => "Chủ tịch",
            "THU_KY" => "Thư ký",
            "PHAN_BIEN" => "Phản biện",
            "UY_VIEN" => "Ủy viên",
            _ => vaiTro ?? ""
        };

        private static string GetVaiTroCss(string? vaiTro) => vaiTro switch
        {
            "CHU_TICH" => "role-chair",
            "THU_KY" => "role-secretary",
            "PHAN_BIEN" => "role-reviewer",
            "UY_VIEN" => "role-member",
            _ => ""
        };

        private static string GetXepLoai(double diem)
        {
            if (diem >= 9.0) return "Xuất sắc";
            if (diem >= 8.0) return "Giỏi";
            if (diem >= 7.0) return "Khá";
            if (diem >= 5.5) return "Trung bình khá";
            if (diem >= 5.0) return "Trung bình";
            return "Không đạt";
        }

        #endregion
    }
}
