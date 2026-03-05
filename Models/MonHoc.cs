using System;
using System.Collections.Generic;

namespace DATN_TMS.Models;

public partial class MonHoc
{
    public int Id { get; set; }

    public int? IdKhoiKienThuc { get; set; }

    public int? IdCtdt { get; set; }

    public string? MaMon { get; set; }

    public string? TenMon { get; set; }

    public int? SoTinChi { get; set; }

    public string? LoaiHocPhan { get; set; }

    public string? DieuKienTienQuyet { get; set; }

    public int? HocKiToChuc { get; set; }

    public string? GhiChu { get; set; }

    public virtual ChuongTrinhDaoTao? IdCtdtNavigation { get; set; }

    public virtual KhoiKienThuc? IdKhoiKienThucNavigation { get; set; }
}
