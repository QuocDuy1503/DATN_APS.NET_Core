using System;
using System.Collections.Generic;

namespace DATN_TMS.Models;

public partial class ChiTietCtdt
{
    public int Id { get; set; }

    public int? IdCtdt { get; set; }

    public int? Stt { get; set; }

    public string? MaHocPhan { get; set; }

    public string? TenHocPhan { get; set; }

    public int? SoTinChi { get; set; }

    public string? LoaiHocPhan { get; set; }

    public string? DieuKienTienQuyet { get; set; }

    public int? HocKiToChuc { get; set; }

    public virtual ChuongTrinhDaoTao? IdCtdtNavigation { get; set; }
}
