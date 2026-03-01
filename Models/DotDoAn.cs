

namespace DATN_TMS.Models;

public partial class DotDoAn
{
    public int Id { get; set; }

    public string? TenDot { get; set; }

    public int? IdKhoaHoc { get; set; }

    public int? IdHocKi { get; set; }

    public DateOnly? NgayBatDauDot { get; set; }

    public DateOnly? NgayKetThucDot { get; set; }

    public DateOnly? NgayBatDauDkNguyenVong { get; set; }

    public DateOnly? NgayKetThucDkNguyenVong { get; set; }

    public DateOnly? NgayBatDauDkDuyetNguyenVong { get; set; }

    public DateOnly? NgayKetThucDkDuyetNguyenVong { get; set; }

    public DateOnly? NgayBatDauDeXuatDeTai { get; set; }

    public DateOnly? NgayKetThucDeXuatDeTai { get; set; }

    public DateOnly? NgayBatDauDuyetDeXuatDeTai { get; set; }

    public DateOnly? NgayKetThucDuyetDeXuatDeTai { get; set; }

    public int? NgayLapHoiDongDuyetDxdt { get; set; }

    public int? NgayDuyetDxdt { get; set; }

    public DateOnly? NgayBatDauNopDeCuong { get; set; }

    public DateOnly? NgayKetThucNopDeCuong { get; set; }

    public DateOnly? NgayBatDauBaoCaoCuoiKi { get; set; }

    public DateOnly? NgayKetThucBaoCaoCuoiKi { get; set; }

    public int? NgayLapHdBcck { get; set; }

    public int? NgayNopTaiLieuBcck { get; set; }

    public int? NgayCongBoKqBcck { get; set; }

    public DateOnly? NgayBatDauBaoCaoGiuaKi { get; set; }

    public DateOnly? NgayKetThucBaoCaoGiuaKi { get; set; }

    public int? NgayLapHdBcgk { get; set; }

    public int? NgayNopTaiLieuBcgk { get; set; }

    public bool? TrangThai { get; set; }

    public virtual ICollection<BaoCaoNop> BaoCaoNops { get; set; } = new List<BaoCaoNop>();

    public virtual ICollection<BaoCaoThongKe> BaoCaoThongKes { get; set; } = new List<BaoCaoThongKe>();

    public virtual ICollection<CauHinhPhieuChamDot> CauHinhPhieuChamDots { get; set; } = new List<CauHinhPhieuChamDot>();

    public virtual ICollection<CauHinhThongBao> CauHinhThongBaos { get; set; } = new List<CauHinhThongBao>();

    public virtual ICollection<DangKyNguyenVong> DangKyNguyenVongs { get; set; } = new List<DangKyNguyenVong>();

    public virtual ICollection<DeTai> DeTais { get; set; } = new List<DeTai>();

    public virtual ICollection<DonPhucKhao> DonPhucKhaos { get; set; } = new List<DonPhucKhao>();

    public virtual ICollection<HoiDongBaoCao> HoiDongBaoCaos { get; set; } = new List<HoiDongBaoCao>();

    public virtual ICollection<KeHoachCongViec> KeHoachCongViecs { get; set; } = new List<KeHoachCongViec>();

    public virtual HocKi? IdHocKiNavigation { get; set; }

    public virtual KhoaHoc? IdKhoaHocNavigation { get; set; }

    public virtual ICollection<NhatKyHuongDan> NhatKyHuongDans { get; set; } = new List<NhatKyHuongDan>();
}
