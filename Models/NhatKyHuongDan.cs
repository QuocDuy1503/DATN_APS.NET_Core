using System;
using System.Collections.Generic;

namespace DATN_TMS.Models;

public partial class NhatKyHuongDan
{
    public int Id { get; set; }

    public int? IdDot { get; set; }

    public DateOnly? NgayHop { get; set; }

    public bool? HinhThucHop { get; set; }

    public TimeOnly? ThoiGianHop { get; set; }

    public string? DiaDiemHop { get; set; }

    public int? IdKeHoachCongViec { get; set; }

    public virtual DotDoAn? IdDotNavigation { get; set; }

    public virtual KeHoachCongViec? IdKeHoachCongViecNavigation { get; set; }
}
