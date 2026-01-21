using System;
using System.Collections.Generic;

namespace DATN_TMS.Areas.BCNKhoa.Models
{
    public class QuanLyHoiDongEditViewModel
    {
        public int Id { get; set; }
        public string MaHoiDong { get; set; } = string.Empty;
        public string TenHoiDong { get; set; } = string.Empty;
        public int IdBoMon { get; set; }
        public DateOnly? NgayBatDau { get; set; }
        public DateOnly? NgayKetThuc { get; set; }
        public bool TrangThai { get; set; }

        public List<ThanhVienHoiDongViewModel> ThanhViens { get; set; } = new List<ThanhVienHoiDongViewModel>();
    }

    //public class ThanhVienHoiDongViewModel
    //{
    //    public int IdGiangVien { get; set; } 
    //    public string TenGiangVien { get; set; } = string.Empty;
    //    public string Email { get; set; } = string.Empty;
    //    public string MaGV { get; set; } = string.Empty; 
    //    public string VaiTro { get; set; } = string.Empty;
    //}
}