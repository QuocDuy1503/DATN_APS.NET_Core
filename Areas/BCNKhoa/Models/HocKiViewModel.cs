using System;

namespace DATN_TMS.Areas.BCNKhoa.Models
{
    public class HocKiViewModel
    {
        public int Id { get; set; }
        public string MaHocKi { get; set; } 
        public int? NamBatDau { get; set; }
        public int? NamKetThuc { get; set; }
        public DateOnly? NgayBatDau { get; set; }
        public bool? TrangThai { get; set; }

        /// <summary>
        /// Trạng thái tự động tính theo năm hiện tại:
        /// "Đang diễn ra" | "Chưa diễn ra" | "Đã kết thúc"
        /// </summary>
        public string TrangThaiText
        {
            get
            {
                int currentYear = DateTime.Now.Year;
                if (NamBatDau.HasValue && currentYear < NamBatDau.Value)
                    return "Chưa diễn ra";
                if (NamKetThuc.HasValue && currentYear > NamKetThuc.Value)
                    return "Đã kết thúc";
                return "Đang diễn ra";
            }
        }
    }
}