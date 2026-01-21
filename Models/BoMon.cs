using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DATN_TMS.Models;

public partial class BoMon
{
    public int Id { get; set; }

    public int? Stt { get; set; }

    public string? TenBoMon { get; set; }

    public string? TenVietTat { get; set; }

    public int? IdNguoiTao { get; set; }

    public DateOnly? NgayTao { get; set; }

    public int? IdNguoiSua { get; set; }

    public DateOnly? NgaySua { get; set; }

    public virtual ICollection<ChuyenNganh> ChuyenNganhs { get; set; } = new List<ChuyenNganh>();

    public virtual ICollection<GiangVien> GiangViens { get; set; } = new List<GiangVien>();

    public virtual ICollection<HoiDongBaoCao> HoiDongBaoCaos { get; set; } = new List<HoiDongBaoCao>();

    public virtual ICollection<Nganh> Nganhs { get; set; } = new List<Nganh>();
    [ForeignKey("IdNguoiTao")]
    public virtual NguoiDung? IdNguoiTaoNavigation { get; set; }

    [ForeignKey("IdNguoiSua")]
    public virtual NguoiDung? IdNguoiSuaNavigation { get; set; }
}
