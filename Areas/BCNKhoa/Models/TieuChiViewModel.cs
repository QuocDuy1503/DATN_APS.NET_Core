using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DATN_TMS.Areas.BCNKhoa.Models
{
    public class TieuChiItem
    {
        public int Id { get; set; }
        public int? Stt { get; set; }
        public string? TenTieuChi { get; set; }
        public string? MoTa { get; set; }
        public double? TrongSo { get; set; }
        public double? DiemToiDa { get; set; }
    }

    public class TieuChiViewModel
    {
        public int? SelectedDotId { get; set; }
        public int? SelectedLoaiPhieuId { get; set; }
        public IEnumerable<SelectListItem>? LoaiPhieuOptions { get; set; }
        public IEnumerable<SelectListItem>? DotOptions { get; set; }
        public List<TieuChiItem> TieuChis { get; set; } = new();

        public double TongTrongSo => TieuChis?.Sum(x => x.TrongSo ?? 0) ?? 0;
    }
}
