using System.ComponentModel.DataAnnotations;

namespace DATN_TMS.Areas.BCNKhoa.Models
{
    // ViewModel hiển thị danh sách người dùng
    public class QuanLyNguoiDungViewModel
    {
        public int Id { get; set; }
        public string? MaSo { get; set; } // MSSV hoặc MaGV
        public string? HoTen { get; set; }
        public string? Email { get; set; }
        public string? Sdt { get; set; }
        public string? AvatarUrl { get; set; }
        public string? LoaiNguoiDung { get; set; } // "Sinh viên", "Giảng viên"
        public List<string> VaiTros { get; set; } = new List<string>(); // Các vai trò
        public string? HocVi { get; set; } // Cho giảng viên
        public string? TenBoMon { get; set; } // Cho giảng viên
        public string? TenChuyenNganh { get; set; } // Cho sinh viên
        public string? TenKhoaHoc { get; set; } // Cho sinh viên
        public int TrangThai { get; set; } // 0: Inactive, 1: Active
    }

    // ViewModel cho form thêm/sửa người dùng
    public class NguoiDungFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [StringLength(100)]
        public string HoTen { get; set; } = null!;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = null!;

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? Sdt { get; set; }

        public string? AvatarUrl { get; set; }

        public int TrangThai { get; set; } = 1;

        // Loại người dùng: "SINH_VIEN" hoặc "GIANG_VIEN"
        [Required(ErrorMessage = "Vui lòng chọn loại người dùng")]
        public string LoaiNguoiDung { get; set; } = null!;

        // Vai trò được chọn (cho edit - có thể nhiều)
        public List<int> SelectedVaiTroIds { get; set; } = new List<int>();

        // Vai trò đơn (cho create giảng viên - chỉ 1)
        public int? SelectedVaiTroId { get; set; }

        // === Thông tin Sinh viên ===
        [StringLength(20)]
        public string? Mssv { get; set; }
        public int? IdChuyenNganh { get; set; }
        public int? IdKhoaHoc { get; set; }
        public double? TinChiTichLuy { get; set; }

        // === Thông tin Giảng viên ===
        [StringLength(20)]
        public string? MaGv { get; set; }
        public string? HocVi { get; set; }
        public int? IdBoMon { get; set; }

        // === Thông tin thêm (để tương lai mở rộng) ===
        public DateOnly? NgaySinh { get; set; }
        public string? DiaChi { get; set; }
        public string? GioiTinh { get; set; }
    }

    // ViewModel cho Edit - chứa đầy đủ thông tin
    public class NguoiDungEditViewModel
    {
        public int Id { get; set; }
        public string? HoTen { get; set; }
        public string? Email { get; set; }
        public string? Sdt { get; set; }
        public string? AvatarUrl { get; set; }
        public int TrangThai { get; set; }

        // Loại người dùng
        public string? LoaiNguoiDung { get; set; }
        public bool IsSinhVien { get; set; }
        public bool IsGiangVien { get; set; }

        // Vai trò
        public List<int> SelectedVaiTroIds { get; set; } = new List<int>();
        public List<VaiTroItem> AllVaiTros { get; set; } = new List<VaiTroItem>();

        // Thông tin Sinh viên
        public string? Mssv { get; set; }
        public int? IdChuyenNganh { get; set; }
        public int? IdKhoaHoc { get; set; }
        public double? TinChiTichLuy { get; set; }
        public string? TenChuyenNganh { get; set; }
        public string? TenKhoaHoc { get; set; }

        // Thông tin Giảng viên
        public string? MaGv { get; set; }
        public string? HocVi { get; set; }
        public int? IdBoMon { get; set; }
        public string? TenBoMon { get; set; }
    }

    public class VaiTroItem
    {
        public int Id { get; set; }
        public string? MaVaiTro { get; set; }
        public string? TenVaiTro { get; set; }
        public bool IsSelected { get; set; }
    }

    // Request model cho API
    public class ToggleStatusRequest
    {
        public int UserId { get; set; }
        public int Status { get; set; }
    }

    public class DeleteUserRequest
    {
        public int UserId { get; set; }
    }
}