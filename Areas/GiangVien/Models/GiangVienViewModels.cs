using X.PagedList;

namespace DATN_TMS.Areas.GiangVien.Models
{
    // === QUẢN LÝ ĐĂNG KÝ ĐỀ TÀI (Duyệt SV đăng ký) ===
    public class DangKyDeTaiGVItem
    {
        public int IdDeTai { get; set; }
        public string? MaDeTai { get; set; }
        public string? TenDeTai { get; set; }
        public string? ChuyenNganh { get; set; }
        public int SoLuongDangKy { get; set; }
        public List<SinhVienDangKyItem> DanhSachSV { get; set; } = new();
    }

    public class SinhVienDangKyItem
    {
        public int IdSvDeTai { get; set; }
        public string? Mssv { get; set; }
        public string? HoTen { get; set; }
        public string? KhoaHoc { get; set; }
        public string? TrangThai { get; set; }
        public string? NgayDangKy { get; set; }
    }

    // === DANH SÁCH SINH VIÊN ===
    public class SinhVienGVItem
    {
        public string? Mssv { get; set; }
        public string? HoTen { get; set; }
        public string? Email { get; set; }
        public string? ChuyenNganh { get; set; }
        public string? KhoaHoc { get; set; }
        public string? TenDeTai { get; set; }
        public string? TrangThai { get; set; }
    }

    // === BẢNG KẾ HOẠCH SINH VIÊN ===
    public class KeHoachSVGVItem
    {
        public int Id { get; set; }
        public int? Stt { get; set; }
        public string? TenCongViec { get; set; }
        public string? NguoiThucHien { get; set; }
        public string? TenDeTai { get; set; }
        public string? NgayBatDau { get; set; }
        public string? NgayKetThuc { get; set; }
        public string? NgayBDThucTe { get; set; }
        public string? NgayKTThucTe { get; set; }
        public string? TrangThai { get; set; }
        public string? StatusCss { get; set; }
        public string? StatusText { get; set; }
    }

    // === NỘP BÁO CÁO ===
    public class NopBaoCaoGVItem
    {
        public int Id { get; set; }
        public int? Stt { get; set; }
        public string? TenBaoCao { get; set; }
        public string? TenSinhVien { get; set; }
        public string? Mssv { get; set; }
        public string? TenDeTai { get; set; }
        public string? NgayNop { get; set; }
        public string? TrangThai { get; set; }
        public string? LoaiBaoCao { get; set; }
        public string? FilePath { get; set; }
        public string? GhiChuGui { get; set; }
    }

    // === ĐÁNH GIÁ KẾT QUẢ ĐỀ TÀI ===
    public class DanhGiaKetQuaIndexViewModel
    {
        public string? TenDot { get; set; }
        public bool CoDot { get; set; }
        public List<DanhGiaKetQuaGVItem> DanhSachDeTai { get; set; } = new();
    }

    public class DanhGiaKetQuaGVItem
    {
        public int IdDeTai { get; set; }
        public string? MaDeTai { get; set; }
        public string? TenDeTai { get; set; }
        public string? ChuyenNganh { get; set; }
        public int SoLuongSV { get; set; }

        // Trạng thái chấm điểm
        public bool DaChamDiem { get; set; }
        public string? TrangThaiText { get; set; } // "Đã nhập điểm" hoặc "Chưa nhập điểm"
        public string? TrangThaiCss { get; set; }

        // Tài liệu báo cáo cuối kỳ
        public bool CoBaoCaoCuoiKyDuyet { get; set; }
        public string? FileBaoCaoCuoiKy { get; set; }
        public string? TenFileBaoCao { get; set; }

        public List<SinhVienDGItem> DanhSachSV { get; set; } = new();
    }

    public class SinhVienDGItem
    {
        public int IdSinhVien { get; set; }
        public string? Mssv { get; set; }
        public string? HoTen { get; set; }
        public string? KhoaHoc { get; set; }
        public double? DiemGVHD { get; set; } // Điểm GVHD chấm
        public bool DaChamDiem { get; set; }
    }

    // === NHẬT KÝ HƯỚNG DẪN ===
    public class NhatKyHuongDanGVItem
    {
        public int Id { get; set; }
        public string? TenGvhd { get; set; }
        public string? NgayHop { get; set; }
        public string? ThoiGian { get; set; }
        public string? HinhThucHop { get; set; }
        public string? DiaDiem { get; set; }
        public string? NoiDung { get; set; }
        public string? MucTieu { get; set; }
        public string? ThanhVien { get; set; }
        public string? ActionListJson { get; set; }
        public int TaskCount { get; set; }
    }

    // === CHI TIẾT ĐỀ TÀI - DUYỆT ĐĂNG KÝ ===
    public class ChiTietDangKyDeTaiGVViewModel
    {
        public int IdDeTai { get; set; }
        public string? MaDeTai { get; set; }
        public string? TenDeTai { get; set; }
        public string? TenChuyenNganh { get; set; }
        public string? NguoiDeXuat { get; set; }
        public string? MucTieu { get; set; }
        public string? YeuCauTinhMoi { get; set; }
        public string? PhamVi { get; set; }
        public string? CongNghe { get; set; }
        public string? KetQuaDuKien { get; set; }
        public string? NhiemVuCuThe { get; set; }
        public int SoLuongNhom { get; set; } // Số nhóm đăng ký
        public int SoLuongDaDuyet { get; set; }
        public bool CoTheDuyet { get; set; }
        public string? ThongBaoGiaiDoan { get; set; }
        public List<NhomDangKyGVItem> DanhSachNhom { get; set; } = new();
    }

    // Nhóm đăng ký (1 hoặc 2 SV)
    public class NhomDangKyGVItem
    {
        // SV 1 (bắt buộc)
        public int IdSvDeTai1 { get; set; }
        public int? IdSinhVien1 { get; set; }
        public string? Mssv1 { get; set; }
        public string? HoTen1 { get; set; }
        public string? Email1 { get; set; }
        public string? KhoaHoc1 { get; set; }
        public double? TinChiTichLuy1 { get; set; }
        public double? GPA1 { get; set; }

        // SV 2 (không bắt buộc)
        public int? IdSvDeTai2 { get; set; }
        public int? IdSinhVien2 { get; set; }
        public string? Mssv2 { get; set; }
        public string? HoTen2 { get; set; }
        public string? Email2 { get; set; }
        public string? KhoaHoc2 { get; set; }
        public double? TinChiTichLuy2 { get; set; }
        public double? GPA2 { get; set; }

        public string? TrangThai { get; set; }
        public string? StatusCss { get; set; }
        public string? NgayDangKy { get; set; }
        public string? NhanXet { get; set; }
    }

    // Giữ lại cho backward compatibility
    public class SinhVienDangKyDetailGVItem
    {
        public int IdSvDeTai { get; set; }
        public string? Mssv { get; set; }
        public string? HoTen { get; set; }
        public string? Email { get; set; }
        public string? KhoaHoc { get; set; }
        public string? TrangThai { get; set; }
        public string? StatusCss { get; set; }
        public string? NgayDangKy { get; set; }
        public string? NhanXet { get; set; }
    }

    // === BẢNG KẾ HOẠCH SINH VIÊN - INDEX ===
    public class BangKeHoachSVIndexViewModel
    {
        public string? TenDot { get; set; }
        public string? HocKi { get; set; }
        public bool CoDot { get; set; }
        public List<DeTaiKeHoachItem> DanhSachDeTai { get; set; } = new();
    }

    public class DeTaiKeHoachItem
    {
        public int IdDeTai { get; set; }
        public string? MaDeTai { get; set; }
        public string? TenDeTai { get; set; }
        public int SoLuongThanhVien { get; set; }
        public int SoLuongCongViec { get; set; }
        public int SoLuongChoXacNhan { get; set; }
        public int SoLuongDaHoanThanh { get; set; }
        public double TienDo { get; set; } // Phần trăm tiến độ
        public string? TienDoCss { get; set; }
        public List<string> DanhSachSinhVien { get; set; } = new();
    }

    // === BẢNG KẾ HOẠCH SINH VIÊN - DETAIL ===
    public class BangKeHoachSVDetailViewModel
    {
        public int IdDeTai { get; set; }
        public string? MaDeTai { get; set; }
        public string? TenDeTai { get; set; }
        public string? TenDot { get; set; }

        // Thống kê tiến độ
        public int TongCongViec { get; set; }
        public int ChuaThucHien { get; set; }
        public int DangThucHien { get; set; }
        public int ChoXacNhan { get; set; }
        public int DaHoanThanh { get; set; }
        public double TienDoTongQuat { get; set; } // %

        // Danh sách sinh viên
        public List<SinhVienKeHoachItem> DanhSachSinhVien { get; set; } = new();

        // Danh sách công việc (có phân trang)
        public IPagedList<CongViecKeHoachItem>? DanhSachCongViec { get; set; }
    }

    public class SinhVienKeHoachItem
    {
        public int IdSinhVien { get; set; }
        public string? Mssv { get; set; }
        public string? HoTen { get; set; }
        public string? Email { get; set; }
        public int SoCongViec { get; set; }
        public double TienDo { get; set; }
    }

    public class CongViecKeHoachItem
    {
        public int Id { get; set; }
        public int? Stt { get; set; }
        public string? TenCongViec { get; set; }
        public string? MoTa { get; set; }
        public string? NguoiThucHien { get; set; }
        public string? Mssv { get; set; }
        public string? NgayBatDau { get; set; }
        public string? NgayKetThuc { get; set; }
        public string? NgayBDThucTe { get; set; }
        public string? NgayKTThucTe { get; set; }
        public string? TrangThai { get; set; }
        public string? StatusCss { get; set; }
        public string? StatusText { get; set; }
        public string? GhiChu { get; set; }
        public bool CanDuyet { get; set; }
        public List<FileMinhChungItem> DanhSachMinhChung { get; set; } = new();
    }

    public class FileMinhChungItem
    {
        public int Id { get; set; }
        public string? TenFile { get; set; }
        public string? DuongDan { get; set; }
        public string? LoaiFile { get; set; }
        public string? NgayNop { get; set; }
    }

    public class DuyetKeHoachRequest
    {
        public int Id { get; set; }
        public string? Action { get; set; }
        public string? NhanXet { get; set; }
    }

    // === CHI TIẾT CÔNG VIỆC - DETAIL TASK ===
    public class DetailTaskViewModel
    {
        public int Id { get; set; }
        public int IdDeTai { get; set; }
        public int? Stt { get; set; }
        public string? TenCongViec { get; set; }
        public string? MoTaCongViec { get; set; }
        public string? NguoiPhuTrach { get; set; }
        public string? TenDot { get; set; }
        public string? NgayBatDau { get; set; }
        public string? NgayKetThuc { get; set; }
        public string? NgayBatDauThucTe { get; set; }
        public string? NgayKetThucThucTe { get; set; }
        public string? TrangThai { get; set; }
        public string? StatusCss { get; set; }
        public string? StatusText { get; set; }
        public string? GhiChu { get; set; }
        public string? NhanXetGiangVien { get; set; }
        public bool CanDuyet { get; set; }
        public List<FileMinhChungItem> DanhSachFileMinhChung { get; set; } = new();
    }

    // === QUẢN LÝ NỘP BÁO CÁO - INDEX ===
    public class QuanLyNopBaoCaoIndexViewModel
    {
        public string? TenDot { get; set; }
        public string? HocKi { get; set; }
        public bool CoDot { get; set; }
        public List<DeTaiBaoCaoItem> DanhSachDeTai { get; set; } = new();
    }

    public class DeTaiBaoCaoItem
    {
        public int IdDeTai { get; set; }
        public string? MaDeTai { get; set; }
        public string? TenDeTai { get; set; }
        public string? TenChuyenNganh { get; set; }
        public int SoSinhVien { get; set; }
        public string? DanhSachSinhVien { get; set; } // "SV1, SV2"

        // Thống kê báo cáo
        public int SoBaoCaoDeCuong { get; set; }
        public int SoBaoCaoGiuaKy { get; set; }
        public int SoBaoCaoCuoiKy { get; set; }
        public int TongBaoCaoChoDuyet { get; set; }
        public string? TrangThaiTongQuat { get; set; } // "Chờ duyệt", "Đã duyệt hết", "Chưa nộp"
        public string? TrangThaiCss { get; set; }
    }

    // === QUẢN LÝ NỘP BÁO CÁO - DETAIL ===
    public class QuanLyNopBaoCaoDetailViewModel
    {
        public int IdDeTai { get; set; }
        public string? MaDeTai { get; set; }
        public string? TenDeTai { get; set; }
        public string? TenChuyenNganh { get; set; }
        public string? TenDot { get; set; }

        // Danh sách sinh viên trong đề tài
        public List<SinhVienBaoCaoItem> DanhSachSinhVien { get; set; } = new();

        // Tất cả báo cáo (nhóm theo loại)
        public List<BaoCaoNopGVDetailItem> BaoCaoDeCuong { get; set; } = new();
        public List<BaoCaoNopGVDetailItem> BaoCaoGiuaKy { get; set; } = new();
        public List<BaoCaoNopGVDetailItem> BaoCaoCuoiKy { get; set; } = new();

        // Deadline
        public string? DeadlineDeCuong { get; set; }
        public string? DeadlineGiuaKy { get; set; }
        public string? DeadlineCuoiKy { get; set; }
    }

    public class SinhVienBaoCaoItem
    {
        public int IdSinhVien { get; set; }
        public string? Mssv { get; set; }
        public string? HoTen { get; set; }
        public string? Email { get; set; }
    }

    public class BaoCaoNopGVDetailItem
    {
        public int IdBaoCao { get; set; }
        public int? IdSinhVien { get; set; }
        public string? TenSinhVien { get; set; }
        public string? Mssv { get; set; }
        public string? TenBaoCao { get; set; }
        public string? FilePath { get; set; }
        public string? TenFile { get; set; }
        public string? NgayNop { get; set; }
        public string? GhiChuGui { get; set; }
        public string? NhanXet { get; set; }
        public string? TrangThai { get; set; }
        public string? TrangThaiText { get; set; }
        public string? TrangThaiCss { get; set; }
        public int? PhienBan { get; set; } // Version number
        public bool LaBanMoiNhat { get; set; }
    }
}
