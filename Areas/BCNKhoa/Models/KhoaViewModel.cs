namespace DATN_TMS.Areas.BCNKhoa.Models
{
    public class KhoaViewModel
    {
        public int Id { get; set; }
        public string MaKhoa { get; set; }
        public string TenKhoa { get; set; }
        public int? NamNhapHoc { get; set; }
        public int? NamTotNghiep { get; set; }
        public bool? TrangThai { get; set; } // True: Đang đào tạo, False: Đã tốt nghiệp
    }
}