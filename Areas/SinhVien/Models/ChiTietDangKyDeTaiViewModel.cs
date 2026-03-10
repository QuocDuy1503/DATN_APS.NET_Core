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
        public bool DaDangKyDeTaiNay { get; set; }
        public bool DaDangKyDeTaiKhac { get; set; }
        public string? TrangThaiDangKyCuaSV { get; set; }
        public string? GiaiDoan { get; set; }
        public string? ThongBaoThoiHan { get; set; }

        // Danh sách thành viên đã đăng ký (đã lọc theo quyền xem)
        public List<ThanhVienDangKyItem> DanhSachThanhVien { get; set; } = new();
        public int SoLuongDaDangKy { get; set; }
        public int SoLuongDaDuyet { get; set; }
        public bool DaToanBoSlot { get; set; }
    }

    public class ThanhVienDangKyItem
    {
        public int IdSinhVien { get; set; }
        public string? Mssv { get; set; }
        public string? HoTen { get; set; }
        public string? TrangThai { get; set; }
        public string? StatusCss { get; set; }
        public string? NgayDangKy { get; set; }
        public bool LaSvHienTai { get; set; }
    }
}
