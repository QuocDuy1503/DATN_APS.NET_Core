namespace DATN_TMS.Areas.GiangVien.Models
{
    // === QUẢN LÝ ĐĂNG KÝ ĐỀ TÀI (Duyệt SV đăng ký) ===
    public class DangKyDeTaiGVItem
    {
        public int IdDeTai { get; set; }
        public string? MaDeTai { get; set; }
        public string? TenDeTai { get; set; }
        public string? ChuyenNganh { get; set; }
        public int SoLuongDangKy { get; set; }
        public List<SinhVienDangKyItem> DanhSachSV { get; set; } = new();
    }

    public class SinhVienDangKyItem
    {
        public int IdSvDeTai { get; set; }
        public string? Mssv { get; set; }
        public string? HoTen { get; set; }
        public string? KhoaHoc { get; set; }
        public string? TrangThai { get; set; }
        public string? NgayDangKy { get; set; }
    }

    // === DANH SÁCH SINH VIÊN ===
    public class SinhVienGVItem
    {
        public string? Mssv { get; set; }
        public string? HoTen { get; set; }
        public string? Email { get; set; }
        public string? ChuyenNganh { get; set; }
        public string? KhoaHoc { get; set; }
        public string? TenDeTai { get; set; }
        public string? TrangThai { get; set; }
    }

    // === BẢNG KẾ HOẠCH SINH VIÊN ===
    public class KeHoachSVGVItem
    {
        public int Id { get; set; }
        public int? Stt { get; set; }
        public string? TenCongViec { get; set; }
        public string? NguoiThucHien { get; set; }
        public string? TenDeTai { get; set; }
        public string? NgayBatDau { get; set; }
        public string? NgayKetThuc { get; set; }
        public string? NgayBDThucTe { get; set; }
        public string? NgayKTThucTe { get; set; }
        public string? TrangThai { get; set; }
        public string? StatusCss { get; set; }
        public string? StatusText { get; set; }
    }

    // === NỘP BÁO CÁO ===
    public class NopBaoCaoGVItem
    {
        public int Id { get; set; }
        public int? Stt { get; set; }
        public string? TenBaoCao { get; set; }
        public string? TenSinhVien { get; set; }
        public string? Mssv { get; set; }
        public string? TenDeTai { get; set; }
        public string? NgayNop { get; set; }
        public string? TrangThai { get; set; }
        public string? LoaiBaoCao { get; set; }
        public string? FilePath { get; set; }
        public string? GhiChuGui { get; set; }
    }

    // === ĐÁNH GIÁ KẾT QUẢ ĐỀ TÀI ===
    public class DanhGiaKetQuaGVItem
    {
        public int IdDeTai { get; set; }
        public string? MaDeTai { get; set; }
        public string? TenDeTai { get; set; }
        public string? ChuyenNganh { get; set; }
        public int SoLuongSV { get; set; }
        public List<SinhVienDGItem> DanhSachSV { get; set; } = new();
    }

    public class SinhVienDGItem
    {
        public string? Mssv { get; set; }
        public string? HoTen { get; set; }
        public string? KhoaHoc { get; set; }
    }

    // === NHẬT KÝ HƯỚNG DẪN ===
    public class NhatKyHuongDanGVItem
    {
        public int Id { get; set; }
        public string? TenGvhd { get; set; }
        public string? NgayHop { get; set; }
        public string? ThoiGian { get; set; }
        public string? HinhThucHop { get; set; }
        public string? DiaDiem { get; set; }
        public string? NoiDung { get; set; }
        public string? MucTieu { get; set; }
        public string? ThanhVien { get; set; }
        public string? ActionListJson { get; set; }
        public int TaskCount { get; set; }
    }
}
