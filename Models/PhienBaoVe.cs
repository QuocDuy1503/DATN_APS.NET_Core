using System;
using System.Collections.Generic;

namespace DATN_TMS.Models;

public partial class PhienBaoVe
{
    public int Id { get; set; }

    public int? IdHdBaocao { get; set; }

    public int? IdSinhVienDeTai { get; set; }

    public int? SttBaoCao { get; set; }

    public string? LinkTaiLieu { get; set; }

    public virtual ICollection<DiemChiTiet> DiemChiTiets { get; set; } = new List<DiemChiTiet>();

    public virtual HoiDongBaoCao? IdHdBaocaoNavigation { get; set; }

    public virtual SinhVienDeTai? IdSinhVienDeTaiNavigation { get; set; }

    public virtual ICollection<KetQuaBaoVeSinhVien> KetQuaBaoVeSinhViens { get; set; } = new List<KetQuaBaoVeSinhVien>();
}
