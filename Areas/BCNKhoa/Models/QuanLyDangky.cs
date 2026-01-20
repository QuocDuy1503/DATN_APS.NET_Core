namespace DATN_TMS.Areas.BCNKhoa.Models
{
    public class QuanLyDangky
    {
        public int Id { get; set; }
        public string MaSinhVien { get; set; }
        public string HoTen { get; set; }
        public string IdSinhVien { get; set; } 
        public string TenKhoa { get; set; }
        public string TenChuyenNganh { get; set; }
        public int TinChiTichLuy { get; set; }
        public int? TrangThai { get; set; }
    }
}
