namespace DATN_TMS.Areas.SinhVien.Models
{
    public class ChiTietDangKyDeTaiViewModel
    {
        // Thông tin đề tài (bên trái)
        public int Id { get; set; }
        public string? MaDeTai { get; set; }
        public string? TenDeTai { get; set; }
        public string? NguoiDeXuat { get; set; }
        public string? GVHD { get; set; }
        public string? TenChuyenNganh { get; set; }
        public string? MucTieu { get; set; }
        public string? PhamVi { get; set; }
        public string? CongNghe { get; set; }
        public string? YeuCauTinhMoi { get; set; }
        public string? KetQuaDuKien { get; set; }
        public string? NhiemVuCuThe { get; set; }
        public string? TrangThaiDeTai { get; set; }

        // Thông tin đăng ký (bên phải)
        public string? MssvSinhVien { get; set; }
        public string? HoTenSinhVien { get; set; }
        public int? IdSinhVienHienTai { get; set; }
        public bool DaDangKyDeTaiNay { get; set; }
        public bool DaDangKyDeTaiKhac { get; set; } // Giữ lại để backward compatible
        public string? TrangThaiDangKyCuaSV { get; set; }
        public string? GiaiDoan { get; set; }
        public string? ThongBaoThoiHan { get; set; }

        // NEW: SV đã được duyệt vào đề tài khác (chặn đăng ký thêm)
        public bool SvDaDuyetDeTaiKhac { get; set; }

        // Danh sách lượt đăng ký (theo nhóm)
        public List<LuotDangKyItem> DanhSachLuotDangKy { get; set; } = new();
        public int SoLuotDangKy { get; set; } // Số lượt đăng ký (1 nhóm = 1 lượt)
        public int SoLuotDaDuyet { get; set; } // Số lượt đã được duyệt
        public bool DaHetLuotDangKy { get; set; } // Đã hết slot đăng ký

        // Trạng thái sĩ số
        public int SoSinhVienDaDuyet { get; set; } // Số SV đã được duyệt chính thức
        public int SoSlotToiDa { get; set; } = 2;
        public bool DaDuSiSo => SoSinhVienDaDuyet >= SoSlotToiDa;
        public string TrangThaiSiSo => $"{SoSinhVienDaDuyet}/{SoSlotToiDa}";

        // BUSINESS RULE #3: Đánh dấu đề tài do SV tự đề xuất
        public bool LaDeTaiSVTuDeXuat { get; set; }

        // ID đợt đồ án hiện tại (để tìm kiếm SV thứ 2)
        public int? IdDot { get; set; }
    }

    /// <summary>
    /// Thông tin 1 lượt đăng ký (có thể 1 hoặc 2 SV)
    /// </summary>
    public class LuotDangKyItem
    {
        public int IdLuotDangKy { get; set; } // ID của SV đại diện (người đăng ký đầu tiên)
        public string? TrangThai { get; set; }
        public string? StatusCss { get; set; }
        public string? NgayDangKy { get; set; }
        public bool LaSvHienTai { get; set; } // SV hiện tại có trong lượt này không
        public bool DaDuyet { get; set; } // Lượt đăng ký đã được duyệt chưa

        // Sinh viên đại diện (người đăng ký)
        public int IdSinhVien1 { get; set; }
        public string? Mssv1 { get; set; }
        public string? HoTen1 { get; set; }

        // Sinh viên thứ 2 (nếu có)
        public int? IdSinhVien2 { get; set; }
        public string? Mssv2 { get; set; }
        public string? HoTen2 { get; set; }
    }

    // Giữ lại cho backward compatibility
    public class ThanhVienDangKyItem
    {
        public int IdSinhVien { get; set; }
        public string? Mssv { get; set; }
        public string? HoTen { get; set; }
        public string? TrangThai { get; set; }
        public string? StatusCss { get; set; }
        public string? NgayDangKy { get; set; }
        public bool LaSvHienTai { get; set; }
        public int? IdNhom { get; set; } // ID của người đăng ký đại diện (để nhóm)
    }
}
