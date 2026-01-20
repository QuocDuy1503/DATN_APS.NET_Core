using System;
using System.Collections.Generic;

namespace DATN_TMS.Models;

public partial class ThanhVienHdBaoCao
{
    public int Id { get; set; }

    public int? IdHdBaocao { get; set; }

    public int? IdGiangVien { get; set; }

    public string? VaiTro { get; set; }

    public virtual GiangVien? IdGiangVienNavigation { get; set; }

    public virtual HoiDongBaoCao? IdHdBaocaoNavigation { get; set; }
}
