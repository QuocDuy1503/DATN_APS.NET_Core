using System;

namespace DATN_TMS.Areas.BCNKhoa.Models
{
    public class NganhViewModel
    {
        public int Id { get; set; }

        public string MaNganh { get; set; }

        public string TenNganh { get; set; }

        public string TenBoMon { get; set; } 

        public string NguoiTao { get; set; }

        public DateOnly? NgayTao { get; set; }

        public string NguoiSua { get; set; }

        public DateOnly? NgaySua { get; set; }
    }
}