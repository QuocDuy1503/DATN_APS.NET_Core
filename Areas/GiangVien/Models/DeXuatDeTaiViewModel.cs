using System.ComponentModel.DataAnnotations;

namespace DATN_TMS.Areas.GiangVien.Models
{
    public class DeXuatDeTaiViewModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Mã ?? tài không ???c ?? tr?ng")]
        public string MaDeTai { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên ?? tài không ???c ?? tr?ng")]
        public string TenDeTai { get; set; } = string.Empty;

        [Required(ErrorMessage = "Chuyên ngành không ???c ?? tr?ng")]
        public int IdChuyenNganh { get; set; }

        public string MucTieuChinh { get; set; } = string.Empty;

        public string PhamViChucNang { get; set; } = string.Empty;

        public string CongNgheSuDung { get; set; } = string.Empty;

        public string YeuCauTinhMoi { get; set; } = string.Empty;

        public string SanPhamKetQuaDuKien { get; set; } = string.Empty;

        public string NhiemVuCuThe { get; set; } = string.Empty;

        // Sinh viên ???c ch?n (t?i ?a 2)
        public int? IdSinhVien1 { get; set; }
        public int? IdSinhVien2 { get; set; }

        public int? IdDot { get; set; }
        public string? TenDot { get; set; }
        public int? IdNguoiDeXuat { get; set; }
        public string? MaGVDeXuat { get; set; }
        public string? TenGVDeXuat { get; set; }

        public List<DeTaiItem> DanhSachDeTai { get; set; } = new();
        public List<ChuyenNganhItem> DanhSachChuyenNganh { get; set; } = new();
        public List<SinhVienDuocChonItem> DanhSachSinhVien { get; set; } = new();
    }

    public class DeTaiItem
    {
        public int Id { get; set; }
        public string MaDeTai { get; set; } = string.Empty;
        public string TenDeTai { get; set; } = string.Empty;
        public string TenChuyenNganh { get; set; } = string.Empty;
        public string TrangThai { get; set; } = string.Empty;
        public string? TenGVDeXuat { get; set; } = string.Empty;
    }

    public class ChuyenNganhItem
    {
        public int Id { get; set; }
        public string TenChuyenNganh { get; set; } = string.Empty;
    }

    public class SinhVienDuocChonItem
    {
        public int IdSinhVien { get; set; }
        public string? Mssv { get; set; }
        public string? HoTen { get; set; }
        public string? TenChuyenNganh { get; set; }
    }
}
