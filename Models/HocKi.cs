using System;
using System.Collections.Generic;

namespace DATN_TMS.Models;

public partial class HocKi
{
    public int Id { get; set; }

    public string? MaHocKi { get; set; }

    public int? NamBatDau { get; set; }

    public int? NamKetThuc { get; set; }

    public int? TuanBatDau { get; set; }

    public DateOnly? NgayBatDau { get; set; }

    public bool? TrangThai { get; set; }

    public virtual ICollection<DotDoAn> DotDoAns { get; set; } = new List<DotDoAn>();
}
