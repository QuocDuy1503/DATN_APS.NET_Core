using DATN_TMS.Areas.GiangVien.Models;

namespace DATN_TMS.Services
{
    /// <summary>
    /// Service xử lý logic nghiệp vụ chấm điểm báo cáo - dùng chung cho tất cả Area (GiangVien, BCNKhoa, GV_BoMon)
    /// </summary>
    public interface IChamDiemBaoCaoService
    {
        Task<int> GetCurrentUserId(string? userEmail);
        Task<bool> CheckCoHoiDong(int currentUserId);
        Task<List<HoiDongChamDiemViewModel>> GetDanhSachHoiDong(int currentUserId);
        Task<ChiTietHoiDongChamDiemViewModel?> GetChiTietHoiDong(int hoiDongId, int currentUserId);
        Task<ChamDiemPhienBaoVeViewModel?> GetChamDiemViewModel(int phienBaoVeId, int currentUserId);
        Task<LuuDiemResult> LuuDiem(LuuDiemRequest request, int currentUserId);
        Task<LuuDiemResult> ThuKyDieuChinhDiem(DieuChinhDiemRequest request, int currentUserId);
        Task<LuuDiemResult> ChuTichXacNhan(XacNhanDiemRequest request, int currentUserId);
        Task<BangDiemTongHopViewModel?> GetBangDiemTongHop(int phienBaoVeId, int currentUserId);
        string? GetValidationError(string key);
    }

    /// <summary>
    /// Kết quả trả về từ các thao tác lưu điểm
    /// </summary>
    public class LuuDiemResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public string? RedirectUrl { get; set; }
    }
}
