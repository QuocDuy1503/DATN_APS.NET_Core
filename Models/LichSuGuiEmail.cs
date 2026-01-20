using System;
using System.Collections.Generic;

namespace DATN_TMS.Models;

public partial class LichSuGuiEmail
{
    public int Id { get; set; }

    public int? IdCauHinh { get; set; }

    public string? NguoiNhan { get; set; }

    public DateTime? ThoiGianGui { get; set; }

    public string? TrangThai { get; set; }

    public virtual CauHinhThongBao? IdCauHinhNavigation { get; set; }
}
