using System;
using System.Collections.Generic;

namespace DATN_TMS.Models;

public partial class GiangVien
{
    public int IdNguoiDung { get; set; }

    public string? MaGv { get; set; }

    public string? HocVi { get; set; }

    public int? IdBoMon { get; set; }

    public virtual ICollection<DeTai> DeTaiIdGvhdNavigations { get; set; } = new List<DeTai>();

    public virtual ICollection<DeTai> DeTaiNguoiDuyetNavigations { get; set; } = new List<DeTai>();

    public virtual ICollection<DiemChiTiet> DiemChiTiets { get; set; } = new List<DiemChiTiet>();

    public virtual BoMon? IdBoMonNavigation { get; set; }

    public virtual NguoiDung IdNguoiDungNavigation { get; set; } = null!;

    public virtual ICollection<ThanhVienHdBaoCao> ThanhVienHdBaoCaos { get; set; } = new List<ThanhVienHdBaoCao>();
}
