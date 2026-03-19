using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using X.PagedList;

namespace DATN_TMS.Areas.BCNKhoa.Models
{
    public class BaoCaoThongKeViewModel
    {
        public string? SelectedDotId { get; set; }
        public List<SelectListItem> DotOptions { get; set; } = new();

        public int TongSinhVien { get; set; }
        public int TongDeTai { get; set; }
        public double TienDoPhanTram { get; set; }

        public IPagedList<DeTaiSummaryItem> DeTaiSummaries { get; set; } = new PagedList<DeTaiSummaryItem>(new List<DeTaiSummaryItem>(), 1, 10);
    }

    public class DeTaiSummaryItem
    {
        public string MaDeTai { get; set; } = string.Empty;
        public string TenDeTai { get; set; } = string.Empty;
        public string SinhVien { get; set; } = string.Empty;
        public string TrangThai { get; set; } = string.Empty;
        public int TaskDone { get; set; }
        public int TaskTotal { get; set; }
        public double TienDoPhanTram => TaskTotal == 0 ? 0 : (double)TaskDone / TaskTotal * 100;
    }
}
