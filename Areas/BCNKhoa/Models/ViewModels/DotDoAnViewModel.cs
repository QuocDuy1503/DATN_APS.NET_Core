using System;
using System.ComponentModel.DataAnnotations;
using DATN_TMS.Models;

namespace DATN_TMS.Areas.BCNKhoa.Models.ViewModels
{
    public class DotDoAnViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đợt")]
        public string TenDot { get; set; } = string.Empty;

        public string Khoa { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ngày bắt đầu không được để trống")]
        public DateTime NgayBatDau { get; set; }

        [Required(ErrorMessage = "Ngày kết thúc không được để trống")]
        public DateTime NgayKetThuc { get; set; }
    }

    /// <summary>
    /// ViewModel hiển thị đợt đồ án với trạng thái động
    /// </summary>
    public class DotDoAnIndexItem
    {
        public int Id { get; set; }
        public string? TenDot { get; set; }
        public string? TenKhoaHoc { get; set; }
        public DateOnly? NgayBatDauDot { get; set; }
        public DateOnly? NgayKetThucDot { get; set; }
        public bool? TrangThaiActive { get; set; }

        // Trạng thái tính toán dựa trên giai đoạn
        public string TrangThaiText { get; set; } = "Chưa diễn ra";
        public string TrangThaiCss { get; set; } = "status-pending";
        public string TrangThaiCode { get; set; } = "CHUA_DIEN_RA";
    }

    /// <summary>
    /// Helper để tính toán trạng thái đợt đồ án
    /// </summary>
    public static class DotDoAnStatusHelper
    {
        /// <summary>
        /// Tính trạng thái động của đợt đồ án dựa trên các giai đoạn
        /// </summary>
        public static (string Code, string Text, string Css) GetTrangThai(DotDoAn dot)
        {
            var today = DateOnly.FromDateTime(DateTime.Now);

            // Ràng buộc 1: Chưa đến thời gian bắt đầu đợt
            if (!dot.NgayBatDauDot.HasValue || today < dot.NgayBatDauDot.Value)
            {
                return ("CHUA_DIEN_RA", "Chưa diễn ra", "status-pending");
            }

            // Ràng buộc 6: Đã hết đợt đồ án
            if (dot.NgayKetThucDot.HasValue && today > dot.NgayKetThucDot.Value)
            {
                return ("KET_THUC", "Kết thúc", "status-closed");
            }

            // Ràng buộc 2: Giai đoạn đăng ký nguyện vọng, duyệt nguyện vọng, đề xuất đề tài, đăng ký đề tài, duyệt đề tài
            // Kiểm tra nếu đang trong giai đoạn chuẩn bị
            bool trongGiaiDoanChuanBi = IsInPreparationPhase(dot, today);
            if (trongGiaiDoanChuanBi)
            {
                return ("CHUAN_BI", "Chuẩn bị", "status-preparing");
            }

            // Ràng buộc 5: Từ giai đoạn bắt đầu báo cáo cuối kì đến hết đợt đồ án
            if (dot.NgayBatDauBaoCaoCuoiKi.HasValue && today >= dot.NgayBatDauBaoCaoCuoiKi.Value)
            {
                return ("CUOI_KY", "Cuối kỳ", "status-final");
            }

            // Ràng buộc 4: Từ giai đoạn bắt đầu báo cáo giữa kì tới bắt đầu báo cáo cuối kì
            if (dot.NgayBatDauBaoCaoGiuaKi.HasValue && today >= dot.NgayBatDauBaoCaoGiuaKi.Value)
            {
                return ("GIUA_KY", "Giữa kỳ", "status-midterm");
            }

            // Ràng buộc 3: Từ giai đoạn kết thúc duyệt đề tài đến bắt đầu báo cáo giữa kì
            // Nếu đã qua giai đoạn chuẩn bị nhưng chưa đến giữa kỳ
            return ("BAT_DAU_THUC_HIEN", "Bắt đầu thực hiện", "status-running");
        }

        /// <summary>
        /// Kiểm tra có đang trong giai đoạn chuẩn bị không
        /// (Đăng ký nguyện vọng, duyệt nguyện vọng, đề xuất đề tài, đăng ký đề tài, duyệt đề tài)
        /// </summary>
        private static bool IsInPreparationPhase(DotDoAn dot, DateOnly today)
        {
            // Kiểm tra giai đoạn đăng ký nguyện vọng
            if (IsInDateRange(today, dot.NgayBatDauDkNguyenVong, dot.NgayKetThucDkNguyenVong))
                return true;

            // Kiểm tra giai đoạn duyệt nguyện vọng
            if (IsInDateRange(today, dot.NgayBatDauDkDuyetNguyenVong, dot.NgayKetThucDkDuyetNguyenVong))
                return true;

            // Kiểm tra giai đoạn đề xuất đề tài
            if (IsInDateRange(today, dot.NgayBatDauDeXuatDeTai, dot.NgayKetThucDeXuatDeTai))
                return true;

            // Kiểm tra giai đoạn duyệt đề xuất đề tài
            if (IsInDateRange(today, dot.NgayBatDauDuyetDeXuatDeTai, dot.NgayKetThucDuyetDeXuatDeTai))
                return true;

            // Kiểm tra giai đoạn đăng ký đề tài
            if (IsInDateRange(today, dot.NgayBatDauDkDeTai, dot.NgayKetThucDkDeTai))
                return true;

            // Kiểm tra nếu chưa kết thúc giai đoạn duyệt đề tài (dựa vào ngày kết thúc đăng ký đề tài)
            // Nếu ngày kết thúc đăng ký đề tài chưa qua thì vẫn đang chuẩn bị
            if (dot.NgayKetThucDkDeTai.HasValue && today <= dot.NgayKetThucDkDeTai.Value)
                return true;

            return false;
        }

        private static bool IsInDateRange(DateOnly today, DateOnly? start, DateOnly? end)
        {
            if (!start.HasValue || !end.HasValue) return false;
            return today >= start.Value && today <= end.Value;
        }
    }
}
