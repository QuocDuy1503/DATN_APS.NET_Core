using System;
using System.Collections.Generic;

namespace DATN_TMS.Models;

public partial class KhoiKienThuc
{
    public int Id { get; set; }

    public int? IdCtdt { get; set; }

    public string? TenKhoi { get; set; }

    public int? TongTinChi { get; set; }

    public string? GhiChu { get; set; }

    public virtual ChuongTrinhDaoTao? IdCtdtNavigation { get; set; }

    public virtual ICollection<MonHoc> MonHocs { get; set; } = new List<MonHoc>();
}
