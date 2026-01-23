using System.ComponentModel.DataAnnotations;

namespace DATN_TMS.Areas.SinhVien.Models
{
    public class DeXuatDeTaiViewModel
    {
        public int? Id { get; set; }

        [Display(Name = "Mã đề tài")]
        [Required(ErrorMessage = "Vui lòng nhập mã đề tài")]
        public string? MaDeTai { get; set; }

        [Display(Name = "Tên đề tài")]
        [Required(ErrorMessage = "Vui lòng nhập tên đề tài")]
        public string? TenDeTai { get; set; }

        [Display(Name = "Chuyên ngành")]
        [Required(ErrorMessage = "Vui lòng chọn chuyên ngành")]
        public int? IdChuyenNganh { get; set; }

        [Display(Name = "Mục tiêu chính")]
        [Required(ErrorMessage = "Vui lòng nhập mục tiêu chính")]
        public string? MucTieuChinh { get; set; }

        [Display(Name = "Phạm vi và chức năng")]
        public string? PhamViChucNang { get; set; }

        [Display(Name = "Công nghệ sử dụng")]
        public string? CongNgheSuDung { get; set; }

        [Display(Name = "Yêu cầu tính mới")]
        public string? YeuCauTinhMoi { get; set; }

        [Display(Name = "Sản phẩm kết quả dự kiến")]
        public string? SanPhamKetQuaDuKien { get; set; }

        // Sinh viên thực hiện
        [Display(Name = "MSSV thứ nhất")]
        public string? MssvSinhVien1 { get; set; }

        [Display(Name = "MSSV thứ hai")]
        public string? MssvSinhVien2 { get; set; }

        // Hidden
        public int? IdDot { get; set; }
        public string? TenDot { get; set; }
        public int? IdNguoiDeXuat { get; set; }

        // Dropdown lists
        public List<ChuyenNganhItem>? DanhSachChuyenNganh { get; set; }

        // Danh sách đề tài đã đề xuất
        public List<DeTaiItem>? DanhSachDeTai { get; set; }

        // Filter
        public string? NamHoc { get; set; }
        public int? HocKy { get; set; }
        public int? IdKhoaHoc { get; set; }
        public string? TuKhoaTimKiem { get; set; }
    }

    public class DeTaiItem
    {
        public int Id { get; set; }
        public string? MaDeTai { get; set; }
        public string? TenDeTai { get; set; }
        public string? TenChuyenNganh { get; set; }
        public string? TrangThai { get; set; }
        public string? TenNguoiDeXuat { get; set; }
        public DateTime? NgayTao { get; set; }
    }
}