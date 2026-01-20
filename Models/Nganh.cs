using System;
using System.Collections.Generic;

namespace DATN_TMS.Models;

public partial class Nganh
{
    public int Id { get; set; }

    public string MaNganh { get; set; } = null!;

    public string? TenNganh { get; set; }

    public string? TenVietTat { get; set; }

    public int? IdNguoiTao { get; set; }

    public DateOnly? NgayTao { get; set; }

    public int? IdNguoiSua { get; set; }

    public DateOnly? NgaySua { get; set; }

    public int? IdBoMon { get; set; }

    public virtual ICollection<ChuongTrinhDaoTao> ChuongTrinhDaoTaos { get; set; } = new List<ChuongTrinhDaoTao>();

    public virtual ICollection<ChuyenNganh> ChuyenNganhs { get; set; } = new List<ChuyenNganh>();

    public virtual BoMon? IdBoMonNavigation { get; set; }
}
