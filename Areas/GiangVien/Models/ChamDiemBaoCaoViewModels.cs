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
        public string VaiTroHienTai { get; set; } = "";
        public bool LaChuTich { get; set; }
        public bool LaThuKy { get; set; }
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

    // ViewModel phiên bảo vệ
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

    // ViewModel trang chấm điểm
    public class ChamDiemPhienBaoVeViewModel
    {
        public int PhienBaoVeId { get; set; }
        public int HoiDongId { get; set; }
        public string TenHoiDong { get; set; } = "";
        public string TenSinhVien { get; set; } = "";
        public string Mssv { get; set; } = "";
        public string TenDeTai { get; set; } = "";
        public double DiemGVHD { get; set; }
        public string NhanXetGVHD { get; set; } = "";
        public List<TieuChiChamDiemViewModel> TieuChis { get; set; } = new();
        public bool DaChamDiem { get; set; }
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

    // Request lưu điểm
    public class LuuDiemRequest
    {
        public int PhienBaoVeId { get; set; }
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
        public string TenSinhVien { get; set; } = "";
        public string Mssv { get; set; } = "";
        public string TenDeTai { get; set; } = "";
        public string MaDeTai { get; set; } = "";

        // Điểm GVHD
        public string TenGVHD { get; set; } = "";
        public double DiemGVHD { get; set; }
        public string NhanXetGVHD { get; set; } = "";

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

        public bool DaXacNhan => TrangThaiXacNhan == "DA_XAC_NHAN";
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
            "CHU_TICH_XAC_NHAN" => "Chủ tịch xác nhận",
            _ => LoaiCapNhat
        };
    }
}
