using System;
using System.Collections.Generic;

namespace DATN_TMS.Areas.BCNKhoa.Models
{
    public class ChiTietDeTaiViewModel
    {
        public int Id { get; set; }
        public string MaDeTai { get; set; }
        public string TenDeTai { get; set; }
        public string NguoiDeXuat { get; set; }
        public string GVHD { get; set; }
        public string TenChuyenNganh { get; set; }

        public string MucTieu { get; set; }
        public string PhamVi { get; set; }
        public string CongNghe { get; set; }
        public string YeuCauTinhMoi { get; set; }
        public string KetQuaDuKien { get; set; }

  
        public string NhomThucHien { get; set; }


        public string TrangThai { get; set; }
        public string NhanXet { get; set; }

        public bool CanReview { get; set; }
        public bool IsProposer { get; set; }
        public string CurrentUserStatus { get; set; }
        public int CouncilMemberCount { get; set; }
        public List<ReviewItem> Reviews { get; set; } = new();
    }

    public class ReviewItem
    {
        public string ReviewerName { get; set; }
        public string? Comment { get; set; }
        public string Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool IsCurrentUser { get; set; }
    }
}
