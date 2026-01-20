using System;
using System.Collections.Generic;

namespace DATN_TMS.Models;

public partial class ChuyenNganh
{
    public int Id { get; set; }

    public int? Stt { get; set; }

    public string? TenChuyenNganh { get; set; }

    public string? TenVietTat { get; set; }

    public int? IdNguoiTao { get; set; }

    public DateOnly? NgayTao { get; set; }

    public int? IdNguoiSua { get; set; }

    public DateOnly? NgaySua { get; set; }

    public int? IdNganh { get; set; }

    public int? IdBoMon { get; set; }

    public virtual ICollection<DeTai> DeTais { get; set; } = new List<DeTai>();

    public virtual BoMon? IdBoMonNavigation { get; set; }

    public virtual Nganh? IdNganhNavigation { get; set; }

    public virtual ICollection<SinhVien> SinhViens { get; set; } = new List<SinhVien>();
}
