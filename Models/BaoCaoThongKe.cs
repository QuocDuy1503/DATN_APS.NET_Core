using System;
using System.Collections.Generic;

namespace DATN_TMS.Models;

public partial class BaoCaoThongKe
{
    public int Id { get; set; }

    public string? TenBaoCao { get; set; }

    public int? IdDot { get; set; }

    public DateTime? NgayTao { get; set; }

    public string? DuLieuJson { get; set; }

    public virtual DotDoAn? IdDotNavigation { get; set; }
}
