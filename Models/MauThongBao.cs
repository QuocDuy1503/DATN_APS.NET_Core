using System;
using System.Collections.Generic;

namespace DATN_TMS.Models;

public partial class MauThongBao
{
    public int Id { get; set; }

    public string? MaMau { get; set; }

    public string? TieuDe { get; set; }

    public string? NoiDungThongBao { get; set; }

    public virtual ICollection<CauHinhThongBao> CauHinhThongBaos { get; set; } = new List<CauHinhThongBao>();
}
