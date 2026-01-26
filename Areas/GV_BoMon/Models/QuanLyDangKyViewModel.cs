namespace DATN_TMS.Areas.GV_BoMon.Models
{
    // Dùng chung với BCNKhoa
    public class QuanLyDangKyViewModel
    {
        public int Id { get; set; }
        public string? MaSinhVien { get; set; }
        public string? HoTen { get; set; }
        public string? IdSinhVien { get; set; }
        public string? TenKhoa { get; set; }
        public string? TenChuyenNganh { get; set; }
        public int TinChiTichLuy { get; set; }
        public int? TrangThai { get; set; }
    }

    public class KetQuaHocTapModel
    {
        public string? HoTen { get; set; }
        public string? MSSV { get; set; }
        public string? ChuyenNganh { get; set; }
        public double TongTinChi { get; set; }
        public double GPA { get; set; }
        public List<BangDiemItem> BangDiem { get; set; } = new List<BangDiemItem>();
    }

    public class BangDiemItem
    {
        public int Stt { get; set; }
        public string? MaHocPhan { get; set; }
        public string? TenHocPhan { get; set; }
        public double SoTc { get; set; }
        public double DiemSo { get; set; }
        public string? DiemChu { get; set; }
        public bool KetQua { get; set; }
    }
}