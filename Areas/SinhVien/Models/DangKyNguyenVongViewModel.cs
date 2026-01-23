using System.ComponentModel.DataAnnotations;

namespace DATN_TMS.Areas.SinhVien.Models
{
    public class DangKyNguyenVongViewModel
    {
        [Display(Name = "MSSV")]
        [Required(ErrorMessage = "Vui lòng nhập MSSV")]
        public string? Mssv { get; set; }

        [Display(Name = "Họ tên")]
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        public string? HoTen { get; set; }

        [Display(Name = "Chuyên ngành")]
        [Required(ErrorMessage = "Vui lòng chọn chuyên ngành")]
        public int? IdChuyenNganh { get; set; }

        public string? TenChuyenNganh { get; set; }

        [Display(Name = "Email")]
        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string? Email { get; set; }

        [Display(Name = "Số điện thoại")]
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? Sdt { get; set; }

        [Display(Name = "Số tín chỉ tích lũy")]
        [Required(ErrorMessage = "Vui lòng nhập số tín chỉ")]
        [Range(0, 200, ErrorMessage = "Số tín chỉ phải từ 0 đến 200")]
        public double? SoTinChiTichLuy { get; set; }

        [Display(Name = "Điểm tích lũy (GPA)")]
        [Range(0, 4.0, ErrorMessage = "GPA phải từ 0 đến 4.0")]
        public double? Gpa { get; set; }

        [Display(Name = "Môn còn nợ")]
        public string? MonConNo { get; set; }

        public int? IdDot { get; set; }
        public string? TenDot { get; set; }
        public int? IdSinhVien { get; set; }

        public List<ChuyenNganhItem>? DanhSachChuyenNganh { get; set; }
    }

    public class ChuyenNganhItem
    {
        public int Id { get; set; }
        public string? TenChuyenNganh { get; set; }
    }
}