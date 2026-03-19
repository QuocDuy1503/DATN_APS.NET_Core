using System;
using System.Collections.Generic;

namespace DATN_TMS.Areas.GiangVien.Models
{
    // ViewModel hiển thị danh sách hội đồng
    public class HoiDongChamDiemViewModel
    {
        public int Id { get; set; }
        public string MaHoiDong { get; set; } = "";
        public string TenHoiDong { get; set; } = "";
        public string LoaiHoiDong { get; set; } = "";
        public string TenBoMon { get; set; } = "";
        public DateOnly? NgayBaoCao { get; set; }
        public string DiaDiem { get; set; } = "";
        public int SoLuongDeTai { get; set; }
        public string VaiTroTrongHoiDong { get; set; } = "";
        public bool DaChamDiem { get; set; }
        public bool CoDenNgayBaoCao { get; set; }
        public bool DaHetHanBaoCao { get; set; }

        public string LoaiHoiDongDisplay => LoaiHoiDong switch
        {
            "GIUA_KY" => "Báo cáo giữa kỳ",
            "CUOI_KY" => "Báo cáo cuối kỳ",
            _ => LoaiHoiDong
        };

        public string VaiTroDisplay => VaiTroTrongHoiDong switch
        {
            "CHU_TICH" => "Chủ tịch",
            "THU_KY" => "Thư ký",
            "PHAN_BIEN" => "Phản biện",
            "UY_VIEN" => "Ủy viên",
            _ => VaiTroTrongHoiDong
        };
    }

    // ViewModel chi tiết hội đồng
    public class ChiTietHoiDongChamDiemViewModel
    {
        public HoiDongChamDiemViewModel HoiDong { get; set; } = new();
        public List<ThanhVienHoiDongChamDiemViewModel> ThanhViens { get; set; } = new();
        public List<PhienBaoVeViewModel> PhienBaoVes { get; set; } = new();
        public List<DeTaiChamDiemViewModel> DanhSachDeTai { get; set; } = new(); // Danh sách đề tài (nhóm theo đề tài)
        public string VaiTroHienTai { get; set; } = "";
        public bool LaChuTich { get; set; }
        public bool LaThuKy { get; set; }
        public bool DaHetHanBaoCao { get; set; }
    }

    // ViewModel thành viên hội đồng
    public class ThanhVienHoiDongChamDiemViewModel
    {
        public int IdGiangVien { get; set; }
        public string TenGiangVien { get; set; } = "";
        public string VaiTro { get; set; } = "";
        public bool DaChamDiem { get; set; }

        public string VaiTroDisplay => VaiTro switch
        {
            "CHU_TICH" => "Chủ tịch",
            "THU_KY" => "Thư ký",
            "PHAN_BIEN" => "Phản biện",
            "UY_VIEN" => "Ủy viên",
            _ => VaiTro
        };
    }

    // ViewModel sinh viên trong đề tài
    public class SinhVienDeTaiChamDiemViewModel
    {
        public int IdSinhVien { get; set; }
        public int IdSinhVienDeTai { get; set; }
        public int PhienBaoVeId { get; set; }
        public string TenSinhVien { get; set; } = "";
        public string Mssv { get; set; } = "";
        public double? DiemGVHD { get; set; }
        public string NhanXetGVHD { get; set; } = "";
        public double? DiemCuaBan { get; set; } // Tổng điểm người dùng hiện tại đã chấm
        public string LinkTaiLieu { get; set; } = "";
        public List<DiemThanhVienViewModel> DiemHoiDong { get; set; } = new();

        // Điểm TB hội đồng cho SV này: group theo thành viên, sum mỗi người, rồi lấy average
        public double? DiemTBHoiDongSV
        {
            get
            {
                if (!DiemHoiDong.Any()) return null;
                var diemTheoThanhVien = DiemHoiDong
                    .GroupBy(d => d.IdNguoiCham)
                    .Select(g => g.Sum(d => d.DiemSo))
                    .ToList();
                return diemTheoThanhVien.Any() ? Math.Round(diemTheoThanhVien.Average(), 2) : null;
            }
        }

        // Điểm trung bình cuối = 30% GVHD + 70% Hội đồng
        public double? DiemTrungBinhCuoi
        {
            get
            {
                var diemHD = DiemTBHoiDongSV;
                if (!DiemGVHD.HasValue || !diemHD.HasValue) return null;
                return Math.Round(DiemGVHD.Value * 0.3 + diemHD.Value * 0.7, 2);
            }
        }
    }

    // ViewModel đề tài trong hội đồng (nhóm các sinh viên cùng đề tài)
    public class DeTaiChamDiemViewModel
    {
        public int IdDeTai { get; set; }
        public string MaDeTai { get; set; } = "";
        public string TenDeTai { get; set; } = "";
        public string TenGVHD { get; set; } = "";
        public int SttBaoCao { get; set; }
        public List<SinhVienDeTaiChamDiemViewModel> DanhSachSinhVien { get; set; } = new();
        public string TrangThaiXacNhan { get; set; } = "CHO_XAC_NHAN";
        public double? DiemTongKetCuoi { get; set; }

        public bool DaXacNhan => TrangThaiXacNhan == "DA_XAC_NHAN";

        // Kiểm tra đã chấm điểm chưa (có ít nhất 1 sinh viên có điểm)
        public bool DaChamDiem => DanhSachSinhVien.Any(sv => sv.DiemCuaBan.HasValue);

        // Tính điểm trung bình (30% GVHD + 70% Hội đồng) trung bình các SV
        public double? DiemTBHoiDong
        {
            get
            {
                var diemCuoiList = DanhSachSinhVien
                    .Select(sv => sv.DiemTrungBinhCuoi)
                    .Where(d => d.HasValue)
                    .Select(d => d!.Value)
                    .ToList();
                return diemCuoiList.Any() ? Math.Round(diemCuoiList.Average(), 2) : (double?)null;
            }
        }

        // Lấy phiên bảo vệ đầu tiên (để dùng cho link chấm điểm)
        public int PhienBaoVeId => DanhSachSinhVien.FirstOrDefault()?.PhienBaoVeId ?? 0;
    }

    // ViewModel phiên bảo vệ (giữ lại để tương thích)
    public class PhienBaoVeViewModel
    {
        public int Id { get; set; }
        public int SttBaoCao { get; set; }
        public int IdSinhVienDeTai { get; set; }
        public string TenSinhVien { get; set; } = "";
        public string Mssv { get; set; } = "";
        public string TenDeTai { get; set; } = "";
        public string MaDeTai { get; set; } = "";
        public string TenGVHD { get; set; } = "";
        public double? DiemGVHD { get; set; }
        public string NhanXetGVHD { get; set; } = "";
        public string LinkTaiLieu { get; set; } = "";
        public List<DiemThanhVienViewModel> DiemHoiDong { get; set; } = new();
        public string TrangThaiXacNhan { get; set; } = "CHO_XAC_NHAN";
        public double? DiemTongKetCuoi { get; set; }

        public bool DaXacNhan => TrangThaiXacNhan == "DA_XAC_NHAN";
    }

    // ViewModel điểm từng thành viên
    public class DiemThanhVienViewModel
    {
        public int IdNguoiCham { get; set; }
        public string TenNguoiCham { get; set; } = "";
        public double DiemSo { get; set; }
        public string NhanXet { get; set; } = "";
    }

    // ViewModel sinh viên để chấm điểm
    public class SinhVienChamDiemInfo
    {
        public int IdSinhVien { get; set; }
        public int IdSinhVienDeTai { get; set; }
        public int PhienBaoVeId { get; set; }
        public string TenSinhVien { get; set; } = "";
        public string Mssv { get; set; } = "";
        public double? DiemGVHD { get; set; }
        public string NhanXetGVHD { get; set; } = "";

        // Điểm đã chấm cho sinh viên này (riêng biệt)
        public List<TieuChiChamDiemViewModel> DiemDaCham { get; set; } = new();
        public bool DaChamDiem => DiemDaCham.Any(d => d.DiemDaCham > 0 || !string.IsNullOrEmpty(d.NhanXetDaCham));
    }

    // ViewModel trang chấm điểm (hỗ trợ nhiều sinh viên cùng đề tài)
    public class ChamDiemPhienBaoVeViewModel
    {
        public int PhienBaoVeId { get; set; } // Phiên bảo vệ đầu tiên
        public int IdDeTai { get; set; }
        public int HoiDongId { get; set; }
        public string TenHoiDong { get; set; } = "";
        public string LoaiHoiDong { get; set; } = "";
        public string TenDeTai { get; set; } = "";
        public string MaDeTai { get; set; } = "";
        public bool LaHoiDongGiuaKy { get; set; }

        // Danh sách sinh viên cùng đề tài
        public List<SinhVienChamDiemInfo> DanhSachSinhVien { get; set; } = new();

        // Giữ lại để tương thích với view cũ
        public string TenSinhVien { get; set; } = "";
        public string Mssv { get; set; } = "";
        public double DiemGVHD { get; set; }
        public string NhanXetGVHD { get; set; } = "";

        // Thông tin vai trò và loại phiếu
        public string VaiTroHienTai { get; set; } = "";
        public bool LaPhanBien { get; set; }
        public string TenLoaiPhieu { get; set; } = "";
        public bool ChiNhanXet { get; set; }

        public List<TieuChiChamDiemViewModel> TieuChis { get; set; } = new();
        public bool DaChamDiem { get; set; }

        public string LoaiHoiDongDisplay => LoaiHoiDong switch
        {
            "GIUA_KY" => "Giữa kì",
            "CUOI_KY" => "Cuối kỳ",
            _ => LoaiHoiDong
        };

        public string VaiTroDisplay => VaiTroHienTai switch
        {
            "CHU_TICH" => "Chủ tịch",
            "THU_KY" => "Thư ký",
            "PHAN_BIEN" => "Phản biện",
            "UY_VIEN" => "Ủy viên",
            _ => VaiTroHienTai
        };

        // Kiểm tra có nhiều sinh viên không
        public bool CoNhieuSinhVien => DanhSachSinhVien.Count > 1;
    }

    // ViewModel tiêu chí chấm điểm
    public class TieuChiChamDiemViewModel
    {
        public int Id { get; set; }
        public string TenTieuChi { get; set; } = "";
        public string MoTaHuongDan { get; set; } = "";
        public double TrongSo { get; set; }
        public double DiemToiDa { get; set; }
        public double DiemDaCham { get; set; }
        public string NhanXetDaCham { get; set; } = "";
    }

    // Request lưu điểm (hỗ trợ nhiều sinh viên với điểm riêng biệt)
    public class LuuDiemRequest
    {
        public int PhienBaoVeId { get; set; }
        public int IdDeTai { get; set; }
        public List<DiemSinhVienInput> DanhSachDiemSinhVien { get; set; } = new(); // Điểm riêng biệt cho từng SV
    }

    // Điểm của từng sinh viên
    public class DiemSinhVienInput
    {
        public int PhienBaoVeId { get; set; }
        public int IdSinhVien { get; set; }
        public List<DiemChiTietInput> DiemChiTiets { get; set; } = new();
    }

    public class DiemChiTietInput
    {
        public int IdTieuChi { get; set; }
        public double DiemSo { get; set; }
        public string NhanXet { get; set; } = "";
    }

    // Request điều chỉnh điểm (thư ký)
    public class DieuChinhDiemRequest
    {
        public int PhienBaoVeId { get; set; }
        public int IdSinhVien { get; set; }
        public double? DiemCu { get; set; }
        public double? DiemMoi { get; set; }
        public string LyDo { get; set; } = "";
    }

    // Request thư ký chỉnh sửa điểm thành viên hội đồng trên bảng điểm tổng hợp
    public class ThuKyChinhSuaDiemThanhVienRequest
    {
        public int PhienBaoVeId { get; set; }
        public int IdSinhVien { get; set; }
        public int IdNguoiCham { get; set; }
        public double DiemCu { get; set; }
        public double DiemMoi { get; set; }
        public string LyDo { get; set; } = "";
    }

    // Request xác nhận điểm (chủ tịch)
    public class XacNhanDiemRequest
    {
        public int PhienBaoVeId { get; set; }
        public int IdSinhVien { get; set; }
        public double DiemTongKet { get; set; }
        public string GhiChu { get; set; } = "";
    }

    // ViewModel bảng điểm tổng hợp
    public class BangDiemTongHopViewModel
    {
        public int PhienBaoVeId { get; set; }
        public int HoiDongId { get; set; }
        public string TenHoiDong { get; set; } = "";
        public string LoaiHoiDong { get; set; } = "";

        // Thông tin đề tài
        public string TenDeTai { get; set; } = "";
        public string MaDeTai { get; set; } = "";
        public string TenGVHD { get; set; } = "";

        // Giữ lại cho backward compatibility (SV đầu tiên)
        public string TenSinhVien { get; set; } = "";
        public string Mssv { get; set; } = "";
        public double DiemGVHD { get; set; }
        public string NhanXetGVHD { get; set; } = "";

        // Danh sách sinh viên cùng đề tài (cho bảng điểm kiểu bảng ngang)
        public List<SinhVienBangDiemItem> DanhSachSinhVien { get; set; } = new();

        // Điểm các thành viên hội đồng
        public List<DiemThanhVienTongHopViewModel> DiemThanhViens { get; set; } = new();

        public double DiemTBHoiDong { get; set; }
        public double ChenhLechGVHD { get; set; }
        public bool CoChenhLechLon { get; set; }

        // Xác nhận
        public string TrangThaiXacNhan { get; set; } = "";
        public double? DiemTongKetCuoi { get; set; }
        public string GhiChuXacNhan { get; set; } = "";

        // Lịch sử
        public List<LichSuCapNhatViewModel> LichSuCapNhat { get; set; } = new();

        // Quyền
        public bool LaThuKy { get; set; }
        public bool LaChuTich { get; set; }
        public bool DaHetHanBaoCao { get; set; }
        public bool ConTrongGiaiDoanBaoCao { get; set; }

        public bool DaXacNhan => TrangThaiXacNhan == "DA_XAC_NHAN";
        public bool LaHoiDongGiuaKy => LoaiHoiDong == "GIUA_KY";

        public string LoaiHoiDongDisplay => LoaiHoiDong switch
        {
            "GIUA_KY" => "Giữa kì",
            "CUOI_KY" => "Cuối kỳ",
            _ => LoaiHoiDong
        };

        // Lấy danh sách thành viên hội đồng (sắp xếp theo vai trò) để tạo header bảng
        public List<DiemThanhVienTongHopViewModel> DanhSachThanhVienHoiDong =>
            DiemThanhViens
                .OrderBy(t => t.VaiTro == "PHAN_BIEN" ? 0 : t.VaiTro == "CHU_TICH" ? 1 : t.VaiTro == "UY_VIEN" ? 2 : 3)
                .ThenBy(t => t.TenGiangVien)
                .ToList();
    }

    // Thông tin điểm cho 1 sinh viên trong bảng tổng hợp
    public class SinhVienBangDiemItem
    {
        public int IdSinhVien { get; set; }
        public int PhienBaoVeId { get; set; }
        public string TenSinhVien { get; set; } = "";
        public string Mssv { get; set; } = "";
        public double DiemGVHD { get; set; }
        // Điểm từng thành viên hội đồng: key = IdGiangVien, value = TongDiem
        public Dictionary<int, double?> DiemTheoThanhVien { get; set; } = new();
        // Điểm TB hội đồng cho SV này
        public double DiemTBHoiDong { get; set; }
        // Điểm trung bình cuối = GVHD*30% + HĐ*70%
        public double DiemTrungBinhCuoi { get; set; }
    }

    public class DiemThanhVienTongHopViewModel
    {
        public int IdGiangVien { get; set; }
        public string TenGiangVien { get; set; } = "";
        public string VaiTro { get; set; } = "";
        public bool DaChamDiem { get; set; }
        public double TongDiem { get; set; }

        public string VaiTroDisplay => VaiTro switch
        {
            "CHU_TICH" => "Chủ tịch",
            "THU_KY" => "Thư ký",
            "PHAN_BIEN" => "Phản biện",
            "UY_VIEN" => "Ủy viên",
            _ => VaiTro
        };
    }

    public class LichSuCapNhatViewModel
    {
        public string TenNguoiCapNhat { get; set; } = "";
        public string LoaiCapNhat { get; set; } = "";
        public double? DiemCu { get; set; }
        public double? DiemMoi { get; set; }
        public string LyDo { get; set; } = "";
        public DateTime? NgayCapNhat { get; set; }

        public string LoaiCapNhatDisplay => LoaiCapNhat switch
        {
            "THU_KY_DIEU_CHINH" => "Thư ký điều chỉnh",
            "THU_KY_CHINH_SUA_DIEM_THANH_VIEN" => "Thư ký sửa điểm thành viên",
            "CHU_TICH_XAC_NHAN" => "Chủ tịch xác nhận",
            _ => LoaiCapNhat
        };
    }
}
