using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DATN_TMS.Areas.BCNKhoa.Models
{
    public class QuanLyHoiDongBaoCaoEditViewModel
    {
        public int Id { get; set; }
        public string MaHoiDong { get; set; } = string.Empty;
        public string TenHoiDong { get; set; } = string.Empty;
        public string LoaiHoiDong { get; set; } = string.Empty;
        public int IdBoMon { get; set; }

        // Cập nhật đúng kiểu DateOnly và TimeOnly
        public DateOnly? NgayBaoCao { get; set; }
        public TimeOnly? ThoiGianDuKien { get; set; } // Mapping từ ThoiGianDuKien

        public string? DiaDiem { get; set; } = string.Empty;
        public bool TrangThai { get; set; }

        public List<ThanhVienHoiDongViewModel> ThanhViens { get; set; } = new List<ThanhVienHoiDongViewModel>();
        public List<DeTaiHoiDongViewModel> DeTais { get; set; } = new List<DeTaiHoiDongViewModel>();
    }

    public class DeTaiHoiDongViewModel
    {
        public int IdDeTai { get; set; }
        public string TenDeTai { get; set; } = string.Empty;
        public string MaDeTai { get; set; } = string.Empty;
        public string ChuyenNganh { get; set; } = string.Empty;
        public string GVHD { get; set; } = string.Empty;
        public List<SinhVienBaoVeViewModel> SinhViens { get; set; } = new List<SinhVienBaoVeViewModel>();
    }

    public class SinhVienBaoVeViewModel
    {
        public int IdPhienBaoVe { get; set; }
        public string MSSV { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string LopKhoa { get; set; } = string.Empty;
        public string ChuyenNganh { get; set; } = string.Empty;
    }

    public class ThanhVienHoiDongViewModel
    {
        public int IdNguoiDung { get; set; }
        public string MaGV { get; set; } = string.Empty;
        public string TenGiangVien { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public string HocVi { get; set; } = string.Empty;

        public string VaiTro { get; set; } = string.Empty;
    }
}