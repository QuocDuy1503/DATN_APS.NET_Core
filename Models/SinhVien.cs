using System;
using System.Collections.Generic;

namespace DATN_TMS.Models;

public partial class SinhVien
{
    public int IdNguoiDung { get; set; }

    public string? Mssv { get; set; }

    public int? IdChuyenNganh { get; set; }

    public int? IdKhoaHoc { get; set; }

    public double? TinChiTichLuy { get; set; }

    public virtual ICollection<BaoCaoNop> BaoCaoNops { get; set; } = new List<BaoCaoNop>();

    public virtual ICollection<DangKyNguyenVong> DangKyNguyenVongs { get; set; } = new List<DangKyNguyenVong>();

    public virtual ICollection<DiemChiTiet> DiemChiTiets { get; set; } = new List<DiemChiTiet>();

    public virtual ICollection<DonPhucKhao> DonPhucKhaos { get; set; } = new List<DonPhucKhao>();

    public virtual ChuyenNganh? IdChuyenNganhNavigation { get; set; }

    public virtual KhoaHoc? IdKhoaHocNavigation { get; set; }

    public virtual NguoiDung IdNguoiDungNavigation { get; set; } = null!;

    public virtual ICollection<KeHoachCongViec> KeHoachCongViecs { get; set; } = new List<KeHoachCongViec>();

    public virtual ICollection<KetQuaBaoVeSinhVien> KetQuaBaoVeSinhViens { get; set; } = new List<KetQuaBaoVeSinhVien>();

    public virtual ICollection<KetQuaHocTap> KetQuaHocTaps { get; set; } = new List<KetQuaHocTap>();

    public virtual ICollection<SinhVienDeTai> SinhVienDeTais { get; set; } = new List<SinhVienDeTai>();
}
