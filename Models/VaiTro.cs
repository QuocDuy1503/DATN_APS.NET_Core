using System;
using System.Collections.Generic;

namespace DATN_TMS.Models;

public partial class VaiTro
{
    public int Id { get; set; }

    public string? MaVaiTro { get; set; }

    public string? TenVaiTro { get; set; }

    public virtual ICollection<NguoiDung> IdNguoiDungs { get; set; } = new List<NguoiDung>();
}
