using System;

namespace DATN_TMS.Areas.GV_BoMon.Models
{
    public class QuanLyHoiDongViewModel
    {
        public int Id { get; set; }
        public string MaHoiDong { get; set; } = string.Empty;
        public string TenHoiDong { get; set; } = string.Empty;
        public string? TenBoMon { get; set; } = string.Empty;
        public string? NguoiTao { get; set; } = string.Empty;
        public int IdBoMon { get; set; }
        public DateTime? NgayBaoCao { get; set; }
        public DateTime? NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }
    }
}