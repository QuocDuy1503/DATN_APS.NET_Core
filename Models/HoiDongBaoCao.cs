using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
namespace DATN_TMS.Models;

public partial class HoiDongBaoCao
{
    public int Id { get; set; }

    public string? MaHoiDong { get; set; }

    public string? TenHoiDong { get; set; }

    public string? LoaiHoiDong { get; set; }

    public int? IdDot { get; set; }

    public int? IdNguoiTao { get; set; }

    public int? IdBoMon { get; set; }

    public DateOnly? NgayBaoCao { get; set; }

    public string? DiaDiem { get; set; }

    public TimeOnly? ThoiGianDuKien { get; set; }

    public bool? TrangThai { get; set; }

    public virtual BoMon? IdBoMonNavigation { get; set; }

    public virtual DotDoAn? IdDotNavigation { get; set; }
    [ForeignKey("IdNguoiTao")]
    public virtual NguoiDung IdNguoiTaoNavigation { get; set; }

    public virtual ICollection<PhienBaoVe> PhienBaoVes { get; set; } = new List<PhienBaoVe>();

    public virtual ICollection<ThanhVienHdBaoCao> ThanhVienHdBaoCaos { get; set; } = new List<ThanhVienHdBaoCao>();
    
}
