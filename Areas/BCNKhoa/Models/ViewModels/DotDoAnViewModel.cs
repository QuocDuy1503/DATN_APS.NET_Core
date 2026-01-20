using System;
using System.ComponentModel.DataAnnotations;

namespace DATN_TMS.Areas.BCNKhoa.Models.ViewModels
{
    public class DotDoAnViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đợt")]
        public string TenDot { get; set; }

        public string Khoa { get; set; }

        [Required(ErrorMessage = "Ngày bắt đầu không được để trống")]
        public DateTime NgayBatDau { get; set; }

        [Required(ErrorMessage = "Ngày kết thúc không được để trống")]
        public DateTime NgayKetThuc { get; set; }
    }
}
