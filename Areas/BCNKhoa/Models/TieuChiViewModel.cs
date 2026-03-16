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
        public int? SelectedLoaiPhieuId { get; set; }
        public bool ChiNhanXet { get; set; }
        public IEnumerable<SelectListItem>? LoaiPhieuOptions { get; set; }
        public List<TieuChiItem> TieuChis { get; set; } = new();

        /// <summary>
        /// Đang trong giai đoạn chấm điểm (giữa kì hoặc cuối kì) - không cho phép chỉnh sửa
        /// </summary>
        public bool DangTrongGiaiDoanChamDiem { get; set; }

        /// <summary>
        /// Thông báo ràng buộc khi đang trong giai đoạn chấm điểm
        /// </summary>
        public string? ThongBaoRangBuoc { get; set; }

        public double TongTrongSo => TieuChis?.Sum(x => x.TrongSo ?? 0) ?? 0;

        /// <summary>
        /// Cho phép chỉnh sửa tiêu chí (không trong giai đoạn chấm điểm)
        /// </summary>
        public bool ChoPhepChinhSua => !DangTrongGiaiDoanChamDiem;
    }
}
